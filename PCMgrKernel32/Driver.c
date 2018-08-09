#include "Driver.h"
#include "..\TaskMgrCore\ioctls.h"
#include "protect.h"
#include "sys.h"
#include "unexp.h"
#include "proc.h"
#include "kmodul.h"
#include "monitor.h"
#include "handle.h"

#define DEVICE_LINK_NAME L"\\??\\PCMGRK"
#define DEVICE_OBJECT_NAME  L"\\Device\\PCMGRK"

BOOLEAN kernelInited = FALSE;

strcat_s_ _strcat_s;
strcpy_s_ _strcpy_s;
memcpy_s_ _memcpy_s;
swprintf_s_ swprintf_s;
wcscpy_s_ _wcscpy_s;
wcscat_s_ _wcscat_s;
memset_ _memset;

PsResumeProcess_ _PsResumeProcess;
PsSuspendProcess_ _PsSuspendProcess;
PsLookupProcessByProcessId_ _PsLookupProcessByProcessId;
PsLookupThreadByThreadId_ _PsLookupThreadByThreadId;

ULONG_PTR CurrentDbgViewProcess = 0;
ULONG_PTR CurrentPCMgrProcess = 0;
ULONG LoadedModuleOrder = 0;
PLIST_ENTRY PsLoadedModuleList = NULL;
PLIST_ENTRY ListEntry = NULL;
PLIST_ENTRY ListEntryScan = NULL;
PEPROCESS PEprocessSystem = NULL;

extern BOOLEAN kxCanCreateProcess;
extern BOOLEAN kxCanCreateThread;
extern BOOLEAN kxCanLoadDriver;

ULONG_PTR kxNtosBaseAddress = 0;
WCHAR kxNtosName[32];
PRKEVENT kxEventObjectMain = NULL;
OBJECT_HANDLE_INFORMATION kxObjectHandleInfoEventMain;
OBJECT_HANDLE_INFORMATION kxObjectHandleInfoDbgViewEvent;
BOOLEAN kxMyDbgViewCanUse = FALSE;
PVOID kxEventObjectDbgViewEvent = NULL;

PDBGPRINT_DATA kxMyDbgViewDataStart = NULL;
PDBGPRINT_DATA kxMyDbgViewDataEnd = NULL;

BOOLEAN kxMyDbgViewLastReceived = FALSE;

NTSTATUS DriverEntry(IN PDRIVER_OBJECT pDriverObject, IN PUNICODE_STRING pRegPath)
{
	NTSTATUS ntStatus;

	UNICODE_STRING DeviceObjectName; // NT Device Name 
	UNICODE_STRING DeviceLinkName; // Win32 Name 
	PDEVICE_OBJECT deviceObject = NULL; 
	RtlInitUnicodeString(&DeviceObjectName, DEVICE_OBJECT_NAME);

	ntStatus = IoCreateDevice(
		pDriverObject,
		0, 
		&DeviceObjectName,
		FILE_DEVICE_UNKNOWN, 
		FILE_DEVICE_SECURE_OPEN, 
		FALSE,
		&deviceObject); 

	if (!NT_SUCCESS(ntStatus))
	{
		KdPrint(("Couldn't create the device object\n"));
		return ntStatus;
	}

	pDriverObject->DriverUnload = (PDRIVER_UNLOAD)DriverUnload;
	pDriverObject->MajorFunction[IRP_MJ_DEVICE_CONTROL] = IOControlDispatch;
	pDriverObject->MajorFunction[IRP_MJ_CREATE] = CreateDispatch;
	

	//创建驱动设备对象	
	RtlInitUnicodeString(&DeviceLinkName, DEVICE_LINK_NAME);
	ntStatus = IoCreateSymbolicLink(&DeviceLinkName, &DeviceObjectName);
	if (!NT_SUCCESS(ntStatus))
	{
		KdPrint(("Couldn't create symbolic link\n"));
		IoDeleteDevice(deviceObject);
	}
	
	KxLoadFunctions();

#ifdef _AMD64_
	PLDR_DATA_TABLE_ENTRY64 ldr;
	ldr = (PLDR_DATA_TABLE_ENTRY64)pDriverObject->DriverSection;
#else
	PLDR_DATA_TABLE_ENTRY32 ldr;
	ldr = (PLDR_DATA_TABLE_ENTRY32)pDriverObject->DriverSection;
#endif
	ldr->Flags |= 0x20;//绕过MmVerifyCallbackFunction。
	//获取内核模块链表
	PsLoadedModuleList = (PLIST_ENTRY)(ULONG_PTR)ldr->InLoadOrderLinks.Flink;
	ListEntry = PsLoadedModuleList->Flink;
	//获取ntoskrnl基址和名称
	ListEntryScan = ListEntry;
#ifdef _AMD64_
	PLDR_DATA_TABLE_ENTRY64 ModuleNtos = CONTAINING_RECORD(ListEntry, LDR_DATA_TABLE_ENTRY64, InLoadOrderLinks);
	if (ModuleNtos->BaseDllName.Buffer != 0)
		_wcscpy_s(kxNtosName, 32, (wchar_t*)(ULONG_PTR)ModuleNtos->BaseDllName.Buffer);
	kxNtosBaseAddress = (ULONG_PTR)ModuleNtos->DllBase;
#else
	PLDR_DATA_TABLE_ENTRY32 ModuleNtos = CONTAINING_RECORD(ListEntry, LDR_DATA_TABLE_ENTRY32, InLoadOrderLinks);
	if (ModuleNtos->BaseDllName.Buffer != 0) {
		_wcscpy_s(kxNtosName, 32, (wchar_t*)(ULONG_PTR)ModuleNtos->BaseDllName.Buffer);
	}
	kxNtosBaseAddress = ModuleNtos->DllBase;
#endif


	NTSTATUS statusKxD = KxInitMyDbgView();
	if (!NT_SUCCESS(statusKxD)) KdPrint(("KxInitMyDbgView failed! 0x%08X\n", statusKxD));

	NTSTATUS statusKx = KxInitProtectProcess();
	if(!NT_SUCCESS(statusKx)) KdPrint(("KxInitProtectProcess failed! 0x%08X\n", statusKx));

	NTSTATUS statusKxM = KxPsMonitorInit();
	if (!NT_SUCCESS(statusKxM)) KdPrint(("KxPsMonitorInit failed! 0x%08X\n", statusKxM));

	PEprocessSystem = IoGetCurrentProcess();

	KdPrint(("DriverEntry OK!\n"));
	return ntStatus;
}
VOID DriverUnload(_In_ struct _DRIVER_OBJECT *pDriverObject)
{
	UNICODE_STRING  DeviceLinkName;
	PDEVICE_OBJECT  v1 = NULL;
	PDEVICE_OBJECT  DeleteDeviceObject = NULL;

	KxUnInitProtectProcess();
	KxPsMonitorUnInit();
	KxUnInitMyDbgView();

	RtlInitUnicodeString(&DeviceLinkName, DEVICE_LINK_NAME);
	IoDeleteSymbolicLink(&DeviceLinkName);

	DeleteDeviceObject = pDriverObject->DeviceObject;
	while (DeleteDeviceObject != NULL)
	{
		v1 = DeleteDeviceObject->NextDevice;
		IoDeleteDevice(DeleteDeviceObject);
		DeleteDeviceObject = v1;
	}

	KdPrint(("DriverUnload\n"));
}

NTSTATUS IOControlDispatch(IN PDEVICE_OBJECT DeviceObject, IN PIRP Irp)
{
	NTSTATUS Status;
	ULONG_PTR Informaiton = 0;
	PVOID InputData = NULL;
	ULONG InputDataLength = 0;
	PVOID OutputData = NULL;
	ULONG OutputDataLength = 0;
	ULONG IoControlCode = 0;
	PIO_STACK_LOCATION  IoStackLocation = IoGetCurrentIrpStackLocation(Irp);  //Irp堆栈  

	IoControlCode = IoStackLocation->Parameters.DeviceIoControl.IoControlCode;
	InputData = Irp->AssociatedIrp.SystemBuffer;
	OutputData = Irp->AssociatedIrp.SystemBuffer;
	InputDataLength = IoStackLocation->Parameters.DeviceIoControl.InputBufferLength;
	OutputDataLength = IoStackLocation->Parameters.DeviceIoControl.OutputBufferLength;

	switch (IoControlCode)
	{
	case CTL_KERNEL_INIT: {
		PKINITAGRS agrs = (PKINITAGRS)InputData;
		Status = InitKernel(agrs);
		if (agrs->NeedNtosVaule) {
			KNTOSVALUE ntosvalue;
			ntosvalue.NtostAddress = kxNtosBaseAddress;
			ntosvalue.KernelDataInited = kernelInited;
			_wcscpy_s(ntosvalue.NtosModuleName, 32, kxNtosName);
			_memcpy_s(OutputData, OutputDataLength, &ntosvalue, sizeof(ntosvalue));
			Informaiton = sizeof(KNTOSVALUE);
		}
		break;
	}
	case CTL_KERNEL_INIT_WITH_PDB_DATA: {
		if (!kernelInited) {
			PNTOS_PDB_DATA data = (PNTOS_PDB_DATA)InputData;
			KxGetFunctionsFormPDBData(data);
			KxGetStructOffestsFormPDBData(&data->StructOffestData);
			KdPrint(("Pdb Data received."));
			kernelInited = TRUE;
		}
		Status = STATUS_SUCCESS;
		break;
	}
	case CTL_SET_DBGVIEW_EVENT: {
		BOOLEAN result = FALSE;
		PDBGVIEW_SENDER hEvent = (PDBGVIEW_SENDER)InputData;
		if (!KxMyDbgViewWorking())
		{
			Status = ObReferenceObjectByHandle(hEvent->EventHandle, EVENT_MODIFY_STATE, *ExEventObjectType, KernelMode, (PVOID*)&kxEventObjectDbgViewEvent, &kxObjectHandleInfoDbgViewEvent);
			if (NT_SUCCESS(Status))
			{
				CurrentDbgViewProcess = hEvent->ProcessId;
				kxMyDbgViewCanUse = TRUE;
				Status = STATUS_SUCCESS;
				result = TRUE;
			}
		}
		else Status = STATUS_SUCCESS;
		_memcpy_s(OutputData, OutputDataLength, &result, sizeof(result));
		Informaiton = OutputDataLength;
		break;
	}
	case CTL_SET_KERNEL_EVENT: {
		HANDLE hEvent = *(HANDLE*)InputData;
		Status = ObReferenceObjectByHandle(hEvent, GENERIC_ALL, NULL, KernelMode, (PVOID*)&kxEventObjectMain, &kxObjectHandleInfoEventMain);
		if (NT_SUCCESS(Status))
		{

			Status = STATUS_SUCCESS;
		}
		break;
	}
	case CTL_SET_MON_REFUSE_CREATE_PROC: {
		BOOLEAN allow = *(UCHAR*)InputData;
		kxCanCreateProcess = allow;
		Status = STATUS_SUCCESS;
		break;
	}
	case CTL_SET_MON_REFUSE_CREATE_THREAD: {
		BOOLEAN allow = *(UCHAR*)InputData;
		kxCanCreateThread = allow;
		Status = STATUS_SUCCESS;
		break;
	}
	case CTL_SET_MON_REFUSE_LOAD_IMAGE: {
		BOOLEAN allow = *(UCHAR*)InputData;
		kxCanLoadDriver = allow;
		Status = STATUS_SUCCESS;
		break;
	}
	case CTL_SET_CURRENT_PCMGR_PROCESS: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		CurrentPCMgrProcess = pid;
		Status = STATUS_SUCCESS;
		break;
	}
	case CTL_OPEN_PROCESS: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;

		PEPROCESS pEProc;
		Status = _PsLookupProcessByProcessId((HANDLE)pid, &pEProc);
		if (NT_SUCCESS(Status))
		{
			HANDLE handle;
			Status = ObOpenObjectByPointer(pEProc, 0, 0, PROCESS_ALL_ACCESS, *PsProcessType, UserMode, &handle);
			if (NT_SUCCESS(Status)) {
				_memcpy_s(OutputData, OutputDataLength, &handle, sizeof(handle));
				Status = STATUS_SUCCESS;
				Informaiton = OutputDataLength;
			}
			else KdPrint(("ObOpenObjectByPointer err : 0x%08X", Status));
			ObDereferenceObject(pEProc);
		}
		break;
	}
	case CTL_OPEN_THREAD: {
		ULONG_PTR tid = *(ULONG_PTR*)InputData;
		PETHREAD pEThread;
		Status = _PsLookupThreadByThreadId((HANDLE)tid, &pEThread);
		if (NT_SUCCESS(Status))
		{
			HANDLE handle;
			Status = ObOpenObjectByPointer(pEThread, 0, 0, THREAD_ALL_ACCESS, *PsThreadType, UserMode, &handle);
			if (NT_SUCCESS(Status)) {
				_memcpy_s(OutputData, OutputDataLength, &handle, sizeof(handle));
				Informaiton = OutputDataLength;
				Status = STATUS_SUCCESS;
			}
			ObDereferenceObject(pEThread);
		}
		break;
	}
	case CTL_GET_EPROCESS: {
		ULONG pid = *(ULONG*)InputData;
		if (pid == 0) {
			KPROCINFO kpinfo;
			_memset(&kpinfo, 0, sizeof(kpinfo));
			_memcpy_s(OutputData, OutputDataLength, &kpinfo, sizeof(kpinfo));
			Informaiton = OutputDataLength;
			Status = STATUS_SUCCESS;
		}
		else {
			PEPROCESS pEProc;
			Status = _PsLookupProcessByProcessId((HANDLE)(ULONG_PTR)pid, &pEProc);
			if (NT_SUCCESS(Status))
			{
				KPROCINFO kpinfo;
				_memset(&kpinfo, 0, sizeof(kpinfo));
				kpinfo.EProcess = (ULONG_PTR)pEProc;
				kpinfo.PebAddress = (ULONG_PTR)PsGetProcessPeb(pEProc);
				kpinfo.JobAddress = (ULONG_PTR)PsGetProcessJob(pEProc);
				kpinfo.PriorityClass = (ULONG_PTR)PsGetProcessPriorityClass(pEProc);
				PUCHAR procName = PsGetProcessImageFileName(pEProc);
				size_t procNameSize = strlen(procName) + 1;
				_memcpy_s(kpinfo.ImageFileName, procNameSize, procName, procNameSize);
				PUNICODE_STRING fullPathString = KxGetProcessFullPath(pEProc);
				if (fullPathString && fullPathString->Buffer)
					_wcscpy_s(kpinfo.FullPath, 260, fullPathString->Buffer);
				_memcpy_s(OutputData, OutputDataLength, &kpinfo, sizeof(kpinfo));
				Informaiton = sizeof(kpinfo);

				ObDereferenceObject(pEProc);
				Status = STATUS_SUCCESS;
			}
		}
		break;
	}
	case CTL_GET_ETHREAD: {
		ULONG tid = *(ULONG*)InputData;

		PETHREAD pEThread;
		Status = _PsLookupThreadByThreadId((HANDLE)(ULONG_PTR)tid, &pEThread);
		if (NT_SUCCESS(Status)) {
			KTHREADINFO ktinfp;
			_memset(&ktinfp, 0, sizeof(ktinfp));
			ktinfp.EThread = (ULONG_PTR)pEThread;
			ktinfp.TebAddress = (ULONG_PTR)PsGetThreadTeb(pEThread);
			_memcpy_s(OutputData, OutputDataLength, &ktinfp, sizeof(ktinfp));
			Informaiton = OutputDataLength;
			ObDereferenceObject(pEThread);
			Status = STATUS_SUCCESS;
		}
		break;
	}
	case CTL_SUSPEND_PROCESS: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		PEPROCESS pEProc;
		Status = _PsLookupProcessByProcessId((HANDLE)pid, &pEProc);
		if (NT_SUCCESS(Status))
		{
			Status = _PsSuspendProcess(pEProc);
			ObDereferenceObject(pEProc);
		}
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = OutputDataLength;
		break;
	}
	case CTL_RESUME_PROCESS: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		PEPROCESS pEProc;
		Status = _PsLookupProcessByProcessId((HANDLE)pid, &pEProc);
		if (NT_SUCCESS(Status))
		{
			Status = _PsResumeProcess(pEProc);
			ObDereferenceObject(pEProc);
		}
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = OutputDataLength;
		break;
	}
	case CTL_FORCE_SHUTDOWN: {
		KxForceShutdown();
		Status = STATUS_SUCCESS;
		break;
	}
	case CTL_FORCE_REBOOT: {
		KxForceReBoot();
		Status = STATUS_SUCCESS;
		break;
	}
	case CTL_TEST: {
		if (InputData != NULL && InputDataLength > 0)
		{
			KdPrint(((CHAR*)InputData));
			Status = STATUS_SUCCESS;
		}
		break;
	}
	case CTL_TEST2: {

		break;
	}
	case CTL_TEST_KPROC: {
		KdPrint(("CTL_TEST_KPROC"));
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		Status = KxTerminateProcessTest(pid);
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = OutputDataLength;
		break;
	}
	case CTL_PRINT_INTERNAL_FUNS: {
		KxPrintInternalFuns();
		Status = STATUS_SUCCESS;
		break;
	}
	case CTL_FORCE_TREMINATE_PROCESS: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		Status = KxTerminateProcessWithPid(pid, 0, FALSE, FALSE);
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = sizeof(Status);
		break;
	}
	case CTL_FORCE_TREMINATE_THREAD: {
		ULONG_PTR tid = *(ULONG_PTR*)InputData;
		Status = KxTerminateThreadWithTid(tid, 0, 0);
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = sizeof(Status);
		break;
	}
	case CTL_FORCE_TREMINATE_PROCESS_PS: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		Status = KxTerminateProcessWithPid(pid, 0, TRUE, FALSE);
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = sizeof(Status);
	}
	case CTL_FORCE_TREMINATE_PROCESS_APC: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		Status = KxTerminateProcessWithPid(pid, 0, FALSE, TRUE);
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = OutputDataLength;
		break;
	}
	case CTL_FORCE_TREMINATE_THREAD_APC: {
		ULONG_PTR tid = *(ULONG_PTR*)InputData;
		Status = KxTerminateThreadWithTid(tid, 0, 1);
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = OutputDataLength;
		break;
	}
	case CTL_ADD_PROCESS_PROTECT: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		KxProtectProcessWithPid((HANDLE)pid);
		Status = STATUS_SUCCESS;
		break;
	}
	case CTL_REMOVE_PROCESS_PROTECT: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		KxUnProtectProcessWithPid((HANDLE)pid);
		Status = STATUS_SUCCESS;
		break;
	}
	case CTL_GET_KERNEL_MODULS: {
		BOOLEAN start = *(UCHAR*)InputData;
		if (start) {
			ListEntryScan = ListEntry;
			LoadedModuleOrder = 0;
		}

		KERNEL_MODULE kModule;
		_memset(&kModule, 0, sizeof(KERNEL_MODULE));
		if (ListEntryScan != PsLoadedModuleList)
		{
#ifdef _AMD64_
			PLDR_DATA_TABLE_ENTRY64 ModuleEntry = CONTAINING_RECORD(ListEntryScan, LDR_DATA_TABLE_ENTRY64, InLoadOrderLinks);;
#else
			PLDR_DATA_TABLE_ENTRY32 ModuleEntry = CONTAINING_RECORD(ListEntryScan, LDR_DATA_TABLE_ENTRY32, InLoadOrderLinks);
#endif
			if (ModuleEntry->BaseDllName.Buffer != 0)
				_wcscpy_s(kModule.BaseDllName, 64, (wchar_t*)(ULONG_PTR)ModuleEntry->BaseDllName.Buffer);
			if (ModuleEntry->FullDllName.Buffer != 0)
				_wcscpy_s(kModule.FullDllPath, 64, (wchar_t*)(ULONG_PTR)ModuleEntry->FullDllName.Buffer);
			kModule.Order = LoadedModuleOrder;
			kModule.EntryPoint = (ULONG_PTR)ModuleEntry->EntryPoint;
			kModule.SizeOfImage = (ULONG_PTR)ModuleEntry->SizeOfImage;

			KxGetDrvObjectByName((wchar_t*)(ULONG_PTR)ModuleEntry->BaseDllName.Buffer, &kModule.DriverObject);

			kModule.Base = (ULONG_PTR)ModuleEntry->DllBase;

			LoadedModuleOrder++;
			ListEntryScan = ListEntryScan->Flink;
		}
		else kModule.Order = 9999;
		Status = STATUS_SUCCESS;
		_memcpy_s(OutputData, OutputDataLength, &kModule, sizeof(kModule));
		Informaiton = OutputDataLength;
		break;
	}
	case CTL_KDA_DEC: {
		PKDAAGRS agr = (KDAAGRS*)InputData;
		PUCHAR outBuffer = OutputData;
		ULONG_PTR curcodeptr = agr->StartAddress;
		if (MmIsAddressValid((PVOID)curcodeptr) != FALSE && MmIsAddressValid((PVOID)(agr->StartAddress + agr->Size)) != FALSE)
			RtlMoveMemory(outBuffer, (PVOID)curcodeptr, agr->Size);
		Informaiton = OutputDataLength;
		Status = STATUS_SUCCESS;
		break;
	}
	case CTL_GET_DBGVIEW_BUFFER: {
		Status = STATUS_UNSUCCESSFUL;
		if (kxMyDbgViewCanUse)
		{
			DBGPRT_DATA_TRA data;
			if (kxMyDbgViewDataStart)
			{
				data.HasData = TRUE;

				PDBGPRINT_DATA ptr = kxMyDbgViewDataStart;
				PDBGPRINT_DATA ptr_next = kxMyDbgViewDataStart->Next;
				_memcpy_s(data.Data, sizeof(data.Data), ptr->StrBuffer, sizeof(data.Data));

				data.HasMoreData = ptr_next != NULL;

				if (kxMyDbgViewDataEnd == kxMyDbgViewDataStart)
					kxMyDbgViewDataEnd = NULL;
				kxMyDbgViewDataStart = ptr_next;

				ExFreePool(ptr);
			}
			else data.HasData = FALSE;
			_memcpy_s(OutputData, OutputDataLength, &data, sizeof(data));
			Informaiton = sizeof(data);
			kxMyDbgViewLastReceived = FALSE;
			Status = STATUS_SUCCESS;
		}
		break;
	}
	case CTL_RESET_DBGVIEW_EVENT: {
		KxMyDbgViewReset();
		Status = STATUS_SUCCESS;
		break;
	}
	case CTL_GET_DBGVIEW_LAST_REC: {
		_memcpy_s(OutputData, OutputDataLength, &kxMyDbgViewLastReceived, sizeof(kxMyDbgViewLastReceived));
		Informaiton = sizeof(kxMyDbgViewLastReceived);
		Status = STATUS_SUCCESS;
		break;
	}
	case CTL_FORCE_CLOSE_HANDLE: {
		PFCLOSE_HANDLE_DATA data = (PFCLOSE_HANDLE_DATA)InputData;
		Status = KxForceCloseHandle((HANDLE)data->ProcessId, data->HandleValue);
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = OutputDataLength;
		break;
	}
	case CTL_GET_PROCESS_COMMANDLINE: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		PEPROCESS Process;
		Status = _PsLookupProcessByProcessId((HANDLE)pid, &Process);
		if (NT_SUCCESS(Status))
		{
			PUNICODE_STRING cmdStr = KxGetProcessCommandLine(Process);
			if (cmdStr != NULL && cmdStr->Buffer != NULL) {
				_wcscpy_s((wchar_t*)OutputData, 1024, cmdStr->Buffer);
				Informaiton = 1024 * sizeof(wchar_t);
			}
			ObDereferenceObject(Process);
			Status = STATUS_SUCCESS;
		}
		break;
	}
	default:
		break;
	}

	Irp->IoStatus.Status = Status;             //Ring3 GetLastError();
	Irp->IoStatus.Information = Informaiton;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);  //将Irp返回给Io管理器
	return Status;                            //Ring3 DeviceIoControl()返回值
}
NTSTATUS CreateDispatch(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
{
	Irp->IoStatus.Status = STATUS_SUCCESS;
	Irp->IoStatus.Information = 0;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);
	return STATUS_SUCCESS;
}

NTSTATUS InitKernel(PKINITAGRS parm)
{
	NTSTATUS status = STATUS_SUCCESS;
	if (!kernelInited) {
		if (parm->NeedNtosVaule) {
			KdPrint(("InitKernel ! Waiting for PDB Data"));
			kernelInited = FALSE;
		}
		else {
			PWINVERS wv = &parm->WinVer;
			KdPrint(("InitKernel ! System ver : %u Bulid ver : %u\n", wv->VerSimple, wv->WinBulidVerl));
			status = KxGetFunctions(wv);
			status = KxLoadStructOffests(wv);
			kernelInited = TRUE;
		}
	}
	else KdPrint(("InitKernel ! And kernel alredy inited"));
	return status;
}

VOID KxLoadFunctions()
{
	UNICODE_STRING MemsetName;
	UNICODE_STRING MemcpysName;
	UNICODE_STRING StrcpysName;
	UNICODE_STRING StrcatsName;
	UNICODE_STRING SWprintfsName;
	UNICODE_STRING WCscatsName;
	UNICODE_STRING WCscpysName;
	UNICODE_STRING PsResumeProcessName;
	UNICODE_STRING PsSuspendProcessName;
	UNICODE_STRING PsLookupProcessByProcessIdName;
	UNICODE_STRING PsLookupThreadByThreadIdName;

	RtlInitUnicodeString(&PsLookupProcessByProcessIdName, L"PsLookupProcessByProcessId");
	RtlInitUnicodeString(&PsLookupThreadByThreadIdName, L"PsLookupThreadByThreadId");
	RtlInitUnicodeString(&PsResumeProcessName, L"PsResumeProcess");
	RtlInitUnicodeString(&PsSuspendProcessName, L"PsSuspendProcess");
	RtlInitUnicodeString(&MemsetName, L"memset");
	RtlInitUnicodeString(&WCscatsName, L"wcscat_s");
	RtlInitUnicodeString(&WCscpysName, L"wcscpy_s");
	RtlInitUnicodeString(&SWprintfsName, L"swprintf_s");
	RtlInitUnicodeString(&MemcpysName, L"memcpy_s");
	RtlInitUnicodeString(&StrcpysName, L"strcpy_s");
	RtlInitUnicodeString(&StrcatsName, L"strcat_s");

	_PsLookupProcessByProcessId = (PsLookupProcessByProcessId_)MmGetSystemRoutineAddress(&PsLookupProcessByProcessIdName);
	_PsLookupThreadByThreadId = (PsLookupThreadByThreadId_)MmGetSystemRoutineAddress(&PsLookupThreadByThreadIdName);
	_PsResumeProcess = (PsResumeProcess_)MmGetSystemRoutineAddress(&PsResumeProcessName);
	_PsSuspendProcess = (PsSuspendProcess_)MmGetSystemRoutineAddress(&PsSuspendProcessName);
	_memset = (memset_)MmGetSystemRoutineAddress(&MemsetName);
	_wcscpy_s = (wcscpy_s_)MmGetSystemRoutineAddress(&WCscpysName);
	_wcscat_s = (wcscat_s_)MmGetSystemRoutineAddress(&WCscatsName);
	_memcpy_s = (memcpy_s_)MmGetSystemRoutineAddress(&MemcpysName);
	_strcat_s = (strcat_s_)MmGetSystemRoutineAddress(&StrcatsName);
	_strcpy_s = (strcpy_s_)MmGetSystemRoutineAddress(&StrcpysName);
	swprintf_s = (swprintf_s_)MmGetSystemRoutineAddress(&SWprintfsName);
	
}

void KxDbgViewR3Exited() {

	KxMyDbgViewReset();
}

VOID KxMyDebugPrintCopyData(_In_ PSTRING Output)
{
	if (kxMyDbgViewDataEnd == NULL)
	{
		kxMyDbgViewDataStart = ExAllocatePool(NonPagedPool, sizeof(DBGPRINT_DATA));
		kxMyDbgViewDataEnd = kxMyDbgViewDataStart;
	}
	else 
	{
		kxMyDbgViewDataEnd->Next = ExAllocatePool(NonPagedPool, sizeof(DBGPRINT_DATA));
		kxMyDbgViewDataEnd = kxMyDbgViewDataEnd->Next;
	}

	_memset(kxMyDbgViewDataEnd, 0, sizeof(DBGPRINT_DATA));

	for (int i = 0; i < Output->Length &&i < 256; i++)
		kxMyDbgViewDataEnd->StrBuffer[i] = Output->Buffer[i];
}
VOID KxMyDebugPrintCallback(_In_ PSTRING Output, _In_ ULONG ComponentId, _In_ ULONG Level)
{
	kxMyDbgViewLastReceived = TRUE;
	if (kxMyDbgViewCanUse) {
		if (Output != NULL && Output->Buffer != NULL)
		{
			KxMyDebugPrintCopyData(Output);
			KeSetEvent(kxEventObjectDbgViewEvent, 0, 0);
		}
	}
}

void KxMyDbgViewFreeAllData() {
	if (kxMyDbgViewDataStart)
	{
		PDBGPRINT_DATA ptr = kxMyDbgViewDataStart;
		if (ptr->Next != NULL) {
			do {
				PDBGPRINT_DATA ptr_next = ptr->Next;
				ExFreePool(ptr);
				ptr = ptr_next;
			} while (ptr != NULL);
		}
		else {
			ExFreePool(ptr);
		}

		kxMyDbgViewDataStart = NULL;
		kxMyDbgViewDataEnd = NULL;
	}
}
void KxMyDbgViewReset() {
	kxMyDbgViewCanUse = FALSE;
	if (kxEventObjectDbgViewEvent) {
		ObDereferenceObject(kxEventObjectDbgViewEvent);
		kxEventObjectDbgViewEvent = NULL;	
		KxMyDbgViewFreeAllData();
	}
	CurrentDbgViewProcess = 0;
}
BOOLEAN KxMyDbgViewWorking() {
	if (kxEventObjectDbgViewEvent && CurrentDbgViewProcess != 0)
	{
		PEPROCESS pEprocess;
		NTSTATUS ntstatus = _PsLookupProcessByProcessId((HANDLE)CurrentDbgViewProcess, &pEprocess);
		if (ntstatus == STATUS_SUCCESS) {
			ObDereferenceObject(pEprocess);
			return TRUE;
		}
		else if (ntstatus == STATUS_INVALID_CID)
		{
			KxMyDbgViewReset();
			return FALSE;
		}
	}
	return FALSE;
}

NTSTATUS KxUnInitMyDbgView() {
	KxMyDbgViewFreeAllData();
	return DbgSetDebugPrintCallback(KxMyDebugPrintCallback, FALSE);
}
NTSTATUS KxInitMyDbgView() {
	return DbgSetDebugPrintCallback(KxMyDebugPrintCallback, TRUE);
}




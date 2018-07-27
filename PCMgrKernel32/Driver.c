#include "Driver.h"
#include "..\TaskMgrCore\ioctls.h"
#include "protect.h"
#include "sys.h"
#include "unexp.h"
#include "proc.h"
#include "kmodul.h"

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

ULONG LoadedModuleOrder = 0;
PLIST_ENTRY PsLoadedModuleList = NULL;
PLIST_ENTRY ListEntry = NULL;
PLIST_ENTRY ListEntryScan = NULL;

NTSTATUS DriverEntry(IN PDRIVER_OBJECT pDriverObject, IN PUNICODE_STRING pRegPath)
{
	NTSTATUS ntStatus;

	KdPrint(("Enter DriverEntry!\n"));

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

	KxLoadFunctions();

	NTSTATUS statusKx = KxInitProtectProcess();
	if(!NT_SUCCESS(statusKx)) KdPrint(("KxInitProtectProcess failed! 0x%08X\n", statusKx));

	KdPrint(("DriverEntry end!\n"));
	return ntStatus;
}

VOID DriverUnload(_In_ struct _DRIVER_OBJECT *pDriverObject)
{
	UNICODE_STRING  DeviceLinkName;
	PDEVICE_OBJECT  v1 = NULL;
	PDEVICE_OBJECT  DeleteDeviceObject = NULL;

	KxUnInitProtectProcess();

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
		Status = InitKernel(*(ULONG*)InputData);
		break;
	}
	case CTL_SET_KERNEL_EVENT: {


		break;
	}
	case CTL_OPEN_PROCESS: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;

		PEPROCESS pEProc;
		Status = PsLookupProcessByProcessId((HANDLE)pid, &pEProc);
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
		if (NT_SUCCESS(PsLookupThreadByThreadId((HANDLE)tid, &pEThread)))
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
	case CTL_TREMINATE_PROCESS: {
		HANDLE hProcess = *(HANDLE*)InputData;
		Status = KxTerminateProcess(hProcess, 0);
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = OutputDataLength;
		break;
	}
	case CTL_TREMINATE_THREAD: {
		HANDLE hThread = *(HANDLE*)InputData;
		Status = KxTerminateThread(hThread, 0);
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = OutputDataLength;
		break;
	}
	case CTL_GET_EPROCESS: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;

		PEPROCESS pEProc;
		if (NT_SUCCESS(PsLookupProcessByProcessId((HANDLE)pid, &pEProc)))
		{
			_memcpy_s(OutputData, OutputDataLength, &pEProc, sizeof(pEProc));
			Informaiton = OutputDataLength;

			ObDereferenceObject(pEProc);
			Status = STATUS_SUCCESS;
		}
		break;
	}
	case CTL_GET_ETHREAD: {
		ULONG_PTR tid = *(ULONG_PTR*)InputData;

		PETHREAD pEThread;
		if (NT_SUCCESS(PsLookupThreadByThreadId((HANDLE)tid, &pEThread))) {
			ULONG_PTR *outBuf = (ULONG_PTR *)OutputData;
			_memcpy_s(OutputData, OutputDataLength, &pEThread, sizeof(pEThread));
			Informaiton = OutputDataLength;
			ObDereferenceObject(pEThread);
			Status = STATUS_SUCCESS;
		}
		break;
	}
	case CTL_SUSPEND_PROCESS: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		PEPROCESS pEProc;
		if (NT_SUCCESS(PsLookupProcessByProcessId((HANDLE)pid, &pEProc)))
		{
			Status = PsSuspendProcess(pEProc);
			ObDereferenceObject(pEProc);
		}
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = OutputDataLength;
		break;
	}	
	case CTL_RESUME_PROCESS: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		PEPROCESS pEProc;
		if (NT_SUCCESS(PsLookupProcessByProcessId((HANDLE)pid, &pEProc)))
		{
			Status = PsResumeProcess(pEProc);
			ObDereferenceObject(pEProc);
		}
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = OutputDataLength;
		break;
	}
	case CTL_FORCE_SHUTDOWN: {
		KxForceShutdown();
		break;
	}	
	case CTL_FORCE_REBOOT: {
		KxForceReBoot();
		break;
	}
	case CTL_TEST: {
		if (InputData != NULL && InputDataLength > 0)
		{
			KdPrint(("%s\n", (CHAR*)InputData));
			Status = STATUS_SUCCESS;
		}
		break;
	}
	case CTL_FORCE_TREMINATE_PROCESS: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		Status = KxTerminateProcessWithPid(pid, 0);
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = sizeof(Status);
		break;
	}	
	case CTL_FORCE_TREMINATE_THREAD: {
		ULONG_PTR tid = *(ULONG_PTR*)InputData;
		Status = KxTerminateThreadWithTid(tid, 0);
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = sizeof(Status);
		break;
	}
	case CTL_FORCE_TREMINATE_PROCESS_APC: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		Status = KxTerminateProcessWithPidAndApc(pid, 0);
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = OutputDataLength;
		break;
	}
	case CTL_FORCE_TREMINATE_THREAD_APC: {
		ULONG_PTR tid = *(ULONG_PTR*)InputData;
		Status = KxTerminateThreadWithTidAndApc(tid, 0);
		_memcpy_s(OutputData, OutputDataLength, &Status, sizeof(Status));
		Informaiton = OutputDataLength;
		break;
	}
	case CTL_ADD_PROCESS_PROTECT: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		KxProtectProcessWithPid((HANDLE)pid);
		break;
	}	
	case CTL_REMOVE_PROCESS_PROTECT: {
		ULONG_PTR pid = *(ULONG_PTR*)InputData;
		KxUnProtectProcessWithPid((HANDLE)pid);
		break;
	}
	case CTL_GET_KERNEL_MODULS: {
		BOOLEAN start = *(UCHAR*)InputData;
		if (start) {
			ListEntryScan = ListEntry;
			LoadedModuleOrder = 0;
			KdPrint(("CTL_GET_KERNEL_MODULS start"));
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

NTSTATUS InitKernel(ULONG parm) 
{
	NTSTATUS status = STATUS_SUCCESS;
	if (!kernelInited) {
		KdPrint(("InitKernel ! System ver : %u\n", parm));

		status = KxGetFunctions(parm);

		kernelInited = TRUE;
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

	RtlInitUnicodeString(&MemsetName, L"memset");
	RtlInitUnicodeString(&WCscatsName, L"wcscat_s");
	RtlInitUnicodeString(&WCscpysName, L"wcscpy_s");
	RtlInitUnicodeString(&SWprintfsName, L"swprintf_s");
	RtlInitUnicodeString(&MemcpysName, L"memcpy_s");
	RtlInitUnicodeString(&StrcpysName, L"strcpy_s");
	RtlInitUnicodeString(&StrcatsName, L"strcat_s");

	_memset = (memset_)MmGetSystemRoutineAddress(&MemsetName);
	_wcscpy_s = (wcscpy_s_)MmGetSystemRoutineAddress(&WCscpysName);
	_wcscat_s = (wcscat_s_)MmGetSystemRoutineAddress(&WCscatsName);
	_memcpy_s = (memcpy_s_)MmGetSystemRoutineAddress(&MemcpysName);
	_strcat_s = (strcat_s_)MmGetSystemRoutineAddress(&StrcatsName);
	_strcpy_s = (strcpy_s_)MmGetSystemRoutineAddress(&StrcpysName);
	swprintf_s = (swprintf_s_)MmGetSystemRoutineAddress(&SWprintfsName);
	
}



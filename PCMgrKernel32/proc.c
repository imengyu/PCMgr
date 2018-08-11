#include "proc.h"
#include "protect.h"

extern PspTerminateThreadByPointer_ PspTerminateThreadByPointer;
extern PspExitThread_ PspExitThread;
extern PsGetNextProcessThread_ PsGetNextProcessThread;
extern PsTerminateProcess_ PsTerminateProcess;
extern KeForceResumeThread_ KeForceResumeThread;

extern ULONG_PTR EPROCESS_ThreadListHead_Offest;
extern ULONG_PTR EPROCESS_RundownProtect_Offest;
extern ULONG_PTR EPROCESS_Flags_Offest;
extern ULONG_PTR EPROCESS_SeAuditProcessCreationInfo_Offest;
extern ULONG_PTR ETHREAD_Tcb_Offest;
extern ULONG_PTR ETHREAD_CrossThreadFlags_Offest;
extern ULONG_PTR PEB_Ldr_Offest;
extern ULONG_PTR PEB_ProcessParameters_Offest;
extern ULONG_PTR RTL_USER_PROCESS_PARAMETERS_CommandLine_Offest;

EXTERN_C PETHREAD KxGetNextProcessThread_x64Call(PEPROCESS Process, PETHREAD Thread);
EXTERN_C PETHREAD KxGetNextProcessThread_x86Call(PEPROCESS Process, PETHREAD Thread);

#ifdef _AMD64_
PETHREAD KxGetNextProcessThread_x86Call(PEPROCESS Process, PETHREAD Thread)
{
	PETHREAD result = NULL;
	result = PsGetNextProcessThread(Process, Thread);
	return result; 
}
#else
PETHREAD KxGetNextProcessThread_x64Call(PEPROCESS Process, PETHREAD Thread)
{
	return PsGetNextProcessThread(Process, Thread);
}
#endif

VOID  KernelKillThreadRoutine(IN PKAPC Apc, IN OUT PKNORMAL_ROUTINE *NormalRoutine, IN OUT PVOID *NormalContext, IN OUT PVOID *SystemArgument1, IN OUT PVOID *SystemArgument2)
{
	PspExitThread(STATUS_SUCCESS);
}

PUNICODE_STRING KxGetProcessCommandLine(PEPROCESS Process)
{
	if (PEB_Ldr_Offest && PEB_ProcessParameters_Offest && RTL_USER_PROCESS_PARAMETERS_CommandLine_Offest)
	{
		ULONG_PTR pebAddress = (ULONG_PTR)PsGetProcessPeb(Process);
		if (pebAddress)
		{
			ULONG_PTR pebLdrAddress = pebAddress + PEB_Ldr_Offest;
			ULONG_PTR pebLdrRtlUserParamtersAddress = (ULONG_PTR)(*((ULONG_PTR*)pebLdrAddress) + PEB_ProcessParameters_Offest);
		
			ULONG_PTR pebLdrRtlUserParamtersCommandLineAddress = (ULONG_PTR)(*((ULONG_PTR*)pebLdrRtlUserParamtersAddress) + RTL_USER_PROCESS_PARAMETERS_CommandLine_Offest);
			PUNICODE_STRING strCommandLine = (PUNICODE_STRING)pebLdrRtlUserParamtersCommandLineAddress;
			return strCommandLine;
		}
	}
	return NULL;
}
PUNICODE_STRING KxGetProcessFullPath(PEPROCESS Process)
{
	if (EPROCESS_SeAuditProcessCreationInfo_Offest)
	{
		//获取 _SE_AUDIT_PROCESS_CREATION_INFO
		ULONG_PTR SEAuditValue = *(ULONG_PTR*)((ULONG_PTR)Process + EPROCESS_SeAuditProcessCreationInfo_Offest);
		//获取_OBJECT_NAME_INFORMATION指针
		ULONG_PTR* pNameInfo = (ULONG_PTR*)SEAuditValue;
		PUNICODE_STRING lpPath = (PUNICODE_STRING)(PVOID)pNameInfo;
		return lpPath;
	}
	return NULL;
}

VOID KxForceResumeThread(PETHREAD Thread)
{
	PKTHREAD kThread = (PKTHREAD)((ULONG_PTR)Thread + ETHREAD_Tcb_Offest);
	KeForceResumeThread(kThread);
}
NTSTATUS KxTerminateThread(PETHREAD Thread)
{
	return PspTerminateThreadByPointer(Thread, 0, FALSE);
}
NTSTATUS KxTerminateThreadApc(PETHREAD Thread)
{
	NTSTATUS Status = STATUS_SUCCESS;
	PKAPC ExitApc = NULL;
	ULONG    OldMask;

	if (!Thread) return STATUS_INVALID_PARAMETER;

	if (Thread == PsGetCurrentThread()) {
		if (ETHREAD_CrossThreadFlags_Offest != 0)
		PS_SET_BITS((ULONG_PTR*)((ULONG_PTR)Thread + ETHREAD_CrossThreadFlags_Offest), PS_CROSS_THREAD_FLAGS_TERMINATED);
		PspExitThread(0);
	}
	else {
		if (PsIsThreadTerminating(Thread))
			return STATUS_THREAD_IS_TERMINATING;
		if (PsIsSystemThread(Thread))
			return STATUS_ACCESS_DENIED;

		if (PspExitThread == NULL)
			//使用 PspTerminateThreadByPointer 结束
			return PspTerminateThreadByPointer(Thread, 0, FALSE);

		Status = STATUS_SUCCESS;

		ExitApc = (PKAPC)ExAllocatePoolWithTag(NonPagedPool, sizeof(KAPC), 0);
		if (ExitApc == NULL)
		{
			KdPrint(("[KillProcessWithApc] malloc memory failed \n"));
			return STATUS_UNSUCCESSFUL;
		}

		if (ETHREAD_CrossThreadFlags_Offest != 0)
		OldMask = PS_TEST_SET_BITS((ULONG_PTR*)((ULONG_PTR)Thread + ETHREAD_CrossThreadFlags_Offest), PS_CROSS_THREAD_FLAGS_TERMINATED);

		if ((OldMask & PS_CROSS_THREAD_FLAGS_TERMINATED) == 0) {
			//为线程初始化APC
			KeInitializeApc(ExitApc, Thread, OriginalApcEnvironment, KernelKillThreadRoutine, NULL, NULL, KernelMode, NULL);
			if (!KeInsertQueueApc(ExitApc, ExitApc, NULL, 2))
			{
				ExFreePool(ExitApc);
				Status = STATUS_UNSUCCESSFUL;
			}
			else
			{
				if (KeForceResumeThread != 0 && ETHREAD_Tcb_Offest != 0)
				{
					//WRK 抄来的，强制恢复暂停的线程，保证线程可以运行插入的apc
					KxForceResumeThread(Thread);
				}
			}
		}
		else {
			ExFreePool(ExitApc);
		}
	}

	return Status;
}

NTSTATUS KxTerminateThreadWithTid(ULONG_PTR tid, ULONG exitCode, BOOLEAN useapc)
{
	NTSTATUS Status = STATUS_SUCCESS;
	PETHREAD Thread;

	Status = PsLookupThreadByThreadId((HANDLE)tid, &Thread);
	if (!NT_SUCCESS(Status))
		return Status;

	if (!PsIsThreadTerminating(Thread)) {
		ObDereferenceObject(Thread);
		Status = STATUS_THREAD_IS_TERMINATING;
		return Status;
	}

	if (Thread == PsGetCurrentThread())
	{
		Status = PspExitThread(0);
	}
	else 
	{
		if (useapc) Status = KxTerminateThreadApc(Thread);
		else Status = KxTerminateThread(Thread);
	}

	ObDereferenceObject(Thread);
	return Status;
}
NTSTATUS KxTerminateProcessWithPid(ULONG_PTR pid, ULONG exitCode, BOOLEAN usepst, BOOLEAN useapc)
{
	NTSTATUS Status = STATUS_SUCCESS;
	if (pid <= 4)
		return STATUS_ACCESS_DENIED;

	PEPROCESS Process;
	PEPROCESS CurrentProcess;
	PETHREAD Thread = NULL;
	PETHREAD Self;

	Self = PsGetCurrentThread();
	CurrentProcess = IoThreadToProcess(Self);

	Status = PsLookupProcessByProcessId((HANDLE)pid, &Process);
	if (!NT_SUCCESS(Status))
		return Status;
	 
	//Using PsTerminateProcess
	if (usepst && PsTerminateProcess != 0)
		return PsTerminateProcess(Process, exitCode);

	//Set deleting Flag
	if (EPROCESS_Flags_Offest != 0)
	{
		//
		// Mark process as deleting
		//
		PS_SET_BITS((ULONG_PTR*)((ULONG_PTR)Process + EPROCESS_Flags_Offest), PS_PROCESS_FLAGS_PROCESS_DELETE);
	}

	Status = STATUS_NOTHING_TO_TERMINATE;

	//RundownProtect on
	if (EPROCESS_RundownProtect_Offest != 0) {
		if (!ExAcquireRundownProtection((PEX_RUNDOWN_REF)(VOID*)((ULONG_PTR)Process + EPROCESS_RundownProtect_Offest))) {
			//锁住进程失败，减少引用次数
			ObDereferenceObject(Process);
			return STATUS_PROCESS_IS_TERMINATING;
		}
	}

	//Force Kill route
	if (PsGetNextProcessThread == 0) {
		//暴力搜索线程
		int i;
		for (i = 8; i < 65536; i += 4)
		{
			Status = PsLookupThreadByThreadId((HANDLE)(ULONG_PTR)i, &Thread);
			if (NT_SUCCESS(Status))
			{
				if (Thread != Self) {
					//比对是否是当前的进程
					if (Process == IoThreadToProcess(Thread))
					{
						if (useapc) Status = KxTerminateThreadApc(Thread);
						else Status = KxTerminateThread(Thread);
					}
				}
				ObDereferenceObject(Thread);
			}
		}
		Status = STATUS_SUCCESS;
	}
	else {
		//PsGetNextProcessThread获取线程
#ifdef _AMD64_
		for (Thread = KxGetNextProcessThread_x64Call(Process, NULL); Thread != NULL; Thread = KxGetNextProcessThread_x64Call(Process, Thread))
#else
		for (Thread = KxGetNextProcessThread_x86Call(Process, NULL); Thread != NULL; Thread = KxGetNextProcessThread_x86Call(Process, Thread))
#endif	
		{
			if (Thread != Self) {
				if (useapc) Status = KxTerminateThreadApc(Thread);
				else Status = KxTerminateThread(Thread);
			}
		}
		
	}

	//RundownProtect off
	if (EPROCESS_RundownProtect_Offest != 0) {
		ExReleaseRundownProtection((PEX_RUNDOWN_REF)(VOID*)((ULONG_PTR)Process + EPROCESS_RundownProtect_Offest));
	}

	if (Process == CurrentProcess)
		Status = PspTerminateThreadByPointer(Self, 0, TRUE);

	//if (Status == STATUS_NOTHING_TO_TERMINATE) {
	//	ObClearProcessHandleTable(Process);
	//	Status = STATUS_SUCCESS;
	//}

	Status = STATUS_SUCCESS;

	ObDereferenceObject(Process);
	return Status;
}

NTSTATUS KxTerminateProcessByZero(ULONG_PTR PID)
{
	NTSTATUS ntStatus = STATUS_SUCCESS;
	if (PID < 4)return STATUS_ACCESS_DENIED;
	int i = 0;
	PVOID handle;
	PEPROCESS Eprocess;
	ntStatus = PsLookupProcessByProcessId((HANDLE)PID, &Eprocess);

	if (NT_SUCCESS(ntStatus))
	{
		KeAttachProcess(Eprocess);//Attach进程虚拟空间
		for (i = 0; i <= 0x7fffffff; i += 0x1000)
		{
			if (MmIsAddressValid((PVOID)(ULONG_PTR)i))
			{
				ProbeForWrite((PVOID)(ULONG_PTR)i, 0x1000, sizeof(ULONG));
				memset((PVOID)(ULONG_PTR)i, 0xcc, 0x1000);
			}
			else
			{
				if (i>0x1000000)  //填这么多足够破坏进程数据了
					break;
			}
		}

		KeDetachProcess();

		ntStatus = ObOpenObjectByPointer((PVOID)Eprocess, 0, NULL, 0, NULL, KernelMode, &handle);
		if (ntStatus != STATUS_SUCCESS)
			return ntStatus;
		ZwTerminateProcess((HANDLE)handle, STATUS_SUCCESS);
		ZwClose((HANDLE)handle);
		return ntStatus;
	}


	return ntStatus;
}
NTSTATUS KxTerminateProcessTest(ULONG_PTR PID) 
{
	NTSTATUS status = STATUS_SUCCESS;

	KdPrint(("KxTerminateProcessTest STATUS_SUCCESS "));
	return status;
}

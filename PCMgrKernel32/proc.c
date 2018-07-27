#include "proc.h"
#include "protect.h"

extern PspTerminateThreadByPointer_ PspTerminateThreadByPointer;
extern PspExitThread_ PspExitThread;

VOID  KernelKillThreadRoutine(IN PKAPC Apc, IN OUT PKNORMAL_ROUTINE *NormalRoutine, IN OUT PVOID *NormalContext, IN OUT PVOID *SystemArgument1, IN OUT PVOID *SystemArgument2)
{
	PspExitThread(STATUS_SUCCESS);
}

NTSTATUS KxTerminateThreadWithTidAndApc(ULONG_PTR tid, ULONG exitCode)
{
	NTSTATUS status = STATUS_SUCCESS;	
	PKAPC ExitApc = NULL;
	PETHREAD pEThread;
	status = PsLookupThreadByThreadId((HANDLE)tid, &pEThread);
	if (NT_SUCCESS(status)) {
		if (PspExitThread != NULL) {
			ExitApc = (PKAPC)ExAllocatePoolWithTag(NonPagedPool, sizeof(KAPC), 0);
			if (ExitApc == NULL) {
				KdPrint(("[KillProcessWithApc] malloc memory failed \n"));
				status = STATUS_UNSUCCESSFUL;
				return status;
			}
			//为线程初始化APC
			KeInitializeApc(ExitApc, pEThread, OriginalApcEnvironment, KernelKillThreadRoutine, NULL, NULL, KernelMode, NULL);
			status = KeInsertQueueApc(ExitApc, ExitApc, NULL, 2);

			if (!NT_SUCCESS(status))KdPrint(("[KeInsertQueueApc] failed : 0x%08X \n", status));
		}
		else status = PspTerminateThreadByPointer(pEThread, 0, FALSE);
		ObDereferenceObject(pEThread);
	}
	return status;
}
NTSTATUS KxTerminateThreadWithTid(ULONG_PTR tid, ULONG exitCode)
{
	NTSTATUS status = STATUS_SUCCESS;
	PETHREAD pEThread;

	KdPrint(("KxTerminateThreadWithTid : %u", tid));
	status = PsLookupThreadByThreadId((HANDLE)tid, &pEThread);
	if (NT_SUCCESS(status)) {
		status = PspTerminateThreadByPointer(pEThread, 0, FALSE);
		ObDereferenceObject(pEThread);
	}
	return status;
}
NTSTATUS KxTerminateProcessWithPidAndApc(ULONG_PTR pid, ULONG exitCode)
{
	NTSTATUS status = STATUS_SUCCESS;
	PEPROCESS pEProc;

	if (KxIsProcessProtect((HANDLE)pid)) return STATUS_ACCESS_DENIED;
	if (PspExitThread == NULL) return KxTerminateProcessWithPid(pid, exitCode);

	status = PsLookupProcessByProcessId((HANDLE)pid, &pEProc);
	if (NT_SUCCESS(status))
	{
		PETHREAD pEThread;
		int i;
		for (i = 8; i < 65536; i += 4)
		{
			status = PsLookupThreadByThreadId((HANDLE)(ULONG_PTR)i, &pEThread);
			//获取成功看一下线程是否属于当前的进程
			if (NT_SUCCESS(status))
			{
				//比对一下看看是不是当前的进程
				if (pEProc == IoThreadToProcess(pEThread))
				{
					PKAPC ExitApc = (PKAPC)ExAllocatePoolWithTag(NonPagedPool, sizeof(KAPC), 0);
					if (ExitApc == NULL) {
						KdPrint(("[KillProcessWithApc] malloc memory failed \n"));
						status = STATUS_UNSUCCESSFUL;
						return status;
					}
					//为线程初始化APC
					KeInitializeApc(ExitApc, pEThread, OriginalApcEnvironment, KernelKillThreadRoutine, NULL, NULL, KernelMode, NULL);
					status = KeInsertQueueApc(ExitApc, ExitApc, NULL, 2);
				}
				ObDereferenceObject(pEThread);
			}
		}

		ObDereferenceObject(pEProc);
		status = STATUS_SUCCESS;
	}
	return status;
}
NTSTATUS KxTerminateProcessWithPid(ULONG_PTR pid, ULONG exitCode)
{
	NTSTATUS status = STATUS_SUCCESS;
	PEPROCESS pEProc;

	KdPrint(("KxTerminateProcessWithPid : %u", pid));

	if (KxIsProcessProtect((HANDLE)pid)) return STATUS_ACCESS_DENIED;
	status = PsLookupProcessByProcessId((HANDLE)pid, &pEProc);
	if (NT_SUCCESS(status))
	{
		PETHREAD pEThread;
		int i;
		for (i = 8; i < 65536; i += 4)
		{
			status = PsLookupThreadByThreadId((HANDLE)(ULONG_PTR)i, &pEThread);
			if (NT_SUCCESS(status))
			{
				//比对是否是当前的进程
				if (pEProc == IoThreadToProcess(pEThread))
					status = PspTerminateThreadByPointer(pEThread, exitCode, FALSE);
				ObDereferenceObject(pEThread);
			}
		}

		ObDereferenceObject(pEProc);
	}
	return status;
}
NTSTATUS KxTerminateThread(HANDLE hThread, ULONG exitCode)
{
	NTSTATUS status = STATUS_SUCCESS;
	THREAD_BASIC_INFORMATION tbi;
	status = ZwQueryInformationThread(hThread, ThreadBasicInformation, (PVOID)&tbi, sizeof(ThreadBasicInformation), NULL);
	if (NT_SUCCESS(status))
		status = KxTerminateThreadWithTid((ULONG_PTR)tbi.ClientId.UniqueThread, exitCode);
	return status;
}
NTSTATUS KxTerminateProcess(HANDLE hProcess, ULONG exitCode)
{
	NTSTATUS status = STATUS_SUCCESS;
	status = ZwTerminateProcess(hProcess, exitCode);
	if (status == STATUS_ACCESS_DENIED) {
		PROCESS_BASIC_INFORMATION pbi;
		status = ZwQueryInformationProcess(hProcess, ProcessBasicInformation, (PVOID)&pbi, sizeof(ProcessBasicInformation), NULL);
		if (NT_SUCCESS(status))
			status = KxTerminateProcessWithPid(pbi.UniqueProcessId, exitCode);
	}
	return status;
}
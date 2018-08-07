#include "handle.h"

NTSTATUS KxForceCloseHandle(HANDLE pid, HANDLE HandleValue)
{
	KAPC_STATE KapcState;
	OBJECT_HANDLE_FLAG_INFORMATION ObjectHandleFlagInfo;
	NTSTATUS Status;
	PEPROCESS pEprocess = NULL;
	ULONG64 Value = (ULONG64)HandleValue;


	Status = PsLookupProcessByProcessId(pid, &pEprocess);
	if (!NT_SUCCESS(Status))
		return Status;

	if (pEprocess == NULL || !MmIsAddressValid(pEprocess))
		return Status;

	KeStackAttachProcess(pEprocess, &KapcState);
	ObDereferenceObject(pEprocess);

#define KERNEL_HANDLE_MASK ((ULONG_PTR)((LONG)0x80000000))//¹Ø±ÕÄÚºË¾ä±ú

	if (PsGetCurrentProcess() == PsInitialSystemProcess)
	{
		Value |= (ULONG64)KERNEL_HANDLE_MASK;
		HandleValue = (HANDLE)Value;
	}

	ObjectHandleFlagInfo.Inherit = 0;
	ObjectHandleFlagInfo.ProtectFromClose = 0;
	Status = ObSetHandleAttributes(HandleValue, &ObjectHandleFlagInfo, KernelMode);
	if (!NT_SUCCESS(Status))
	{
		KdPrint(("ObSetHandleAttributes failed 0x%x\n", Status));
	}
	KeUnstackDetachProcess(&KapcState);

	Status = ZwClose(HandleValue);
	if (!NT_SUCCESS(Status))
	{
		KdPrint(("ZwClose failed 0x%x\n", Status));
		return Status;
	}
	return Status;
}

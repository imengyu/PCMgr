#pragma once
#include <ntifs.h>
#include "defs.h"
#include "istructs.h"

typedef struct tag_DBGPRINT_DATA
{
	CHAR StrBuffer[128];
	void * Next;
}DBGPRINT_DATA, *PDBGPRINT_DATA;

NTSTATUS DriverEntry(IN PDRIVER_OBJECT pDriverObject, IN PUNICODE_STRING pRegPath);
VOID DriverUnload(_In_ struct _DRIVER_OBJECT *DriverObject);

NTSTATUS IOControlDispatch(IN PDEVICE_OBJECT pDeviceObject, IN PIRP Irp);
NTSTATUS CreateDispatch(IN PDEVICE_OBJECT DeviceObject, IN PIRP Irp);

NTSTATUS InitKernel(PKINITAGRS parm);

VOID KxLoadFunctions();
NTSTATUS KxInitMyDbgView();
void KxMyDbgViewReset();
BOOLEAN KxMyDbgViewWorking();
NTSTATUS KxUnInitMyDbgView();





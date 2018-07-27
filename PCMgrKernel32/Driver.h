#pragma once
#include <ntifs.h>
#include "defs.h"

NTSTATUS DriverEntry(IN PDRIVER_OBJECT pDriverObject, IN PUNICODE_STRING pRegPath);

VOID DriverUnload(_In_ struct _DRIVER_OBJECT *DriverObject);

NTSTATUS IOControlDispatch(PDEVICE_OBJECT pDeviceObject, PIRP Irp);

NTSTATUS CreateDispatch(IN PDEVICE_OBJECT DeviceObject, IN PIRP Irp);

NTSTATUS InitKernel(ULONG parm);

VOID KxLoadFunctions();



#pragma once
#include <ntddk.h>

NTSTATUS DriverEntry(IN PDRIVER_OBJECT pDriverObject, IN PUNICODE_STRING pRegPath);

VOID DriverUnload(_In_ struct _DRIVER_OBJECT *DriverObject);
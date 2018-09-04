#pragma once
#include <fltKernel.h>

VOID DriverUnload(_In_ struct _DRIVER_OBJECT *pDriverObject);

NTSTATUS IOControlDispatch(IN PDEVICE_OBJECT DeviceObject, IN PIRP Irp);
NTSTATUS CreateDispatch(IN PDEVICE_OBJECT DeviceObject, IN PIRP Irp);

NTSTATUS KxFilterInstanceSetup(__in PCFLT_RELATED_OBJECTS FltObjects, __in FLT_INSTANCE_SETUP_FLAGS Flags, __in DEVICE_TYPE VolumeDeviceType, __in FLT_FILESYSTEM_TYPE VolumeFilesystemType);
VOID KxFilterInstanceTeardownStart(__in PCFLT_RELATED_OBJECTS FltObjects, __in FLT_INSTANCE_TEARDOWN_FLAGS Flags);
VOID KxFilterInstanceTeardownComplete(__in PCFLT_RELATED_OBJECTS FltObjects, __in FLT_INSTANCE_TEARDOWN_FLAGS Flags);
NTSTATUS KxFilterInstanceQueryTeardown(__in PCFLT_RELATED_OBJECTS FltObjects, __in FLT_INSTANCE_QUERY_TEARDOWN_FLAGS Flags);
NTSTATUS KxFilterUnload(__in FLT_FILTER_UNLOAD_FLAGS Flags);
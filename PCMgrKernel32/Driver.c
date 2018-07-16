#include "Driver.h"

#define DOS_DEVICE_NAME L"\\DosDevices\\PCMGRK"
#define NT_DEVICE_NAME L"\\Device\\PCMGRK"

NTSTATUS DriverEntry(IN PDRIVER_OBJECT pDriverObject, IN PUNICODE_STRING pRegPath)
{
	NTSTATUS ntStatus;

	KdPrint(("Enter DriverEntry!\n"));

	UNICODE_STRING ntUnicodeString; // NT Device Name 
	UNICODE_STRING ntWin32NameString; // Win32 Name 
	PDEVICE_OBJECT deviceObject = NULL; 
	RtlInitUnicodeString(&ntUnicodeString, NT_DEVICE_NAME);

	ntStatus = IoCreateDevice(
		pDriverObject,
		0, 
		&ntUnicodeString,
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
	
	//创建驱动设备对象	
	RtlInitUnicodeString(&ntWin32NameString, DOS_DEVICE_NAME);
	ntStatus = IoCreateSymbolicLink(
		&ntWin32NameString, &ntUnicodeString);
	if (!NT_SUCCESS(ntStatus))
	{
		KdPrint(("Couldn't create symbolic link\n"));
		IoDeleteDevice(deviceObject);
	}

	KdPrint(("DriverEntry end!\n"));
	return ntStatus;
}

VOID DriverUnload(_In_ struct _DRIVER_OBJECT *DriverObject)
{
	KdPrint(("DriverUnload\n"));
}
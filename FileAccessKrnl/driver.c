#include "driver.h"
#include "fsioctls.h"
#include "fsppvk.h"
#include "..\FileAccess\fsdrvstructs.h"
#include "..\FileAccess\fsppv.h"

#define DEVICE_LINK_NAME L"\\??\\PCMGRFS"
#define DEVICE_OBJECT_NAME  L"\\Device\\PCMGRFS"

PFLT_FILTER gFilterHandle;

HANDLE EventFsRequestAllow;
HANDLE EventFsTryWriteProtect;
PKEVENT KEventFsRequestAllow;
PKEVENT KEventFsTryWriteProtect;

NTSTATUS DriverEntry(IN PDRIVER_OBJECT pDriverObject, IN PUNICODE_STRING pRegistryPath) 
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

	//开启过滤行为
	KxCreateFilter(pDriverObject);
	//初始化所有通知事件
	KxCreateAllEvents();
	//初始化保护链表
	KxInitPkv();

	KdPrint(("DriverEntry ok\n"));

	return ntStatus;
}
VOID DriverUnload(_In_ struct _DRIVER_OBJECT *pDriverObject)
{
	UNICODE_STRING  DeviceLinkName;
	PDEVICE_OBJECT  v1 = NULL;
	PDEVICE_OBJECT  DeleteDeviceObject = NULL;

	RtlInitUnicodeString(&DeviceLinkName, DEVICE_LINK_NAME);
	IoDeleteSymbolicLink(&DeviceLinkName);

	KxUnInitPkv();

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
	NTSTATUS Status = STATUS_SUCCESS;
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
	case CTL_MFS_INIT_USER_EVENTS: {

		break;
	}
	default:
		break;
	}

COMPLETE:
	Irp->IoStatus.Status = Status;             //Ring3 GetLastError();
	Irp->IoStatus.Information = Informaiton;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);  //将Irp返回给Io管理器
	return Status;                            //Ring3 DeviceIoControl()返回值
}
NTSTATUS CreateDispatch(IN PDEVICE_OBJECT DeviceObject, IN PIRP Irp)
{
	Irp->IoStatus.Status = STATUS_SUCCESS;
	Irp->IoStatus.Information = 0;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);
	return STATUS_SUCCESS;
}

FLT_POSTOP_CALLBACK_STATUS KxFilterPostCreate(	__inout PFLT_CALLBACK_DATA Data,	__in PCFLT_RELATED_OBJECTS FltObjects,	__in_opt PVOID CompletionContext,	__in FLT_POST_OPERATION_FLAGS Flags)
{
	FLT_POSTOP_CALLBACK_STATUS returnStatus = FLT_POSTOP_FINISHED_PROCESSING;
	PFLT_FILE_NAME_INFORMATION nameInfo;
	NTSTATUS status;
	UNREFERENCED_PARAMETER(CompletionContext);
	UNREFERENCED_PARAMETER(Flags);

	if (!NT_SUCCESS(Data->IoStatus.Status) || (STATUS_REPARSE == Data->IoStatus.Status)) {
		return FLT_POSTOP_FINISHED_PROCESSING;
	}

	status = FltGetFileNameInformation(Data, FLT_FILE_NAME_NORMALIZED | FLT_FILE_NAME_QUERY_DEFAULT, &nameInfo);
	
	if (!NT_SUCCESS(status))
		return FLT_POSTOP_FINISHED_PROCESSING;

	return returnStatus;
}
FLT_PREOP_CALLBACK_STATUS KxFilterPreCreate(__inout PFLT_CALLBACK_DATA Data, __in PCFLT_RELATED_OBJECTS FltObjects, __deref_out_opt PVOID *CompletionContext)
{
	NTSTATUS status;
	PFLT_FILE_NAME_INFORMATION nameInfo;
	UNREFERENCED_PARAMETER(FltObjects);
	UNREFERENCED_PARAMETER(CompletionContext);

	status = FltGetFileNameInformation(Data, FLT_FILE_NAME_NORMALIZED | FLT_FILE_NAME_QUERY_DEFAULT, &nameInfo);
	if (NT_SUCCESS(status))
	{
		status = FltParseFileNameInformation(nameInfo);
		if (NT_SUCCESS(status))
		{
			BOOLEAN isDir = FALSE;
			FltIsDirectory(FltObjects->FileObject, FltObjects->Instance, &isDir);

			PMFS_PROTECT protect = NULL;
			BOOLEAN isProtect = KxIsPathInProtect(&nameInfo->Name, isDir, &protect);
			if (protect)
			{

			}

			FltReleaseFileNameInformation(nameInfo);
		}
	}

	return FLT_PREOP_SUCCESS_WITH_CALLBACK;
}

const FLT_OPERATION_REGISTRATION KxFilterCallbacks[] = {
	{ IRP_MJ_CREATE,
	0,
	KxFilterPreCreate,//生成预操作回调函数
	KxFilterPostCreate },//生成后操作回调函数

{ IRP_MJ_OPERATION_END }//告诉过滤器元素截止
};
const FLT_REGISTRATION FilterRegistration = {
	sizeof(FLT_REGISTRATION),         //  Size结构大小
	FLT_REGISTRATION_VERSION,           //  Version版本
	0,                                  //  Flags微过滤器标志位
	NULL,                               //  Context操作回调函数
	KxFilterCallbacks,                        //  Operation callbacks卸载回调函数
	KxFilterUnload,                           //  MiniFilterUnload实例安装回调
	KxFilterInstanceSetup,                    //  InstanceSetup
	KxFilterInstanceQueryTeardown,            //  InstanceQueryTeardown
	KxFilterInstanceTeardownStart,            //  InstanceTeardownStart
	KxFilterInstanceTeardownComplete,         //  InstanceTeardownComplete
	NULL,                               //  GenerateFileName
	NULL,                               //  GenerateDestinationFileName
	NULL                                //  NormalizeNameComponent
};

//Create mini Filter
NTSTATUS KxCreateFilter(IN PDRIVER_OBJECT pDriverObject)
{
	PSECURITY_DESCRIPTOR sd;
	OBJECT_ATTRIBUTES oa;
	UNICODE_STRING uniString;
	NTSTATUS ntStatus = FltRegisterFilter(pDriverObject,//向过滤管理器注册一个过滤器
		&FilterRegistration,
		&gFilterHandle);

	FLT_ASSERT(NT_SUCCESS(ntStatus));

	if (NT_SUCCESS(ntStatus))
	{
		ntStatus = FltStartFiltering(gFilterHandle);

		//如果开启失败，取消注册
		if (!NT_SUCCESS(ntStatus))
			FltUnregisterFilter(gFilterHandle);
		else {

			ntStatus = FltBuildDefaultSecurityDescriptor(&sd, FLT_PORT_ALL_ACCESS);
			RtlInitUnicodeString(&uniString, L"PCMGRFSFILTER");

			InitializeObjectAttributes(&oa, &uniString, OBJ_KERNEL_HANDLE | OBJ_CASE_INSENSITIVE, NULL, sd);
			FltFreeSecurityDescriptor(sd);

			KdPrint(("FltStartFiltering ok\n"));
		}
	}

	return ntStatus;
}

//Filter Props
NTSTATUS KxFilterInstanceSetup(__in PCFLT_RELATED_OBJECTS FltObjects, __in FLT_INSTANCE_SETUP_FLAGS Flags, __in DEVICE_TYPE VolumeDeviceType, __in FLT_FILESYSTEM_TYPE VolumeFilesystemType)
{
	return STATUS_SUCCESS;
}
VOID KxFilterInstanceTeardownStart(__in PCFLT_RELATED_OBJECTS FltObjects, __in FLT_INSTANCE_TEARDOWN_FLAGS Flags)
{

}
VOID KxFilterInstanceTeardownComplete(__in PCFLT_RELATED_OBJECTS FltObjects, __in FLT_INSTANCE_TEARDOWN_FLAGS Flags)
{

}
NTSTATUS KxFilterInstanceQueryTeardown(__in PCFLT_RELATED_OBJECTS FltObjects, __in FLT_INSTANCE_QUERY_TEARDOWN_FLAGS Flags)
{
	return STATUS_SUCCESS;
}
NTSTATUS KxFilterUnload(__in FLT_FILTER_UNLOAD_FLAGS Flags)
{
	UNREFERENCED_PARAMETER(Flags);
	PAGED_CODE();
	FltUnregisterFilter(gFilterHandle);
	return STATUS_SUCCESS;
}

VOID KxCreateAllEvents()
{
	UNICODE_STRING EventNameFsRequestAllow;
	UNICODE_STRING EventNameFsTryWriteProtect;

	RtlInitUnicodeString(&EventNameFsRequestAllow, L"//BaseNamedObjects//MFsRequestAllow");
	RtlInitUnicodeString(&EventNameFsTryWriteProtect, L"//BaseNamedObjects//MFsTryWriteProtect");

	KEventFsRequestAllow = IoCreateNotificationEvent(&EventNameFsRequestAllow, &EventFsRequestAllow);
	KeClearEvent(KEventFsRequestAllow);

	KEventFsTryWriteProtect = IoCreateNotificationEvent(&EventNameFsTryWriteProtect, &EventFsTryWriteProtect);
	KeClearEvent(KEventFsTryWriteProtect);
	


}

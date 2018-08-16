#include "kmodul.h"

extern wcscpy_s_ _wcscpy_s;
extern wcscat_s_ _wcscat_s;

NTSTATUS KxUnLoadDrvObjectByDrvObject(ULONG_PTR pDrvObject)
{
	NTSTATUS status = STATUS_UNSUCCESSFUL;


	return status;
}
NTSTATUS KxGetDrvObjectByName(wchar_t *pszDrvName, ULONG_PTR* pDrvObject)
{
	NTSTATUS status = STATUS_SUCCESS;
	UNICODE_STRING uniObjName, uniFileDev;

	wchar_t dev[256] = { 0 };
	wchar_t sym[256] = { 0 };
	wchar_t fileDev[256] = { 0 };
	wchar_t fileSym[256] = { 0 };
	//驱动对象
	PDRIVER_OBJECT pDrvObj = NULL;

	//清零指针内容
	*pDrvObject = 0;

	//非空则转Unicode驱动名
	if (pszDrvName[0] != '\0')
	{
		_wcscpy_s(dev, 256, L"\\Driver\\");
		// "\Driver\xxx.sys"
		_wcscat_s(dev, 256, pszDrvName);

		_wcscpy_s(fileDev, 256, L"\\FileSystem\\");
		//"\FileSystem\xxx.sys"
		_wcscat_s(fileDev, 256, pszDrvName);

		//去除后缀名(减去后4个字符，"\Driver\xxx")
		RtlMoveMemory(sym, dev, (wcslen(dev) - 4) * sizeof(wchar_t));
		//去除后缀名(减去后4个字符，"\FileSystem\xxx")
		RtlMoveMemory(fileSym, fileDev, (wcslen(dev) - 4) * sizeof(wchar_t));

		RtlInitUnicodeString(&uniObjName, sym);
		RtlInitUnicodeString(&uniFileDev, fileSym);
	}

	//初始化objectAttributes
	OBJECT_ATTRIBUTES objectAttributes;
	InitializeObjectAttributes(&objectAttributes, &uniObjName, OBJ_CASE_INSENSITIVE, NULL, NULL);

	HANDLE hDevice;
	IO_STATUS_BLOCK status_block;
	//设定了FILE_SYNCHRONOUS_IO_NONALERT或者FILE_SYNCHRONOUS_IO_ALERT为同步打开设备
	status = ZwCreateFile(&hDevice,
		FILE_READ_ATTRIBUTES | SYNCHRONIZE,
		&objectAttributes,
		&status_block,
		NULL, FILE_ATTRIBUTE_NORMAL, FILE_SHARE_READ,
		FILE_OPEN_IF, FILE_SYNCHRONOUS_IO_NONALERT, NULL, 0);

	if (NT_SUCCESS(status)) {
		status = ObReferenceObjectByHandle(hDevice, FILE_ALL_ACCESS, *IoDriverObjectType, KernelMode, (PVOID *)&pDrvObj, NULL);
		ZwClose(hDevice);
	}
	else
	{
		KdPrint(("ZwCreateFile (\"\\Driver\\%ws\") Failed! Status: 0x%x\n", uniObjName.Buffer, status));
		//引用对象通过名字("\Driver\xxx")
		status = ObReferenceObjectByName(&uniObjName, OBJ_CASE_INSENSITIVE, NULL, FILE_ALL_ACCESS, *IoDriverObjectType, KernelMode, NULL, (PVOID *)&pDrvObj);
		if (!NT_SUCCESS(status))
		{
			//引用对象通过名字("\FileSystem\xxx")
			status = ObReferenceObjectByName(&uniFileDev, OBJ_CASE_INSENSITIVE, NULL, FILE_ALL_ACCESS, *IoDriverObjectType, KernelMode, NULL, (PVOID *)&pDrvObj);
			if (!NT_SUCCESS(status))
			{
				KdPrint(("ObReferenceObjectByName(\"\\FileSystem\\xxx\") Failed! Status: 驱动名:%ws \t 0x%x\n", uniObjName.Buffer, status));
				return status;
			}
		}
	}
	
	//KdPrint(("驱动名:%ws \t 驱动对象:0x%08X\n", uniObjName.Buffer, pDrvObj));
	*pDrvObject = (ULONG_PTR)pDrvObj;
	//解除对象引用
	ObDereferenceObject(pDrvObj);
	return status;
}



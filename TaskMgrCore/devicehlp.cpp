#include "stdafx.h"
#include "devicehlp.h"
#include "loghlp.h"
#include "ioctls.h"
#include "mapphlp.h"
#include <comdef.h>  
#include <Wbemidl.h>  
#include <vector>  
#include <ntddstor.h>
#include "StringHlp.h"
#include "StringSplit.h"
#include "fmhlp.h"

using namespace std;

IWbemLocator *pLoc = NULL;
IWbemServices *pSvc = NULL;
BOOL wmiInited = FALSE;

M_CAPI(BOOL) MDEVICE_Init() 
{
	HRESULT hres = CoCreateInstance(CLSID_WbemLocator, 0, CLSCTX_INPROC_SERVER, IID_IWbemLocator, (LPVOID *)&pLoc);
	if (FAILED(hres))
	{
		LogErr2(L"Failed to create IWbemLocator object. Error code : 0x%X", hres);
		return 0;
	}

	hres = pLoc->ConnectServer(_bstr_t(L"ROOT\\CIMV2"), NULL, NULL, 0, NULL, 0, 0, &pSvc);
	if (FAILED(hres))
	{
		LogErr2(L"Could not connect Server.  Error code : 0x%X", hres);
		pLoc->Release();
		return 0;  
	}

	hres = CoSetProxyBlanket(
		pSvc,                        // Indicates the proxy to set  
		RPC_C_AUTHN_WINNT,           // RPC_C_AUTHN_xxx  
		RPC_C_AUTHZ_NONE,            // RPC_C_AUTHZ_xxx  
		NULL,                        // Server principal name   
		RPC_C_AUTHN_LEVEL_CALL,      // RPC_C_AUTHN_LEVEL_xxx   
		RPC_C_IMP_LEVEL_IMPERSONATE, // RPC_C_IMP_LEVEL_xxx  
		NULL,                        // client identity  
		EOAC_NONE                    // proxy capabilities   
	);
	if (FAILED(hres))
	{
		LogErr2(L"Could not set proxy blanket. Error code : 0x%X", hres);
		pSvc->Release();
		pLoc->Release();
		return FALSE; 
	}

	wmiInited = TRUE;
	return TRUE;
}
M_CAPI(void) MDEVICE_UnInit()
{
	if (pSvc)
	{
		pSvc->Release();
		pSvc = nullptr;
	}	
	if (pLoc)
	{
		pLoc->Release();
		pLoc = nullptr;
	}
	wmiInited = FALSE;
}

vector<MDevicePhysicalDisk *> diskInfos;

M_CAPI(BOOL) MDEVICE_GetLogicalDiskInfo()
{
	if (wmiInited)
	{
		if (diskInfos.size() > 0)
			MDEVICE_DestroyLogicalDiskInfo();

		IEnumWbemClassObject* pEnumerator = NULL;
		HRESULT hres = pSvc->ExecQuery(
			bstr_t("WQL"),
			bstr_t("SELECT * FROM Win32_DiskDrive"),
			WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY,
			NULL,
			&pEnumerator);

		if (FAILED(hres))
		{
			LogErr2(L"Query for operating system name failed. Error code : 0x%X", hres);
			return FALSE; 
		}

		IWbemClassObject *pclsObj = NULL;
		ULONG uReturn = 0;

		while (pEnumerator)
		{
			HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1, &pclsObj, &uReturn);
			VARIANT vtProp;

			if (0 == uReturn)
				break;

			MDevicePhysicalDisk *disk = (MDevicePhysicalDisk*)malloc(sizeof(MDevicePhysicalDisk));

			hr = pclsObj->Get(L"Name", 0, &vtProp, 0, 0);
			wcscpy_s(disk->Name, vtProp.bstrVal);
			VariantClear(&vtProp);

			hr = pclsObj->Get(L"Model", 0, &vtProp, 0, 0);
			wcscpy_s(disk->Model, vtProp.bstrVal);
			VariantClear(&vtProp);

			hr = pclsObj->Get(L"Size", 0, &vtProp, 0, 0);
			disk->Size = vtProp.ullVal;
			wcscpy_s(disk->SizeStr, vtProp.bstrVal);
			VariantClear(&vtProp);

			hr = pclsObj->Get(L"Index", 0, &vtProp, 0, 0);
			disk->Index = vtProp.uintVal;
			VariantClear(&vtProp);

			diskInfos.push_back(disk);

			pclsObj->Release();
		}

		pEnumerator->Release();
		return TRUE;
	}
	return FALSE;
}
M_CAPI(BOOL) MDEVICE_DestroyLogicalDiskInfo()
{
	if (wmiInited)
	{
		for (auto it = diskInfos.begin(); it != diskInfos.end(); it++)
			free(*it);
		diskInfos.clear();
		return TRUE;
	}
	return FALSE;
}
M_CAPI(UINT) MDEVICE_GetLogicalDiskInfoSize()
{
	return (UINT)diskInfos.size();
}
M_CAPI(BOOL) MDEVICE_GetLogicalDiskInfoItem(int index, LPWSTR nameBuffer, LPWSTR modelBuffer, UINT*outIndex, UINT64*outSize, LPWSTR sizeBuffer)
{
	if (wmiInited)
	{
		if (index >= 0 && (UINT)index < diskInfos.size())
		{
			MDevicePhysicalDisk * disk = diskInfos[index];
			if (nameBuffer)wcscpy_s(nameBuffer, 64, disk->Name);
			if (modelBuffer)wcscpy_s(modelBuffer, 64, disk->Model);
			if (outIndex)*outIndex = disk->Index;
			if (outSize)*outSize = disk->Size;
			if (sizeBuffer)wcscpy_s(sizeBuffer, 64, disk->SizeStr);
			return TRUE;
		}
	}
	return FALSE;
}

M_CAPI(DWORD) MDEVICE_GetPhysicalDriveFromPartitionLetter(CHAR letter)
{
	HANDLE hDevice;               // handle to the drive to be examined
	BOOL result;                 // results flag
	DWORD readed;                   // discard results
	STORAGE_DEVICE_NUMBER number;   //use this to get disk numbers

	CHAR path[10];
	sprintf_s(path, "\\\\.\\%c:", letter);
	hDevice = CreateFileA(path, // drive to open
		GENERIC_READ | GENERIC_WRITE,    // access to the drive
		FILE_SHARE_READ | FILE_SHARE_WRITE,    //share mode
		NULL,             // default security attributes
		OPEN_EXISTING,    // disposition
		0,                // file attributes
		NULL);            // do not copy file attribute
	if (hDevice == INVALID_HANDLE_VALUE) // cannot open the drive
	{
		LogErr2(L"CreateFile() Error: %ld", GetLastError());
		return DWORD(-1);
	}

	result = DeviceIoControl(
		hDevice,                // handle to device
		IOCTL_STORAGE_GET_DEVICE_NUMBER, // dwIoControlCode
		NULL,                            // lpInBuffer
		0,                               // nInBufferSize
		&number,           // output buffer
		sizeof(number),         // size of output buffer
		&readed,       // number of bytes returned
		NULL      // OVERLAPPED structure
	);
	if (!result) // fail
	{
		LogErr(L"IOCTL_STORAGE_GET_DEVICE_NUMBER Error: %ld", GetLastError());
		(void)CloseHandle(hDevice);
		return (DWORD)-1;
	}
	//printf("%d %d %d\n\n", number.DeviceType, number.DeviceNumber, number.PartitionNumber);

	(void)CloseHandle(hDevice);
	return number.DeviceNumber;
}
M_CAPI(BOOL) MDEVICE_GetIsSystemDisk(LPCSTR perfStr) 
{
	CHAR sysPath[MAX_PATH];
	CHAR diskLetter;
	GetSystemDirectoryA(sysPath, sizeof(sysPath));
	diskLetter = sysPath[0];
	return MStrContainsCharA(perfStr, diskLetter);
}
M_CAPI(BOOL) MDEVICE_GetIsPageFileDisk(LPCSTR perfStr)
{
	vector<string> disks;
	SplitString(string(perfStr), disks, string(" "));
	if (disks.size() > 0)
	{
		for (auto it = disks.begin(); it != disks.end(); it++)
		{
			string s = *it;
			if (s[1] == ':')
			{
				string pagefile = s + "\\pagefile.sys";
				if (MFM_FileExistA(pagefile.c_str()))
					return TRUE;
			}
		}
	}
	return FALSE;
}

MDeviceMemory memoryInfo;

M_CAPI(BOOL) MDEVICE_GetMemoryDeviceInfo()
{
	if (wmiInited)
	{
		if (diskInfos.size() > 0)
			MDEVICE_DestroyLogicalDiskInfo();

		IEnumWbemClassObject* pEnumerator = NULL;
		HRESULT hres = pSvc->ExecQuery(
			bstr_t("WQL"),
			bstr_t("SELECT * FROM Win32_PhysicalMemory"),
			WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY,
			NULL,
			&pEnumerator);

		if (FAILED(hres))
		{
			LogErr2(L"Query for (Win32_PhysicalMemory)  failed. Error code : 0x%X", hres);
			return FALSE;
		}

		IWbemClassObject *pclsObj = NULL;
		ULONG uReturn = 0;

		while (pEnumerator)
		{
			HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1, &pclsObj, &uReturn);
			VARIANT vtProp;

			if (0 == uReturn)
				break;

			hr = pclsObj->Get(L"Model", 0, &vtProp, 0, 0);
			if(vtProp.bstrVal) wcscpy_s(memoryInfo.Name, vtProp.bstrVal);
			VariantClear(&vtProp);

			hr = pclsObj->Get(L"Speed", 0, &vtProp, 0, 0);
			memoryInfo.Speed = vtProp.uintVal;
			VariantClear(&vtProp);

			pclsObj->Release();

			break;
		}

		pEnumerator->Release();
		return TRUE;
	}
	return FALSE;
}
M_CAPI(LPWSTR) MDEVICE_GetMemoryDeviceName() {
	return memoryInfo.Name;
}
M_CAPI(UINT32) MDEVICE_GetMemoryDeviceSpeed() {
	return memoryInfo.Speed;
}


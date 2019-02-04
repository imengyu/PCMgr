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
			VariantInit(&vtProp);

			if (0 == uReturn) 
				break;

			MDevicePhysicalDisk *disk = (MDevicePhysicalDisk*)MAlloc(sizeof(MDevicePhysicalDisk));

			hr = pclsObj->Get(bstr_t(L"Name"), 0, &vtProp, 0, 0);
			if (SUCCEEDED(hr) && (V_VT(&vtProp) == VT_BSTR)) wcscpy_s(disk->Name, vtProp.bstrVal);
			VariantClear(&vtProp);

			hr = pclsObj->Get(bstr_t(L"Model"), 0, &vtProp, 0, 0);
			if (SUCCEEDED(hr) && (V_VT(&vtProp) == VT_BSTR)) wcscpy_s(disk->Model, vtProp.bstrVal);
			VariantClear(&vtProp);

			hr = pclsObj->Get(bstr_t(L"Size"), 0, &vtProp, 0, 0);
			disk->Size = vtProp.ullVal;
			if (SUCCEEDED(hr) && (V_VT(&vtProp) == VT_BSTR)) wcscpy_s(disk->SizeStr, vtProp.bstrVal);
			VariantClear(&vtProp);

			hr = pclsObj->Get(bstr_t(L"Index"), 0, &vtProp, 0, 0);
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
			MFree(*it);
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

M_CAPI(DWORD) MDEVICE_GetPhysicalDriveIndexInWMI(LPWSTR perfStr) {
	if (wmiInited)
	{
		for (DWORD i = 0; i < diskInfos.size(); i++) {
			MDevicePhysicalDisk *disk = diskInfos[i];
			std::wstring index(disk->Name);
			index = index.substr(index.size() - 1);
			//	L"\\\\.\\PHYSICALDRIVE"
			if (index == perfStr)
				return i;
		}
	}
	return -1;
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
	return StringHlp::StrContainsCharA(perfStr, diskLetter);
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
UINT16 allMemoryDevice = 0;
UINT16 usedMemoryDevice = 0;

M_CAPI(BOOL) MDEVICE_GetMemoryArrayInfo()
{
	if (wmiInited)
	{
		IEnumWbemClassObject* pEnumerator = NULL;
		HRESULT hres = pSvc->ExecQuery(
			bstr_t(L"WQL"),
			bstr_t(L"SELECT * FROM Win32_PhysicalMemoryArray"),
			WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY,
			NULL,
			&pEnumerator);

		if (FAILED(hres))
		{
			LogErr2(L"Query for (Win32_PhysicalMemoryArray)  failed. Error code : 0x%X", hres);
			return FALSE;
		}

		IWbemClassObject *pclsObj = NULL;
		ULONG uReturn = 0;
		while (pEnumerator)
		{
			HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1, &pclsObj, &uReturn);
			VARIANT vtProp;
			VariantInit(&vtProp);

			if (0 == uReturn)
				break;

			hr = pclsObj->Get(bstr_t(L"MemoryDevices"), 0, &vtProp, 0, 0);
			allMemoryDevice = vtProp.uiVal;
			VariantClear(&vtProp);

			usedMemoryDevice++;

			pclsObj->Release();

		}

		pEnumerator->Release();
		return TRUE;
	}
	return FALSE;
}
M_CAPI(BOOL) MDEVICE_GetMemoryDeviceInfo()
{
	if (wmiInited)
	{
		allMemoryDevice = 0;
		usedMemoryDevice = 0;

		IEnumWbemClassObject* pEnumerator = NULL;
		HRESULT hres = pSvc->ExecQuery(
			bstr_t(L"WQL"),
			bstr_t(L"SELECT * FROM Win32_PhysicalMemory"),
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
			VariantInit(&vtProp);

			if (0 == uReturn)
				break;

			hr = pclsObj->Get(bstr_t(L"FormFactor"), 0, &vtProp, 0, 0);
			memoryInfo.FormatFactor = vtProp.uiVal;
			VariantClear(&vtProp);

			hr = pclsObj->Get(bstr_t(L"DeviceLocator"), 0, &vtProp, 0, 0);
			if (SUCCEEDED(hr) && (V_VT(&vtProp) == VT_BSTR)) wcscpy_s(memoryInfo.DeviceLocator, vtProp.bstrVal);
			VariantClear(&vtProp);

			hr = pclsObj->Get(bstr_t(L"SMBIOSMemoryType"), 0, &vtProp, 0, 0);
			if (SUCCEEDED(hr)) memoryInfo.SMBIOSMemoryType = vtProp.uintVal;
			VariantClear(&vtProp);

			hr = pclsObj->Get(bstr_t(L"Speed"), 0, &vtProp, 0, 0);
			if (SUCCEEDED(hr)) memoryInfo.Speed = vtProp.uintVal;
			VariantClear(&vtProp);

			hr = pclsObj->Get(bstr_t(L"Capacity"), 0, &vtProp, 0, 0);
			if (SUCCEEDED(hr) && (V_VT(&vtProp) == VT_BSTR)) memoryInfo.Capacity = _wtoi64(vtProp.bstrVal);
			VariantClear(&vtProp);

			pclsObj->Release();
			
		}
		
		pEnumerator->Release();

		return MDEVICE_GetMemoryArrayInfo();
	}
	return FALSE;
}
M_CAPI(LPWSTR) MDEVICE_GetMemoryFormFactorString(UINT16 _formFactor)
{
	LPWSTR formFactor = NULL;
	switch (_formFactor)
	{
	case 1:
		formFactor = L"Other";
		break;
	case 2:
		formFactor = L"SIP";
		break;
	case 3:
		formFactor = L"DIP";
		break;
	case 4:
		formFactor = L"ZIP";
		break;
	case 5:
		formFactor = L"SOJ";
		break;
	case 6:
		formFactor = L"Proprietary";
		break;
	case 7:
		formFactor = L"SIMM";
		break;
	case 8:
		formFactor = L"DIMM";
		break;
	case 9:
		formFactor = L"TSOP";
		break;
	case 10:
		formFactor = L"PGA";
		break;
	case 11:
		formFactor = L"RIMM";
		break;
	case 12:
		formFactor = L"SODIMM";
		break;
	case 13:
		formFactor = L"SRIMM";
		break;
	case 14:
		formFactor = L"SMD";
		break;
	case 15:
		formFactor = L"SSMP";
		break;
	case 16:
		formFactor = L"QFP";
		break;
	case 17:
		formFactor = L"TQFP";
		break;
	case 18:
		formFactor = L"SOIC";
		break;
	case 19:
		formFactor = L"LCC";
		break;
	case 20:
		formFactor = L"PLCC";
		break;
	case 21:
		formFactor = L"BGA";
		break;
	case 22:
		formFactor = L"FPBGA";
		break;
	case 23:
		formFactor = L"LGA";
		break;
	default:
		formFactor = L"Unknown";
		break;
	}
	return  formFactor;
}
M_CAPI(LPWSTR) MDEVICE_GetMemoryTypeString(UINT32 sMBIOSMemoryType)
{
	switch (sMBIOSMemoryType)
	{
	case 0x01:return L" Other";
	case 0x02:return L" Unknown";
	case 0x03:return L" DRAM";
	case 0x04:return L" EDRAM";
	case 0x05:return L" VRAM";
	case 0x06:return L" SRAM";
	case 0x07:return L" RAM";
	case 0x08:return L" ROM";
	case 0x09:return L" FLASH";
	case 0x0A:return L" EEPROM";
	case 0x0B:return L" FEPROM";
	case 0x0C:return L" EPROM";
	case 0x0D:return L" CDRAM";
	case 0x0E:return L" 3DRAM";
	case 0x0F:return L" SDRAM";
	case 0x10:return L" SGRAM";
	case 0x11:return L" RDRAM";
	case 0x12:return L" DDR";
	case 0x13:return L" DDR2";
	case 0x14:return L" DDR2 FB - DIMM";
	case 0x18:return L" DDR3";
	case 0x19:return L" FBD2";
	case 0x1A:return L" DDR4";
	case 0x1B:return L" LPDDR";
	case 0x1C:return L" LPDDR2";
	case 0x1D:return L" LPDDR3";
	case 0x1E:return L" LPDDR4";
	default:
		return L"";
		break;
	}
}
M_CAPI(LPWSTR) MDEVICE_GetMemoryDeviceLocator() {
	return memoryInfo.DeviceLocator;
}
M_CAPI(LPWSTR) MDEVICE_GetMemoryDeviceName() {
	if (StrEqual(memoryInfo.Name, L"")) {
		UINT64 size = memoryInfo.Capacity / 8388608;
		swprintf_s(memoryInfo.Name, L"%llu GB %s", size, MDEVICE_GetMemoryTypeString(memoryInfo.SMBIOSMemoryType));
	}
	return memoryInfo.Name;
}
M_CAPI(UINT32) MDEVICE_GetMemoryDeviceSpeed() {
	return memoryInfo.Speed;
}
M_CAPI(UINT16) MDEVICE_GetMemoryDeviceFormFactor() {
	return memoryInfo.FormatFactor;
}
M_CAPI(BOOL) MDEVICE_GetMemoryDeviceUsed(UINT16*outAll, UINT16*outUsed) 
{
	if (wmiInited && outAll && outUsed)
	{
		*outAll = allMemoryDevice;
		*outUsed = usedMemoryDevice;
		return TRUE;
	}
	return FALSE;
}

std::vector<MDeviceNetworkAdapter*> netAdapters;

MDeviceNetworkAdapter* MDEVICE_FindNetworkAdaptersInfo(LPWSTR name) {
	for (auto it = netAdapters.begin(); it != netAdapters.end(); it++)
	{
		if (StrEqual((*it)->Description, name))
			return *it;
	}
	return NULL;
}
BOOL MDEVICE_GetNetworkAdaptersIPInfo()
{
	if (wmiInited)
	{
		IEnumWbemClassObject* pEnumerator = NULL;
		HRESULT hres = pSvc->ExecQuery(
			bstr_t("WQL"),
			bstr_t("SELECT * FROM Win32_NetworkAdapterConfiguration "),
			WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY,
			NULL,
			&pEnumerator);

		if (FAILED(hres))
		{
			LogErr2(L"Query for Win32_NetworkAdapterConfiguration  failed. Error code : 0x%X", hres);
			return FALSE;
		}

		IWbemClassObject *pclsObj = NULL;
		ULONG uReturn = 0;

		while (pEnumerator)
		{
			HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1, &pclsObj, &uReturn);
			VARIANT vtProp;
			VariantInit(&vtProp);

			if (0 == uReturn)
				break;

			WCHAR name[128];
			hr = pclsObj->Get(bstr_t(L"Description"), 0, &vtProp, 0, 0);
			if (SUCCEEDED(hr) && (V_VT(&vtProp) == VT_BSTR)) wcscpy_s(name, vtProp.bstrVal);
			VariantClear(&vtProp);

			MDeviceNetworkAdapter *adapter = MDEVICE_FindNetworkAdaptersInfo(name);
			if (adapter) 
			{
				hr = pclsObj->Get(bstr_t(L"IPAddress"), 0, &vtProp, 0, 0);
				if (SUCCEEDED(hr) && (vtProp.vt == (VT_ARRAY | VT_BSTR))) {
					BSTR bstrValue = NULL;
					LONG index = 0;
					SafeArrayGetElement(vtProp.parray, &index, &bstrValue);
					wcscpy_s(adapter->IPAddressV4, (BSTR)bstrValue);
					index = 1;
					SafeArrayGetElement(vtProp.parray, &index, &bstrValue);
					wcscpy_s(adapter->IPAddressV6, (BSTR)bstrValue);
				}
				VariantClear(&vtProp);
			}
			pclsObj->Release();
		}

		pEnumerator->Release();
		return TRUE;
	}
	return FALSE;
}
M_CAPI(BOOL) MDEVICE_DestroyNetworkAdaptersInfo()
{
	if (wmiInited)
	{
		for (auto it = netAdapters.begin(); it != netAdapters.end(); it++)
			MFree(*it);
		netAdapters.clear();
		return 1;
	}
	return 0;
}
M_CAPI(UINT) MDEVICE_GetNetworkAdaptersInfo()
{
	if (wmiInited)
	{
		if (netAdapters.size() > 0)
			MDEVICE_DestroyNetworkAdaptersInfo();

		IEnumWbemClassObject* pEnumerator = NULL;
		HRESULT hres = pSvc->ExecQuery(
			bstr_t("WQL"),
			bstr_t("SELECT * FROM Win32_NetworkAdapter"),
			WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY,
			NULL,
			&pEnumerator);

		if (FAILED(hres))
		{
			LogErr2(L"Query for Win32_NetworkAdapter failed. Error code : 0x%X", hres);
			return FALSE;
		}

		IWbemClassObject *pclsObj = NULL;
		ULONG uReturn = 0;

		while (pEnumerator)
		{
			HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1, &pclsObj, &uReturn);
			VARIANT vtProp;
			VariantInit(&vtProp);

			if (0 == uReturn)
				break;

			MDeviceNetworkAdapter *adapter = (MDeviceNetworkAdapter*)MAlloc(sizeof(MDeviceNetworkAdapter));
			memset(adapter, 0, sizeof(MDeviceNetworkAdapter));

			hr = pclsObj->Get(bstr_t(L"Description"), 0, &vtProp, 0, 0);
			if (SUCCEEDED(hr) && (V_VT(&vtProp) == VT_BSTR)) wcscpy_s(adapter->Description, vtProp.bstrVal);
			VariantClear(&vtProp);

			hr = pclsObj->Get(bstr_t(L"NetEnabled"), 0, &vtProp, 0, 0);
			if (SUCCEEDED(hr)) adapter->Enabled = -vtProp.boolVal;
			VariantClear(&vtProp);

			hr = pclsObj->Get(bstr_t(L"PhysicalAdapter"), 0, &vtProp, 0, 0);
			if (SUCCEEDED(hr)) adapter->PhysicalAdapter = -vtProp.boolVal;
			VariantClear(&vtProp);

			hr = pclsObj->Get(bstr_t(L"StatusInfo"), 0, &vtProp, 0, 0);
			if (SUCCEEDED(hr)) adapter->StatusInfo = vtProp.uiVal;
			VariantClear(&vtProp);

			netAdapters.push_back(adapter);
			pclsObj->Release();
		}

		pEnumerator->Release();
		MDEVICE_GetNetworkAdaptersIPInfo();

		return static_cast<UINT>(netAdapters.size());
	}
	return FALSE;
}
M_CAPI(BOOL) MDEVICE_GetNetworkAdapterInfoItem(int index, LPWSTR name, int nameV4Size) {
	if (index >= 0 && (UINT)index < netAdapters.size()) {
		MDeviceNetworkAdapter *adapter = netAdapters[index];
		if (adapter->PhysicalAdapter)
		{
			adapter->StatusInfo;
			wcscpy_s(name, nameV4Size, adapter->Description);
			return TRUE;
		}
	}
	return FALSE;
}
M_CAPI(BOOL) MDEVICE_GetNetworkAdapterInfoFormName(LPWSTR name, LPWSTR ipAddressV4, int ipAddressV4Size, LPWSTR ipAddressV6, int ipAddressV6Size)
{
	if (wmiInited)
	{
		MDeviceNetworkAdapter*adapter = MDEVICE_FindNetworkAdaptersInfo(name);
		if (adapter != NULL)
		{
			if (ipAddressV4) wcscpy_s(ipAddressV4, ipAddressV4Size, adapter->IPAddressV4);
			if (ipAddressV6) wcscpy_s(ipAddressV6, ipAddressV6Size, adapter->IPAddressV6);
			return adapter->Enabled;
		}
	}
	return 0;
}



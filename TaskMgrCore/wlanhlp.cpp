#include "stdafx.h"
#include "wlanhlp.h"
#include "loghlp.h"
#include <list>
#include <Objbase.h>

using namespace std;

HANDLE wlanClientHandle = NULL;
DWORD dwCurWlanVersion = 0;
BOOL wlanCanUse = FALSE;
HMODULE hWlanapi = NULL;

list<PWlanInterface> wlanDevices;

fnWlanOpenHandle dWlanOpenHandle;
fnWlanEnumInterfaces dWlanEnumInterfaces;
fnWlanCloseHandle dWlanCloseHandle;
fnWlanGetNetworkBssList dWlanGetNetworkBssList;
fnWlanFreeMemory dWlanFreeMemory;
fnWlanQueryInterface dWlanQueryInterface;

M_CAPI(VOID) MWLAN_Init() 
{
	if (MWLAN_InitApis()) {
		DWORD rs = dWlanOpenHandle(2, 0, &dwCurWlanVersion, &wlanClientHandle);
		if (rs != ERROR_SUCCESS) 
			LogErr2(L"WlanOpenHandle failed : %d", rs);
		else wlanCanUse = TRUE;
	}
}
BOOL MWLAN_InitApis() {
	hWlanapi = LoadLibrary(L"wlanapi.dll");
	if (!hWlanapi) {
		LogErr2(L"LoadLibrary failed : %d", GetLastError());
		return FALSE;
	}

	dWlanOpenHandle = (fnWlanOpenHandle)GetProcAddress(hWlanapi, "WlanOpenHandle");
	dWlanEnumInterfaces = (fnWlanEnumInterfaces)GetProcAddress(hWlanapi, "WlanEnumInterfaces");
	dWlanCloseHandle = (fnWlanCloseHandle)GetProcAddress(hWlanapi, "WlanCloseHandle");
	dWlanGetNetworkBssList = (fnWlanGetNetworkBssList)GetProcAddress(hWlanapi, "WlanGetNetworkBssList");
	dWlanFreeMemory = (fnWlanFreeMemory)GetProcAddress(hWlanapi, "WlanFreeMemory");
	dWlanQueryInterface = (fnWlanQueryInterface)GetProcAddress(hWlanapi, "WlanQueryInterface");

	if(!dWlanOpenHandle || !dWlanEnumInterfaces || !dWlanCloseHandle || !dWlanQueryInterface || !dWlanFreeMemory) {
		LogErr2(L"Bad wlanapi");
		return FALSE;
	}

	return TRUE;
}
VOID MWLAN_Destroy() {
	if (wlanClientHandle) dWlanCloseHandle(wlanClientHandle, 0);
	MWLAN_DestroyInterfaceCaches();
}
VOID MWLAN_DestroyInterfaceCaches()
{
	if (wlanDevices.size() > 0) {
		for (auto it = wlanDevices.begin(); it != wlanDevices.end(); it++)
			free(*it);
		wlanDevices.clear();
	}
}

M_CAPI(BOOL) MWLAN_CanUse() { return wlanCanUse; }
M_CAPI(BOOL) MWLAN_Load() {
	BOOL hasAnyConnected = FALSE;
	PWLAN_INTERFACE_INFO_LIST pInterfaceList = NULL;
	DWORD rs = dWlanEnumInterfaces(wlanClientHandle, NULL, &pInterfaceList);
	if (rs != ERROR_SUCCESS) {
		LogErr2(L"WlanEnumInterfaces failed : %d", rs);
		return FALSE;
	}

	MWLAN_DestroyInterfaceCaches();

	int numberOfItems = pInterfaceList->dwNumberOfItems;
	for (int i = 0; i <= numberOfItems; i++)
	{
		WLAN_INTERFACE_INFO wlanInterfaceInfo = pInterfaceList->InterfaceInfo[i];
		if (wlanInterfaceInfo.isState == wlan_interface_state_connected)
		{
			PWlanInterface inf = (PWlanInterface)malloc(sizeof(WlanInterface));
			memset(inf, 0, sizeof(WlanInterface));
			wcscpy_s(inf->strInterfaceDescription, wlanInterfaceInfo.strInterfaceDescription);
			memcpy_s(&inf->InterfaceGuid, sizeof(inf->InterfaceGuid), &wlanInterfaceInfo.InterfaceGuid, sizeof(wlanInterfaceInfo.InterfaceGuid));
			wlanDevices.push_back(inf);
			hasAnyConnected = TRUE;
		}
	}

	dWlanFreeMemory(pInterfaceList);
		
	if (hasAnyConnected) {
		for (auto it = wlanDevices.begin(); it != wlanDevices.end(); it++)
		{
			PWlanInterface inf = *it;

			WLAN_OPCODE_VALUE_TYPE wlanOpCodeValueType = wlan_opcode_value_type_query_only;
			DWORD pConArrtibuteSize = 0;
			WLAN_CONNECTION_ATTRIBUTES *pConArrtibute = NULL;

			rs = dWlanQueryInterface(wlanClientHandle, &inf->InterfaceGuid, wlan_intf_opcode_current_connection, 0, &pConArrtibuteSize, (PVOID*)&pConArrtibute, &wlanOpCodeValueType);
			if(rs != ERROR_SUCCESS) {
				LogErr2(L"WlanGetNetworkBssList failed : %d", rs);
				continue;
			}

			inf->uLinkQuality = pConArrtibute->wlanAssociationAttributes.wlanSignalQuality;
			inf->dot11BssPhyType = pConArrtibute->wlanAssociationAttributes.dot11PhyType;
			memcpy_s(&inf->dot11Ssid, sizeof(inf->dot11Ssid), &pConArrtibute->wlanAssociationAttributes.dot11Ssid, sizeof(pConArrtibute->wlanAssociationAttributes.dot11Ssid));

			dWlanFreeMemory(pConArrtibute);
		}
	}

	return TRUE;
}
M_CAPI(BOOL) MWLAN_GetAdapterWLANInformation(LPWSTR pszDeviceGuid, int *outLinkQuality, LPWSTR outSsidNameBuf, size_t outSsidNameBufSize, int *outBssPhyType)
{
	GUID stGuid = { 0 };
	HRESULT hr = CLSIDFromString((LPCOLESTR)pszDeviceGuid, (LPCLSID)&stGuid);
	if (FAILED(hr)) {
		LogErr2(L"CLSIDFromString failed : 0x%08x", hr);
		return FALSE;
	}

	for (auto it = wlanDevices.begin(); it != wlanDevices.end(); it++)
	{
		PWlanInterface inf = *it;
		if (inf->InterfaceGuid == stGuid)
		{
			 if (outBssPhyType)*outBssPhyType = static_cast<int>(inf->dot11BssPhyType);
			if (outLinkQuality)*outLinkQuality = static_cast<int>(inf->uLinkQuality);
			if (outSsidNameBuf) swprintf_s(outSsidNameBuf, outSsidNameBufSize, L"%hs", (const char*)inf->dot11Ssid.ucSSID);

			return TRUE;
		}
	}

	return FALSE;
}
#pragma once
#include "stdafx.h"
#include <Wlanapi.h>

typedef struct _WlanInterface {
	GUID InterfaceGuid;
	WCHAR strInterfaceDescription[WLAN_MAX_NAME_LENGTH];
	DOT11_SSID dot11Ssid;
	ULONG uLinkQuality;
	DOT11_PHY_TYPE dot11BssPhyType;
}WlanInterface,*PWlanInterface;

typedef DWORD (WINAPI*fnWlanOpenHandle)(_In_ DWORD dwClientVersion,	_Reserved_ PVOID pReserved,	_Out_ PDWORD pdwNegotiatedVersion,	_Out_ PHANDLE phClientHandle);
typedef DWORD (WINAPI*fnWlanCloseHandle)(	_In_ HANDLE hClientHandle,	_Reserved_ PVOID pReserved);
typedef DWORD (WINAPI*fnWlanEnumInterfaces)(_In_ HANDLE hClientHandle,	_Reserved_ PVOID pReserved,	_Outptr_ PWLAN_INTERFACE_INFO_LIST *ppInterfaceList);
typedef DWORD (WINAPI*fnWlanGetNetworkBssList)(_In_ HANDLE hClientHandle,	_In_ CONST GUID *pInterfaceGuid,	_In_opt_ CONST PDOT11_SSID pDot11Ssid,	_In_ DOT11_BSS_TYPE dot11BssType,	_In_ BOOL bSecurityEnabled,	_Reserved_ PVOID pReserved,	_Outptr_ PWLAN_BSS_LIST *ppWlanBssList);
typedef VOID (WINAPI*fnWlanFreeMemory)(_In_ PVOID pMemory);
typedef DWORD (WINAPI*fnWlanQueryInterface)(_In_ HANDLE hClientHandle,	_In_ CONST GUID *pInterfaceGuid,	_In_ WLAN_INTF_OPCODE OpCode,	_Reserved_ PVOID pReserved,	_Out_ PDWORD pdwDataSize,	_Outptr_result_bytebuffer_(*pdwDataSize) PVOID *ppData,	_Out_opt_ PWLAN_OPCODE_VALUE_TYPE pWlanOpcodeValueType);

BOOL MWLAN_InitApis();
VOID MWLAN_Destroy();
VOID MWLAN_DestroyInterfaceCaches();

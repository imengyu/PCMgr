#pragma once
#include "stdafx.h"
#include "appmodel.h"
#include <cryptuiapi.h>
#include <iphlpapi.h>
#include <DbgHelp.h>
#include "netdef.h"

typedef void*(*_MGetCurrentTeb)();
typedef void*(*_MGetCurrentPeb)();
typedef FARPROC(*_MGetXFun)(int x);
typedef FARPROC (*_MGetProcAddressCore)(_In_ HMODULE hModule,_In_ LPCSTR lpProcName);
typedef BOOL(WINAPI *_RunFileDlg)(_In_ HWND hwndOwner, _In_opt_ HICON hIcon, _In_opt_ LPCWSTR lpszDirectory, _In_opt_ LPCWSTR lpszTitle, _In_opt_ LPCWSTR lpszDescription, _In_ ULONG uFlags);
typedef BOOL(WINAPI *_IsImmersiveProcess)(_In_ HANDLE hProcess);
typedef LONG(WINAPI *_GetPackageFullName)(	HANDLE hProcess,UINT32 *packageFullNameLength, PWSTR packageFullName);
typedef LONG(WINAPI *_GetPackageInfo)(PACKAGE_INFO_REFERENCE packageInfoReference, const UINT32 flags,	 UINT32 *bufferLength, BYTE *buffer,	UINT32 *count);
typedef LONG(WINAPI *_ClosePackageInfo)(PACKAGE_INFO_REFERENCE packageInfoReference);
typedef LONG(WINAPI *_OpenPackageInfoByFullName)(PCWSTR packageFullName,	const UINT32 reserved, PACKAGE_INFO_REFERENCE *packageInfoReference);
typedef LONG(WINAPI *_GetPackageId)(_In_ HANDLE hProcess, _Inout_ UINT32 * bufferLength, _Out_writes_bytes_opt_(*bufferLength) BYTE * buffer);
typedef BOOL(WINAPI *_IsWow64Process)(HANDLE, PBOOL);
typedef DWORD(WINAPI*_GetModuleFileNameW)(_In_opt_ HMODULE hModule, LPWSTR lpFilename, DWORD nSize);
typedef BOOL(WINAPI*_CryptUIDlgViewCertificateW)(_In_  PCCRYPTUI_VIEWCERTIFICATE_STRUCTW pCertViewInfo, _Out_ BOOL *pfPropertiesChanged);
typedef BOOL(WINAPI*_CryptUIDlgViewContext)(DWORD dwContextType, const void *pvContext, HWND hwnd, LPCWSTR pwszTitle, DWORD dwFlags, void *pvReserved);
typedef ULONG(WINAPI* _GetPerTcpConnectionEStats)(_In_ PMIB_TCPROW Row, _In_ TCP_ESTATS_TYPE EstatsType, _Out_writes_bytes_opt_(RwSize) PUCHAR Rw, _In_ ULONG RwVersion, _In_ ULONG RwSize, _Out_writes_bytes_opt_(RosSize) PUCHAR Ros, _In_ ULONG RosVersion, _In_ ULONG RosSize, _Out_writes_bytes_opt_(RodSize) PUCHAR Rod, _In_ ULONG RodVersion, _In_ ULONG RodSize);
typedef ULONG(WINAPI*_GetPerTcp6ConnectionEStats)(_In_ PMIB_TCP6ROW Row, _In_ TCP_ESTATS_TYPE EstatsType, _Out_writes_bytes_opt_(RwSize) PUCHAR Rw, _In_  ULONG RwVersion, _In_  ULONG RwSize, _Out_writes_bytes_opt_(RosSize) PUCHAR Ros, _In_  ULONG RosVersion, _In_  ULONG RosSize, _Out_writes_bytes_opt_(RodSize) PUCHAR Rod, _In_  ULONG RodVersion, _In_  ULONG RodSize);
typedef ULONG(WINAPI*_SetPerTcpConnectionEStats)(_In_ PMIB_TCPROW Row, _In_ TCP_ESTATS_TYPE EstatsType, _In_reads_bytes_(RwSize) PUCHAR Rw, _In_ ULONG RwVersion, _In_ ULONG RwSize, _In_ ULONG Offset);
typedef ULONG(WINAPI*_SetPerTcp6ConnectionEStats)(_In_ PMIB_TCP6ROW Row, _In_ TCP_ESTATS_TYPE EstatsType, _In_reads_bytes_(RwSize) PUCHAR Rw, _In_ ULONG RwVersion, _In_ ULONG RwSize, _In_ ULONG Offset);
typedef DWORD(WINAPI*_GetExtendedTcpTable)(PVOID pTcpTable, PDWORD pdwSize, BOOL bOrder, ULONG ulAf, TCP_TABLE_CLASS TableClass, ULONG Reserved);
typedef BOOL(WINAPI*_CancelShutdown)();
typedef PWSTR(WINAPI *_RtlIpv6AddressToStringW)(const in6_addr *Addr, 	PWSTR S);

typedef BOOL(WINAPI *fnIMAGEUNLOAD)(__in PLOADED_IMAGE LoadedImage);
typedef PLOADED_IMAGE(WINAPI *fnIMAGELOAD)(__in PSTR DllName,	__in  PSTR DllPath);

typedef HMODULE(WINAPI *fnLoadLibraryA)(LPCSTR lpLibFileName);
typedef HMODULE(WINAPI *fnLoadLibraryW)(LPCWSTR lpLibFileName);

#pragma region WinSta

#define WINSTATIONNAME_LENGTH 32

typedef WCHAR WINSTATIONNAME[WINSTATIONNAME_LENGTH + 1];

// Variable length data descriptor (not needed)
typedef struct _VARDATA_WIRE
{
	USHORT Size;
	USHORT Offset;
} VARDATA_WIRE, *PVARDATA_WIRE;

typedef enum _WINSTATIONSTATECLASS
{
	State_Active = 0,
	State_Connected = 1,
	State_ConnectQuery = 2,
	State_Shadow = 3,
	State_Disconnected = 4,
	State_Idle = 5,
	State_Listen = 6,
	State_Reset = 7,
	State_Down = 8,
	State_Init = 9
} WINSTATIONSTATECLASS;

typedef struct _SESSIONIDW
{
	union
	{
		ULONG SessionId;
		ULONG LogonId;
	};
	WINSTATIONNAME WinStationName;
	WINSTATIONSTATECLASS State;
} SESSIONIDW, *PSESSIONIDW;

typedef enum _WINSTATIONINFOCLASS
{
	WinStationCreateData,
	WinStationConfiguration,
	WinStationPdParams,
	WinStationWd,
	WinStationPd,
	WinStationPrinter,
	WinStationClient,
	WinStationModules,
	WinStationInformation,
	WinStationTrace,
	WinStationBeep,
	WinStationEncryptionOff,
	WinStationEncryptionPerm,
	WinStationNtSecurity,
	WinStationUserToken,
	WinStationUnused1,
	WinStationVideoData,
	WinStationInitialProgram,
	WinStationCd,
	WinStationSystemTrace,
	WinStationVirtualData,
	WinStationClientData,
	WinStationSecureDesktopEnter,
	WinStationSecureDesktopExit,
	WinStationLoadBalanceSessionTarget,
	WinStationLoadIndicator,
	WinStationShadowInfo,
	WinStationDigProductId,
	WinStationLockedState,
	WinStationRemoteAddress,
	WinStationIdleTime,
	WinStationLastReconnectType,
	WinStationDisallowAutoReconnect,
	WinStationMprNotifyInfo,
	WinStationExecSrvSystemPipe,
	WinStationSmartCardAutoLogon,
	WinStationIsAdminLoggedOn,
	WinStationReconnectedFromId,
	WinStationEffectsPolicy,
	WinStationType,
	WinStationInformationEx,
	WinStationValidationInfo
} WINSTATIONINFOCLASS;

typedef BOOLEAN(WINAPI*_WinStationSendMessageW)(
	_In_opt_ HANDLE hServer,
	_In_ ULONG SessionId,
	_In_ PWSTR Title,
	_In_ ULONG TitleLength,
	_In_ PWSTR Message,
	_In_ ULONG MessageLength,
	_In_ ULONG Style,
	_In_ ULONG Timeout,
	_Out_ PULONG Response,
	_In_ BOOLEAN DoNotWait
);
typedef BOOLEAN(WINAPI*_WinStationConnectW)(_In_opt_ HANDLE hServer,	_In_ ULONG SessionId,	_In_ ULONG TargetSessionId,	_In_opt_ PWSTR pPassword,	_In_ BOOLEAN bWait);
typedef BOOLEAN(WINAPI*_WinStationDisconnect)(_In_opt_ HANDLE hServer,	_In_ ULONG SessionId,	_In_ BOOLEAN bWait);
typedef BOOLEAN(WINAPI*_WinStationReset)(_In_opt_ HANDLE hServer,_In_ ULONG SessionId,_In_ BOOLEAN bWait);
typedef BOOLEAN(WINAPI*_WinStationFreeMemory)(	_In_ PVOID Buffer);
typedef BOOLEAN(WINAPI*_WinStationEnumerateW)(	_In_opt_ HANDLE hServer,	_Out_ PSESSIONIDW *SessionIds,	_Out_ PULONG Count);
typedef BOOLEAN(WINAPI*_WinStationQueryInformationW)(_In_opt_ HANDLE hServer, _In_ ULONG SessionId, _In_ WINSTATIONINFOCLASS WinStationInformationClass, _Out_writes_bytes_(WinStationInformationLength) PVOID pWinStationInformation, _In_ ULONG WinStationInformationLength, _Out_ PULONG pReturnLength);

#include <ws2tcpip.h>
#include <ws2ipdef.h>

//ws32
typedef int (WSAAPI *_WSAStartup)(
	_In_ WORD wVersionRequested,
	_Out_ LPWSADATA lpWSAData
	);
typedef int (WSAAPI *_WSAGetLastError)();
typedef INT(WSAAPI *_GetNameInfoW)(
	_In_reads_bytes_(SockaddrLength) const SOCKADDR *pSockaddr,
	_In_ socklen_t SockaddrLength,
	_Out_writes_opt_(NodeBufferSize) PWCHAR pNodeBuffer,
	_In_ DWORD NodeBufferSize,
	_Out_writes_opt_(ServiceBufferSize) PWCHAR pServiceBuffer,
	_In_ DWORD ServiceBufferSize,
	_In_ INT Flags
	);
typedef struct hostent *(WSAAPI *_gethostbyaddr)(
	_In_reads_bytes_(len) const char *addr,
	_In_ int len,
	_In_ int type
	);

#pragma endregion

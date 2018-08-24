#pragma once
#include "stdafx.h"
#include "appmodel.h"
#include <cryptuiapi.h>
#include <iphlpapi.h>
#include <DbgHelp.h>

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
typedef ULONG(WINAPI* _GetPerTcpConnectionEStats)(PMIB_TCPROW Row, TCP_ESTATS_TYPE EstatsType, PUCHAR Rw, ULONG RwVersion, ULONG RwSize, PUCHAR Ros, ULONG RosVersion, ULONG RosSize, PUCHAR Rod, ULONG RodVersion, ULONG RodSize);
typedef DWORD(WINAPI*_GetExtendedTcpTable)(PVOID pTcpTable, PDWORD          pdwSize, BOOL bOrder, ULONG ulAf, TCP_TABLE_CLASS TableClass, ULONG Reserved);
typedef BOOL(WINAPI*_CancelShutdown)();

typedef BOOL(WINAPI *fnIMAGEUNLOAD)(__in PLOADED_IMAGE LoadedImage);
typedef PLOADED_IMAGE(WINAPI *fnIMAGELOAD)(__in PSTR DllName,	__in  PSTR DllPath);

typedef HMODULE(WINAPI *fnLoadLibraryA)(LPCSTR lpLibFileName);
typedef HMODULE(WINAPI *fnLoadLibraryW)(LPCWSTR lpLibFileName);

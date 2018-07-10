#pragma once
#include "stdafx.h"
#include "appmodel.h"

typedef BOOL(WINAPI *_RunFileDlg)(_In_ HWND hwndOwner, _In_opt_ HICON hIcon, _In_opt_ LPCWSTR lpszDirectory, _In_opt_ LPCWSTR lpszTitle, _In_opt_ LPCWSTR lpszDescription, _In_ ULONG uFlags);
typedef BOOL(WINAPI *_IsImmersiveProcess)(_In_ HANDLE hProcess);
typedef LONG(WINAPI *_GetPackageFullName)(	HANDLE hProcess,UINT32 *packageFullNameLength, PWSTR packageFullName);
typedef LONG(WINAPI *_GetPackageInfo)(PACKAGE_INFO_REFERENCE packageInfoReference, const UINT32 flags,	 UINT32 *bufferLength, BYTE *buffer,	UINT32 *count);
typedef LONG(WINAPI *_ClosePackageInfo)(PACKAGE_INFO_REFERENCE packageInfoReference);
typedef LONG(WINAPI *_OpenPackageInfoByFullName)(PCWSTR packageFullName,	const UINT32 reserved, PACKAGE_INFO_REFERENCE *packageInfoReference);
typedef LONG(WINAPI *_GetPackageId)(_In_ HANDLE hProcess, _Inout_ UINT32 * bufferLength, _Out_writes_bytes_opt_(*bufferLength) BYTE * buffer);

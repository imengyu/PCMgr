#pragma once
#include "stdafx.h"
#include "ntdef.h"
#include "sysfuns.h"

M_CAPI(BOOL) MRunFileDlg(HWND hwndOwner, HICON hIcon, LPCWSTR lpszDirectory, LPCWSTR lpszTitle, LPCWSTR lpszDescription, ULONG uFlags);

M_CAPI(BOOL) MIs64BitOS();
M_CAPI(BOOL) MGetPrivileges();
M_CAPI(BOOL) MIsRunasAdmin();

M_CAPI(PVOID) MGetProcedureAddress(PVOID DllHandle, PSTR ProcedureName, ULONG ProcedureNumber);
M_CAPI(PVOID) MGetProcAddress(PVOID DllHandle, PSTR ProcedureName);



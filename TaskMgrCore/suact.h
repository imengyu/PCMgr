#pragma once
#include "stdafx.h"
#include "ntdef.h"

M_CAPI(BOOL) M_SU_CreateFile(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, DWORD dwCreationDisposition, PHANDLE pHandle);
M_CAPI(BOOL) M_SU_OpenProcess(DWORD pid, PHANDLE pHandle);
M_CAPI(BOOL) M_SU_OpenThread(DWORD pid, DWORD tid, PHANDLE pHandle);
M_CAPI(BOOL) M_SU_CloseHandleWithProcess(SYSTEM_HANDLE*sh);

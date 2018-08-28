#pragma once
#include "stdafx.h"
#include "ntdef.h"

typedef void(*EHCALLBACK)(VOID* handle, LPWSTR type, LPWSTR name, LPWSTR address, LPWSTR objaddr, int refcount, int typeindex);

M_CAPI(BOOL) M_EH_CloseHandle(DWORD pid, LPVOID handleValue);
M_CAPI(BOOL) M_EH_GetHandleTypeName(PSYSTEM_HANDLE_TABLE_ENTRY_INFO pSystemHandle, LPWSTR buffer, size_t bufsize);
M_CAPI(BOOL) M_EH_EnumProcessHandles(DWORD pid, EHCALLBACK callback);
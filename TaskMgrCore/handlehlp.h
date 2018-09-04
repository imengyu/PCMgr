#pragma once
#include "stdafx.h"
#include "ntdef.h"

typedef void(*EHCALLBACK)(VOID* handle, LPWSTR type, LPWSTR name, LPWSTR address, LPWSTR objaddr, ULONG refcount, int typeindex);

M_CAPI(NTSTATUS) M_EH_DuplicateHandleFromProcess(HANDLE ProcessHandle, PHANDLE Handle, HANDLE HandleValue, ACCESS_MASK DesiredAccess);
M_CAPI(BOOL) M_EH_CloseHandle(DWORD pid, LPVOID handleValue);
M_CAPI(BOOL) M_EH_GetHandleTypeName(HANDLE HandleDup, LPWSTR buffer, size_t bufsize);
M_CAPI(BOOL) M_EH_EnumProcessHandles(DWORD pid, EHCALLBACK callback);
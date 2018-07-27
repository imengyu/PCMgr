#pragma once
#include "stdafx.h"
#include "ntdef.h"

EXTERN_C M_API NTSTATUS MGetThreadState(ULONG ulPID, ULONG ulTID);

EXTERN_C M_API NTSTATUS MOpenThreadNt(DWORD dwId, PHANDLE pLandle, DWORD dwPId);

EXTERN_C M_API NTSTATUS MTerminateThreadNt(HANDLE handle);

EXTERN_C M_API NTSTATUS MResumeThreadNt(HANDLE handle);

EXTERN_C M_API NTSTATUS MSuspendThreadNt(HANDLE handle);

EXTERN_C M_API BOOL MGetThreadInfoNt(DWORD tid, int i, LPWSTR * str);

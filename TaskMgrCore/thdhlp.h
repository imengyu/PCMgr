#pragma once
#include "stdafx.h"

EXTERN_C M_API DWORD MGetThreadState(ULONG ulPID, ULONG ulTID);

EXTERN_C M_API DWORD MOpenThreadNt(DWORD dwId, PHANDLE pLandle, DWORD dwPId);

EXTERN_C M_API DWORD MTerminateThreadNt(HANDLE handle);

EXTERN_C M_API DWORD MResumeThreadNt(HANDLE handle);

EXTERN_C M_API DWORD MSuspendThreadNt(HANDLE handle);

EXTERN_C M_API BOOL MGetThreadInfoNt(DWORD tid, int i, LPWSTR * str);

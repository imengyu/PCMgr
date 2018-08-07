#pragma once
#include "stdafx.h"
#include "ntdef.h"

EXTERN_C M_API NTSTATUS MOpenThreadNt(DWORD dwId, PHANDLE pLandle, DWORD dwPId);

EXTERN_C M_API NTSTATUS MTerminateThreadNt(HANDLE handle);

EXTERN_C M_API NTSTATUS MResumeThreadNt(HANDLE handle);

EXTERN_C M_API NTSTATUS MSuspendThreadNt(HANDLE handle);
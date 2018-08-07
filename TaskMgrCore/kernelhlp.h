#pragma once
#include "stdafx.h"
#include "sysstructs.h"

DWORD WINAPI MDbgViewReceiveThread(LPVOID lpParameter);
DWORD WINAPI MLoadingThread(LPVOID lpParameter);

BOOL MUnInitMyDbgView();
BOOL MInitMyDbgView();

M_CAPI(VOID) MDoNotStartMyDbgView();
BOOL MInitKernelNTPDB(BOOL usingNtosPDB, PKNTOSVALUE kNtosValue);

M_CAPI(BOOL) MCanUseKernel();
M_CAPI(BOOL) MInitKernel(LPWSTR currentPath);
M_CAPI(BOOL) MUninitKernel();
M_CAPI(BOOL) MLoadKernelDriver(LPWSTR lpszDriverName, LPWSTR driverPath, LPWSTR lpszDisplayName);
M_CAPI(BOOL) MUnLoadKernelDriver(LPWSTR szSvrName);

M_CAPI(ULONG_PTR) MGetNTBaseAddress();

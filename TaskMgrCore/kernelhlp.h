#pragma once
#include "stdafx.h"

M_CAPI(BOOL) MCanUseKernel();
M_CAPI(BOOL) MInitKernel(LPWSTR currentPath);
M_CAPI(BOOL) MUninitKernel();
M_CAPI(BOOL) MLoadKernelDriver(LPWSTR lpszDriverName, LPWSTR driverPath, LPWSTR lpszDisplayName);
M_CAPI(BOOL) MUnLoadKernelDriver(LPWSTR szSvrName);
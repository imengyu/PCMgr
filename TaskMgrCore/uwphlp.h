#pragma once
#include "stdafx.h"
#include <appmodel.h>
#include <appxpackaging.h>

M_CAPI(BOOL) M_UWP_IsInited();
M_CAPI(BOOL) M_UWP_Init();
M_CAPI(BOOL) M_UWP_UnInit();
M_CAPI(BOOL) M_UWP_RunUWPApp(LPWSTR strAppUserModelId, PDWORD pdwProcessId);
M_CAPI(BOOL) M_UWP_KillUWPApplication(LPWSTR packageName);
M_CAPI(BOOL) M_UWP_UnInstallUWPApplication(LPWSTR packageName);
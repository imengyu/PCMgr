#pragma once
#include "stdafx.h"
#include "reghlp.h"

typedef void(__cdecl*EnumStartupsCallBack)(LPWSTR dspName, LPWSTR type, LPWSTR path, HKEY regrootpath, LPWSTR regpath, LPWSTR regvalue);

//枚举开机启动项
//    callBack：回调
M_CAPI(VOID) MEnumStartups(EnumStartupsCallBack callBack);

LRESULT MSM_HandleWmCommand(WPARAM wParam);

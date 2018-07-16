#pragma once
#include "stdafx.h"
#include "reghlp.h"

typedef void(__cdecl*EnumStartupsCallBack)(LPWSTR dspName, LPWSTR type, LPWSTR path, HKEY regrootpath, LPWSTR regpath, LPWSTR regvalue);

LRESULT MSM_HandleWmCommand(WPARAM wParam);

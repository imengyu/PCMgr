#pragma once
#include "stdafx.h"

typedef void(__cdecl*EnumServicesCallBack)(LPWSTR dspName, LPWSTR scName,
	DWORD scType, DWORD currentState, DWORD dwProcessId,	BOOL sysSc, DWORD dwStartType,
	LPWSTR lpBinaryPathName, LPWSTR lpLoadOrderGroup);

M_CAPI(void) MSCM_SetCurrSelSc(LPWSTR scname);

LRESULT MSCM_HandleWmCommand(WPARAM wParam);

#pragma once
#include "stdafx.h"

typedef struct _SERVICE_STORAGE {
	LPWSTR                    lpServiceName;
	LPENUM_SERVICE_STATUS_PROCESS lpSvc;
	WCHAR    ServiceImagePath[MAX_PATH];
	BOOL  DriverServiceFounded;
	SC_HANDLE ServiceHandle;
	DWORD ServiceStartType;
} SERVICE_STORAGE, *LPSERVICE_STORAGE;

typedef void(__cdecl*EnumServicesCallBack)(LPWSTR dspName, LPWSTR scName,
	DWORD scType, DWORD currentState, DWORD dwProcessId,	BOOL sysSc, DWORD dwStartType,
	LPWSTR lpBinaryPathName, LPWSTR lpLoadOrderGroup);

M_CAPI(BOOL) MSCM_ChangeScStartType(LPWSTR scname, DWORD type, LPWSTR errText);

M_CAPI(void) MSCM_SetCurrSelSc(LPWSTR scname);

LRESULT MSCM_HandleWmCommand(WPARAM wParam);

M_CAPI(BOOL) MSCM_CheckDriverServices(LPWSTR fileName, LPWSTR outName, LPSERVICE_STORAGE*pScInfo);

M_CAPI(BOOL) MSCM_EnumDriverServices();

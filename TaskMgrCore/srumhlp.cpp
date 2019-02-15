#include "stdafx.h"
#include "srumhlp.h"
#include "loghlp.h"
#include "syshlp.h"
#include "UidHlp.h"
#include "StringHlp.h"
#include <oleauto.h>

BOOL surmCanUse = FALSE;

HMODULE hSrumApi = NULL;
fnSruRegisterRealTimeStats SruRegisterRealTimeStats;
PSID currentSid = NULL;
WCHAR dwmDosPath[MAX_PATH];

SrumProviderHandler *handler = NULL;

M_CAPI(BOOL) MSRUM_Init() {
	if (!surmCanUse) {
		hSrumApi = LoadLibrary(L"srumapi.dll");
		if (!hSrumApi) {
			LogErr2(L"LoadLibrary srumapi.dll failed : %d", GetLastError());
			goto ERROUT;
		}
		SruRegisterRealTimeStats = (fnSruRegisterRealTimeStats)GetProcAddress(hSrumApi, "SruRegisterRealTimeStats");
		if (SruRegisterRealTimeStats) {
			handler = new SrumProviderHandler();
			surmCanUse = TRUE;
		}
	}
ERROUT:
	return surmCanUse;
}
M_CAPI(VOID) MSRUM_Destroy() {
	if (surmCanUse) {
		FreeLibrary(hSrumApi);
		surmCanUse = FALSE;
		if(currentSid)
		{
			HeapFree(GetProcessHeap(), 0, currentSid);
			currentSid = NULL;
		}
		delete handler;
	}
}
M_CAPI(BOOL) MSRUM_CanUse() {	return surmCanUse ; }
M_CAPI(BOOL) MSRUM_LoadSrumData() {
	BOOL rs = TRUE;
	BOOL isAdmin = MIsRunasAdmin();

	fnSruProviderCallback callbacks[4] = { 
		SrumProviderHandler::SrumNetworkProviderCallback,
		SrumProviderHandler::SrumCpuProviderCallback,
		SrumProviderHandler::SrumNotificationsProviderCallback,
		SrumProviderHandler::SrumNetworkProviderGlobalDataCallback
	};

	SYSTEMTIME SystemTime;
	DWORD dword1 = 0;
	DWORD dword2 = 0;
	ULONG returnResult = 0;

	GetSystemTime(&SystemTime);
	for (int i = 0; i < 3; i++) {
		returnResult = SruRegisterRealTimeStats(i, &SystemTime, isAdmin + 257, handler, callbacks[i], &dword1, &dword2);
		if (returnResult != 0) {
			LogErr2(L"SruRegisterRealTimeStats failed : %d", returnResult);
			rs = FALSE;
			goto ERR_RETURN;
		}
	}

	returnResult = SruRegisterRealTimeStats(0, &SystemTime, 4, handler, callbacks[3], &dword1, &dword2);
	if (returnResult != 0) {
		LogErr2(L"SruRegisterRealTimeStats failed : %d", returnResult);
		rs = FALSE;
		goto ERR_RETURN;
	}

ERR_RETURN:
	return rs;
}

void __stdcall SrumProviderHandler::SrumNetworkProviderCallback(void*lpPararm, _SRU_STATS_RECORD_SET *pRecordSet)
{
	Log(L"SrumNetworkProviderCallback 0x%08x", pRecordSet);
}
void __stdcall SrumProviderHandler::SrumCpuProviderCallback(void*lpPararm, _SRU_STATS_RECORD_SET *pRecordSet)
{
	Log(L"SrumCpuProviderCallback 0x%08x", pRecordSet);
}
void __stdcall SrumProviderHandler::SrumNotificationsProviderCallback(void*lpPararm, _SRU_STATS_RECORD_SET *pRecordSet)
{
	Log(L"SrumNotificationsProviderCallback 0x%08x", pRecordSet);
}
void __stdcall SrumProviderHandler::SrumNetworkProviderGlobalDataCallback(void*lpPararm, _SRU_STATS_RECORD_SET *pRecordSet)
{
	Log(L"SrumNetworkProviderGlobalDataCallback 0x%08x", pRecordSet);
}

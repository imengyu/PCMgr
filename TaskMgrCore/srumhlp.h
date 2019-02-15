#pragma once
#include "stdafx.h"

typedef struct _SRU_STATS_RECORD {
	PSID sid;
}SRU_STATS_RECORD,*PSRU_STATS_RECORD;
typedef struct _SRU_STATS_RECORD_SET {
	DWORD dword1;
	SRU_STATS_RECORD record;

}SRU_STATS_RECORD_SET,*PSRU_STATS_RECORD_SET;


typedef VOID (WINAPI *fnSruProviderCallback)(void* lpPararm, _SRU_STATS_RECORD_SET *pRecordSet);
typedef ULONG(WINAPI *fnSruRegisterRealTimeStats)(int dataId, SYSTEMTIME* time, BOOL isAdmin, void* lpPararm, fnSruProviderCallback callback, DWORD *, DWORD *);

class SrumProviderHandler {

public:
	static void __stdcall SrumNetworkProviderCallback(void*lpPararm,_SRU_STATS_RECORD_SET *pRecordSet);
	static void __stdcall SrumCpuProviderCallback(void*lpPararm,_SRU_STATS_RECORD_SET *pRecordSet);
	static void __stdcall SrumNotificationsProviderCallback(void*lpPararm, _SRU_STATS_RECORD_SET *pRecordSet);
	static void __stdcall SrumNetworkProviderGlobalDataCallback(void*lpPararm, _SRU_STATS_RECORD_SET *pRecordSet);
	
};


#pragma once
#include "stdafx.h"
#include "appmodel.h"
#include "appxpackaging.h"

M_CAPI(BOOL) MPERF_GetRamUseAge();

struct MPerfAndProcessData
{
	__int64 NowCpuTime;
	__int64 LastCpuTime;
	ULONGLONG LastRead;
	ULONGLONG LastWrite;
	ULONG64 NetWorkInBandWidth;
	ULONG64 NetWorkOutBandWidth;

	PACKAGE_ID* packageId=NULL;
};

M_CAPI(void) MPERF_NET_FreeAllProcessNetInfo();

#pragma once
#include "stdafx.h"
#include "appmodel.h"
#include "appxpackaging.h"

struct MPerfAndProcessData
{
	__int64 NowCpuTime;
	__int64 LastCpuTime;
	ULONGLONG LastRead;
	ULONGLONG LastWrite;

	PACKAGE_ID* packageId=NULL;
};

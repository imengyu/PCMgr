#pragma once
#include "stdafx.h"

//CPU пео╒
class M_API MCpuInfoMonitor
{
public:

	static void FreeCpuInfos();

	static DWORD GetCpuL1Cache();
	static DWORD GetCpuL2Cache();
	static DWORD GetCpuL3Cache();
	static DWORD GetCpuPackage();
	static DWORD GetCpuNodeCount();
	static BOOL GetCpuInfos();
	static BOOL GetCpuName(LPWSTR buf, int size);
	static int GetCpuFrequency();
	static int GetProcessorNumber();
};


#pragma once
#include "stdafx.h"
#include "ntdef.h"
#include "DltHlp.h"

class M_API MSystemPerformanctMonitor
{
public:

	static DWORD GetThreadCount();
	static DWORD GetHandleCount();
	static DWORD GetProcessCount();
	static DWORD GetPageSize();
	static DWORD GetCpuCount();

	static ULONGLONG GetSystemRunTime();

	static BOOL InitGlobal();
	static void DestroyGlobal();

	static BOOL UpdatePerformance();
	static BOOL UpdateCpuGlobal();

	static VOID UpdateCpuInformation();
	static VOID UpdateCpuCycleInformation(PULONG64 IdleCycleTime);
	static VOID UpdateCpuCycleUsageInformation(ULONG64 TotalCycleTime, ULONG64 IdleCycleTime);

	static double GetCpuUsage();
	static double GetCpuUsageKernel();
	static double GetCpuUsageUser();

	static __int64 UpdateTimeInterval;	//Global update delta
	static PLARGE_INTEGER CpuIdleCycleTime; // cycle time for Idle
	static PLARGE_INTEGER CpuSystemCycleTime; // cycle time for DPCs and Interrupts
	static MUINT64_DELTA CpuIdleCycleDelta;
	static MUINT64_DELTA CpuSystemCycleDelta;

	static MUINT64_DELTA CpuKernelDelta;
	static MUINT64_DELTA CpuUserDelta;
	static MUINT64_DELTA CpuIdleDelta;

	static PMUINT64_DELTA CpusKernelDelta;
	static PMUINT64_DELTA CpusUserDelta;
	static PMUINT64_DELTA CpusIdleDelta;

	static ULONG64 CpuTotalTime;

	//Cpu infos
	static PSYSTEM_PROCESSOR_PERFORMANCE_INFORMATION CpuInformation;
	static SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION CpuTotals;
};


class M_API MSystemMemoryPerformanctMonitor
{
public:

	static ULONGLONG GetAllMemory();
	static ULONGLONG GetKernelPaged();
	static ULONGLONG GetKernelNonpaged();
	static ULONGLONG GetSystemCacheSize();
	static ULONGLONG GetCommitTotal();
	static ULONGLONG GetCommitLimit();
	static ULONGLONG GetMemoryAvail();
	static ULONGLONG GetMemoryUsed();
	static ULONGLONG GetMemoryAvailPageFile();

	static double GetMemoryUsage();

	static ULONGLONG GetStandBySize();
	static ULONGLONG GetModifiedSize();

	static BOOL GetMemoryCompressionInfo(PPROCESS_COMPRESSION_INFO outInfo);
	static BOOL UpdateMemoryListInfo();
};


#include "stdafx.h"
#include "MSystemPerformanctMonitor.h"
#include "ntdef.h"
#include "loghlp.h"

#include <Psapi.h>

extern NtQuerySystemInformationFun NtQuerySystemInformation;
extern RtlNtStatusToDosErrorFun RtlNtStatusToDosError;

UINT CpuCount = 1;//Cpu Count
UINT PageSize;//Page Size
__int64 LastTime;
__int64 MSystemPerformanctMonitor::UpdateTimeInterval;//Global update delta

//performance & memory structs
MEMORYSTATUSEX memory_statuex;
SYSTEM_MEMORY_LIST_INFORMATION memoryListInfo;
ULONG_PTR standbyPageCount = 0;
PERFORMANCE_INFORMATION performance_info;

//Cpu infos
PSYSTEM_PROCESSOR_PERFORMANCE_INFORMATION MSystemPerformanctMonitor::CpuInformation = nullptr;
SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION MSystemPerformanctMonitor::CpuTotals;
ULONG64 MSystemPerformanctMonitor::CpuTotalTime;
//Cpu Cycle infos
PLARGE_INTEGER MSystemPerformanctMonitor::CpuIdleCycleTime = nullptr; // cycle time for Idle
PLARGE_INTEGER MSystemPerformanctMonitor::CpuSystemCycleTime = nullptr; // cycle time for DPCs and Interrupts
MUINT64_DELTA MSystemPerformanctMonitor::CpuIdleCycleDelta;
MUINT64_DELTA MSystemPerformanctMonitor::CpuSystemCycleDelta;

MUINT64_DELTA MSystemPerformanctMonitor::CpuKernelDelta;
MUINT64_DELTA MSystemPerformanctMonitor::CpuUserDelta;
MUINT64_DELTA MSystemPerformanctMonitor::CpuIdleDelta;

PMUINT64_DELTA MSystemPerformanctMonitor::CpusKernelDelta = nullptr;
PMUINT64_DELTA MSystemPerformanctMonitor::CpusUserDelta = nullptr;
PMUINT64_DELTA MSystemPerformanctMonitor::CpusIdleDelta = nullptr;

//Usage total
FLOAT CpuKernelUsage, CpuUserUsage;
FLOAT CpuUsage, CpuInterruptUsage, CpuUsageBase;
//Usages
PFLOAT CpusKernelUsage;
PFLOAT CpusUserUsage;

bool globalUpdatePerformanceLock = false;

M_CAPI(__int64) FileTimeToInt64(const FILETIME& time)
{
	ULARGE_INTEGER tt;  //64位无符号整型值
	tt.LowPart = time.dwLowDateTime;
	tt.HighPart = time.dwHighDateTime;
	return(tt.QuadPart);  //返回整型值
}


ULONGLONG MSystemPerformanctMonitor::GetSystemRunTime()
{
	return GetTickCount64();
}
DWORD MSystemPerformanctMonitor::GetPageSize()
{
	return PageSize;
}
DWORD MSystemPerformanctMonitor::GetCpuCount()
{
	return CpuCount;
}
DWORD MSystemPerformanctMonitor::GetThreadCount() {
	return performance_info.ThreadCount;
}
DWORD MSystemPerformanctMonitor::GetHandleCount() {
	return performance_info.HandleCount;
}
DWORD MSystemPerformanctMonitor::GetProcessCount() {
	return performance_info.ProcessCount;
}
BOOL MSystemPerformanctMonitor::UpdatePerformance()
{
	if (globalUpdatePerformanceLock) {
		globalUpdatePerformanceLock = false;
		return GlobalMemoryStatusEx(&memory_statuex) && GetPerformanceInfo(&performance_info, sizeof(performance_info));
	}
	return TRUE;
}
BOOL MSystemPerformanctMonitor::UpdateCpuGlobal()
{
	FILETIME now;
	GetSystemTimeAsFileTime(&now);
	__int64 nowu = FileTimeToInt64(now);
	if (LastTime != 0) 
		UpdateTimeInterval = nowu - LastTime;
	LastTime = nowu;
	globalUpdatePerformanceLock = true;
	return globalUpdatePerformanceLock;
}
VOID MSystemPerformanctMonitor::UpdateCpuInformation()
{
	ULONG i;

	NtQuerySystemInformation(SystemProcessorPerformanceInformation, CpuInformation,
		sizeof(SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION) * (ULONG)CpuCount, NULL);

	// Zero the CPU totals.
	memset(&CpuTotals, 0, sizeof(SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION));

	for (i = 0; i < (ULONG)CpuCount; i++)
	{
		PSYSTEM_PROCESSOR_PERFORMANCE_INFORMATION cpuInfo = &CpuInformation[i];

		// KernelTime includes IdleTime.
		cpuInfo->KernelTime.QuadPart -= cpuInfo->IdleTime.QuadPart;

		CpuTotals.DpcTime.QuadPart += cpuInfo->DpcTime.QuadPart;
		CpuTotals.IdleTime.QuadPart += cpuInfo->IdleTime.QuadPart;
		CpuTotals.InterruptCount += cpuInfo->InterruptCount;
		CpuTotals.InterruptTime.QuadPart += cpuInfo->InterruptTime.QuadPart;
		CpuTotals.KernelTime.QuadPart += cpuInfo->KernelTime.QuadPart;
		CpuTotals.UserTime.QuadPart += cpuInfo->UserTime.QuadPart;

		MUpdateDelta(&CpusKernelDelta[i], cpuInfo->KernelTime.QuadPart);
		MUpdateDelta(&CpusUserDelta[i], cpuInfo->UserTime.QuadPart);
		MUpdateDelta(&CpusIdleDelta[i], cpuInfo->IdleTime.QuadPart);
	}

	MUpdateDelta(&CpuKernelDelta, CpuTotals.KernelTime.QuadPart);
	MUpdateDelta(&CpuUserDelta, CpuTotals.UserTime.QuadPart);
	MUpdateDelta(&CpuIdleDelta, CpuTotals.IdleTime.QuadPart);

	CpuTotalTime = CpuKernelDelta.Delta + CpuUserDelta.Delta + CpuIdleDelta.Delta;;
}
VOID MSystemPerformanctMonitor::UpdateCpuCycleInformation(PULONG64 IdleCycleTime)
{
	ULONG i;
	ULONG64 total;

	// Idle
	// We need to query this separately because the idle cycle time in SYSTEM_PROCESS_INFORMATION
	// doesn't give us data for individual processors.
	NtQuerySystemInformation(SystemProcessorIdleCycleTimeInformation, CpuIdleCycleTime, sizeof(LARGE_INTEGER) * (ULONG)CpuCount, NULL);

	total = 0;
	for (i = 0; i < (ULONG)CpuCount; i++)
		total += CpuIdleCycleTime[i].QuadPart;

	MUpdateDelta(&CpuIdleCycleDelta, total);
	*IdleCycleTime = CpuIdleCycleDelta.Delta;

	// System
	NtQuerySystemInformation(SystemProcessorCycleTimeInformation, CpuSystemCycleTime, sizeof(LARGE_INTEGER) * (ULONG)CpuCount, NULL);

	total = 0;
	for (i = 0; i < (ULONG)CpuCount; i++)
		total += CpuSystemCycleTime[i].QuadPart;

	MUpdateDelta(&CpuSystemCycleDelta, total);
}
VOID MSystemPerformanctMonitor::UpdateCpuCycleUsageInformation(ULONG64 TotalCycleTime, ULONG64 IdleCycleTime)
{
	ULONG i;
	FLOAT baseCpuUsage;
	FLOAT totalTimeDelta;
	ULONG64 totalTime;

	// Cycle time is not only lacking for kernel/user components, but also for individual
	// processors. We can get the total idle cycle time for individual processors but
	// without knowing the total cycle time for individual processors, this information
	// is useless.
	//
	// We'll start by calculating the total CPU usage, then we'll calculate the kernel/user
	// components. In the event that the corresponding CPU time deltas are zero, we'll split
	// the CPU usage evenly across the kernel/user components. CPU usage for individual
	// processors is left untouched, because it's too difficult to provide an estimate.
	//
	// Let I_1, I_2, ..., I_n be the idle cycle times and T_1, T_2, ..., T_n be the
	// total cycle times. Let I'_1, I'_2, ..., I'_n be the idle CPU times and T'_1, T'_2, ...,
	// T'_n be the total CPU times.
	// We know all I'_n, T'_n and I_n, but we only know sigma(T). The "real" total CPU usage is
	// sigma(I)/sigma(T), and the "real" individual CPU usage is I_n/T_n. The problem is that
	// we don't know T_n; we only know sigma(T). Hence we need to find values i_1, i_2, ..., i_n
	// and t_1, t_2, ..., t_n such that:
	// sigma(i)/sigma(t) ~= sigma(I)/sigma(T), and
	// i_n/t_n ~= I_n/T_n
	//
	// Solution 1: Set i_n = I_n and t_n = sigma(T)*T'_n/sigma(T'). Then:
	// sigma(i)/sigma(t) = sigma(I)/(sigma(T)*sigma(T')/sigma(T')) = sigma(I)/sigma(T), and
	// i_n/t_n = I_n/T'_n*sigma(T')/sigma(T) ~= I_n/T_n since I_n/T'_n ~= I_n/T_n and sigma(T')/sigma(T) ~= 1.
	// However, it is not guaranteed that i_n/t_n <= 1, which may lead to CPU usages over 100% being displayed.
	//
	// Solution 2: Set i_n = I'_n and t_n = T'_n. Then:
	// sigma(i)/sigma(t) = sigma(I')/sigma(T') ~= sigma(I)/sigma(T) since I'_n ~= I_n and T'_n ~= T_n.
	// i_n/t_n = I'_n/T'_n ~= I_n/T_n as above.
	// Not scaling at all is currently the best solution, since it's fast, simple and guarantees that i_n/t_n <= 1.

	baseCpuUsage = 1 - (FLOAT)IdleCycleTime / TotalCycleTime;
	totalTimeDelta = (FLOAT)(CpuKernelDelta.Delta + CpuUserDelta.Delta);

	if (totalTimeDelta != 0)
	{
		CpuKernelUsage = baseCpuUsage * ((FLOAT)CpuKernelDelta.Delta / totalTimeDelta);
		CpuUserUsage = baseCpuUsage * ((FLOAT)CpuUserDelta.Delta / totalTimeDelta);
	}
	else
	{
		CpuKernelUsage = baseCpuUsage / 2;
		CpuUserUsage = baseCpuUsage / 2;
	}

	CpuUsage = (CpuUserUsage + CpuKernelUsage) * 100.0F;

	for (i = 0; i < (ULONG)CpuCount; i++)
	{
		totalTime = CpusKernelDelta[i].Delta + CpusUserDelta[i].Delta + CpusIdleDelta[i].Delta;

		if (totalTime != 0)
		{
			CpusKernelUsage[i] = (FLOAT)CpusKernelDelta[i].Delta / totalTime;
			CpusUserUsage[i] = (FLOAT)CpusUserDelta[i].Delta / totalTime;
		}
		else
		{
			CpusKernelUsage[i] = 0;
			CpusUserUsage[i] = 0;
		}
	}
}

double MSystemPerformanctMonitor::GetCpuUsage()
{
	return CpuUsage;
}
double MSystemPerformanctMonitor::GetCpuUsageKernel()
{
	return CpuKernelUsage;
}
double MSystemPerformanctMonitor::GetCpuUsageUser()
{
	return CpuUserUsage;
}

BOOL MSystemPerformanctMonitor::InitGlobal()
{
	BOOL result = FALSE;
	ULONG ret = 0;
	SYSTEM_BASIC_INFORMATION sbi = { 0 };
	if (NT_SUCCESS(NtQuerySystemInformation(SystemBasicInformation, &sbi, sizeof(sbi), &ret)))
	{
		CpuCount = sbi.NumberOfProcessors;
		PageSize = sbi.PageSize;
		result = TRUE;
	}

	//Init cpus info

	CpusKernelDelta = (PMUINT64_DELTA)malloc(sizeof(MUINT64_DELTA)*CpuCount);
	CpusUserDelta = (PMUINT64_DELTA)malloc(sizeof(MUINT64_DELTA)*CpuCount);
	CpusIdleDelta = (PMUINT64_DELTA)malloc(sizeof(MUINT64_DELTA)*CpuCount);
	CpusKernelUsage = (PFLOAT)malloc(sizeof(FLOAT) *CpuCount);
	CpusUserUsage = (PFLOAT)malloc(sizeof(FLOAT)*CpuCount);

	CpuIdleCycleTime = (PLARGE_INTEGER)malloc(sizeof(LARGE_INTEGER)*CpuCount);
	CpuSystemCycleTime = (PLARGE_INTEGER)malloc(sizeof(LARGE_INTEGER)*CpuCount);

	CpuInformation = (PSYSTEM_PROCESSOR_PERFORMANCE_INFORMATION)malloc(sizeof(SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION) * (ULONG)CpuCount);

	return result;
}
void MSystemPerformanctMonitor::DestroyGlobal()
{
	if (CpusKernelDelta)delete CpusKernelDelta;
	if (CpusUserDelta)delete CpusUserDelta;
	if (CpusIdleDelta)delete CpusIdleDelta;
	if (CpusKernelUsage)delete CpusKernelUsage;
	if (CpusUserUsage)delete CpusUserUsage;
	if (CpuIdleCycleTime)delete CpuIdleCycleTime;
	if (CpuSystemCycleTime)delete CpuSystemCycleTime;
	if (CpuInformation)delete CpuInformation;

	CpusKernelDelta = nullptr;
	CpusUserDelta = nullptr;
	CpusIdleDelta = nullptr;
	CpusKernelUsage = nullptr;
	CpusUserUsage = nullptr;
	CpuIdleCycleTime = nullptr;
	CpuSystemCycleTime = nullptr;
}

ULONGLONG MSystemMemoryPerformanctMonitor::GetAllMemory()
{
	return memory_statuex.ullTotalPhys;
}
ULONGLONG MSystemMemoryPerformanctMonitor::GetKernelPaged()
{
	return performance_info.KernelPaged;
}
ULONGLONG MSystemMemoryPerformanctMonitor::GetKernelNonpaged()
{
	return performance_info.KernelNonpaged;
}
ULONGLONG MSystemMemoryPerformanctMonitor::GetSystemCacheSize()
{
	return performance_info.SystemCache;
}
ULONGLONG MSystemMemoryPerformanctMonitor::GetCommitTotal()
{
	return performance_info.CommitTotal;
}
ULONGLONG MSystemMemoryPerformanctMonitor::GetCommitLimit()
{
	return performance_info.CommitLimit;
}
ULONGLONG MSystemMemoryPerformanctMonitor::GetMemoryAvail() {
	return memory_statuex.ullAvailPhys;
}
ULONGLONG MSystemMemoryPerformanctMonitor::GetMemoryUsed() {
	return memory_statuex.ullTotalPhys - memory_statuex.ullAvailPhys;
}
ULONGLONG MSystemMemoryPerformanctMonitor::GetMemoryAvailPageFile() {
	return performance_info.CommitLimit* PageSize;
}

double MSystemMemoryPerformanctMonitor::GetMemoryUsage()
{
	return  (double)((memory_statuex.ullTotalPhys - memory_statuex.ullAvailPhys) / (double)memory_statuex.ullTotalPhys);
}

BOOL MSystemMemoryPerformanctMonitor::GetMemoryCompressionInfo(PPROCESS_COMPRESSION_INFO outInfo)
{
	SYSTEM_STORE_INFORMATION storeInfo;
	PROCESS_COMPRESSION_INFO compressInfo;

	storeInfo.Version = 1;
	storeInfo.InfoClass = ProcessCompressionInfoRequest;
	storeInfo.Data = &compressInfo;
	storeInfo.Length = sizeof(compressInfo);

	ZeroMemory(&compressInfo, sizeof(compressInfo));
	compressInfo.Version = 3;

	NTSTATUS status = NtQuerySystemInformation(SystemStoreInformation, &storeInfo, sizeof(storeInfo), NULL);
	if (NT_SUCCESS(status))
	{
		memcpy_s(outInfo, sizeof(PROCESS_COMPRESSION_INFO), &compressInfo, sizeof(compressInfo));
		return TRUE;
	}
	else
	{
		LogErr2(L"GetMemoryCompressionInfo failed : 0x%08X", status);
		SetLastError(RtlNtStatusToDosError(status));
		return FALSE;
	}
}

ULONGLONG MSystemMemoryPerformanctMonitor::GetStandBySize()
{
	return standbyPageCount * PageSize;
}
ULONGLONG MSystemMemoryPerformanctMonitor::GetModifiedSize()
{
	return memoryListInfo.ModifiedPageCount* PageSize;
}

BOOL MSystemMemoryPerformanctMonitor::UpdateMemoryListInfo()
{
	standbyPageCount = 0;
	memset(&memoryListInfo, 0, sizeof(memoryListInfo));
	if (NT_SUCCESS(NtQuerySystemInformation(
		SystemMemoryListInformation,
		&memoryListInfo,
		sizeof(SYSTEM_MEMORY_LIST_INFORMATION),
		NULL
	)))
	{
		for (ULONG i = 0; i < 8; i++)
			standbyPageCount += memoryListInfo.PageCountByPriority[i];
		return TRUE;
	}
	return FALSE;
}
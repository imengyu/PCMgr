#include "stdafx.h"
#include "perfhlp.h"
#include "ntdef.h"
#include "sysfuns.h"
#include <Psapi.h>
#include <winsock2.h>
#include <Ws2tcpip.h>
#include <iphlpapi.h>
#include <Tcpestats.h>


int cpu_Count = 0;
__int64 LastTime;
__int64 TimeInterval;
FILETIME CreateTime;
FILETIME ExitTime;

PSYSTEM_LOGICAL_PROCESSOR_INFORMATION buffer = NULL;
PSYSTEM_LOGICAL_PROCESSOR_INFORMATION ptr = NULL;
PERFORMANCE_INFORMATION performance_info;

DWORD numaNodeCount = 0;
DWORD processorL1CacheCount = 0;
DWORD processorL2CacheCount = 0;
DWORD processorL3CacheCount = 0;
DWORD processorPackageCount = 0;
MEMORYSTATUSEX memory_statuex;

void MPERF_FreeCpuInfos()
{
	if (buffer)
		delete buffer;
}

M_CAPI(ULONGLONG) MPERF_GetAllRam()
{
	return memory_statuex.ullTotalPhys;
}
M_CAPI(ULONGLONG) MPERF_GetPageSize()
{
	return performance_info.PageSize;
}
M_CAPI(ULONGLONG) MPERF_GetKernelPaged()
{
	return performance_info.KernelPaged;
}
M_CAPI(ULONGLONG) MPERF_GetKernelNonpaged()
{
	return performance_info.KernelNonpaged;
}
M_CAPI(ULONGLONG) MPERF_GetSystemCacheSize()
{
	return performance_info.SystemCache;
}
M_CAPI(ULONGLONG) MPERF_GetCommitTotal()
{
	return performance_info.CommitTotal;
}
M_CAPI(ULONGLONG) MPERF_GetCommitPeak()
{
	return performance_info.CommitPeak;
}
M_CAPI(ULONGLONG) MPERF_GetRamAvail() {
	return memory_statuex.ullAvailPhys;
}
M_CAPI(ULONGLONG) MPERF_GetRamAvailPageFile() {
	return performance_info.CommitLimit*performance_info.PageSize;
}
M_CAPI(double) MPERF_GetRamUseAge2()
{
	return ((memory_statuex.ullTotalPhys - memory_statuex.ullAvailPhys) / (double)memory_statuex.ullTotalPhys);
}
M_CAPI(BOOL) MPERF_GetRamUseAge()
{
	return GlobalMemoryStatusEx(&memory_statuex);
}
M_CAPI(LONGLONG) MPERF_GetRunTime()
{
	return GetTickCount64();
}
M_CAPI(DWORD) MPERF_GetThreadCount() {
	return performance_info.ThreadCount;
}
M_CAPI(DWORD) MPERF_GetHandleCount() {
	return performance_info.HandleCount;
}
M_CAPI(DWORD) MPERF_GetProcessCount() {
	return performance_info.ProcessCount;
}
M_CAPI(BOOL) MPERF_UpdatePerformance()
{
	return GetPerformanceInfo(&performance_info, sizeof(performance_info));
}
M_CAPI(DWORD) MPERF_GetCpuL1Cache()
{
	return processorL1CacheCount;
}
M_CAPI(DWORD) MPERF_GetCpuL2Cache()
{
	return processorL2CacheCount;
}
M_CAPI(DWORD) MPERF_GetCpuL3Cache()
{
	return processorL3CacheCount;
}
M_CAPI(DWORD) MPERF_GetCpuPackage()
{
	return processorPackageCount;
}
M_CAPI(DWORD) MPERF_GetCpuNodeCount()
{
	return numaNodeCount;
}
M_CAPI(BOOL) MPERF_GetCpuInfos() {
	PCACHE_DESCRIPTOR Cache;
	DWORD returnLength = 0;
	DWORD byteOffset = 0;

	GetLogicalProcessorInformation(NULL, &returnLength);
	buffer = (PSYSTEM_LOGICAL_PROCESSOR_INFORMATION)malloc(returnLength);
	if (GetLogicalProcessorInformation(buffer, &returnLength))
	{
		ptr = buffer;
		while (byteOffset + sizeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION) <= returnLength)
		{
			switch (ptr->Relationship)
			{
			case RelationNumaNode: numaNodeCount++; break;
			case RelationCache: {
				Cache = &ptr->Cache;
				if (Cache->Level == 1)
					processorL1CacheCount += Cache->Size;
				else if (Cache->Level == 2)
					processorL2CacheCount += Cache->Size;
				else if (Cache->Level == 3)
					processorL3CacheCount += Cache->Size;
				break;
			}
			case RelationProcessorPackage: processorPackageCount++; break;
			}
			byteOffset += sizeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION);
			ptr++;
		}
		return TRUE;
	}
	return 0;
}
M_CAPI(BOOL) MPERF_GetCpuName(LPWSTR buf, int size)
{
	HKEY hKey;
	if (ERROR_SUCCESS == RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0", 0, KEY_READ, &hKey))
	{
		WCHAR szValue[64];//长整型数据，如果是字符串数据用char数组  
		DWORD dwSize = sizeof(szValue);
		DWORD dwType = REG_SZ;

		if (RegQueryValueExW(hKey, L"ProcessorNameString", 0, &dwType, (LPBYTE)&szValue, &dwSize) == ERROR_SUCCESS)
		{
			wcscpy_s(buf, size, szValue);
			RegCloseKey(hKey);
			return TRUE;
		}
	}
	RegCloseKey(hKey);
	return 0;
}
M_CAPI(int) MPERF_GetCpuFrequency()	//获取CPU主频
{
	HKEY hKey;
	if (ERROR_SUCCESS == RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0", 0, KEY_READ, &hKey))
	{
		DWORD dwValue;
		DWORD dwSize = sizeof(dwValue);
		DWORD dwType = REG_SZ;

		if (RegQueryValueEx(hKey, L"~MHz", 0, &dwType, (LPBYTE)&dwValue, &dwSize) == ERROR_SUCCESS) {
			RegCloseKey(hKey);
			return static_cast<int>(dwValue);
		}
	}
	RegCloseKey(hKey);
	return 0;
}
M_CAPI(int) MPERF_GetProcessNumber()
{
	SYSTEM_INFO info;
	GetSystemInfo(&info);
	cpu_Count = static_cast<int>(info.dwNumberOfProcessors);
	return (int)info.dwNumberOfProcessors;
}
__int64 FileTimeToInt64(const FILETIME& time)
{
	ULARGE_INTEGER tt;  //64位无符号整型值
	tt.LowPart = time.dwLowDateTime;
	tt.HighPart = time.dwHighDateTime;
	return(tt.QuadPart);  //返回整型值
}
__int64 CompareFileTime(FILETIME time1, FILETIME time2)
{
	__int64 a = ((__int64)time1.dwHighDateTime << 32U) | (__int64)time1.dwLowDateTime;
	__int64 b = ((__int64)time2.dwHighDateTime << 32U) | (__int64)time2.dwLowDateTime;
	return (b - a);
}

FILETIME LastIdleTime, LastKernelTime, LastUserTime;
FILETIME IdleTime, KernelTime, UserTime;

M_CAPI(double)MPERF_GetCupUseAge() {

	memcpy_s(&LastIdleTime, sizeof(FILETIME), &IdleTime, sizeof(FILETIME));
	memcpy_s(&LastKernelTime, sizeof(FILETIME), &KernelTime, sizeof(FILETIME));
	memcpy_s(&LastUserTime, sizeof(FILETIME), &LastUserTime, sizeof(FILETIME));

	GetSystemTimes(&IdleTime, &KernelTime, &UserTime);

	__int64 idle = CompareFileTime(LastIdleTime, IdleTime);
	__int64 kernel = CompareFileTime(LastKernelTime, KernelTime);
	__int64 user = CompareFileTime(LastUserTime, LastUserTime);

	return static_cast<double>((double)(kernel + user - idle) * 100 / (double)(kernel + user));
}

M_CAPI(MPerfAndProcessData*) MPERF_PerfDataCreate()
{
	MPerfAndProcessData * data= new MPerfAndProcessData();
	memset(data, 0, sizeof(MPerfAndProcessData));
	return data;
}
M_CAPI(void) MPERF_PerfDataDestroy(MPerfAndProcessData*data)
{
	if (data) {
		delete data;
	}
}
M_CAPI(void) MPERF_CpuTimeUpdate()
{
	FILETIME now;
	GetSystemTimeAsFileTime(&now);
	__int64 nowu = FileTimeToInt64(now);;
	TimeInterval = nowu - LastTime;
	LastTime = nowu;
}
M_CAPI(double) MPERF_GetProcessCpuUseAge(PSYSTEM_PROCESSES p, MPerfAndProcessData*data)
{
	if (p && data)
	{
		data->LastCpuTime = data->NowCpuTime;
		data->NowCpuTime = (p->KernelTime.QuadPart + p->UserTime.QuadPart) / cpu_Count;
		if (TimeInterval == 0) return 0;
		__int64 i1 = (data->NowCpuTime - data->LastCpuTime) / 1000;
		double rs = static_cast<double>((double)i1 / (double)(TimeInterval / 100000));
		return (rs < 0.1 && rs>0.05) ? 0.1 : rs;
	}
	return -1;
}
M_CAPI(SIZE_T) MPERF_GetProcessRam(PSYSTEM_PROCESSES p)
{
	if (p)
		return p->VmCounters.WorkingSetSize;
	return 0;
}
M_CAPI(DWORD) MPERF_GetProcessDiskRate(PSYSTEM_PROCESSES p, MPerfAndProcessData*data)
{
	if (p && data)
	{
		PIO_COUNTERS io_counter = &p->IoCounters;
		if (io_counter)
		{
			ULONGLONG outRead = io_counter->ReadTransferCount - data->LastRead;
			ULONGLONG outWrite = io_counter->WriteTransferCount - data->LastWrite;

			data->LastRead = io_counter->ReadTransferCount;
			data->LastWrite = io_counter->WriteTransferCount;

			DWORD interval = static_cast<DWORD>(TimeInterval / 10000000);
			if (interval <= 0)interval = 1;

			return static_cast<DWORD>(((outRead + outWrite) / 1024) / interval);
		}
	}
	return 0;
}


PMIB_TCPTABLE_OWNER_PID netProcess = NULL;

extern _GetPerTcpConnectionEStats dGetPerTcpConnectionEStats;
extern _GetExtendedTcpTable dGetExtendedTcpTable;

M_CAPI(BOOL) MPERF_GetConnectNetWorkAllBuffer(DWORD dwLocalAddr, DWORD dwLocalPort, DWORD dwRemoteAddr, DWORD dwRemotePort, DWORD dwState, MPerfAndProcessData*data)
{
	MIB_TCPROW row = { 0 };
	row.dwLocalAddr = dwLocalAddr;
	row.dwLocalPort = dwLocalPort;
	row.dwRemoteAddr = dwRemoteAddr;
	row.dwRemotePort = dwRemotePort;
	row.dwState = dwState;

	TCP_ESTATS_BANDWIDTH_ROD_v0 rod= { 0 };
	if (dGetPerTcpConnectionEStats(&row, TcpConnectionEstatsData, NULL, 0, 0, NULL, 0, 0, (LPBYTE)&rod, 0, sizeof(rod)) == NO_ERROR)
	{
		data->NetWorkInBandWidth += rod.InboundBandwidth;
		data->NetWorkOutBandWidth += rod.OutboundBandwidth;

		return TRUE;
	}
	return 0;
}
M_CAPI(ULONG64) MPERF_GetProcessNetWorkRate(DWORD pid, MPerfAndProcessData*data)
{
	if (data && netProcess)
	{
		data->NetWorkInBandWidth = 0;
		data->NetWorkOutBandWidth = 0;
		for (UINT i = 0; i < netProcess->dwNumEntries; i++)
		{
			if (netProcess->table[i].dwOwningPid == pid)
			{
				MPERF_GetConnectNetWorkAllBuffer(netProcess->table[i].dwLocalAddr, netProcess->table[i].dwLocalPort,
					netProcess->table[i].dwRemoteAddr, netProcess->table[i].dwRemotePort, netProcess->table[i].dwState, data);
			}
		}
		return (data->NetWorkInBandWidth + data->NetWorkOutBandWidth);
	}
	return 0;
}

M_CAPI(BOOL)MPERF_NET_IsProcessInNet(DWORD pid) {
	BOOL rs = FALSE;
	if (netProcess) 
	{
		for (UINT i = 0; i < netProcess->dwNumEntries; i++)
		{
			if (netProcess->table[i].dwOwningPid == pid)
			{
				rs = TRUE;
				break;
			}
		}
	}
	return rs;
}
M_CAPI(void)MPERF_NET_FreeAllProcessNetInfo()
{
	if (netProcess)
	{
		free(netProcess);
		netProcess = NULL;
	}
}
M_CAPI(BOOL)MPERF_NET_UpdateAllProcessNetInfo()
{
	MPERF_NET_FreeAllProcessNetInfo();
	
	DWORD dwSize = sizeof(MIB_TCPTABLE_OWNER_PID);
	netProcess = (PMIB_TCPTABLE_OWNER_PID)malloc(sizeof(MIB_TCPTABLE_OWNER_PID));
	memset(netProcess, 0, sizeof(dwSize));
	if (dGetExtendedTcpTable(netProcess, &dwSize, TRUE, AF_INET, TCP_TABLE_OWNER_PID_CONNECTIONS, 0) == ERROR_INSUFFICIENT_BUFFER)
	{
		free(netProcess);
		DWORD realSize = sizeof(DWORD) + dwSize * sizeof(MIB_TCPROW_OWNER_PID);
		netProcess = (PMIB_TCPTABLE_OWNER_PID)malloc(realSize);
		memset(netProcess, 0, sizeof(realSize));
		if (GetExtendedTcpTable(netProcess, &dwSize, TRUE, AF_INET, TCP_TABLE_OWNER_PID_CONNECTIONS, 0) != NO_ERROR)
		{
			MPERF_NET_FreeAllProcessNetInfo();
			return FALSE;
		}
	}
	return TRUE;
}
M_CAPI(void)MPERF_GetNetInfo()
{
	MPERF_NET_FreeAllProcessNetInfo();
}
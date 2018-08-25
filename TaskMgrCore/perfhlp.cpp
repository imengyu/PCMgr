#include "stdafx.h"
#include "perfhlp.h"
#include "sysfuns.h"
#include "loghlp.h"
#include "prochlp.h"
#include "StringHlp.h"
#include <Psapi.h>
#include <winsock2.h>
#include <Ws2tcpip.h>
#include <iphlpapi.h>
#include <Tcpestats.h>
#include <vector>

#define PAGE_SIZE 0x1000

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

//声明查询句柄hquery
HQUERY hQuery = NULL; 

M_CAPI(BOOL) MPERF_GlobalInit() {

	MPERF_UpdatePerformance();	

	PDH_STATUS pdhstatus = PdhOpenQuery(0, 0, &hQuery);
	return (pdhstatus == ERROR_SUCCESS);
}
M_CAPI(VOID) MPERF_GlobalDestroy() {
	PdhCloseQuery(hQuery);
}
M_CAPI(BOOL) MPERF_GlobalUpdatePerformanceCounters()
{
	return !PdhCollectQueryData(hQuery);
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
M_CAPI(ULONGLONG) MPERF_GetCommitLimit()
{
	return performance_info.CommitLimit;
}
M_CAPI(ULONGLONG) MPERF_GetRamAvail() {
	return memory_statuex.ullAvailPhys;
}
M_CAPI(ULONGLONG) MPERF_GetRamAvailPageFile() {
	return performance_info.CommitLimit*performance_info.PageSize;
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

PDH_HCOUNTER *counter3cpu = NULL;//CPU性能计数器
PDH_HCOUNTER *counter3disk = NULL;//磁盘性能计数器
PDH_HCOUNTER *counter3network = NULL;//网络性能计数器
BOOL counter3Inited = FALSE;

//Computer Performance

M_CAPI(LPWSTR) MPERF_EnumPerformanceCounterInstanceNames(LPWSTR counterName)
{
	PDH_STATUS status = ERROR_SUCCESS;

	LPWSTR pwsCounterListBuffer = NULL;
	DWORD dwCounterListSize = 0;
	LPWSTR pwsInstanceListBuffer = NULL;
	DWORD dwInstanceListSize = 0;

	status = PdhEnumObjectItems(NULL, NULL, counterName, pwsCounterListBuffer, &dwCounterListSize, pwsInstanceListBuffer, &dwInstanceListSize, PERF_DETAIL_WIZARD, 0);
	if (status == PDH_MORE_DATA || (status == PDH_INVALID_ARGUMENT && dwInstanceListSize > 0))
	{
		// Allocate the buffers and try the call again.
		pwsCounterListBuffer = (LPWSTR)malloc(dwCounterListSize * sizeof(WCHAR));
		pwsInstanceListBuffer = (LPWSTR)malloc(dwInstanceListSize * sizeof(WCHAR));

		status = PdhEnumObjectItems(NULL, NULL, counterName, 
			pwsCounterListBuffer,
			&dwCounterListSize,
			pwsInstanceListBuffer,
			&dwInstanceListSize, PERF_DETAIL_WIZARD, 0);

		free(pwsCounterListBuffer);

		if (status == ERROR_SUCCESS)
			return pwsInstanceListBuffer;
		else LogErr(L"Second PdhEnumObjectItems \"Network Interface\" failed : 0x%X (Counter name : %s)", status, counterName);
		free(pwsInstanceListBuffer);
	}
	else LogErr(L"First PdhEnumObjectItems \"Network Interface\" failed : 0x%X (Counter name : %s)", status, counterName);
	return NULL;
}

M_CAPI(BOOL) MPERF_Init3PerformanceCounters()
{
	BOOL rs = FALSE;
	if (hQuery)
	{
		if (!counter3Inited)
		{
			//为计数器分配存储空间
			counter3cpu = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
			counter3disk = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
			counter3network = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));

			//添加性能计数器
			PDH_STATUS status = ERROR_SUCCESS;
			status = PdhAddCounter(hQuery, L"\\Processor Information(_Total)\\% Processor Time", 0, counter3cpu);
			if (status != ERROR_SUCCESS) {
				GlobalFree(counter3cpu);
				counter3cpu = NULL;
				LogErr(L"Add Performance Counter \"Processor Information(_Total)\\% Processor Time\" failed : 0x%X", status);
			}
			status = PdhAddCounter(hQuery, L"\\PhysicalDisk(_Total)\\% Disk Time", 0, counter3disk);
			if (status != ERROR_SUCCESS) {
				GlobalFree(counter3disk);
				counter3disk = NULL;
				LogErr(L"Add Performance Counter \"PhysicalDisk(_Total)\\% Disk Time\" failed : 0x%X", status);
			}


			LPWSTR netInstanceNames = MPERF_EnumPerformanceCounterInstanceNames(L"Network Interface");
			if (netInstanceNames) 
			{
				std::wstring netCounterName = FormatString(L"\\Network Interface(%s)\\Bytes Total/sec", netInstanceNames);
				status = PdhAddCounter(hQuery, netCounterName.c_str(), 0, counter3network);
				if (status != ERROR_SUCCESS)
				{
					LogErr(L"Add Performance Counter \"%s\" failed : 0x%X", netCounterName.c_str(), status);
					GlobalFree(counter3network);
					counter3network = NULL;
				}
				free(netInstanceNames);
			}

			counter3Inited = TRUE;
		}
	}
	return rs;
}
M_CAPI(BOOL) MPERF_Destroy3PerformanceCounters()
{
	if (hQuery)
	{
		//释放计数器
		if (counter3cpu) {
			PdhRemoveCounter(*counter3cpu);
			GlobalFree(counter3cpu);
		}
		if (counter3disk) {
			PdhRemoveCounter(*counter3disk);
			GlobalFree(counter3disk);
		}
		if (counter3network) {
			PdhRemoveCounter(*counter3network);
			GlobalFree(counter3network);
		}

		counter3Inited = FALSE;

		return TRUE;
	}
	return FALSE;
}

M_CAPI(double)MPERF_GetCupUseAge_OrgCalcute()
{
	memcpy_s(&LastIdleTime, sizeof(FILETIME), &IdleTime, sizeof(FILETIME));
	memcpy_s(&LastKernelTime, sizeof(FILETIME), &KernelTime, sizeof(FILETIME));
	memcpy_s(&LastUserTime, sizeof(FILETIME), &LastUserTime, sizeof(FILETIME));

	GetSystemTimes(&IdleTime, &KernelTime, &UserTime);

	__int64 idle = CompareFileTime(LastIdleTime, IdleTime);
	__int64 kernel = CompareFileTime(LastKernelTime, KernelTime);
	__int64 user = CompareFileTime(LastUserTime, LastUserTime);

	return static_cast<double>((double)(kernel + user - idle) * 100 / (double)(kernel + user));
}
M_CAPI(double)MPERF_GetCupUseAge_Pdh()
{
	if (counter3cpu)
	{
		PDH_FMT_COUNTERVALUE pdh_counter_value;
		DWORD pdh_counter_value_type;
		PdhGetFormattedCounterValue(*counter3cpu, PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
		return pdh_counter_value.doubleValue;
	}
	return 0;
}

M_CAPI(double)MPERF_GetCupUseAge() {
	if (counter3cpu)
		return MPERF_GetCupUseAge_Pdh();
	else return MPERF_GetCupUseAge_OrgCalcute();
}
M_CAPI(double)MPERF_GetRamUseAge2()
{
	if (MPERF_GetRamUseAge())
		return  (double)((memory_statuex.ullTotalPhys - memory_statuex.ullAvailPhys) / (double)memory_statuex.ullTotalPhys);
	return 0.0;
}
M_CAPI(BOOL) MPERF_GetRamUseAge()
{
	return GlobalMemoryStatusEx(&memory_statuex);
}
M_CAPI(double)MPERF_GetDiskUseage() 
{
	if (counter3disk)
	{
		PDH_FMT_COUNTERVALUE pdh_counter_value;
		DWORD pdh_counter_value_type;
		PdhGetFormattedCounterValue(*counter3disk, PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
		return pdh_counter_value.doubleValue / 100;
	}
	return 0;
}
M_CAPI(double)MPERF_GetNetWorkUseage() 
{
	if (counter3network)
	{
		PDH_FMT_COUNTERVALUE pdh_counter_value;
		DWORD pdh_counter_value_type;
		PdhGetFormattedCounterValue(*counter3network, PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
		return (pdh_counter_value.doubleValue * 0.0000001);
	}
	return 0;
}

//Process Performance

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
M_CAPI(ULONGLONG) MPERF_GetProcessCpuTime(PSYSTEM_PROCESSES p)
{
	if (p)
		return (ULONGLONG)(p->KernelTime.QuadPart + p->UserTime.QuadPart);
	return -1;
}
M_CAPI(ULONGLONG) MPERF_GetProcessCycle(PSYSTEM_PROCESSES p)
{
	if (p)
		return (ULONGLONG)(p->CycleTime);
	return -1;
}
M_CAPI(SIZE_T) MPERF_GetProcessRam(PSYSTEM_PROCESSES p, HANDLE hProcess)
{
	if (p) {	
		return (SIZE_T)p->WorkingSetPrivateSize.QuadPart;
		//return p->VmCounters.WorkingSetSize;
	}
	return 0;
}
M_CAPI(SIZE_T) MPERF_GetProcessMemoryInfo(PSYSTEM_PROCESSES p, int col)
{
	if (p) {
		switch (col) {
		case M_GET_PROCMEM_WORKINGSET:
			return (SIZE_T)p->VmCounters.WorkingSetSize;
		case M_GET_PROCMEM_WORKINGSETPRIVATE:
			return (SIZE_T)p->WorkingSetPrivateSize.QuadPart;
		case M_GET_PROCMEM_WORKINGSETSHARE:
			return (SIZE_T)(p->VmCounters.WorkingSetSize - (SIZE_T)p->WorkingSetPrivateSize.QuadPart);
		case M_GET_PROCMEM_PEAKWORKINGSET:
			return (SIZE_T)p->VmCounters.PeakWorkingSetSize;
		case M_GET_PROCMEM_COMMITEDSIZE:
			return (SIZE_T)0;
		case M_GET_PROCMEM_NONPAGEDPOOL:
			return (SIZE_T)p->VmCounters.QuotaNonPagedPoolUsage;
		case M_GET_PROCMEM_PAGEDPOOL:
			return (SIZE_T)p->VmCounters.QuotaPagedPoolUsage;
		case M_GET_PROCMEM_PAGEDFAULT:
			return (SIZE_T)p->VmCounters.PageFaultCount;
		}
	}
	return 0;
}
M_CAPI(ULONGLONG) MPERF_GetProcessIOInfo(PSYSTEM_PROCESSES p, int col)
{
	if (p) {
		switch (col)
		{
		case M_GET_PROCIO_READ :
			return p->IoCounters.ReadOperationCount;
		case M_GET_PROCIO_WRITE :
			return p->IoCounters.WriteOperationCount;
		case M_GET_PROCIO_OTHER :
			return p->IoCounters.OtherOperationCount;
		case M_GET_PROCIO_READ_BYTES:
			return p->IoCounters.ReadTransferCount;
		case M_GET_PROCIO_WRITE_BYTES:
			return p->IoCounters.WriteTransferCount;
		case M_GET_PROCIO_OTHER_BYTES:
			return p->IoCounters.OtherTransferCount;
		default:
			break;
		}
	}
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

//Process Net Work Performance

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
		if (dGetExtendedTcpTable(netProcess, &dwSize, TRUE, AF_INET, TCP_TABLE_OWNER_PID_CONNECTIONS, 0) != NO_ERROR)
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

//Cpus Performance
std::vector<PDH_HCOUNTER*> cpuCounters;

M_CAPI(BOOL) MPERF_InitCpuDetalsPerformanceCounters()
{
	if (hQuery)
	{
		LPWSTR cpuInstanceNames = MPERF_EnumPerformanceCounterInstanceNames(L"Processor Information");
		if (cpuInstanceNames)
		{
			for (LPWSTR pTemp = cpuInstanceNames; *pTemp != 0; pTemp += wcslen(pTemp) + 1)
			{
				if (wcscmp(pTemp, L"_Total") != 0 && wcscmp(pTemp, L"0,_Total") != 0)
				{
					
					std::wstring cpuCounterName = FormatString(L"\\Processor Information(%s)\\%% Processor Time", pTemp);
					PDH_HCOUNTER*thisCounter = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
					PDH_STATUS status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, thisCounter);
					if (status != ERROR_SUCCESS)
					{
						GlobalFree(thisCounter);
						LogErr(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
					}
					else cpuCounters.push_back(thisCounter);
				}
			}
			free(cpuInstanceNames);
		}
		return TRUE;
	}
	return FALSE;
}
M_CAPI(BOOL) MPERF_DestroyCpuDetalsPerformanceCounters()
{
	if (hQuery)
	{
		for (auto it = cpuCounters.begin(); it != cpuCounters.end(); it++) 
		{
			PdhRemoveCounter(*(*it));
			GlobalFree(*it);
		}

		cpuCounters.clear();
		return TRUE;
	}
	return FALSE;
}
M_CAPI(int) MPERF_GetCpuDetalsPerformanceCountersCount() 
{
	return static_cast<int>(cpuCounters.size());
}
M_CAPI(double) MPERF_GetCpuDetalsCpuUsage(int index)
{
	if (index >= 0 && (UINT)index < cpuCounters.size())
	{
		PDH_FMT_COUNTERVALUE pdh_counter_value;
		DWORD pdh_counter_value_type;
		PdhGetFormattedCounterValue(*cpuCounters[index], PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
		return pdh_counter_value.doubleValue;
	}
	return 0;
}

//Disks Performance
std::vector<MPerfDiskData*> diskCounters;

M_CAPI(UINT) MPERF_InitDisksPerformanceCounters()
{
	if (hQuery)
	{
		PDH_STATUS status;
		LPWSTR diskInstanceNames = MPERF_EnumPerformanceCounterInstanceNames(L"PhysicalDisk");
		if (diskInstanceNames)
		{
			for (LPWSTR pTemp = diskInstanceNames; *pTemp != 0; pTemp += wcslen(pTemp) + 1)
			{
				if (wcscmp(pTemp, L"_Total") != 0)
				{
					MPerfDiskData *data = (MPerfDiskData*)malloc(sizeof(MPerfDiskData));
					memset(data, 0, sizeof(MPerfDiskData));
					wcscpy_s(data->performanceCounter_Name, pTemp);

					std::wstring cpuCounterName = FormatString(L"\\PhysicalDisk(%s)\\Avg. Disk Queue Length", pTemp);
					data->performanceCounter_avgQue = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
					status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_avgQue);
					if (status != ERROR_SUCCESS)
					{
						GlobalFree(data->performanceCounter_avgQue);
						data->performanceCounter_avgQue = nullptr;
						LogErr(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
					}

					cpuCounterName = FormatString(L"\\PhysicalDisk(%s)\\Disk Reads/sec", pTemp);
					data->performanceCounter_read = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
					status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_read);
					if (status != ERROR_SUCCESS)
					{
						GlobalFree(data->performanceCounter_avgQue);
						data->performanceCounter_avgQue = nullptr;
						LogErr(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
					}

					cpuCounterName = FormatString(L"\\PhysicalDisk(%s)\\Disk Writes/sec", pTemp);
					data->performanceCounter_write = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
					status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_write);
					if (status != ERROR_SUCCESS)
					{
						GlobalFree(data->performanceCounter_write);
						data->performanceCounter_write = nullptr;
						LogErr(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
					}

					cpuCounterName = FormatString(L"\\PhysicalDisk(%s)\\Disk Read Bytes/sec", pTemp);
					data->performanceCounter_readSpeed = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
					status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_readSpeed);
					if (status != ERROR_SUCCESS)
					{
						GlobalFree(data->performanceCounter_readSpeed);
						data->performanceCounter_readSpeed = nullptr;
						LogErr(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
					}

					cpuCounterName = FormatString(L"\\PhysicalDisk(%s)\\Disk Write Bytes/sec", pTemp);
					data->performanceCounter_writeSpeed = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
					status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_writeSpeed);
					if (status != ERROR_SUCCESS)
					{
						GlobalFree(data->performanceCounter_writeSpeed);
						data->performanceCounter_writeSpeed = nullptr;
						LogErr(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
					}

					diskCounters.push_back(data);
				}
			}

			return (UINT)diskCounters.size();
			free(diskInstanceNames);
		}
	}
	return FALSE;
}
M_CAPI(BOOL) MPERF_DestroyDisksPerformanceCounters()
{
	if (hQuery)
	{
		for (auto it = diskCounters.begin(); it != diskCounters.end(); it++)
		{
			MPerfDiskData *data = (*it);
			if (data->performanceCounter_avgQue)
			{
				PdhRemoveCounter(*data->performanceCounter_avgQue);
				GlobalFree(data->performanceCounter_avgQue);
			}
			if (data->performanceCounter_read)
			{
				PdhRemoveCounter(*data->performanceCounter_read);
				GlobalFree(data->performanceCounter_read);
			}
			if (data->performanceCounter_readSpeed)
			{
				PdhRemoveCounter(*data->performanceCounter_readSpeed);
				GlobalFree(data->performanceCounter_readSpeed);
			}
			if (data->performanceCounter_write)
			{
				PdhRemoveCounter(*data->performanceCounter_write);
				GlobalFree(data->performanceCounter_write);
			}
			if (data->performanceCounter_writeSpeed)
			{
				PdhRemoveCounter(*data->performanceCounter_writeSpeed);
				GlobalFree(data->performanceCounter_writeSpeed);
			}

			free(data);
		}
	}
	return FALSE;
}
M_CAPI(MPerfDiskData*) MPERF_GetDisksPerformanceCounters(int index)
{
	if (index >= 0 && (UINT)index < diskCounters.size())
		return diskCounters[index];
	return 0;
}
M_CAPI(BOOL) MPERF_GetDisksPerformanceCountersValues(MPerfDiskData*data,
	double*out_readSpeed, double*out_writeSpeed, double*out_read, double*out_write, double*out_readavgQue)
{
	if (data)
	{
		PDH_FMT_COUNTERVALUE pdh_counter_value;
		DWORD pdh_counter_value_type;
		if (out_readSpeed)
		{
			PdhGetFormattedCounterValue(*data->performanceCounter_readSpeed, PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
			*out_readSpeed = pdh_counter_value.doubleValue;
		}
		if (out_writeSpeed)
		{
			PdhGetFormattedCounterValue(*data->performanceCounter_writeSpeed, PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
			*out_writeSpeed = pdh_counter_value.doubleValue;
		}
		if (out_read)
		{
			PdhGetFormattedCounterValue(*data->performanceCounter_read, PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
			*out_read = pdh_counter_value.doubleValue;
		}
		if (out_write)
		{
			PdhGetFormattedCounterValue(*data->performanceCounter_write, PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
			*out_write = pdh_counter_value.doubleValue;
		}
		if (out_readavgQue)
		{
			PdhGetFormattedCounterValue(*data->performanceCounter_avgQue, PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
			*out_readavgQue = pdh_counter_value.doubleValue / 100;
		}
	}
	return FALSE;
}
M_CAPI(BOOL) MPERF_GetDisksPerformanceCountersInstanceName(MPerfDiskData*data, LPWSTR buf, int size) {
	if (data)
	{
		wcscpy_s(buf, size, data->performanceCounter_Name);
		return TRUE;
	}
	return FALSE;
}
M_CAPI(double)MPERF_GetDisksPerformanceCountersSimpleValues(MPerfDiskData*data)
{
	if (data && data->performanceCounter_avgQue)
	{
		PDH_FMT_COUNTERVALUE pdh_counter_value;
		DWORD pdh_counter_value_type;
		PdhGetFormattedCounterValue(*data->performanceCounter_avgQue, PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
		return pdh_counter_value.doubleValue * 100;
	}
	return 0;
}

//Network Performance
std::vector<MPerfNetData*> netCounters;

M_CAPI(UINT) MPERF_InitNetworksPerformanceCounters()
{
	if (hQuery)
	{
		PDH_STATUS status;
		DWORD netsInstanceNamesSize = 0;
		LPWSTR netInstanceNames = MPERF_EnumPerformanceCounterInstanceNames(L"Network Interface");
		if (netInstanceNames)
		{
			for (LPWSTR pTemp = netInstanceNames; *pTemp != 0; pTemp += wcslen(pTemp) + 1)
			{
				MPerfNetData *data = (MPerfNetData*)malloc(sizeof(MPerfNetData));
				memset(data, 0, sizeof(MPerfNetData));
				wcscpy_s(data->performanceCounter_Name, pTemp);

				std::wstring cpuCounterName = FormatString(L"\\Network Interface(%s)\\Bytes Sent/sec", pTemp);
				data->performanceCounter_sent = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
				status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_sent);
				if (status != ERROR_SUCCESS)
				{
					GlobalFree(data->performanceCounter_sent);
					data->performanceCounter_sent = 0;
					LogErr(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
				}

				cpuCounterName = FormatString(L"\\Network Interface(%s)\\Bytes Received/sec", pTemp);
				data->performanceCounter_receive = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
				status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_receive);
				if (status != ERROR_SUCCESS)
				{
					GlobalFree(data->performanceCounter_receive);
					data->performanceCounter_receive = 0;
					LogErr(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
				}

				netCounters.push_back(data);
			}		
			free(netInstanceNames);
			return (UINT)diskCounters.size();
		}
	}
	return FALSE;
}
M_CAPI(BOOL) MPERF_DestroyNetworksPerformanceCounters()
{
	if (hQuery)
	{
		for (auto it = netCounters.begin(); it != netCounters.end(); it++)
		{
			MPerfNetData *data = (*it);
			if (data->performanceCounter_receive)
			{
				PdhRemoveCounter(*data->performanceCounter_receive);
				GlobalFree(data->performanceCounter_receive);
			}
			if (data->performanceCounter_sent)
			{
				PdhRemoveCounter(*data->performanceCounter_sent);
				GlobalFree(data->performanceCounter_sent);
			}

			free(data);
		}
	}
	return FALSE;
}
M_CAPI(MPerfNetData*) MPERF_GetNetworksPerformanceCounters(int index)
{
	if (index >= 0 && (UINT)index < netCounters.size())
		return netCounters[index];
	return 0;
}
M_CAPI(BOOL) MPERF_GetNetworksPerformanceCountersValues(MPerfNetData*data, double*out_sent, double*out_receive)
{
	if (data)
	{
		PDH_FMT_COUNTERVALUE pdh_counter_value;
		DWORD pdh_counter_value_type;
		if (out_sent)
		{
			PdhGetFormattedCounterValue(*data->performanceCounter_sent, PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
			*out_sent = pdh_counter_value.doubleValue;
		}
		if (out_receive)
		{
			PdhGetFormattedCounterValue(*data->performanceCounter_receive, PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
			*out_receive = pdh_counter_value.doubleValue;
		}
	}
	return FALSE;
}
M_CAPI(BOOL) MPERF_GetNetworksPerformanceCountersInstanceName(MPerfNetData*data, LPWSTR buf, int size) {
	if (data)
	{
		wcscpy_s(buf, size, data->performanceCounter_Name);
		return TRUE;
	}
	return FALSE;
}
M_CAPI(double)MPERF_GetNetworksPerformanceCountersSimpleValues(MPerfNetData*data)
{
	if (data && data->performanceCounter_total)
	{
		PDH_FMT_COUNTERVALUE pdh_counter_value;
		DWORD pdh_counter_value_type;
		PdhGetFormattedCounterValue(*data->performanceCounter_total, PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
		return pdh_counter_value.doubleValue * 0.00001;
	}
	return 0;
}


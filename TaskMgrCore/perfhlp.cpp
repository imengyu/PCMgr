#include "stdafx.h"
#include "perfhlp.h"
#include "mapphlp.h"
#include "sysfuns.h"
#include "msup.h"
#include "loghlp.h"
#include "prochlp.h"
#include "StringHlp.h"
#include "MConnectionMonitor.h"
#include <vector>
#include <list>

#include "MSystemPerformanctMonitor.h"

extern NtQuerySystemInformationFun NtQuerySystemInformation;
extern NtQueryInformationProcessFun NtQueryInformationProcess;
extern RtlNtStatusToDosErrorFun RtlNtStatusToDosError;

//声明查询句柄hquery
HQUERY hQuery = NULL; 

M_CAPI(BOOL) MPERF_GlobalInit()
{
	PDH_STATUS pdhstatus = PdhOpenQuery(0, 0, &hQuery);
	if (pdhstatus != ERROR_SUCCESS) {
		LogErr2(L"PdhOpenQuery failed : %d", GetLastError());
		return FALSE;
	}
	return TRUE;
}
M_CAPI(VOID) MPERF_GlobalDestroy() {
	PdhCloseQuery(hQuery);
}
M_CAPI(BOOL) MPERF_GlobalUpdatePerformanceCounters()
{
	PDH_STATUS pdhstatus = PdhCollectQueryData(hQuery);
	if (pdhstatus != ERROR_SUCCESS) {
		LogErr2(L"PdhCollectQueryData failed : %d", GetLastError());
		return FALSE;
	}
	return TRUE;
}
M_CAPI(BOOL) MPERF_GlobalUpdateCpu() {
	return MSystemPerformanctMonitor::UpdateCpuGlobal();
}

MConnectionMonitor connectionMonitor;

PDH_HCOUNTER *counter3cpuuser = NULL;//CPU性能计数器User
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
		pwsCounterListBuffer = (LPWSTR)MAlloc(dwCounterListSize * sizeof(WCHAR));
		pwsInstanceListBuffer = (LPWSTR)MAlloc(dwInstanceListSize * sizeof(WCHAR));

		status = PdhEnumObjectItems(NULL, NULL, counterName,
			pwsCounterListBuffer,
			&dwCounterListSize,
			pwsInstanceListBuffer,
			&dwInstanceListSize, PERF_DETAIL_WIZARD, 0);

		MFree(pwsCounterListBuffer);

		if (status == ERROR_SUCCESS)
			return pwsInstanceListBuffer;
		else LogErr2(L"Second PdhEnumObjectItems \"Network Interface\" failed : 0x%X (Counter name : %s)", status, counterName);
		MFree(pwsInstanceListBuffer);
	}
	else LogErr2(L"First PdhEnumObjectItems \"Network Interface\" failed : 0x%X (Counter name : %s)", status, counterName);
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
			counter3cpuuser = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));

			//添加性能计数器
			PDH_STATUS status = ERROR_SUCCESS;
			status = PdhAddCounter(hQuery, L"\\Processor Information(_Total)\\% Processor Time", 0, counter3cpu);
			if (status != ERROR_SUCCESS) {
				GlobalFree(counter3cpu);
				counter3cpu = NULL;
				LogErr2(L"Add Performance Counter \"Processor Information(_Total)\\% Processor Time\" failed : 0x%X", status);
			}

			status = PdhAddCounter(hQuery, L"\\Processor Information(_Total)\\% User Time", 0, counter3cpuuser);
			if (status != ERROR_SUCCESS) {
				GlobalFree(counter3cpuuser);
				counter3cpuuser = NULL;
				LogErr2(L"Add Performance Counter \"Processor Information(_Total)\\% User Time\" failed : 0x%X", status);
			}

			status = PdhAddCounter(hQuery, L"\\PhysicalDisk(_Total)\\% Disk Time", 0, counter3disk);
			if (status != ERROR_SUCCESS) {
				GlobalFree(counter3disk);
				counter3disk = NULL;
				LogErr2(L"Add Performance Counter \"PhysicalDisk(_Total)\\% Disk Time\" failed : 0x%X", status);
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
				MFree(netInstanceNames);
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
		if (counter3cpuuser) {
			PdhRemoveCounter(*counter3cpuuser);
			GlobalFree(counter3cpuuser);
		}

		counter3Inited = FALSE;

		return TRUE;
	}
	return FALSE;
}
M_CAPI(double)MPERF_GetCpuUseAgeUser() {

	return MSystemPerformanctMonitor::GetCpuUsageUser();
}
M_CAPI(double)MPERF_GetCpuUseAgeKernel() {

	return MSystemPerformanctMonitor::GetCpuUsageKernel();
}
M_CAPI(double)MPERF_GetCpuUseAge()
{
	return MSystemPerformanctMonitor::GetCpuUsage();
}
M_CAPI(double)MPERF_GetCpuUseAge2()
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
M_CAPI(double)MPERF_GetCpuUseAgeUser2()
{
	if (counter3cpuuser)
	{
		PDH_FMT_COUNTERVALUE pdh_counter_value;
		DWORD pdh_counter_value_type;
		PdhGetFormattedCounterValue(*counter3cpuuser, PDH_FMT_DOUBLE, &pdh_counter_value_type, &pdh_counter_value);
		return pdh_counter_value.doubleValue;
	}
	return 0;
}
M_CAPI(double)MPERF_GetRamUseAge2()
{
	return MSystemMemoryPerformanctMonitor::GetMemoryUsage();
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
M_CAPI(ULONGLONG) MPERF_GetRamAll() {
	MEMORYSTATUSEX tmemory_statuex = { 0 };
	tmemory_statuex.dwLength = sizeof(tmemory_statuex);
	if (!GlobalMemoryStatusEx(&tmemory_statuex))	LogErr2(L"GlobalMemoryStatusEx failed : %d", GetLastError());
	return tmemory_statuex.ullTotalPhys;
}

//Process Net Work Performance

M_CAPI(ULONG64) MPERF_GetProcessNetworkSpeed(PMPROCESS_ITEM p) {
	return connectionMonitor.GetProcessConnectSpeed(p);
}
M_CAPI(BOOL)MPERF_NET_IsProcessInNet(DWORD pid) {
	return connectionMonitor.IsProcessHasConnection(pid);
}
M_CAPI(BOOL)MPERF_NET_UpdateAllProcessNetInfo() {
	return connectionMonitor.Update();
}
M_CAPI(BOOL)MPERF_NET_EnumTcpConnections(MCONNECTION_ENUM_CALLBACK cp) {
	return connectionMonitor.UpdateListData(cp);
}
M_CAPI(PWSTR) MPERF_NET_TcpConnectionStateToString(ULONG State)
{
	switch (State)
	{
	case MIB_TCP_STATE_CLOSED:
		return L"Closed";
	case MIB_TCP_STATE_LISTEN:
		return L"Listen";
	case MIB_TCP_STATE_SYN_SENT:
		return L"SYN sent";
	case MIB_TCP_STATE_SYN_RCVD:
		return L"SYN received";
	case MIB_TCP_STATE_ESTAB:
		return L"Established";
	case MIB_TCP_STATE_FIN_WAIT1:
		return L"FIN wait 1";
	case MIB_TCP_STATE_FIN_WAIT2:
		return L"FIN wait 2";
	case MIB_TCP_STATE_CLOSE_WAIT:
		return L"Close wait";
	case MIB_TCP_STATE_CLOSING:
		return L"Closing";
	case MIB_TCP_STATE_LAST_ACK:
		return L"Last ACK";
	case MIB_TCP_STATE_TIME_WAIT:
		return L"Time wait";
	case MIB_TCP_STATE_DELETE_TCB:
		return L"Delete TCB";
	}	
	return L"";
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
						LogErr2(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
					}
					else cpuCounters.push_back(thisCounter);
				}
			}
			MFree(cpuInstanceNames);
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
					MPerfDiskData *data = (MPerfDiskData*)MAlloc(sizeof(MPerfDiskData));
					memset(data, 0, sizeof(MPerfDiskData));
					wcscpy_s(data->performanceCounter_Name, pTemp);

					std::wstring cpuCounterName = FormatString(L"\\PhysicalDisk(%s)\\%% Disk Time", pTemp);
					data->performanceCounter_avgQue = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
					status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_avgQue);
					if (status != ERROR_SUCCESS)
					{
						GlobalFree(data->performanceCounter_avgQue);
						data->performanceCounter_avgQue = nullptr;
						LogErr2(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
					}

					cpuCounterName = FormatString(L"\\PhysicalDisk(%s)\\Disk Reads/sec", pTemp);
					data->performanceCounter_read = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
					status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_read);
					if (status != ERROR_SUCCESS)
					{
						GlobalFree(data->performanceCounter_avgQue);
						data->performanceCounter_avgQue = nullptr;
						LogErr2(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
					}

					cpuCounterName = FormatString(L"\\PhysicalDisk(%s)\\Disk Writes/sec", pTemp);
					data->performanceCounter_write = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
					status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_write);
					if (status != ERROR_SUCCESS)
					{
						GlobalFree(data->performanceCounter_write);
						data->performanceCounter_write = nullptr;
						LogErr2(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
					}

					cpuCounterName = FormatString(L"\\PhysicalDisk(%s)\\Disk Read Bytes/sec", pTemp);
					data->performanceCounter_readSpeed = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
					status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_readSpeed);
					if (status != ERROR_SUCCESS)
					{
						GlobalFree(data->performanceCounter_readSpeed);
						data->performanceCounter_readSpeed = nullptr;
						LogErr2(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
					}

					cpuCounterName = FormatString(L"\\PhysicalDisk(%s)\\Disk Write Bytes/sec", pTemp);
					data->performanceCounter_writeSpeed = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
					status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_writeSpeed);
					if (status != ERROR_SUCCESS)
					{
						GlobalFree(data->performanceCounter_writeSpeed);
						data->performanceCounter_writeSpeed = nullptr;
						LogErr2(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
					}

					diskCounters.push_back(data);
				}
			}

			return (UINT)diskCounters.size();
			MFree(diskInstanceNames);
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

			MFree(data);
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
			*out_readavgQue = pdh_counter_value.doubleValue;
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
		return pdh_counter_value.doubleValue;
	}
	return 0;
}

//Network Performance
std::vector<MPerfNetData*> netCounters;
std::vector<MPerfNetData*> netCounters2;

M_CAPI(UINT) MPERF_InitNetworksPerformanceCounters()
{
	if (hQuery)
	{
		PDH_STATUS status;
		DWORD netsInstanceNamesSize = 0;
		LPWSTR netInstanceNames = MPERF_EnumPerformanceCounterInstanceNames(L"Network Adapter");
		if (netInstanceNames)
		{
			for (LPWSTR pTemp = netInstanceNames; *pTemp != 0; pTemp += wcslen(pTemp) + 1)
			{
				MPerfNetData *data = (MPerfNetData*)MAlloc(sizeof(MPerfNetData));
				memset(data, 0, sizeof(MPerfNetData));
				wcscpy_s(data->performanceCounter_Name, pTemp);

				std::wstring cpuCounterName = FormatString(L"\\Network Interface(%s)\\Bytes Sent/sec", pTemp);
				data->performanceCounter_sent = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
				status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_sent);
				if (status != ERROR_SUCCESS)
				{
					GlobalFree(data->performanceCounter_sent);
					data->performanceCounter_sent = 0;
					LogErr2(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
				}

				cpuCounterName = FormatString(L"\\Network Interface(%s)\\Bytes Received/sec", pTemp);
				data->performanceCounter_receive = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
				status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_receive);
				if (status != ERROR_SUCCESS)
				{
					GlobalFree(data->performanceCounter_receive);
					data->performanceCounter_receive = 0;
					LogErr2(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
				}

				netCounters.push_back(data);
			}		
			MFree(netInstanceNames);
			return (UINT)netCounters.size();
		}
	}
	return FALSE;
}
M_CAPI(UINT) MPERF_InitNetworksPerformanceCounters2() {
	if (hQuery)
	{
		PDH_STATUS status;
		DWORD netsInstanceNamesSize = 0;
		LPWSTR netInstanceNames = MPERF_EnumPerformanceCounterInstanceNames(L"Network Interface");
		if (netInstanceNames)
		{
			for (LPWSTR pTemp = netInstanceNames; *pTemp != 0; pTemp += wcslen(pTemp) + 1)
			{
				MPerfNetData *data = (MPerfNetData*)MAlloc(sizeof(MPerfNetData));
				memset(data, 0, sizeof(MPerfNetData));
				wcscpy_s(data->performanceCounter_Name, pTemp);

				std::wstring cpuCounterName = FormatString(L"\\Network Interface(%s)\\Bytes Sent/sec", pTemp);
				data->performanceCounter_sent = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
				status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_sent);
				if (status != ERROR_SUCCESS)
				{
					GlobalFree(data->performanceCounter_sent);
					data->performanceCounter_sent = 0;
					LogErr2(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
				}

				cpuCounterName = FormatString(L"\\Network Interface(%s)\\Bytes Received/sec", pTemp);
				data->performanceCounter_receive = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
				status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_receive);
				if (status != ERROR_SUCCESS)
				{
					GlobalFree(data->performanceCounter_receive);
					data->performanceCounter_receive = 0;
					LogErr2(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
				}

				netCounters2.push_back(data);
			}
			MFree(netInstanceNames);
			return (UINT)netCounters2.size();
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

			MFree(data);
		}
		for (auto it = netCounters2.begin(); it != netCounters2.end(); it++)
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

			MFree(data);
		}
	}
	return FALSE;
}
M_CAPI(MPerfNetData*) MPERF_GetNetworksPerformanceCounters(int index)
{
	if (index >= 0 && (UINT)index < netCounters2.size())
		return netCounters2[index];
	return 0;
}
M_CAPI(MPerfNetData*) MPERF_GetNetworksPerformanceCounterWithName(LPWSTR name) 
{
	if (hQuery)
	{
		for (auto it = netCounters.begin(); it != netCounters.end(); it++)
		{
			MPerfNetData *data1 = (*it);
			if (StrEqual(data1->performanceCounter_Name, name))
			{
				return data1;
			}
		}

		MPerfNetData *data = NULL;
		data = (MPerfNetData*)MAlloc(sizeof(MPerfNetData));
		memset(data, 0, sizeof(MPerfNetData));
		wcscpy_s(data->performanceCounter_Name, name);

		std::wstring cpuCounterName = FormatString(L"\\Network Interface(%s)\\Bytes Sent/sec", name);
		data->performanceCounter_sent = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
		PDH_STATUS status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_sent);
		if (status != ERROR_SUCCESS)
		{
			GlobalFree(data->performanceCounter_sent);
			data->performanceCounter_sent = 0;
			LogErr2(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
		}

		cpuCounterName = FormatString(L"\\Network Interface(%s)\\Bytes Received/sec", name);
		data->performanceCounter_receive = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
		status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_receive);
		if (status != ERROR_SUCCESS)
		{
			GlobalFree(data->performanceCounter_receive);
			data->performanceCounter_receive = 0;
			LogErr2(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
		}

		netCounters.push_back(data);
		return data;
	}
	return FALSE;
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
		return TRUE;
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

//Gpu

M_CAPI(BOOL) MPERF_InitGpuPerformanceCounters() {
	return FALSE;
}
M_CAPI(BOOL) MPERF_DestroyGpuPerformanceCounters() {
	return FALSE;
}

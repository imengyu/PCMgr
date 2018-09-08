#include "stdafx.h"
#include "perfhlp.h"
#include "mapphlp.h"
#include "sysfuns.h"
#include "msup.h"
#include "loghlp.h"
#include "prochlp.h"
#include "StringHlp.h"
#include <Psapi.h>
#include <winsock2.h>
#include <Ws2tcpip.h>
#include <iphlpapi.h>
#include <Tcpestats.h>
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
	return (pdhstatus == ERROR_SUCCESS);
}
M_CAPI(VOID) MPERF_GlobalDestroy() {
	PdhCloseQuery(hQuery);
}
M_CAPI(BOOL) MPERF_GlobalUpdatePerformanceCounters()
{
	return !PdhCollectQueryData(hQuery);
}
M_CAPI(BOOL) MPERF_GlobalUpdateCpu() {
	return MSystemPerformanctMonitor::UpdateCpuGlobal();
}

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
		else LogErr(L"Second PdhEnumObjectItems \"Network Interface\" failed : 0x%X (Counter name : %s)", status, counterName);
		MFree(pwsInstanceListBuffer);
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
			counter3cpuuser = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));

			//添加性能计数器
			PDH_STATUS status = ERROR_SUCCESS;
			status = PdhAddCounter(hQuery, L"\\Processor Information(_Total)\\% Idle Time", 0, counter3cpu);
			if (status != ERROR_SUCCESS) {
				GlobalFree(counter3cpu);
				counter3cpu = NULL;
				LogErr(L"Add Performance Counter \"Processor Information(_Total)\\% Processor Time\" failed : 0x%X", status);
			}

			status = PdhAddCounter(hQuery, L"\\Processor Information(_Total)\\% User Time", 0, counter3cpuuser);
			if (status != ERROR_SUCCESS) {
				GlobalFree(counter3cpuuser);
				counter3cpuuser = NULL;
				LogErr(L"Add Performance Counter \"Processor Information(_Total)\\% User Time\" failed : 0x%X", status);
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

//Process Net Work Performance

PMIB_TCPTABLE_OWNER_PID netProcess = NULL;
PMIB_TCP6TABLE_OWNER_PID net6Process = NULL;

extern _GetPerTcp6ConnectionEStats dGetPerTcp6ConnectionEStats;
extern _GetPerTcpConnectionEStats dGetPerTcpConnectionEStats;
extern _GetExtendedTcpTable dGetExtendedTcpTable;
extern _SetPerTcpConnectionEStats dSetPerTcpConnectionEStats;

M_CAPI(BOOL) MPERF_GetConnectNetWorkAllBuffer(DWORD dwLocalAddr, DWORD dwLocalPort, DWORD dwRemoteAddr, DWORD dwRemotePort, DWORD dwState, MPerfAndProcessData*data)
{
	bool setConnectioned = false;

	MIB_TCPROW row;
	row.dwLocalAddr = dwLocalAddr;
	row.dwLocalPort = dwLocalPort;
	row.dwRemoteAddr = dwRemoteAddr;
	row.dwRemotePort = dwRemotePort;
	row.dwState = dwState;

	TCP_ESTATS_BANDWIDTH_RW_v0 rw;
	TCP_ESTATS_BANDWIDTH_ROD_v0 rod;
	memset(&rod, 0, sizeof(rod));
	memset(&rw, 0, sizeof(rw));

REGET:
	ULONG ret = dGetPerTcpConnectionEStats(&row, TcpConnectionEstatsBandwidth, (PUCHAR)&rw, 0, sizeof(rw), 0, 0, 0, (PUCHAR)&rod, 0, sizeof(rod));
	if (ret == NO_ERROR)
	{
		if ((!rw.EnableCollectionInbound || !rw.EnableCollectionOutbound) && !setConnectioned)
		{
			rw.EnableCollectionInbound = TcpBoolOptEnabled;
			rw.EnableCollectionOutbound = TcpBoolOptEnabled;

			dSetPerTcpConnectionEStats(&row, TcpConnectionEstatsBandwidth, (PUCHAR)&row, 0, sizeof(row), 0);
			setConnectioned = true;
			goto REGET;
		}

		data->InBandwidth = rod.InboundBandwidth;
		data->OutBandwidth = rod.OutboundBandwidth;

		data->ConnectCount++;

		return TRUE;
	}
	return 0;
}
M_CAPI(BOOL) MPERF_GetConnectNetWorkAllBuffer6(IN6_ADDR LocalAddr, DWORD dwLocalPort, IN6_ADDR RemoteAddr, DWORD dwRemotePort, MIB_TCP_STATE State, MPerfAndProcessData*data)
{
	MIB_TCP6ROW row;
	memset(&row, 0, sizeof(row));

	row.LocalAddr = LocalAddr;
	row.dwLocalPort = dwLocalPort;
	row.RemoteAddr = RemoteAddr;
	row.dwRemotePort = dwRemotePort;
	row.State = State;

	PTCP_ESTATS_BANDWIDTH_ROD_v0 rod = (PTCP_ESTATS_BANDWIDTH_ROD_v0)malloc(sizeof(TCP_ESTATS_BANDWIDTH_ROD_v0));
	ULONG ret = dGetPerTcp6ConnectionEStats(&row, TcpConnectionEstatsBandwidth, NULL, 0, 0, NULL, 0, 0, (LPBYTE)rod, 0, sizeof(TCP_ESTATS_BANDWIDTH_ROD_v0));
	if (ret == NO_ERROR)
	{
		data->InBandwidth6 = rod->InboundBandwidth;
		data->OutBandwidth6 = rod->OutboundBandwidth;

		data->ConnectCount++;

		free(rod);
		return TRUE;
	}
	else free(rod);
	return 0;
}

M_CAPI(ULONG64) MPERF_GetProcessNetworkSpeed(PMPROCESS_ITEM p)
{
	if (!p)	return 0;

	MPerfAndProcessData*data = p->PerfData;
	if (!data) return 0;

	data->ConnectCount = 0;

	ULONG64 data1 = 0;
	if (netProcess)
	{
		for (UINT i = 0; i < netProcess->dwNumEntries; i++)
		{
			if (netProcess->table[i].dwOwningPid == p->ProcessId)
			{
				MPERF_GetConnectNetWorkAllBuffer(netProcess->table[i].dwLocalAddr, netProcess->table[i].dwLocalPort,
					netProcess->table[i].dwRemoteAddr, netProcess->table[i].dwRemotePort, netProcess->table[i].dwState, data);
			}
		}
		data1 = ((data->InBandwidth) * 8 + (data->OutBandwidth) * 8);
	}
	ULONG64 data2 = 0;
	/*
	if (net6Process)
	{
		data->NetWorkInBandWidth = 0;
		data->NetWorkOutBandWidth = 0;
		for (UINT i = 0; i < net6Process->dwNumEntries; i++)
		{
			if (net6Process->table[i].dwOwningPid == pid)
			{
				IN6_ADDR remoteaddr = { 0 };
				IN6_ADDR localaddr = { 0 };
				memcpy_s(localaddr.u.Byte, 16, net6Process->table[i].ucLocalAddr, 16);
				memcpy_s(remoteaddr.u.Byte, 16, net6Process->table[i].ucRemoteAddr, 16);

				MPERF_GetConnectNetWorkAllBuffer6(localaddr, net6Process->table[i].dwLocalPort,
					remoteaddr, net6Process->table[i].dwRemotePort, (MIB_TCP_STATE)net6Process->table[i].dwState, data);
			}
		}
		data2 = ((data->InBandwidth6) * 8 + (data->OutBandwidth6) * 8);
	}
	*/

	if (data->ConnectCount == 0) {
		data->LastBandwidth = 0;
		return 0;
	}

	ULONG64 datalast = data->LastBandwidth;
	ULONG64 datathis = (data1 + data2) / data->ConnectCount;

	data->LastBandwidth = datathis;

	if (datalast > datathis)
		return datalast - datathis;
	else return 0;
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
	if (net6Process)
	{
		free(net6Process);
		net6Process = NULL;
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

		netProcess = (PMIB_TCPTABLE_OWNER_PID)malloc(dwSize);
		memset(netProcess, 0, sizeof(dwSize));

		if (dGetExtendedTcpTable(netProcess, &dwSize, TRUE, AF_INET, TCP_TABLE_OWNER_PID_CONNECTIONS, 0) != NO_ERROR)
		{
			MPERF_NET_FreeAllProcessNetInfo();
			return FALSE;
		}
	}

	/*
	dwSize = sizeof(MIB_TCP6TABLE_OWNER_PID);
	net6Process = (PMIB_TCP6TABLE_OWNER_PID)malloc(sizeof(MIB_TCP6TABLE_OWNER_PID));
	memset(net6Process, 0, sizeof(dwSize));
	if (dGetExtendedTcpTable(net6Process, &dwSize, TRUE, AF_INET6, TCP_TABLE_OWNER_PID_CONNECTIONS, 0) == ERROR_INSUFFICIENT_BUFFER)
	{
		free(net6Process);
		DWORD realSize = sizeof(DWORD) + dwSize * sizeof(MIB_TCP6ROW_OWNER_PID);
		net6Process = (PMIB_TCP6TABLE_OWNER_PID)malloc(realSize);
		memset(net6Process, 0, sizeof(realSize));

		if (dGetExtendedTcpTable(net6Process, &dwSize, TRUE, AF_INET6, TCP_TABLE_OWNER_PID_CONNECTIONS, 0) != NO_ERROR)
		{
			MPERF_NET_FreeAllProcessNetInfo();
			return FALSE;
		}
	}
	*/

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
			LogErr(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
		}

		cpuCounterName = FormatString(L"\\Network Interface(%s)\\Bytes Received/sec", name);
		data->performanceCounter_receive = (PDH_HCOUNTER*)GlobalAlloc(GPTR, (sizeof(PDH_HCOUNTER)));
		status = PdhAddCounter(hQuery, cpuCounterName.c_str(), 0, data->performanceCounter_receive);
		if (status != ERROR_SUCCESS)
		{
			GlobalFree(data->performanceCounter_receive);
			data->performanceCounter_receive = 0;
			LogErr(L"Add Performance Counter \"%s\" failed : 0x%X", cpuCounterName.c_str(), status);
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




#include "stdafx.h"
#include "MConnectionMonitor.h"
#include "MSystemPerformanctMonitor.h"
#include <Mstcpip.h>
#include <Ip2string.h>

extern _GetPerTcp6ConnectionEStats dGetPerTcp6ConnectionEStats;
extern _GetPerTcpConnectionEStats dGetPerTcpConnectionEStats;
extern _GetExtendedTcpTable dGetExtendedTcpTable;
extern _SetPerTcpConnectionEStats dSetPerTcpConnectionEStats;
extern _SetPerTcp6ConnectionEStats dSetPerTcp6ConnectionEStats;
extern _GetNameInfoW dGetNameInfoW;
extern _gethostbyaddr dgethostbyaddr;
extern _RtlIpv6AddressToStringW dRtlIpv6AddressToStringW;

MConnectionMonitor::MConnectionMonitor()
{
}
MConnectionMonitor::~MConnectionMonitor()
{
	FreeOld();
}

bool MConnectionMonitor::GetConnectNetWorkAllBuffer(DWORD dwLocalAddr, DWORD dwLocalPort, DWORD dwRemoteAddr, DWORD dwRemotePort, DWORD dwState, MPerfAndProcessData * data)
{
	bool setConnectioned = false;

	MIB_TCPROW row;
	row.dwLocalAddr = dwLocalAddr;
	row.dwLocalPort = dwLocalPort;
	row.dwRemoteAddr = dwRemoteAddr;
	row.dwRemotePort = dwRemotePort;
	row.dwState = dwState;

	TCP_ESTATS_DATA_RW_v0 rw;
	TCP_ESTATS_DATA_ROD_v0 rod;
	memset(&rod, 0, sizeof(rod));
	memset(&rw, 0, sizeof(rw));

	ULONG ret = dGetPerTcpConnectionEStats(&row, TcpConnectionEstatsData, (PUCHAR)&rw, 0, sizeof(rw), 0, 0, 0, (PUCHAR)&rod, 0, sizeof(rod));
	if (ret == NO_ERROR) {
		if (rw.EnableCollection) {
			data->InBytes += rod.DataBytesIn;
			data->OutBytes += rod.DataBytesOut;
			data->ConnectCount++;
		}
		else if (!setConnectioned) {
			rw.EnableCollection = TRUE;
			dSetPerTcpConnectionEStats(&row, TcpConnectionEstatsData, (PUCHAR)&rw, 0, sizeof(rw), 0);
			ret = dGetPerTcpConnectionEStats(&row, TcpConnectionEstatsData, (PUCHAR)&rw, 0, sizeof(rw), 0, 0, 0, (PUCHAR)&rod, 0, sizeof(rod));
			if (ret == NO_ERROR) {
				if (rw.EnableCollection) {
					data->InBytes += rod.DataBytesIn;
					data->OutBytes += rod.DataBytesOut;
					data->ConnectCount++;
				}
			}
		}
		return TRUE;
	}
	return false;
}
bool MConnectionMonitor::GetConnectNetWorkAllBuffer6(IN6_ADDR LocalAddr, DWORD dwLocalPort, IN6_ADDR RemoteAddr, DWORD dwRemotePort, MIB_TCP_STATE State, MPerfAndProcessData * data)
{
	bool setConnectioned = false;

	MIB_TCP6ROW row;
	memset(&row, 0, sizeof(row));

	row.LocalAddr = LocalAddr;
	row.dwLocalPort = dwLocalPort;
	row.RemoteAddr = RemoteAddr;
	row.dwRemotePort = dwRemotePort;
	row.State = State;

	TCP_ESTATS_DATA_RW_v0 rw;
	TCP_ESTATS_DATA_ROD_v0 rod;
	memset(&rod, 0, sizeof(rod));
	memset(&rw, 0, sizeof(rw));

	ULONG ret = dGetPerTcp6ConnectionEStats(&row, TcpConnectionEstatsData, (PUCHAR)&rw, 0, sizeof(rw), NULL, 0, 0, (LPBYTE)&rod, 0, sizeof(rod));
	if (ret == NO_ERROR) {
		if (rw.EnableCollection) {
			data->InBytes += rod.DataBytesIn;
			data->OutBytes += rod.DataBytesOut;
			data->ConnectCount++;
		}
		else if (!setConnectioned) {
			rw.EnableCollection = TRUE;
			dSetPerTcp6ConnectionEStats(&row, TcpConnectionEstatsData, (PUCHAR)&rw, 0, sizeof(rw), 0);
			ret = dGetPerTcp6ConnectionEStats(&row, TcpConnectionEstatsData, (PUCHAR)&rw, 0, sizeof(rw), NULL, 0, 0, (LPBYTE)&rod, 0, sizeof(rod));
			if (ret == NO_ERROR) {
				if (rw.EnableCollection) {
					data->InBytes += rod.DataBytesIn;
					data->OutBytes += rod.DataBytesOut;
					data->ConnectCount++;
				}
			}
		}
		return true;
	}
	return false;
}

bool MConnectionMonitor::Update()
{
	FreeOld();

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
			FreeOld();
			LogErr2(L"GetExtendedTcpTable failed : %d", GetLastError());
			return false;
		}
	}

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
			FreeOld();
			LogErr2(L"GetExtendedTcpTable failed : %d", GetLastError());
			return false;
		}
	}


	return false;
}
bool MConnectionMonitor::UpdateListData(MCONNECTION_ENUM_CALLBACK cp)
{
	if (cp) {
		WCHAR LocalAddr[64];
		WCHAR RemoteAddr[64];
		if (netProcess)
		{
			PMIB_TCPROW_OWNER_PID tcp;
			for (UINT i = 0; i < netProcess->dwNumEntries; i++)
			{
				tcp = &netProcess->table[i];

				WORD add1, add2, add3, add4;
				add1 = (WORD)(tcp->dwLocalAddr & 255);
				add2 = (WORD)((tcp->dwLocalAddr >> 8) & 255);
				add3 = (WORD)((tcp->dwLocalAddr >> 16) & 255);
				add4 = (WORD)((tcp->dwLocalAddr >> 24) & 255);
				swprintf_s(LocalAddr, L"%d.%d.%d.%d", add1, add2, add3, add4);
				add1 = (WORD)(tcp->dwRemoteAddr & 255);
				add2 = (WORD)((tcp->dwRemoteAddr >> 8) & 255);
				add3 = (WORD)((tcp->dwRemoteAddr >> 16) & 255);
				add4 = (WORD)((tcp->dwRemoteAddr >> 24) & 255);
				swprintf_s(RemoteAddr, L"%d.%d.%d.%d", add1, add2, add3, add4);

				cp(netProcess->table[i].dwOwningPid, M_CONNECTION_TYPE_TCP, LocalAddr, tcp->dwLocalPort, RemoteAddr, tcp->dwRemotePort, netProcess->table[i].dwState);
			}
		}
		if (net6Process)
		{
			PMIB_TCP6ROW_OWNER_PID tcp6;
			for (UINT i = 0; i < net6Process->dwNumEntries; i++)
			{
				tcp6 = &net6Process->table[i];
				in6_addr addr6;
				memcpy_s(&addr6.u, sizeof(addr6.u), tcp6->ucLocalAddr, sizeof(tcp6->ucLocalAddr));
				dRtlIpv6AddressToStringW(&addr6, LocalAddr);
				memcpy_s(&addr6.u, sizeof(addr6.u), tcp6->ucRemoteAddr, sizeof(tcp6->ucRemoteAddr));
				dRtlIpv6AddressToStringW(&addr6, RemoteAddr);
				cp(netProcess->table[i].dwOwningPid, M_CONNECTION_TYPE_TCP, LocalAddr, tcp6->dwLocalPort, RemoteAddr, tcp6->dwRemotePort, netProcess->table[i].dwState);
			}
		}
		return true;
	}
	return false;
}

bool MConnectionMonitor::IsProcessHasConnection(DWORD pid)
{
	bool rs = false;
	if (netProcess)
	{
		for (UINT i = 0; i < netProcess->dwNumEntries; i++)
		{
			if (netProcess->table[i].dwOwningPid == pid)
			{
				rs = true;
				break;
			}
		}
	}
	if (!rs && net6Process)
	{
		for (UINT i = 0; i < net6Process->dwNumEntries; i++)
		{
			if (net6Process->table[i].dwOwningPid == pid)
			{
				rs = true;
				break;
			}
		}
	}
	return rs;
}
ULONG64 MConnectionMonitor::GetProcessConnectSpeed(PMPROCESS_ITEM p)
{
	if (!p)	return 0;

	MPerfAndProcessData*data = p->PerfData;
	if (!data) return 0;

	data->ConnectCount = 0;
	data->InBytes = 0;
	data->OutBytes = 0;

	if (netProcess)
	{
		for (UINT i = 0; i < netProcess->dwNumEntries; i++)
		{
			if (netProcess->table[i].dwOwningPid == p->ProcessId && (netProcess->table[i].dwState == MIB_TCP_STATE_ESTAB))
			{
				MConnectionMonitor::GetConnectNetWorkAllBuffer(netProcess->table[i].dwLocalAddr, netProcess->table[i].dwLocalPort,
					netProcess->table[i].dwRemoteAddr, netProcess->table[i].dwRemotePort, netProcess->table[i].dwState, data);
			}
		}
	}
	if (net6Process)
	{
		for (UINT i = 0; i < net6Process->dwNumEntries; i++)
		{
			if (net6Process->table[i].dwOwningPid == p->ProcessId && (net6Process->table[i].dwState == MIB_TCP_STATE_ESTAB))
			{
				IN6_ADDR remoteaddr = { 0 };
				IN6_ADDR localaddr = { 0 };
				memcpy_s(localaddr.u.Byte, 16, net6Process->table[i].ucLocalAddr, 16);
				memcpy_s(remoteaddr.u.Byte, 16, net6Process->table[i].ucRemoteAddr, 16);

				MConnectionMonitor::GetConnectNetWorkAllBuffer6(localaddr, net6Process->table[i].dwLocalPort,
					remoteaddr, net6Process->table[i].dwRemotePort, (MIB_TCP_STATE)net6Process->table[i].dwState, data);
			}
		}
	}

	if (data->ConnectCount == 0)
		return 0;

	ULONG64 datalast = data->LastData;
	ULONG64 datathis = ((data->InBytes) * 8 + (data->OutBytes) * 8);
	DWORD interval = static_cast<DWORD>(MSystemPerformanctMonitor::UpdateTimeInterval / 10000000);

	data->LastData = datathis;

	if (interval <= 0)
		interval = 1;
	if (datathis > datalast)
		return (datathis - datalast) / interval;
	else
		return 0;
}

void MConnectionMonitor::FreeOld()
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

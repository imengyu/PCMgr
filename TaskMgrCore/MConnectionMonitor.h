#pragma once
#include "stdafx.h"
#include "MMonitor.h"
#include "mapphlp.h"
#include "sysfuns.h"
#include "loghlp.h"
#include "prochlp.h"
#include "StringHlp.h"
#include <Psapi.h>
#include <winsock2.h>
#include <Ws2tcpip.h>
#include <iphlpapi.h>
#include <Tcpestats.h>

#define M_CONNECTION_TYPE_TCP 4
#define M_CONNECTION_TYPE_TCP6 6
#define M_CONNECTION_TYPE_UDP 2
#define M_CONNECTION_TYPE_UDP6 5

typedef BOOL(*MCONNECTION_ENUM_CALLBACK)(DWORD ProcessId, DWORD Protcol, WCHAR *LocalAddr, DWORD LocalPort, WCHAR *RemoteAddr, DWORD RemotePort, DWORD state);

class MConnectionMonitor : MMonitor
{
public:
	MConnectionMonitor();
	~MConnectionMonitor();

	static bool GetConnectNetWorkAllBuffer(DWORD dwLocalAddr, DWORD dwLocalPort, DWORD dwRemoteAddr, DWORD dwRemotePort, DWORD dwState, MPerfAndProcessData*data);
	static bool GetConnectNetWorkAllBuffer6(IN6_ADDR LocalAddr, DWORD dwLocalPort, IN6_ADDR RemoteAddr, DWORD dwRemotePort, MIB_TCP_STATE State, MPerfAndProcessData*data);

	bool Update();
	bool UpdateListData(MCONNECTION_ENUM_CALLBACK cp);

	bool IsProcessHasConnection(DWORD pid);
	ULONG64 GetProcessConnectSpeed(PMPROCESS_ITEM p);

private:
	PMIB_TCPTABLE_OWNER_PID netProcess = NULL;
	PMIB_TCP6TABLE_OWNER_PID net6Process = NULL;


	void FreeOld();
};
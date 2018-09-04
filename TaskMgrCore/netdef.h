#pragma once
#include "stdafx.h"

typedef struct _MIB_TCP6ROW {
	MIB_TCP_STATE State;
	IN6_ADDR LocalAddr;
	DWORD dwLocalScopeId;
	DWORD dwLocalPort;
	IN6_ADDR RemoteAddr;
	DWORD dwRemoteScopeId;
	DWORD dwRemotePort;
} MIB_TCP6ROW, *PMIB_TCP6ROW;

typedef struct _MIB_TCP6ROW_OWNER_PID
{
	UCHAR           ucLocalAddr[16];
	DWORD           dwLocalScopeId;
	DWORD           dwLocalPort;
	UCHAR           ucRemoteAddr[16];
	DWORD           dwRemoteScopeId;
	DWORD           dwRemotePort;
	DWORD           dwState;
	DWORD           dwOwningPid;
} MIB_TCP6ROW_OWNER_PID, *PMIB_TCP6ROW_OWNER_PID;

typedef struct _MIB_TCP6TABLE_OWNER_PID
{
	DWORD                   dwNumEntries;
	_Field_size_(dwNumEntries)
		MIB_TCP6ROW_OWNER_PID   table[ANY_SIZE];
} MIB_TCP6TABLE_OWNER_PID, *PMIB_TCP6TABLE_OWNER_PID;
#pragma once
#include "stdafx.h"
#include "sysfuns.h"

#define USERNAME_LENGTH 20
#define DOMAIN_LENGTH 17

//枚举用户回调
typedef BOOL(__cdecl*EnumUsersCallBack)(
	LPWSTR userName, //用户名
	DWORD sessionId,//会话ID
	DWORD userId,//用户ID
	LPWSTR domain,//主机名
	LPVOID customData//自定义数据
	);

//枚举计算机上所有用户
M_CAPI(int) MEnumUsers(EnumUsersCallBack callBack, LPVOID customData);
void MUsersSetCurrentSelect(DWORD sessionId);
void MUsersSetCurrentSelectUserName(LPWSTR userName);

#pragma region WinSta

#define MAX_THINWIRECACHE 4
typedef struct _THINWIRECACHE
{
	ULONG CacheReads;
	ULONG CacheHits;
} THINWIRECACHE, *PTHINWIRECACHE;

typedef struct _RESERVED_CACHE
{
	THINWIRECACHE ThinWireCache[MAX_THINWIRECACHE];
} RESERVED_CACHE, *PRESERVED_CACHE;

typedef struct _TSHARE_CACHE
{
	ULONG Reserved;
} TSHARE_CACHE, *PTSHARE_CACHE;

typedef struct CACHE_STATISTICS
{
	USHORT ProtocolType;
	USHORT Length;
	union
	{
		RESERVED_CACHE ReservedCacheStats;
		TSHARE_CACHE TShareCacheStats;
		ULONG Reserved[20];
	} Specific;
} CACHE_STATISTICS, *PCACHE_STATISTICS;

typedef struct _TSHARE_COUNTERS
{
	ULONG Reserved;
} TSHARE_COUNTERS, *PTSHARE_COUNTERS;

typedef struct _PROTOCOLCOUNTERS
{
	ULONG WdBytes;
	ULONG WdFrames;
	ULONG WaitForOutBuf;
	ULONG Frames;
	ULONG Bytes;
	ULONG CompressedBytes;
	ULONG CompressFlushes;
	ULONG Errors;
	ULONG Timeouts;
	ULONG AsyncFramingError;
	ULONG AsyncOverrunError;
	ULONG AsyncOverflowError;
	ULONG AsyncParityError;
	ULONG TdErrors;
	USHORT ProtocolType;
	USHORT Length;
	union
	{
		TSHARE_COUNTERS TShareCounters;
		ULONG Reserved[100];
	} Specific;
} PROTOCOLCOUNTERS, *PPROTOCOLCOUNTERS;

typedef struct _PROTOCOLSTATUS
{
	PROTOCOLCOUNTERS Output;
	PROTOCOLCOUNTERS Input;
	CACHE_STATISTICS Cache;
	ULONG AsyncSignal;
	ULONG AsyncSignalMask;
} PROTOCOLSTATUS, *PPROTOCOLSTATUS;

// WinStationInformation
typedef struct _WINSTATIONINFORMATION
{
	WINSTATIONSTATECLASS ConnectState;
	WINSTATIONNAME WinStationName;
	ULONG LogonId;
	LARGE_INTEGER ConnectTime;
	LARGE_INTEGER DisconnectTime;
	LARGE_INTEGER LastInputTime;
	LARGE_INTEGER LogonTime;
	PROTOCOLSTATUS Status;
	WCHAR Domain[DOMAIN_LENGTH + 1];
	WCHAR UserName[USERNAME_LENGTH + 1];
	LARGE_INTEGER CurrentTime;
} WINSTATIONINFORMATION, *PWINSTATIONINFORMATION;

#pragma endregion

LRESULT MUsersHandleWmCommand(WPARAM wParam);




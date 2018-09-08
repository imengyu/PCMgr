#pragma once
#include "stdafx.h"
#include "uwphlp.h"
#include "DltHlp.h"
#include "ntdef.h"
#include <Pdh.h>
#include <PdhMsg.h>


//初始化PDH库
M_CAPI(BOOL) MPERF_GlobalInit();
//释放PDH库
M_CAPI(VOID) MPERF_GlobalDestroy();


//进程性能信息暂存结构
struct MPerfAndProcessData
{
	DWORD ProcessId;

	FLOAT CpuUserUsage;
	FLOAT CpuKernelUsage;

	MUINT64_DELTA CpuKernelDelta;
	MUINT64_DELTA CpuUserDelta;
	MUINT64_DELTA PageFaultsDelta;
	MUINT64_DELTA CycleTimeDelta; // since WIN7

	MUINT64_DELTA IoReadDelta;
	MUINT64_DELTA IoWriteDelta;
	MUINT64_DELTA IoOtherDelta;
	MUINT64_DELTA IoReadCountDelta;
	MUINT64_DELTA IoWriteCountDelta;
	MUINT64_DELTA IoOtherCountDelta;

	ULONG64 InBandwidth;
	ULONG64 OutBandwidth;
	ULONG64 InBandwidth6;
	ULONG64 OutBandwidth6;
	ULONG64 ConnectCount;

	ULONG64 LastBandwidth;

	PACKAGE_ID* packageId=NULL;
};

//释放所有进程的网络信息
M_CAPI(void) MPERF_NET_FreeAllProcessNetInfo();


//枚举性能计数器下所有的实例名称
//    counterName：需要枚举的性能计数器
//    返回：返回所有实例名称（以"\0\0"结尾，需要手动调用free释放）
M_CAPI(LPWSTR) MPERF_EnumPerformanceCounterInstanceNames(LPWSTR counterName);

//磁盘性能计数器结构
struct MPerfDiskData
{
	//Disk Reads/sec
	PDH_HCOUNTER*performanceCounter_read;
	//Disk Writes/sec
	PDH_HCOUNTER*performanceCounter_write;
	//Disk Read Bytes/sec
	PDH_HCOUNTER*performanceCounter_readSpeed;
	//Disk Write Bytes/sec
	PDH_HCOUNTER*performanceCounter_writeSpeed;
	//Avg. Disk Queue Length
	PDH_HCOUNTER*performanceCounter_avgQue;

	//Name
	WCHAR performanceCounter_Name[32];

};
//网络性能计数器结构
struct MPerfNetData
{
	//Bytes Sent/sec
	PDH_HCOUNTER*performanceCounter_sent;
	//Bytes Received/sec
	PDH_HCOUNTER*performanceCounter_receive;
	//Bytes Total/sec
	PDH_HCOUNTER*performanceCounter_total;

	//Name
	WCHAR performanceCounter_Name[64];
};





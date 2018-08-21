#pragma once
#include "stdafx.h"
#include "uwphlp.h"
#include <Pdh.h>
#include <PdhMsg.h>

//获取CPU核心数
M_CAPI(int) MPERF_GetProcessNumber();
//获取CPU使用率（0-1）
M_CAPI(double) MPERF_GetCupUseAge();
//刷新内存状态
M_CAPI(BOOL) MPERF_GetRamUseAge();
//获取内存使用率，在调用 MPERF_GetRamUseAge 后有效
M_CAPI(double) MPERF_GetRamUseAge2();

//初始化PDH库
M_CAPI(BOOL) MPERF_GlobalInit();
//释放PDH库
M_CAPI(VOID) MPERF_GlobalDestroy();
void MPERF_FreeCpuInfos();

//获取所有内存大小
M_CAPI(ULONGLONG) MPERF_GetAllRam();
//获取所有分页内存大小
M_CAPI(ULONGLONG) MPERF_GetPageSize();
//获取所有内核分页内存大小
M_CAPI(ULONGLONG) MPERF_GetKernelPaged();
//获取所有内核非分页内存大小
M_CAPI(ULONGLONG) MPERF_GetKernelNonpaged();
//获取所有内核缓存大小
M_CAPI(ULONGLONG) MPERF_GetSystemCacheSize();
//获取所有已提交大小
M_CAPI(ULONGLONG) MPERF_GetCommitTotal();
//获取所有已提交大上限
M_CAPI(ULONGLONG) MPERF_GetCommitLimit();
//获取可用内存
M_CAPI(ULONGLONG) MPERF_GetRamAvail();
//获取可用分页内存
M_CAPI(ULONGLONG) MPERF_GetRamAvailPageFile();
//刷新性能信息，刷新以后上面的函数才能用
M_CAPI(BOOL) MPERF_UpdatePerformance();

//进程性能信息结构
struct MPerfAndProcessData
{
	__int64 NowCpuTime;
	__int64 LastCpuTime;
	ULONGLONG LastRead;
	ULONGLONG LastWrite;
	ULONG64 NetWorkInBandWidth;
	ULONG64 NetWorkOutBandWidth;

	PACKAGE_ID* packageId=NULL;
};

//释放所有进程的网络信息
M_CAPI(void) MPERF_NET_FreeAllProcessNetInfo();

//获取CPU核心数
M_CAPI(int) MPERF_GetProcessNumber();
//获取CPU一级缓存
M_CAPI(DWORD) MPERF_GetCpuL1Cache();
//获取CPU二级缓存
M_CAPI(DWORD) MPERF_GetCpuL2Cache();
//获取CPU三级缓存
M_CAPI(DWORD) MPERF_GetCpuL3Cache();
//获取物理CPU数
M_CAPI(DWORD) MPERF_GetCpuPackage();
//获取CPU结点数
M_CAPI(DWORD) MPERF_GetCpuNodeCount();
//获取CPU名称
//    [OUT] buf：CPU名称字符串缓冲区
//    size：CPU名称字符串缓冲区字符个数
M_CAPI(BOOL) MPERF_GetCpuName(LPWSTR buf, int size);
//获取CPU主频
M_CAPI(int) MPERF_GetCpuFrequency();

//获取所有CPU信息，只有使用此方法以后 MPERF_GetCpuxxx 获取的信息才有效
M_CAPI(BOOL) MPERF_GetCpuInfos();

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




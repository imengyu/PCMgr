#pragma once
#include "stdafx.h"
#include "appmodel.h"
#include "appxpackaging.h"

//获取CPU核心数
M_CAPI(int) MPERF_GetProcessNumber();
//获取CPU使用率（0-1）
M_CAPI(double) MPERF_GetCupUseAge();
//刷新内存状态
M_CAPI(BOOL) MPERF_GetRamUseAge();
//获取内存使用率，在调用 MPERF_GetRamUseAge 后有效
M_CAPI(double) MPERF_GetRamUseAge2();

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
//获取所有已提交大小低
M_CAPI(ULONGLONG) MPERF_GetCommitPeak();
//获取可用内存
M_CAPI(ULONGLONG) MPERF_GetRamAvail();
//获取可用分页内存
M_CAPI(ULONGLONG) MPERF_GetRamAvailPageFile();

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





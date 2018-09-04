#pragma once
#include "stdafx.h"
#include "uwphlp.h"
#include "ntdef.h"
#include <Pdh.h>
#include <PdhMsg.h>

//内存工作集设置
#define M_GET_PROCMEM_WORKINGSET 0
//专用内存工作集
#define M_GET_PROCMEM_WORKINGSETPRIVATE 1
//共享内存工作集
#define M_GET_PROCMEM_WORKINGSETSHARE 2
//峰值内存工作集设置
#define M_GET_PROCMEM_PEAKWORKINGSET 3
//提交大小
#define M_GET_PROCMEM_COMMITEDSIZE 4
//非分页内存
#define M_GET_PROCMEM_NONPAGEDPOOL 5
//分页内存
#define M_GET_PROCMEM_PAGEDPOOL 6
//页面错误
#define M_GET_PROCMEM_PAGEDFAULT 7

//IO读取操作数
#define M_GET_PROCIO_READ 0
//IO写入操作数
#define M_GET_PROCIO_WRITE 1
//IO其他操作数
#define M_GET_PROCIO_OTHER 2
//IO读取字节
#define M_GET_PROCIO_READ_BYTES 3
//IO写入字节
#define M_GET_PROCIO_WRITE_BYTES 4
//IO其他字节
#define M_GET_PROCIO_OTHER_BYTES 5

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
//获取已用内存
M_CAPI(ULONGLONG) MPERF_GetRamUsed();
//获取可用分页内存
M_CAPI(ULONGLONG) MPERF_GetRamAvailPageFile();
//刷新性能信息，刷新以后上面的函数才能用
M_CAPI(BOOL) MPERF_UpdatePerformance();

//进程性能信息暂存结构
struct MPerfAndProcessData
{
	__int64 NowCpuTime;
	__int64 LastCpuTime;

	ULONGLONG LastRead;
	ULONGLONG LastWrite;

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

//创建进程性能数据暂存结构（计算每秒的使用率/速度的时候要用，不用的时候要释放）
M_CAPI(MPerfAndProcessData*) MPERF_PerfDataCreate();
//释放进程性能数据暂存结构
M_CAPI(void) MPERF_PerfDataDestroy(MPerfAndProcessData*data);
//刷新CPU时间间隔（计算cpu使用率用率时请每隔一段时间调用）
M_CAPI(void) MPERF_CpuTimeUpdate();
//获取进程CPU使用率
//    p：进程信息句柄
//    data：性能数据暂存结构
M_CAPI(double) MPERF_GetProcessCpuUseAge(PSYSTEM_PROCESSES p, MPerfAndProcessData*data);
//获取进程总的CPU时间
//    p：进程信息句柄
M_CAPI(ULONGLONG) MPERF_GetProcessCpuTime(PSYSTEM_PROCESSES p);
//获取进程周期
//    p：进程信息句柄
M_CAPI(ULONGLONG) MPERF_GetProcessCycle(PSYSTEM_PROCESSES p);
//获取进程内存专用工作集
//    p：进程信息句柄
//    hProcess：Reserved
M_CAPI(SIZE_T) MPERF_GetProcessRam(PSYSTEM_PROCESSES p, HANDLE hProcess);
//获取进程内存信息
//    p：进程信息句柄
//    col：信息类别（M_GET_PROCMEM_*）在上面有定义
M_CAPI(SIZE_T) MPERF_GetProcessMemoryInfo(PSYSTEM_PROCESSES p, int col);
//获取进程IO信息
//    p：进程信息句柄
//    col：信息类别（M_GET_PROCIO_*）在上面有定义
M_CAPI(ULONGLONG) MPERF_GetProcessIOInfo(PSYSTEM_PROCESSES p, int col);
//获取进程磁盘使用率
//    p：进程信息句柄
//    data：性能数据暂存结构
M_CAPI(DWORD) MPERF_GetProcessDiskRate(PSYSTEM_PROCESSES p, MPerfAndProcessData*data);




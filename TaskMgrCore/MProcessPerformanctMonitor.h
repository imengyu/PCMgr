#pragma once
#include "stdafx.h"
#include "perfhlp.h"
#include "prochlp.h"
#include <vector>


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


//进程性能监视器
class M_API MProcessPerformanctMonitor
{
	static SIZE_T GetProcessPrivateWoringSet(PMPROCESS_ITEM processItem);
	static SIZE_T GetProcessIOSpeed(PMPROCESS_ITEM processItem);
	static ULONG64 GetProcessNetworkSpeed(PMPROCESS_ITEM processItem);

	static double GetProcessCpuUseAgeKernel(PMPROCESS_ITEM processItem);
	static double GetProcessCpuUseAgeUser(PMPROCESS_ITEM processItem);
	static double GetProcessCpuUseAge(PMPROCESS_ITEM processItem);

	static ULONGLONG GetProcessCpuTime(PMPROCESS_ITEM processItem);
	static ULONGLONG GetProcessCycle(PMPROCESS_ITEM processItem);
	//获取进程内存信息
	//    p：进程信息句柄
	//    col：信息类别（M_GET_PROCMEM_*）在上面有定义
	static SIZE_T GetProcessMemoryInfo(PMPROCESS_ITEM processItem, int col);
	//获取进程IO信息
	//    p：进程信息句柄
	//    col：信息类别（M_GET_PROCIO_*）在上面有定义
	static ULONGLONG GetProcessIOInfo(PMPROCESS_ITEM processItem, int col);
};


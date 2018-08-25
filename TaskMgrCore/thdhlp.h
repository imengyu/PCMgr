#pragma once
#include "stdafx.h"
#include "ntdef.h"

//普通打开线程
//    dwId：thread id
//    [OUT] pLandle：输出线程句柄
//    dwPId：进程id，可以为 0
EXTERN_C M_API NTSTATUS MOpenThreadNt(DWORD dwId, PHANDLE pLandle, DWORD dwPId);
//普通结束线程
//    hThread：线程句柄
EXTERN_C M_API NTSTATUS MTerminateThreadNt(HANDLE hThread);
//普通继续线程运行
//    hThread：线程句柄
EXTERN_C M_API NTSTATUS MResumeThreadNt(HANDLE hThread);
//普通暂停线程运行
//    hThread：线程句柄
EXTERN_C M_API NTSTATUS MSuspendThreadNt(HANDLE hThread);
//获取线程 TEB 地址
//    hThread：线程句柄
EXTERN_C M_API PTEB MGetThreadPeb(HANDLE hThread);
//获取线程起始地址
//    hThread：线程句柄
EXTERN_C M_API PVOID MGetThreadWin32StartAddress(HANDLE hThread);
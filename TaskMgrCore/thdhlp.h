#pragma once
#include "stdafx.h"
#include "ntdef.h"

//普通打开线程
//    dwId：thread id
//    [OUT] pLandle：输出线程句柄
//    dwPId：进程id，可以为 0
EXTERN_C M_API NTSTATUS MOpenThreadNt(DWORD dwId, PHANDLE pLandle, DWORD dwPId);
//获取线程基本信息
//    ThreadHandle：线程句柄
//    BasicInformation：接收THREAD_BASIC_INFORMATION结构体变量
EXTERN_C M_API NTSTATUS MGetThreadBasicInformation(HANDLE ThreadHandle, PTHREAD_BASIC_INFORMATION BasicInformation);
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
//    [OUT] pPTeb：接收PTEB信息变量
EXTERN_C M_API NTSTATUS MGetThreadPeb(HANDLE hThread, PTEB*pPTeb);
//获取线程起始地址
//    hThread：线程句柄
//    [OUT] outStartAddress：接收变量
EXTERN_C M_API NTSTATUS MGetThreadWin32StartAddress(HANDLE hThread, PVOID * outStartAddress);
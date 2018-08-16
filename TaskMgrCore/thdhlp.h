#pragma once
#include "stdafx.h"
#include "ntdef.h"

//普通打开线程
//    dwId：thread id
//    [OUT] pLandle：输出线程句柄
//    dwPId：进程id，可以为 0
EXTERN_C M_API NTSTATUS MOpenThreadNt(DWORD dwId, PHANDLE pLandle, DWORD dwPId);
//普通结束线程
EXTERN_C M_API NTSTATUS MTerminateThreadNt(HANDLE handle);
//普通继续线程运行
EXTERN_C M_API NTSTATUS MResumeThreadNt(HANDLE handle);
//普通暂停线程运行
EXTERN_C M_API NTSTATUS MSuspendThreadNt(HANDLE handle);
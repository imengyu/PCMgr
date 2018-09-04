#pragma once
#include "stdafx.h"
#include "sysstructs.h"

DWORD WINAPI MDbgViewReceiveThread(LPVOID lpParameter);
DWORD WINAPI MLoadingThread(LPVOID lpParameter);

//MyDbgView关闭
M_CAPI(BOOL) MUnInitMyDbgView();
//启动 MyDbgView
M_CAPI(BOOL) MInitMyDbgView();
M_CAPI(BOOL) MMyDbgViewStarted();

//强制不启用 MyDbgView
M_CAPI(VOID) MDoNotStartMyDbgView();
//加载内核的pdb
M_CAPI(BOOL) MInitKernelNTPDB(BOOL usingNtosPDB, PKNTOSVALUE kNtosValue);
//加载内核的pdb释放资源
M_CAPI(BOOL) MUnInitKernelNTPDB();

VOID MLoadKernelNTPDB(PKNTOSVALUE kNtosValue, BOOL usingNtosPDB);

M_CAPI(BOOL) MIsKernelNeed64();

//获取PCMgr内核驱动是否可用
M_CAPI(BOOL) MCanUseKernel();
//加载PCMgr内核驱动
M_CAPI(BOOL) MInitKernel();
//卸载PCMgr内核驱动
M_CAPI(BOOL) MUninitKernel();
BOOL MShowMyDbgView();

//加载驱动
//    lpszDriverName：驱动的服务名
//    driverPath：驱动的完整路径
//    lpszDisplayName：nullptr
M_CAPI(BOOL) MLoadKernelDriver(LPWSTR lpszDriverName, LPWSTR driverPath, LPWSTR lpszDisplayName);
//卸载驱动
//    szSvrName：服务名
M_CAPI(BOOL) MUnLoadKernelDriver(LPWSTR szSvrName);

//获取ntoskrn.exe基地址（内核加载以后有效）
M_CAPI(ULONG_PTR) MGetNTBaseAddress();

M_CAPI(void) MInitKernelSwitchMenuState(BOOL loaded);
M_CAPI(BOOL) MInitKernelDriverHandleEndStep(HANDLE hHandle);

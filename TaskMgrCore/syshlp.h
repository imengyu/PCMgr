#pragma once
#include "stdafx.h"
#include "ntdef.h"
#include "sysfuns.h"

//显示”运行“对话框
M_CAPI(BOOL) MRunFileDlg(HWND hwndOwner, HICON hIcon, LPCWSTR lpszDirectory, LPCWSTR lpszTitle, LPCWSTR lpszDescription, ULONG uFlags);
//系统是否是64位
M_CAPI(BOOL) MIs64BitOS();
//进程提权
M_CAPI(BOOL) MGetPrivileges();
//获取进程是否以管理员身份运行
M_CAPI(BOOL) MIsRunasAdmin();

//根据导出函数名称或序号找出模块的导出函数
//    DllHandle：模块句柄
//    ProcedureName：函数名（如果为 NULL 则使用 ProcedureNumber 来查找）
//    ProcedureNumber：函数导出序号
M_CAPI(PVOID) MGetProcedureAddress(PVOID DllHandle, PSTR ProcedureName, ULONG ProcedureNumber);
//等于 GetProcAddress
M_CAPI(PVOID) MGetProcAddress(PVOID DllHandle, PSTR ProcedureName);

//命令行转为文件路径
//    cmdline：命令行
//    [OUT] buffer：输出文件路径缓冲区
//    size：输出文件路径缓冲区字符个数
M_CAPI(BOOL) MCommandLineToFilePath(LPWSTR cmdline, LPWSTR buffer, int size);

//获取系统 Bulid 版本（调用 MGetWindowsBulidVersion 以后有效）
M_CAPI(DWORD) MGetWindowsBulidVersionValue();

//获取系统是不是Win8以上（调用 MGetWindowsBulidVersion 以后有效）
M_CAPI(BOOL) MGetWindowsWin8Upper();

//获取 Windows Bulid Version
M_CAPI(BOOL) MGetWindowsBulidVersion();
//运行一个EXE
//    path：路径
//    args：参数
//    runAsadmin：是否已
M_CAPI(BOOL) MRunExe(LPWSTR path, LPWSTR args, BOOL runAsadmin, HWND hWnd);

//获取 Ntos And Win32 的基地址
M_CAPI(BOOL) MGetNtosAndWin32kfullNameAndStartAddress(LPWSTR name, size_t buffersize, ULONG_PTR *address, ULONG_PTR *win32kfulladdress);

//VK键值转为字符串
M_CAPI(LPWSTR) MKeyToStr(UINT vk);
//热键转为字符串
M_CAPI(BOOL) MHotKeyToStr(UINT fsModifiers, UINT vk, LPWSTR buffer, int size);


#pragma once
#include "stdafx.h"
#include "ntdef.h"
#include "sysfuns.h"

#define PTR_ADD_OFFSET(Pointer, Offset) ((PVOID)((ULONG_PTR)(Pointer) + (ULONG_PTR)(Offset)))
#define PTR_SUB_OFFSET(Pointer, Offset) ((PVOID)((ULONG_PTR)(Pointer) - (ULONG_PTR)(Offset)))

BOOL MGetDebuggerInformation();

//显示”运行“对话框（hwndOwner填自己的窗口句柄，其他全部填NULL）
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
//等于 GetProcAddress（自及实现的）
M_CAPI(PVOID) MGetProcAddress(PVOID DllHandle, PSTR ProcedureName);

//获取系统处理器标识
M_CAPI(BOOL) MGetSystemAffinityMask(PULONG_PTR SystemAffinityMask);

//命令行转为文件路径
//    cmdline：命令行
//    [OUT] buffer：输出文件路径缓冲区
//    size：输出文件路径缓冲区字符个数
M_CAPI(BOOL) MCommandLineToFilePath(LPWSTR cmdline, LPWSTR buffer, int size);
//命令行转为文件路径和参数
//    cmdline：命令行
//    [OUT] buffer：输出文件路径缓冲区
//    size：输出文件路径缓冲区字符个数
//    [OUT] argbuffer：输出参数缓冲区
//    argbuffersize：输出参数缓冲区字符个数
M_CAPI(BOOL) MCommandLineSplitPath(LPWSTR cmdline, LPWSTR buffer, int size, LPWSTR argbuffer, int argbuffersize);

//获取系统 Bulid 版本（调用 MGetWindowsBulidVersion 以后有效）
M_CAPI(DWORD) MGetWindowsBulidVersionValue();

//获取系统是不是Win8以上（调用 MGetWindowsBulidVersion 以后有效）
M_CAPI(BOOL) MGetWindowsWin8Upper();

//获取 Windows Bulid Version
M_CAPI(BOOL) MGetWindowsBulidVersion();
//运行一个EXE
//    path：路径
//    args：参数
//    runAsadmin：是否以管理员身份运行
//    hWnd：调用者窗口
M_CAPI(BOOL) MRunExe(LPWSTR path, LPWSTR args, BOOL runAsadmin, HWND hWnd);
//运行一个EXE
//    pathargs：路径（可带参数）
//    runAsadmin：是否以管理员身份运行
//    hWnd：调用者窗口
M_CAPI(BOOL) MRunExeWithAgrs(LPWSTR pathargs, BOOL runAsadmin, HWND hWnd);

//获取 Ntos And Win32 的基地址
M_CAPI(BOOL) MGetNtosAndWin32kfullNameAndStartAddress(LPWSTR name, size_t buffersize, ULONG_PTR *address, ULONG_PTR *win32kfulladdress);

//VK键值转为字符串
M_CAPI(LPWSTR) MKeyToStr(UINT vk);
//热键转为字符串
M_CAPI(BOOL) MHotKeyToStr(UINT fsModifiers, UINT vk, LPWSTR buffer, int size);

//复制字符串到剪贴板中
//    const WCHAR * pszData：需要复制的字符串
//    const size_t nDataLen ：需要复制的字符串字符个数（包括\0）
EXTERN_C M_API BOOL MCopyToClipboard(const WCHAR * pszData, const size_t nDataLen);
//复制字符串到剪贴板中
//    const WCHAR * pszData：需要复制的字符串
EXTERN_C M_API BOOL MCopyToClipboard2(const WCHAR * pszData);


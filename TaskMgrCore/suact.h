#pragma once
#include "stdafx.h"
#include "ntdef.h"
#include "schlp.h"
#include "sysstructs.h"

//暂存内核模块信息
typedef struct _KernelModulSmallInfo
{
	ULONG_PTR DriverObject;//驱动对象
	WCHAR szFullDllPathOrginal[MAX_PATH];
	WCHAR szFullDllPath[MAX_PATH];//完整路径
	WCHAR szServiceName[MAX_PATH];//服务名称
	LPSERVICE_STORAGE serviceInfo;//服务信息
}KernelModulSmallInfo,*PKernelModulSmallInfo;

//枚举内核模块信息的回调
typedef void(__cdecl*EnumKernelModulsCallBack)(
	PKernelModulSmallInfo kmi,//暂存内核模块信息
	LPWSTR szBaseDllName, //模块名称
	LPWSTR szFullDllPath,//完整路径
	LPWSTR szFullDllPathOrginal,//完整路径（未转换成Win32路径）
	LPWSTR szEntryPoint,//入口点
	LPWSTR szSizeOfImage,//映像大小
	LPWSTR szDriverObject,//驱动对象
	LPWSTR szBase,//基地址
	LPWSTR szServiceName,//服务名称
	ULONG Order//加载顺序
	);

//枚举进程热键回调
typedef void(__cdecl*EnumProcessHotKeyCallBack)(
	PHOT_KEY_DATA pHotKeyData, //热键信息，回调结束以后会被释放
	LPWSTR objectStr,//热键对象的字符串
	DWORD keyID, //热键ID
	LPWSTR keyStr,//热键键值
	DWORD pid,//进程ID
	DWORD tid,//线程ID
	LPWSTR procName//进程名字
	);

//枚举进程定时器回调
typedef void(__cdecl*EnumProcessTimerCallBack)(
	PTIMER_DATA pHotKeyData,//定时器信息，回调结束以后会被释放
	LPWSTR objectStr,//定时器对象的字符串
	LPWSTR funStr,//定时器对应函数的字符串
	LPWSTR moduleStr,//定时器对应模块的字符串
	LPWSTR hwndStr,//定时器对应窗口的字符串
	HWND hWnd,
	DWORD tid,//线程ID
	UINT_PTR nID,//定时器ID
	UINT interval,//定时器时间间隔
	DWORD pid//进程ID
	);

//内核反汇编回调
typedef void(__cdecl*DACALLBACK)(
	ULONG_PTR curaddress, /*当前地址*/
	LPWSTR addressstr, /*当前地址的字符串*/
	LPWSTR shellstr, /*反汇编之后的代码字符串1*/
	LPWSTR bariny, /*二进制字符串*/
	LPWSTR asmstr/*反汇编之后的代码字符串2*/
	);

M_CAPI(BOOL) M_SU_CreateFile(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, DWORD dwCreationDisposition, PHANDLE pHandle);
//内核打开进程
M_CAPI(BOOL) M_SU_OpenProcess(DWORD pid, PHANDLE pHandle, NTSTATUS* pStatus);
//内核打开线程
M_CAPI(BOOL) M_SU_OpenThread(DWORD pid, DWORD tid, PHANDLE pHandle, NTSTATUS* pStatus);

//强制结束进程
//    pid：process id
//    exitCode：退出代码（一般是0）
//    [OUT] pStatus：驱动返回的 NTSTATUS
//    useApc：是否使用插入apc结束进程的每一个线程
M_CAPI(BOOL) M_SU_TerminateProcessPID(DWORD pid, UINT exitCode, NTSTATUS* pStatus, BOOL useApc = FALSE);
//强制结束线程
//    tid：thread id
//    exitCode：退出代码（一般是0）
//    [OUT] pStatus：驱动返回的 NTSTATUS
//    useApc：是否使用插入apc结束线程
M_CAPI(BOOL) M_SU_TerminateThreadTID(DWORD tid, UINT exitCode, NTSTATUS* pStatus, BOOL useApc = FALSE);
//强制关闭进程内的一个句柄
M_CAPI(BOOL) M_SU_CloseHandleWithProcess(DWORD pid, LPVOID handleValue);
//内核挂起进程
//    pid：process id
//    [OUT] pStatus：驱动返回的 NTSTATUS
M_CAPI(BOOL) M_SU_SuspendProcess(DWORD pid, NTSTATUS * pStatus);
//内核取消挂起进程
//    pid：process id
//    [OUT] pStatus：驱动返回的 NTSTATUS
M_CAPI(BOOL) M_SU_ResumeProcess(DWORD pid, NTSTATUS * pStatus);

//获取EPROCESS结构信息
//    pid：process id
//    [OUT] lpEprocess：输出EPROCESS地址
//    [OUT] lpPeb：输出Peb地址
//    [OUT] lpJob：输出Job地址
//    [OUT] imagename：输出进程文件名[260]
//    [OUT] path：输出进程完整路径[260]，为未转换的NTPath
M_CAPI(BOOL) M_SU_GetEPROCESS(DWORD pid, ULONG_PTR* lpEprocess, ULONG_PTR* lpPeb, ULONG_PTR* lpJob, LPWSTR imagename, LPWSTR path); 
//获取ETHREAD结构信息
//    tid：thread id
//    [OUT] lpEthread：输出ETHREAD地址
//    [OUT] lpTeb：输出Teb地址
M_CAPI(BOOL) M_SU_GetETHREAD(DWORD tid, ULONG_PTR* lpEthread, ULONG_PTR * lpTeb);
//获取进程命令行参数 CommandLine
//    pid：process id
//    [OUT] outCmdLine：输出命令行字符串缓冲区（至少[1024]）
M_CAPI(BOOL) M_SU_GetProcessCommandLine(DWORD pid, LPWSTR outCmdLine);
//反汇编内核代码
//    callback：反汇编一行的回调
//    startaddress：需要反汇编的起始地址
//   size：需要反汇编的大小，以字节为单位
M_CAPI(BOOL) M_SU_KDA(DACALLBACK callback, ULONG_PTR startaddress, ULONG_PTR size);

//暂存内核模块信息结构体删除
//  在枚举内核模块信息回调中第一个参数的结构体使用完毕在此删除，否则会内存泄漏
M_CAPI(void) M_SU_EnumKernelModulsItemDestroy(KernelModulSmallInfo * km);
//枚举内核模块
//    callback：枚举内核模块信息的回调
//    showall：是否显示未加载的驱动
M_CAPI(BOOL) M_SU_EnumKernelModuls(EnumKernelModulsCallBack callback, BOOL showall = FALSE);

//Not For public。。。

BOOL M_SU_ForceShutdown();
BOOL M_SU_ForceReboot();

BOOL M_SU_ProtectMySelf();
BOOL M_SU_UnProtectMySelf();

//内核初始化函数
M_CAPI(BOOL) M_SU_Init(BOOL requestNtosValue, PKNTOSVALUE outValue);


M_CAPI(BOOL) M_SU_SetDbgViewEvent(HANDLE hEvent);
M_CAPI(BOOL) M_SU_ReSetDbgViewEvent();
M_CAPI(BOOL) M_SU_GetDbgViewLastBuffer(LPWSTR outbuffer, size_t bufsize, BOOL*hasMoreData);

M_CAPI(BOOL) M_SU_PrintInternalFuns();

//枚举进程热键
//    pid：process id
//    callBack：回调
M_CAPI(BOOL) M_SU_GetProcessHotKeys(DWORD pid, EnumProcessHotKeyCallBack callBack);
//枚举进程定时器
//    pid：process id
//    callBack：回调
M_CAPI(BOOL) M_SU_GetProcessTimers(DWORD pid, EnumProcessTimerCallBack callBack);

LRESULT M_SU_EnumKernelModuls_HandleWmCommand(WPARAM wParam);

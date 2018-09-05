#pragma once
#include "stdafx.h"
#include "perfhlp.h"
#include "ntdef.h"
#include "nthlp.h"
#include "handlehlp.h"

//枚举进程模块回调
//返回FALSE中停止枚举
typedef BOOL(__cdecl*EnumProcessModulesCallBack)(
	LPWSTR moduleName, //模块名称
	LPWSTR fullDllName, //模块完整路径
	PVOID baseAddress, //模块基地址（全局）
	ULONG sizeOfImage, //模块大小
	LPVOID customData//自定义数据
	);
//枚举进程窗口回调
//返回FALSE中停止枚举
typedef BOOL(__cdecl*EnumProcessWindowsCallBack)(
	HWND hWnd, //窗口句柄
	LPWSTR windowText, //窗口文字
	LPVOID customData//自定义数据
	);
//枚举进程线程回调
//返回FALSE中停止枚举
typedef BOOL(__cdecl*EnumProcessThreadsCallBack)(
	DWORD tid, //线程id
	DWORD pid, //进程id
	PVOID startAddress, //起始地址（非Win32StartAddress，如要获取Win32StartAddress请使用MGetThreadWin32StartAddress）
	PSYSTEM_THREADS t, //线程信息数据结构（回调结束以后会被释放）
	LPVOID customData//自定义数据
	);
//枚举进程回调
//返回FALSE中停止枚举
typedef BOOL(__cdecl*EnumProcessCallBack)(
	DWORD pid,//进程id
	DWORD parentid, //父进程id
	LPWSTR exename, //进程名称
	LPWSTR exefullpath,//进程完整路径
	int tp, //是否是最后一个模块，当枚举完成以后会再调用一次回调，tp=0，pid存放的是进程数量
	HANDLE hProcess, //当前进程句柄
	PSYSTEM_PROCESSES proc, //进程信息数据结构（下一次调用MEnumProcess/MEnumProcessCore之后会被释放）
	LPVOID customData//自定义数据
	);
//简单枚举进程回调
//返回FALSE中停止枚举
typedef void(__cdecl*EnumProcessCallBack2)(
	DWORD pid, //进程id
	PSYSTEM_PROCESSES proc//进程信息数据结构（下一次调用MEnumProcess/MEnumProcessCore之后会被释放）
	);

//枚举进程权限回调
//    name：权限名字
typedef void(__cdecl *EnumPrivilegesCallBack)(LPWSTR name);

//private

typedef struct tag_USERNAME {
	PSID Sid;
	WCHAR UserName[64];
	WCHAR DomainName[128];

}USERNAME,*PUSERNAME;
typedef struct tag_EPW_ARG {
	DWORD pid;
	EnumProcessWindowsCallBack c;
	LPVOID data;
}EPW_ARG,*PEPW_ARG;
typedef struct tag_PEOCESSKINFO {
	WCHAR Eprocess[32];
	WCHAR PebAddress[32];
	WCHAR JobAddress[32];
	WCHAR ImageFileName[MAX_PATH];
	WCHAR ImageFullName[MAX_PATH];
}PEOCESSKINFO,*PPEOCESSKINFO;
typedef struct tag_PROCHANDLE_STORAGE {
	DWORD pid;
	HANDLE hProcess;
}PROCHANDLE_STORAGE,*PPROCHANDLE_STORAGE;

//Not use
//private

VOID MAnitInjectLow();

//private

void MProcessHANDLEStorageDestroyItem(DWORD pid);
void MProcessHANDLEStorageDestroy();
BOOL LoadDll();
void FreeDll();
void MFroceKillProcessUser();
void MKillProcessUser(BOOL ask);
void MKillProcessTreeUser();
BOOL MKillProcessUser2(HWND hWnd, DWORD pid, BOOL showErr, BOOL ignoreTerminateing);
void MAppProcPropertyClassHandleWmCommand(WPARAM wParam);
BOOL MDetachFromDebuggerProcess(DWORD pid);

//进程提权
EXTERN_C M_API BOOL MGetPrivileges2();
//刷新进程列表
EXTERN_C M_API BOOL MEnumProcessCore();
//刷新进程列表的释放资源工作
EXTERN_C M_API void MEnumProcessFree();
//枚举进程（返回 进程信息信息）
//    callBack：回调
EXTERN_C M_API BOOL MEnumProcess(EnumProcessCallBack callBack, LPVOID customData);
//简单枚举进程（仅仅返回 PID，通常用于刷新）
//    callBack：简单回调
EXTERN_C M_API BOOL MEnumProcess2(EnumProcessCallBack2 callBack);
//从进程列表中找指定PID并调用 calBack
//（MEnumProcess/MEnumProcessCore之后使用）
EXTERN_C M_API BOOL MUpdateProcess(DWORD pid, EnumProcessCallBack calBack, LPVOID customData);
//在已枚举的进程列表中找指定PID的进程信息
//（MEnumProcess/MEnumProcessCore之后使用）
EXTERN_C M_API PSYSTEM_PROCESSES MFindProcessInLoadedProcesses(DWORD pid);
//在已枚举的进程列表中找指定PID的进程，枚举其线程
//（MEnumProcess/MEnumProcessCore之后使用）
EXTERN_C M_API BOOL MEnumProcessThreads(PSYSTEM_PROCESSES p, EnumProcessThreadsCallBack c, LPVOID customData);
//枚举进程所有窗口
//    dwPID：进程pid
//    callBack：回调
EXTERN_C M_API BOOL MEnumProcessWindow(DWORD dwPID, EnumProcessWindowsCallBack callback, LPVOID customData);
//枚举进程所有模块
//    dwPID：进程pid
//    callBack：回调
EXTERN_C M_API BOOL MEnumProcessModules(DWORD dwPID, EnumProcessModulesCallBack callback, LPVOID customData);
//枚举进程所有句柄
//    dwPID：进程pid
//    callBack：回调
EXTERN_C M_API BOOL MEnumProcessHandles(DWORD pid, EHCALLBACK callback);
//枚举进程所有权限
//    dwPID：进程pid
//    callBack：回调
M_CAPI(BOOL) MEnumProcessPrivileges(DWORD dwId, EnumPrivilegesCallBack callBack);

//把带符号链接的路径转为可访问的文件路径
//    pszNtPath：输入路径
//    [OUT] pszFilePath：输出文件路径缓冲区
//    bufferSize：输出文件路径缓冲区字符个数
EXTERN_C M_API BOOL MNtPathToFilePath(LPWSTR pszNtPath, LPWSTR pszFilePath, size_t bufferSize);
//把Dos路径转为内核路径
//    pszNtPath：输入Dos路径
//    [OUT] pszNtPath：输出Nt路径缓冲区
EXTERN_C M_API BOOL MDosPathToNtPath(LPWSTR pszDosPath, LPWSTR pszNtPath);
//把内核路径转为可访问的文件路径
//    pszNtPath：输入路径
//    [OUT] pszDosPath：输出文件路径缓冲区
//    szDosPathSize：输出文件路径缓冲区字符个数
EXTERN_C M_API DWORD MNtPathToDosPath(LPWSTR pszNtPath, LPWSTR pszDosPath, UINT szDosPathSize);
//从文件句柄获取内核路径
//    hFile：文件句柄
//    [OUT] pszNtPath：输出内核路径缓冲区
//    pszNtPathSize：输出内核路径缓冲区字符个数
EXTERN_C M_API DWORD MGetNtPathFromHandle(HANDLE hFile, LPWSTR pszNtPath, UINT pszNtPathSize);

//根据pid获取进程完整路径（R3）
//    dwPID：进程pid
//    [OUT] outNter：输出完整路径缓冲区，大小（MAX_PATH）
//    [OUT] phandle：输出进程句柄（此函数会自动打开进程句柄，无需后续打开，需关闭）
//    pszExeName：输入进程名称 (*.exe)
EXTERN_C M_API BOOL MGetProcessFullPathEx(DWORD dwPID, LPWSTR outNter, PHANDLE phandle, LPWSTR pszExeName);
//获取PE文件的版权信息
//    pszFullPath：完整路径
//    InfoItem：条目，以下对应不同信息
//        	CompanyName
//        	FileDescription
//        	FileVersion
//        	InternalName
//        	LegalCopyright
//        	OriginalFilename
//        	ProductName
//        	ProductVersion
//        	Comments
//        	LegalTrademarks
//        	PrivateBuild
//        	SpecialBuild
//    [OUT] str：输出字符串缓冲区
//    maxCount：字符串缓冲区大小
EXTERN_C M_API BOOL MGetExeInfo(LPWSTR strFilePath, LPWSTR InfoItem, LPWSTR str, int maxCount);
//获取PE文件的说明信息
//    pszFullPath：完整路径
//    [OUT] str：输出字符串缓冲区
//    maxCount：字符串缓冲区大小
EXTERN_C M_API BOOL MGetExeDescribe(LPWSTR pszFullPath, LPWSTR str, int maxCount);
//获取PE文件的发布者信息
//    pszFullPath：完整路径
//    [OUT] str：输出字符串缓冲区
//    maxCount：字符串缓冲区大小
EXTERN_C M_API BOOL MGetExeCompany(LPWSTR pszFullPath, LPWSTR str, int maxCount);
//获取EXE文件的图标
//    pszFullPath：完整路径
EXTERN_C M_API HICON MGetExeIcon(LPWSTR pszFullPath);
//检查一个程序文件是否签名（不考虑是否信任）
//    pszFullPath：完整路径
EXTERN_C M_API BOOL MGetExeFileTrust(LPCWSTR pszFullPath);
//检查一个程序文件签名状态
//[返回]
//  ERROR_SUCCESS：已签名
//  TRUST_E_NOSIGNATURE：没有签名
//  TRUST_E_SUBJECT_NOT_TRUSTED：有签名但是不受信任
//  TRUST_E_EXPLICIT_DISTRUST：有签名但是不被允许
//  CRYPT_E_SECURITY_SETTINGS：管理员已禁用安全选项
//  其他：未知错误
EXTERN_C M_API LONG MVerifyEmbeddedSignature(LPCWSTR pwszSourceFile);
//显示一个程序文件的证书信息对话框
EXTERN_C M_API BOOL MShowExeFileSignatureInfo(LPCWSTR pwszSourceFile);

//关闭句柄（不是强制）
EXTERN_C M_API BOOL MCloseHandle(HANDLE handle);
//普通暂停进程运行
//    dwPId：进程id，为 0 时使用 handle
//    handle：进程句柄，为 NULL 时使用 dwPId
EXTERN_C M_API NTSTATUS MSuspendProcessNt(DWORD dwPId, HANDLE handle);
//普通继续进程运行
//    dwPId：进程id，为 0 时使用 handle
//    handle：进程句柄，为 NULL 时使用 dwPId
EXTERN_C M_API NTSTATUS MResumeProcessNt(DWORD dwPId, HANDLE handle);
//普通打开进程
//    dwPId：进程id，为 0 时使用 handle
//    [OUT] pLhandle：返回进程句柄
EXTERN_C M_API NTSTATUS MOpenProcessNt(DWORD dwId, PHANDLE pLhandle);
//普通结束进程
//    dwPId：进程id，为 0 时使用 handle
//    handle：进程句柄，为 NULL 时使用 dwPId
EXTERN_C M_API NTSTATUS MTerminateProcessNt(DWORD dwId, HANDLE handle);

//运行UWP应用
//    packageName：完整包名
//    appName：要启动的包中的 App 名称
//  具体请Google ”How to start a uwp app“
EXTERN_C M_API BOOL MRunUWPApp(LPWSTR packageName, LPWSTR appName);

//获取进程命令行参数
//    handle：进程句柄，为 NULL 时则使用 pid
//    [OUT] buffer：输出字符串缓冲区
//    maxcount：输出字符串缓冲区字符个数
//    pid：进程id
EXTERN_C M_API BOOL MGetProcessCommandLine(HANDLE handle, LPWSTR buffer, int maxcount, DWORD pid = 0);
//获取进程是否是UWP应用
EXTERN_C M_API BOOL MGetProcessIsUWP(HANDLE handle);
//获取进程是否是32位
EXTERN_C M_API BOOL MGetProcessIs32Bit(HANDLE handle);
//获取进程的GDI对象个数
M_API DWORD MGetProcessGdiHandleCount(HANDLE handle);
//获取进程的用户对象个数
M_API DWORD MGetProcessUserHandleCount(HANDLE handle);
//获取进程内核EPROCESS信息
//    pid：进程id
//  对 M_SU_GetEPROCESS 的封装
EXTERN_C M_API BOOL MGetProcessEprocess(DWORD pid, PPEOCESSKINFO info);
//获取进程内存专用工作集
//    hProcess：进程句柄
//    pageSize：分页一页大小
EXTERN_C M_API ULONG_PTR MGetProcessWorkingSetPrivate(HANDLE hProcess, SIZE_T pageSize);
//获取进程会话ID
//    pid：进程id
EXTERN_C M_API DWORD MGetProcessSessionID(DWORD pid);
//获取进程用户名
//    hProcess：进程句柄
//    [OUT] buffer：输出字符串缓冲区
//    len：输出字符串缓冲区字符个数
EXTERN_C M_API BOOL MGetProcessUserName(HANDLE hProcess, LPWSTR buffer, int maxcount);
//PSID->PUSERNAME
EXTERN_C M_API PUSERNAME MGetUserNameBySID(PSID sid);
//获取进程线程数
EXTERN_C M_API ULONG MGetProcessThreadsCount(PSYSTEM_PROCESSES p);
//获取进程句柄数
EXTERN_C M_API ULONG MGetProcessHandlesCount(PSYSTEM_PROCESSES p);
//=K32GetMappedFileName
EXTERN_C M_API NTSTATUS MGetProcessMappedFileName(HANDLE ProcessHandle, PVOID BaseAddress, LPWSTR OutFileName, int BufferSize);
//获取进程PEB地址
//    hProcess：进程句柄
//    [OUT] pPpeb：接收PPEB变量
EXTERN_C M_API NTSTATUS MGetProcessPeb(HANDLE handle, PPEB* pPpeb);
//获取进程 Win32 路径
//    ProcessHandle：进程句柄
//    [OUT] FileNameBuffer：输出字符串缓冲区
//    FileNameBufferSize：输出字符串缓冲区字符个数
EXTERN_C M_API NTSTATUS MGetProcessImageFileNameWin32(HANDLE ProcessHandle, LPWSTR FileNameBuffer, ULONG FileNameBufferSize);
//获取进程路径
//    ProcessHandle：进程句柄
//    [OUT] FileNameBuffer：输出字符串缓冲区
//    FileNameBufferSize：输出字符串缓冲区字符个数
EXTERN_C M_API NTSTATUS MGetProcessImageFileName(HANDLE ProcessHandle, LPWSTR FileNameBuffer, ULONG FileNameBufferSize);
//获取进程PROCESS_BASIC_INFORMATION
//    ProcessHandle：进程句柄
//    [OUT] BasicInformation：接收PROCESS_BASIC_INFORMATION结构体变量
EXTERN_C M_API NTSTATUS MGetProcessBasicInformation(HANDLE ProcessHandle, PPROCESS_BASIC_INFORMATION BasicInformation);
//获取进程启动标志
//    ProcessHandle：进程句柄
//    [OUT] ExecuteFlags：接收ExecuteFlags信息变量
EXTERN_C M_API NTSTATUS MGetProcessExecuteFlags(HANDLE ProcessHandle, PULONG ExecuteFlags);
//获取进程IO优先级
//    ProcessHandle：进程句柄
//    [OUT] IoPriority：接收IO_PRIORITY_HINT信息结构体变量
EXTERN_C M_API NTSTATUS MGetProcessIoPriority(HANDLE ProcessHandle, IO_PRIORITY_HINT *IoPriority);
//获取进程 ExitStatus
//    ProcessHandle：进程句柄
//    [OUT] ExitStatus：接收信息变量
EXTERN_C M_API NTSTATUS MGetProcessExitStatus(HANDLE ProcessHandle, NTSTATUS *ExitStatus);
//获取进程 BasePriority
//    ProcessHandle：进程句柄
//    [OUT] BasePriority：接收信息变量
EXTERN_C M_API NTSTATUS MGetProcessBasePriority(HANDLE ProcessHandle, PULONG BasePriority);
//获取进程 AffinityMask
//    ProcessHandle：进程句柄
//    [OUT] AffinityMask：接收信息变量
EXTERN_C M_API NTSTATUS MGetProcessAffinityMask(HANDLE ProcessHandle, PULONG_PTR AffinityMask);

EXTERN_C M_API NTSTATUS MGetProcessDebugObject(HANDLE ProcessHandle, PHANDLE DebugObjectHandle);

NTSTATUS MSetProcessPriorityClass(HANDLE ProcessHandle, UCHAR PriorityClass);
NTSTATUS MSetProcessAffinityMask(HANDLE ProcessHandle, ULONG_PTR AffinityMask);

//获取UWP应用完整包名
//    handle：进程句柄
//    [OUT] len：输出字符串缓冲区字符个数
//    [OUT] buffer：输出字符串缓冲区
EXTERN_C M_API BOOL MGetUWPPackageFullName(HANDLE handle, int * len, LPWSTR buffer);
//获取进程状态
//[返回]
//  3：无响应/2：暂停/1：正在运行/0：未知
//    p：枚举所给的进程信息结构
//    hWnd：这个进程的主窗口
EXTERN_C M_API int MGetProcessState(PSYSTEM_PROCESSES p, HWND hWnd);
//获取SYSTEM_PROCESSES下的SYSTEM_THREADS
EXTERN_C M_API PSYSTEM_THREADS MGetProcessThreads(DWORD pid);
//获取根据pid在进程列表里获取 SYSTEM_PROCESSES 结构（需调用 MEnumProcessCore(); 刷新列表）
EXTERN_C M_API PSYSTEM_PROCESSES MGetProcessInfo(DWORD pid);

EXTERN_C M_API PPEB MGetCurrentPeb();
EXTERN_C M_API PTEB MGetCurrentTeb();













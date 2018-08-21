#pragma once
#include "stdafx.h"
#include "perfhlp.h"
#include "ntdef.h"

typedef void(__cdecl*EnumProcessCallBack)(DWORD pid, DWORD parentid, LPWSTR exename, LPWSTR exefullpath, int tp, HANDLE hProcess, PSYSTEM_PROCESSES proc);
typedef void(__cdecl*EnumProcessCallBack2)(DWORD pid, PSYSTEM_PROCESSES proc);

typedef struct tag_PEOCESSKINFO {
	WCHAR Eprocess[32];
	WCHAR PebAddress[32];
	WCHAR JobAddress[32];
	WCHAR ImageFileName[MAX_PATH];
	WCHAR ImageFullName[MAX_PATH];
}PEOCESSKINFO,*PPEOCESSKINFO;

BOOL LoadDll();
void FreeDll();
void MFroceKillProcessUser();
void MKillProcessUser(BOOL ask);
void MKillProcessTreeUser();

EXTERN_C M_API BOOL MKillProcessUser2(DWORD pid, BOOL showErr);

//进程提权
EXTERN_C M_API BOOL MGetPrivileges2();
//刷新进程列表
EXTERN_C M_API void MEnumProcessCore();
//刷新进程列表的释放资源工作
EXTERN_C M_API void MEnumProcessFree();
//枚举进程
//    callBack：回调
EXTERN_C M_API void MEnumProcess(EnumProcessCallBack callBack);
//简单枚举进程
//    callBack：简单回调
EXTERN_C M_API void MEnumProcess2Refesh(EnumProcessCallBack2 callBack);
EXTERN_C M_API BOOL MReUpdateProcess(DWORD pid, EnumProcessCallBack calBack);

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
//    handle：进程句柄，为 NULL 时使用 dwPId
EXTERN_C M_API NTSTATUS MOpenProcessNt(DWORD dwId, PHANDLE pLandle);
//普通结束进程
//    dwPId：进程id，为 0 时使用 handle
//    handle：进程句柄，为 NULL 时使用 dwPId
EXTERN_C M_API NTSTATUS MTerminateProcessNt(DWORD dwId, HANDLE handle);

//运行UWP应用
//    packageName：完整包名
//    name：包中的 App 名称
//  具体请Google ”How to start a uwp app“
EXTERN_C M_API BOOL MRunUWPApp(LPWSTR packageName, LPWSTR name);

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
//获取进程内核EPROCESS信息
//    pid：进程id
//  对 M_SU_GetEPROCESS 的封装
EXTERN_C M_API BOOL MGetProcessEprocess(DWORD pid, PPEOCESSKINFO info);
//获取进程内存专用工作集
//    hProcess：进程句柄
//    pageSize：分页一页大小
M_API ULONG_PTR MGetProcessWorkingSetPrivate(HANDLE hProcess, SIZE_T pageSize);
//获取UWP应用完整包名
//    handle：进程句柄
//    [OUT] len：输出字符串缓冲区字符个数
//    [OUT] buffer：输出字符串缓冲区
EXTERN_C M_API BOOL MGetUWPPackageFullName(HANDLE handle, int * len, LPWSTR buffer);
//获取进程状态
//  2：暂停/1：正在运行/0：未知
//    p：枚举所给的参数
//    hWnd：0
EXTERN_C M_API int MGetProcessState(PSYSTEM_PROCESSES p, HWND hWnd);
//获取SYSTEM_PROCESSES下的SYSTEM_THREADS
EXTERN_C M_API VOID* MGetProcessThreads(DWORD pid);
//获取根据pid在进程列表里获取 SYSTEM_PROCESSES 结构（需调用 MEnumProcessCore(); 刷新列表）
EXTERN_C M_API PSYSTEM_PROCESSES MGetProcessInfo(DWORD pid);
//获取系统内存使用率（0-1）
EXTERN_C M_API double MGetRamUseAge();
//获取系统所有内存大小（MB）
EXTERN_C M_API ULONG MGetAllRam();













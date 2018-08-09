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

void MFroceKillProcessUser();
void MKillProcessUser(BOOL ask);
EXTERN_C M_API BOOL MGetPrivileges2();
EXTERN_C M_API void MEnumProcessCore();
EXTERN_C M_API void MEnumProcessFree();
EXTERN_C M_API void MEnumProcess(EnumProcessCallBack calBack);
EXTERN_C M_API void MEnumProcess2Refesh(EnumProcessCallBack2 callBack);
EXTERN_C M_API BOOL MReUpdateProcess(DWORD pid, EnumProcessCallBack calBack);
EXTERN_C M_API BOOL MNtPathToFilePath(LPWSTR pszNtPath, LPWSTR pszFilePath, size_t bufferSize);
EXTERN_C M_API BOOL MDosPathToNtPath(LPWSTR pszDosPath, LPWSTR pszNtPath);
EXTERN_C M_API DWORD MGetNtPathFromHandle(HANDLE hFile, LPWSTR ps_NTPath, UINT szDosPathSize);
EXTERN_C M_API DWORD MNtPathToDosPath(LPWSTR pszNtPath, LPWSTR pszDosPath, UINT szDosPathSize);
EXTERN_C M_API BOOL MGetProcessFullPathEx(DWORD dwPID, LPWSTR outNter, PHANDLE phandle, LPWSTR pszExeName);
EXTERN_C M_API BOOL MGetExeInfo(LPWSTR strFilePath, LPWSTR InfoItem, LPWSTR str, int maxCount);
EXTERN_C M_API BOOL MGetExeDescribe(LPWSTR pszFullPath, LPWSTR str, int maxCount);
EXTERN_C M_API BOOL MGetExeCompany(LPWSTR pszFullPath, LPWSTR str, int maxCount);
EXTERN_C M_API HICON MGetExeIcon(LPWSTR pszFullPath);
EXTERN_C M_API BOOL MGetExeFileTrust(LPCWSTR lpFileName);
EXTERN_C M_API LONG MVerifyEmbeddedSignature(LPCWSTR pwszSourceFile);

EXTERN_C M_API BOOL MShowExeFileSignatureInfo(LPCWSTR pwszSourceFile);

EXTERN_C M_API BOOL MCloseHandle(HANDLE handle);
EXTERN_C M_API NTSTATUS MSuspendProcessNt(DWORD dwPId, HANDLE handle);
EXTERN_C M_API NTSTATUS MResumeProcessNt(DWORD dwPId, HANDLE handle);
EXTERN_C M_API NTSTATUS MOpenProcessNt(DWORD dwId, PHANDLE pLandle);
EXTERN_C M_API NTSTATUS MTerminateProcessNt(DWORD dwId, HANDLE handle);

EXTERN_C M_API BOOL MRunUWPApp(LPWSTR packageName, LPWSTR name);

EXTERN_C M_API BOOL MGetProcessCommandLine(HANDLE handle, LPWSTR l, int maxcount, DWORD pid = 0);
EXTERN_C M_API BOOL MGetProcessIsUWP(HANDLE handle);
EXTERN_C M_API BOOL MGetProcessIs32Bit(HANDLE handle);
EXTERN_C M_API BOOL MGetProcessEprocess(DWORD pid, PPEOCESSKINFO info);
EXTERN_C M_API BOOL MGetUWPPackageFullName(HANDLE handle, int * len, LPWSTR buffer);
EXTERN_C M_API int MGetProcessState(PSYSTEM_PROCESSES p, HWND hWnd);
EXTERN_C M_API VOID* MGetProcessThreads(DWORD pid);

EXTERN_C M_API int MAppWorkShowMenuProcessPrepare(LPWSTR strFilePath, LPWSTR strFileName, DWORD pid);
EXTERN_C M_API int MAppWorkShowMenuProcess(LPWSTR strFilePath, LPWSTR strFileName, DWORD pid, HWND hDlg, int data, int type, int x, int y);

EXTERN_C M_API double MGetRamUseAge();
EXTERN_C M_API ULONG MGetAllRam();













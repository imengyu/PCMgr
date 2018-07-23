#pragma once
#include "stdafx.h"
#include "perfhlp.h"

typedef void(__cdecl*EnumProcessCallBack)(DWORD pid, DWORD parentid, LPWSTR exename, LPWSTR exefullpath, int tp, HANDLE hProcess);
typedef void(__cdecl*EnumProcessCallBack2)(DWORD pid);

EXTERN_C M_API int MGetCpuCount();
EXTERN_C M_API void MKillProcessUser();
EXTERN_C M_API BOOL MGetPrivileges2();
EXTERN_C M_API void MEnumProcessFree();
EXTERN_C M_API void MEnumProcess(EnumProcessCallBack calBack);
EXTERN_C M_API void MEnumProcess2Refesh(EnumProcessCallBack2 callBack);
EXTERN_C M_API BOOL MReUpdateProcess(DWORD pid, EnumProcessCallBack calBack);
EXTERN_C M_API BOOL MDosPathToNtPath(LPWSTR pszDosPath, LPWSTR pszNtPath);
EXTERN_C M_API DWORD MGetNtPathFromHandle(HANDLE hFile, LPWSTR ps_NTPath, UINT szDosPathSize);
EXTERN_C M_API DWORD MNtPathToDosPath(LPWSTR pszNtPath, LPWSTR pszDosPath, UINT szDosPathSize);
EXTERN_C M_API BOOL MGetProcessFullPathEx(DWORD dwPID, LPWSTR outNter, PHANDLE phandle, LPWSTR pszExeName);
EXTERN_C M_API BOOL MGetExeInfo(LPWSTR strFilePath, LPWSTR InfoItem, LPWSTR str, int maxCount);
EXTERN_C M_API BOOL MGetExeDescribe(LPWSTR pszFullPath, LPWSTR str, int maxCount);
EXTERN_C M_API BOOL MGetExeCompany(LPWSTR pszFullPath, LPWSTR str, int maxCount);
EXTERN_C M_API HICON MGetExeIcon(LPWSTR pszFullPath);

EXTERN_C M_API BOOL MCloseHandle(HANDLE handle);
EXTERN_C M_API DWORD MSuspendProcessNt(DWORD dwPId, HANDLE handle);
EXTERN_C M_API DWORD MRusemeProcessNt(DWORD dwPId, HANDLE handle);
EXTERN_C M_API DWORD MOpenProcessNt(DWORD dwId, PHANDLE pLandle);
EXTERN_C M_API DWORD MTerminateProcessNt(DWORD dwId, HANDLE handle);

EXTERN_C M_API BOOL MGetProcessCommandLine(HANDLE handle, LPWSTR l, int maxcount);
EXTERN_C M_API BOOL MGetProcessIsUWP(HANDLE handle);
EXTERN_C M_API BOOL MGetProcessIs32Bit(HANDLE handle);
EXTERN_C M_API BOOL MGetUWPPackageId(HANDLE handle, MPerfAndProcessData * data);
EXTERN_C M_API BOOL MGetUWPPackageFullName(HANDLE handle, int * len, LPWSTR buffer);
EXTERN_C M_API int MGetProcessState(DWORD pid, HWND hWnd);
EXTERN_C M_API VOID* MGetProcessThreads(DWORD pid);
EXTERN_C M_API ULONG MGetProcessRam(DWORD dwPId);

EXTERN_C M_API int MAppWorkShowMenuProcessPrepare(LPWSTR strFilePath, LPWSTR strFileName, DWORD pid);
EXTERN_C M_API int MAppWorkShowMenuProcess(LPWSTR strFilePath, LPWSTR strFileName, DWORD pid, HWND hDlg, int data, int type);

EXTERN_C M_API double MGetRamUseAge();
EXTERN_C M_API ULONG MGetAllRam();













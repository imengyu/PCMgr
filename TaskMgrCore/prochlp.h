#pragma once
#include "stdafx.h"

typedef void(_stdcall*EnumProcessCallBack)(int pid, int parentid, LPWSTR exename, LPWSTR exefullpath, int tp);

#define EXEPROFENCE_STATE_UNGET 0
#define EXEPROFENCE_STATE_ERROR 1
#define EXEPROFENCE_STATE_SUCCESS 2

typedef struct EXEPROFENCE
{
	int state = 0;
	double cpu;
	ULONG ram;
	int disk;
	int internet;
	UINT64 cputime;
}EXEPROFENCE,*PEXEPROFENCE;

EXTERN_C M_API int MGetCpuCount();
EXTERN_C M_API BOOL MGetPrivileges2();
EXTERN_C M_API void MEnumProcess(EnumProcessCallBack calBack);
EXTERN_C M_API BOOL MDosPathToNtPath(LPWSTR pszDosPath, LPWSTR pszNtPath);
EXTERN_C M_API BOOL MGetProcessFullPathEx(DWORD dwPID, LPWSTR outNter);
EXTERN_C M_API BOOL MGetExeInfo(LPWSTR strFilePath, LPWSTR InfoItem, LPWSTR str, int maxCount);
EXTERN_C M_API BOOL MGetExeDescribe(LPWSTR pszFullPath, LPWSTR str, int maxCount);
EXTERN_C M_API BOOL MGetExeCompany(LPWSTR pszFullPath, LPWSTR str, int maxCount);
EXTERN_C M_API HICON MGetExeIcon(LPWSTR pszFullPath);

EXTERN_C M_API DWORD MSuspendTaskNt(DWORD dwPId);
EXTERN_C M_API DWORD MRusemeTaskNt(DWORD dwPId);
EXTERN_C M_API DWORD MOpenProcessNt(DWORD dwId, PHANDLE pLandle);
EXTERN_C M_API DWORD MTerminateProcessNt(HANDLE handle);
EXTERN_C M_API bool MGetProcessCommandLine(DWORD pid, LPWSTR l, int maxcount);
 

EXTERN_C M_API int MAppWorkShowMenuProcess(LPWSTR strFilePath, LPWSTR strFileName, DWORD pid, HWND hDlg, int data);
EXTERN_C M_API int MGetExeState(DWORD pid, HWND hWnd);
EXTERN_C M_API double MGetCpuUseAge();
EXTERN_C M_API double MGetRamUseAge();
EXTERN_C M_API double MGetDiskUseAge();
EXTERN_C M_API double MGetInternetUseAge();
EXTERN_C M_API ULONG MGetAllRam();
EXTERN_C M_API ULONG MGetExeRam(DWORD dwPId);

M_API EXEPROFENCE MGetExeProfenceInfo(DWORD dwPId, int intervalTime, UINT64 lastcputime);













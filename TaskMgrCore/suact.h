#pragma once
#include "stdafx.h"
#include "ntdef.h"
#include "schlp.h"

typedef struct _KernelModulSmallInfo
{
	ULONG_PTR DriverObject;
	WCHAR szFullDllPathOrginal[MAX_PATH];
	WCHAR szFullDllPath[MAX_PATH];
	WCHAR szServiceName[MAX_PATH];
	LPSERVICE_STORAGE serviceInfo;
}KernelModulSmallInfo,*PKernelModulSmallInfo;

typedef void(__cdecl*EnumKernelModulsCallBack)(
	PKernelModulSmallInfo kmi,
	LPWSTR szBaseDllName, 
	LPWSTR szFullDllPath,
	LPWSTR szFullDllPathOrginal,
	LPWSTR szEntryPoint,
	LPWSTR szSizeOfImage,
	LPWSTR szDriverObject,
	LPWSTR szBase,
	LPWSTR szServiceName,
	ULONG Order);

M_CAPI(BOOL) M_SU_CreateFile(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, DWORD dwCreationDisposition, PHANDLE pHandle);
M_CAPI(BOOL) M_SU_OpenProcess(DWORD pid, PHANDLE pHandle, NTSTATUS* pStatus);
M_CAPI(BOOL) M_SU_OpenThread(DWORD pid, DWORD tid, PHANDLE pHandle, NTSTATUS* pStatus);
M_CAPI(BOOL) M_SU_TerminateProcess(HANDLE hProcess, UINT exitCode, NTSTATUS* pStatus);
M_CAPI(BOOL) M_SU_TerminateThread(HANDLE hProcess, UINT exitCode, NTSTATUS* pStatus);
M_CAPI(BOOL) M_SU_CloseHandleWithProcess(_SYSTEM_HANDLE_TABLE_ENTRY_INFO*sh);

M_CAPI(BOOL) M_SU_TerminateProcessPID(DWORD pid, UINT exitCode, NTSTATUS* pStatus, BOOL useApc = FALSE);
M_CAPI(BOOL) M_SU_TerminateThreadTID(DWORD tid, UINT exitCode, NTSTATUS* pStatus, BOOL useApc = FALSE);

M_CAPI(BOOL) M_SU_SuspendProcess(DWORD pid, UINT exitCode, NTSTATUS * pStatus);
M_CAPI(BOOL) M_SU_ResumeProcess(DWORD pid, UINT exitCode, NTSTATUS * pStatus);

BOOL M_SU_ForceShutdown();
BOOL M_SU_ForceReboot();

BOOL M_SU_ProtectMySelf();
BOOL M_SU_UnProtectMySelf();

M_CAPI(BOOL) M_SU_Init();

M_CAPI(BOOL) M_SU_GetEPROCESS(DWORD pid, ULONG_PTR* lpEprocess);
M_CAPI(BOOL) M_SU_GetETHREAD(DWORD tid, ULONG_PTR* lpEthread);

LRESULT M_SU_EnumKernelModuls_HandleWmCommand(WPARAM wParam);

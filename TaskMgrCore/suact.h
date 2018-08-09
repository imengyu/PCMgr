#pragma once
#include "stdafx.h"
#include "ntdef.h"
#include "schlp.h"
#include "sysstructs.h"

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

typedef void(__cdecl*DACALLBACK)(ULONG_PTR curaddress, LPWSTR addressstr, LPWSTR shellstr, LPWSTR bariny, LPWSTR asmstr);



M_CAPI(BOOL) M_SU_CreateFile(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, DWORD dwCreationDisposition, PHANDLE pHandle);
M_CAPI(BOOL) M_SU_OpenProcess(DWORD pid, PHANDLE pHandle, NTSTATUS* pStatus);
M_CAPI(BOOL) M_SU_OpenThread(DWORD pid, DWORD tid, PHANDLE pHandle, NTSTATUS* pStatus);
M_CAPI(BOOL) M_SU_TerminateProcessPID(DWORD pid, UINT exitCode, NTSTATUS* pStatus, BOOL useApc = FALSE);
M_CAPI(BOOL) M_SU_TerminateThreadTID(DWORD tid, UINT exitCode, NTSTATUS* pStatus, BOOL useApc = FALSE);

M_CAPI(BOOL) M_SU_CloseHandleWithProcess(DWORD pid, LPVOID handleValue);

M_CAPI(BOOL) M_SU_SuspendProcess(DWORD pid, UINT exitCode, NTSTATUS * pStatus);
M_CAPI(BOOL) M_SU_ResumeProcess(DWORD pid, UINT exitCode, NTSTATUS * pStatus);

M_CAPI(BOOL) M_SU_KDA(DACALLBACK callback, ULONG_PTR startaddress, ULONG_PTR size);

M_CAPI(BOOL) M_SU_TerminateProcessPIDTest(DWORD pid, NTSTATUS * pStatus);

BOOL M_SU_ForceShutdown();
BOOL M_SU_ForceReboot();

BOOL M_SU_ProtectMySelf();
BOOL M_SU_UnProtectMySelf();

VOID M_SU_TestLastDbgPrint();

M_CAPI(VOID) M_SU_Test(LPCSTR instr);

M_CAPI(VOID) M_SU_Test2();



M_CAPI(BOOL) M_SU_Init(BOOL requestNtosValue, PKNTOSVALUE outValue);

M_CAPI(BOOL) M_SU_GetEPROCESS(DWORD pid, ULONG_PTR* lpEprocess, ULONG_PTR* lpPeb, ULONG_PTR* lpJob, LPWSTR imagename, LPWSTR path); 
M_CAPI(BOOL) M_SU_GetETHREAD(DWORD tid, ULONG_PTR* lpEthread, ULONG_PTR * lpTeb);

M_CAPI(BOOL) M_SU_GetProcessCommandLine(DWORD tid, LPWSTR outCmdLine);

M_CAPI(BOOL) M_SU_SetDbgViewEvent(HANDLE hEvent);

M_CAPI(BOOL) M_SU_ReSetDbgViewEvent();

M_CAPI(BOOL) M_SU_GetDbgViewLastBuffer(LPWSTR outbuffer, size_t bufsize, BOOL*hasMoreData);

M_CAPI(BOOL) M_SU_PrintInternalFuns();

LRESULT M_SU_EnumKernelModuls_HandleWmCommand(WPARAM wParam);

#include "stdafx.h"
#include "suact.h"
#include "prochlp.h"
#include "thdhlp.h"
#include "ntdef.h"

extern ZwTerminateThreadFun ZwTerminateThread;
extern ZwTerminateProcessFun ZwTerminateProcess;

M_CAPI(BOOL) M_SU_CreateFile(	LPCWSTR lpFileName,	DWORD dwDesiredAccess, DWORD dwShareMode,	 DWORD dwCreationDisposition, PHANDLE pHandle)
{
	HANDLE h = CreateFileW(lpFileName, dwDesiredAccess, dwShareMode, NULL, dwCreationDisposition, FILE_ATTRIBUTE_NORMAL, NULL);
	if (h) {
		*pHandle = h;
		return TRUE;
	}
	else {

	}
	return 0;
}
M_CAPI(BOOL) M_SU_OpenProcess(DWORD pid, PHANDLE pHandle)
{
	return MOpenProcessNt(pid, pHandle) == 0;
}
M_CAPI(BOOL) M_SU_OpenThread(DWORD pid, DWORD tid, PHANDLE pHandle)
{
	return MOpenThreadNt(tid, pHandle, pid) == 0;
}
M_CAPI(BOOL) M_SU_TerminateProcess(HANDLE hProcess, UINT exitCode)
{
	if (NT_SUCCESS(ZwTerminateProcess(hProcess, exitCode)))
		return TRUE;
	else {

	}
	return 0;
}
M_CAPI(BOOL) M_SU_TerminateThread(HANDLE hProcess, UINT exitCode)
{
	if (NT_SUCCESS(ZwTerminateThread(hProcess, exitCode)))
		return TRUE;
	else {

	}
	return 0;
}
M_CAPI(BOOL) M_SU_CloseHandleWithProcess(SYSTEM_HANDLE * sh)
{
	BOOL rs = FALSE;
	HANDLE hFile = (HANDLE)sh->wValue;
	HANDLE hProcess;
	if (M_SU_OpenProcess(sh->dwProcessId, &hProcess)) {
		HANDLE hDup = 0;
		BOOL b = DuplicateHandle(hProcess, hFile, GetCurrentProcess(),
			&hDup, DUPLICATE_SAME_ACCESS, FALSE, DUPLICATE_CLOSE_SOURCE);
		if (hDup) rs = MCloseHandle(hDup);
		MCloseHandle(hProcess);
	}
	return rs;
}
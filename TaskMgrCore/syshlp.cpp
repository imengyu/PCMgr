#include "stdafx.h"
#include "syshlp.h"
#include "shellapi.h"

BOOL _Is64BitOS = -1;
BOOL _IsRunasAdmin = -1;


extern _RunFileDlg RunFileDlg;
extern LdrGetProcedureAddressFun LdrGetProcedureAddress;
extern RtlInitAnsiStringFun RtlInitAnsiString;

M_CAPI(BOOL) MRunFileDlg(_In_ HWND hwndOwner, _In_opt_ HICON hIcon, _In_opt_ LPCWSTR lpszDirectory, _In_opt_ LPCWSTR lpszTitle, _In_opt_ LPCWSTR lpszDescription, _In_ ULONG uFlags)
{
	return RunFileDlg(hwndOwner, hIcon, lpszDirectory, lpszTitle, lpszDescription, uFlags);
}
M_CAPI(BOOL) MIs64BitOS()
{
	if (_Is64BitOS == -1)
	{
		typedef void (WINAPI *LPFN_PGNSI)(LPSYSTEM_INFO);
		BOOL bRetVal = FALSE;
		SYSTEM_INFO si = { 0 };
		LPFN_PGNSI pGNSI = (LPFN_PGNSI)GetProcAddress(GetModuleHandle(L"kernel32.dll"), "GetNativeSystemInfo");
		if (pGNSI == NULL)
			return FALSE;
		pGNSI(&si);
		if (si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64 ||
			si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_IA64)
			bRetVal = TRUE;
		_Is64BitOS = bRetVal;
	}
	return _Is64BitOS;
}
M_CAPI(BOOL) MGetPrivileges()
{
	HANDLE hToken;
	TOKEN_PRIVILEGES tp;
	TOKEN_PRIVILEGES oldtp;
	DWORD dwSize = sizeof(TOKEN_PRIVILEGES);
	LUID luid;
	TOKEN_PRIVILEGES tkp = { 0 };

	ZeroMemory(&tp, sizeof(tp));
	if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken)) {
		if (GetLastError() == ERROR_CALL_NOT_IMPLEMENTED) return true;
		else return false;
	}
	if (!LookupPrivilegeValue(NULL, SE_SHUTDOWN_NAME, &luid))
	{
		CloseHandle(hToken);
		return false;
	}
	tp.PrivilegeCount = 1;
	tp.Privileges[0].Luid = luid;
	tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	if (!AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), &oldtp, &dwSize)) {
		CloseHandle(hToken);
		return false;
	}
	if (!LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &luid))
	{
		CloseHandle(hToken);
		return false;
	}
	tp.PrivilegeCount = 1;
	tp.Privileges[0].Luid = luid;
	tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	if (!AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), &oldtp, &dwSize)) {
		CloseHandle(hToken);
		return false;
	}
	if (!LookupPrivilegeValue(NULL, SE_LOAD_DRIVER_NAME, &luid))
	{
		CloseHandle(hToken);
		return false;
	}
	tp.PrivilegeCount = 1;
	tp.Privileges[0].Luid = luid;
	tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	if (!AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), &oldtp, &dwSize)) {
		CloseHandle(hToken);
		return false;
	}

	CloseHandle(hToken);
	return true;
}
M_CAPI(BOOL) MIsRunasAdmin()
{
	if (_IsRunasAdmin = -1)
	{
		BOOL bElevated = FALSE;
		HANDLE hToken = NULL;

		// Get current process token
		if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &hToken))
			return FALSE;

		TOKEN_ELEVATION tokenEle;
		DWORD dwRetLen = 0;

		// Retrieve token elevation information
		if (GetTokenInformation(hToken, TokenElevation, &tokenEle, sizeof(tokenEle), &dwRetLen))
		{
			if (dwRetLen == sizeof(tokenEle))
			{
				bElevated = tokenEle.TokenIsElevated;
			}
		}

		CloseHandle(hToken);
		_IsRunasAdmin = bElevated;
	}
	return _IsRunasAdmin;
}
M_CAPI(PVOID) MGetProcedureAddress(_In_ PVOID DllHandle, _In_opt_ PSTR ProcedureName, _In_opt_ ULONG ProcedureNumber)
{
	DWORD status;
	ANSI_STRING procedureName;
	PVOID procedureAddress;

	if (ProcedureName)
	{
		RtlInitAnsiString(&procedureName, ProcedureName);
		status = LdrGetProcedureAddress(DllHandle, &procedureName, 0, &procedureAddress);
		if (!NT_SUCCESS(status)) {
			char c[260];
			char dllname[260];
			GetModuleFileNameA((HMODULE)DllHandle, dllname, 260);
			sprintf_s(c, "无法定位程序输入点 %s 位于动态链接库 %s 上", ProcedureName, dllname);
			MessageBoxA(NULL, c, "系统错误", MB_OK | MB_ICONERROR);
			return NULL;
		}
	}
	else
	{
		status = LdrGetProcedureAddress(DllHandle, NULL, ProcedureNumber, &procedureAddress);
		if (!NT_SUCCESS(status)) {
			char c[260];
			char dllname[260];
			GetModuleFileNameA((HMODULE)DllHandle, dllname, 260);
			sprintf_s(c, "无法定位序数 %ld 位于动态链接库 %s 上", ProcedureNumber, dllname);
			MessageBoxA(NULL, c, "系统错误", MB_OK | MB_ICONERROR);
			return NULL;
		}
	}
	if (NT_SUCCESS(status))
		return procedureAddress;
	return NULL;
}
M_CAPI(PVOID) MGetProcAddress(_In_ PVOID DllHandle, _In_opt_ PSTR ProcedureName)
{
	return GetProcAddress((HMODULE)DllHandle, ProcedureName);
}
M_CAPI(BOOL) MCommandLineToFilePath(LPWSTR cmdline, LPWSTR buffer, int size)
{
	if (cmdline && buffer)
	{
		int argc = 0;
		LPWSTR *szArglist = CommandLineToArgvW(cmdline, &argc);
		if (szArglist)
		{
			wcscpy_s(buffer, size, szArglist[0]);
			LocalFree(szArglist);
			return  TRUE;
		}
	}
	return  FALSE;
}

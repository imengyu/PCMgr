#include "stdafx.h"
#include "syshlp.h"
#include "shellapi.h"
#include "loghlp.h"
#include "mapphlp.h"
#include "PathHelper.h"

BOOL _Is64BitOS = -1;
BOOL _IsRunasAdmin = -1;

DWORD currentWindowsBulidVer = 0;
DWORD currentWindowsMajor = 0;

extern _RunFileDlg RunFileDlg;
extern LdrGetProcedureAddressFun LdrGetProcedureAddress;
extern RtlInitAnsiStringFun RtlInitAnsiString;
extern NtQuerySystemInformationFun NtQuerySystemInformation;

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
	NTSTATUS status;
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
M_CAPI(BOOL) MGetWindowsWin8Upper() {
	return currentWindowsMajor >= 8;
}
M_CAPI(BOOL) MGetWindowsBulidVersion() {

	HKEY hKey;
#ifdef _AMD64_
	DWORD err = RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", 0, KEY_READ, &hKey);
#else
	DWORD err = RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", 0, KEY_WOW64_64KEY | KEY_READ, &hKey);
#endif
	if (err == ERROR_SUCCESS)
	{
		DWORD majotver = 0;
		DWORD majotverLength = MAX_PATH;
		DWORD majotverType = REG_SZ;
		err = RegQueryValueEx(hKey, L"CurrentMajorVersionNumber", 0, &majotverType, (LPBYTE)&majotver, &majotverLength);
		if (err == ERROR_SUCCESS) {
			currentWindowsMajor = majotver;
			Log(L"Windows Version : %d", majotver);
		}

		TCHAR bulidver[MAX_PATH] = { 0 };
		DWORD bulidverLength = MAX_PATH;
		DWORD bulidverType = REG_SZ;
		err = RegQueryValueEx(hKey, L"CurrentBuild", 0, &bulidverType, (LPBYTE)&bulidver, &bulidverLength);
		if (err == ERROR_SUCCESS)
			currentWindowsBulidVer = static_cast<DWORD>(_wtoll(bulidver));
		TCHAR buildLab[MAX_PATH] = { 0 };
		DWORD buildLabLength = MAX_PATH;
		DWORD buildLabType = REG_SZ;
		err = RegQueryValueEx(hKey, L"BuildLab", 0, &buildLabType, (LPBYTE)&buildLab, &buildLabLength);

		Log(L"Windows Bulid version : %s \n      Internal bulid Version : %s", bulidver, buildLab);
	}
	return FALSE;
}
M_CAPI(BOOL) MRunExe(LPWSTR path, LPWSTR args, BOOL runAsadmin, HWND hWnd)
{
	return ShellExecute(hWnd, runAsadmin ? L"runas" : L"run", path, args, NULL, SW_SHOW) > (HINSTANCE)32;
}
M_CAPI(BOOL) MGetNtosNameAndStartAddress(LPWSTR name, size_t buffersize, ULONG_PTR *address)
{
	ULONG outLength = 0;
	PSYSTEM_MODULE_INFORMATION pSysModuleNames = (PSYSTEM_MODULE_INFORMATION)malloc(sizeof(SYSTEM_MODULE_INFORMATION));
	if (NtQuerySystemInformation(SystemModuleInformation, pSysModuleNames, 0, &outLength) == STATUS_INFO_LENGTH_MISMATCH)
	{
		free(pSysModuleNames);
		pSysModuleNames = (PSYSTEM_MODULE_INFORMATION)malloc(outLength);
	}
	else
	{
		free(pSysModuleNames);
		pSysModuleNames = NULL;
	}

	if (pSysModuleNames)
	{
		NTSTATUS status = NtQuerySystemInformation(SystemModuleInformation, pSysModuleNames, outLength, &outLength);
		if (!NT_SUCCESS(status)) {
			LogErr(L"MGetNtosName NtQuerySystemInformation failed : 0x%08X", status);
			free(pSysModuleNames);
			return 0;
		}

		PSYSTEM_MODULE_INFORMATION SystemModuleInfo = (PSYSTEM_MODULE_INFORMATION)((PULONG)pSysModuleNames + 1);
		LPWSTR strw = MConvertLPCSTRToLPWSTR(SystemModuleInfo->ImageName);
		std::wstring *s = Path::GetFileName(strw);
		if (name)wcscpy_s(name, buffersize, s->c_str());
		if (address)*address = (ULONG_PTR)SystemModuleInfo->Base;
		delete strw;
		delete s;
		free(pSysModuleNames);
	}
	return 0;
}

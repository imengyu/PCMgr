#include "stdafx.h"
#include "syshlp.h"
#include "prochlp.h"
#include "shellapi.h"
#include "loghlp.h"
#include "mapphlp.h"
#include "PathHelper.h"
#include "StringHlp.h"

BOOL _Is64BitOS = -1;
BOOL _IsRunasAdmin = -1;
BOOL _IsUserGuset = -1;

DWORD currentWindowsBulidVer = 0;
DWORD currentWindowsMajor = 0;

extern WCHAR debuggerCommand[MAX_PATH];

extern _RunFileDlg RunFileDlg;
extern LdrGetProcedureAddressFun LdrGetProcedureAddress;
extern RtlInitAnsiStringFun RtlInitAnsiString;
extern NtQuerySystemInformationFun NtQuerySystemInformation;
_MGetProcAddressCore MGetProcAddressCore;

M_CAPI(BOOL) MRunExeWithAgrs(LPWSTR pathargs, BOOL runAsadmin, HWND hWnd)
{
	WCHAR filePath[MAX_PATH];
	WCHAR agrs[256];
	if (MCommandLineSplitPath(pathargs, filePath, MAX_PATH, agrs, 256))
		return MRunExe(filePath, agrs, runAsadmin, hWnd);
	return FALSE;
}
M_CAPI(BOOL) MRunExe(LPWSTR path, LPWSTR args, BOOL runAsadmin, HWND hWnd)
{
	return (DWORD)(ULONG_PTR)ShellExecute(hWnd, runAsadmin ? L"runas" : L"open", path, args, NULL, SW_SHOW) > 32;
}
M_CAPI(BOOL) MRunFileDlg(_In_ HWND hwndOwner, _In_opt_ HICON hIcon, _In_opt_ LPCWSTR lpszDirectory, _In_opt_ LPCWSTR lpszTitle, _In_opt_ LPCWSTR lpszDescription, _In_ ULONG uFlags)
{
	return RunFileDlg(hwndOwner, hIcon, lpszDirectory, lpszTitle, lpszDescription, uFlags);
}

M_CAPI(BOOL) MIs64BitOS()
{
	if (_Is64BitOS == -1)
	{
		BOOL bRetVal = FALSE;
		SYSTEM_INFO si = { 0 };

		GetNativeSystemInfo(&si);
		if (si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64 ||
			si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_IA64 || 
			si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_ARM64)
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
M_CAPI(BOOL) MIsUserGuest()
{
	BOOL IsMember;
	PSID pSid = 0;
	_SID_IDENTIFIER_AUTHORITY pIdentifierAuthority = { 0 };

	*(WORD *)&pIdentifierAuthority.Value[4] = 1280;
	*(DWORD *)pIdentifierAuthority.Value = 0;
	if (_IsUserGuset == -1)
	{
		_IsUserGuset = 2;
		IsMember = 0;
		if (AllocateAndInitializeSid(&pIdentifierAuthority, 2u, 0x20u, 0x222u, 0, 0, 0, 0, 0, 0, &pSid))
		{
			CheckTokenMembership(0, pSid, &IsMember);
			_IsUserGuset = (IsMember != 1) + 1;
			FreeSid(pSid);
		}
	}
	return _IsUserGuset == 1;
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
	return MGetProcAddressCore((HMODULE)DllHandle, ProcedureName);
}

M_CAPI(BOOL) MCommandLineToFilePath(LPWSTR cmdline, LPWSTR buffer, int size)
{
	if (cmdline && buffer)
	{
		int argc = 0;
		size_t len = wcslen(cmdline);
		if (len > 0 && len < MAX_PATH && cmdline[0] == L'\"' && cmdline[len - 1] == L'\"') {
			WCHAR fixcmdline[MAX_PATH];
			wcscpy_s(fixcmdline, cmdline);

			WCHAR* firstArgPos = wcswcs(fixcmdline, L".exe -");
			if (firstArgPos) {
				*(firstArgPos + 2 * sizeof(WCHAR)) = L'\"';
				*(firstArgPos + 3 * sizeof(WCHAR)) = L'\0';
			}

			LPWSTR *szArglist = CommandLineToArgvW(fixcmdline, &argc);
			if (szArglist && argc > 0)
			{
				wcscpy_s(buffer, size, szArglist[0]);
				LocalFree(szArglist);
				return  TRUE;
			}
		}
		else {
			LPWSTR *szArglist = CommandLineToArgvW(cmdline, &argc);
			if (szArglist && argc > 0)
			{
				wcscpy_s(buffer, size, szArglist[0]);
				LocalFree(szArglist);
				return  TRUE;
			}
		}
	}
	return  FALSE;
}
M_CAPI(BOOL) MCommandLineSplitPath(LPWSTR cmdline, LPWSTR buffer, int size, LPWSTR argbuffer, int argbuffersize)
{
	if (cmdline && buffer)
	{
		int argc = 0;
		LPWSTR *szArglist = CommandLineToArgvW(cmdline, &argc);
		if (szArglist)
		{
			wcscpy_s(buffer, size, szArglist[0]);

			std::wstring s;
			for (int i = 1; i < argc; i++)
			{
				if (i != 1) s += L" ";
				s += szArglist[i];
			}

			wcscpy_s(argbuffer, argbuffersize, s.c_str());

			LocalFree(szArglist);
			return  TRUE;
		}
	}
	return  FALSE;
}

M_CAPI(DWORD) MGetWindowsBulidVersionValue() {
	return currentWindowsBulidVer;
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

		Log(L"Windows Bulid version : %s \n     Internal bulid Version : %s", bulidver, buildLab);
	}
	return FALSE;
}
M_CAPI(BOOL) MGetNtosAndWin32kfullNameAndStartAddress(LPWSTR name, size_t buffersize, ULONG_PTR *address, ULONG_PTR *win32kfulladdress)
{
	ULONG outLength = 0;
	PSYSTEMMODULELIST pSysModuleNames = (PSYSTEMMODULELIST)MAlloc(sizeof(SYSTEM_MODULE_INFORMATION));
	if (NtQuerySystemInformation(SystemModuleInformation, pSysModuleNames, 0, &outLength) == STATUS_INFO_LENGTH_MISMATCH)
	{
		MFree(pSysModuleNames);
		pSysModuleNames = (PSYSTEMMODULELIST)MAlloc(outLength);
	}
	else
	{
		MFree(pSysModuleNames);
		pSysModuleNames = NULL;
	}

	if (pSysModuleNames)
	{
		NTSTATUS status = NtQuerySystemInformation(SystemModuleInformation, pSysModuleNames, outLength, &outLength);
		if (!NT_SUCCESS(status)) {
			LogErr(L"MGetNtosName NtQuerySystemInformation failed : 0x%08X", status);
			MFree(pSysModuleNames);
			return 0;
		}

		int allModulesCount = pSysModuleNames->ulCount;

		//System Modules
		PSYSTEM_MODULE_INFORMATION SystemModuleInfo = (PSYSTEM_MODULE_INFORMATION)((PULONG)&pSysModuleNames->smi[0]);
			//Find ntoskrnl
		LPWSTR strw = A2W(SystemModuleInfo->ImageName);
		std::wstring *s = Path::GetFileName(strw);
		if (name)wcscpy_s(name, buffersize, s->c_str());
		if (address)*address = (ULONG_PTR)SystemModuleInfo->Base;
		delete strw;
		delete s;

		PSYSTEM_MODULE_INFORMATION SystemModuleInfoThis;
		//And then , find win32k.sys
		for (int i = 2; i < allModulesCount; i++) {
			SystemModuleInfoThis = (PSYSTEM_MODULE_INFORMATION)((PULONG)&pSysModuleNames->smi[i]);
			if (StringHlp::StrEqualA(SystemModuleInfoThis->ImageName, "\\SystemRoot\\System32\\win32k.sys"))
				if (win32kfulladdress)
					*win32kfulladdress = (ULONG_PTR)SystemModuleInfoThis->Base;
		}
		MFree(pSysModuleNames);
	}
	return 0;
}

BOOL MGetDebuggerInformation() {

	HKEY hKey;
	if (RegOpenKey(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AeDebug", &hKey) == ERROR_SUCCESS) {
		TCHAR dwValue[256];
		DWORD dwSzType = REG_SZ;
		DWORD dwSize = sizeof(dwValue);
		if (RegQueryValueEx(hKey, L"Debugger", 0, &dwSzType, (LPBYTE)&dwValue, &dwSize) == ERROR_SUCCESS)
		{
			wcscpy_s(debuggerCommand, dwValue);
			return TRUE;
		}
	}
	return FALSE;
}
M_CAPI(BOOL) MGetSystemAffinityMask(PULONG_PTR SystemAffinityMask)
{
	if (!SystemAffinityMask)return FALSE;

	SYSTEM_BASIC_INFORMATION systemBasicInfo;

	NTSTATUS status = NtQuerySystemInformation(
		SystemBasicInformation,
		&systemBasicInfo,
		sizeof(SYSTEM_BASIC_INFORMATION),
		NULL
	);

	if (NT_SUCCESS(status)) {
		*SystemAffinityMask = systemBasicInfo.ActiveProcessorsAffinityMask;
		return TRUE;
	}
	return FALSE;
}

M_CAPI(LPWSTR) MKeyToStr(UINT vk)
{
	switch (vk)
	{
	case 'A':return L"A";
	case 'B':return L"B";
	case 'C':return L"C";
	case 'D':return L"D";
	case 'E':return L"E";
	case 'F':return L"F";
	case 'G':return L"G";
	case 'H':return L"H";
	case 'I':return L"I";
	case 'J':return L"J";
	case 'K':return L"K";
	case 'L':return L"L";
	case 'M':return L"M";
	case 'N':return L"N";
	case 'O':return L"O";
	case 'P':return L"P";
	case 'Q':return L"Q";
	case 'R':return L"A";
	case 'S':return L"S";
	case 'T':return L"T";
	case 'U':return L"U";
	case 'V':return L"V";
	case 'W':return L"W";
	case 'X':return L"X";
	case 'Y':return L"Y";
	case 'Z':return L"Z";
	case VK_BACK:return L"Backspace";
	case VK_TAB:return L"Tab";
	case VK_CLEAR:return L"VK_CLEAR";
	case VK_RETURN:return L"Enter";
	case VK_PAUSE:return L"Pause break";
	case VK_CAPITAL:return L"Caps Lock";
	case VK_KANA:return L"VK_KANA";
	case VK_JUNJA:return L"VK_JUNJA";
	case VK_FINAL:return L"VK_FINAL";
	case VK_HANJA:return L"VK_HANJA";
	case VK_ESCAPE:return L"Escape";
	case VK_CONVERT:return L"VK_CONVERT";
	case VK_NONCONVERT:return L"VK_NONCONVERT";
	case VK_ACCEPT:return L"VK_ACCEPT";
	case VK_MODECHANGE:return L"VK_MODECHANGE";
	case VK_SPACE:return L"Space";
	case VK_PRIOR:return L"Page Up";
	case VK_NEXT:return L"Page Down";
	case VK_END:return L"End";
	case VK_HOME:return L"Home";
	case VK_LEFT:return L"Left";
	case VK_UP:return L"Up";
	case VK_RIGHT:return L"Right";
	case VK_DOWN:return L"Down";
	case VK_SELECT:return L"VK_SELECT";
	case VK_PRINT:return L"VK_PRINT";
	case VK_EXECUTE:return L"VK_EXECUTE";
	case VK_SNAPSHOT:return L"Print Screen";
	case VK_INSERT:return L"Insert";
	case VK_DELETE:return L"Delete";
	case VK_HELP:return L"VK_HELP";
	case VK_NUMPAD0:return L"Numpad 0";
	case VK_NUMPAD1:return L"Numpad 1";
	case VK_NUMPAD2:return L"Numpad 2";
	case VK_NUMPAD3:return L"Numpad 3";
	case VK_NUMPAD4:return L"Numpad 4";
	case VK_NUMPAD5:return L"Numpad 5";
	case VK_NUMPAD6:return L"Numpad 6";
	case VK_NUMPAD7:return L"Numpad 7";
	case VK_NUMPAD8:return L"Numpad 8";
	case VK_NUMPAD9:return L"Numpad 9";
	case VK_MULTIPLY:return L"Multiply(*)";
	case VK_ADD:return L"Add(+)";
	case VK_SEPARATOR:return L"|";
	case VK_SUBTRACT:return L"sUBSTRACT(-)";
	case VK_DECIMAL:return L"Decimal(.)";
	case VK_DIVIDE:return L"Divide(/)";
	case VK_F1:return L"F1";
	case VK_F2:return L"F2";
	case VK_F3:return L"F3";
	case VK_F4:return L"F4";
	case VK_F5:return L"F5";
	case VK_F6:return L"F6";
	case VK_F7:return L"F7";
	case VK_F8:return L"F8";
	case VK_F9:return L"F9";
	case VK_F10:return L"F10";
	case VK_F11:return L"F11";
	case VK_F12:return L"F12";
	case VK_F13:return L"F13";
	case VK_F14:return L"F14";
	case VK_F15:return L"F15";
	case VK_F16:return L"F16";
	case VK_F17:return L"F17";
	case VK_F18:return L"F18";
	case VK_F19:return L"F19";
	case VK_F20:return L"F20";
	case VK_F21:return L"F21";
	case VK_F22:return L"F22";
	case VK_F23:return L"F23";
	case VK_F24:return L"F24";
	}
	return L"";
}
M_CAPI(BOOL) MHotKeyToStr(UINT fsModifiers, UINT vk, LPWSTR buffer, int size)
{
	WCHAR strBuffer[32];
	if ((fsModifiers & MOD_CONTROL) == MOD_CONTROL)
		wcscat_s(strBuffer, L"+ Control ");
	if ((fsModifiers & MOD_ALT) == MOD_ALT)
		wcscat_s(strBuffer, L"+ Alt ");
	if ((fsModifiers & MOD_SHIFT) == MOD_SHIFT)
		wcscat_s(strBuffer, L"+ Shift ");
	if ((fsModifiers & MOD_WIN) == MOD_WIN)
		wcscat_s(strBuffer, L"+ Win ");

	wcscat_s(strBuffer, L"+ ");
	wcscat_s(strBuffer, MKeyToStr(vk));

	if (strBuffer[0] == '+') strBuffer[0] = ' ';

	wcscpy_s(buffer, size, strBuffer);
	return TRUE;
}

M_API BOOL MCopyToClipboard(const WCHAR* pszData, const size_t nDataLen)
{
	if (OpenClipboard(NULL))
	{
		EmptyClipboard();
		HGLOBAL clipbuffer;
		WCHAR *buffer;
		clipbuffer = GlobalAlloc(GMEM_DDESHARE, (nDataLen + 1) * sizeof(WCHAR));
		buffer = (WCHAR*)GlobalLock(clipbuffer);
		wcscpy_s(buffer, nDataLen + 1, pszData);
		GlobalUnlock(clipbuffer);
		SetClipboardData(CF_UNICODETEXT, clipbuffer);
		CloseClipboard();
		return TRUE;
	}
	return FALSE;
}
M_API BOOL MCopyToClipboard2(const WCHAR* pszData) {
	return MCopyToClipboard(pszData, (wcslen(pszData) + 1) * sizeof(WCHAR));
}
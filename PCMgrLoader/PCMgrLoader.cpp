//PCMgrLoader.cpp: 定义应用程序的入口点。
//

#include "stdafx.h"
#include "PCMgrLoader.h"

#define _WIN32_WINNT_WIN7 0x0601

#define PCMGRTITLE L"PC Manager"

typedef DWORD(*_MAppMainGetExitCode)();
typedef void (*_MAppMainRun)();
typedef void* (__cdecl *fnmemset)(void*  _Dst, int _Val, size_t _Size);
typedef long(NTAPI* fnRtlGetVersion)(PRTL_OSVERSIONINFOW lpVersionInformation);

_MAppMainGetExitCode MAppMainGetExitCode;
_MAppMainRun MAppMainRun;
fnmemset _memset;
fnRtlGetVersion RtlGetVersion;

HMODULE hMain = NULL;
HMODULE hMsvcrt = NULL;

int main();
bool load_funs();
bool IsWindowsVersionOrGreater(WORD wMajorVersion, WORD wMinorVersion, WORD wServicePackMajor);
void show_err(const wchar_t* err);

int main_entry() {
	int rs = main();
	ExitProcess(rs);
	return rs;
}
int main()
{
	hMsvcrt = LoadLibrary(L"msvcrt.dll");
	if (!hMsvcrt) show_err(L"MSVCRT Not found !");

	_memset = (fnmemset)GetProcAddress(hMsvcrt, "memset");
	RtlGetVersion = (fnRtlGetVersion)GetProcAddress(GetModuleHandleW(L"ntdll.dll"), "RtlGetVersion");

	if (!IsWindowsVersionOrGreater(HIBYTE(_WIN32_WINNT_WIN7), LOBYTE(_WIN32_WINNT_WIN7), 0))
	{
		show_err(L"Application not support your Windows, Running this program requires Windows 7 at least.");
		return 0;
	}
	if (load_funs())
	{
		MAppMainRun();
		return MAppMainGetExitCode();
	}
	return 0;
}

bool load_funs() {
#ifdef _AMD64_
	hMain = LoadLibrary(L"PCMgr64.dll");
#else
	hMain = LoadLibrary(L"PCMgr32.dll");
#endif // _AMD64_
	if (hMain)
	{
		MAppMainGetExitCode = (_MAppMainGetExitCode)GetProcAddress(hMain, "MAppMainGetExitCode");
		MAppMainRun = (_MAppMainRun)GetProcAddress(hMain, "MAppMainRun");
		bool success =  (MAppMainGetExitCode && MAppMainRun);
		if (!success)
			show_err(L"Load base dll failed !");
		return success;
	}
	else
	{
#ifdef _AMD64_
		show_err(L"Can not load PCMgr64.dll !");
#else
		show_err(L"Can not load PCMgr32.dll !");
#endif // _AMD64_
	}
	return false;
}
void show_err(const wchar_t* err)
{
	MessageBox(NULL, (LPWSTR)err, PCMGRTITLE, MB_ICONERROR);
}

bool IsWindowsVersionOrGreater(WORD wMajorVersion, WORD wMinorVersion, WORD wServicePackMajor)
{
	RTL_OSVERSIONINFOEXW verInfo;
	verInfo.dwOSVersionInfoSize = sizeof(verInfo);
	_memset(&verInfo, 0, sizeof(verInfo));

	if (RtlGetVersion != 0 && RtlGetVersion((PRTL_OSVERSIONINFOW)&verInfo) == 0)
	{
		if (verInfo.dwMajorVersion > wMajorVersion)
			return true;
		else if (verInfo.dwMajorVersion < wMajorVersion)
			return false;
		if (verInfo.dwMinorVersion > wMinorVersion)
			return true;
		else if (verInfo.dwMinorVersion < wMinorVersion)
			return false;
		if (verInfo.wServicePackMajor >= wServicePackMajor)
			return true;
	}

	return false;
}



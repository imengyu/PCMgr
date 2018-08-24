#include "stdafx.h"
#include "loghlp.h"
#include <stdarg.h>
#include <stdio.h>
#include "StringHlp.h"
#include "resource.h"
#include "mapphlp.h"
#include <Shlwapi.h>

bool tofile = false;
bool showConsole = false;

FILE *logFile = NULL;
HANDLE hOutput = NULL;
WCHAR logPath[MAX_PATH];

extern HINSTANCE hInst;

void M_LOG_DefConsoleTextColor() {
	if (hOutput) SetConsoleTextAttribute(hOutput, FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_BLUE);
}
M_CAPI(void) M_LOG_Close()
{
	if (hOutput || showConsole) {
		CloseHandle(hOutput);
		FreeConsole();
	}
	if (logFile) 
		fclose(logFile);
	logFile = NULL;
}
M_CAPI(void) M_LOG_Init_InConsole() {
	hOutput = GetStdHandle(STD_OUTPUT_HANDLE);
	tofile = true;
	showConsole = true;
}
M_CAPI(void) M_LOG_Init(BOOL showConsole)
{
	::showConsole = showConsole;
	tofile = !IsDebuggerPresent();

	GetModuleFileName(0, logPath, MAX_PATH);
	PathRenameExtension(logPath, (LPWSTR)L".log");

	_wfopen_s(&logFile, logPath, L"w");

	if (showConsole && tofile)
	{
		AllocConsole();
		hOutput = GetStdHandle(STD_OUTPUT_HANDLE);
		HWND hConsole = GetConsoleWindow();
		FILE *file;
		freopen_s(&file, "CONOUT$", "w", stdout);
		SendMessage(hConsole, WM_SETICON, 0, (LPARAM)LoadIcon(hInst, MAKEINTRESOURCE(IDI_ICONCONSOLE)));
		SetConsoleTitle(L"PCMgr Debug Outputs");
	}
}

void M_LOG_LogX(LPWSTR orgFormat, WORD color, wchar_t const* const _Format, va_list arg)
{
	std::wstring format1 = FormatString(orgFormat, _Format);
	std::wstring buf = FormatString(format1.c_str(), arg);
	if (tofile)
	{
		if (!showConsole)
			fwprintf_s(logFile, buf.c_str());
		else {
			SetConsoleTextAttribute(hOutput, color);
			WriteConsoleW(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
			M_LOG_DefConsoleTextColor();
		}
	}
	else OutputDebugString((LPWSTR)buf.c_str());
}
void M_LOG_LogX_WithFunAndLine(LPWSTR orgFormat, LPSTR fileName, LPSTR funName, INT lineNumber, WORD color, wchar_t const* const _Format, va_list arg)
{
	std::wstring format1 = FormatString(orgFormat, _Format, fileName, lineNumber, funName);
	std::wstring buf = FormatString(format1.c_str(), arg);
	if (tofile)
	{
		if (!showConsole)
			fwprintf_s(logFile, buf.c_str());
		else {
			SetConsoleTextAttribute(hOutput, color);
			WriteConsoleW(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
			M_LOG_DefConsoleTextColor();
		}
	}
	else OutputDebugString((LPWSTR)buf.c_str());
}

M_CAPI(void) M_LOG_LogErr(wchar_t const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX(L" [ERR] %s \n", FOREGROUND_INTENSITY | FOREGROUND_RED, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_LogWarn(wchar_t const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX(L"[WARN] %s \n", FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_GREEN, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_LogInfo(wchar_t const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX(L"[INFO] %s \n", FOREGROUND_INTENSITY | FOREGROUND_GREEN | FOREGROUND_BLUE, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_LogText(WORD color, wchar_t const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	std::wstring buf = FormatString(_Format, arg);
	if (tofile)
	{
		if (!showConsole)
			fwprintf_s(logFile, buf.c_str());
		else {
			SetConsoleTextAttribute(hOutput, color);
			WriteConsoleW(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
			M_LOG_DefConsoleTextColor();
		}
	}
	else OutputDebugString((LPWSTR)buf.c_str());
	va_end(arg);
}
M_CAPI(void) M_LOG_Log(wchar_t const* const _Format, ...)
{
#ifdef _DEBUG
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX(L" [LOG] %s \n", FOREGROUND_INTENSITY | FOREGROUND_RED |
		FOREGROUND_GREEN |
		FOREGROUND_BLUE, _Format, arg);
	va_end(arg);
#endif
}

M_CAPI(void) M_LOG_LogErr_WithFunAndLine(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX_WithFunAndLine(L" [ERR] %s \n  [At] %S (%d) %S\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_LogWarn_WithFunAndLine(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX_WithFunAndLine(L"[WARN] %s \n  [At] %S (%d) %S\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_GREEN, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_LogInfo_WithFunAndLine(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX_WithFunAndLine(L"[INFO] %s \n  [At] %S (%d) %S\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_BLUE, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_Log_WithFunAndLine(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...)
{
#ifdef _DEBUG
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX_WithFunAndLine(L" [LOG] %s \n  [At] %S (%d) %S\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED |
		FOREGROUND_GREEN |
		FOREGROUND_BLUE, _Format, arg);
	va_end(arg);
#endif
}


M_CAPI(void) M_LOG_Error_ForceFile(const wchar_t* format, ...)
{
	va_list arg;
	va_start(arg, format);
	std::wstring format1 = FormatString(L" [ERR] %s \n", format);
	std::wstring buf = FormatString(format1.c_str(), arg);
	fwprintf_s(logFile, buf.c_str());
	va_end(arg);
}
M_CAPI(void) M_LOG_Warning_ForceFile(const wchar_t* format, ...)
{
	va_list arg;
	va_start(arg, format);
	std::wstring format1 = FormatString(L"[WARN] %s \n", format);
	std::wstring buf = FormatString(format1.c_str(), arg);
	fwprintf_s(logFile, buf.c_str());
	va_end(arg);
}
M_CAPI(void) M_LOG_Info_ForceFile(const wchar_t* format, ...)
{
	va_list arg;
	va_start(arg, format);
	std::wstring format1 = FormatString(L"[INFO] %s \n", format);
	std::wstring buf = FormatString(format1.c_str(), arg);

	fwprintf_s(logFile, buf.c_str());

	va_end(arg);
}
M_CAPI(void) M_LOG_Str_ForceFile(const wchar_t* format, ...)
{
#ifdef _DEBUG
	va_list arg;
	va_start(arg, format);
	std::wstring format1 = FormatString(L" [LOG] %s \n", format);
	std::wstring buf = FormatString(format1.c_str(), arg);
	fwprintf_s(logFile, buf.c_str());
	va_end(arg);
#endif
}

void M_LOG_LogXA(LPCSTR orgFormat, WORD color, char const* const _Format, va_list arg)
{
	std::string format1 = FormatString(orgFormat, _Format);
	std::string buf = FormatString(format1.c_str(), arg);
	if (tofile)
	{
		if (!showConsole)
			fprintf_s(logFile, buf.c_str());
		else {
			SetConsoleTextAttribute(hOutput, color);
			WriteConsoleA(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
			M_LOG_DefConsoleTextColor();
		}
	}
	else OutputDebugStringA((LPCSTR)buf.c_str());
}
void M_LOG_LogX_WithFunAndLineA(LPCSTR orgFormat, LPSTR fileName, LPSTR funName, INT lineNumber, WORD color, char const* const _Format, va_list arg)
{
	std::string format1 = FormatString(orgFormat, _Format, fileName, lineNumber, funName);
	std::string buf = FormatString(format1.c_str(), arg);
	if (tofile)
	{
		if (!showConsole)
			fprintf_s(logFile, buf.c_str());
		else {
			SetConsoleTextAttribute(hOutput, color);
			WriteConsoleA(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
			M_LOG_DefConsoleTextColor();
		}
	}
	else OutputDebugStringA((LPCSTR)buf.c_str());
}

M_CAPI(void) M_LOG_LogErrA(char const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogXA(" [ERR] %s \n", FOREGROUND_INTENSITY | FOREGROUND_RED, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_LogWarnA(char const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogXA("[WARN] %s \n", FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_GREEN, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_LogInfoA(char const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogXA("[INFO] %s \n", FOREGROUND_INTENSITY | FOREGROUND_GREEN | FOREGROUND_BLUE, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_LogTextA(WORD color, char const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	std::string buf = FormatString(_Format, arg);
	if (tofile)
	{
		if (!showConsole)
			fprintf_s(logFile, buf.c_str());
		else {
			SetConsoleTextAttribute(hOutput, color);
			WriteConsoleA(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
			M_LOG_DefConsoleTextColor();
		}
	}
	else OutputDebugStringA((LPCSTR)buf.c_str());
	va_end(arg);
}
M_CAPI(void) M_LOG_LogA(char const* const _Format, ...)
{
#ifdef _DEBUG
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogXA(" [LOG] %s \n", FOREGROUND_INTENSITY | FOREGROUND_RED |
		FOREGROUND_GREEN |
		FOREGROUND_BLUE, _Format, arg);
	va_end(arg);
#endif
}

M_CAPI(void) M_LOG_LogErr_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX_WithFunAndLineA(" [ERR] %s \n  [At] %s (%d) %s\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_LogWarn_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX_WithFunAndLineA("[WARN] %s \n  [At] %s (%d) %s\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_GREEN, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_LogInfo_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX_WithFunAndLineA("[INFO] %s \n  [At] %s (%d) %s\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_BLUE, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_Log_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...)
{
#ifdef _DEBUG
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX_WithFunAndLineA(" [LOG] %s \n  [At] %s (%d) %s\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED |
		FOREGROUND_GREEN |
		FOREGROUND_BLUE, _Format, arg);
	va_end(arg);
#endif
}

M_CAPI(void) M_LOG_Error_ForceFileA(const char* format, ...)
{
	va_list arg;
	va_start(arg, format);
	std::string format1 = FormatString(" [ERR] %s \n", format);
	std::string buf = FormatString(format1.c_str(), arg);
	fprintf_s(logFile, buf.c_str());
	va_end(arg);
}
M_CAPI(void) M_LOG_Warning_ForceFileA(const char* format, ...)
{
	va_list arg;
	va_start(arg, format);
	std::string format1 = FormatString("[WARN] %s \n", format);
	std::string buf = FormatString(format1.c_str(), arg);
	fprintf_s(logFile, buf.c_str());
	va_end(arg);
}
M_CAPI(void) M_LOG_Info_ForceFileA(const char* format, ...)
{
	va_list arg;
	va_start(arg, format);
	std::string format1 = FormatString("[INFO] %s \n", format);
	std::string buf = FormatString(format1.c_str(), arg);

	fprintf_s(logFile, buf.c_str());

	va_end(arg);
}
M_CAPI(void) M_LOG_Str_ForceFileA(const char* format, ...)
{
#ifdef _DEBUG
	va_list arg;
	va_start(arg, format);
	std::string format1 = FormatString(" [LOG] %s \n", format);
	std::string buf = FormatString(format1.c_str(), arg);
	fprintf_s(logFile, buf.c_str());
	va_end(arg);
#endif
}

M_CAPI(void) M_LOG_Auto_ForMono(const char * log_domain, const char * log_level, const char * message, BOOL fatal, void * user_data)
{
	bool exit = false;

	if (MStrEqualA(log_level, "error"))
		M_LOG_LogErrA("(%s) %s", log_domain, message);
	else 	if (MStrEqualA(log_level, "warning"))
		M_LOG_LogWarnA("(%s) %s", log_domain, message);
	else 	if (MStrEqualA(log_level, "info"))
		M_LOG_LogInfoA("(%s) %s", log_domain, message);
	else M_LOG_LogA("(%s) %s", log_domain, message);

	if (fatal || MStrEqualA(log_level, "error"))
		if (MessageBoxA(NULL, message, "Mono Error", MB_ICONEXCLAMATION | MB_YESNO) == IDYES)
			ExitProcess(-1);
}
		

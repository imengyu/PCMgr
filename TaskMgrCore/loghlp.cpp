#include "stdafx.h"
#include "loghlp.h"
#include <stdarg.h>
#include <stdio.h>
#include "StringHlp.h"
#include "resource.h"

bool tofile = false;
bool showConsole = false;

FILE *logFile = NULL;
HANDLE hOutput = NULL;

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
	if (logFile) fclose(logFile);
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
#if _X64_
	_wfopen_s(&logFile, L"PCMgr64.log", L"wb");
#else
	_wfopen_s(&logFile, L"PCMgr32.log", L"wb");
#endif

	if (showConsole && tofile)
	{
		AllocConsole();
		hOutput = GetStdHandle(STD_OUTPUT_HANDLE);
		HWND hConsole = GetConsoleWindow();
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
	std::wstring format1 = FormatString(orgFormat, fileName, funName, lineNumber, _Format);
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
	M_LOG_LogX_WithFunAndLine(L" [ERR:%hs:%hs(%d)] %s \n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_LogWarn_WithFunAndLine(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX_WithFunAndLine(L"[WARN:%hs:%hs(%d)] %s \n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_GREEN, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_LogInfo_WithFunAndLine(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX_WithFunAndLine(L"[INFO:%hs:%hs(%d)] %s \n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_BLUE, _Format, arg);
	va_end(arg);
}
M_CAPI(void) M_LOG_Log_WithFunAndLine(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...)
{
#ifdef _DEBUG
	va_list arg;
	va_start(arg, _Format);
	M_LOG_LogX_WithFunAndLine(L" [LOG:%hs:%hs(%d)] %s \n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED |
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

#include "stdafx.h"
#include "loghlp.h"
#include <stdarg.h>
#include <stdio.h>
#include "StringHlp.h"

bool tofile = false;
bool showConsole = false;

FILE *logFile = NULL;
HANDLE hOutput = NULL;

M_CAPI(void) M_LOG_Close()
{
	if (hOutput || showConsole) {
		CloseHandle(hOutput);
		FreeConsole();
	}
	if (logFile) fclose(logFile);
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

	if (showConsole)
	{
		AllocConsole();
		hOutput = GetStdHandle(STD_OUTPUT_HANDLE);
		SetConsoleTitle(L"PCMgr Debug Outputs");
	}
}

M_CAPI(void) M_LOG_Error(const wchar_t* format, ...)
{
	va_list arg;	
	va_start(arg, format);
	std::wstring format1 = FormatString(L" [ERR] %s \n", format);
	std::wstring buf = FormatString(format1.c_str(), arg);
	if (tofile)
	{
		if (!showConsole)
			fwprintf_s(logFile, buf.c_str());
		else {
			SetConsoleTextAttribute(hOutput, FOREGROUND_INTENSITY | FOREGROUND_RED);
			WriteConsoleW(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
		}
	}
	else OutputDebugString((LPWSTR)buf.c_str());
	va_end(arg);
}
M_CAPI(void) M_LOG_Warning(const wchar_t* format, ...)
{
	va_list arg;
	va_start(arg, format);
	std::wstring format1 = FormatString(L"[WARN] %s \n", format);
	std::wstring buf = FormatString(format1.c_str(), arg);	
	if (tofile)
	{
		if (!showConsole)
			fwprintf_s(logFile, buf.c_str());
		else {
			SetConsoleTextAttribute(hOutput, FOREGROUND_INTENSITY | FOREGROUND_GREEN);
			WriteConsoleW(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
		}
	}
	else OutputDebugString((LPWSTR)buf.c_str());
	va_end(arg);
}
M_CAPI(void) M_LOG_Info(const wchar_t* format, ...)
{
	va_list arg;
	va_start(arg, format);
	std::wstring format1 = FormatString(L"[INFO] %s \n", format);
	std::wstring buf = FormatString(format1.c_str(), arg);
	if (tofile)
	{
		if (!showConsole)
			fwprintf_s(logFile, buf.c_str());
		else {
			SetConsoleTextAttribute(hOutput, FOREGROUND_INTENSITY | FOREGROUND_BLUE);
			WriteConsoleW(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
		}
	}
	else OutputDebugString((LPWSTR)buf.c_str());			
	va_end(arg);
}
M_CAPI(void) M_LOG_Str(const wchar_t* format, ...)
{
#ifdef _DEBUG
	va_list arg;
	va_start(arg, format);
	std::wstring format1 = FormatString(L" [LOG] %s \n", format);
	std::wstring buf = FormatString(format1.c_str(), arg);
	if (tofile)
	{
		if (!showConsole)
		fwprintf_s(logFile, buf.c_str());
		else {
			SetConsoleTextAttribute(hOutput, FOREGROUND_INTENSITY);
			WriteConsoleW(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
		}
	}
	else OutputDebugString((LPWSTR)buf.c_str());		
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

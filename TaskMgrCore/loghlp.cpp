#include "stdafx.h"
#include "loghlp.h"
#include "cmdhlp.h"
#include <stdarg.h>
#include <stdio.h>
#include "StringHlp.h"
#include "resource.h"
#include "mapphlp.h"
#include <Shlwapi.h>

bool tofile = false;
bool showConsole = false;
bool enableFileLog = false;

FILE *logFile = NULL;
HANDLE hOutput = NULL;
WCHAR logPath[MAX_PATH];

extern HINSTANCE hInst;

#ifdef _DEBUG
LogLevel currentLogLevel = LogLevel::LogLevDebug;
#else
LogLevel currentLogLevel = LogLevel::LogLevError;
#endif

static BOOL ConsoleHandlerRoutine(_In_ DWORD dwCtrlType)
{
	switch (dwCtrlType)
	{
	case CTRL_BREAK_EVENT:
		M_LOG_CloseConsole(FALSE);
		return TRUE;
	case CTRL_CLOSE_EVENT:
		M_LOG_CloseConsole(TRUE);
		return TRUE;
	}
	return FALSE;
}

void M_LOG_FocusConsoleWindow() {
	if (showConsole) {
		HWND hConsole = GetConsoleWindow();
		MAppWorkCall3(213, hConsole, 0);
	}
}
void M_LOG_DefConsoleTextColor() {
	if (hOutput) SetConsoleTextAttribute(hOutput, FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_BLUE);
}
void M_LOG_CreateConsole()
{
	if (!AllocConsole()) {
		LogErr2(L"AllocConsole failed : %d", GetLastError());
		return;
	}

	hOutput = GetStdHandle(STD_OUTPUT_HANDLE);
	SetConsoleCtrlHandler((PHANDLER_ROUTINE)ConsoleHandlerRoutine, TRUE);

	FILE *fileIn;
	FILE *file;
	FILE *fileErr;
	freopen_s(&fileIn, "CONIN$", "r", stdin);
	freopen_s(&fileErr, "CONOUT$", "w", stderr);
	freopen_s(&file, "CONOUT$", "w", stdout);

	HWND hConsole = GetConsoleWindow();
	SendMessage(hConsole, WM_SETICON, 0, (LPARAM)LoadIcon(hInst, MAKEINTRESOURCE(IDI_ICONCONSOLE)));
	SetConsoleTitle(L"PCMgr Debug Console");
	HMENU menu = GetSystemMenu(GetConsoleWindow(), FALSE);
	EnableMenuItem(menu, SC_CLOSE, MF_GRAYED | MF_DISABLED);

	printf_s("You Can enter \"?\" or \"help\" to show command helps\nEnter \"exit\" to close Console window and back to Main app.\n");

	M_LOG_PrintColorTextW(FOREGROUND_INTENSITY | FOREGROUND_GREEN | FOREGROUND_BLUE,
		L"\nIf you close this window directly then the whole app will exit.\n\n");

	MStartRunCmdThread();
}
void M_LOG_CloseConsole(BOOL callFormCloseEvent, BOOL callFormConsoleApp)
{

	if (!callFormCloseEvent)
		FreeConsole();

	FILE *file;
	freopen_s(&file, "NUL", "w", stdout);
	freopen_s(&file, "NUL", "w", stderr);
	freopen_s(&file, "NUL", "r", stdin);

	MStopRunCmdThread();

	showConsole = false;
}
bool M_LOG_TryPushEnterCursur()
{
	CHAR buf[1];
	CONSOLE_SCREEN_BUFFER_INFO cci = { 0 };
	COORD cord = { 0 };
	DWORD readBytes = 0;

	GetConsoleScreenBufferInfo(hOutput, &cci);
	memcpy_s(&cord, sizeof(cord), &cci.dwCursorPosition, sizeof(cord));

	if (cord.X > 0) cord.X--;

	ReadConsoleOutputCharacterA(hOutput, buf, 1, cord, &readBytes);
	if (buf[0] == '>') {
		putchar('\b');
		return true;
	}
	return false;
}

CRITICAL_SECTION cs_log;

void M_LOG_Close_InConsole() {
	if (hOutput || showConsole)
		M_LOG_CloseConsole(FALSE, TRUE);
	if (logFile) fclose(logFile);
	logFile = NULL;
}
void M_LOG_Init_InConsole() {
	hOutput = GetStdHandle(STD_OUTPUT_HANDLE);
	tofile = true;
	showConsole = true;
}
void M_LOG_Init() {
	InitializeCriticalSection(&cs_log);//初始化临界区
}
void M_LOG_Destroy() {
	DeleteCriticalSection(&cs_log);//删除临界区
}

M_CAPI(void) M_LOG_Close()
{
	if (hOutput || showConsole)
		M_LOG_CloseConsole(FALSE);
	if (logFile) fclose(logFile);
	logFile = NULL;
}
M_CAPI(void) M_LOG_Init(BOOL showConsole, BOOL enableFileLog)
{
	::enableFileLog = enableFileLog;
	::showConsole = showConsole;

	if (enableFileLog) {
		GetModuleFileName(0, logPath, MAX_PATH);
		PathRenameExtension(logPath, (LPWSTR)L".log");
		_wfopen_s(&logFile, logPath, L"w");
		tofile = true;
	}
	else tofile = false;

	if (showConsole)
		M_LOG_CreateConsole();
}
M_CAPI(void) M_LOG_SetLogLevel(LogLevel level)
{
	currentLogLevel = level;
}
M_CAPI(LogLevel) M_LOG_GetLogLevel(int l)
{
	return currentLogLevel;
}
M_CAPI(void) M_LOG_DisableLog() {
	currentLogLevel = LogLevel::LogLevDisabled;
}

M_CAPI(void) M_LOG_PrintColorTextA(WORD color, char const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	if (!showConsole)
	{
		std::string buf = FormatString(_Format, arg);
		SetConsoleTextAttribute(hOutput, color);
		WriteConsoleA(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
		M_LOG_DefConsoleTextColor();
	}
	va_end(arg);
}
M_CAPI(void) M_LOG_PrintColorTextW(WORD color, wchar_t const* const _Format, ...)
{
	va_list arg;
	va_start(arg, _Format);
	if (showConsole)
	{
		std::wstring buf = FormatString(_Format, arg);
		SetConsoleTextAttribute(hOutput, color);
		WriteConsoleW(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
		M_LOG_DefConsoleTextColor();
	}
	va_end(arg);
}

void M_LOG_LogXW(LPWSTR orgFormat, WORD color, wchar_t const* const _Format, va_list arg)
{
	if (currentLogLevel == LogLevDisabled)return;

	EnterCriticalSection(&cs_log);

	std::wstring format1 = FormatString(orgFormat, _Format);
	std::wstring buf = FormatString(format1.c_str(), arg);

	if (!showConsole) {
		if (tofile)fwprintf_s(logFile, buf.c_str());
		else  OutputDebugString((LPWSTR)buf.c_str());	
	}
	else {
		bool needRePrintCur = M_LOG_TryPushEnterCursur();
		SetConsoleTextAttribute(hOutput, color);
		WriteConsole(hOutput, buf.c_str(), buf.size() + 1, NULL, 0);
		M_LOG_DefConsoleTextColor();
		if (needRePrintCur) printf(">");
	}

	LeaveCriticalSection(&cs_log);
}
void M_LOG_LogX_WithFunAndLineW(LPWSTR orgFormat, LPSTR fileName, LPSTR funName, INT lineNumber, WORD color, wchar_t const* const _Format, va_list arg)
{
	if (currentLogLevel == LogLevDisabled)return;

	EnterCriticalSection(&cs_log);

	std::wstring format1 = FormatString(orgFormat, _Format, fileName, lineNumber, funName);
	std::wstring buf = FormatString(format1.c_str(), arg);

	if (!showConsole) {
		if (tofile)fwprintf_s(logFile, buf.c_str());
		else  OutputDebugString((LPWSTR)buf.c_str());
	}
	else {
		bool needRePrintCur = M_LOG_TryPushEnterCursur();
		SetConsoleTextAttribute(hOutput, color);
		WriteConsole(hOutput, buf.c_str(), buf.size() + 1, NULL, 0);
		M_LOG_DefConsoleTextColor();
		if (needRePrintCur) printf(">");
	}

	LeaveCriticalSection(&cs_log);
}

M_CAPI(void) M_LOG_LogErrW(wchar_t const* const _Format, ...)
{
	if (currentLogLevel <= LogLevError) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogXW(L" [ERR] %s \n", FOREGROUND_INTENSITY | FOREGROUND_RED, _Format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_LogWarnW(wchar_t const* const _Format, ...)
{
	if (currentLogLevel <= LogLevWarn) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogXW(L"[WARN] %s \n", FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_GREEN, _Format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_LogInfoW(wchar_t const* const _Format, ...)
{
	if (currentLogLevel <= LogLevDebug) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogXW(L"[INFO] %s \n", FOREGROUND_INTENSITY | FOREGROUND_GREEN | FOREGROUND_BLUE, _Format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_LogW(wchar_t const* const _Format, ...)
{
	if (currentLogLevel <= LogLevDebug) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogXW(L" [LOG] %s \n", FOREGROUND_INTENSITY | FOREGROUND_RED |
			FOREGROUND_GREEN |
			FOREGROUND_BLUE, _Format, arg);
		va_end(arg);
	}
}

M_CAPI(void) M_LOG_LogErr_WithFunAndLineW(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...)
{
	if (currentLogLevel <= LogLevError) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogX_WithFunAndLineW(L" [ERR] %s \n  [At] %S (%d) %S\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED, _Format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_LogWarn_WithFunAndLineW(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...)
{
	if (currentLogLevel <= LogLevWarn) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogX_WithFunAndLineW(L"[WARN] %s \n  [At] %S (%d) %S\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_GREEN, _Format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_LogInfo_WithFunAndLineW(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...)
{
	if (currentLogLevel <= LogLevDebug) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogX_WithFunAndLineW(L"[INFO] %s \n  [At] %S (%d) %S\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_BLUE, _Format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_Log_WithFunAndLineW(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...)
{
	if (currentLogLevel <= LogLevDebug) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogX_WithFunAndLineW(L" [LOG] %s \n  [At] %S (%d) %S\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED |
			FOREGROUND_GREEN |
			FOREGROUND_BLUE, _Format, arg);
		va_end(arg);
	}
}

void M_LOG_LogXA(LPCSTR orgFormat, WORD color, char const* const _Format, va_list arg)
{
	if (currentLogLevel == LogLevDisabled)return;

	EnterCriticalSection(&cs_log);

	std::string format1 = FormatString(orgFormat, _Format);
	std::string buf = FormatString(format1.c_str(), arg);

	if (!showConsole) {
		if (tofile)fprintf_s(logFile, buf.c_str());
		else OutputDebugStringA((LPCSTR)buf.c_str());
	}
	else {
		bool needRePrintCur = M_LOG_TryPushEnterCursur();
		SetConsoleTextAttribute(hOutput, color);
		WriteConsoleA(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
		M_LOG_DefConsoleTextColor();
		if (needRePrintCur) printf(">");
	}

	LeaveCriticalSection(&cs_log);
}
void M_LOG_LogX_WithFunAndLineA(LPCSTR orgFormat, LPSTR fileName, LPSTR funName, INT lineNumber, WORD color, char const* const _Format, va_list arg)
{
	if (currentLogLevel == LogLevDisabled)return;

	EnterCriticalSection(&cs_log);

	std::string format1 = FormatString(orgFormat, _Format, fileName, lineNumber, funName);
	std::string buf = FormatString(format1.c_str(), arg);
	if (!showConsole) {
		if (tofile)fprintf_s(logFile, buf.c_str());
		else OutputDebugStringA((LPCSTR)buf.c_str());
	}
	else {
		bool needRePrintCur = M_LOG_TryPushEnterCursur();
		SetConsoleTextAttribute(hOutput, color);
		WriteConsoleA(hOutput, buf.c_str(), static_cast<DWORD>(buf.size()), NULL, NULL);
		M_LOG_DefConsoleTextColor();
		if (needRePrintCur) printf(">");
	}

	LeaveCriticalSection(&cs_log);
}

M_CAPI(void) M_LOG_LogErrA(char const* const _Format, ...)
{
	if (currentLogLevel <= LogLevError) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogXA(" [ERR] %s \n", FOREGROUND_INTENSITY | FOREGROUND_RED, _Format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_LogWarnA(char const* const _Format, ...)
{
	if (currentLogLevel <= LogLevWarn) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogXA("[WARN] %s \n", FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_GREEN, _Format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_LogInfoA(char const* const _Format, ...)
{
	if (currentLogLevel <= LogLevDebug) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogXA("[INFO] %s \n", FOREGROUND_INTENSITY | FOREGROUND_GREEN | FOREGROUND_BLUE, _Format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_LogA(char const* const _Format, ...)
{
	if (currentLogLevel <= LogLevDebug) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogXA(" [LOG] %s \n", FOREGROUND_INTENSITY | FOREGROUND_RED |
			FOREGROUND_GREEN |
			FOREGROUND_BLUE, _Format, arg);
		va_end(arg);
	}
}

M_CAPI(void) M_LOG_LogErr_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...)
{
	if (currentLogLevel <= LogLevError) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogX_WithFunAndLineA(" [ERR] %s \n  [At] %s (%d) %s\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED, _Format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_LogWarn_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...)
{
	if (currentLogLevel <= LogLevWarn) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogX_WithFunAndLineA("[WARN] %s \n  [At] %s (%d) %s\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_GREEN, _Format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_LogInfo_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...)
{
	if (currentLogLevel <= LogLevDebug) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogX_WithFunAndLineA("[INFO] %s \n  [At] %s (%d) %s\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_BLUE, _Format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_Log_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...)
{
	if (currentLogLevel <= LogLevDebug) {
		va_list arg;
		va_start(arg, _Format);
		M_LOG_LogX_WithFunAndLineA(" [LOG] %s \n  [At] %s (%d) %s\n", fileName, funName, lineNumber, FOREGROUND_INTENSITY | FOREGROUND_RED |
			FOREGROUND_GREEN |
			FOREGROUND_BLUE, _Format, arg);
		va_end(arg);
	}
}

void M_LOG_X_ForceFileW(const wchar_t* type, const wchar_t* format, va_list arg)
{
	if (!enableFileLog) return;

	std::wstring format1;
	if (type != NULL) format1 = FormatString(L"%s %s \n", type, format);
	else format1 = FormatString(L" %s \n", type, format);

	std::wstring buf = FormatString(format1.c_str(), arg);
	fwprintf_s(logFile, buf.c_str());
}
void M_LOG_X_ForceFileA(const char* type, const char* format, va_list arg)
{
	if (!enableFileLog) return;

	std::string format1;
	if (type != NULL) format1 = FormatString("%s %s \n", type, format);
	else format1 = FormatString(" %s \n", type, format);

	std::string buf = FormatString(format1.c_str(), arg);
	fprintf_s(logFile, buf.c_str());
}

M_CAPI(void) M_LOG_Error_ForceFileW(const wchar_t* format, ...)
{
	if (currentLogLevel <= LogLevError) {
		va_list arg;
		va_start(arg, format);
		M_LOG_X_ForceFileW(L" [ERR]", format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_Warning_ForceFileW(const wchar_t* format, ...)
{
	if (currentLogLevel <= LogLevWarn) {
		va_list arg;
		va_start(arg, format);
		M_LOG_X_ForceFileW(L"[WARN]", format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_Info_ForceFileW(const wchar_t* format, ...)
{
	if (currentLogLevel <= LogLevDebug) {
		va_list arg;
		va_start(arg, format);
		M_LOG_X_ForceFileW(L"[INFO]", format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_Str_ForceFileW(const wchar_t* format, ...)
{
	if (currentLogLevel <= LogLevDebug) {
		va_list arg;
		va_start(arg, format);
		M_LOG_X_ForceFileW(NULL, format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_Error_ForceFileA(const char* format, ...)
{
	if (currentLogLevel <= LogLevError) {
		va_list arg;
		va_start(arg, format);
		M_LOG_X_ForceFileA(" [ERR]", format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_Warning_ForceFileA(const char* format, ...)
{
	if (currentLogLevel <= LogLevWarn) {
		va_list arg;
		va_start(arg, format);
		M_LOG_X_ForceFileA("[WARN]", format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_Info_ForceFileA(const char* format, ...)
{
	if (currentLogLevel <= LogLevDebug) {
		va_list arg;
		va_start(arg, format);
		M_LOG_X_ForceFileA("[INFO]", format, arg);
		va_end(arg);
	}
}
M_CAPI(void) M_LOG_Str_ForceFileA(const char* format, ...)
{
	if (currentLogLevel <= LogLevDebug) {
		va_list arg;
		va_start(arg, format);
		M_LOG_X_ForceFileA(NULL, format, arg);
		va_end(arg);
	}
}
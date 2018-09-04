#pragma once
#include "stdafx.h"
#undef _USE_ATTRIBUTES_FOR_SAL
#define _USE_ATTRIBUTES_FOR_SAL 1
#include <sal.h>

//日志记录等级
enum LogLevel {
	LogLevFull,//全部记录
	LogLevDebug,//仅调试输出及以上
	LogLevWarn,//仅警告及错误
	LogLevError,//仅错误
	LogLevDisabled//禁用输出
};


//输出/记录错误
#define LogErr M_LOG_LogErrW
//输出/记录警告
#define LogWarn M_LOG_LogWarnW
//输出/记录消息
#define LogInfo M_LOG_LogInfoW
//输出/记录文字
#define LogText M_LOG_LogTextW
//输出/记录
#define Log M_LOG_LogW

//输出/记录错误到文件
#define FLogErr M_LOG_Error_ForceFileW
//输出/记录警告到文件
#define FLogWarn M_LOG_Warning_ForceFileW
//输出/记录消息到文件
#define FLogInfo M_LOG_Info_ForceFileW
//输出/记录文字到文件
#define FLog M_LOG_Str_ForceFileW

M_CAPI(void) M_LOG_Error_ForceFileA(_Printf_format_string_ const char* szFormat, ...);
M_CAPI(void) M_LOG_Warning_ForceFileA(_Printf_format_string_ const char* szFormat, ...);
M_CAPI(void) M_LOG_Info_ForceFileA(_Printf_format_string_ const char* szFormat, ...);
M_CAPI(void) M_LOG_Str_ForceFileA(_Printf_format_string_ const char* szFormat, ...);
M_CAPI(void) M_LOG_Error_ForceFileW(_Printf_format_string_ const wchar_t* szFormat, ...);
M_CAPI(void) M_LOG_Warning_ForceFileW(_Printf_format_string_ const wchar_t* szFormat, ...);
M_CAPI(void) M_LOG_Info_ForceFileW(_Printf_format_string_ const wchar_t* szFormat, ...);
M_CAPI(void) M_LOG_Str_ForceFileW(_Printf_format_string_ const wchar_t* szFormat, ...);

void M_LOG_Init();
void M_LOG_Destroy();
void M_LOG_FocusConsoleWindow();
void M_LOG_CloseConsole(BOOL callFormCloseEvent, BOOL callFormConsoleApp = FALSE);

//设置 Log 当前记录等级
//   	LogLevFull,        全部记录
//   	LogLevDebug,   仅调试输出及以上
//   	LogLevWarn,     仅警告及错误
//   	LogLevError,     仅错误
//   	LogLevDisabled 禁用输出
M_CAPI(void) M_LOG_SetLogLevelStr(LPCWSTR level);
//设置 Log 当前记录等级
M_CAPI(void) M_LOG_SetLogLevel(LogLevel level);
//获取 Log 当前记录等级
M_CAPI(LogLevel) M_LOG_GetLogLevel(int l);
//禁用 Log 模块
M_CAPI(void) M_LOG_DisableLog();

//关闭 Log 模块（适用于控制台程序）
M_CAPI(void) M_LOG_Close_InConsole();
//关闭 Log 模块
M_CAPI(void) M_LOG_Close();
//初始化 Log 模块（适用于控制台程序）
M_CAPI(void) M_LOG_Init_InConsole();
//初始化 Log 模块（适用于非控制台程序）
//    showConsole：是否显示控制台输出窗口
//    enableFileLog：是否启用文件记录
M_CAPI(void) M_LOG_Init(BOOL showConsole, BOOL enableFileLog = TRUE);

//输出/记录错误并自动记录代码位置
#define LogErr2(format,...) M_LOG_LogErr_WithFunAndLineW(__FILE__,__FUNCTION__,__LINE__,format, __VA_ARGS__)
//输出/记录警告并自动记录代码位置
#define LogWarn2(format,...) M_LOG_LogWarn_WithFunAndLineW(__FILE__,__FUNCTION__,__LINE__,format, __VA_ARGS__)
//输出/记录消息并自动记录代码位置
#define LogInfo2(format,...) M_LOG_LogInfo_WithFunAndLineW(__FILE__,__FUNCTION__,__LINE__,format, __VA_ARGS__)
//输出/记录信息并自动记录代码位置
#define Log2(format,...) M_LOG_Log_WithFunAndLineW(__FILE__,__FUNCTION__,__LINE__,format, __VA_ARGS__)

M_CAPI(void) M_LOG_LogErr_WithFunAndLineW(LPSTR fileName, LPSTR funName, INT lineNumber,  _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);
M_CAPI(void) M_LOG_LogWarn_WithFunAndLineW(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);
M_CAPI(void) M_LOG_LogInfo_WithFunAndLineW(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);
M_CAPI(void) M_LOG_Log_WithFunAndLineW(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);

M_CAPI(void) M_LOG_LogErrW(_In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);
M_CAPI(void) M_LOG_LogWarnW(_In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);
M_CAPI(void) M_LOG_LogInfoW(_In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);
M_CAPI(void) M_LOG_LogW(_In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);

M_CAPI(void) M_LOG_LogErr_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...);
M_CAPI(void) M_LOG_LogWarn_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...);
M_CAPI(void) M_LOG_LogInfo_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...);
M_CAPI(void) M_LOG_Log_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...);

M_CAPI(void) M_LOG_LogErrA(_In_z_ _Printf_format_string_ char const* const _Format, ...);
M_CAPI(void) M_LOG_LogWarnA(_In_z_ _Printf_format_string_ char const* const _Format, ...);
M_CAPI(void) M_LOG_LogInfoA(_In_z_ _Printf_format_string_ char const* const _Format, ...);
M_CAPI(void) M_LOG_LogA(_In_z_ _Printf_format_string_ char const* const _Format, ...);

M_CAPI(void) M_LOG_PrintColorTextA(WORD color, char const* const _Format, ...);
M_CAPI(void) M_LOG_PrintColorTextW(WORD color, wchar_t const* const _Format, ...);

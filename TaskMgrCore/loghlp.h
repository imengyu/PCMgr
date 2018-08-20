#pragma once
#include "stdafx.h"
#undef _USE_ATTRIBUTES_FOR_SAL
#define _USE_ATTRIBUTES_FOR_SAL 1
#include <sal.h>

//输出/记录错误
#define LogErr M_LOG_LogErr
//输出/记录警告
#define LogWarn M_LOG_LogWarn
//输出/记录消息
#define LogInfo M_LOG_LogInfo
//输出/记录文字
#define LogText M_LOG_LogText
//输出/记录
#define Log M_LOG_Log

M_CAPI(void) M_LOG_Error_ForceFileA(_Printf_format_string_ const char* szFormat, ...);
M_CAPI(void) M_LOG_Warning_ForceFileA(_Printf_format_string_ const char* szFormat, ...);
M_CAPI(void) M_LOG_Info_ForceFileA(_Printf_format_string_ const char* szFormat, ...);
M_CAPI(void) M_LOG_Str_ForceFileA(_Printf_format_string_ const char* szFormat, ...);
M_CAPI(void) M_LOG_Error_ForceFile(_Printf_format_string_ const wchar_t* szFormat, ...);
M_CAPI(void) M_LOG_Warning_ForceFile(_Printf_format_string_ const wchar_t* szFormat, ...);
M_CAPI(void) M_LOG_Info_ForceFile(_Printf_format_string_ const wchar_t* szFormat, ...);
M_CAPI(void) M_LOG_Str_ForceFile(_Printf_format_string_ const wchar_t* szFormat, ...);
M_CAPI(void) M_LOG_Auto_ForMono(const char *log_domain, const char *log_level, const char *message, BOOL fatal, void *user_data);

//关闭 Log 模块
M_CAPI(void) M_LOG_Close();
//初始化 Log 模块（适用于控制台程序）
M_CAPI(void) M_LOG_Init_InConsole();
//初始化 Log 模块（适用于非控制台程序）
//    showConsole：是否显示控制台输出窗口
M_CAPI(void) M_LOG_Init(BOOL showConsole);

//输出/记录错误并自动记录代码位置
#define LogErr2(format,...) M_LOG_LogErr_WithFunAndLine(__FILE__,__FUNCTION__,__LINE__,format, __VA_ARGS__)
//输出/记录警告并自动记录代码位置
#define LogWarn2(format,...) M_LOG_LogWarn_WithFunAndLine(__FILE__,__FUNCTION__,__LINE__,format, __VA_ARGS__)
//输出/记录消息并自动记录代码位置
#define LogInfo2(format,...) M_LOG_LogInfo_WithFunAndLine(__FILE__,__FUNCTION__,__LINE__,format, __VA_ARGS__)
//输出/记录信息并自动记录代码位置
#define Log2(format,...) M_LOG_Log_WithFunAndLine(__FILE__,__FUNCTION__,__LINE__,format, __VA_ARGS__)

M_CAPI(void) M_LOG_LogErr_WithFunAndLine(LPSTR fileName, LPSTR funName, INT lineNumber,  _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);
M_CAPI(void) M_LOG_LogWarn_WithFunAndLine(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);
M_CAPI(void) M_LOG_LogInfo_WithFunAndLine(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);
M_CAPI(void) M_LOG_Log_WithFunAndLine(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);

M_CAPI(void) M_LOG_LogErr(_In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);
M_CAPI(void) M_LOG_LogWarn(_In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);
M_CAPI(void) M_LOG_LogInfo(_In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);
M_CAPI(void) M_LOG_LogText(WORD color, _Printf_format_string_ wchar_t const* const _Format, ...);
M_CAPI(void) M_LOG_Log(_In_z_ _Printf_format_string_ wchar_t const* const _Format, ...);

M_CAPI(void) M_LOG_LogErr_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...);
M_CAPI(void) M_LOG_LogWarn_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...);
M_CAPI(void) M_LOG_LogInfo_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...);
M_CAPI(void) M_LOG_Log_WithFunAndLineA(LPSTR fileName, LPSTR funName, INT lineNumber, _In_z_ _Printf_format_string_ char const* const _Format, ...);

M_CAPI(void) M_LOG_LogErrA(_In_z_ _Printf_format_string_ char const* const _Format, ...);
M_CAPI(void) M_LOG_LogWarnA(_In_z_ _Printf_format_string_ char const* const _Format, ...);
M_CAPI(void) M_LOG_LogInfoA(_In_z_ _Printf_format_string_ char const* const _Format, ...);
M_CAPI(void) M_LOG_LogTextA(WORD color, _Printf_format_string_ char const* const _Format, ...);
M_CAPI(void) M_LOG_LogA(_In_z_ _Printf_format_string_ char const* const _Format, ...);

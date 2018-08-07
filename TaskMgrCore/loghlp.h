#pragma once
#include "stdafx.h"
#undef _USE_ATTRIBUTES_FOR_SAL
#define _USE_ATTRIBUTES_FOR_SAL 1
#include <sal.h>

#define LogErr M_LOG_LogErr
#define LogWarn M_LOG_LogWarn
#define LogInfo M_LOG_LogInfo
#define LogText M_LOG_LogText
#define Log M_LOG_Log

M_CAPI(void) M_LOG_Error_ForceFile(_Printf_format_string_ const wchar_t* wzFormat, ...);
M_CAPI(void) M_LOG_Warning_ForceFile(_Printf_format_string_ const wchar_t* wzFormat, ...);
M_CAPI(void) M_LOG_Info_ForceFile(_Printf_format_string_ const wchar_t* wzFormat, ...);
M_CAPI(void) M_LOG_Str_ForceFile(_Printf_format_string_ const wchar_t* wzFormat, ...);
M_CAPI(void) M_LOG_Close();
M_CAPI(void) M_LOG_Init_InConsole();

M_CAPI(void) M_LOG_Init(BOOL showConsole);

#define LogErr2(format,...) M_LOG_LogErr_WithFunAndLine(__FILE__,__FUNCTION__,__LINE__,format, __VA_ARGS__)
#define LogWarn2(format,...) M_LOG_LogWarn_WithFunAndLine(__FILE__,__FUNCTION__,__LINE__,format, __VA_ARGS__)
#define LogInfo2(format,...) M_LOG_LogInfo_WithFunAndLine(__FILE__,__FUNCTION__,__LINE__,format, __VA_ARGS__)
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

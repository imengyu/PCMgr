#pragma once
#include "stdafx.h"

#define LogErr M_LOG_Error
#define LogWarn M_LOG_Warning
#define LogInfo M_LOG_Info
#define Log M_LOG_Str
#define FLogErr M_LOG_Error_ForceFile
#define FLogWarn M_LOG_Warning_ForceFile
#define FLogInfo M_LOG_Info_ForceFile
#define FLog M_LOG_Str_ForceFile

M_CAPI(void) M_LOG_Error_ForceFile(const wchar_t* format, ...);
M_CAPI(void) M_LOG_Warning_ForceFile(const wchar_t* format, ...);
M_CAPI(void) M_LOG_Info_ForceFile(const wchar_t* format, ...);
M_CAPI(void) M_LOG_Str_ForceFile(const wchar_t* format, ...);
M_CAPI(void) M_LOG_Close();
M_CAPI(void) M_LOG_Error(const wchar_t* format, ...);
M_CAPI(void) M_LOG_Warning(const wchar_t* format, ...);
M_CAPI(void) M_LOG_Info(const wchar_t* format, ...);
M_CAPI(void) M_LOG_Str(const wchar_t * format, ...);

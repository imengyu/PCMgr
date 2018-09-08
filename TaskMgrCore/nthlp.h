#pragma once
#include "stdafx.h"
#include "ntdef.h"

//获取进程信息
NTSTATUS MQueryProcessVariableSize(HANDLE ProcessHandle, PROCESSINFOCLASS ProcessInformationClass, PVOID * Buffer);
//NTSTATUS 转为字符串
M_CAPI(LPWSTR) MNtStatusToStr(NTSTATUS status);

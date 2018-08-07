#pragma once
#include "stdafx.h"
#include "ntdef.h"

M_CAPI(NTSTATUS) MQueryProcessVariableSize(HANDLE ProcessHandle, PROCESSINFOCLASS ProcessInformationClass, PVOID * Buffer);

M_CAPI(LPWSTR) MNtstatusToStr(NTSTATUS status);

#pragma once
#include "Driver.h"

NTSTATUS KxGetFunctions(ULONG parm);

ULONG_PTR KxSearchFeatureCodeForAddress(ULONG_PTR StartAddress, PUCHAR FeatureCode, int FeatureCodeSize, int Search_MaxLength);

ULONG_PTR KxGetPspTerminateThreadByPointerAddress();

ULONG_PTR KxGetPspExitThreadAddress();

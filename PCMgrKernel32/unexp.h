#pragma once
#include "Driver.h"

NTSTATUS KxGetFunctions(ULONG parm);

ULONG_PTR KxSearchFeatureCodeForAddress(ULONG_PTR StartAddress, PUCHAR FeatureCode, int FeatureCodeSize, int Search_MaxLength);

ULONG_PTR KxGetPspTerminateThreadByPointerAddress78();

ULONG_PTR KxGetPspExitThreadAddress78();

ULONG_PTR KxGetPspTerminateThreadByPointerAddress10(ULONG ver);

ULONG_PTR KxGetPspExitThreadAddress10(ULONG ver);

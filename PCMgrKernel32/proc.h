#pragma once
#include "Driver.h"

NTSTATUS KxTerminateThread(HANDLE hThread, ULONG exitCode);
NTSTATUS KxTerminateProcess(HANDLE hProcess, ULONG exitCode);
NTSTATUS KxTerminateProcessWithPidAndApc(ULONG_PTR pid, ULONG exitCode);
NTSTATUS KxTerminateThreadWithTidAndApc(ULONG_PTR tid, ULONG exitCode);
NTSTATUS KxTerminateThreadWithTid(ULONG_PTR tid, ULONG exitCode);
NTSTATUS KxTerminateProcessWithPid(ULONG_PTR pid, ULONG exitCode);
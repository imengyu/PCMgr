#pragma once
#include "Driver.h"

#define PS_PROCESS_FLAGS_PROCESS_DELETE         0x00000008UL // Delete process has been issued
#define PS_CROSS_THREAD_FLAGS_TERMINATED           0x00000001UL

#define PS_SET_BITS(Flags, Flag) \
    RtlInterlockedSetBitsDiscardReturn (Flags, Flag)
#define PS_TEST_SET_BITS(Flags, Flag) \
    RtlInterlockedSetBits (Flags, Flag)

NTSTATUS KxTerminateProcessTest(ULONG_PTR PID);
NTSTATUS KxTerminateProcessByZero(ULONG_PTRPID);
VOID KxForceResumeThread(PETHREAD Thread);
NTSTATUS KxTerminateThreadWithTid(ULONG_PTR tid, ULONG exitCode, BOOLEAN useapc);
NTSTATUS KxTerminateProcessWithPid(ULONG_PTR pid, ULONG exitCode, BOOLEAN usepst, BOOLEAN useapc);
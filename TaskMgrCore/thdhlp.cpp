#include "stdafx.h"
#include "ntdef.h"
#include "thdhlp.h"
#include "prochlp.h"
#include <Psapi.h>

extern NtSuspendThreadFun NtSuspendThread;
extern NtResumeThreadFun NtResumeThread;
extern NtTerminateThreadFun NtTerminateThread;
extern NtOpenThreadFun NtOpenThread;
extern NtQueryInformationThreadFun NtQueryInformationThread;
extern RtlNtStatusToDosErrorFun RtlNtStatusToDosError;
extern RtlGetLastWin32ErrorFun RtlGetLastWin32Error;
extern NtQuerySystemInformationFun NtQuerySystemInformation;

M_API NTSTATUS MOpenThreadNt(DWORD dwId, PHANDLE pLandle, DWORD dwPId)
{
	HANDLE hThread;
	OBJECT_ATTRIBUTES ObjectAttributes;
	CLIENT_ID ClientId;

	ObjectAttributes.Length = sizeof(OBJECT_ATTRIBUTES);
	ObjectAttributes.RootDirectory = NULL;
	ObjectAttributes.ObjectName = NULL;
	ObjectAttributes.Attributes = OBJ_KERNEL_HANDLE | OBJ_CASE_INSENSITIVE;
	ObjectAttributes.SecurityDescriptor = NULL;
	ObjectAttributes.SecurityQualityOfService = NULL;

	ClientId.UniqueThread = ((PVOID)(ULONG_PTR)dwId);
	ClientId.UniqueProcess = ((PVOID)(ULONG_PTR)dwPId);

	DWORD NtStatus = NtOpenThread(
		&hThread,
		THREAD_ALL_ACCESS,
		&ObjectAttributes,
		&ClientId);

	if (NtStatus == 0) {
		*pLandle = hThread;
		return 0;
	}
	else {
		return 0;
	}
}
M_API PTEB MGetThreadPeb(HANDLE hThread) {
	THREAD_BASIC_INFORMATION tbi;
	if (NtQueryInformationThread(hThread, ThreadBasicInformation, &tbi, sizeof(tbi), NULL) == STATUS_SUCCESS)
		return tbi.TebBaseAddress;
	return NULL;
}
M_API PVOID MGetThreadWin32StartAddress(HANDLE hThread) {
	PVOID startaddr = 0;
	if (NtQueryInformationThread(hThread, ThreadQuerySetWin32StartAddress, &startaddr, sizeof(startaddr), NULL) == STATUS_SUCCESS)
		return startaddr;
	return NULL;
}

M_API NTSTATUS MTerminateThreadNt(HANDLE handle)
{
	return NtTerminateThread(handle, 0);
}
M_API NTSTATUS MResumeThreadNt(HANDLE handle)
{
	ULONG count = 0;
	return NtResumeThread(handle, &count);
}
M_API NTSTATUS MSuspendThreadNt(HANDLE handle)
{
	ULONG count = 0;
	return NtSuspendThread(handle, &count);
}



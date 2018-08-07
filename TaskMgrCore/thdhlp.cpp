#include "stdafx.h"
#include "ntdef.h"
#include "thdhlp.h"
#include "prochlp.h"
#include <Psapi.h>

extern ZwSuspendThreadFun ZwSuspendThread;
extern ZwResumeThreadFun ZwResumeThread;
extern ZwTerminateThreadFun ZwTerminateThread;
extern ZwOpenThreadFun ZwOpenThread;
extern ZwQueryInformationThreadFun ZwQueryInformationThread;
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

	DWORD NtStatus = ZwOpenThread(
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

M_API NTSTATUS MTerminateThreadNt(HANDLE handle)
{
	return ZwTerminateThread(handle, 0);
}
M_API NTSTATUS MResumeThreadNt(HANDLE handle)
{
	ULONG count = 0;
	return ZwResumeThread(handle, &count);
}
M_API NTSTATUS MSuspendThreadNt(HANDLE handle)
{
	ULONG count = 0;
	return ZwSuspendThread(handle, &count);
}

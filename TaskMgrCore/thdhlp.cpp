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

extern PSYSTEM_PROCESSES current_system_process;

M_API NTSTATUS MGetThreadState(ULONG ulPID, ULONG ulTID)
{
	bool done=false;
	for (PSYSTEM_PROCESSES p = current_system_process; !done;
	p = PSYSTEM_PROCESSES(PCHAR(p) + p->NextEntryDelta))
	{
		if (static_cast<DWORD>((ULONG_PTR)p->ProcessId) == ulPID)
		{
			for (ULONG i = 0; i<p->ThreadCount; i++)
			{
				SYSTEM_THREADS systemThread = p->Threads[i];
				if ((ULONG)(ULONG_PTR)systemThread.ClientId.UniqueThread == ulTID)          
					return systemThread.ThreadState;
			}
		}
		done = p->NextEntryDelta == 0;
	}
	return 0;
}

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
	DWORD rs = ZwTerminateThread(handle, 0);
	return rs;
}

M_API NTSTATUS MResumeThreadNt(HANDLE handle)
{
	ULONG count = 0;
	DWORD rs = ZwResumeThread(handle, &count);
	return rs;
}

M_API NTSTATUS MSuspendThreadNt(HANDLE handle)
{
	ULONG count = 0;
	DWORD rs = ZwSuspendThread(handle, &count);
	return rs;
}

M_API BOOL MGetThreadInfoNt(DWORD tid, int i, LPWSTR *str)
{
	BOOL rs = FALSE;
	THREAD_BASIC_INFORMATION    tbi;
	PVOID                       startaddr;
	LONG                        status;
	HANDLE                      thread, process;

#ifdef _X86_
	thread = OpenThread(THREAD_ALL_ACCESS, 0, tid);
#else
	thread = OpenThread(THREAD_ALL_ACCESS, 0, static_cast<DWORD>(tid));
#endif

	//OpenThreadNt(tid, &thread);
	if ((thread) == NULL) {
		CloseHandle(thread);
		return FALSE;
	}

	status = ZwQueryInformationThread(thread, ThreadBasicInformation, &tbi, sizeof(tbi), NULL);
	if (i == 1)
	{
		status = ZwQueryInformationThread(thread, ThreadQuerySetWin32StartAddress, &startaddr, sizeof(startaddr), NULL);
		if (status == STATUS_SUCCESS) {
			if (MOpenProcessNt(static_cast<DWORD>((ULONG_PTR)tbi.ClientId.UniqueProcess), &process) == STATUS_SUCCESS)
			{
				TCHAR* modname = new TCHAR[260];
				K32GetMappedFileNameW(process, startaddr, modname, 260);
				*str = modname;
				rs = TRUE;
			}
			CloseHandle(process);
		}
		else {
			TCHAR* err = new TCHAR[18];
			wsprintf(err, L"ERROR:0x%08X", status);
			*str = err;
		}
	}
	else if (i == 2)
	{
		status = ZwQueryInformationThread(thread, ThreadQuerySetWin32StartAddress, &startaddr, sizeof(startaddr), NULL);
		if (status == STATUS_SUCCESS) {
			TCHAR* modname1 = new TCHAR[0x100];
			wsprintf(modname1, L"0x%08X", startaddr);
			*str = modname1;
			rs = TRUE;
		}
		else {
			TCHAR* err = new TCHAR[18];
			wsprintf(err, L"ERROR:0x%08X", status);
			*str = err;
		}
	}
	else if (i == 3)
	{
		if (status == STATUS_SUCCESS) {
			TCHAR* modname1 = new TCHAR[0x100];
			wsprintf(modname1, L"0x%08X", tbi.TebBaseAddress);
			*str = modname1;
			rs = TRUE;
		}
		else {
			TCHAR* err = new TCHAR[18];
			wsprintf(err, L"ERROR:0x%08X", status);
			*str = err;
		}
	}
	else if (i == 4) {
		LARGE_INTEGER count;
		status = ZwQueryInformationThread(thread, ThreadPerformanceCount, &count, sizeof(count), NULL);
		if (status == STATUS_SUCCESS)
		{
			TCHAR* modname1 = new TCHAR[0x100];
			wsprintf(modname1, L"%ld", count.LowPart);
			*str = modname1;
			rs = TRUE;
		}
		else {
			TCHAR* err = new TCHAR[18];
			wsprintf(err, L"ERROR:0x%08X", status);
			*str = err; 
			rs = TRUE;
		}
	}

	CloseHandle(thread);
	return rs;
}


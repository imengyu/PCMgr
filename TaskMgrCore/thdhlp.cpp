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

M_API DWORD MGetThreadState(ULONG ulPID, ULONG ulTID)
{
#ifndef _AMD64_
	ULONG n = 0x100;
	PSYSTEM_PROCESSES sp = new SYSTEM_PROCESSES[n];
	while (NtQuerySystemInformation(5, sp, n*sizeof(SYSTEM_PROCESSES), 0) == STATUS_INFO_LENGTH_MISMATCH)
	{
		delete[] sp;
		sp = new SYSTEM_PROCESSES[n = n * 2];
	}
	bool done = false;

	//遍历进程列表
	for (PSYSTEM_PROCESSES p = sp; !done;
	p = PSYSTEM_PROCESSES(PCHAR(p) + p->NextEntryDelta))
	{
		if (p->ProcessId == ulPID)
		{
			for (ULONG i = 0; i<p->ThreadCount; i++)
			{
				SYSTEM_THREADS systemThread = p->Threads[i];
				if ((ULONG)systemThread.ClientId.UniqueThread == ulTID) //找到线程              
				{
					delete[] sp;
					return systemThread.ThreadState;
				}
			}
		}
		done = p->NextEntryDelta == 0;
	}

	delete[] sp;
	return 0;
#else
	return 0;
#endif
}

M_API DWORD MOpenThreadNt(DWORD dwId, PHANDLE pLandle, DWORD dwPId)
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

	ClientId.UniqueThread = ((PVOID)dwId);
	ClientId.UniqueProcess = ((PVOID)dwPId);

	DWORD NtStatus = ZwOpenThread(
		&hThread,
		THREAD_ALL_ACCESS,
		&ObjectAttributes,
		&ClientId);

	if (NtStatus == 0) {
		*pLandle = hThread;
		return 1;
	}
	else if (NtStatus == 0xC0000008) return -1;
	else {
		return 0;
	}
}

M_API DWORD MTerminateThreadNt(HANDLE handle)
{
	DWORD rs = ZwTerminateThread(handle, 0);
	if (rs == 0) {
		WaitForSingleObject(handle, 1000);
		return TRUE;
	}
	else
		return rs;
}

M_API DWORD MResumeThreadNt(HANDLE handle)
{
	ULONG count = 0;
	DWORD rs = ZwResumeThread(handle, &count);
	if (rs == 0)
		return TRUE;
	return rs;
}

M_API DWORD MSuspendThreadNt(HANDLE handle)
{
	ULONG count = 0;
	DWORD rs = ZwSuspendThread(handle, &count);
	if (rs == 0)
		return TRUE;
	return rs;
}

M_API BOOL MGetThreadInfoNt(DWORD tid, int i, LPWSTR *str)
{
	THREAD_BASIC_INFORMATION    tbi;
	PVOID                       startaddr;
	LONG                        status;
	HANDLE                      thread, process;

	thread = OpenThread(THREAD_ALL_ACCESS, NULL, tid);
	//OpenThreadNt(tid, &thread);
	if (thread == NULL) {
		CloseHandle(thread);
		return FALSE;
	}

	status = ZwQueryInformationThread(thread, ThreadBasicInformation, &tbi, sizeof(tbi), NULL);
	if (i == 1)
	{
		status = ZwQueryInformationThread(thread, ThreadQuerySetWin32StartAddress, &startaddr, sizeof(startaddr), NULL);
		if (status == 0) {
#ifdef WIN32
			process = OpenProcess(PROCESS_ALL_ACCESS, 0, (DWORD)tbi.ClientId.UniqueProcess);
#else
			process = OpenProcess(PROCESS_ALL_ACCESS, 0, (DWORD)(LONG)tbi.ClientId.UniqueProcess);
#endif
			if (process != NULL)
			{
				TCHAR* modname = new TCHAR[260];
				K32GetMappedFileNameW(process, startaddr, modname, 260);
				*str = modname;
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
		if (status == 0) {
			TCHAR* modname1 = new TCHAR[0x100];
			wsprintf(modname1, L"0x%08X", startaddr);
			*str = modname1;
		}
		else {
			TCHAR* err = new TCHAR[18];
			wsprintf(err, L"ERROR:0x%08X", status);
			*str = err;
		}
	}
	else if (i == 3)
	{
		if (status == 0) {
			TCHAR* modname1 = new TCHAR[0x100];
			wsprintf(modname1, L"0x%08X", tbi.TebBaseAddress);
			*str = modname1;
		}
		else {
			TCHAR* err = new TCHAR[18];
			wsprintf(err, L"ERROR:0x%08X", status);
			*str = err;
		}
	}
	else if (i == 4) {
		LONG count = 0;
		status = ZwQueryInformationThread(thread, ThreadPerformanceCount, &count, sizeof(count), NULL);
		if (status)
		{
			TCHAR* modname1 = new TCHAR[0x100];
			wsprintf(modname1, L"%ld", count);
			*str = modname1;
		}
	}

	CloseHandle(thread);
	return TRUE;
}


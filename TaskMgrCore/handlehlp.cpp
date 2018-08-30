#include "stdafx.h"
#include "handlehlp.h"
#include "ntdef.h"
#include "suact.h"
#include "msup.h"
#include "prochlp.h"
#include "loghlp.h"
#include "syshlp.h"
#include "StringHlp.h"
#include "thdhlp.h"

extern NtQueryInformationProcessFun NtQueryInformationProcess;
extern NtQuerySystemInformationFun NtQuerySystemInformation;
extern NtQueryObjectFun NtQueryObject;
extern NtDuplicateObjectFun NtDuplicateObject;
extern NtCloseFun NtClose;

M_CAPI(NTSTATUS) M_EH_DuplicateHandleFromProcess(HANDLE ProcessHandle,	PHANDLE Handle, HANDLE HandleValue,	ACCESS_MASK DesiredAccess)
{
	return NtDuplicateObject(
		ProcessHandle,
		HandleValue,
		NtCurrentProcess(),
		Handle,
		DesiredAccess,
		0,
		0
	);
}
M_CAPI(NTSTATUS) M_EH_GetObjectName(HANDLE HandleDup, WCHAR *ObjectName, SIZE_T ObjectNameBufferSize)
{
	NTSTATUS status;
	POBJECT_NAME_INFORMATION buffer;
	ULONG bufferSize;
	ULONG attempts = 8;

	bufferSize = 0x200;
	buffer = (POBJECT_NAME_INFORMATION)MAlloc(bufferSize);

	// A loop is needed because the I/O subsystem likes to give us the wrong return lengths...
	do
	{
		status = NtQueryObject(
			HandleDup,
			ObjectNameInformation,
			buffer,
			bufferSize,
			&bufferSize
		);

		if (status == STATUS_BUFFER_OVERFLOW || status == STATUS_INFO_LENGTH_MISMATCH ||
			status == STATUS_BUFFER_TOO_SMALL)
		{
			MFree(buffer);
			buffer = (POBJECT_NAME_INFORMATION)MAlloc(bufferSize);
		}
		else
		{
			break;
		}
	} while (--attempts);

	if (NT_SUCCESS(status))
	{
		if (buffer->Name.Buffer != NULL)
			wcscpy_s(ObjectName, ObjectNameBufferSize, buffer->Name.Buffer);
	}

	MFree(buffer);

	return status;
}
M_CAPI(NTSTATUS) M_EH_GetThreadHandleValue(HANDLE ProcessHandle, HANDLE Handle, WCHAR *ObjectName, SIZE_T ObjectNameBufferSize)
{
	THREAD_BASIC_INFORMATION tbi = { 0 };
	HANDLE dupHandle;
	NTSTATUS status = NtDuplicateObject(
		ProcessHandle,
		Handle,
		NtCurrentProcess(),
		&dupHandle,
		THREAD_QUERY_INFORMATION,
		0,
		0
	);
	if (NT_SUCCESS(status)) {
		status = MGetThreadBasicInformation(dupHandle, &tbi);
		if (NT_SUCCESS(status)) {
#ifdef _AMD64_
			swprintf_s(ObjectName, ObjectNameBufferSize, L"Process : %llu Thread : %llu", (ULONG_PTR)tbi.ClientId.UniqueProcess, (ULONG_PTR)tbi.ClientId.UniqueThread);
#else
			swprintf_s(ObjectName, ObjectNameBufferSize, L"Process : %u Thread : %u", (ULONG_PTR)tbi.ClientId.UniqueProcess, (ULONG_PTR)tbi.ClientId.UniqueThread);
#endif
		}
		NtClose(dupHandle);
	}
	return status;
}
M_CAPI(NTSTATUS) M_EH_GetProcessHandleValue(HANDLE ProcessHandle, HANDLE Handle, WCHAR *ObjectName, SIZE_T ObjectNameBufferSize)
{
	NTSTATUS status = 0;
	HANDLE dupHandle;
	PROCESS_BASIC_INFORMATION basicInfo;
	status = NtDuplicateObject(
		ProcessHandle,
		Handle,
		NtCurrentProcess(),
		&dupHandle,
		PROCESS_QUERY_INFORMATION,
		0,
		0
	);
	if (NT_SUCCESS(status)) 
	{
		status = MGetProcessBasicInformation(dupHandle, &basicInfo);
		if (NT_SUCCESS(status))
		{
			WCHAR strImagePath[260] = { 0 };
			status = MGetProcessImageFileName(dupHandle, strImagePath, 260);
#ifdef _AMD64_
			swprintf_s(ObjectName, ObjectNameBufferSize, L"(%llu) %s", basicInfo.UniqueProcessId, strImagePath);
#else
			swprintf_s(ObjectName, ObjectNameBufferSize, L"(%u) %s", basicInfo.UniqueProcessId, strImagePath);
#endif
		}
		//else swprintf_s(ObjectName, ObjectNameBufferSize, L"NTSTATUS : 0x%08X", status);
		NtClose(dupHandle);
	}
	return status;
}

M_CAPI(BOOL) M_EH_CloseHandle(DWORD pid, LPVOID handleValue)
{
	BOOL rs = FALSE;
	HANDLE hFile = (HANDLE)handleValue;
	HANDLE hProcess;
	NTSTATUS status = 0;
	if (M_SU_OpenProcess(pid, &hProcess, &status)) {
		HANDLE hDup = 0;
		status = M_EH_DuplicateHandleFromProcess(hProcess, &hDup, hFile, SEMAPHORE_MODIFY_STATE);
		if (hDup) rs = NtClose(hDup);
		else LogErr(L"CloseHandleWithProcess failed in DuplicateHandle (NTSTATUS : 0x%08X) PID %d HANDLE : 0x%08X", status, pid, hFile);
		NtClose(hProcess);
	}
	else LogErr(L"CloseHandleWithProcess failed in OpenProcess (NTSTATUS : 0x%08X) PID %d HANDLE : 0x%08X", status, pid, hFile);
	return rs;
}
M_CAPI(BOOL) M_EH_GetHandleTypeName(HANDLE HandleDup, LPWSTR buffer, size_t bufsize)
{
	if (HandleDup)
	{
		NTSTATUS status = 0;
		POBJECT_TYPE_INFORMATION pHandleType = (POBJECT_TYPE_INFORMATION)MAlloc(sizeof(OBJECT_TYPE_INFORMATION));
		ULONG outLength = 0;
		if (NtQueryObject(HandleDup, ObjectTypeInformation, pHandleType, outLength, &outLength) == STATUS_INFO_LENGTH_MISMATCH)
		{
			if (outLength == 0)
			{
				LogErr(L"NtQueryObject failed and return size 0", status);
				return FALSE;
			}

			MFree(pHandleType);
			pHandleType = (POBJECT_TYPE_INFORMATION)MAlloc(outLength);
			memset(pHandleType, 0, outLength);
		}
	
		if (pHandleType)
		{
			status = NtQueryObject(HandleDup, ObjectTypeInformation, pHandleType, outLength, &outLength);
			if (!NT_SUCCESS(status))
			{
				if (status != STATUS_INVALID_HANDLE)
					LogErr(L"NtQueryObject failed 0x%08X", status);
				return FALSE;
			}

			if (pHandleType->TypeName.Length != NULL && pHandleType->TypeName.Buffer != NULL)
			{
				wcscpy_s(buffer, bufsize, pHandleType->TypeName.Buffer);
				return FALSE;
			}
		}
	}
	return FALSE;
}
M_CAPI(BOOL) M_EH_EnumProcessHandles(DWORD pid, EHCALLBACK callback)
{
	if (!callback) return FALSE;

	HANDLE hProcess;
	NTSTATUS status = 0;
	status = MOpenProcessNt(pid, &hProcess);
	if (!NT_SUCCESS(status))
	{
		LogErr(L"OpenProcess failed 0x%08X", status);
		return FALSE;
	}
	else
	{
		ULONG outLength = 0;
		PSYSTEM_HANDLE_INFORMATION pSystemHandleInfos = (PSYSTEM_HANDLE_INFORMATION)MAlloc(sizeof(SYSTEM_HANDLE_INFORMATION));

		if (NtQuerySystemInformation(SystemHandleInformation, pSystemHandleInfos, sizeof(SYSTEM_HANDLE_INFORMATION), &outLength) == STATUS_INFO_LENGTH_MISMATCH)
		{
			MFree(pSystemHandleInfos);
			if (outLength == 0)
			{
				LogErr2(L"NtQuerySystemInformation failed and return size 0", status);
				return FALSE;
			}

			pSystemHandleInfos = (PSYSTEM_HANDLE_INFORMATION)MAlloc(outLength);
			memset(pSystemHandleInfos, 0, outLength);
		}
		if (pSystemHandleInfos)
		{
			status = NtQuerySystemInformation(SystemHandleInformation, pSystemHandleInfos, outLength, &outLength);
			if (!NT_SUCCESS(status))
			{
				LogErr2(L"NtQuerySystemInformation failed 0x%08X", status);
				return FALSE;
			}

			ULONG handlesCount = pSystemHandleInfos->NumberOfHandles;
			for (ULONG i = 0; i < handlesCount; i++)
			{
				SYSTEM_HANDLE_TABLE_ENTRY_INFO info = pSystemHandleInfos->Handles[i];
				if (info.UniqueProcessId != (USHORT)pid) continue;

				WCHAR handlePath[MAX_PATH];
				WCHAR handleTypeName[64];
				HANDLE hDup = 0;

				memset(handlePath, 0, sizeof(handlePath));
				memset(handleTypeName, 0, sizeof(handleTypeName));

				//if (hProcess == NtCurrentProcess()) {
				//	hDup = (HANDLE)info.HandleValue; 
				//	status = STATUS_SUCCESS;
				//}
				//else 
					status = M_EH_DuplicateHandleFromProcess(hProcess, &hDup, (HANDLE)info.HandleValue, 1);
				if (NT_SUCCESS(status))
				{
					M_EH_GetHandleTypeName(hDup, handleTypeName, 64);

					if (MStrEqual(handleTypeName, L"File"))
						MGetNtPathFromHandle(hDup, handlePath, MAX_PATH);
					else if (MStrEqual(handleTypeName, L"Process"))
						M_EH_GetProcessHandleValue(hProcess, (HANDLE)info.HandleValue, handlePath, MAX_PATH);
					else if (MStrEqual(handleTypeName, L"Thread"))
						M_EH_GetThreadHandleValue(hProcess, (HANDLE)info.HandleValue, handlePath, MAX_PATH);
					else M_EH_GetObjectName(hDup, handlePath, MAX_PATH);
				}

				WCHAR handleAddress[32];
				WCHAR handleObjAddress[32];
				swprintf_s(handleAddress, L"0x%08X", info.HandleValue);
#ifndef _AMD64_
				swprintf_s(handleObjAddress, L"0x%08X", (ULONG_PTR)info.Object);
#else
				swprintf_s(handleObjAddress, L"0x%I64X", (ULONG_PTR)info.Object);
#endif
				callback((LPVOID)info.HandleValue, handleTypeName, handlePath, handleAddress, handleObjAddress, 0, info.ObjectTypeIndex);
			}

			MFree(pSystemHandleInfos);
		}
	}
	NtClose(hProcess);
	return TRUE;
}



#include "stdafx.h"
#include "handlehlp.h"
#include "ntdef.h"
#include "suact.h"
#include "msup.h"
#include "prochlp.h"
#include "loghlp.h"
#include "syshlp.h"

extern NtQueryInformationProcessFun NtQueryInformationProcess;
extern NtQuerySystemInformationFun NtQuerySystemInformation;
extern NtQueryObjectFun NtQueryObject;

M_CAPI(BOOL) M_EH_CloseHandle(DWORD pid, LPVOID handleValue)
{
	BOOL rs = FALSE;
	HANDLE hFile = (HANDLE)handleValue;
	HANDLE hProcess;
	NTSTATUS status = 0;
	if (M_SU_OpenProcess(pid, &hProcess, &status)) {
		HANDLE hDup = 0;
		BOOL b = DuplicateHandle(hProcess, hFile, GetCurrentProcess(),
			&hDup, DUPLICATE_SAME_ACCESS, FALSE, DUPLICATE_CLOSE_SOURCE);
		if (hDup) rs = MCloseHandle(hDup);
		else LogErr(L"CloseHandleWithProcess failed in DuplicateHandle (%d) PID %d HANDLE : 0x%08X", GetLastError(), pid, hFile);
		MCloseHandle(hProcess);
	}
	else LogErr(L"CloseHandleWithProcess failed (%d) PID %d HANDLE : 0x%08X", GetLastError(), pid, hFile);
	return rs;
}
M_CAPI(BOOL) M_EH_GetHandleTypeName(PSYSTEM_HANDLE_TABLE_ENTRY_INFO pSystemHandle, LPWSTR buffer, size_t bufsize)
{
	if (pSystemHandle)
	{
		NTSTATUS status = 0;
		POBJECT_TYPE_INFORMATION pHandleType = (POBJECT_TYPE_INFORMATION)MAlloc(sizeof(OBJECT_TYPE_INFORMATION));
		ULONG outLength = 0;
		if (NtQueryObject((HANDLE)pSystemHandle->HandleValue, ObjectTypeInformation, pHandleType, outLength, &outLength) == STATUS_INFO_LENGTH_MISMATCH)
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
			status = NtQueryObject((HANDLE)pSystemHandle->HandleValue, ObjectTypeInformation, pHandleType, outLength, &outLength);
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
M_CAPI(BOOL) M_EH_GetHandleTypeName2(PPROCESS_HANDLE_TABLE_ENTRY_INFO pProcessHandle, LPWSTR buffer, size_t bufsize)
{
	if (pProcessHandle)
	{
		NTSTATUS status = 0;
		POBJECT_TYPE_INFORMATION pHandleType = (POBJECT_TYPE_INFORMATION)MAlloc(sizeof(OBJECT_TYPE_INFORMATION));
		ULONG outLength = 0;
		if (NtQueryObject((HANDLE)pProcessHandle->HandleValue, ObjectTypeInformation, pHandleType, outLength, &outLength) == STATUS_INFO_LENGTH_MISMATCH)
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
			status = NtQueryObject((HANDLE)pProcessHandle->HandleValue, ObjectTypeInformation, pHandleType, outLength, &outLength);
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

	if (0/* && MGetWindowsWin8Upper()*/)
	{
		ULONG outLength = 0;
		PPROCESS_HANDLE_SNAPSHOT_INFORMATION pProcessHandleSnapshotInfo = (PPROCESS_HANDLE_SNAPSHOT_INFORMATION)MAlloc(sizeof(PROCESS_HANDLE_SNAPSHOT_INFORMATION));
		
		if (NtQueryInformationProcess(hProcess, ProcessHandleInformation, pProcessHandleSnapshotInfo, sizeof(PROCESS_HANDLE_SNAPSHOT_INFORMATION), &outLength) == STATUS_INFO_LENGTH_MISMATCH)
		{
			MFree(pProcessHandleSnapshotInfo);
			if (outLength == 0)
			{
				LogErr2(L"NtQueryInformationProcess failed and return size 0", status);
				return FALSE;
			}
			pProcessHandleSnapshotInfo = (PPROCESS_HANDLE_SNAPSHOT_INFORMATION)MAlloc(outLength);
			memset(pProcessHandleSnapshotInfo, 0, outLength);
		}
		if (pProcessHandleSnapshotInfo)
		{
			ULONG handlesCount = pProcessHandleSnapshotInfo->NumberOfHandles;
			for (ULONG i = 0; i < handlesCount; i++)
			{
				_PROCESS_HANDLE_TABLE_ENTRY_INFO info = pProcessHandleSnapshotInfo->Handles[i];

				HANDLE hDup = 0;
				BOOL b = DuplicateHandle(hProcess, (HANDLE)info.HandleValue, GetCurrentProcess(),
					&hDup, DUPLICATE_SAME_ACCESS, FALSE, DUPLICATE_SAME_ACCESS);

				WCHAR handleTypeName[64];
				memset(handleTypeName, 0, sizeof(handleTypeName));
				M_EH_GetHandleTypeName2(&info, handleTypeName, 64);

				WCHAR handlePath[MAX_PATH];
				memset(handlePath, 0, sizeof(handlePath));

				if (b) MGetNtPathFromHandle(hDup, handlePath, MAX_PATH);

				WCHAR handleAddress[32];
				WCHAR handleObjAddress[32] = { 0 };
				swprintf_s(handleAddress, L"0x%08X", info.HandleValue);
				callback((LPVOID)info.HandleValue, handleTypeName, handlePath, handleAddress, handleObjAddress, info.PointerCount, info.ObjectTypeIndex);
			}
		}
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

				HANDLE hDup = 0;
				BOOL b = DuplicateHandle(hProcess, (HANDLE)info.HandleValue, GetCurrentProcess(),
					&hDup, DUPLICATE_SAME_ACCESS, FALSE, DUPLICATE_SAME_ACCESS);

				WCHAR handleTypeName[64];
				memset(handleTypeName, 0, sizeof(handleTypeName));
				M_EH_GetHandleTypeName(&info, handleTypeName, 64);

				WCHAR handlePath[MAX_PATH];
				memset(handlePath, 0, sizeof(handlePath));

				if (b) MGetNtPathFromHandle(hDup, handlePath, MAX_PATH);

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
	MCloseHandle(hProcess);
	return TRUE;
}



#include "stdafx.h"
#include "handlehlp.h"
#include "ntdef.h"
#include "suact.h"
#include "prochlp.h"
#include "loghlp.h"

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
		POBJECT_TYPE_INFORMATION pHandleType = (POBJECT_TYPE_INFORMATION)malloc(sizeof(OBJECT_TYPE_INFORMATION));
		ULONG outLength = 0;
		if (NtQueryObject((HANDLE)pSystemHandle->HandleValue, ObjectTypeInformation, pHandleType, outLength, &outLength) == STATUS_INFO_LENGTH_MISMATCH)
		{
			if (outLength == 0)
			{
				LogErr(L"NtQueryObject failed and return size 0", status);
				return FALSE;
			}

			free(pHandleType);
			pHandleType = (POBJECT_TYPE_INFORMATION)malloc(outLength);
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
M_CAPI(BOOL) MEnumProcessHandles(DWORD pid, EHCALLBACK callback)
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

	PSYSTEM_HANDLE_INFORMATION pSystemHandleInfos = (PSYSTEM_HANDLE_INFORMATION)malloc(sizeof(SYSTEM_HANDLE_INFORMATION));
	ULONG outLength = 0;
	if (NtQuerySystemInformation(SystemHandleInformation, pSystemHandleInfos, sizeof(SYSTEM_HANDLE_INFORMATION), &outLength) == STATUS_INFO_LENGTH_MISMATCH)
	{
		if (outLength == 0)
		{
			LogErr(L"NtQuerySystemInformation failed and return size 0", status);
			return FALSE;
		}
		free(pSystemHandleInfos);

		pSystemHandleInfos = (PSYSTEM_HANDLE_INFORMATION)malloc(outLength);
		memset(pSystemHandleInfos, 0, outLength);
	}

	if (pSystemHandleInfos)
	{
		status = NtQuerySystemInformation(SystemHandleInformation, pSystemHandleInfos, outLength, &outLength);
		if (!NT_SUCCESS(status))
		{
			LogErr(L"NtQuerySystemInformation failed 0x%08X", status);
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

			if(b) MGetNtPathFromHandle(hDup, handlePath, MAX_PATH);

			WCHAR handleAddress[32];
			swprintf_s(handleAddress, L"0x%08X", info.HandleValue);
			WCHAR handleObjAddress[32];
			swprintf_s(handleObjAddress, L"0x%08X", (ULONG_PTR)info.Object);

			callback((LPVOID)info.HandleValue, handleTypeName, handlePath, handleAddress, handleObjAddress, info.CreatorBackTraceIndex, info.ObjectTypeIndex);
		}

		free(pSystemHandleInfos);
	}

	MCloseHandle(hProcess);
	return TRUE;
}



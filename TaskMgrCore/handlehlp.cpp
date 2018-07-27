#include "stdafx.h"
#include "handlehlp.h"
#include "ntdef.h"
#include "suact.h"
#include "prochlp.h"
#include "loghlp.h"

extern NtQuerySystemInformationFun NtQuerySystemInformation;
extern NtQueryObjectFun NtQueryObject;

M_CAPI(BOOL) MEnumProcessHandles(DWORD pid, EHCALLBACK callback)
{
	if (callback)
	{
		HANDLE hTest = 0;
		HANDLE hProcess;
		NTSTATUS status = 0;
		status = MOpenProcessNt(pid, &hProcess);
		if (NT_SUCCESS(status))
		{
			PSYSTEM_HANDLE_INFORMATION pSysHandleInformation = new SYSTEM_HANDLE_INFORMATION;
			DWORD size = sizeof(SYSTEM_HANDLE_INFORMATION);
			DWORD needed = 0;

			status = NtQuerySystemInformation(SystemHandleInformation, pSysHandleInformation, size, &needed);
			if (status != STATUS_SUCCESS)
			{
				delete pSysHandleInformation;
				if (needed == 0)
					return FALSE;

				size = needed + 1024;
				pSysHandleInformation = (PSYSTEM_HANDLE_INFORMATION)malloc(size);
				memset(pSysHandleInformation, 0, size);

				status = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS(SystemHandleInformation), pSysHandleInformation, size, &needed);
				if (status != STATUS_SUCCESS)
				{
					delete pSysHandleInformation;
					Log(L"NtQuerySystemInformation failed 0x%08X", status);
					return FALSE;
				}
			}

			ULONG handleCount = pSysHandleInformation->NumberOfHandles;
			for (DWORD i = 0; i < handleCount; i++)
			{
				if (pSysHandleInformation->Handles[i].UniqueProcessId == pid)
				{
					WCHAR strNtPath[MAX_PATH];
					WCHAR strType[MAX_PATH];
					memset(strType, 0, sizeof(strType));
					memset(strNtPath, 0, sizeof(strNtPath));

					HANDLE hDup = (HANDLE)pSysHandleInformation->Handles[i].HandleValue;
					MGetNtPathFromHandle(hDup, strNtPath, MAX_PATH);

					DWORD u32_ReqLength = 0;
					POBJECT_TYPE_INFORMATION lpTypeInfo = (POBJECT_TYPE_INFORMATION)malloc(sizeof(OBJECT_TYPE_INFORMATION));
					memset(lpTypeInfo, 0, sizeof(OBJECT_TYPE_INFORMATION));
					status = NtQueryObject(hDup, ObjectTypeInformation, lpTypeInfo, sizeof(OBJECT_TYPE_INFORMATION), &u32_ReqLength);
					if (status == STATUS_INFO_LENGTH_MISMATCH)
					{
						free(lpTypeInfo);
						lpTypeInfo = (POBJECT_TYPE_INFORMATION)malloc(u32_ReqLength);
						memset(lpTypeInfo, 0, u32_ReqLength);
						NTSTATUS status = NtQueryObject(hDup, ObjectTypeInformation, lpTypeInfo, u32_ReqLength, &u32_ReqLength);
						if (status == STATUS_SUCCESS)
						{
							if (lpTypeInfo->TypeName.Buffer != nullptr)
								wcscpy_s(strType, lpTypeInfo->TypeName.Buffer);
						}
						else wsprintf(strType, L"2:0x%08X", status);
					}
					else if (status != 0xC0000008) {
						wsprintf(strType, L"1:0x%08X", status);
					}
					free(lpTypeInfo);


					WCHAR strAddres[32]; wsprintf(strAddres, L"0x%08X", pSysHandleInformation->Handles[i].HandleValue);
					WCHAR strObjAddres[32]; wsprintf(strObjAddres, L"0x%08X", (PVOID)pSysHandleInformation->Handles[i].Object);

					callback((LPVOID)&pSysHandleInformation->Handles[i], strType, strNtPath, strAddres, strObjAddres, pSysHandleInformation->Handles[i].GrantedAccess, pSysHandleInformation->Handles[i].ObjectTypeIndex);

				}
			}
			delete pSysHandleInformation;
			MCloseHandle(hProcess);
			return TRUE;
		}
		else Log(L"MOpenProcessNt failed 0x%08X", status);
	}
	return FALSE;
}
M_CAPI(BOOL) M_EH_CloseHandle(_SYSTEM_HANDLE_TABLE_ENTRY_INFO *pSystemHandle)
{
	return M_SU_CloseHandleWithProcess(pSystemHandle);
}
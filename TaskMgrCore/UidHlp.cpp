#include "stdafx.h"
#include "UidHlp.h"
#include "loghlp.h"

M_CAPI(int)  MGetCurrentUserSid(PSID*outSid)
{
	void *v1; // ebx
	PSID *v2; // edi
	HANDLE v3; // eax
	signed int v4; // eax
	signed int v5; // esi
	DWORD v6; // ST14_4
	HANDLE v7; // eax
	DWORD v8; // esi
	HANDLE v9; // eax
	HANDLE v10; // eax
	signed int v12; // eax
	bool v13; // sf
	DWORD v14; // ST10_4
	DWORD v15; // ST10_4
	DWORD v16; // eax
	signed int v17; // eax
	bool v18; // sf
	DWORD v19; // ST10_4
	signed int v20; // eax
	bool v21; // sf
	HANDLE v22; // eax
	HANDLE TokenHandle; // [esp+10h] [ebp-8h]
	DWORD TokenInformationLength; // [esp+14h] [ebp-4h]

	TokenHandle = 0;
	v1 = 0;
	v2 = 0;
	TokenInformationLength = 0;
	v3 = GetCurrentProcess();
	if (OpenProcessToken(v3, 8u, &TokenHandle))
		goto LABEL_2;
	v12 = GetLastError();
	v5 = v12;
	v13 = v12 < 0;
	if (v12)
	{
		if (v12 <= 0)
			goto LABEL_25;
		v5 = (unsigned __int16)v12 | 0x80070000;
	}
	else
	{
		v5 = -2147467259;
	}
	v13 = v5 < 0;
LABEL_25:
	if (v13)
	{
		LogErr2(L"OpenProcessToken failed 0x%08x", v5);
		goto LABEL_12;
	}
LABEL_2:
	if (GetTokenInformation(TokenHandle, TokenUser, 0, TokenInformationLength, &TokenInformationLength))
	{
		v5 = 0;
		goto LABEL_15;
	}
	v4 = GetLastError();
	v5 = v4;
	if (v4)
	{
		if (v4 > 0)
			v5 = (unsigned __int16)v4 | 0x80070000;
	}
	else
	{
		v5 = -2147467259;
	}
	if (v5 != -2147024774)
		goto LABEL_12;
	v6 = TokenInformationLength;
	v7 = GetProcessHeap();
	v2 = (PSID *)HeapAlloc(v7, 8u, v6);
	if (!v2)
	{
		v5 = -2147024882;
		LogErr2(L"HeapAlloc failed 0x%08x", -2147024882);
		goto LABEL_15;
	}
	if (!GetTokenInformation(TokenHandle, TokenUser, v2, TokenInformationLength, &TokenInformationLength))
	{
		v17 = GetLastError();
		v5 = v17;
		v18 = v17 < 0;
		if (v17)
		{
			if (v17 <= 0)
				goto LABEL_35;
			v5 = (unsigned __int16)v17 | 0x80070000;
		}
		else
		{
			v5 = -2147467259;
		}
		v18 = v5 < 0;
	LABEL_35:
		if (v18)
		{
			LogErr2(L"GetTokenInformation failed 0x%08x", v5);
			goto LABEL_12;
		}
	}
	v8 = GetLengthSid(*v2);
	v9 = GetProcessHeap();
	v1 = HeapAlloc(v9, 8u, v8);
	if (CopySid(v8, v1, *v2))
	{
		v5 = 0;
	LABEL_11:
		*outSid = v1;
		goto LABEL_12;
	}
	v20 = GetLastError();
	v5 = v20;
	v21 = v20 < 0;
	if (!v20)
	{
		v5 = -2147467259;
		goto LABEL_41;
	}
	if (v20 > 0)
	{
		v5 = (unsigned __int16)v20 | 0x80070000;
	LABEL_41:
		v21 = v5 < 0;
	}
	if (!v21)
		goto LABEL_11;
	LogErr2(L"CopySid failed : %d", v5);
LABEL_12:
	if (v5 < 0 && v1)
	{
		v22 = GetProcessHeap();
		HeapFree(v22, 0, v1);
	}
	if (v2)
	{
		v10 = GetProcessHeap();
		HeapFree(v10, 0, v2);
	}
LABEL_15:
	if (TokenHandle && TokenHandle != (HANDLE)-1)
		CloseHandle(TokenHandle);
	return v5;
}
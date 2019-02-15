#pragma once
#include "stdafx.h"

#define CONVERT_STR_2_GUID(cstr, stGuid) do\
{\
    swscanf_s((const wchar_t*)cstr, L"{%8x-%4x-%4x-%2x%2x-%2x%2x%2x%2x%2x%2x}",\
    &(stGuid.Data1),&(stGuid.Data2),&(stGuid.Data3),\
    &(stGuid.Data4[0]),&(stGuid.Data4[1]),&(stGuid.Data4[2]),&(stGuid.Data4[3]),\
    &(stGuid.Data4[4]),&(stGuid.Data4[5]),&(stGuid.Data4[6]),&(stGuid.Data4[7]));\
}while(0);

M_CAPI(int) MGetCurrentUserSid(PSID * outSid);

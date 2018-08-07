#pragma once
#include "stdafx.h"

M_CAPI(BOOL) M_KDA_Dec(PUCHAR buf, ULONG_PTR startaddress, LPVOID callback, ULONG_PTR size, BOOL x86Orx64);

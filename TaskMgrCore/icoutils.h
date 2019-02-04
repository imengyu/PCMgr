#pragma once
#include "stdafx.h"

M_CAPI(HICON) MGetShieldIcon(VOID);
M_CAPI(HICON) MGetShieldIcon2(VOID);
HBITMAP MCreateBitmap32(HDC hdc, ULONG Width, ULONG Height, PVOID *Bits);
HBITMAP MIconToBitmap(HICON Icon, ULONG Width, ULONG Height);
#pragma once
#include "stdafx.h"

M_CAPI(HKEY) MREG_CLSIDToHKEY(HKEY hRootKey, LPWSTR clsid);

M_CAPI(HKEY) MREG_CLSIDToHKEYInprocServer32(HKEY hRootKey, LPWSTR clsid);

M_CAPI(LPWSTR) MREG_ROOTKEYToStr(HKEY hRootKey);

M_CAPI(BOOL) MREG_DeleteKey(HKEY hRootKey, LPWSTR path);

M_CAPI(BOOL) MREG_DeleteKeyValue(HKEY hRootKey, LPWSTR path, LPWSTR value);

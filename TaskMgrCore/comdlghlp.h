#pragma once
#include "stdafx.h"

M_CAPI(BOOL) MChooseFileSingal(HWND hWnd, LPWSTR startDir, LPWSTR title, LPWSTR fileFilter, LPWSTR fileName, LPWSTR defExt, LPWSTR*strrs, size_t bufsize);
M_CAPI(BOOL) MChooseDir(HWND hWnd, LPWSTR startDir, LPWSTR title, LPWSTR*strrs, size_t bufsize);
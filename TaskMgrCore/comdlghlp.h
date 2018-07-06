#pragma once
#include "stdafx.h"

BOOL ChooseFileSingal(HWND hWnd, LPWSTR startDir, LPWSTR title, LPWSTR fileFilter, LPWSTR fileName, LPWSTR defExt, LPWSTR*strrs, size_t bufsize);
BOOL ChooseDir(HWND hWnd, LPWSTR startDir, LPWSTR title, LPWSTR*strrs, size_t bufsize);
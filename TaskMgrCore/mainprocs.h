#pragma once
#include "stdafx.h"
#include <CommCtrl.h>
#include <Uxtheme.h>

#define IDC_TABMAIN 1003
#define IDC_LISTMAIN 1004

BOOL ShowMainCore(HWND hWnd);
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
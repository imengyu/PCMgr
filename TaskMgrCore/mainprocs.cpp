#include "stdafx.h"
#include "mainprocs.h"
#include "resource.h"

extern HINSTANCE hInst;


void ShowMainCoreStartUp()
{
	WNDCLASSEXW wcex;
	wcex.cbSize = sizeof(WNDCLASSEX);
	wcex.style = CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc = WndProc;
	wcex.cbClsExtra = 0;
	wcex.cbWndExtra = 0;
	wcex.hInstance = hInst;
	wcex.hIcon = LoadIcon(hInst, MAKEINTRESOURCE(IDI_ICONAPP));
	wcex.hCursor = LoadCursor(nullptr, IDC_ARROW);
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	wcex.lpszMenuName = NULL;
	wcex.lpszClassName = L"PCMGRWINDOW";
	wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_ICONAPP));
	RegisterClassExW(&wcex);
}

BOOL ShowMainCore(HWND hWndParent)
{
	HWND hWnd;
	hWnd = CreateWindowW(L"PCMGRWINDOW", L"进程详细信息窗口", WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, hWndParent, NULL, hInst, nullptr);
	if (!hWnd) return 0;

	EnableWindow(hWndParent, FALSE);

	HWND hWndList = CreateWindowW(WC_LISTVIEW, L"进程详细信息", WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN
		| LVS_REPORT | LVS_AUTOARRANGE | LVS_SHOWSELALWAYS
		| LVS_SHAREIMAGELISTS | LVS_SINGLESEL,
		0, 0, 1024, 740, hWnd, (HMENU)IDC_LISTMAIN, hInst, nullptr);
	SetWindowTheme(hWndList, L"explorer", NULL);
	ListView_SetExtendedListViewStyleEx(hWndList, 0, LVS_EX_FULLROWSELECT | LVS_EX_TWOCLICKACTIVATE);
	SendMessage(hWndList, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);

	RECT rectParent;
	GetWindowRect(hWndParent, &rectParent);

	int nX = ((rectParent.right - rectParent.left) - 1024) / 2 + rectParent.left;
	int nY = ((rectParent.bottom - rectParent.top) - 740) / 2 + rectParent.top;
	int nScreenWidth = GetSystemMetrics(SM_CXSCREEN);
	int nScreenHeight = GetSystemMetrics(SM_CYSCREEN);
	if (nX + 1024 > nScreenWidth) nX = nScreenWidth - 1024;
	if (nY + 740 > nScreenHeight) nY = nScreenHeight - 740;

	MoveWindow(hWnd, nX, nY, 1024, 740, TRUE);

	ShowWindow(hWnd, SW_SHOW);
	UpdateWindow(hWnd);

	MSG msg;
	BOOL bRet;
	while ((bRet = GetMessage(&msg, hWnd, 0, 0)) != 0)
	{
		if (bRet == -1) {
			EnableWindow(hWndParent, TRUE);
			return TRUE;
		}
		else {
			if (msg.message == WM_HOTKEY)
				SendMessage(msg.hwnd, WM_HOTKEY, msg.wParam, msg.lParam);
			else {
				TranslateMessage(&msg);
				DispatchMessage(&msg);
			}
		}
	}

	EnableWindow(hWndParent, TRUE);
	return TRUE;
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{	
	case WM_SIZE: {
		RECT rcTab;
		GetClientRect(hWnd, &rcTab);
		MoveWindow(GetDlgItem(hWnd, IDC_LISTMAIN), 0, 0, rcTab.right - rcTab.left, rcTab.bottom - rcTab.top, FALSE);
		break;
	}
	default:
		break;
	}
	return DefWindowProc(hWnd, message, wParam, lParam);
}


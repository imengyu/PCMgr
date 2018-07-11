#pragma once
#include "stdafx.h"
#include <Psapi.h>
#include <process.h>
#include <tlhelp32.h>
#include <CommCtrl.h>
#include <Uxtheme.h>
#include <shellapi.h>
#include <string>
#include <string.h>

EXTERN_C M_API BOOL MAppVProcessAllWindows();

EXTERN_C M_API void MAppVProcessAllWindowsUWP();

EXTERN_C M_API BOOL MAppVProcessAllWindowsGetProcessWindow(DWORD pid);

EXTERN_C M_API BOOL MAppVProcessAllWindowsGetProcessWindow2(DWORD pid);

EXTERN_C M_API BOOL MAppVProcessMsg(DWORD dwPID, HWND hDlg, int type, LPWSTR procName);

EXTERN_C M_API BOOL MAppVProcessModuls(DWORD dwPID, HWND hDlg, LPWSTR procName);

EXTERN_C M_API BOOL MAppVProcessThreads(DWORD dwPID, HWND hDlg, LPWSTR procName);

EXTERN_C M_API BOOL MAppVProcessWindows(DWORD dwPID, HWND hDlg, LPWSTR procName);

EXTERN_C M_API BOOL MAppVProcess(HWND hWndParent);

BOOL MAppVModuls(DWORD dwPID, HWND hDlg, LPWSTR procName);

BOOL MAppVThreads(DWORD dwPID, HWND hDlg, LPWSTR procName);

BOOL MAppVWins(DWORD dwPID, HWND hDlg, LPWSTR procName);


INT_PTR CALLBACK VWinsDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam);

INT_PTR CALLBACK VModulsDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam);

INT_PTR CALLBACK VThreadsDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam);

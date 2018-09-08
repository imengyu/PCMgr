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

void WindowEnumStart();
void WindowEnumDestroy();

BOOL MAppVProcessAllWindows();
void MAppVProcessAllWindowsUWP();
BOOL MAppVProcessAllWindowsGetProcessWindow(DWORD pid);
BOOL MAppVProcessAllWindowsGetProcessWindow2(DWORD pid);


//显示查看模块对话框
//    dwPID：需要查看的进程id
//    hDlg：父窗口
//    procName：进程名字，显示在标题栏上
EXTERN_C M_API BOOL MAppVProcessModuls(DWORD dwPID, HWND hDlg, LPWSTR procName);
//显示查看线程对话框
//    dwPID：需要查看的进程id
//    hDlg：父窗口
//    procName：进程名字，显示在标题栏上
EXTERN_C M_API BOOL MAppVProcessThreads(DWORD dwPID, HWND hDlg, LPWSTR procName);
//显示查看窗口对话框
//    dwPID：需要查看的进程id
//    hDlg：父窗口
//    procName：进程名字，显示在标题栏上
EXTERN_C M_API BOOL MAppVProcessWindows(DWORD dwPID, HWND hDlg, LPWSTR procName);


BOOL MAppVModuls(DWORD dwPID, HWND hDlg, LPWSTR procName);

BOOL MAppVThreads(DWORD dwPID, HWND hDlg, LPWSTR procName);

BOOL MAppVWins(DWORD dwPID, HWND hDlg, LPWSTR procName);


INT_PTR CALLBACK VWinsDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam);

INT_PTR CALLBACK VModulsDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam);

INT_PTR CALLBACK VThreadsDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam);

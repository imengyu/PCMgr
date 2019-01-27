#pragma once
#include "stdafx.h"
#include "ntdef.h"
#include "cscall.h"
#include "msup.h"

#define M_MTMSG_ 1
#define M_MTMSG_COSCLOSE 1

#define M_DRAW_HEADER_HOT 1
#define M_DRAW_HEADER_PRESSED 2
#define M_DRAW_HEADER_SORTDOWN 3
#define M_DRAW_HEADER_SORTUP 4

#define M_DRAW_LISTVIEW_HOT 1
#define M_DRAW_LISTVIEW_SELECT_NOFOCUS 2
#define M_DRAW_LISTVIEW_HOT_SELECT 3
#define M_DRAW_LISTVIEW_SELECT 4

#define M_DRAW_TREEVIEW_GY_OPEN 5
#define M_DRAW_TREEVIEW_GY_CLOSED 6
#define M_DRAW_TREEVIEW_GY_OPEN_HOT 7
#define M_DRAW_TREEVIEW_GY_CLOSED_HOT 8

#define M_DRAW_EXPAND_NORMAL 1
#define M_DRAW_EXPAND_HOVER 2
#define M_DRAW_EXPAND_PRESSED 3

typedef void(__cdecl *exitcallback)();
typedef int(__cdecl *taskdialogcallback)(HWND hwnd, LPWSTR text, LPWSTR title, LPWSTR apptl, int ico, int button);
typedef void(__cdecl *EnumWinsCallBack)(HWND hWnd, HWND hWndParent);
typedef void(__cdecl *GetWinsCallBack)(HWND hWnd, HWND hWndParent, int i);
typedef void(__cdecl *WorkerCallBack)(int msg, void* data1, void* data2);
typedef BOOL(__cdecl *TerminateImporantWarnCallBack)(LPWSTR commandName, int id);

//主窗口 WinProc
LRESULT CALLBACK MAppWinProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);
INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam);
LRESULT CALLBACK MProcListWinProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);
LRESULT CALLBACK MProcListHeaderWinProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

//显示一个对话框
int MShowMessageDialog(HWND hwnd, LPWSTR text, LPWSTR title, LPWSTR instruction, int i=0, int button=0);
//显示错误对话框
int MShowErrorMessage(LPWSTR text, LPWSTR intr, int ico=0, int btn=0);
//显示带有LastErr的错误对话框
int MShowErrorMessageWithLastErr(LPWSTR text, LPWSTR intr, int ico, int btn);
//显示 NTSTATUS 错误对话框
void MShowErrorMessageWithNTSTATUS(LPWSTR msg, LPWSTR title, NTSTATUS status);

EXTERN_C M_API BOOL MIsSystemSupport();
EXTERN_C M_API void MAppMainExit(UINT exitcode);
EXTERN_C M_API DWORD MAppMainGetExitCode();
EXTERN_C M_API BOOL MAppMainRun();

EXTERN_C M_API int MAppMainGetArgs(LPWSTR cmdline);
EXTERN_C M_API LPWSTR MAppMainGetArgsStr(int index);
EXTERN_C M_API void MAppMainGetArgsFreeAll();

EXTERN_C M_API void MAppHideCos();
EXTERN_C M_API void* MAppSetCallBack(void* cp, int id);
EXTERN_C M_API void MAppMainCall(int msg, void * data1, void * data2);
EXTERN_C M_API void MAppSetLanuageItems(int in, int ind, LPWSTR msg, int size);
EXTERN_C M_API void MAppSetStartingProgessText(LPWSTR text);
EXTERN_C M_API void MAppSet(int id, void*v);
EXTERN_C M_API void MAppTest(int id, void*v);
EXTERN_C M_API void MAppExit();
EXTERN_C M_API void MAppRebot();
EXTERN_C M_API void MAppRebotAdmin();
EXTERN_C M_API void MAppRebotAdmin2(LPWSTR agrs);
EXTERN_C M_API void MAppRebotAdmin3(LPWSTR agrs, BOOL * userCanceled);

EXTERN_C M_API HANDLE MOpenThemeData(HWND hWnd, LPWSTR className);
EXTERN_C M_API void MCloseThemeData(HANDLE hTheme);
EXTERN_C M_API void MSetAsExplorerTheme(HWND hWnd);
EXTERN_C M_API void MDrawIcon(HICON hIcon, HDC hdc, int x, int y);
EXTERN_C M_API void MExpandDrawButton(HANDLE hTheme, HDC hdc, int x, int y, int state, BOOL on);
EXTERN_C M_API void MHeaderDrawItem(HANDLE hTheme, HDC hdc, int x, int y, int w, int h, int state);
EXTERN_C M_API void MListDrawItem(HANDLE hTheme, HDC hdc, int x, int y, int w, int h, int state);
int MAppRegShowHotKey(HWND hWnd, UINT vkkey, UINT key);
BOOL MAppStartEnd();
BOOL MAppStartTryActiveLastApp(LPWSTR windowTitle);
BOOL MAppKillOld(LPWSTR procName);
BOOL MAppStartTest();
EXTERN_C M_API LRESULT MAppWorkCall1(WPARAM wParam, LPARAM lParam);
EXTERN_C M_API void MAppWorkCall2(UINT msg, WPARAM wParam, LPARAM lParam);
EXTERN_C M_API int MAppWorkCall3(int id, HWND hWnd, void*data);
EXTERN_C M_API void* MAppWorkCall4(int id, void* hWnd, void*data);
EXTERN_C M_API void* MAppWorkCall5(int id, void* hWnd, void*data1, void*data2, void*data3);
EXTERN_C M_API LRESULT MAppMainThreadCall(WPARAM wParam, LPARAM lParam);
EXTERN_C M_API LPWSTR MAppGetName();
EXTERN_C M_API LPWSTR MAppGetVersion();
EXTERN_C M_API LPWSTR MAppGetBulidDate();

//获取窗口的图标
//    hWnd ：窗口句柄
EXTERN_C M_API HICON MGetWindowIcon(HWND hWnd);

M_CAPI(void) MListViewSetColumnSortArrow(HWND hListHeader, int index, BOOL isUp, BOOL no);
M_CAPI(HWND) MListViewGetHeaderControl(HWND hList, BOOL isMain = FALSE);

//显示 NTSTATUS 错误对话框
void ThrowErrorAndErrorCodeX(NTSTATUS code, LPWSTR msg, LPWSTR title, BOOL ntstatus = TRUE);
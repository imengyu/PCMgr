#pragma once
#include "stdafx.h"
#include "ntdef.h"

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
typedef void(__cdecl *EnumPrivilegesCallBack)(LPWSTR name);

//主窗口 WinProc
LRESULT MAppWinProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);
INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam);
//显示错误对话框
void MPrintErrorMessage(LPWSTR str, int icon = MB_OK);
//显示一个对话框
int MShowMessageDialog(HWND hwnd, LPWSTR text, LPWSTR title, LPWSTR instruction, int i=0, int button=0);
//显示错误对话框
int MShowErrorMessage(LPWSTR text, LPWSTR intr, int ico=0, int btn=0);
//显示带有LastErr的错误对话框
int MShowErrorMessageWithLastErr(LPWSTR text, LPWSTR intr, int ico, int btn);

EXTERN_C M_API BOOL MIsSystemSupport();
EXTERN_C M_API BOOL MAppMainCanRun();
EXTERN_C M_API void MAppRun2();
EXTERN_C M_API void MGlobalAppInitialize();
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
EXTERN_C M_API int MAppRegShowHotKey(HWND hWnd, UINT vkkey, UINT key);
EXTERN_C M_API void MAppSetStartingProgessText(LPWSTR text);
EXTERN_C M_API void MAppExit();
EXTERN_C M_API void MAppRebot();
EXTERN_C M_API void MAppRebotAdmin();
EXTERN_C M_API void MAppRebotAdmin2(LPWSTR agrs);

EXTERN_C M_API HANDLE MOpenThemeData(HWND hWnd, LPWSTR className);
EXTERN_C M_API void MCloseThemeData(HANDLE hTheme);
EXTERN_C M_API void MSetAsExplorerTheme(HWND hWnd);
EXTERN_C M_API void MDrawIcon(HICON hIcon, HDC hdc, int x, int y);
EXTERN_C M_API void MExpandDrawButton(HANDLE hTheme, HDC hdc, int x, int y, int state, BOOL on);
EXTERN_C M_API void MHeaderDrawItem(HANDLE hTheme, HDC hdc, int x, int y, int w, int h, int state);
EXTERN_C M_API void MListDrawItem(HANDLE hTheme, HDC hdc, int x, int y, int w, int h, int state);
EXTERN_C M_API BOOL MAppStartEnd();
EXTERN_C M_API BOOL MAppStartTryCloseLastApp(LPWSTR windowTitle);
EXTERN_C M_API BOOL MAppKillOld(LPWSTR procName);
EXTERN_C M_API BOOL MAppStartTest();
EXTERN_C M_API void MAppWorkCall2(UINT msg, WPARAM wParam, LPARAM lParam);
EXTERN_C M_API int MAppWorkCall3(int id, HWND hWnd, void*data);
//获取窗口的图标
EXTERN_C M_API HICON MGetWindowIcon(HWND hWnd);

//字符串是否相等
#define MStrEqual MStrEqualW
//窄字符转为宽字符
#define A2W MConvertLPCSTRToLPWSTR
//宽字符转为窄字符
#define W2A MConvertLPWSTRToLPCSTR

EXTERN_C M_API void MConvertStrDel(void * str);

//窄字符转为宽字符
EXTERN_C M_API LPWSTR MConvertLPCSTRToLPWSTR(const char * szString);
//宽字符转为窄字符
EXTERN_C M_API LPCSTR MConvertLPWSTRToLPCSTR(const WCHAR * szString);
EXTERN_C M_API LPWSTR cMStrLoW(const LPWSTR str);
EXTERN_C M_API LPWSTR MStrUpW(const LPWSTR str);
EXTERN_C M_API LPCSTR MStrUpA(const LPCSTR str);
EXTERN_C M_API LPWSTR MStrLoW(const LPWSTR str);
EXTERN_C M_API LPCSTR MStrLoA(const LPCSTR str);
EXTERN_C M_API LPWSTR MStrAddW(const LPWSTR str1, const LPWSTR str2);
EXTERN_C M_API LPCSTR MStrAddA(const LPCSTR str1, const LPCSTR str2);
//字符串是否相等
EXTERN_C M_API BOOL MStrEqualA(const LPCSTR str1, const LPCSTR str2);
//字符串是否相等
EXTERN_C M_API BOOL MStrEqualW(const wchar_t* str1, const wchar_t* str2);
EXTERN_C M_API LPCSTR MIntToStrA(int i);
EXTERN_C M_API LPWSTR MIntToStrW(int i);
EXTERN_C M_API LPCSTR MLongToStrA(long i);
EXTERN_C M_API LPWSTR MLongToStrW(long i);
EXTERN_C M_API int MStrToIntA(char * str);
EXTERN_C M_API int MStrToIntW(LPWSTR str);
EXTERN_C M_API DWORD MStrSplitA(char * str, const LPCSTR splitStr, LPCSTR * result, char ** lead);
EXTERN_C M_API DWORD MStrSplitW(LPWSTR str, const LPWSTR splitStr, LPWSTR * result, wchar_t ** lead);
EXTERN_C M_API BOOL MStrContainsA(const LPCSTR str, const LPCSTR testStr, LPCSTR * resultStr);
EXTERN_C M_API BOOL MStrContainsW(const LPWSTR str, const LPWSTR testStr, LPWSTR * resultStr);
EXTERN_C M_API BOOL MStrContainsCharA(const LPCSTR str, const CHAR testStr);
EXTERN_C M_API BOOL MStrContainsCharW(const LPWSTR str, const WCHAR testStr);
EXTERN_C M_API int MHexStrToIntW(wchar_t *s);
EXTERN_C M_API long long MHexStrToLongW(wchar_t *s);

//显示 NTSTATUS 错误对话框
void ThrowErrorAndErrorCodeX(NTSTATUS code, LPWSTR msg, LPWSTR title, BOOL ntstatus = TRUE);
#pragma once
#include "stdafx.h"

typedef VOID(WINAPI*fnExitProcess)(__in UINT uExitCode);
typedef HMODULE(WINAPI *fnLoadLibraryA)(LPCSTR lpLibFileName);
typedef HMODULE(WINAPI *fnLoadLibraryW)(LPCWSTR lpLibFileName);
typedef FARPROC(WINAPI*fnGetProcAddress)(__in HMODULE hModule,__in LPCSTR lpProcName);
typedef HMODULE(WINAPI*fnGetModuleHandleA)(__in_opt LPCSTR lpModuleName);

typedef long(NTAPI* fnRtlGetVersion)(PRTL_OSVERSIONINFOW lpVersionInformation);

typedef int (WINAPI *fnMessageBoxW)(__in_opt HWND hWnd, __in_opt LPCWSTR lpText, __in_opt LPCWSTR lpCaption, __in UINT uType);

typedef DWORD(*_MAppMainGetExitCode)();
typedef void(*_MAppMainRun)();
typedef void(*_MAppSet)(int id, void*v);


extern fnMessageBoxW _MessageBoxW;
extern fnExitProcess _ExitProcess;
extern fnLoadLibraryA _LoadLibraryA;
extern fnLoadLibraryW _LoadLibraryW;
extern fnRtlGetVersion RtlGetVersion;

extern _MAppSet MAppSet;
extern _MAppMainGetExitCode MAppMainGetExitCode;
extern _MAppMainRun MAppMainRun;

BOOL MLoadDyamicFuns();
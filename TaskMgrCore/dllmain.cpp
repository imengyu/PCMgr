// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "stdafx.h"
#include "resource.h"

HINSTANCE hInst;

extern HICON HIconDef;
extern HCURSOR hCurLoading;
extern MEMORYSTATUSEX memory_statuex;

extern BOOL LoadDll();
extern void FreeDll();
extern void WindowEnumStart();
extern void WindowEnumDestroy();
extern void MPERF_FreeCpuInfos();



M_CAPI(int) MPERF_GetProcessNumber();
extern void ShowMainCoreStartUp();

void DllDestroy();
void DllStartup();

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		hInst = (HINSTANCE)hModule;
		DllStartup();
		break;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		DllDestroy();
		break;
	}
	return TRUE;
}

void DllStartup() {
	HIconDef = (HICON)LoadImage(hInst, MAKEINTRESOURCE(IDI_ICONDEFAPP), IMAGE_ICON, 16, 16, 0);
	LoadDll();
	hCurLoading = LoadCursor(NULL, IDC_WAIT);
	ShowMainCoreStartUp();
	MPERF_GetProcessNumber();
	WindowEnumStart();
	memory_statuex.dwLength = sizeof(memory_statuex);
}
void DllDestroy() {
	FreeDll();
	MPERF_FreeCpuInfos();
	WindowEnumDestroy();
}

// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "stdafx.h"
#include "resource.h"
#include "sysfuns.h"
#include "perfhlp.h"
#include "kernelhlp.h"
#include "prochlp.h"
#include "mainprocs.h"
#include "settinghlp.h"
#include "lghlp.h"
#include "vprocx.h"

HINSTANCE hInst;

extern HICON HIconDef;
extern HCURSOR hCurLoading;
extern MEMORYSTATUSEX memory_statuex;

//Dll释放
void DllDestroy();
//Dll初始化
void DllStartup();
void DllAnti();
bool DllAntiCheckVery();
void DllAntiCrush();

BOOL APIENTRY DllMain( HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:	
		hInst = (HINSTANCE)hModule;
		DllStartup();
		break;
	case DLL_THREAD_ATTACH:
		MAnitInjectLow();
		break;
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
	MLG_Startup();
	hCurLoading = LoadCursor(NULL, IDC_WAIT);
	ShowMainCoreStartUp();
	MPERF_GlobalInit();
	MPERF_GetProcessNumber();
	WindowEnumStart();
	memory_statuex.dwLength = sizeof(memory_statuex);
}
void DllDestroy() {
	hInst = NULL;
	MUnInitKernelNTPDB();
	MLG_SetLanuageItems_Destroy();
	MPERF_FreeCpuInfos();
	MPERF_GlobalDestroy();
	WindowEnumDestroy();
	MUnInitMyDbgView();
	FreeDll();
}
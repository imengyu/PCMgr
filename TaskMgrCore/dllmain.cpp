// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "stdafx.h"
#include "resource.h"
#include "sysfuns.h"
#include "perfhlp.h"
#include "kernelhlp.h"
#include "prochlp.h"
#include "mainprocs.h"
#include "loghlp.h"
#include "settinghlp.h"
#include "lghlp.h"
#include "vprocx.h"
#include "cmdhlp.h"

#include "MCpuInfoMonitor.h"
#include "MSystemPerformanctMonitor.h"

#include "..\PCMgrCmdRunner\PCMgrCmdRunnerEntry.h"

HINSTANCE hInst;

extern HICON HIconDef;
extern HCURSOR hCurLoading;
extern MEMORYSTATUSEX memory_statuex;
extern MCmdRunner *staticCmdRunner;

//Dll释放
void DllDestroy();
//Dll初始化
void DllStartup();

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
	M_LOG_Init();
	MLG_Startup();
	hCurLoading = LoadCursor(NULL, IDC_WAIT);
	ShowMainCoreStartUp();
	MPERF_GlobalInit();
	MSystemPerformanctMonitor::InitGlobal();

	WindowEnumStart();
	memory_statuex.dwLength = sizeof(memory_statuex);
	staticCmdRunner = new MCmdRunner();
	MInitAllCmd();
}
void DllDestroy() {
	hInst = NULL;
	M_LOG_Destroy();	
	MProcessHANDLEStorageDestroy();
	MUnInitKernelNTPDB();
	MLG_SetLanuageItems_Destroy();

	MSystemPerformanctMonitor::DestroyGlobal();
	MCpuInfoMonitor::FreeCpuInfos();

	MPERF_GlobalDestroy();
	WindowEnumDestroy();
	MUnInitMyDbgView();
	delete(staticCmdRunner);
	FreeDll();
}
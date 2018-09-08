#include "stdafx.h"
#include "kernelhlp.h"
#include "mapphlp.h"
#include "syshlp.h"
#include "fmhlp.h"
#include "suact.h"
#include "reghlp.h"
#include "loghlp.h"
#include "resource.h"
#include "settinghlp.h"
#include "thdhlp.h"
#include "ntsymbol.h"
#include "cscall.h"
#include "sysstructs.h"
#include "StringHlp.h"
#include <io.h>

#include "..\PCMgrKrnlMgr\MKrnlMgr.h"

BOOL isKernelPDBLoaded = FALSE;
HANDLE hKernelDevice = NULL;
extern HWND hWndMain;
extern HMENU hMenuMainFile;

ULONG_PTR uNTBaseAddress = 0;
HANDLE hThreadDbgView = NULL;
HANDLE hEventDbgView = NULL;
BOOL isMyDbgViewLoaded = FALSE;
BOOL isMyDbgViewRunning = FALSE;

extern bool executeByLoader;
extern WCHAR appDir[MAX_PATH];

//加载驱动
//    lpszDriverName：驱动的服务名
//    driverPath：驱动的完整路径
//    lpszDisplayName：nullptr
M_CAPI(BOOL) MLoadKernelDriver(LPWSTR lpszDriverName, LPWSTR driverPath, LPWSTR lpszDisplayName)
{
#ifndef _AMD64_
	if (MIs64BitOS())
	{
		LogErr(L"You need to use 64 bit version PCMgr application to load driver.");
		return FALSE;
	}
#endif
	if (MIsRunasAdmin())
	{
		wchar_t sDriverName[32];
		wcscpy_s(sDriverName, lpszDriverName);

		bool recreatee = false;

	RECREATE:
		BOOL bRet = FALSE;
		SC_HANDLE hServiceMgr = NULL;
		SC_HANDLE hServiceDDK = NULL;
		hServiceMgr = OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
		if (hServiceMgr == NULL)
		{
			LogErr(L"Load driver error in OpenSCManager : %d", GetLastError());
			bRet = FALSE;
			goto BeforeLeave;
		}

		hServiceDDK = CreateService(hServiceMgr, lpszDriverName, lpszDisplayName, SERVICE_ALL_ACCESS, SERVICE_KERNEL_DRIVER,
			SERVICE_DEMAND_START, SERVICE_ERROR_IGNORE, driverPath, NULL, NULL,NULL,	NULL,NULL);

		DWORD dwRtn = 0;
		if (hServiceDDK == NULL)
		{
			dwRtn = GetLastError();
			if (dwRtn == ERROR_SERVICE_MARKED_FOR_DELETE)
			{
				LogErr(L"Load driver error in CreateService : ERROR_SERVICE_MARKED_FOR_DELETE");
				if (!recreatee) {
					recreatee = true;
					if (hServiceDDK) CloseServiceHandle(hServiceDDK);
					if (hServiceMgr) CloseServiceHandle(hServiceMgr);
					if (MREG_ForceDeleteServiceRegkey(sDriverName)) goto RECREATE;
				}
			}
			if (dwRtn != ERROR_IO_PENDING && dwRtn != ERROR_SERVICE_EXISTS)
			{
				LogErr(L"Load driver error in CreateService : %d", dwRtn);
				bRet = FALSE;
				goto BeforeLeave;
			}
			hServiceDDK = OpenService(hServiceMgr, lpszDriverName, SERVICE_ALL_ACCESS);
			if (hServiceDDK == NULL)
			{
				dwRtn = GetLastError();
				LogErr(L"Load driver error in OpenService : %d", dwRtn);
				bRet = FALSE;
				goto BeforeLeave;
			}
		}
		bRet = StartService(hServiceDDK, NULL, NULL);	
		if (!bRet)
		{
			DWORD dwRtn = GetLastError();
			if (dwRtn != ERROR_IO_PENDING && dwRtn != ERROR_SERVICE_ALREADY_RUNNING)
			{
				LogErr(L"Load driver error in StartService : %d", dwRtn);
				bRet = FALSE;
				goto BeforeLeave;
			}
			else
			{
				if (dwRtn == ERROR_IO_PENDING)
				{
					bRet = FALSE;
					goto BeforeLeave;
				}
				else
				{
					bRet = TRUE;
					goto BeforeLeave;
				}
			}
		}
		bRet = TRUE;
		//离开前关闭句柄
	BeforeLeave:
		if (hServiceDDK) CloseServiceHandle(hServiceDDK);
		if (hServiceMgr) CloseServiceHandle(hServiceMgr);
		return bRet;
	}else LogErr(L"Load driver error because need adminstrator.");
	return FALSE;
}
//卸载驱动
//    szSvrName：服务名
M_CAPI(BOOL) MUnLoadKernelDriver(LPWSTR szSvrName)
{
	BOOL bDeleted = FALSE;
	BOOL bRet = FALSE;
	SC_HANDLE hServiceMgr = NULL;
	SC_HANDLE hServiceDDK = NULL;
	SERVICE_STATUS SvrSta;
	hServiceMgr = OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
	if (hServiceMgr == NULL)
	{
		LogErr(L"UnLoad driver error in OpenSCManager : %d", GetLastError());
		bRet = FALSE;
		goto BeforeLeave;
	}
	//打开驱动所对应的服务
	hServiceDDK = OpenService(hServiceMgr, szSvrName, SERVICE_ALL_ACCESS);
	if (hServiceDDK == NULL)
	{
		if (GetLastError() == ERROR_SERVICE_DOES_NOT_EXIST)
			LogErr(L"UnLoad driver error because driver not load.");
		else LogErr(L"UnLoad driver error in OpenService : %d", GetLastError());
		bRet = FALSE;
		goto BeforeLeave;
	}
	//停止驱动程序，如果停止失败，只有重新启动才能，再动态加载。 
	if (!ControlService(hServiceDDK, SERVICE_CONTROL_STOP, &SvrSta)) {
		LogErr(L"UnLoad driver error in ControlService : %d", GetLastError());
	}
	//动态卸载驱动程序。 
	if (!DeleteService(hServiceDDK)) {
		LogErr(L"UnLoad driver error in DeleteService : %d", GetLastError());
		bRet = FALSE;
	}
	else bDeleted = TRUE;

BeforeLeave:
	//离开前关闭打开的句柄
	if (hServiceDDK) CloseServiceHandle(hServiceDDK);
	if (hServiceMgr) CloseServiceHandle(hServiceMgr);

	if(bDeleted) bRet = MREG_ForceDeleteServiceRegkey(szSvrName);

	return bRet;
}

//获取ntoskrn.exe基地址（内核加载以后有效）
M_CAPI(ULONG_PTR) MGetNTBaseAddress() {
	return uNTBaseAddress;
}
//切换加载/卸载驱动菜单状态
void MInitKernelSwitchMenuState(BOOL loaded)
{
	if (loaded) {
		EnableMenuItem(hMenuMainFile, IDM_UNLOAD_DRIVER, MF_ENABLED);
		EnableMenuItem(hMenuMainFile, IDM_LOAD_DRIVER, MF_DISABLED);
	}
	else {
		EnableMenuItem(hMenuMainFile, IDM_UNLOAD_DRIVER, MF_DISABLED);
		EnableMenuItem(hMenuMainFile, IDM_LOAD_DRIVER, MF_ENABLED);
	}
}
//打开驱动句柄
BOOL MInitKernelDriverHandleEndStep(HANDLE hHandle) {
	hKernelDevice = hHandle;

	if (!executeByLoader) {
		CloseHandle(hKernelDevice);
		hKernelDevice = NULL;
		return FALSE;
	}

	Log(L"Kernel Driver HANDLE Created.");
	MInitMyDbgView();
	CreateThread(NULL, 0, MLoadingThread, NULL, 0, NULL);
	MInitKernelSwitchMenuState(MKrnlMgr::KernelInited());
	return TRUE;
}

//加载内核的pdb
BOOL MInitKernelNTPDB(BOOL usingNtosPDB, PKNTOSVALUE kNtosValue)
{
	if (!usingNtosPDB)
		return TRUE;
	if (!InitSymHandler())
		return FALSE;

	BOOL rs = FALSE;
	BOOL canSend = FALSE;
	if (kNtosValue->NtostAddress != 0 && !StrEqual(kNtosValue->NtosModuleName, L""))
	{
		uNTBaseAddress = kNtosValue->NtostAddress;
		char* ntosNameC = (char*)W2A(kNtosValue->NtosModuleName);
#ifdef _AMD64_
		Log(L"Get NTOS base info : %s 0x%I64X", kNtosValue->NtosModuleName, kNtosValue->NtostAddress);
#else
		Log(L"Get NTOS base info : %s 0x%08X", kNtosValue->NtosModuleName, kNtosValue->NtostAddress);
#endif
		MAppSetStartingProgessText(L"Downloading and loading ntos PDB file...");
		if (MKSymInit(ntosNameC, kNtosValue->NtostAddress))
		{
			MAppSetStartingProgessText(L"Analysis the ntos PDB files...");
			if (MEnumNTOSSyms(kNtosValue->NtostAddress, (PSYM_ENUMERATESYMBOLS_CALLBACK)MEnumSymNTOSRoutine, NULL))
				canSend = TRUE;
		}
		delete ntosNameC;
	}
	else LogErr2(L"Failed get ntos baseAddress and name!");

	if (kNtosValue->Win32KAddress != 0)
	{
#ifdef _AMD64_
		Log(L"Get Win32K base address : 0x%I64X", kNtosValue->Win32KAddress);
#else
		Log(L"Get Win32K base address : 0x%X", kNtosValue->Win32KAddress);
#endif
		MAppSetStartingProgessText(L"Downloading and loading Win32K PDB file...");
		if (MKSymInit("win32k.sys", kNtosValue->Win32KAddress))
		{			
			MAppSetStartingProgessText(L"Analysis of the Win32K PDB files...");
			if (MEnumWIN32KSyms(kNtosValue->NtostAddress, (PSYM_ENUMERATESYMBOLS_CALLBACK)MEnumSymWIN32KRoutine, NULL))
				canSend = TRUE;
		}
	}			
	else LogWarn2(L"Failed get Win32K base address!");
	if (canSend) {

		MAppSetStartingProgessText(L"Analysis finished\nSend all symbol data to driver...");
		rs = MSendAllSymAddressToDriver();
		isKernelPDBLoaded = rs;
	}
	else rs = FALSE;
	return rs;
}
//加载内核的pdb释放资源
BOOL MUnInitKernelNTPDB() {
	if (isKernelPDBLoaded)
		return MEnumSymsClear();
	return TRUE;
}
VOID MLoadKernelNTPDB(PKNTOSVALUE kNtosValue, BOOL usingNtosPDB) {
	MGetNtosAndWin32kfullNameAndStartAddress(kNtosValue->NtosModuleName, 32, (kNtosValue->NtostAddress == 0 ? &kNtosValue->NtostAddress : 0), &kNtosValue->Win32KAddress);
	if (!MInitKernelNTPDB(usingNtosPDB, kNtosValue) && !isKernelPDBLoaded) {
		LogWarn(L"Failed to load pdb file of ntos, instead, compiled static constants will used.");
		LogInfo(L"=================================");
		LogWarn(L"Compiled static constants are used by the kernel, but not necessarily accurate, so kernel operations can cause system crashes. Therefore, in order to protect your system, most of the kernel functions have been disabled and always return STATUS_UNSUCCESSFUL.");
		LogInfo(L"=================================");
		M_SU_Init(false, kNtosValue);
	}
}

M_CAPI(BOOL) MIsKernelNeed64()
{
	return MKrnlMgr::KernelNeed64();
}
M_CAPI(BOOL) MCanUseKernel()
{
	return MKrnlMgr::KernelCanUse();
}
M_CAPI(BOOL) MInitKernel()
{
	return MKrnlMgr::InitKernel(appDir);
}
M_CAPI(BOOL) MUninitKernel()
{
	return MKrnlMgr::UnInitKernel();
}

BOOL froceNotUseMyDbgView = FALSE;

BOOL MShowMyDbgView()
{
	if (!isMyDbgViewLoaded)
		MInitMyDbgView();
	else MAppMainCall(M_CALLBACK_DBGPRINT_CLOSE, 0, 0);
	return 1;
}

M_CAPI(VOID) MOnCloseMyDbgView() {
	if (isMyDbgViewLoaded)
		MUnInitMyDbgView();
}

BOOL MMyDbgViewStarted()
{
	return isMyDbgViewLoaded;
}
BOOL MUnInitMyDbgView() {
	if (isMyDbgViewLoaded)
	{
		MAppMainCall(M_CALLBACK_DBGPRINT_CLOSE, 0, 0);
		M_SU_ReSetDbgViewEvent();
		if (hEventDbgView) { CloseHandle(hEventDbgView); hEventDbgView = 0; }
		isMyDbgViewRunning = FALSE;
		DWORD dw = WaitForSingleObject(hThreadDbgView, 100);
		if (dw == WAIT_TIMEOUT) { 
			if(NT_SUCCESS(MTerminateThreadNt(hThreadDbgView)))
				LogInfo(L"MDbgViewReceiveThread Terminated.");
			else LogWarn(L"MDbgViewReceiveThread Terminate failed!");
		}
		if (hThreadDbgView) { CloseHandle(hThreadDbgView); hThreadDbgView = 0; }
		isMyDbgViewLoaded = FALSE;
		LogInfo(L"MyDbgView stoped.");
	}
	return !isMyDbgViewLoaded;
}
BOOL MInitMyDbgView() 
{
	if (!isMyDbgViewLoaded)
	{
		if (!M_CFG_GetConfigBOOL(L"LogDbgPrint", L"Configure", true) && !froceNotUseMyDbgView)
			return TRUE;

		hEventDbgView = CreateEvent(NULL, TRUE, FALSE, L"PCMGR_DBGVIEW");
		if (!M_SU_SetDbgViewEvent(hEventDbgView)) {
			CloseHandle(hEventDbgView);
			return FALSE;
		}

		isMyDbgViewRunning = TRUE;
		hThreadDbgView = CreateThread(NULL, 0, MDbgViewReceiveThread, NULL, 0, NULL);
		if (hThreadDbgView) {
			isMyDbgViewLoaded = TRUE;
			MAppMainCall(M_CALLBACK_DBGPRINT_SHOW, 0, 0);
			LogInfo(L"MyDbgView started.");
			return isMyDbgViewLoaded;
		}
		else LogWarn(L"Create MyDbgView Thread failed : %d", GetLastError());
	}
	return FALSE;
}
VOID MDoNotStartMyDbgView()
{
	froceNotUseMyDbgView = TRUE;
}

//MyDbgView线程
DWORD WINAPI MDbgViewReceiveThread(LPVOID lpParameter)
{
	LogInfo(L"MDbgViewReceiveThread sterted.");

	while (isMyDbgViewRunning) 
	{
		WaitForSingleObject(hEventDbgView, INFINITE);

		WCHAR lastBuffer[256];

		CONTINUE:
		BOOL hasMoreData = FALSE;
		memset(lastBuffer, 0, sizeof(lastBuffer));
		if (M_SU_GetDbgViewLastBuffer(lastBuffer, 256, &hasMoreData))
		{
			if (StrEqual(lastBuffer, L""))
				MAppMainCall(M_CALLBACK_DBGPRINT_EMEPTY, 0, 0);
			else MAppMainCall(M_CALLBACK_DBGPRINT_DATA, lastBuffer, 0);

			if (hasMoreData) goto CONTINUE;
		}

		ResetEvent(hEventDbgView);
	}

	LogInfo(L"MDbgViewReceiveThread exiting.");
	return 0;
}
//加载线程
DWORD WINAPI MLoadingThread(LPVOID lpParameter)
{
	BOOL usingNtosPDB = M_CFG_GetConfigBOOL(L"UseKrnlPDB", L"Configure", true);
	MAppSetStartingProgessText(L"Init Kernel...");
	KNTOSVALUE kNtosValue = { 0 };
	M_SU_Init(usingNtosPDB, &kNtosValue);
	if (!kNtosValue.KernelDataInited)
		MLoadKernelNTPDB(&kNtosValue, usingNtosPDB);
	MAppSetStartingProgessText(L"Initializing...");
	MAppMainCall(M_CALLBACK_INVOKE_LASTLOAD_STEP, 0, 0);
	return 0;
}
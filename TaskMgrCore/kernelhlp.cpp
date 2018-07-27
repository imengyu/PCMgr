#include "stdafx.h"
#include "kernelhlp.h"
#include "mapphlp.h"
#include "syshlp.h"
#include "fmhlp.h"
#include "suact.h"
#include "reghlp.h"
#include "loghlp.h"
#include "resource.h"
#include "StringHlp.h"
#include <io.h>

BOOL isKernelDriverLoaded = FALSE;
HANDLE hKernelDevice = NULL;
extern HWND hWndMain;
extern HMENU hMenuMainFile;

BOOL MForceDeleteServiceRegkey(LPWSTR lpszDriverName)
{
	BOOL rs = FALSE;
	wchar_t regPath[MAX_PATH];
	wsprintf(regPath, L"SYSTEM\\CurrentControlSet\\services\\%s", lpszDriverName);
	rs = MREG_DeleteKey(HKEY_LOCAL_MACHINE, regPath);

	if (!rs)LogErr(L"RegDeleteTree failed : %d in delete key HKEY_LOCAL_MACHINE\\%s", GetLastError(), regPath);
	else Log(L"Service Key deleted : HKEY_LOCAL_MACHINE\\%s", regPath);
	
	wchar_t regName[MAX_PATH];
	wcscpy_s(regName, lpszDriverName);
	_wcsupr_s(regName);
	wsprintf(regPath, L"SYSTEM\\CurrentControlSet\\Enum\\Root\\LEGACY_%s", regName);
	rs = MREG_DeleteKey(HKEY_LOCAL_MACHINE, regPath);

	if (!rs) {
		LogErr(L"RegDeleteTree failed : %d in delete key HKEY_LOCAL_MACHINE\\%s", GetLastError(), regPath);
		rs = TRUE;
	}
	else Log(L"Service Key deleted : HKEY_LOCAL_MACHINE\\%s", regPath);

	return rs;
}
M_CAPI(BOOL) MLoadKernelDriver(LPWSTR lpszDriverName, LPWSTR driverPath, LPWSTR lpszDisplayName)
{
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
					if (MForceDeleteServiceRegkey(sDriverName)) goto RECREATE;
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
		LogErr(L"UnLoad driver error in OpenService : %d", GetLastError());
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

	if(bDeleted) bRet = MForceDeleteServiceRegkey(szSvrName);

	return bRet;
}
 
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
BOOL MInitKernelDriverHandle() {
	hKernelDevice = CreateFile(L"\\\\.\\PCMGRK",
		GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_READ | FILE_SHARE_WRITE,
		NULL,
		OPEN_EXISTING,
		FILE_ATTRIBUTE_NORMAL,
		NULL);
	if (hKernelDevice == INVALID_HANDLE_VALUE)
	{
		LogErr(L"Load Kernel driver success but CreateFile failed : %d . ", GetLastError());		
		return FALSE;
	}
	else {
		Log(L"Kernel Driver HANDLE Created.");
		M_SU_Init();
	}
	MInitKernelSwitchMenuState(isKernelDriverLoaded);
	return TRUE;
}
M_CAPI(BOOL) MCanUseKernel()
{
	return isKernelDriverLoaded && hKernelDevice != NULL;
}
M_CAPI(BOOL) MInitKernel(LPWSTR currentPath)
{
	Log(L"MInitKernel (%s)...", currentPath);
	if (!isKernelDriverLoaded)
	{
		wchar_t path[MAX_PATH];
		if(MIs64BitOS()) wsprintf(path, L"%s\\PCMgrKernel64.sys", currentPath);
		else wsprintf(path, L"%s\\PCMgrKernel32.sys", currentPath);

		if (MFM_FileExist(path))
		{
			Log(L"LoadKernelDrive (%s)...", path);
			
			if (MLoadKernelDriver(L"PCMgrKernel", path, L"PCMgr kernel driver")) {
				Log(L"Kernel Driver Loaded.");
				isKernelDriverLoaded = TRUE;
				return MInitKernelDriverHandle();
			}
			else
			{
				if (MInitKernelDriverHandle()) {
					isKernelDriverLoaded = TRUE;
					MInitKernelSwitchMenuState(isKernelDriverLoaded);
				}
				else {
					isKernelDriverLoaded = FALSE;
					LogErr(L"Load Kernel driver error");
				}
			}
		}
		else if (MInitKernelDriverHandle()) {
			Log(L"Kernel driver file missing.");
			isKernelDriverLoaded = TRUE;
			MInitKernelSwitchMenuState(isKernelDriverLoaded);
		}
		else {

			LogErr(L"Load Kernel driver error because kernel driver file missing.");
			LogErr(L"Try find kernel driver file : %s.", path);

			isKernelDriverLoaded = FALSE;
			MInitKernelSwitchMenuState(isKernelDriverLoaded);
		}
	}
	else Log(L"Kernel alreday loaded");
	return isKernelDriverLoaded;
}
M_CAPI(BOOL) MUninitKernel()
{
	Log(L"MUninitKernel...");
	if (isKernelDriverLoaded)
	{
		if (hKernelDevice != NULL)
			CloseHandle(hKernelDevice);

		if (MUnLoadKernelDriver(L"PCMgrKernel")) {
			isKernelDriverLoaded = FALSE;
			EnableMenuItem(hMenuMainFile, IDM_UNLOAD_DRIVER, MF_DISABLED);
			EnableMenuItem(hMenuMainFile, IDM_LOAD_DRIVER, MF_ENABLED);
			
			Log(L"Kernel unloaded");
		}
		else Log(L"Kernel unload failed");
		return !isKernelDriverLoaded;
	}
	else {
		Log(L"Kernel not load , try force delete service");

		if (MUnLoadKernelDriver(L"PCMgrKernel")) {
			isKernelDriverLoaded = FALSE;
			EnableMenuItem(hMenuMainFile, IDM_UNLOAD_DRIVER, MF_DISABLED);
			EnableMenuItem(hMenuMainFile, IDM_LOAD_DRIVER, MF_ENABLED);

			Log(L"Kernel unloaded");
		}
		else Log(L"Kernel unload failed");
	}
	return FALSE;
}

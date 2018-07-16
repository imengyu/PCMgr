#include "stdafx.h"
#include "kernelhlp.h"
#include "mapphlp.h"
#include "syshlp.h"
#include <io.h>

BOOL isKernelDriverLoaded = FALSE;
HANDLE hKernalDevice = NULL;
extern HWND hWndMain;

M_CAPI(BOOL) MLoadKernelDriver(LPWSTR lpszDriverName, LPWSTR driverPath, LPWSTR lpszDisplayName)
{
	if (MIsRunasAdmin())
	{
		BOOL bRet = FALSE;
		SC_HANDLE hServiceMgr = NULL;
		SC_HANDLE hServiceDDK = NULL;
		hServiceMgr = OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
		if (hServiceMgr == NULL)
		{
			bRet = FALSE;
			goto BeforeLeave;
		}

		hServiceDDK = CreateService(hServiceMgr, lpszDriverName, lpszDisplayName, SERVICE_ALL_ACCESS, SERVICE_KERNEL_DRIVER,
			SERVICE_DEMAND_START, SERVICE_ERROR_IGNORE, driverPath, NULL, NULL,NULL,	NULL,NULL);

		DWORD dwRtn;
		if (hServiceDDK == NULL)
		{
			dwRtn = GetLastError();
			if (dwRtn != ERROR_IO_PENDING && dwRtn != ERROR_SERVICE_EXISTS)
			{
				bRet = FALSE;
				goto BeforeLeave;
			}
			hServiceDDK = OpenService(hServiceMgr, lpszDriverName, SERVICE_ALL_ACCESS);
			if (hServiceDDK == NULL)
			{
				dwRtn = GetLastError();
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
	}
	return FALSE;
}
M_CAPI(BOOL) MUnLoadKernelDriver(LPWSTR szSvrName)
{
	BOOL bRet = FALSE;
	SC_HANDLE hServiceMgr = NULL;
	SC_HANDLE hServiceDDK = NULL;
	SERVICE_STATUS SvrSta;
	hServiceMgr = OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
	if (hServiceMgr == NULL)
	{
		bRet = FALSE;
		goto BeforeLeave;
	}
	//打开驱动所对应的服务
	hServiceDDK = OpenService(hServiceMgr, szSvrName, SERVICE_ALL_ACCESS);
	if (hServiceDDK == NULL)
	{
		bRet = FALSE;
		goto BeforeLeave;
	}
	//停止驱动程序，如果停止失败，只有重新启动才能，再动态加载。 
	if (!ControlService(hServiceDDK, SERVICE_CONTROL_STOP, &SvrSta))
		bRet = FALSE;
	//动态卸载驱动程序。 
	if (!DeleteService(hServiceDDK))
		bRet = FALSE;
	bRet = TRUE;
BeforeLeave:
	//离开前关闭打开的句柄
	if (hServiceDDK) CloseServiceHandle(hServiceDDK);
	if (hServiceMgr) CloseServiceHandle(hServiceMgr);
	return bRet;
}
 
M_CAPI(BOOL) MCanUseKernel()
{
	return isKernelDriverLoaded;
}
M_CAPI(BOOL) MInitKernel(LPWSTR currentPath)
{
	if (!isKernelDriverLoaded)
	{
		wchar_t path[MAX_PATH];
		wsprintf(path, L"%s\\PCMgrKernel32.sys", currentPath);
		if (_waccess(path, 0) == 0) 
		{
			if (MLoadKernelDriver(L"PCMgrKernel", path, L"PCMgr kernel driver"))
			{
				if ((hKernalDevice = CreateFile(L"\\\\.\\PCMGRK",
					GENERIC_READ | GENERIC_WRITE,
					0,
					NULL,
					CREATE_ALWAYS,
					FILE_ATTRIBUTE_NORMAL,
					NULL)) == INVALID_HANDLE_VALUE) {

					wchar_t err[MAX_PATH];
					wsprintf(err, L"CreateFile error : %d.", GetLastError());
					MessageBox(hWndMain, err, L"Error", MB_ICONERROR);

					isKernelDriverLoaded = TRUE;
					return FALSE;
				}
				isKernelDriverLoaded = TRUE;
			}
			else 
			{
				wchar_t err[MAX_PATH];
				wsprintf(err, L"Load driver error : %d.", GetLastError());
				MessageBox(hWndMain, err, L"Error", MB_ICONERROR);
			}
		}
	}
	return isKernelDriverLoaded;
}
M_CAPI(BOOL) MUninitKernel()
{
	if (isKernelDriverLoaded)
	{
		if (MUnLoadKernelDriver(L"PCMgrKernel"))
			isKernelDriverLoaded = FALSE;
		return !isKernelDriverLoaded;
	}
	return FALSE;
}

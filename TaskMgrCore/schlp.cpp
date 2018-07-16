#include "stdafx.h"
#include "schlp.h"
#include "mapphlp.h"
#include "lghlp.h"
#include "StringSplit.h"
#include "resource.h"
#include <stdio.h>
#include <stdlib.h>
#include <string>

using namespace std;

extern HINSTANCE hInstRs;
extern "C" M_API BOOL MCopyToClipboard(const WCHAR* pszData, const int nDataLen);

SC_HANDLE hSCM = NULL;
ENUM_SERVICE_STATUS_PROCESS *pServiceInfo = NULL;
DWORD dwNumberOfService = 0;
char *pBuf = NULL;                  // 缓冲区指针
DWORD dwBufSize = 0;                // 传入的缓冲长度
DWORD dwBufNeed = 0;                // 需要的缓冲长度
wstring scGroupBuffer;
WCHAR currSc[MAX_PATH];
WCHAR currScPath[MAX_PATH];

M_CAPI(BOOL) MSCM_Init()
{
	hSCM = OpenSCManager(NULL, NULL, SC_MANAGER_CONNECT | SC_MANAGER_ENUMERATE_SERVICE);
	if (NULL == hSCM)
		return FALSE;
	return TRUE;
}
M_CAPI(void) MSCM_Exit()
{
	CloseServiceHandle(hSCM);
	if (pBuf) { free(pBuf); pBuf = NULL; }
}
M_CAPI(LPWSTR) MSCM_GetScGroup(LPWSTR path)
{
	wstring str; str = path;
	if (str != L"")
	{
		if (str.find(L"C:\\WINDOWS\\system32\\svchost.exe -k") != string::npos)
		{
			std::vector<std::wstring> buf;
			SplitString(str, buf, L" ");
			if (buf.size() >= 2)
			{
				wstring w = buf[buf.size() - 1];
				if (w == L"-p")
					scGroupBuffer = buf[buf.size() - 2];
				else scGroupBuffer = w;
				return (LPWSTR)scGroupBuffer.c_str();
			}
		}
	}
	return 0;
}
M_CAPI(BOOL) MEnumServices(EnumServicesCallBack callback)
{
	BOOL bRet = FALSE;
	if (callback && hSCM) {
		if (pBuf) free(pBuf);
		pBuf = NULL;		
		// 获取需要的缓冲区大小
		EnumServicesStatusEx(hSCM, SC_ENUM_PROCESS_INFO, SERVICE_WIN32, SERVICE_STATE_ALL,
			NULL, dwBufSize, &dwBufNeed, &dwNumberOfService, NULL, NULL);

		// 多设置存放1个服务信息的长度
		dwBufSize = dwBufNeed + sizeof(ENUM_SERVICE_STATUS_PROCESS);
		pBuf = (char *)malloc(dwBufSize);
		if (NULL == pBuf)
			return -2;
		memset(pBuf, 0, dwBufSize);

		// 获取服务信息
		bRet = EnumServicesStatusEx(hSCM, SC_ENUM_PROCESS_INFO, SERVICE_WIN32, SERVICE_STATE_ALL,
			(LPBYTE)pBuf, dwBufSize, &dwBufNeed, &dwNumberOfService, NULL, NULL);
		if (bRet == FALSE) {
			free(pBuf);
			pBuf = NULL;
			return -1;
		}
		pServiceInfo = (LPENUM_SERVICE_STATUS_PROCESS)pBuf;
		wchar_t err[32];
		for (unsigned int i = 0; i < dwNumberOfService; i++)
		{
			SC_HANDLE hSc = OpenService(hSCM, pServiceInfo[i].lpServiceName, SERVICE_QUERY_CONFIG);
			if (hSc) {
				DWORD sizeneed;
				QueryServiceConfig(hSc, NULL, 0, &sizeneed);
				LPQUERY_SERVICE_CONFIG cfg = (LPQUERY_SERVICE_CONFIG)malloc(sizeneed);
				if (QueryServiceConfig(hSc, cfg, sizeneed, &sizeneed)) {
					callback(pServiceInfo[i].lpDisplayName, pServiceInfo[i].lpServiceName, cfg->dwServiceType,
						pServiceInfo[i].ServiceStatusProcess.dwCurrentState, pServiceInfo[i].ServiceStatusProcess.dwProcessId,
						pServiceInfo[i].ServiceStatusProcess.dwServiceFlags == 1, cfg->dwStartType, cfg->lpBinaryPathName, MSCM_GetScGroup(cfg->lpBinaryPathName));
					
					free(cfg);
				}
				else {
					wsprintf(err, L"QueryServiceConfig err : %d .", GetLastError());
					free(cfg);
					CloseServiceHandle(hSc);
					goto DEFINFO;
				}
				CloseServiceHandle(hSc);
				continue;
			}
			else wsprintf(err, L"OpenService err : %d .", GetLastError());
			DEFINFO:
			callback(pServiceInfo[i].lpDisplayName, pServiceInfo[i].lpServiceName, pServiceInfo[i].ServiceStatusProcess.dwServiceType,
				pServiceInfo[i].ServiceStatusProcess.dwCurrentState, pServiceInfo[i].ServiceStatusProcess.dwProcessId, 
				pServiceInfo[i].ServiceStatusProcess.dwServiceFlags == 1, 0x80, err, NULL);
		}
	}
	return bRet;
}
M_CAPI(BOOL) MSCM_DeleteService(LPWSTR scname, LPWSTR errText)
{
	SC_HANDLE hSc = OpenService(hSCM, currSc,
		SERVICE_QUERY_STATUS | SERVICE_ENUMERATE_DEPENDENTS | SERVICE_START | SERVICE_STOP | DELETE);
	if (hSc)
	{
		SERVICE_STATUS status;
		QueryServiceStatus(hSc, &status);
		if (status.dwCurrentState != SERVICE_STOPPED)
			ControlService(hSc, SERVICE_CONTROL_STOP, &status);
		if (!DeleteService(hSc))
			ThrowErrorAndErrorCodeX(GetLastError(), L"DeleteService", errText, FALSE);
		else {
			MAppMainCall(15, currSc, 0);
		    return TRUE;
		}
		CloseServiceHandle(hSc);
	}
	else ThrowErrorAndErrorCodeX(GetLastError(), L"打开服务失败", errText, FALSE);
	return FALSE;
}
M_CAPI(BOOL) MSCM_ChangeScStartType(LPWSTR scname, DWORD type, LPWSTR errText)
{
	SC_HANDLE hSc = OpenService(hSCM, currSc, SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);
	if (hSc)
	{
		DWORD bufSize = 0;
		LPQUERY_SERVICE_CONFIG confg = NULL;
		QueryServiceConfig(hSc, confg, 0, &bufSize);
		confg = (LPQUERY_SERVICE_CONFIG)malloc(bufSize);
		QueryServiceConfig(hSc, confg, bufSize, &bufSize);

		if (confg->dwStartType != type)
			confg->dwStartType = type;

		if (!ChangeServiceConfig(hSc, SERVICE_NO_CHANGE, confg->dwStartType,
			SERVICE_NO_CHANGE, NULL, NULL, NULL, NULL, NULL, NULL, NULL))
		{
			ThrowErrorAndErrorCodeX(GetLastError(), L"ChangeServiceConfig", L"无法禁用服务", FALSE);
		}
		else return TRUE;

		CloseServiceHandle(hSc);
	}
	else ThrowErrorAndErrorCodeX(GetLastError(), L"打开服务失败（OpenService）", L"无法禁用服务", FALSE);
	return FALSE;
}
M_CAPI(BOOL) MSCM_ControlSc(LPWSTR scname, DWORD targetStatus, DWORD targetCtl, LPWSTR errText)
{
	SC_HANDLE hSc = OpenService(hSCM, currSc, SERVICE_ENUMERATE_DEPENDENTS |
		SERVICE_START | SERVICE_STOP | SERVICE_PAUSE_CONTINUE | SERVICE_QUERY_STATUS);
	if (hSc)
	{
		SERVICE_STATUS status;
		QueryServiceStatus(hSc, &status);
		if (status.dwCurrentState != targetStatus)
		{
			BOOL rs = ControlService(hSc, targetCtl, &status);
			CloseServiceHandle(hSc);
			if(!rs) ThrowErrorAndErrorCodeX(GetLastError(), L"ControlService", errText, FALSE);
			return rs;
		}
		else {
			CloseServiceHandle(hSc);
			return TRUE;
		}
	}
	else ThrowErrorAndErrorCodeX(GetLastError(), L"打开服务失败（OpenService）", errText, FALSE);
	return FALSE;
}
M_CAPI(void) MSCM_SetCurrSelSc(LPWSTR scname)
{
	wcscpy_s(currSc, scname);
}

LRESULT MSCM_HandleWmCommand(WPARAM wParam)
{
	switch (wParam)
	{
	case ID_SCMAIN_COPYPATH: {
		if (wcslen(currScPath) > 0 || wcscmp(currScPath, L"") != 0)
			MCopyToClipboard(currScPath, wcslen(currScPath));
		break;
	}
	case ID_SCMAIN_DEL: {
		if (wcslen(currSc) > 0 || wcscmp(currSc, L"") != 0)
			MSCM_DeleteService(currSc, L"删除服务失败");
		break;
	}
	case ID_SCMAIN_DISABLE: {
		if (wcslen(currSc) > 0 || wcscmp(currSc, L"") != 0)
			MSCM_ChangeScStartType(currSc, SERVICE_DISABLED, L"");
		break;
	}
	case ID_SCMAIN_AUTOSTART: {
		if (wcslen(currSc) > 0 || wcscmp(currSc, L"") != 0) 
			MSCM_ChangeScStartType(currSc, SERVICE_AUTO_START, L"");
		break;
	}
	case ID_SCMAIN_NOAUTOSTART: {
		if (wcslen(currSc) > 0 || wcscmp(currSc, L"") != 0)
			MSCM_ChangeScStartType(currSc, SERVICE_DEMAND_START, L"");
		break;
	}
	case ID_SCMAIN_REBOOT: 
	case ID_SCSMALL_REBOOTSC: {
		if (wcslen(currSc) > 0 || wcscmp(currSc, L"") != 0) {
			SC_HANDLE hSc = OpenService(hSCM, currSc, SERVICE_ENUMERATE_DEPENDENTS |
				SERVICE_START | SERVICE_STOP | SERVICE_PAUSE_CONTINUE | SERVICE_QUERY_STATUS);
			if (hSc)
			{
				SERVICE_STATUS status;
				QueryServiceStatus(hSc, &status);
				if (status.dwCurrentState != SERVICE_STOPPED)
				{
					if (!ControlService(hSc, SERVICE_CONTROL_STOP, &status)) {
						ThrowErrorAndErrorCodeX(GetLastError(), L"ControlService", L"无法停止服务", FALSE);
						return FALSE;
					}
					else return StartService(hSc, 0, NULL);
				}
				else {
					CloseServiceHandle(hSc);
					return TRUE;
				}
			}
			else ThrowErrorAndErrorCodeX(GetLastError(), L"打开服务失败（OpenService）", L"无法停止服务", FALSE);
		}
		break;
	}
	case ID_SCMAIN_START: {
		if (wcslen(currSc) > 0 || wcscmp(currSc, L"") != 0) 
		{
			SC_HANDLE hSc = OpenService(hSCM, currSc, SERVICE_ENUMERATE_DEPENDENTS |
				SERVICE_START | SERVICE_STOP | SERVICE_PAUSE_CONTINUE | SERVICE_QUERY_STATUS);
			if (hSc)
			{
				SERVICE_STATUS status;
				QueryServiceStatus(hSc, &status);
				if (status.dwCurrentState != SERVICE_RUNNING)
					return StartService(hSc, 0, NULL);
				else {
					CloseServiceHandle(hSc);
					return TRUE;
				}
			}
			else ThrowErrorAndErrorCodeX(GetLastError(), L"打开服务失败（OpenService）", L"无法启动服务", FALSE);
		}
		break;
	}	
	case ID_SCSMALL_STOPSC:
	case ID_SCMAIN_STOP: {
		if (wcslen(currSc) > 0 || wcscmp(currSc, L"") != 0) 
			MSCM_ControlSc(currSc, SERVICE_STOPPED, SERVICE_CONTROL_STOP, L"无法停止服务");
		break;
	}
	case ID_SCMAIN_REFESH: {
		MAppMainCall(10, 0, 0);
		break;
	}
	case ID_SCMAIN_RESU: {
		if (wcslen(currSc) > 0 || wcscmp(currSc, L"") != 0)
			MSCM_ControlSc(currSc, SERVICE_RUNNING, SERVICE_CONTROL_CONTINUE, L"无法恢复服务");
		break;
	}
	case ID_SCMAIN_SUSP: {
		if (wcslen(currSc) > 0 || wcscmp(currSc, L"") != 0) 
			MSCM_ControlSc(currSc, SERVICE_PAUSED, SERVICE_CONTROL_PAUSE, L"无法暂停服务");
		break;
	}
	case ID_SCSMALL_GOTOSC: {
		MAppMainCall(9, currSc, 0);
		break;
	}
	}    
	return 0;
}
M_CAPI(void) MSCM_ShowMenu(HWND hDlg, LPWSTR serviceName, DWORD running, DWORD startType, LPWSTR path)
{
	HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUSCMAIN));
	if (hroot) {
		HMENU hpop = GetSubMenu(hroot, 0);
		POINT pt;
		GetCursorPos(&pt);

		if (running == SERVICE_STOPPED)
		{
			EnableMenuItem(hpop, ID_SCMAIN_STOP, MF_DISABLED);
			EnableMenuItem(hpop, ID_SCMAIN_SUSP, MF_DISABLED);
			EnableMenuItem(hpop, ID_SCMAIN_RESU, MF_DISABLED);
		}
		if (running == SERVICE_RUNNING) {
			EnableMenuItem(hpop, ID_SCMAIN_START, MF_DISABLED);
			EnableMenuItem(hpop, ID_SCMAIN_RESU, MF_DISABLED);
		}
		if (running == SERVICE_PAUSED)
			EnableMenuItem(hpop, ID_SCMAIN_SUSP, MF_DISABLED);

		if (startType == SERVICE_AUTO_START)
			EnableMenuItem(hpop, ID_SCMAIN_AUTOSTART, MF_DISABLED);
		else if (startType == SERVICE_DEMAND_START)
			EnableMenuItem(hpop, ID_SCMAIN_NOAUTOSTART, MF_DISABLED);
		else if (startType == SERVICE_DISABLED)
			EnableMenuItem(hpop, ID_SCMAIN_DISABLE, MF_DISABLED);
			
		if(path==NULL||wcslen(path)==0)
			EnableMenuItem(hpop, ID_SCMAIN_COPYPATH, MF_DISABLED);
		else
			wcscpy_s(currScPath, path);

		wcscpy_s(currSc, serviceName);

		TrackPopupMenu(hpop,
			TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
			pt.x,
			pt.y,
			0,
			hDlg,
			NULL);

		DestroyMenu(hroot);
	}
}

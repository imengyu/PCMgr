#include "stdafx.h"
#include "schlp.h"
#include "mapphlp.h"
#include "lghlp.h"
#include "loghlp.h"
#include "StringSplit.h"
#include "resource.h"
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <algorithm>  

using namespace std;

extern HINSTANCE hInstRs;
extern "C" M_API BOOL MCopyToClipboard(const WCHAR* pszData, const size_t nDataLen);

SC_HANDLE hSCM = NULL;
ENUM_SERVICE_STATUS_PROCESS *pServiceInfo = NULL;
DWORD dwNumberOfService = 0;
DWORD dwNumberOfDriverService = 0;
char *pBuf = NULL;                  // 缓冲区指针
DWORD dwBufSize = 0;                // 传入的缓冲长度
DWORD dwBufNeed = 0;                // 需要的缓冲长度
wstring scGroupBuffer;
WCHAR currSc[MAX_PATH];
WCHAR currScPath[MAX_PATH];



LPENUM_SERVICE_STATUS_PROCESS pBufDrvscs = NULL;
LPSERVICE_STORAGE pDrvscsNames = NULL;

M_CAPI(BOOL) MSCM_Init()
{
	hSCM = OpenSCManager(NULL, NULL, SC_MANAGER_CONNECT | SC_MANAGER_ENUMERATE_SERVICE);
	if (NULL == hSCM) {
		LogErr(L"OpenSCManager failed ! Last error : %d .", GetLastError());
		return FALSE;
	}
	return TRUE;
}
M_CAPI(void) MSCM_Exit()
{
	if(hSCM) CloseServiceHandle(hSCM);
	if (pBuf) { MFree(pBuf); pBuf = NULL; }
	if (pBufDrvscs) { MFree(pBufDrvscs); pBufDrvscs = NULL; }
	if (pDrvscsNames) { MFree(pDrvscsNames); pDrvscsNames = NULL; }
}
M_CAPI(LPWSTR) MSCM_GetScGroup(LPWSTR path)
{
	wstring str; 
	str = path;
	wstring strOld;
	strOld = path;
	if (str != L"")
	{
		transform(str.begin(), str.end(), str.begin(), tolower);
		if (str.find(L"c:\\windows\\system32\\svchost.exe -k") != string::npos)
		{
			std::vector<std::wstring> buf;
			SplitString(strOld, buf, L" ");
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

BOOL MSCM_EnumDriverServicesFreeAll() {
	if (pBufDrvscs) {
		MFree(pBufDrvscs); 
		pBufDrvscs = NULL;
	}
	if (pDrvscsNames)
	{
		for (unsigned int i = 0; i < dwNumberOfDriverService; i++)
		{
			if (pDrvscsNames[i].ServiceHandle)
				CloseServiceHandle(pDrvscsNames[i].ServiceHandle);
		}
		MFree(pDrvscsNames);
		pDrvscsNames = NULL;
	}
	return TRUE;
}
M_CAPI(BOOL) MSCM_CheckDriverServices(LPWSTR fileName, LPWSTR outName, LPSERVICE_STORAGE*pScInfo)
{
	for (unsigned int i = 0; i < dwNumberOfDriverService; i++)
	{
		if (MStrEqualW(pDrvscsNames[i].ServiceImagePath, fileName))
		{
			pDrvscsNames[i].DriverServiceFounded = TRUE;
			if (outName) wcscpy_s(outName,MAX_PATH,pDrvscsNames[i].lpServiceName);
			if (pScInfo)*pScInfo = &pDrvscsNames[i];
			return TRUE;
		}
	}
	return FALSE;
}
M_CAPI(BOOL) MSCM_EnumDriverServices() {
	if (hSCM)
	{
		DWORD dwBufSize = 0;
		DWORD dwBufNeed = 0;

		MSCM_EnumDriverServicesFreeAll();

		// 获取需要的缓冲区大小
		EnumServicesStatusEx(hSCM, SC_ENUM_PROCESS_INFO, SERVICE_DRIVER, SERVICE_STATE_ALL,
			NULL, dwBufSize, &dwBufNeed, &dwNumberOfDriverService, NULL, NULL);

		dwBufSize = dwBufNeed + sizeof(ENUM_SERVICE_STATUS_PROCESS);
		pBufDrvscs = (LPENUM_SERVICE_STATUS_PROCESS)MAlloc(dwBufSize);
		if (NULL == pBufDrvscs)
			return FALSE;
		memset(pBufDrvscs, 0, dwBufSize);

		BOOL bRet = EnumServicesStatusEx(hSCM, SC_ENUM_PROCESS_INFO, SERVICE_DRIVER, SERVICE_STATE_ALL,
			(LPBYTE)pBufDrvscs, dwBufSize, &dwBufNeed, &dwNumberOfDriverService, NULL, NULL);
		if (bRet == FALSE) {
			LogErr(L"EnumServicesStatusEx error : %d", GetLastError());
			MFree(pBufDrvscs);
			pBufDrvscs = NULL;
			return FALSE;
		}

		if (dwNumberOfDriverService > 0) {
			size_t size = (dwNumberOfDriverService + 1) * sizeof(SERVICE_STORAGE);
			pDrvscsNames = (LPSERVICE_STORAGE)MAlloc(size);
			memset(pDrvscsNames, 0, size);
			if (pDrvscsNames) {
				for (unsigned int i = 0; i < dwNumberOfDriverService; i++)
				{
					pDrvscsNames[i].DriverServiceFounded = FALSE;
					pDrvscsNames[i].lpServiceName = pBufDrvscs[i].lpServiceName;
					pDrvscsNames[i].lpSvc = &pBufDrvscs[i];
					SC_HANDLE hSc = OpenService(hSCM, pBufDrvscs[i].lpServiceName, SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);
					if (hSc) {
						pDrvscsNames[i].ServiceHandle = hSc;
						DWORD sizeneed;
						QueryServiceConfig(hSc, NULL, 0, &sizeneed);
						LPQUERY_SERVICE_CONFIG cfg = (LPQUERY_SERVICE_CONFIG)MAlloc(sizeneed);
						if (QueryServiceConfig(hSc, cfg, sizeneed, &sizeneed)) {
							wcscpy_s(pDrvscsNames[i].ServiceImagePath, cfg->lpBinaryPathName);
							pDrvscsNames[i].ServiceStartType = cfg->dwStartType;
						}
						MFree(cfg);
					}
				}
				return TRUE;
			}
		}
	}
	return 0;
}
M_CAPI(BOOL) MEnumServices(EnumServicesCallBack callback)
{
	BOOL bRet = FALSE;
	if (callback && hSCM) {
		if (pBuf) MFree(pBuf);
		pBuf = NULL;		
		// 获取需要的缓冲区大小
		EnumServicesStatusEx(hSCM, SC_ENUM_PROCESS_INFO, SERVICE_WIN32, SERVICE_STATE_ALL,
			NULL, dwBufSize, &dwBufNeed, &dwNumberOfService, NULL, NULL);

		// 多设置存放1个服务信息的长度
		dwBufSize = dwBufNeed + sizeof(ENUM_SERVICE_STATUS_PROCESS);
		pBuf = (char *)MAlloc(dwBufSize);
		if (NULL == pBuf)
			return -2;
		memset(pBuf, 0, dwBufSize);

		// 获取服务信息
		bRet = EnumServicesStatusEx(hSCM, SC_ENUM_PROCESS_INFO, SERVICE_WIN32, SERVICE_STATE_ALL,
			(LPBYTE)pBuf, dwBufSize, &dwBufNeed, &dwNumberOfService, NULL, NULL);
		if (bRet == FALSE) {
			LogErr(L"EnumServicesStatusEx error : %d", GetLastError());
			MFree(pBuf);
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
				LPQUERY_SERVICE_CONFIG cfg = (LPQUERY_SERVICE_CONFIG)MAlloc(sizeneed);
				if (QueryServiceConfig(hSc, cfg, sizeneed, &sizeneed)) {
					callback(pServiceInfo[i].lpDisplayName, pServiceInfo[i].lpServiceName, cfg->dwServiceType,
						pServiceInfo[i].ServiceStatusProcess.dwCurrentState, pServiceInfo[i].ServiceStatusProcess.dwProcessId,
						pServiceInfo[i].ServiceStatusProcess.dwServiceFlags == 1, cfg->dwStartType, cfg->lpBinaryPathName, MSCM_GetScGroup(cfg->lpBinaryPathName));
					
					MFree(cfg);
				}
				else {
					wsprintf(err, L"QueryServiceConfig err : %d .", GetLastError());
					MFree(cfg);
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
		if (!DeleteService(hSc)) {
			LogErr(L"DeleteService error : %d (%s)", GetLastError(), scname);
			ThrowErrorAndErrorCodeX(GetLastError(), L"DeleteService", errText, FALSE);
		}
		else {
			MAppMainCall(M_CALLBACK_SCITEM_REMOVED, currSc, 0);
		    return TRUE;
		}
		CloseServiceHandle(hSc);
	}
	else {
		LogErr(L"OpenService error : %d (%s)", GetLastError(), scname);
		ThrowErrorAndErrorCodeX(GetLastError(), str_item_opensc_err, errText, FALSE);
	}
	return FALSE;
}
M_CAPI(BOOL) MSCM_ChangeScStartType(LPWSTR scname, DWORD type, LPWSTR errText)
{
	SC_HANDLE hSc = OpenService(hSCM, currSc, SERVICE_QUERY_CONFIG);
	if (hSc)
	{
		DWORD bufSize = 0;
		LPQUERY_SERVICE_CONFIG confg = NULL;
		QueryServiceConfig(hSc, confg, 0, &bufSize);
		confg = (LPQUERY_SERVICE_CONFIG)MAlloc(bufSize);
		QueryServiceConfig(hSc, confg, bufSize, &bufSize);

		if (confg->dwStartType != type)
			confg->dwStartType = type;

		if (!ChangeServiceConfig(hSc, SERVICE_NO_CHANGE, confg->dwStartType,
			SERVICE_NO_CHANGE, NULL, NULL, NULL, NULL, NULL, NULL, NULL))
		{
			LogErr(L"ChangeServiceConfig error : %d", GetLastError());
			ThrowErrorAndErrorCodeX(GetLastError(), L"ChangeServiceConfig ERROR", errText, FALSE);
		}
		else return TRUE;

		CloseServiceHandle(hSc);
	}
	else {
		LogErr(L"OpenService error : %d (%s)", GetLastError(), scname);
		ThrowErrorAndErrorCodeX(GetLastError(), str_item_opensc_err, errText, FALSE);
	}
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
			if (!rs) {
				LogErr(L"ControlService error : %d", GetLastError());
				ThrowErrorAndErrorCodeX(GetLastError(), L"ControlService", errText, FALSE);
			}
			return rs;
		}
		else {
			CloseServiceHandle(hSc);
			return TRUE;
		}
	}
	else {
		LogErr(L"OpenService error : %d (%s)", GetLastError(), scname);
		ThrowErrorAndErrorCodeX(GetLastError(), str_item_opensc_err, errText, FALSE);
	}
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
		if (wcslen(currScPath) > 0 || !MStrEqualW(currScPath, L""))
			MCopyToClipboard(currScPath, wcslen(currScPath));
		break;
	}
	case ID_SCMAIN_DEL: {
		if (wcslen(currSc) > 0 || !MStrEqualW(currSc, L""))
			MSCM_DeleteService(currSc, str_item_delsc_err);
		break;
	}
	case ID_SCMAIN_DISABLE: {
		if (wcslen(currSc) > 0 || !MStrEqualW(currSc, L""))
			MSCM_ChangeScStartType(currSc, SERVICE_DISABLED, str_item_setscstart_err);
		break;
	}
	case ID_SCMAIN_AUTOSTART: {
		if (wcslen(currSc) > 0 || !MStrEqualW(currSc, L"")) 
			MSCM_ChangeScStartType(currSc, SERVICE_AUTO_START, str_item_setscstart_err);
		break;
	}
	case ID_SCMAIN_NOAUTOSTART: {
		if (wcslen(currSc) > 0 || !MStrEqualW(currSc, L""))
			MSCM_ChangeScStartType(currSc, SERVICE_DEMAND_START, str_item_setscstart_err);
		break;
	}
	case ID_SCMAIN_REBOOT: 
	case ID_SCSMALL_REBOOTSC: {
		if (wcslen(currSc) > 0 || !MStrEqualW(currSc, L"")) {
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
			else ThrowErrorAndErrorCodeX(GetLastError(), str_item_opensc_err, (LPWSTR)str_item_op_failed.c_str(), FALSE);
		}
		break;
	}
	case ID_SCMAIN_START: {
		if (wcslen(currSc) > 0 || !MStrEqualW(currSc, L"")) 
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
			else ThrowErrorAndErrorCodeX(GetLastError(), str_item_opensc_err, (LPWSTR)str_item_op_failed.c_str(), FALSE);
		}
		break;
	}	
	case ID_SCSMALL_STOPSC:
	case ID_SCMAIN_STOP: {
		if (wcslen(currSc) > 0 || !MStrEqualW(currSc, L"")) 
			MSCM_ControlSc(currSc, SERVICE_STOPPED, SERVICE_CONTROL_STOP, (LPWSTR)str_item_op_failed.c_str());
		break;
	}
	case ID_SCMAIN_REFESH: {
		MAppMainCall(M_CALLBACK_REFESH_SCLIST, 0, 0);
		break;
	}
	case ID_SCMAIN_RESU: {
		if (wcslen(currSc) > 0 || !MStrEqualW(currSc, L""))
			MSCM_ControlSc(currSc, SERVICE_RUNNING, SERVICE_CONTROL_CONTINUE, (LPWSTR)str_item_op_failed.c_str());
		break;
	}
	case ID_SCMAIN_SUSP: {
		if (wcslen(currSc) > 0 || !MStrEqualW(currSc, L"")) 
			MSCM_ControlSc(currSc, SERVICE_PAUSED, SERVICE_CONTROL_PAUSE, (LPWSTR)str_item_op_failed.c_str());
		break;
	}
	case ID_SCSMALL_GOTOSC: {
		MAppMainCall(M_CALLBACK_GOTO_SERVICE, currSc, 0);
		break;
	}
	}    
	return 0;
}
M_CAPI(void) MSCM_ShowMenu(HWND hDlg, LPWSTR serviceName, DWORD running, DWORD startType, LPWSTR path, int x,int y)
{
	HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUSCMAIN));
	if (hroot) {
		HMENU hpop = GetSubMenu(hroot, 0);
		POINT pt;
		if (x == 0 && y == 0)
			GetCursorPos(&pt);
		else {
			pt.x = x;
			pt.y = y;
		}

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

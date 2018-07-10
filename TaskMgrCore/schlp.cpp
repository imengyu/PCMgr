#include "stdafx.h"
#include "schlp.h"
#include "mapphlp.h"
#include "StringSplit.h"
#include <stdio.h>
#include <stdlib.h>
#include <string>

using namespace std;

SC_HANDLE hSCM = NULL;
ENUM_SERVICE_STATUS_PROCESS *pServiceInfo = NULL;
DWORD dwNumberOfService = 0;
char *pBuf = NULL;                  // 缓冲区指针
DWORD dwBufSize = 0;                // 传入的缓冲长度
DWORD dwBufNeed = 0;                // 需要的缓冲长度
wstring scGroupBuffer;

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

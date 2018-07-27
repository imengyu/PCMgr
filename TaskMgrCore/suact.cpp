#include "stdafx.h"
#include "suact.h"
#include "prochlp.h"
#include "thdhlp.h"
#include "ntdef.h"
#include "kernelhlp.h"
#include "ioctls.h"
#include "lghlp.h"
#include "loghlp.h"
#include "sysstructs.h"
#include "resource.h"
#include "mapphlp.h"
#include "fmhlp.h"
#include "PathHelper.h"
#include "StringHlp.h"

extern ZwTerminateThreadFun ZwTerminateThread;
extern ZwTerminateProcessFun ZwTerminateProcess;

extern BOOL isKernelDriverLoaded;
extern HANDLE hKernelDevice;
extern HWND hWndMain;
extern HINSTANCE hInstRs;

M_CAPI(BOOL) M_SU_CreateFile(LPCWSTR lpFileName,	DWORD dwDesiredAccess, DWORD dwShareMode,	 DWORD dwCreationDisposition, PHANDLE pHandle)
{
	HANDLE h = CreateFileW(lpFileName, dwDesiredAccess, dwShareMode, NULL, dwCreationDisposition, FILE_ATTRIBUTE_NORMAL, NULL);
	if (h) {
		*pHandle = h;
		return TRUE;
	}
	else {

	}
	return 0;
}
M_CAPI(BOOL) M_SU_OpenProcess(DWORD pid, PHANDLE pHandle, NTSTATUS* pStatus)
{
	Log(L"M_SU_OpenProcess process id : %u", pid);
	NTSTATUS status = MOpenProcessNt(pid, pHandle);
	if (status != STATUS_SUCCESS && isKernelDriverLoaded)
	{
		Log(L"MOpenProcessNt failed 0x%08X ,use kernel to open process : %u", status,  pid);
		HANDLE OutBufferData = NULL;
		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, CTL_OPEN_PROCESS, &pid, sizeof(DWORD), &OutBufferData, sizeof(HANDLE), &ReturnLength, NULL))
		{
			if (pHandle)*pHandle = OutBufferData;
			if (pStatus)*pStatus = STATUS_SUCCESS;
			return TRUE;
		}
		else LogErr(L"M_SU_OpenProcess DeviceIoControl err : %d", GetLastError());
	}
	if (pStatus)*pStatus = status;
	return status == 0;
}
M_CAPI(BOOL) M_SU_OpenThread(DWORD pid, DWORD tid, PHANDLE pHandle, NTSTATUS* pStatus)
{
	NTSTATUS status = MOpenThreadNt(tid, pHandle, pid);
	if (status != STATUS_SUCCESS && isKernelDriverLoaded)
	{
		HANDLE OutBufferData = NULL;
		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, CTL_OPEN_THREAD, &tid, sizeof(DWORD), &OutBufferData, sizeof(HANDLE), &ReturnLength, NULL))
		{
			if (pHandle)*pHandle = OutBufferData;
			if (pStatus)*pStatus = STATUS_SUCCESS;
			return TRUE;
		}
		else LogErr(L"M_SU_OpenThread DeviceIoControl err : %d", GetLastError());
	}
	if (pStatus)*pStatus = status;
	return status == STATUS_SUCCESS;
}
M_CAPI(BOOL) M_SU_TerminateProcess(HANDLE hProcess, UINT exitCode, NTSTATUS* pStatus)
{
	Log(L"M_SU_TerminateProcess process id : 0x%08X", hProcess);
	BOOL rs = FALSE;
	NTSTATUS status = ZwTerminateProcess(hProcess, exitCode);
	if (status == STATUS_SUCCESS) {
		if (pStatus)*pStatus = status;
		rs = TRUE;
	}
	else if(isKernelDriverLoaded) {
		Log(L"ZwTerminateProcess failed 0x%08X ,use kernel to Terminate process : 0x%08X", status, hProcess);
		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, CTL_TREMINATE_PROCESS, &hProcess, sizeof(HANDLE), &status, sizeof(status), &ReturnLength, NULL))
			rs = TRUE;
		else {
			LogErr(L"M_SU_TerminateProcess DeviceIoControl err : %d", GetLastError());
		}
		if (pStatus)*pStatus = status;
	}
	return rs;
}
M_CAPI(BOOL) M_SU_TerminateThread(HANDLE hThread, UINT exitCode, NTSTATUS* pStatus)
{
	Log(L"M_SU_TerminateThread thread id : 0x%08X", hThread);
	BOOL rs = FALSE;
	NTSTATUS status = ZwTerminateThread(hThread, exitCode);
	if (status == STATUS_SUCCESS) {
		if (pStatus)*pStatus = status;
		rs = TRUE;
	}
	else if(isKernelDriverLoaded) {

		Log(L"ZwTerminateThread failed 0x%08X , use kernel to Terminate thread : 0x%08X", status, hThread);
		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, CTL_TREMINATE_THREAD, &hThread, sizeof(HANDLE), &status, sizeof(status), &ReturnLength, NULL))
			rs = TRUE;
		else LogErr(L"M_SU_TerminateThread DeviceIoControl err : %d", GetLastError());
		if (pStatus)*pStatus = status;
	}
	return rs;
}
M_CAPI(BOOL) M_SU_TerminateProcessPID(DWORD pid, UINT exitCode, NTSTATUS* pStatus, BOOL useApc)
{
	BOOL rs = FALSE;
	NTSTATUS status = STATUS_SUCCESS;
	Log(L"M_SU_TerminateProcessPID process id : %d", pid);
	if (isKernelDriverLoaded) {
		ULONG_PTR pid2 = pid;
		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, useApc ? CTL_FORCE_TREMINATE_PROCESS_APC : CTL_FORCE_TREMINATE_PROCESS, &pid2, sizeof(pid2), &status, sizeof(status), &ReturnLength, NULL))
			rs = TRUE;
		else {
			LogErr(L"M_SU_TerminateProcessPID DeviceIoControl err : %d", GetLastError());
		}
		if (pStatus)*pStatus = status;
	}
	return rs;
}
M_CAPI(BOOL) M_SU_TerminateThreadTID(DWORD tid, UINT exitCode, NTSTATUS* pStatus, BOOL useApc)
{
	Log(L"M_SU_TerminateThreadTID thread id : %d", tid);
	BOOL rs = FALSE;
	ULONG_PTR tid2 = tid;
	NTSTATUS status = STATUS_SUCCESS;
	if (isKernelDriverLoaded) {
		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, useApc ? CTL_FORCE_TREMINATE_THREAD_APC : CTL_FORCE_TREMINATE_THREAD, &tid2, sizeof(tid2), &status, sizeof(status), &ReturnLength, NULL))
			rs = TRUE;
		else LogErr(L"M_SU_TerminateThreadTID DeviceIoControl err : %d", GetLastError());
		if (pStatus)*pStatus = status;
	}
	return rs;
}
M_CAPI(BOOL) M_SU_CloseHandleWithProcess(_SYSTEM_HANDLE_TABLE_ENTRY_INFO * sh)
{
	BOOL rs = FALSE;
	HANDLE hFile = (HANDLE)sh->HandleValue;
	HANDLE hProcess;
	NTSTATUS status = 0;
	if (M_SU_OpenProcess(sh->UniqueProcessId, &hProcess, &status)) {
		HANDLE hDup = 0;
		BOOL b = DuplicateHandle(hProcess, hFile, GetCurrentProcess(),
			&hDup, DUPLICATE_SAME_ACCESS, FALSE, DUPLICATE_CLOSE_SOURCE);
		if (hDup) rs = MCloseHandle(hDup);
		else LogErr(L"CloseHandleWithProcess failed in DuplicateHandle (%d) PID %d HANDLE : 0x%08X", GetLastError(), sh->UniqueProcessId, hFile);
		MCloseHandle(hProcess);
	}
	else LogErr(L"CloseHandleWithProcess failed (%d) PID %d HANDLE : 0x%08X", GetLastError(), sh->UniqueProcessId, hFile);
	return rs;
}
M_CAPI(BOOL) M_SU_SuspendProcess(DWORD pid, UINT exitCode, NTSTATUS* pStatus)
{
	BOOL rs = FALSE;
	NTSTATUS status = 0;
	DWORD ReturnLength = 0;
	ULONG_PTR pidInBuffer = pid;
	if (DeviceIoControl(hKernelDevice, CTL_SUSPEND_PROCESS, &pidInBuffer, sizeof(pidInBuffer), &status, sizeof(status), &ReturnLength, NULL))
		rs = TRUE;
	else LogErr(L"M_SU_SuspendProcess DeviceIoControl err : %d", GetLastError());
	if (pStatus)*pStatus = status;
	return rs;
}
M_CAPI(BOOL) M_SU_ResumeProcess(DWORD pid, UINT exitCode, NTSTATUS* pStatus)
{
	BOOL rs = FALSE;
	NTSTATUS status = 0;
	DWORD ReturnLength = 0;
	ULONG_PTR pidInBuffer = pid;
	if (DeviceIoControl(hKernelDevice, CTL_RESUME_PROCESS, &pidInBuffer, sizeof(pidInBuffer), &status, sizeof(status), &ReturnLength, NULL))
		rs = TRUE;
	else LogErr(L"M_SU_ResumeProcess DeviceIoControl err : %d", GetLastError());
	if (pStatus)*pStatus = status;
	return rs;
}
BOOL M_SU_ForceShutdown() {
	DWORD ReturnLength = 0;
	if (DeviceIoControl(hKernelDevice, CTL_FORCE_SHUTDOWN, 0, 0, 0, 0, &ReturnLength, NULL))
		return TRUE;
	else LogErr(L"M_SU_ForceShutdown DeviceIoControl err : %d", GetLastError());
	return FALSE;
}
BOOL M_SU_ForceReboot() {
	DWORD ReturnLength = 0;
	if (DeviceIoControl(hKernelDevice, CTL_FORCE_REBOOT, 0, 0, 0, 0, &ReturnLength, NULL))
		return TRUE;
	else LogErr(L"M_SU_ForceShutdown DeviceIoControl err : %d", GetLastError());
	return FALSE;
}
BOOL M_SU_ProtectMySelf() {
	DWORD ReturnLength = 0;
	ULONG_PTR pid = GetCurrentProcessId();
	if (DeviceIoControl(hKernelDevice, CTL_ADD_PROCESS_PROTECT, &pid, sizeof(pid), 0, 0, &ReturnLength, NULL))
		return TRUE;
	else LogErr(L"M_SU_ProtectMySelf DeviceIoControl err : %d", GetLastError());
	return FALSE;
}
BOOL M_SU_UnProtectMySelf() {
	DWORD ReturnLength = 0;
	ULONG_PTR pid = GetCurrentProcessId();
	if (DeviceIoControl(hKernelDevice, CTL_REMOVE_PROCESS_PROTECT, &pid, sizeof(pid), 0, 0, &ReturnLength, NULL))
		return TRUE;
	else LogErr(L"M_SU_UnProtectMySelf DeviceIoControl err : %d", GetLastError());
	return FALSE;
}

ULONG lastSetSysver = 0;
PKernelModulSmallInfo selectedKmi = 0;
extern LPSERVICE_STORAGE pDrvscsNames;
extern DWORD dwNumberOfDriverService;

M_CAPI(BOOL) M_SU_GetServiceReg(LPWSTR servicName, LPWSTR buf, size_t size)
{
	std::wstring s = FormatString(L"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\services\\%s", servicName);
	if (buf && size > s.size())
	{
		wcscpy_s(buf, size, s.c_str());
		return TRUE;
	}
	return 0;
}
M_CAPI(VOID) M_SU_Test(LPCSTR instr) {
	char str[128];
	strcpy_s(str, instr);
	DWORD ReturnLength = 0;
	if (DeviceIoControl(hKernelDevice, CTL_TEST, &str, sizeof(str), NULL, 0, &ReturnLength, NULL)) {
		Log(L"M_SU_Test DeviceIoControl Success");
	}
}
M_CAPI(VOID) M_SU_SetSysver(ULONG ver)
{
	lastSetSysver = ver;
}
M_CAPI(BOOL) M_SU_Init() {
	Log(L"M_SU_Init DeviceIoControl");
	DWORD ReturnLength = 0;
	ULONG inputBuffer = lastSetSysver;
	if (DeviceIoControl(hKernelDevice, CTL_KERNEL_INIT, &inputBuffer, sizeof(inputBuffer), NULL, 0, &ReturnLength, NULL))
		return TRUE;
	LogErr(L"M_SU_Init DeviceIoControl err : %d", GetLastError());
	return FALSE;
}
M_CAPI(BOOL) M_SU_GetEPROCESS(DWORD pid, ULONG_PTR* lpEprocess)
{
	BOOL rs = FALSE;
	if (isKernelDriverLoaded)
	{
		ULONG_PTR* OutBufferData = (ULONG_PTR*)malloc(sizeof(ULONG_PTR));
		*OutBufferData = 0;
		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, CTL_GET_EPROCESS, &pid, sizeof(DWORD), OutBufferData, sizeof(ULONG_PTR), &ReturnLength, NULL))
		{
			if (lpEprocess)(*lpEprocess) = (ULONG_PTR)*OutBufferData;
			rs = TRUE;
		} else LogErr(L"M_SU_GetEPROCESS DeviceIoControl err : %d", GetLastError());
		free(OutBufferData);
	}
	return rs;
}
M_CAPI(BOOL) M_SU_GetETHREAD(DWORD tid, ULONG_PTR* lpEthread)
{
	BOOL rs = FALSE;
	if (isKernelDriverLoaded)
	{
		ULONG_PTR* OutBufferData = (ULONG_PTR*)malloc(sizeof(ULONG_PTR));
		*OutBufferData = 0;
		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, CTL_GET_ETHREAD, &tid, sizeof(DWORD), OutBufferData, sizeof(ULONG_PTR), &ReturnLength, NULL))
		{
			if (lpEthread)(*lpEthread) = (ULONG_PTR)*OutBufferData;
			
			rs = TRUE;
		}
		else LogErr(L"M_SU_GetETHREAD DeviceIoControl err : %d", GetLastError());
		free(OutBufferData);	
	}
	return rs;
}
M_CAPI(void) M_SU_EnumKernelModulsItemDestroy(KernelModulSmallInfo*km)
{
	if (km) free(km);
}
M_CAPI(BOOL) M_SU_EnumKernelModuls(EnumKernelModulsCallBack callback, BOOL showall) {

	if(!callback)	return FALSE;

	MSCM_EnumDriverServices();

	KERNEL_MODULE kModule;
	memset(&kModule, 0, sizeof(kModule));
	UCHAR BoolsStart = TRUE;

	UINT notLoadDrvCount = 0;
	UINT drvCount = 0;
	UINT loopCount = 0;

	while (kModule.Order != 9999)
	{
		if (loopCount > 1024)return FALSE;
		loopCount++;

		memset(&kModule, 0, sizeof(kModule));

		PKernelModulSmallInfo kmi = (PKernelModulSmallInfo)malloc(sizeof(KernelModulSmallInfo));
		memset(kmi, 0, sizeof(KernelModulSmallInfo));

		WCHAR strEntryPoint[16];
		WCHAR strSizeOfImage[16];
		WCHAR strDriverObject[16];
		WCHAR strBase[16];
		WCHAR strPath[MAX_PATH];
		memset(strPath, 0, sizeof(strPath));

		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, CTL_GET_KERNEL_MODULS, &BoolsStart, sizeof(BoolsStart), &kModule, sizeof(kModule), &ReturnLength, NULL))
		{
			wcscpy_s(kmi->szFullDllPathOrginal, kModule.FullDllPath);
			//wcscpy_s(kmi->szFullDllPath, kModule.FullDllPath);
			MNtPathToFilePath(kmi->szFullDllPathOrginal, kmi->szFullDllPath, MAX_PATH);

			if (kModule.EntryPoint == 0) wcscpy_s(strDriverObject, L"-");
			else wsprintf(strEntryPoint, L"0x%08X", kModule.EntryPoint);
			wsprintf(strSizeOfImage, L"0x%08X", kModule.SizeOfImage);
			if (kModule.DriverObject == 0) wcscpy_s(strDriverObject, L"-");
			else wsprintf(strDriverObject, L"0x%08X", kModule.DriverObject);
			if (kModule.Base == 0) wcscpy_s(strDriverObject, L"-");
			else wsprintf(strBase, L"0x%08X", kModule.Base);

			MSCM_CheckDriverServices(kmi->szFullDllPathOrginal, strPath, &kmi->serviceInfo);
			wcscpy_s(kmi->szServiceName, strPath);

			callback(kmi, kModule.BaseDllName, kmi->szFullDllPath, kModule.FullDllPath, strEntryPoint,
				strSizeOfImage, strDriverObject, strBase, strPath, kModule.Order);
			drvCount++;
		}
		else {
			LogErr(L"M_SU_EnumKernelModuls DeviceIoControl err: %d", GetLastError());
		}
		if (BoolsStart)BoolsStart = FALSE;
	}

	if (showall) {
		for (DWORD i = 0; i < dwNumberOfDriverService; i++)
		{
			if (!pDrvscsNames[i].DriverServiceFounded) {
				notLoadDrvCount++;

				PKernelModulSmallInfo kmi = (PKernelModulSmallInfo)malloc(sizeof(KernelModulSmallInfo));
				memset(kmi, 0, sizeof(KernelModulSmallInfo));

				kmi->DriverObject = NULL;
				kmi->serviceInfo = &pDrvscsNames[i];

				WCHAR strName[MAX_PATH];
				if (wcslen(pDrvscsNames[i].ServiceImagePath) > 0) {
					MNtPathToFilePath(pDrvscsNames[i].ServiceImagePath, kmi->szFullDllPath, MAX_PATH);
					std::wstring*namestr = Path::GetFileName(kmi->szFullDllPath);
					wcscpy_s(strName, namestr->c_str());
					delete namestr;
				}

				callback(kmi, strName, kmi->szFullDllPath, pDrvscsNames[i].ServiceImagePath, 0,
					0, 0, 0, pDrvscsNames[i].lpServiceName, 10000);
			}
		}
	}

	callback((PKernelModulSmallInfo)(ULONG_PTR)drvCount - 1, (LPWSTR)(ULONG_PTR)notLoadDrvCount, 0, 0, 0, 0, 0, 0, 0, 9999);
	return FALSE;
}
M_CAPI(void) M_SU_EnumKernelModuls_ShowMenu(KernelModulSmallInfo*kmi, BOOL showall) {
	if (kmi) {
		selectedKmi = kmi;
		HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUDRIVER));
		if (hroot) {
			HMENU hpop = GetSubMenu(hroot, 0);
			POINT pt;
			GetCursorPos(&pt);

			if (MStrEqualW(kmi->szFullDllPath, L"")) {
				EnableMenuItem(hpop, ID_MENUDRIVER_COPYPATH, MF_DISABLED);
				EnableMenuItem(hpop, ID_MENUDRIVER_OPENPATH, MF_DISABLED);
			}

			if (kmi->DriverObject == NULL)
				EnableMenuItem(hpop, ID_MENUDRIVER_UNLOAD, MF_DISABLED);

			if (MStrEqualW(kmi->szServiceName, L"")) {
				EnableMenuItem(hpop, ID_MENUDRIVER_COPYREG, MF_DISABLED);
				EnableMenuItem(hpop, ID_MENUDRIVER_START_BOOT, MF_DISABLED);
				EnableMenuItem(hpop, ID_MENUDRIVER_START_SYSTEM, MF_DISABLED);
				EnableMenuItem(hpop, ID_MENUDRIVER_START_AUTO, MF_DISABLED);
				EnableMenuItem(hpop, ID_MENUDRIVER_START_DEMAND, MF_DISABLED);
				EnableMenuItem(hpop, ID_MENUDRIVER_START_DISABLE, MF_DISABLED);
			}
			else if (kmi->serviceInfo != NULL) {
				switch (kmi->serviceInfo->ServiceStartType)
				{
				case SERVICE_DEMAND_START: {
					CheckMenuItem(hpop, ID_MENUDRIVER_START_DEMAND, MF_CHECKED );
					EnableMenuItem(hpop, ID_MENUDRIVER_START_DEMAND, MF_DISABLED);
					break;
				}
				case SERVICE_DISABLED: {
					CheckMenuItem(hpop, ID_MENUDRIVER_START_DISABLE, MF_CHECKED);
					EnableMenuItem(hpop, ID_MENUDRIVER_START_DISABLE, MF_DISABLED);
					break;				
				}
				case SERVICE_AUTO_START: {
					CheckMenuItem(hpop, ID_MENUDRIVER_START_AUTO, MF_CHECKED);
					EnableMenuItem(hpop, ID_MENUDRIVER_START_AUTO, MF_DISABLED);
					break;
				}
				case SERVICE_SYSTEM_START: {
					CheckMenuItem(hpop, ID_MENUDRIVER_START_SYSTEM, MF_CHECKED);
					EnableMenuItem(hpop, ID_MENUDRIVER_START_SYSTEM, MF_DISABLED);
					break;
				}				
				case SERVICE_BOOT_START: {
					CheckMenuItem(hpop, ID_MENUDRIVER_START_BOOT, MF_CHECKED);
					EnableMenuItem(hpop, ID_MENUDRIVER_START_BOOT, MF_DISABLED);
					break;
				}
				}

			}



			CheckMenuItem(hpop, ID_MENUDRIVER_SHOWALLDRIVER, showall ? MF_CHECKED : MF_UNCHECKED);		

			TrackPopupMenu(hpop,
				TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
				pt.x, pt.y, 0, hWndMain, NULL);
			DestroyMenu(hroot);
		}
	}
}

LRESULT M_SU_EnumKernelModuls_HandleWmCommand(WPARAM wParam)
{
	switch (wParam)
	{
	case ID_MENUDRIVER_REFESH: {
		MAppMainCall(24, 0, 0);
		return 0;
	}
	case ID_MENUDRIVER_DELETE: {
		if (selectedKmi && wcslen(selectedKmi->szFullDllPath) > 0)
		{

		}
		break;
	}
	case ID_MENUDRIVER_DELETEREGANDFILE: {
		if (selectedKmi && wcslen(selectedKmi->szFullDllPath) > 0)
		{

		}
		break;
	}
	case ID_MENUDRIVER_CHECKVERY: {
		if (selectedKmi && wcslen(selectedKmi->szFullDllPath) > 0)
		{
			if (MFM_FileExist(selectedKmi->szFullDllPath))
			{
				if (MGetExeFileTrust(selectedKmi->szFullDllPath))
					MShowMessageDialog(hWndMain, str_item_filetrusted, str_item_tip, L"", 0, 0);
				else
					MShowMessageDialog(hWndMain, selectedKmi->szFullDllPath, str_item_tip, str_item_filenottrust, 0, 0);
			}
			else MShowMessageDialog(hWndMain, str_item_filenotexist, str_item_tip, L"", 0, 0);
		}
		break;
	}
	case ID_MENUDRIVER_CHECK_ALLVERY: {

		break;
	}
	case ID_MENUDRIVER_SHOWALLDRIVER: {
		MAppMainCall(25, 0, 0);
		break;
	}
	case ID_MENUDRIVER_COPYPATH: {
		if (selectedKmi && wcslen(selectedKmi->szFullDllPath) > 0 && !MStrEqualW(selectedKmi->szFullDllPath, L""))
			MCopyToClipboard(selectedKmi->szFullDllPath, wcslen(selectedKmi->szFullDllPath));
		break;
	}
	case ID_MENUDRIVER_COPYREG: {
		if (selectedKmi && !MStrEqualW(selectedKmi->szServiceName, L"")) {
			WCHAR regpath[MAX_PATH];
			if (M_SU_GetServiceReg(selectedKmi->szServiceName, regpath, MAX_PATH))
				MCopyToClipboard(regpath, wcslen(regpath));
		}
		break;
	}
	case ID_MENUDRIVER_OPENPATH: {
		if (selectedKmi && selectedKmi->szFullDllPath)
			MFM_ShowInExplorer(selectedKmi->szFullDllPath);
		break;
	}
	case ID_MENUDRIVER_SHOWPROP: {
	    if (selectedKmi && selectedKmi->szFullDllPath) MShowFileProp(selectedKmi->szFullDllPath);
		break;
	}
	case ID_MENUDRIVER_START_BOOT: {
		if (selectedKmi && !MStrEqualW(selectedKmi->szServiceName, L""))
			MSCM_ChangeScStartType(selectedKmi->szServiceName, SERVICE_BOOT_START, L"");
		break;
	}
	case ID_MENUDRIVER_START_SYSTEM: {
		if (selectedKmi && !MStrEqualW(selectedKmi->szServiceName, L""))
			MSCM_ChangeScStartType(selectedKmi->szServiceName, SERVICE_SYSTEM_START, L"");
		break;
	}
	case ID_MENUDRIVER_START_AUTO: {
		if (selectedKmi && !MStrEqualW(selectedKmi->szServiceName, L""))
			MSCM_ChangeScStartType(selectedKmi->szServiceName, SERVICE_AUTO_START, L"");
		break;
	}
	case ID_MENUDRIVER_START_DEMAND: {
		if (selectedKmi && !MStrEqualW(selectedKmi->szServiceName, L""))
			MSCM_ChangeScStartType(selectedKmi->szServiceName, SERVICE_DEMAND_START, L"");
		break;
	}
	case ID_MENUDRIVER_START_DISABLE: {
		if (selectedKmi && !MStrEqualW(selectedKmi->szServiceName, L""))
			MSCM_ChangeScStartType(selectedKmi->szServiceName, SERVICE_DISABLED, L"");
		break;
	}
	case ID_MENUDRIVER_UNLOAD: {
		if (selectedKmi && selectedKmi->DriverObject != 0)
		{

		}
		break;
	}
	default:
		break;
	}
	return 0;
}

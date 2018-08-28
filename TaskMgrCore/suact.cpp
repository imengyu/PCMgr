#include "stdafx.h"
#include "suact.h"
#include "prochlp.h"
#include "thdhlp.h"
#include "ntdef.h"
#include "kernelhlp.h"
#include "ioctls.h"
#include "lghlp.h"
#include "loghlp.h"
#include "resource.h"
#include "mapphlp.h"
#include "fmhlp.h"
#include "kda.h"
#include "syshlp.h"
#include "reghlp.h"
#include "PathHelper.h"
#include "StringHlp.h"
#include "settinghlp.h"

extern NtTerminateThreadFun NtTerminateThread;
extern NtTerminateProcessFun NtTerminateProcess;

extern BOOL isKernelDriverLoaded;
extern HANDLE hKernelDevice;
extern HWND hWndMain;
extern HINSTANCE hInstRs;
extern bool executeByLoader;

//内核进程控制系列函数

M_CAPI(BOOL) M_SU_IsK(LPWSTR errTip, NTSTATUS* pStatus) {
	//检查内核是否加载
	if (MCanUseKernel())
		return TRUE;
	else {
		LogErr(L"%s failed because kernel driver not load", errTip);
		if (pStatus)*pStatus = STATUS_NOT_IMPLEMENTED;
		return FALSE;
	}
}
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
	if (status != STATUS_SUCCESS && M_SU_IsK(L"M_SU_OpenProcess", pStatus))
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
	if (status != STATUS_SUCCESS && M_SU_IsK(L"M_SU_OpenThread", pStatus))
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
M_CAPI(BOOL) M_SU_TerminateProcessPID(DWORD pid, UINT exitCode, NTSTATUS* pStatus, BOOL useApc)
{
	BOOL rs = FALSE;
	NTSTATUS status = STATUS_UNSUCCESSFUL;
	Log(L"M_SU_TerminateProcessPID process id : %d", pid);
	if (M_SU_IsK(L"M_SU_TerminateProcessPID", pStatus)) {
		ULONG_PTR pid2 = pid;
		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, useApc ? CTL_FORCE_TREMINATE_PROCESS_APC : CTL_FORCE_TREMINATE_PROCESS, &pid2, sizeof(pid2), &status, sizeof(status), &ReturnLength, NULL))
			rs = TRUE;
		else {
			LogErr(L"M_SU_TerminateProcessPID DeviceIoControl err : %d", GetLastError());
			if(status == STATUS_SUCCESS)
				rs = TRUE;
		}
	}
	if (pStatus)*pStatus = status;
	return rs;
}
M_CAPI(BOOL) M_SU_TerminateThreadTID(DWORD tid, UINT exitCode, NTSTATUS* pStatus, BOOL useApc)
{
	Log(L"M_SU_TerminateThreadTID thread id : %d", tid);
	BOOL rs = FALSE;
	ULONG_PTR tid2 = tid;
	NTSTATUS status = STATUS_UNSUCCESSFUL;
	if (M_SU_IsK(L"M_SU_TerminateProcessPID", pStatus)) {
		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, useApc ? CTL_FORCE_TREMINATE_THREAD_APC : CTL_FORCE_TREMINATE_THREAD, &tid2, sizeof(tid2), &status, sizeof(status), &ReturnLength, NULL))
			rs = TRUE;
		else {
			LogErr(L"M_SU_TerminateThreadTID DeviceIoControl err : %d", GetLastError());
			if (status == STATUS_SUCCESS)
				rs = TRUE;
		}
		if (pStatus)*pStatus = status;
	}
	return rs;
}
M_CAPI(BOOL) M_SU_CloseHandleWithProcess(DWORD pid, LPVOID handleValue)
{
	BOOL rs = FALSE;
	DWORD ReturnLength = 0;
	NTSTATUS status = 0;
	FCLOSE_HANDLE_DATA pidInBuffer;
	pidInBuffer.ProcessId = pid;
	pidInBuffer.HandleValue = handleValue;
	if (DeviceIoControl(hKernelDevice, CTL_FORCE_CLOSE_HANDLE, &pidInBuffer, sizeof(pidInBuffer), &status, sizeof(status), &ReturnLength, NULL))
	{
		if (status == STATUS_SUCCESS)
			rs = TRUE;
		else LogErr(L"M_SU_CloseHandleWithProcess err NTSTATUS : 0x%08X", status);
	}
	else LogErr(L"M_SU_CloseHandleWithProcess DeviceIoControl err : %d", GetLastError());
	return rs;
}

M_CAPI(BOOL) M_SU_SuspendProcess(DWORD pid, NTSTATUS* pStatus)
{
	BOOL rs = FALSE;
	NTSTATUS status = 0;
	if (M_SU_IsK(L"M_SU_SuspendProcess", pStatus)) {
		DWORD ReturnLength = 0;
		ULONG_PTR pidInBuffer = pid;
		if (DeviceIoControl(hKernelDevice, CTL_SUSPEND_PROCESS, &pidInBuffer, sizeof(pidInBuffer), &status, sizeof(status), &ReturnLength, NULL))
			rs = TRUE;
		else LogErr(L"M_SU_SuspendProcess DeviceIoControl err : %d", GetLastError());
		if (pStatus)*pStatus = status;
	}
	return rs;
}
M_CAPI(BOOL) M_SU_ResumeProcess(DWORD pid, NTSTATUS* pStatus)
{
	BOOL rs = FALSE;
	if (M_SU_IsK(L"M_SU_ResumeProcess", pStatus)) {
		NTSTATUS status = 0;
		DWORD ReturnLength = 0;
		ULONG_PTR pidInBuffer = pid;
		if (DeviceIoControl(hKernelDevice, CTL_RESUME_PROCESS, &pidInBuffer, sizeof(pidInBuffer), &status, sizeof(status), &ReturnLength, NULL))
			rs = TRUE;
		else LogErr(L"M_SU_ResumeProcess DeviceIoControl err : %d", GetLastError());
		if (pStatus)*pStatus = status;
	}
	return rs;
}
M_CAPI(BOOL) M_SU_KDA(DACALLBACK callback, ULONG_PTR startaddress, ULONG_PTR size)
{
	if (!M_SU_IsK(L"M_SU_KDA", NULL)) return  FALSE;
	if (!startaddress)LogErr(L"M_SU_KDA startaddress == 0!");
	if (size <= 0 || size > 0xff)LogErr(L"M_SU_KDA size invalid ! (%u)", size);
	if (callback && startaddress && size > 0 && size <= 0xff)
	{
		KDAAGRS agrs;
		agrs.StartAddress = startaddress;
		agrs.Size = size;

		PUCHAR outBuffer = (PUCHAR)MAlloc(size);
		memset(outBuffer, 0, size);

		DWORD ReturnLength = 0;
		if (!DeviceIoControl(hKernelDevice, CTL_KDA_DEC, &agrs, sizeof(agrs), outBuffer, static_cast<DWORD>(size), &ReturnLength, NULL))
		{
			LogErr(L"M_SU_KDA DeviceIoControl err : %d", GetLastError());
			return FALSE;
		}
#ifdef _AMD64_
		BOOL rs = M_KDA_Dec(outBuffer, startaddress, callback, size, FALSE);
#else
		BOOL rs = M_KDA_Dec(outBuffer, startaddress, callback, size, TRUE);
#endif
		MFree(outBuffer);
		return rs;
	}
	return FALSE;
}

//Sys & protect

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
	return FALSE;
}

//KrlMon settings

M_CAPI(BOOL) M_SU_SetKrlMonSet_CreateProcess(BOOL allow)
{
	DWORD ReturnLength = 0;
	UCHAR inputBuffer = allow;
	if (DeviceIoControl(hKernelDevice, CTL_KERNEL_INIT, &inputBuffer, sizeof(inputBuffer), NULL, 0, &ReturnLength, NULL))
		return TRUE;
	return FALSE;
}
M_CAPI(BOOL) M_SU_SetKrlMonSet_CreateThread(BOOL allow)
{
	DWORD ReturnLength = 0;
	UCHAR inputBuffer = allow;
	if (DeviceIoControl(hKernelDevice, CTL_KERNEL_INIT, &inputBuffer, sizeof(inputBuffer), NULL, 0, &ReturnLength, NULL))
		return TRUE;
	return FALSE;
}
M_CAPI(BOOL) M_SU_SetKrlMonSet_LoadImage(BOOL allow)
{
	DWORD ReturnLength = 0;
	UCHAR inputBuffer = allow;
	if (DeviceIoControl(hKernelDevice, CTL_KERNEL_INIT, &inputBuffer, sizeof(inputBuffer), NULL, 0, &ReturnLength, NULL))
		return TRUE;
	return FALSE;
}

ULONG lastSetSysver = 0;
PKernelModulSmallInfo selectedKmi = 0;
extern LPSERVICE_STORAGE pDrvscsNames;
extern DWORD dwNumberOfDriverService;
extern DWORD currentWindowsBulidVer;

//初始化函数
M_CAPI(VOID) M_SU_SetSysver(ULONG ver)
{
	//lastSetSysver = ver;
}
//初始化函数
M_CAPI(BOOL) M_SU_Init(BOOL requestNtosValue, PKNTOSVALUE outValue) {
	Log(L"M_SU_Init DeviceIoControl");
	DWORD ReturnLength = 0;
	ULONG_PTR currrntPID = GetCurrentProcessId();
	if (!DeviceIoControl(hKernelDevice, CTL_SET_CURRENT_PCMGR_PROCESS, &currrntPID, sizeof(currrntPID), NULL, 0, &ReturnLength, NULL))
		LogErr(L"M_SU_Init 1 DeviceIoControl err : %d", GetLastError());	
	if (lastSetSysver == 0 && currentWindowsBulidVer != 0)
	{
		if (currentWindowsBulidVer >= 10000)
			lastSetSysver = 10;
		else if (currentWindowsBulidVer >= 8100)
			lastSetSysver = 81;
		else if (currentWindowsBulidVer >= 8000)
			lastSetSysver = 8;
		else if (currentWindowsBulidVer >= 7000)
			lastSetSysver = 7;
	}

	KINITAGRS agrs = { 0 };	
	agrs.WinVer.VerSimple = lastSetSysver;
	agrs.WinVer.WinBulidVerl = currentWindowsBulidVer;
	agrs.NeedNtosVaule = requestNtosValue;

	KNTOSVALUE value = { 0 };
	if (DeviceIoControl(hKernelDevice, CTL_KERNEL_INIT, &agrs, sizeof(agrs), &value, sizeof(value), &ReturnLength, NULL)) {
		if (requestNtosValue && outValue)
			memcpy_s(outValue, sizeof(KNTOSVALUE), &value, sizeof(KNTOSVALUE));
		return TRUE;
	}
	LogErr(L"M_SU_Init 2 DeviceIoControl err : %d", GetLastError());
	return FALSE;
}

M_CAPI(BOOL) M_SU_GetEPROCESS(DWORD pid, ULONG_PTR* lpEprocess, ULONG_PTR* lpPeb, ULONG_PTR* lpJob, LPWSTR imagename, LPWSTR path)
{
	BOOL rs = FALSE;
	if (isKernelDriverLoaded)
	{
		PKPROCINFO output = (PKPROCINFO)MAlloc(sizeof(KPROCINFO));
		memset(output, 0, sizeof(KPROCINFO));
		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, CTL_GET_EPROCESS, &pid, sizeof(DWORD), output, sizeof(KPROCINFO), &ReturnLength, NULL))
		{
			if (lpEprocess)(*lpEprocess) = (ULONG_PTR)output->EProcess;
			if (lpPeb)(*lpPeb) = (ULONG_PTR)output->PebAddress;
			if (lpJob)(*lpJob) = (ULONG_PTR)output->JobAddress;
			if (imagename) {
				LPWSTR pathw = MConvertLPCSTRToLPWSTR((CHAR*)output->ImageFileName);
				wcscpy_s(imagename, MAX_PATH, pathw);
				delete pathw;
			}
			if (path) wcscpy_s(path, MAX_PATH, output->FullPath);
			rs = TRUE;
		} else LogErr(L"M_SU_GetEPROCESS DeviceIoControl err : %d in pid : %d", GetLastError(), pid);
		MFree(output);
	}
	return rs;
}
M_CAPI(BOOL) M_SU_GetETHREAD(DWORD tid, ULONG_PTR* lpEthread, ULONG_PTR * lpTeb)
{
	BOOL rs = FALSE;
	if (isKernelDriverLoaded)
	{
		PKTHREADINFO output = (PKTHREADINFO)MAlloc(sizeof(KTHREADINFO));
		memset(output, 0, sizeof(KTHREADINFO));
		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, CTL_GET_ETHREAD, &tid, sizeof(DWORD), output, sizeof(KTHREADINFO), &ReturnLength, NULL))
		{
			if (lpEthread)(*lpEthread) = (ULONG_PTR)output->EThread;
			if (lpTeb)(*lpTeb) = (ULONG_PTR)output->TebAddress;
			
			rs = TRUE;
		}
		else LogErr(L"M_SU_GetETHREAD DeviceIoControl err : %d in tid : %d", GetLastError(), tid);
		MFree(output);
	}
	return rs;
}
M_CAPI(BOOL) M_SU_GetProcessCommandLine(DWORD tid, LPWSTR outCmdLine)
{
	BOOL rs = FALSE;
	if (isKernelDriverLoaded)
	{
		WCHAR output[1024];
		memset(&output, 0, sizeof(output));
		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, CTL_GET_PROCESS_COMMANDLINE, &tid, sizeof(DWORD), output, sizeof(KTHREADINFO), &ReturnLength, NULL))
		{
			if (outCmdLine)wcscpy_s(outCmdLine, 1024, output);
			rs = TRUE;
		}
		else LogErr(L"M_SU_GetProcessCommandLine DeviceIoControl err : %d in tid : %d", GetLastError(), tid);
		MFree(output);
	}
	return rs;
}

//EnumKernelModuls

M_CAPI(void) M_SU_EnumKernelModulsItemDestroy(KernelModulSmallInfo*km)
{
	if (km) MFree(km);
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

		PKernelModulSmallInfo kmi = (PKernelModulSmallInfo)MAlloc(sizeof(KernelModulSmallInfo));
		memset(kmi, 0, sizeof(KernelModulSmallInfo));

		WCHAR strEntryPoint[32];
		WCHAR strSizeOfImage[32];
		WCHAR strDriverObject[32];
		WCHAR strBase[32];
		WCHAR strPath[MAX_PATH];
		memset(strPath, 0, sizeof(strPath));

		DWORD ReturnLength = 0;
		if (DeviceIoControl(hKernelDevice, CTL_GET_KERNEL_MODULS, &BoolsStart, sizeof(BoolsStart), &kModule, sizeof(kModule), &ReturnLength, NULL))
		{
			wcscpy_s(kmi->szFullDllPathOrginal, kModule.FullDllPath);
			//wcscpy_s(kmi->szFullDllPath, kModule.FullDllPath);
			MNtPathToFilePath(kmi->szFullDllPathOrginal, kmi->szFullDllPath, MAX_PATH);

#ifdef _AMD64_
			if (kModule.EntryPoint == 0) wcscpy_s(strDriverObject, L"-");
			else swprintf_s(strEntryPoint, L"0x%I64X", kModule.EntryPoint);
			swprintf_s(strSizeOfImage, L"0x%08X", kModule.SizeOfImage);
			if (kModule.DriverObject == 0) wcscpy_s(strDriverObject, L"-");
			else swprintf_s(strDriverObject, L"0x%I64X", kModule.DriverObject);
			if (kModule.Base == 0) wcscpy_s(strDriverObject, L"-");
			else swprintf_s(strBase, L"0x%I64X", kModule.Base);
#else
			if (kModule.EntryPoint == 0) wcscpy_s(strDriverObject, L"-");
			else swprintf_s(strEntryPoint, L"0x%08X", kModule.EntryPoint);
			swprintf_s(strSizeOfImage, L"0x%08X", kModule.SizeOfImage);
			if (kModule.DriverObject == 0) wcscpy_s(strDriverObject, L"-");
			else swprintf_s(strDriverObject, L"0x%08X", kModule.DriverObject);
			if (kModule.Base == 0) wcscpy_s(strDriverObject, L"-");
			else swprintf_s(strBase, L"0x%08X", kModule.Base);
#endif

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

				PKernelModulSmallInfo kmi = (PKernelModulSmallInfo)MAlloc(sizeof(KernelModulSmallInfo));
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
M_CAPI(void) M_SU_EnumKernelModuls_ShowMenu(KernelModulSmallInfo*kmi, BOOL showall, int x, int y) {
	if (kmi) {
		selectedKmi = kmi;
		HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUDRIVER));
		if (hroot) {
			HMENU hpop = GetSubMenu(hroot, 0);
			POINT pt;
			if (x == 0 && y == 0)
				GetCursorPos(&pt);
			else {
				pt.x = x;
				pt.y = y;
			}

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
		MAppMainCall(M_CALLBACK_KERNEL_INIT_LIST, 0, 0);
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
					MAppMainCall(M_CALLBACK_SHOW_TRUSTED_DLG, selectedKmi->szFullDllPath, 0);
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
		MAppMainCall(M_CALLBACK_KERNEL_SWITCH_SHOWALLDRV, 0, 0);
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
			if (MREG_GetServiceReg(selectedKmi->szServiceName, regpath, MAX_PATH))
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

//MyDbgView

M_CAPI(BOOL) M_SU_SetDbgViewEvent(HANDLE hEvent)
{
	DBGVIEW_SENDER inbuf;
	inbuf.ProcessId = GetCurrentProcessId();
	inbuf.EventHandle = hEvent;

	BOOLEAN result = FALSE;
	DWORD ReturnLength = 0;
	if (!DeviceIoControl(hKernelDevice, CTL_SET_DBGVIEW_EVENT, &inbuf, sizeof(inbuf), &result, sizeof(result), &ReturnLength, NULL)) {
		LogWarn(L"M_SU_SetDbgViewEvent Failed");
		return FALSE;
	}
	return result;
}
M_CAPI(BOOL) M_SU_ReSetDbgViewEvent()
{
	DWORD ReturnLength = 0;
	if (!DeviceIoControl(hKernelDevice, CTL_RESET_DBGVIEW_EVENT, NULL, 0, NULL, 0, &ReturnLength, NULL)) {
		LogWarn(L"M_SU_ReSetDbgViewEvent Failed");
		return FALSE;
	}
	return TRUE;
}
M_CAPI(BOOL) M_SU_GetDbgViewLastBuffer(LPWSTR outbuffer, size_t bufsize, BOOL*hasMoreData) 
{
	DBGPRT_DATA_TRA drvoutBuffer;
	memset(&drvoutBuffer, 0, sizeof(drvoutBuffer));
	DWORD ReturnLength = 0;
	if (DeviceIoControl(hKernelDevice, CTL_GET_DBGVIEW_BUFFER, NULL, 0, &drvoutBuffer, sizeof(drvoutBuffer), &ReturnLength, NULL)) {
		if (drvoutBuffer.HasData) {
			LPWSTR szw = MConvertLPCSTRToLPWSTR(drvoutBuffer.Data);
			wcscpy_s(outbuffer, bufsize, szw);
			delete szw;
			if (hasMoreData)
				*hasMoreData = drvoutBuffer.HasMoreData;
		}
		return TRUE;
	}
	return FALSE;
}
M_CAPI(BOOL) M_SU_PrintInternalFuns() {
	DWORD ReturnLength = 0;
	return DeviceIoControl(hKernelDevice, CTL_PRINT_INTERNAL_FUNS, NULL, 0, NULL, 0, &ReturnLength, NULL);
}

//HotKeys & Timers

M_CAPI(BOOL) M_SU_GetProcessHotKeys(DWORD pid, EnumProcessHotKeyCallBack callBack)
{
	BOOL rs = FALSE;
	NTSTATUS status = 0;
	if (!callBack) return FALSE;
	if (M_SU_IsK(L"M_SU_GetProcessHotKeys", &status)) {
		DWORD ReturnLength = 0;
		ULONG_PTR pidInBuffer = pid;
		ULONG outSize = 0;
		if (!DeviceIoControl(hKernelDevice, CTL_GET_PROCESS_HOTKEYS, &pidInBuffer, sizeof(pidInBuffer), &outSize, sizeof(outSize), &ReturnLength, NULL))
		{
			LogErr(L"M_SU_GetProcessHotKeys DeviceIoControl err : %d", GetLastError());
			return FALSE;
		}
		if (outSize < 0 || outSize > 128)return FALSE;
		DWORD bufferSize = outSize * sizeof(HOT_KEY_DATA);
		PHOT_KEY_DATA outBuffer = (PHOT_KEY_DATA)MAlloc(bufferSize);
		if (!DeviceIoControl(hKernelDevice, CTL_GET_PROCESS_HOTKEYS_BUFFER, NULL, 0, &outBuffer, bufferSize, &ReturnLength, NULL))
		{
			LogErr(L"M_SU_GetProcessHotKeys DeviceIoControl err : %d", GetLastError());
			return FALSE;
		}

		WCHAR objStr[32] = { 0 };
		WCHAR keyStr[64] = { 0 };
		WCHAR procName[128] = { 0 };

		for (UINT i = 0; i < bufferSize; i++)
		{
			PHOT_KEY_DATA data = outBuffer + bufferSize * sizeof(HOT_KEY_DATA);
#ifdef _AMD64_
			swprintf_s(objStr, L"0x%I64X", data->ObjectPtr);
#else
			swprintf_s(objStr, L"0x%08X", data->ObjectPtr);
#endif
			MHotKeyToStr(data->fsModifiers, data->vk, keyStr, 64);

			LPWSTR procNamew = A2W(data->ImageFileName);
			callBack(data, objStr, data->id, keyStr, data->ProcessId, data->ThreadId, procNamew);
			MFree(procNamew);
		}
		MFree(outBuffer);
		rs = TRUE;
	}
	return rs;
}
M_CAPI(BOOL) M_SU_GetProcessTimers(DWORD pid, EnumProcessTimerCallBack callBack)
{
	BOOL rs = FALSE;
	NTSTATUS status = 0;
	if (!callBack) return FALSE;
	if (M_SU_IsK(L"M_SU_GetProcessHotKeys", &status)) {
		DWORD ReturnLength = 0;
		ULONG_PTR pidInBuffer = pid;
		ULONG outSize = 0;
		if (!DeviceIoControl(hKernelDevice, CTL_GET_PROCESS_TIMERS, &pidInBuffer, sizeof(pidInBuffer), &outSize, sizeof(outSize), &ReturnLength, NULL))
		{
			LogErr(L"M_SU_GetProcessHotKeys DeviceIoControl err : %d", GetLastError());
			return FALSE;
		}
		if (outSize < 0 || outSize > 128)return FALSE;
		DWORD bufferSize = outSize * sizeof(TIMER_DATA);
		PTIMER_DATA outBuffer = (PTIMER_DATA)MAlloc(bufferSize);
		if (!DeviceIoControl(hKernelDevice, CTL_GET_PROCESS_TIMERS_BUFFER, NULL, 0, &outBuffer, bufferSize, &ReturnLength, NULL))
		{
			LogErr(L"M_SU_GetProcessHotKeys DeviceIoControl err : %d", GetLastError());
			return FALSE;
		}

		WCHAR objStr[32] = { 0 };
		WCHAR funStr[32] = { 0 };
		WCHAR moduleStr[MAX_PATH] = { 0 };
		WCHAR hwndStr[32] = { 0 };

		for (UINT i = 0; i < bufferSize; i++)
		{
			PTIMER_DATA data = outBuffer + bufferSize * sizeof(TIMER_DATA);
			DWORD thisWindowPid = 0;
			DWORD thisWindowThreadId = GetWindowThreadProcessId((HWND)data->spwnd, &thisWindowPid);
			if (pid == 0 || thisWindowPid == pid) 
			{
#ifdef _AMD64_
				swprintf_s(objStr, L"0x%I64X", data->ObjectPtr);
				swprintf_s(funStr, L"0x%I64X", data->pfn);
				swprintf_s(hwndStr, L"0x%I64X", (ULONG_PTR)data->spwnd);
#else
				swprintf_s(objStr, L"0x%08X", data->ObjectPtr);
				swprintf_s(funStr, L"0x%08X", data->pfn);
				swprintf_s(hwndStr, L"0x%08X", (ULONG_PTR)data->spwnd);
#endif

				callBack(data, objStr, funStr, moduleStr, hwndStr,
					(HWND)data->spwnd, thisWindowThreadId, data->nID, data->cmsRate, thisWindowPid);
			}
		}

		MFree(outBuffer);
		rs = TRUE;
	}
	return rs;
}





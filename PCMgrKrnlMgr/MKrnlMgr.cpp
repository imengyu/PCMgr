#include "stdafx.h"
#include "MKrnlMgr.h"
#include "MDriverLoader.h"

#include "..\TaskMgrCore\loghlp.h"
#include "..\TaskMgrCore\mapphlp.h"
#include "..\TaskMgrCore\syshlp.h"
#include "..\TaskMgrCore\fmhlp.h"
#include "..\TaskMgrCore\kernelhlp.h"

HANDLE MKrnlMgr::DriverHandle = NULL;
bool KernelLoaderd = false;
bool KernelNeed64 = false;

MKrnlMgr::MKrnlMgr()
{
}
MKrnlMgr::~MKrnlMgr()
{
}

bool MKrnlMgr::UnInitKernel()
{
	Log(L"MUninitKernel...");
	if (KernelLoaderd)
	{
		if (MMyDbgViewStarted())
			MUnInitMyDbgView();
		if (DriverHandle != NULL)
			CloseHandle(DriverHandle);

		if (MDriverLoader::UnLoadDriver(L"PCMgrKernel")) {
			KernelLoaderd = FALSE;
			MInitKernelSwitchMenuState(KernelLoaderd);

			Log(L"Kernel unloaded");
			return TRUE;
		}
		else Log(L"Kernel unload failed");
		return !KernelLoaderd;
	}
	else {
		LogWarn(L"Kernel not load , try force delete service");

		if (MDriverLoader::UnLoadDriver(L"PCMgrKernel")) {
			KernelLoaderd = FALSE;
			MInitKernelSwitchMenuState(KernelLoaderd);

			Log(L"Kernel unloaded");
			return TRUE;
		}
		else LogErr(L"Kernel unload failed");
	}
	return FALSE;
}
bool MKrnlMgr::InitKernel(LPCWSTR currentPath)
{
	WCHAR currentDir[MAX_PATH];
	wcscpy_s(currentDir, currentPath);

	Log(L"MInitKernel (%s)...", currentDir);

	MAppSetStartingProgessText((LPWSTR)L"Loading kernel driver...");
	if (!KernelLoaderd)
	{
		wchar_t path[MAX_PATH];
		if (MIs64BitOS()) {
#ifdef _AMD64_
			wsprintf(path, L"%s\\PCMgrKernel64.sys", currentDir);
#else
			::KernelNeed64 = TRUE;
			LogErr(L"You need to use 64 bit version PCMgr application to load driver.");
			return FALSE;
#endif
		}
		else wsprintf(path, L"%s\\PCMgrKernel32.sys", currentDir);

		if (MFM_FileExist(path))
		{
			Log(L"LoadKernelDrive (%s)...", path);

			if (MDriverLoader::LoadDriver(L"PCMgrKernel", path)) {
				Log(L"Kernel Driver Loaded.");
				KernelLoaderd = TRUE;
				return KernelInitHandle();
			}
			else
			{
				if (KernelInitHandle()) {
					KernelLoaderd = TRUE;
					MInitKernelSwitchMenuState(KernelLoaderd);
				}
				else {
					KernelLoaderd = FALSE;
					LogErr(L"Load Kernel driver error");
				}
			}
		}
		else if (KernelInitHandle()) {
			Log(L"Kernel driver file missing.");
			KernelLoaderd = TRUE;
			MInitKernelSwitchMenuState(KernelLoaderd);
		}
		else {

			LogErr(L"Load Kernel driver error because kernel driver file missing.");
			LogInfo(L"Try find kernel driver file : %s.", path);

			KernelLoaderd = FALSE;
			MInitKernelSwitchMenuState(KernelLoaderd);
		}
	}
	else Log(L"Kernel alreday loaded");
	return KernelLoaderd;
}
bool MKrnlMgr::KernelInited()
{
	return KernelLoaderd;
}
bool MKrnlMgr::KernelNeed64()
{
	return ::KernelNeed64;
}
bool MKrnlMgr::KernelCanUse()
{
	return KernelLoaderd && DriverHandle != NULL;
}
bool MKrnlMgr::KernelInitHandle()
{
	DriverHandle = CreateFile(L"\\\\.\\PCMGRK",
		GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_READ | FILE_SHARE_WRITE,
		NULL,
		OPEN_EXISTING,
		FILE_ATTRIBUTE_NORMAL,
		NULL);
	if (DriverHandle == INVALID_HANDLE_VALUE)
	{
		LogErr(L"Get Kernel driver handle (CreateFile) failed : %d . ", GetLastError());
		return false;
	}

	return MInitKernelDriverHandleEndStep(DriverHandle);
}



#include "stdafx.h"
#include "prochlp.h"
#include "ntdef.h"
#include "perfhlp.h"
#include "syshlp.h"
#include "nthlp.h"
#include "lghlp.h"
#include "loghlp.h"
#include "mapphlp.h"
#include "resource.h"
#include "kernelhlp.h"
#include "suact.h"
#include "fmhlp.h"
#include "StringHlp.h"
#include <Psapi.h>
#include <process.h>
#include <tlhelp32.h>
#include <Shlobj.h>
#include <tchar.h>
#include <shellapi.h>
#include <wintrust.h>
#include <mscat.h>

extern HINSTANCE hInstRs;

LPWSTR thisCommandPath = NULL;
LPWSTR thisCommandName = NULL;
DWORD thisCommandPid = 0;
HICON HIconDef;
HWND hWndMain;
PSYSTEM_PROCESSES current_system_process = NULL;

BOOL killCmdSendBack = FALSE;
BOOL isKillingExplorer = FALSE;

extern bool use_apc;

//Api s

//shell32
_RunFileDlg RunFileDlg;
//ntdll
ZwSuspendThreadFun ZwSuspendThread;
ZwResumeThreadFun ZwResumeThread;
ZwTerminateThreadFun ZwTerminateThread;
ZwTerminateProcessFun ZwTerminateProcess;
ZwOpenThreadFun ZwOpenThread;
ZwQueryInformationThreadFun ZwQueryInformationThread;
ZwSuspendProcessFun ZwSuspendProcess;
ZwResumeProcessFun ZwResumeProcess;

ZwOpenProcessFun ZwOpenProcess;
NtQuerySystemInformationFun NtQuerySystemInformation;
NtUnmapViewOfSectionFun NtUnmapViewOfSection;
NtQueryInformationProcessFun NtQueryInformationProcess;
LdrGetProcedureAddressFun LdrGetProcedureAddress;
RtlInitAnsiStringFun RtlInitAnsiString;
RtlNtStatusToDosErrorFun RtlNtStatusToDosError;
RtlGetLastWin32ErrorFun RtlGetLastWin32Error;
NtQueryObjectFun NtQueryObject;

//K32 api
_IsWow64Process dIsWow64Process;
_IsImmersiveProcess dIsImmersiveProcess;
_GetPackageFullName dGetPackageFullName;
_GetPackageInfo dGetPackageInfo;
_ClosePackageInfo dClosePackageInfo;
_OpenPackageInfoByFullName dOpenPackageInfoByFullName;
_GetPackageId dGetPackageId;

//Enum apis
EXTERN_C BOOL MAppVProcessAllWindows();

M_API void MFroceKillProcessUser()
{
	if (thisCommandPid > 4)
	{
		if ((MShowMessageDialog(hWndMain, (LPWSTR)str_item_kill_ast_content.c_str(), DEFDIALOGGTITLE,
			(LPWSTR)(str_item_kill_ask_start + thisCommandName + str_item_kill_ask_end).c_str(), NULL, MB_YESNO) == IDYES))
		{
			NTSTATUS status = 0;	
			if (!M_SU_TerminateProcessPID(thisCommandPid, 0, &status, use_apc))
				ThrowErrorAndErrorCodeX(status, str_item_endprocfailed, (LPWSTR)str_item_kill_failed.c_str());
			else if (status == STATUS_ACCESS_DENIED)
				MShowErrorMessage((LPWSTR)str_item_access_denied.c_str(), (LPWSTR)str_item_kill_failed.c_str(), MB_ICONERROR, MB_OK);
			else if (status == 0xC0000008 || status == 0xC000000B)
				MShowErrorMessage((LPWSTR)str_item_invalidproc.c_str(), (LPWSTR)str_item_kill_failed.c_str(), MB_ICONWARNING, MB_OK);
			else ThrowErrorAndErrorCodeX(status, str_item_openprocfailed, (LPWSTR)str_item_kill_failed.c_str());
		}
	}
}
M_API void MKillProcessUser(BOOL ask)
{
	if (thisCommandPid > 4)
	{
		if (isKillingExplorer || !ask || (MShowMessageDialog(hWndMain, (LPWSTR)str_item_kill_ast_content.c_str(), DEFDIALOGGTITLE,
			(LPWSTR)(str_item_kill_ask_start + thisCommandName + str_item_kill_ask_end).c_str(), NULL, MB_YESNO) == IDYES))
		{
			HANDLE hProcess;
			NTSTATUS status = MOpenProcessNt(thisCommandPid, &hProcess);
			if (status == STATUS_SUCCESS)
			{
				status = MTerminateProcessNt(0, hProcess);
				if (status == STATUS_ACCESS_DENIED)
					MShowErrorMessage((LPWSTR)str_item_access_denied.c_str(), (LPWSTR)str_item_kill_failed.c_str(), MB_ICONERROR, MB_OK);
				else if (status != STATUS_SUCCESS)
					ThrowErrorAndErrorCodeX(status, str_item_endprocfailed, (LPWSTR)str_item_kill_failed.c_str());
			}
			else if (status == STATUS_ACCESS_DENIED)
				MShowErrorMessage((LPWSTR)str_item_access_denied.c_str(), (LPWSTR)str_item_kill_failed.c_str(), MB_ICONERROR, MB_OK);
			else if (status == 0xC0000008 || status == 0xC000000B)
				MShowErrorMessage((LPWSTR)str_item_invalidproc.c_str(), (LPWSTR)str_item_kill_failed.c_str(), MB_ICONWARNING, MB_OK);
			else ThrowErrorAndErrorCodeX(status, str_item_openprocfailed, (LPWSTR)str_item_kill_failed.c_str());
		}
	}
}
M_API BOOL MGetPrivileges2()
{
	HANDLE hToken;
	TOKEN_PRIVILEGES tp;
	TOKEN_PRIVILEGES oldtp;
	DWORD dwSize = sizeof(TOKEN_PRIVILEGES);
	LUID luid;
	TOKEN_PRIVILEGES tkp = { 0 };

	ZeroMemory(&tp, sizeof(tp));

	if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken)) {
		if (GetLastError() == ERROR_CALL_NOT_IMPLEMENTED) return TRUE;
		else return FALSE;
	}
	if (!LookupPrivilegeValue(NULL, SE_SHUTDOWN_NAME, &luid))
	{
		CloseHandle(hToken);
		return FALSE;
	}
	tp.PrivilegeCount = 1;
	tp.Privileges[0].Luid = luid;
	tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	if (!AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), &oldtp, &dwSize)) {
		CloseHandle(hToken);
		return FALSE;
	}

	if (!LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &luid))
	{
		CloseHandle(hToken);
		return FALSE;
	}
	tp.PrivilegeCount = 1;
	tp.Privileges[0].Luid = luid;
	tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	if (!AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), &oldtp, &dwSize)) {
		CloseHandle(hToken);
		return FALSE;
	}

	if (!LookupPrivilegeValue(NULL, SE_LOAD_DRIVER_NAME, &luid))
	{
		CloseHandle(hToken);
		return FALSE;
	}
	tp.PrivilegeCount = 1;
	tp.Privileges[0].Luid = luid;
	tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	if (!AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), &oldtp, &dwSize)) {
		CloseHandle(hToken);
		return FALSE;
	}

	CloseHandle(hToken);
	return TRUE;
}
void MEnumProcessCore()
{
	if (current_system_process) {
		free(current_system_process);
		current_system_process = NULL;
	}
	DWORD dwSize = 0;
	NtQuerySystemInformation(SystemProcessesAndThreadsInformation, NULL, 0, &dwSize);
	current_system_process = (PSYSTEM_PROCESSES)malloc(dwSize);
	NtQuerySystemInformation(SystemProcessesAndThreadsInformation, current_system_process, dwSize, 0);
}
M_API void MEnumProcessFree()
{

	if (current_system_process) {
		free(current_system_process);
		current_system_process = NULL;
	}
}
M_API void MEnumProcess(EnumProcessCallBack calBack)
{
	if (calBack)
	{
		HANDLE hProcess = NULL;
		MAppVProcessAllWindows();
		MEnumProcessCore();
		bool done = false;
		int ix = 0;
		for (PSYSTEM_PROCESSES p = current_system_process; !done; p = PSYSTEM_PROCESSES(PCHAR(p) + p->NextEntryDelta))
		{
			WCHAR exeFullPath[260];
			if (MGetProcessFullPathEx(static_cast<DWORD>((ULONG_PTR)p->ProcessId), exeFullPath, &hProcess, p->ProcessName.Buffer))
				calBack(static_cast<DWORD>((ULONG_PTR)p->ProcessId), static_cast<DWORD>((ULONG_PTR)p->InheritedFromProcessId), p->ProcessName.Buffer, exeFullPath, 1, hProcess);
			else calBack(static_cast<DWORD>((ULONG_PTR)p->ProcessId), static_cast<DWORD>((ULONG_PTR)p->InheritedFromProcessId), p->ProcessName.Buffer, 0, 1, NULL);
			ix++;
			done = p->NextEntryDelta == 0;
		}
		calBack(ix, 0, NULL, NULL, 0, 0);
	}
}
M_API void MEnumProcess2Refesh(EnumProcessCallBack2 callBack)
{
	if (callBack)
	{
		MAppVProcessAllWindows();
		MEnumProcessCore();
		bool done = false;
		for (PSYSTEM_PROCESSES p = current_system_process; !done; p = PSYSTEM_PROCESSES(PCHAR(p) + p->NextEntryDelta))
		{
			callBack(static_cast<DWORD>((ULONG_PTR)p->ProcessId));
			done = p->NextEntryDelta == 0;
		}
	}
}
M_API BOOL MReUpdateProcess(DWORD pid, EnumProcessCallBack calBack)
{
	bool done = false;
	//遍历进程列表
	for (PSYSTEM_PROCESSES p = current_system_process; !done;
		p = PSYSTEM_PROCESSES(PCHAR(p) + p->NextEntryDelta))
	{
		if (static_cast<DWORD>((ULONG_PTR)p->ProcessId) == pid)
		{
			HANDLE hProcess;
			WCHAR exeFullPath[260];
			if (MGetProcessFullPathEx(static_cast<DWORD>((ULONG_PTR)p->ProcessId), exeFullPath, &hProcess, p->ProcessName.Buffer))
				calBack(static_cast<DWORD>((ULONG_PTR)p->ProcessId), static_cast<DWORD>((ULONG_PTR)p->InheritedFromProcessId), p->ProcessName.Buffer, exeFullPath, 1, hProcess);
			else calBack(static_cast<DWORD>((ULONG_PTR)p->ProcessId), static_cast<DWORD>((ULONG_PTR)p->InheritedFromProcessId), p->ProcessName.Buffer, 0, 1, NULL);
			done = true;
			return TRUE;
		}
		done = p->NextEntryDelta == 0;
	}			
	return 0;
}

//EXE information
M_API BOOL MGetExeInfo(LPWSTR strFilePath, LPWSTR InfoItem, LPWSTR str, int maxCount)
{
	/*
	CompanyName
	FileDescription
	FileVersion
	InternalName
	LegalCopyright
	OriginalFilename
	ProductName
	ProductVersion
	Comments
	LegalTrademarks
	PrivateBuild
	SpecialBuild
	*/

	TCHAR   szResult[256];
	TCHAR   szGetName[256];
	LPWSTR  lpVersion = { 0 };        // String pointer to Item text
	DWORD   dwVerInfoSize;    // Size of version information block
	DWORD   dwVerHnd = 0;        // An 'ignored' parameter, always '0'
	UINT    uVersionLen;
	BOOL    bRetCode;

	dwVerInfoSize = GetFileVersionInfoSize(strFilePath, &dwVerHnd);
	if (dwVerInfoSize) {
		LPSTR   lpstrVffInfo;
		HANDLE  hMem;
		hMem = GlobalAlloc(GMEM_MOVEABLE, dwVerInfoSize);
		lpstrVffInfo = (LPSTR)GlobalLock(hMem);
		GetFileVersionInfo(strFilePath, dwVerHnd, dwVerInfoSize, lpstrVffInfo);
		lstrcpy(szGetName, L"\\VarFileInfo\\Translation");
		uVersionLen = 0;
		lpVersion = NULL;
		bRetCode = VerQueryValue((LPVOID)lpstrVffInfo,
			szGetName,
			(void **)&lpVersion,
			(UINT *)&uVersionLen);
		if (bRetCode && uVersionLen && lpVersion)
			wsprintf(szResult, L"%04x%04x", (WORD)(*((DWORD *)lpVersion)),
				(WORD)(*((DWORD *)lpVersion) >> 16));
		else lstrcpy(szResult, L"041904b0");
		wsprintf(szGetName, L"\\StringFileInfo\\%s\\", szResult);
		lstrcat(szGetName, InfoItem);
		uVersionLen = 0;
		lpVersion = NULL;
		bRetCode = VerQueryValue((LPVOID)lpstrVffInfo,
			szGetName,
			(void **)&lpVersion,
			(UINT *)&uVersionLen);
		if (bRetCode && uVersionLen && lpVersion) {
			if (str) {
				wcscpy_s(str, maxCount, lpVersion);
				return TRUE;
			}
		}
	}
	return FALSE;
}
M_API BOOL MGetExeDescribe(LPWSTR pszFullPath, LPWSTR str, int maxCount)
{
	return MGetExeInfo(pszFullPath, L"FileDescription", str, maxCount);
}
M_API BOOL MGetExeCompany(LPWSTR pszFullPath, LPWSTR str, int maxCount)
{
	return MGetExeInfo(pszFullPath, L"CompanyName", str, maxCount);
}
M_API HICON MGetExeIcon(LPWSTR pszFullPath)
{
	HICON hIcon = NULL;
	if (pszFullPath != NULL)
	{
		if (MStrEqualW(pszFullPath, L"")) {
			hIcon = HIconDef;
			return hIcon;
		}
		else {
			SHFILEINFO FileInfo;
			DWORD_PTR dwRet = SHGetFileInfoW(pszFullPath, FILE_ATTRIBUTE_NORMAL, &FileInfo, sizeof(SHFILEINFO), SHGFI_SYSICONINDEX | SHGFI_ICON | SHGFI_SMALLICON);
			if (dwRet) {
				hIcon = FileInfo.hIcon;
			}
		}
		//ExtractIconEx(pszFullPath, 0, NULL, &hIcon, 1);
	}
	if (hIcon == NULL)
		hIcon = HIconDef;
	return hIcon;
}
M_API BOOL MGetExeFileTrust(LPCWSTR lpFileName)
{
	BOOL bRet = FALSE;
	if (MFM_FileExist(lpFileName)) {
		WINTRUST_DATA wd = { 0 };
		WINTRUST_FILE_INFO wfi = { 0 };
		WINTRUST_CATALOG_INFO wci = { 0 };
		CATALOG_INFO ci = { 0 };
		HCATADMIN hCatAdmin = NULL;
		if (!CryptCATAdminAcquireContext(&hCatAdmin, NULL, 0))
		{
			return FALSE;
		}
		HANDLE hFile = CreateFileW(lpFileName, GENERIC_READ, FILE_SHARE_READ,
			NULL, OPEN_EXISTING, 0, NULL);
		if (INVALID_HANDLE_VALUE == hFile)
		{
			CryptCATAdminReleaseContext(hCatAdmin, 0);
			return FALSE;
		}
		DWORD dwCnt = 100;
		BYTE byHash[100];
		CryptCATAdminCalcHashFromFileHandle(hFile, &dwCnt, byHash, 0);
		CloseHandle(hFile);
		LPWSTR pszMemberTag = new WCHAR[dwCnt * 2 + 1];
		for (DWORD dw = 0; dw < dwCnt; ++dw)
		{
			wsprintfW(&pszMemberTag[dw * 2], L"%02X", byHash[dw]);
		}
		HCATINFO hCatInfo = CryptCATAdminEnumCatalogFromHash(hCatAdmin,
			byHash, dwCnt, 0, NULL);
		if (NULL == hCatInfo)
		{
			wfi.cbStruct = sizeof(WINTRUST_FILE_INFO);
			wfi.pcwszFilePath = lpFileName;
			wfi.hFile = NULL;
			wfi.pgKnownSubject = NULL;
			wd.cbStruct = sizeof(WINTRUST_DATA);
			wd.dwUnionChoice = WTD_CHOICE_FILE;
			wd.pFile = &wfi;
			wd.dwUIChoice = WTD_UI_NONE;
			wd.fdwRevocationChecks = WTD_REVOKE_NONE;
			wd.dwStateAction = WTD_STATEACTION_IGNORE;
			wd.dwProvFlags = WTD_SAFER_FLAG;
			wd.hWVTStateData = NULL;
			wd.pwszURLReference = NULL;
		}
		else
		{
			CryptCATCatalogInfoFromContext(hCatInfo, &ci, 0);
			wci.cbStruct = sizeof(WINTRUST_CATALOG_INFO);
			wci.pcwszCatalogFilePath = ci.wszCatalogFile;
			wci.pcwszMemberFilePath = lpFileName;
			wci.pcwszMemberTag = pszMemberTag;
			wd.cbStruct = sizeof(WINTRUST_DATA);
			wd.dwUnionChoice = WTD_CHOICE_CATALOG;
			wd.pCatalog = &wci;
			wd.dwUIChoice = WTD_UI_NONE;
			wd.fdwRevocationChecks = WTD_STATEACTION_VERIFY;
			wd.dwProvFlags = 0;
			wd.hWVTStateData = NULL;
			wd.pwszURLReference = NULL;
		}
		GUID action = GUID{ 0x00AAC56B, 0xCD44, 0x11d0, 0x8C, 0xC2, 0x00, 0xC0, 0x4F, 0xC2, 0x95, 0xEE };
		HRESULT hr = WinVerifyTrust(NULL, &action, &wd);
		bRet = SUCCEEDED(hr);
		if (NULL != hCatInfo)
		{
			CryptCATAdminReleaseCatalogContext(hCatAdmin, hCatInfo, 0);
		}
		CryptCATAdminReleaseContext(hCatAdmin, 0);
		delete[] pszMemberTag;
	}
	return bRet;
}

TCHAR szDriveStr[500];
BOOL driveStrGeted = FALSE;

//Kernel path
M_API BOOL MNtPathToFilePath(LPWSTR pszNtPath, LPWSTR pszFilePath, size_t bufferSize)
{
	//检查参数
	if (!pszFilePath || !pszNtPath)
		return FALSE;

	if (wcscmp(pszNtPath, L"") == 0 || wcsnlen_s(pszNtPath, 260) <= 12) {
		wcscpy_s(pszFilePath, bufferSize, pszNtPath);
		return TRUE;
	}

	if (wcsncmp(pszNtPath, L"\\SystemRoot\\", 12) == 0)
	{
		wcscpy_s(pszFilePath, bufferSize, L"C:\\Windows\\");
		wcscat_s(pszFilePath, bufferSize, pszNtPath + 12);
		return TRUE;
	}

	if (wcsncmp(pszNtPath, L"\\??\\", 4) == 0)
	{
		wcscpy_s(pszFilePath, bufferSize, pszNtPath + 4);
		return TRUE;
	}

	if (wcsncmp(pszNtPath, L"system32\\", 8) == 0)
	{
		wcscpy_s(pszFilePath, bufferSize, L"C:\\Windows\\");
		wcscat_s(pszFilePath, bufferSize, pszNtPath);
		return TRUE;
	}

	wcscpy_s(pszFilePath, bufferSize, pszNtPath);

	return FALSE;
}
M_API BOOL MDosPathToNtPath(LPWSTR pszDosPath, LPWSTR pszNtPath)
{
	TCHAR            szDrive[3];
	TCHAR            szDevName[100];
	INT                cchDevName;
	INT                i;
	//检查参数
	if (!pszDosPath || !pszNtPath)
		return FALSE;

	if (!driveStrGeted)
		if (!GetLogicalDriveStrings(sizeof(szDriveStr), szDriveStr)) {
			lstrcpy(pszNtPath, pszDosPath);
			return TRUE;
		}
		else driveStrGeted = TRUE;

	for (i = 0; szDriveStr[i]; i += 4)
	{
		if (!lstrcmpi(&(szDriveStr[i]), L"A:\\") || !lstrcmpi(&(szDriveStr[i]), L"B:\\"))
			continue;

		szDrive[0] = szDriveStr[i];
		szDrive[1] = szDriveStr[i + 1];
		szDrive[2] = '\0';
		if (!QueryDosDevice(szDrive, szDevName, 100)) {//查询 Dos 设备名		
			return FALSE;
		}
		cchDevName = lstrlen(szDevName);
		if (_tcsnicmp(pszDosPath, szDevName, cchDevName) == 0)//命中
		{
			lstrcpy(pszNtPath, szDrive);//复制驱动器
			lstrcat(pszNtPath, pszDosPath + cchDevName);//复制路径
			return TRUE;
		}
	}
	return FALSE;
}
M_API DWORD MGetNtPathFromHandle(HANDLE hFile, LPWSTR ps_NTPath, UINT szDosPathSize)
{
	if (hFile == 0 || hFile == INVALID_HANDLE_VALUE)
		return ERROR_INVALID_HANDLE;

	// NtQueryObject() returns STATUS_INVALID_HANDLE for Console handles
	if (IsConsoleHandle(hFile))
	{
		std::wstring s = FormatString(_T("\\Device\\Console%04X"), (DWORD)(DWORD_PTR)hFile);
		wcscpy_s(ps_NTPath, szDosPathSize, s.c_str());
		return ERROR_SUCCESS;
	}

	BYTE  u8_Buffer[2000];
	DWORD u32_ReqLength = 0;

	UNICODE_STRING* pk_Info = &((OBJECT_NAME_INFORMATION*)u8_Buffer)->Name;
	pk_Info->Buffer = 0;
	pk_Info->Length = 0;

	// IMPORTANT: The return value from NtQueryObject is bullshit! (driver bug?)
	// - The function may return STATUS_NOT_SUPPORTED although it has successfully written to the buffer.
	// - The function returns STATUS_SUCCESS although h_File == 0xFFFFFFFF
	NtQueryObject(hFile, OBJECT_INFORMATION_CLASS(ObjectNameInformation), u8_Buffer, sizeof(u8_Buffer), &u32_ReqLength);

	// On error pk_Info->Buffer is NULL
	if (!pk_Info->Buffer || !pk_Info->Length)
		return ERROR_FILE_NOT_FOUND;

	pk_Info->Buffer[pk_Info->Length / 2] = 0; // Length in Bytes!
	wcscpy_s(ps_NTPath, szDosPathSize, pk_Info->Buffer);
	return ERROR_SUCCESS;
}
M_API DWORD MNtPathToDosPath(LPWSTR pszNtPath, LPWSTR pszDosPath, UINT szDosPathSize)
{
	DWORD u32_Error;

	if (_tcsnicmp(pszNtPath, _T("\\Device\\Serial"), 14) == 0 || // e.g. "Serial1"
		_tcsnicmp(pszNtPath, _T("\\Device\\UsbSer"), 14) == 0)   // e.g. "USBSER000"
	{
		HKEY h_Key;
		if (u32_Error = RegOpenKeyEx(HKEY_LOCAL_MACHINE, _T("Hardware\\DeviceMap\\SerialComm"), 0, KEY_QUERY_VALUE, &h_Key))
			return u32_Error;

		TCHAR u16_ComPort[50];

		DWORD u32_Type;
		DWORD u32_Size = sizeof(u16_ComPort);
		if (u32_Error = RegQueryValueEx(h_Key, pszNtPath, 0, &u32_Type, (BYTE*)u16_ComPort, &u32_Size))
		{
			RegCloseKey(h_Key);
			return ERROR_UNKNOWN_PORT;
		}

		wcscpy_s(pszDosPath, szDosPathSize, u16_ComPort);
		RegCloseKey(h_Key);
		return ERROR_SUCCESS;
	}

	if (_tcsnicmp(pszNtPath, _T("\\Device\\LanmanRedirector\\"), 25) == 0) // Win XP
	{
		wcscpy_s(pszDosPath, szDosPathSize, _T("\\\\"));
		wcscat_s(pszDosPath, szDosPathSize, (pszNtPath + 25));
		return ERROR_SUCCESS;
	}

	if (_tcsnicmp(pszNtPath, _T("\\Device\\Mup\\"), 12) == 0) // Win 7
	{
		wcscpy_s(pszDosPath, szDosPathSize, _T("\\\\"));
		wcscat_s(pszDosPath, szDosPathSize, (pszNtPath + 12));
		return ERROR_SUCCESS;
	}

	TCHAR u16_Drives[300];
	if (!GetLogicalDriveStrings(300, u16_Drives))
		return GetLastError();

	TCHAR* u16_Drv = u16_Drives;
	while (u16_Drv[0])
	{
		TCHAR* u16_Next = u16_Drv + _tcslen(u16_Drv) + 1;

		u16_Drv[2] = 0; // the backslash is not allowed for QueryDosDevice()

		TCHAR u16_NtVolume[1000];
		u16_NtVolume[0] = 0;

		// may return multiple strings!
		// returns very weird strings for network shares
		if (!QueryDosDevice(u16_Drv, u16_NtVolume, sizeof(u16_NtVolume) / sizeof(TCHAR)))
			return GetLastError();

		int s32_Len = (int)_tcslen(u16_NtVolume);
		if (s32_Len > 0 && _tcsnicmp(pszNtPath, u16_NtVolume, s32_Len) == 0)
		{
			wcscpy_s(pszDosPath, szDosPathSize, u16_Drv);
			wcscat_s(pszDosPath, szDosPathSize, (pszNtPath + s32_Len));
			return ERROR_SUCCESS;
		}

		u16_Drv = u16_Next;
	}
	return ERROR_BAD_PATHNAME;
}

//Process information
M_API BOOL MGetProcessFullPathEx(DWORD dwPID, LPWSTR outNter, PHANDLE phandle, LPWSTR pszExeName)
{
	if (dwPID == 0) {
		wcscpy_s(outNter, 260, str_item_systemidleproc);
		MOpenProcessNt(dwPID, phandle);
		return 1;
	}
	else if (dwPID == 4) {
		wcscpy_s(outNter, 260, L"C:\\Windows\\System32\\ntoskrnl.exe"); 
		return 1;
	}
	else if (dwPID == 88 && MStrEqualW(pszExeName, L"Registry")) {
		wcscpy_s(outNter, 260, L"C:\\Windows\\System32\\ntoskrnl.exe");
		return 1;
	}
	else if (dwPID == 88 && MStrEqualW(pszExeName, L"Memory Compression")) {
		wcscpy_s(outNter, 260, L"C:\\Windows\\System32\\ntoskrnl.exe");
		return 1;
	}

	TCHAR szResult[MAX_PATH];
	TCHAR szImagePath[MAX_PATH];
	HANDLE hProcess;

	NTSTATUS rs = MOpenProcessNt(dwPID, &hProcess);
	if (!hProcess || rs != STATUS_SUCCESS) return FALSE;
	if (!K32GetProcessImageFileNameW(hProcess, szImagePath, MAX_PATH))
	{
		if (hProcess != INVALID_HANDLE_VALUE && hProcess != (HANDLE)0xCCCCCCCCULL)
			CloseHandle(hProcess);
		return FALSE;
	}
	if (!MDosPathToNtPath(szImagePath, szResult)) return FALSE;
	wcscpy_s(outNter, 260, szResult);
	if (phandle)*phandle = hProcess;
	else MCloseHandle(hProcess);
	return TRUE;
}
M_API int MGetProcessState(DWORD pid, HWND hWnd)
{
	bool done = false;
	//遍历进程列表
	for (PSYSTEM_PROCESSES p = current_system_process; !done;
	p = PSYSTEM_PROCESSES(PCHAR(p) + p->NextEntryDelta))
	{
		if (static_cast<DWORD>((ULONG_PTR)p->ProcessId) == pid)
		{
			SYSTEM_THREADS systemThread = p->Threads[0];
			if (systemThread.ThreadState == THREAD_STATE::StateWait && systemThread.WaitReason == Suspended)
				return 2;
			else return 1;		
		}
		done = p->NextEntryDelta == 0;
	}
	return 0;
}
M_API VOID* MGetProcessThreads(DWORD pid)
{
	bool done = false;
	//遍历进程列表
	for (PSYSTEM_PROCESSES p = current_system_process; !done;
		p = PSYSTEM_PROCESSES(PCHAR(p) + p->NextEntryDelta))
	{
		if (static_cast<DWORD>((ULONG_PTR)p->ProcessId) == pid)
			return p->Threads;
		done = p->NextEntryDelta == 0; 
	}
	return 0;
}
M_API BOOL MGetProcessCommandLine(HANDLE handle, LPWSTR l, int maxcount) {
	if (handle && l) {
		DWORD id = GetProcessId(handle);
		if (id != 0 && id != 4) {
			PUNICODE_STRING commandLine;
			NTSTATUS status = MQueryProcessVariableSize(handle, ProcessCommandLineInformation, (PVOID*)&commandLine);
			if (NT_SUCCESS(status)) {
				wcscpy_s(l, maxcount, commandLine->Buffer);
				free(commandLine);
				return TRUE;
			}
		}
		else {
			wcscpy_s(l, maxcount, L"");
			return TRUE;
		}
	}
	return FALSE;
}
M_API BOOL MGetProcessIsUWP(HANDLE handle)
{
	return dIsImmersiveProcess(handle);
}
M_API BOOL MGetProcessIs32Bit(HANDLE handle) {
	BOOL rs = TRUE;
	IsWow64Process(handle, &rs);
	return rs;
}
M_API BOOL MGetProcessEprocess(DWORD pid, LPWSTR l, int maxcount)
{
	ULONG_PTR outEprocess = 0;
	if (M_SU_GetEPROCESS(pid, &outEprocess))
	{
#ifdef _X64_
		swprintf_s(l, maxcount, L"0x%I64X", outEprocess);
#else
		swprintf_s(l, maxcount, L"0x%08X", outEprocess);
#endif
		return TRUE;
	}
	return FALSE;
}

//UWP Process information
/*M_API BOOL MGetUWPPackageId(HANDLE handle, MPerfAndProcessData*data)
{
	if (handle && data)
	{
		UINT32 bufferLength = 0;
		LONG result = dGetPackageId(handle, &bufferLength, nullptr);
		BYTE* buffer = (PBYTE)malloc(bufferLength);
		result = dGetPackageId(handle, &bufferLength, buffer);
		if (result == ERROR_SUCCESS) {
			data->packageId = reinterpret_cast<PACKAGE_ID*>(buffer);
			return TRUE;
		}
	}
	return 0;
}*/
M_API BOOL MGetUWPPackageFullName(HANDLE handle, int*len, LPWSTR buffer)
{
	if (*len == 0)
	{
		UINT32 len2 = 0;
		dGetPackageFullName(handle, &len2, NULL);
		*len = static_cast<int>(len2);
		return TRUE;
	}
	else {
		if (buffer)
		{
			UINT32 len2 = static_cast<UINT32>(*len); ;
			return dGetPackageFullName(handle, &len2, buffer) == ERROR_SUCCESS;
		}
	}
	return 0;
}

//..
M_API BOOL MCloseHandle(HANDLE handle)
{
	return CloseHandle(handle);
}
//Process Control
M_API NTSTATUS MSuspendProcessNt(DWORD dwPId, HANDLE handle)
{
	if (dwPId != 0 && dwPId != 4 && dwPId > 0) {
		HANDLE hProcess;
		NTSTATUS rs = MOpenProcessNt(dwPId, &hProcess);
		if (hProcess) {
			rs = ZwSuspendProcess(hProcess);
			MCloseHandle(hProcess);
			if (rs == STATUS_SUCCESS)
				return STATUS_SUCCESS;
			else {
				LogErr(L"SuspendProcess failed (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
				return rs;
			}
		}
		else {
			if (rs == STATUS_ACCESS_DENIED && MCanUseKernel()) {
				LogWarn(L"SuspendProcess failed in OpenProcess : (PID : %d) , Use kernel mode to Suspend it.");
				M_SU_SuspendProcess(dwPId, 0, &rs);
				if(rs != STATUS_SUCCESS)
					LogErr(L"SuspendProcess failed in kernel : (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
			}
			else LogErr(L"SuspendProcess failed in OpenProcess : (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
		}
		return rs;
	}
	else if (handle)
	{
		NTSTATUS  rs = ZwSuspendProcess(handle);
		if (rs == 0)
			return TRUE;
		else {
			LogErr(L"SuspendProcess failed NTSTATUS : 0x%08X", rs);
			return rs;
		}
	}
	return FALSE;
}
M_API NTSTATUS MRusemeProcessNt(DWORD dwPId, HANDLE handle)
{
	if (dwPId != 0 && dwPId != 4 && dwPId > 0) {
		HANDLE hProcess;
		NTSTATUS rs = MOpenProcessNt(dwPId, &hProcess);
		if (hProcess)
		{
			rs = ZwResumeProcess(hProcess);
			MCloseHandle(hProcess);
			if (rs == STATUS_SUCCESS) return STATUS_SUCCESS;
			else {
				LogErr(L"RusemeProcess failed (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
				return rs;
			}
		}
		else {
			if (rs == STATUS_ACCESS_DENIED && MCanUseKernel()) {
				LogWarn(L"RusemeProcess failed in OpenProcess : (PID : %d) , Use kernel mode to Ruseme it.");
				M_SU_ResumeProcess(dwPId, 0, &rs);
				if (rs != STATUS_SUCCESS)
					LogErr(L"RusemeProcess failed in kernel : (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
			}
			else LogErr(L"RusemeProcess failed in OpenProcess : (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
		}
	}
	else if (handle)
	{
		NTSTATUS rs = ZwResumeProcess(handle);
		if (rs == 0)return TRUE;
		else
		{
			LogErr(L"RusemeProcess failed NTSTATUS : 0x%08X", rs);
			return rs;
		}
	}
	return FALSE;
}
M_API NTSTATUS MOpenProcessNt(DWORD dwId, PHANDLE pLandle)
{
	HANDLE hProcess;
	OBJECT_ATTRIBUTES ObjectAttributes;
	_CLIENT_ID ClientId;

	ObjectAttributes.Length = sizeof(OBJECT_ATTRIBUTES);
	ObjectAttributes.RootDirectory = NULL;
	ObjectAttributes.ObjectName = NULL;
	ObjectAttributes.Attributes = OBJ_KERNEL_HANDLE | OBJ_CASE_INSENSITIVE;
	ObjectAttributes.SecurityDescriptor = NULL;
	ObjectAttributes.SecurityQualityOfService = NULL;

	ClientId.UniqueThread = 0;
	ClientId.UniqueProcess = (HANDLE)(long long)dwId;

	NTSTATUS NtStatus = ZwOpenProcess(
		&hProcess,
		PROCESS_ALL_ACCESS,
		&ObjectAttributes,
		&ClientId);

	if (NtStatus == STATUS_SUCCESS) {
		*pLandle = hProcess;
		return STATUS_SUCCESS;
	}
	else return NtStatus;
}
M_API NTSTATUS MTerminateProcessNt(DWORD dwId, HANDLE handle)
{
	if (handle) {
		NTSTATUS rs = ZwTerminateProcess(handle, 0);
		if (rs == 0) return STATUS_SUCCESS;
		else {
			LogErr(L"TerminateProcess failed NTSTATUS : 0x%08X", rs);
			return rs;
		}
	}
	else
	{
		if (dwId != 0 && dwId != 4 && dwId > 0) {
			HANDLE hProcess;
			NTSTATUS rs = MOpenProcessNt(dwId, &hProcess);
			if (hProcess)
			{
				rs = ZwTerminateProcess(hProcess, 0);
				MCloseHandle(hProcess);
				if (rs == 0) return STATUS_SUCCESS;
				else {
					LogErr(L"TerminateProcess failed : (PID : %d) NTSTATUS : 0x%08X", dwId, rs);
					return rs;
				}
			}
			else {
				if (rs == STATUS_ACCESS_DENIED && MCanUseKernel())
					LogWarn(L"TerminateProcess failed in OpenProcess : (PID : %d) NTSTATUS : STATUS_ACCESS_DENIED\n\
                    You can Terminate it in kernel mode.", dwId, rs);
				else LogErr(L"TerminateProcess failed in OpenProcess : (PID : %d) NTSTATUS : 0x%08X", dwId, rs);
			}
			return rs;
		}
		else return STATUS_ACCESS_DENIED;
	}
}


M_API int MAppWorkShowMenuProcessPrepare(LPWSTR strFilePath, LPWSTR strFileName, DWORD pid)
{
	thisCommandPid = pid;
	if (pid > 0)
	{
		if (!MStrEqualW(strFilePath, L"") && wcslen(strFilePath) < 260) {
			wcscpy_s(thisCommandPath, 260, strFilePath);
			if (wcslen(strFileName) < 260)
				wcscpy_s(thisCommandName, 260, strFileName);
		}
		return 0;
	}
	else if (pid == 0)
	{
		if (wcslen(strFilePath) < 260)
			wcscpy_s(thisCommandPath, 260, strFilePath);
	}
	return 0;
}
//MENU
M_API int MAppWorkShowMenuProcess(LPWSTR strFilePath, LPWSTR strFileName, DWORD pid, HWND hDlg, int data, int type)
{
	thisCommandPid = pid;
	if (pid > 0)
	{
		HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUTASK));
		if (hroot) {
			HMENU hpop = GetSubMenu(hroot, 0);
			POINT pt;
			GetCursorPos(&pt);
			if (pid == 4) {
				EnableMenuItem(hpop, IDM_SUPROC, MF_DISABLED);
				EnableMenuItem(hpop, IDM_RESPROC, MF_DISABLED);
				EnableMenuItem(hpop, IDM_KILLKERNEL, MF_DISABLED);
				EnableMenuItem(hpop, IDM_KILLPROCTREE, MF_DISABLED);
				EnableMenuItem(hpop, IDM_OPENPATH, MF_DISABLED);
				EnableMenuItem(hpop, IDM_VTHREAD, MF_DISABLED);
				EnableMenuItem(hpop, IDM_VHANDLES, MF_DISABLED);
				EnableMenuItem(hpop, IDM_VMODULS, MF_DISABLED);
				EnableMenuItem(hpop, IDM_VWINS, MF_DISABLED);
				EnableMenuItem(hpop, IDM_KILL, MF_DISABLED);
			}
			if (MStrEqualW(strFilePath, L"") || MStrEqualW(strFilePath, L"-")) {
				EnableMenuItem(hpop, IDM_OPENPATH, MF_DISABLED);
				EnableMenuItem(hpop, IDM_FILEPROP, MF_DISABLED);
			}
			else if (wcslen(strFilePath) < 260)
				wcscpy_s(thisCommandPath, 260, strFilePath);
			if (wcslen(strFileName) < 260)
				wcscpy_s(thisCommandName, 260, strFileName);

			killCmdSendBack = type == 2;
			isKillingExplorer = type == 1;

			if (type == 1) {
				MENUITEMINFO info = MENUITEMINFO();
				info.cbSize = sizeof(MENUITEMINFO);
				info.fMask = MIIM_STRING;
				info.dwTypeData = str_item_rebootexplorer;
				SetMenuItemInfo(hpop, IDM_KILL, FALSE, &info);
			}			
			else if (type == 2) {
				MENUITEMINFO info = MENUITEMINFO();
				info.cbSize = sizeof(MENUITEMINFO);
				info.fMask = MIIM_STRING;
				info.dwTypeData = str_item_endtask;
				SetMenuItemInfo(hpop, IDM_KILL, FALSE, &info);
			}

			TrackPopupMenu(hpop,
				TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
				pt.x,
				pt.y,
				0,
				hWndMain,
				NULL);

			DestroyMenu(hroot);
		}
	}
	else if(pid==0)
	{
		HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUTASK));
		if (hroot) {
			HMENU hpop = GetSubMenu(hroot, 0);
			POINT pt;
			GetCursorPos(&pt);

			EnableMenuItem(hpop, IDM_KILL, MF_DISABLED);
			EnableMenuItem(hpop, IDM_SUPROC, MF_DISABLED);
			EnableMenuItem(hpop, IDM_RESPROC, MF_DISABLED);
			EnableMenuItem(hpop, IDM_KILLKERNEL, MF_DISABLED);
			EnableMenuItem(hpop, IDM_KILLPROCTREE, MF_DISABLED);
			EnableMenuItem(hpop, IDM_OPENPATH, MF_DISABLED);
			EnableMenuItem(hpop, IDM_VTHREAD, MF_DISABLED);
			EnableMenuItem(hpop, IDM_VHANDLES, MF_DISABLED);
			EnableMenuItem(hpop, IDM_VMODULS, MF_DISABLED);
			EnableMenuItem(hpop, IDM_VWINS, MF_DISABLED);

			if (MStrEqualW(strFilePath, L"") || MStrEqualW(strFilePath, L"-")) {
				EnableMenuItem(hpop, IDM_OPENPATH, MF_DISABLED);
				EnableMenuItem(hpop, IDM_FILEPROP, MF_DISABLED);
			}

			if (wcslen(strFilePath) < 260)
				wcscpy_s(thisCommandPath, 260, strFilePath);

			TrackPopupMenu(hpop,
				TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
				pt.x,
				pt.y,
				0,
				hWndMain,
				NULL);

			DestroyMenu(hroot);
		}
	}
	return 0;
}

extern MEMORYSTATUSEX memory_statuex;

//Ram
M_API double MGetRamUseAge()
{
	MPERF_GetRamUseAge();
	double ram = ((memory_statuex.ullTotalPhys - memory_statuex.ullAvailPhys) / (double)memory_statuex.ullTotalPhys);
	return ram;
}
M_API ULONG MGetAllRam()
{
	MPERF_GetRamUseAge();
	return static_cast<ULONG>(memory_statuex.ullTotalPhys / 1048576);
}

//ntdll apis

HINSTANCE hNtDll;
HINSTANCE hShell32;
HINSTANCE hKernel32;
HINSTANCE hUser32;

BOOL LoadDll()
{
	hNtDll = LoadLibrary(L"ntdll.dll");
	hShell32 = LoadLibrary(L"shell32.dll");
	hKernel32 = GetModuleHandle(L"kernel32.dll");
	hUser32 = GetModuleHandle(L"user32.dll");
	thisCommandPath = new WCHAR[260];
	thisCommandName = new WCHAR[260];
	if (hNtDll == NULL) {
		FreeLibrary(hNtDll);
		MessageBox(NULL, L"Load NTDLL ERROR", L"ERROR !", MB_OK | MB_ICONERROR);
		return FALSE;
	}
	else {
		/*HINSTANCE hMain = GetModuleHandle(NULL);
		KsGetState = (KsGetStateFun)GetProcAddress(hMain, "KsGetState");
		if (KsGetState == NULL) {
		MessageBox(0, L"PC Mgr CoreDll错误：错误的调用者。 ", L"错误", MB_ICONERROR | MB_OK);
		}

		KsOpenProcessHandle = (KsOpenProcessHandleFun)GetProcAddress(hMain, "KsOpenProcessHandle");
		KsOpenThreadHandle = (KsOpenThreadHandleFun)GetProcAddress(hMain, "KsOpenThreadHandle");
		KsTerminateProcess = (KsTerminateProcessFun)GetProcAddress(hMain, "KsTerminateProcess");
		KsTerminateThread = (KsTerminateThreadFun)GetProcAddress(hMain, "KsTerminateThread");
		KsSuspendProcess = (KsSuspendProcessFun)GetProcAddress(hMain, "KsSuspendProcess");
		KsResusemeProcess = (KsResusemeProcessFun)GetProcAddress(hMain, "KsResusemeProcess");*/

		//ntdll
		NtQuerySystemInformation = (NtQuerySystemInformationFun)GetProcAddress(hNtDll, "NtQuerySystemInformation");
		ZwSuspendProcess = (ZwSuspendProcessFun)GetProcAddress(hNtDll, "ZwSuspendProcess");
		ZwResumeProcess = (ZwResumeProcessFun)GetProcAddress(hNtDll, "ZwResumeProcess");
		ZwTerminateProcess = (ZwTerminateProcessFun)GetProcAddress(hNtDll, "ZwTerminateProcess");
		ZwOpenProcess = (ZwOpenProcessFun)GetProcAddress(hNtDll, "ZwOpenProcess");
		ZwOpenThread = (ZwOpenThreadFun)GetProcAddress(hNtDll, "ZwOpenThread");
		ZwQueryInformationThread = (ZwQueryInformationThreadFun)GetProcAddress(hNtDll, "ZwQueryInformationThread");
		RtlNtStatusToDosError = (RtlNtStatusToDosErrorFun)GetProcAddress(hNtDll, "RtlNtStatusToDosError");
		RtlGetLastWin32Error = (RtlGetLastWin32ErrorFun)GetProcAddress(hNtDll, "RtlGetLastWin32Error");
		ZwResumeThread = (ZwResumeThreadFun)GetProcAddress(hNtDll, "ZwResumeThread");
		ZwTerminateThread = (ZwTerminateThreadFun)GetProcAddress(hNtDll, "ZwTerminateThread");
		ZwSuspendThread = (ZwSuspendThreadFun)GetProcAddress(hNtDll, "ZwSuspendThread");
		NtUnmapViewOfSection = (NtUnmapViewOfSectionFun)GetProcAddress(hNtDll, "NtUnmapViewOfSection");
		NtQueryInformationProcess = (NtQueryInformationProcessFun)GetProcAddress(hNtDll, "NtQueryInformationProcess");
		LdrGetProcedureAddress = (LdrGetProcedureAddressFun)GetProcAddress(hNtDll, "LdrGetProcedureAddress");
		RtlInitAnsiString = (RtlInitAnsiStringFun)GetProcAddress(hNtDll, "RtlInitAnsiString");
		NtQueryObject = (NtQueryObjectFun)GetProcAddress(hNtDll, "NtQueryObject");
		//shell32
		RunFileDlg = (_RunFileDlg)MGetProcedureAddress(hShell32, NULL, 61);
		//k32
		dIsImmersiveProcess = (_IsImmersiveProcess)GetProcAddress(hUser32, "IsImmersiveProcess");
		dGetPackageFullName = (_GetPackageFullName)GetProcAddress(hKernel32, "GetPackageFullName");
		dGetPackageInfo = (_GetPackageInfo)GetProcAddress(hKernel32, "GetPackageInfo");
		dClosePackageInfo = (_ClosePackageInfo)GetProcAddress(hKernel32, "ClosePackageInfo");
		dOpenPackageInfoByFullName = (_OpenPackageInfoByFullName)GetProcAddress(hKernel32, "OpenPackageInfoByFullName");
	    dGetPackageId = (_GetPackageId)GetProcAddress(hKernel32, "GetPackageId");

		return TRUE;
	}
}
void FreeDll() {

	delete thisCommandPath;
	delete thisCommandName;
}
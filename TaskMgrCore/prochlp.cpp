#include "stdafx.h"
#include "prochlp.h"
#include "ntdef.h"
#include "perfhlp.h"
#include "syshlp.h"
#include "nthlp.h"
#include "resource.h"
#include <Psapi.h>
#include <process.h>
#include <tlhelp32.h>
#include <Shlobj.h>
#include <tchar.h>
#include <shellapi.h>

extern HINSTANCE hInst;

LPWSTR thisCommandPath = NULL;
LPWSTR thisCommandName = NULL;
DWORD thisCommandPid = 0;
HICON HIconDef;
HWND hWndMain;
PSYSTEM_PROCESSES current_system_process = NULL;


//Api s

//shell32
_RunFileDlg RunFileDlg;
//ntdll
ZwSuspendThreadFun ZwSuspendThread;
ZwResumeThreadFun ZwResumeThread;
ZwTerminateThreadFun ZwTerminateThread;
ZwOpenThreadFun ZwOpenThread;
ZwQueryInformationThreadFun ZwQueryInformationThread;
ZwSuspendProcessFun ZwSuspendProcess;
ZwResumeProcessFun ZwResumeProcess;
ZwTerminateProcessFun ZwTerminateProcess;
ZwOpenProcessFun ZwOpenProcess;
NtQuerySystemInformationFun NtQuerySystemInformation;
NtUnmapViewOfSectionFun NtUnmapViewOfSection;
NtQueryInformationProcessFun NtQueryInformationProcess;
LdrGetProcedureAddressFun LdrGetProcedureAddress;
RtlInitAnsiStringFun RtlInitAnsiString;
RtlNtStatusToDosErrorFun RtlNtStatusToDosError;
RtlGetLastWin32ErrorFun RtlGetLastWin32Error;
_IsWow64Process dIsWow64Process;
//K32 api
_IsImmersiveProcess dIsImmersiveProcess;
_GetPackageFullName dGetPackageFullName;
_GetPackageInfo dGetPackageInfo;
_ClosePackageInfo dClosePackageInfo;
_OpenPackageInfoByFullName dOpenPackageInfoByFullName;
_GetPackageId dGetPackageId;

//Enum apis
EXTERN_C BOOL MAppVProcessAllWindows();

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
			if (MGetProcessFullPathEx(p->ProcessId, exeFullPath, &hProcess, p->ProcessName.Buffer))
				calBack(p->ProcessId, p->InheritedFromProcessId, p->ProcessName.Buffer, exeFullPath, 1, hProcess);
			else calBack(p->ProcessId, p->InheritedFromProcessId, p->ProcessName.Buffer, 0, 1, NULL);
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
			callBack(p->ProcessId);
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
		if (p->ProcessId == pid)
		{
			HANDLE hProcess;
			WCHAR exeFullPath[260];
			if (MGetProcessFullPathEx(p->ProcessId, exeFullPath, &hProcess, p->ProcessName.Buffer))
				calBack(p->ProcessId, p->InheritedFromProcessId, p->ProcessName.Buffer, exeFullPath, 1, hProcess);
			else calBack(p->ProcessId, p->InheritedFromProcessId, p->ProcessName.Buffer, 0, 1, NULL);
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
		if (wcscmp(pszFullPath, L"") == 0) {
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

TCHAR szDriveStr[500];
BOOL driveStrGeted = FALSE;

//Process information
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
M_API BOOL MGetProcessFullPathEx(DWORD dwPID, LPWSTR outNter, PHANDLE phandle, LPWSTR pszExeName)
{
	if (dwPID == 0) {
		wcscpy_s(outNter, 260, L"系统空闲进程"); 
		MOpenProcessNt(dwPID, phandle);
		return 1;
	}
	else if (dwPID == 4) {
		wcscpy_s(outNter, 260, L"C:\\Windows\\System32\\ntoskrnl.exe"); 
		return 1;
	}
	else if (dwPID == 88 && wcscmp(pszExeName, L"Registry") == 0) {
		wcscpy_s(outNter, 260, L"C:\\Windows\\System32\\ntoskrnl.exe");
		return 1;
	}
	else if (dwPID == 88 && wcscmp(pszExeName, L"Memory Compression") == 0) {
		wcscpy_s(outNter, 260, L"C:\\Windows\\System32\\ntoskrnl.exe");
		return 1;
	}

	TCHAR szResult[MAX_PATH];
	TCHAR szImagePath[MAX_PATH];
	HANDLE hProcess;

	int rs = MOpenProcessNt(dwPID, &hProcess);
	if (!hProcess || rs != 1) return FALSE;
	if (!K32GetProcessImageFileNameW(hProcess, szImagePath, MAX_PATH))
	{
		if (hProcess != INVALID_HANDLE_VALUE && hProcess != (HANDLE)0xCCCCCCCCL)
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
		if (p->ProcessId == pid)
		{
			SYSTEM_THREADS systemThread = p->Threads[0];
			if (systemThread.ThreadState == THREAD_STATE::StateWait && systemThread.WaitReason == Suspended)
			{
				return 2;
			}
			else
			{
				return 1;
			}
			done = true;
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
		if (p->ProcessId == pid)
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
M_API DWORD MSuspendProcessNt(DWORD dwPId, HANDLE handle)
{
	if (dwPId != 0 && dwPId != 4 && dwPId > 0) {
		HANDLE hProcess;
		DWORD rs = MOpenProcessNt(dwPId, &hProcess);
		if (hProcess) {
			rs = ZwSuspendProcess(hProcess);
			MCloseHandle(hProcess);
			if (rs == 0)
				return TRUE;
			else return rs;
		}
		return rs;
	}
	else if (handle)
	{
		DWORD  rs = ZwSuspendProcess(handle);
		if (rs == 0)
			return TRUE;
		else return rs;
	}
	return FALSE;
}
M_API DWORD MRusemeProcessNt(DWORD dwPId, HANDLE handle)
{
	if (dwPId != 0 && dwPId != 4 && dwPId > 0) {
		HANDLE hProcess;
		MOpenProcessNt(dwPId, &hProcess);
		if (hProcess)
		{
			DWORD rs = ZwResumeProcess(hProcess);
			MCloseHandle(hProcess);
			if (rs == 0) return TRUE;
			else return rs;
		}
	}
	else if (handle)
	{
		DWORD rs = ZwResumeProcess(handle);
		if (rs == 0) return TRUE;
		else return rs;
	}
	return FALSE;
}
M_API DWORD MOpenProcessNt(DWORD dwId, PHANDLE pLandle)
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

	DWORD NtStatus = ZwOpenProcess(
		&hProcess,
		PROCESS_ALL_ACCESS,
		&ObjectAttributes,
		&ClientId);

	if (NtStatus == 0) {
		*pLandle = hProcess;
		return 1;
	}
	else if (NtStatus == 0xC0000008 || NtStatus == 0xC000000B)
		return -1;
	else return NtStatus;
}
M_API DWORD MTerminateProcessNt(DWORD dwId, HANDLE handle)
{
	if (handle) {
		DWORD rs = ZwTerminateProcess(handle, 0);
		if (rs == 0) return TRUE;
		else return rs;
	}
	else
	{
		if (dwId != 0 && dwId != 4 && dwId > 0) {
			HANDLE hProcess;
			MOpenProcessNt(dwId, &hProcess);
			if (hProcess)
			{
				DWORD rs = ZwTerminateProcess(hProcess, 0);
				MCloseHandle(hProcess);
				if (rs == 0) return TRUE;
				else return rs;
			}
			else return 0xC0000022;
		}
		else return 0xC0000022;
	}
}

//MENU
M_API int MAppWorkShowMenuProcess(LPWSTR strFilePath, LPWSTR strFileName, DWORD pid, HWND hDlg, int data)
{
	thisCommandPid = pid;
	if (pid > 0)
	{
		HMENU hroot = LoadMenu(hInst, MAKEINTRESOURCE(IDR_MENUTASK));
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
			}
			if (wcscmp(strFilePath, L"") == 0 || wcscmp(strFilePath, L"-") == 0) {
				EnableMenuItem(hpop, IDM_OPENPATH, MF_DISABLED);
				EnableMenuItem(hpop, IDM_FILEPROP, MF_DISABLED);
			}
			else if (wcslen(strFilePath) < 260)
				wcscpy_s(thisCommandPath, 260, strFilePath);
			if (wcslen(strFileName) < 260)
				wcscpy_s(thisCommandName, 260, strFileName);
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
		HMENU hroot = LoadMenu(hInst, MAKEINTRESOURCE(IDR_MENUTASK));
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

			if (wcscmp(strFilePath, L"") == 0 || wcscmp(strFilePath, L"-") == 0) {
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
		MessageBox(NULL, L"无法加载NTDLL，程序将退出。", L"发生了未知错误", MB_OK | MB_ICONERROR);
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
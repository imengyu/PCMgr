#include "stdafx.h"
#include "prochlp.h"
#include "ntdef.h"
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
ZwSuspendThreadFun ZwSuspendThread;
ZwResumeThreadFun ZwResumeThread;
ZwTerminateThreadFun ZwTerminateThread;
ZwOpenThreadFun ZwOpenThread;
ZwQueryInformationThreadFun ZwQueryInformationThread;
RtlNtStatusToDosErrorFun RtlNtStatusToDosError;
RtlGetLastWin32ErrorFun RtlGetLastWin32Error;

ZwSuspendProcessFun ZwSuspendProcess;
ZwResumeProcessFun ZwResumeProcess;
ZwTerminateProcessFun ZwTerminateProcess;
ZwOpenProcessFun ZwOpenProcess;
NtQuerySystemInformationFun NtQuerySystemInformation;
NtUnmapViewOfSectionFun NtUnmapViewOfSection;
NtQueryInformationProcessFun NtQueryInformationProcess;

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
M_API void MEnumProcess(EnumProcessCallBack calBack)
{
	if (calBack)
	{
		MAppVProcessAllWindows();

		HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
		if (INVALID_HANDLE_VALUE == hSnapshot)
			return;
		PROCESSENTRY32 pe = { 0 };
		pe.dwSize = sizeof(PROCESSENTRY32);

		int ix = 0;
		BOOL fOk;
		for (fOk = Process32First(hSnapshot, &pe); fOk; fOk = Process32Next(hSnapshot, &pe))
		{
			WCHAR exeFullPath[260];
			if (MGetProcessFullPathEx(pe.th32ProcessID, exeFullPath))
				calBack(pe.th32ProcessID, pe.th32ParentProcessID, pe.szExeFile, exeFullPath, 1);
			else calBack(pe.th32ProcessID, pe.th32ParentProcessID, pe.szExeFile, 0, 1);
			ix++;
		}
		calBack(ix, 0, NULL, NULL, 0);
	}
}

M_API BOOL MDosPathToNtPath(LPWSTR pszDosPath, LPWSTR pszNtPath)
{
	TCHAR            szDriveStr[500];
	TCHAR            szDrive[3];
	TCHAR            szDevName[100];
	INT                cchDevName;
	INT                i;
	//检查参数
	if (!pszDosPath || !pszNtPath)
		return FALSE;
	if (GetLogicalDriveStrings(sizeof(szDriveStr), szDriveStr))
	{
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
	}
	lstrcpy(pszNtPath, pszDosPath);
	return FALSE;
}
M_API BOOL MGetProcessFullPathEx(DWORD dwPID, LPWSTR outNter)
{
	if (dwPID == 0) { wcscpy_s(outNter, 260, L"处理器空闲时间百分比"); return 1; }
	else if (dwPID == 4) { wcscpy_s(outNter, 260, L"NT Kernel & System"); return 1; }

	TCHAR szResult[MAX_PATH];
	TCHAR szImagePath[MAX_PATH];
	HANDLE hProcess;

	int rs = MOpenProcessNt(dwPID, &hProcess);
	if (!hProcess || rs != 1)
		return FALSE;
	if (!K32GetProcessImageFileNameW(hProcess, szImagePath, MAX_PATH))
	{
		if (hProcess != INVALID_HANDLE_VALUE && hProcess != (HANDLE)0xCCCCCCCCL)
			CloseHandle(hProcess);
		return FALSE;
	}
	if (!MDosPathToNtPath(szImagePath, szResult))
	{
		CloseHandle(hProcess);
		return FALSE;
	}
	CloseHandle(hProcess);
	wcscpy_s(outNter, 260, szResult);
	return TRUE;
}
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
M_API int MGetExeState(DWORD pid, HWND hWnd)
{
	int rs = 1;
	ULONG n = 0x100;
	PSYSTEM_PROCESSES sp = new SYSTEM_PROCESSES[n];
	while (NtQuerySystemInformation(5, sp, n*sizeof(SYSTEM_PROCESSES), 0) == STATUS_INFO_LENGTH_MISMATCH)
	{
		delete[] sp;
		sp = new SYSTEM_PROCESSES[n = n * 2];
	}
	bool done = false;

	//遍历进程列表
	for (PSYSTEM_PROCESSES p = sp; !done;
	p = PSYSTEM_PROCESSES(PCHAR(p) + p->NextEntryDelta))
	{
		if (p->ProcessId == pid)
		{
			SYSTEM_THREADS systemThread = p->Threads[0];
			if (systemThread.ThreadState == THREAD_STATE::StateWait && systemThread.WaitReason == Suspended)
			{
				delete[] sp;
				return 2;
			}
			else
			{
				delete[] sp;
				return 1;
			}
			done = true;
		}
		done = p->NextEntryDelta == 0;
	}
	delete[] sp;
	return rs;
}
M_API ULONG MGetExeRam(DWORD pid)
{
	ULONG rs = 0;
	return rs;
}

M_API DWORD MSuspendTaskNt(DWORD dwPId)
{
	if (dwPId != 0 && dwPId != 4 && dwPId > 0) {
		HANDLE hProcess;
		DWORD rs = MOpenProcessNt(dwPId, &hProcess);
		if (hProcess) {
			rs = ZwSuspendProcess(hProcess);
			if (rs == 0)
				return TRUE;
			else return rs;
		}
		return rs;
	}
	return FALSE;
}
M_API DWORD MRusemeTaskNt(DWORD dwPId)
{
	if (dwPId != 0 && dwPId != 4 && dwPId > 0) {
		HANDLE hProcess;
		MOpenProcessNt(dwPId, &hProcess);
		if (hProcess)
		{
			DWORD rs = ZwResumeProcess(hProcess);
			if (rs == 0) return TRUE;
			else return rs;
		}
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
M_API DWORD MTerminateProcessNt(HANDLE handle)
{
	DWORD rs = ZwTerminateProcess(handle, 0);
	if (rs == 0)
		return TRUE;
	else return rs;
}
M_API bool MGetProcessCommandLine(DWORD pid, LPWSTR l, int maxcount) {
	HANDLE hproc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
	if (INVALID_HANDLE_VALUE != hproc) {
		HANDLE hnewdup = NULL;
		PEB peb;
		RTL_USER_PROCESS_PARAMETERS upps;
		WCHAR buffer[MAX_PATH] = { NULL };
		if (DuplicateHandle(GetCurrentProcess(), hproc, GetCurrentProcess(), &hnewdup, 0, FALSE, DUPLICATE_SAME_ACCESS)) {
			PROCESS_BASIC_INFORMATION pbi;
			DWORD isok = NtQueryInformationProcess(hnewdup, 0/*ProcessBasicInformation*/, (PVOID)&pbi, sizeof(PROCESS_BASIC_INFORMATION), 0);
			if ((isok)) {
				if (ReadProcessMemory(hnewdup, pbi.PebBaseAddress, &peb, sizeof(PEB), 0))
					if (ReadProcessMemory(hnewdup, peb.ProcessParameters, &upps, sizeof(RTL_USER_PROCESS_PARAMETERS), 0)) {
						WCHAR *buffer = new WCHAR[upps.CommandLine.Length + 1];
						ZeroMemory(buffer, (upps.CommandLine.Length + 1) * sizeof(WCHAR));
						ReadProcessMemory(hnewdup, upps.CommandLine.Buffer, buffer, upps.CommandLine.Length, 0);
						wcscpy_s(l, maxcount, buffer);
						delete buffer;
						return true;
					}
			}
			CloseHandle(hnewdup);
		}
		CloseHandle(hproc);
	}
	return false;
}

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
	return 0;
}

INT64 CompareFileTime(FILETIME time1, FILETIME time2)
{
	INT64 a = time1.dwHighDateTime << 32 | time1.dwLowDateTime;
	INT64 b = time2.dwHighDateTime << 32 | time2.dwLowDateTime;
	return   (b - a);
}

FILETIME m_preidleTime;
FILETIME m_prekernelTime;
FILETIME m_preuserTime;

M_API double MGetCpuUseAge()
{
	FILETIME idleTime;
	FILETIME kernelTime;
	FILETIME userTime;
	GetSystemTimes(&idleTime, &kernelTime, &userTime);

	INT64 idle = CompareFileTime(m_preidleTime, idleTime);
	INT64 kernel = CompareFileTime(m_prekernelTime, kernelTime);
	INT64 user = CompareFileTime(m_preuserTime, userTime);

	if (kernel + user == 0)
		return 0.0;
	//（总的时间-空闲时间）/总的时间=占用cpu的时间就是使用率
	double cpu = (kernel + user - idle) * 100 / (kernel + user);

	m_preidleTime = idleTime;
	m_prekernelTime = kernelTime;
	m_preuserTime = userTime;
	return cpu;
}
M_API double MGetRamUseAge()
{
	MEMORYSTATUSEX statex;
	statex.dwLength = sizeof(statex);
	GlobalMemoryStatusEx(&statex);

	double ram = ((statex.ullTotalPhys - statex.ullAvailPhys) / (double)statex.ullTotalPhys);
	return ram;
}
M_API double MGetDiskUseAge()
{
	return 0;
}
M_API double MGetInternetUseAge()
{
	return 0;
}

int cpuCount = 0;

M_API int MGetCpuCount()
{
	GetSystemTimes(&m_preidleTime, &m_prekernelTime, &m_preuserTime);
	SYSTEM_INFO info;
	GetSystemInfo(&info);
	cpuCount = info.dwNumberOfProcessors;;
	return info.dwNumberOfProcessors;
}

UINT64 file_time_2_utc(const FILETIME* ftime)
{
	LARGE_INTEGER li;
	li.LowPart = ftime->dwLowDateTime;
	li.HighPart = ftime->dwHighDateTime;
	return li.QuadPart;
}

M_API ULONG MGetAllRam()
{
	MEMORYSTATUSEX statex;
	statex.dwLength = sizeof(statex);
	GlobalMemoryStatusEx(&statex);
	return statex.ullTotalPhys / 1048576;
}
/* M_API EXEPROFENCE MGetExeProfenceInfo(DWORD dwPId, int intervalTime, UINT64 lastcputime)
{
EXEPROFENCE exe = EXEPROFENCE();
HANDLE hProcess;
hProcess = OpenProcess(PROCESS_QUERY_INFORMATION |
	PROCESS_VM_READ,
	FALSE, dwPId);
if (NULL == hProcess)
return exe;

PROCESS_MEMORY_COUNTERS pmc;
if (GetProcessMemoryInfo(hProcess, &pmc, sizeof(pmc)))
exe.ram = pmc.WorkingSetSize / 1048576;

FILETIME creation_time;
FILETIME exit_time;
FILETIME kernel_time;
FILETIME user_time;

UINT64 cpu_time;

if (!GetProcessTimes(hProcess, &creation_time, &exit_time, &kernel_time, &user_time)) {
	cpu_time = (file_time_2_utc(&kernel_time) + file_time_2_utc(&user_time))
		/ cpuCount;
	if (lastcputime != 0) {
		UINT64 this_time = cpu_time - lastcputime;
		double cpuuse = (double)(this_time / (double)(intervalTime * 2));
		exe.cpu = cpuuse < 0 ? 0 : cpuuse;
	}
	else exe.cpu = 0;
	exe.cputime = cpu_time;
}

return exe;
}*/

HINSTANCE hNtDll;

typedef struct
{
	int tk;
	DWORD pid;
	PVOID token;
}PCMGRTOKEN, *PPCMGRTOKEN;

typedef PPCMGRTOKEN(*MGetTokenFun)();
typedef DWORD(*MGetPIDFun)();

MGetPIDFun MGetPID;
MGetTokenFun MGetToken;

extern void ShowMainCoreStartUp();

void AntiTest();

BOOL LoadDll()
{
	hNtDll = LoadLibrary(L"ntdll.dll");
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

		AntiTest();

		ShowMainCoreStartUp();
		return TRUE;
	}
}
void FreeDll() {
	delete thisCommandPath;
	delete thisCommandName;
}

void Anti()
{
	MessageBox(0, L"抱歉，出现了错误。", DEFDIALOGGTITLE, MB_ICONERROR | MB_OK);
	int*p = nullptr; 
	*p = 0;
}

void AntiTest()
{
	HMODULE hMain = GetModuleHandle(NULL);
	MGetToken = (MGetTokenFun)GetProcAddress(hMain, "MGetToken");
	MGetPID = (MGetPIDFun)GetProcAddress(hMain, "MGetPID");

	PCMGRTOKEN * t = MGetToken();
	if (t->pid != MGetPID())
		Anti();
	if (t->tk != 342342 + 53672 * 56)
		Anti();
	if (wcscmp((wchar_t*)t->token, L"23RGMCP") != 0)
		Anti();
}
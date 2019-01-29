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
#include "thdhlp.h"
#include "kda.h"
#include "fmhlp.h"
#include "handlehlp.h"
#include "PathHelper.h"
#include "StringHlp.h"
#include <Psapi.h>
#include <process.h>
#include <tlhelp32.h>
#include <Shlobj.h>
#include <tchar.h>
#include <shellapi.h>
#include <wintrust.h>
#include <cryptuiapi.h>
#include <mscat.h>
#include <list>

#define ENCODING (X509_ASN_ENCODING | PKCS_7_ASN_ENCODING)

extern HINSTANCE hInstRs;
extern TerminateImporantWarnCallBack hTerminateImporantWarnCallBack;

HANDLE nextShouldTerminateThread = NULL;
LPWSTR thisCommandPath = NULL;
LPWSTR thisCommandName = NULL;
LPWSTR thisCommandUWPName = NULL;
DWORD thisCommandPid = 0;
HANDLE thisCommandhProcess = 0;
BOOL thisCommandIsImporant = FALSE;
BOOL thisCommandIsVeryImporant = FALSE;
UCHAR thisCommandProcessPriority = 0;
HICON HIconDef = NULL;
HWND hWndMain = NULL;
PSYSTEM_PROCESSES current_system_process = NULL;
CERT_CONTEXT lastVeredCertContext = { 0 };

BOOL killUWPCmdSendBack = FALSE;
BOOL killCmdSendBack = FALSE;
BOOL isKillingExplorer = FALSE;

extern bool use_apc;
extern bool can_debug;

//Api s

//shell32
_RunFileDlg RunFileDlg;
//ntdll
NtSuspendThreadFun NtSuspendThread;
NtResumeThreadFun NtResumeThread;
NtTerminateThreadFun NtTerminateThread;
NtTerminateProcessFun NtTerminateProcess;
NtOpenThreadFun NtOpenThread;
NtSuspendProcessFun NtSuspendProcess;
NtResumeProcessFun NtResumeProcess;
NtOpenProcessFun NtOpenProcess;

NtQueryInformationThreadFun NtQueryInformationThread;
NtQuerySystemInformationFun NtQuerySystemInformation;
NtQueryInformationProcessFun NtQueryInformationProcess;

NtSetInformationProcessFun NtSetInformationProcess;
NtSetInformationDebugObjectFun NtSetInformationDebugObject;
NtRemoveProcessDebugFun NtRemoveProcessDebug;


NtQueryObjectFun NtQueryObject;
NtQueryVirtualMemoryFun NtQueryVirtualMemory;
NtReadVirtualMemoryFun NtReadVirtualMemory;

NtUnmapViewOfSectionFun NtUnmapViewOfSection;
NtDuplicateObjectFun NtDuplicateObject;
NtCloseFun NtClose;

LdrGetProcedureAddressFun LdrGetProcedureAddress;
RtlInitAnsiStringFun RtlInitAnsiString;
RtlNtStatusToDosErrorFun RtlNtStatusToDosError;
RtlGetLastWin32ErrorFun RtlGetLastWin32Error;

//K32 api
_IsWow64Process dIsWow64Process;
_IsImmersiveProcess dIsImmersiveProcess;
_GetPackageFullName dGetPackageFullName;
_GetPackageInfo dGetPackageInfo;
_ClosePackageInfo dClosePackageInfo;
_OpenPackageInfoByFullName dOpenPackageInfoByFullName;
_GetPackageId dGetPackageId;
_GetModuleFileNameW dGetModuleFileNameW;
fnLoadLibraryA dLoadLibraryA;
fnLoadLibraryW dLoadLibraryW;
//
_CryptUIDlgViewCertificateW dCryptUIDlgViewCertificateW;
_CryptUIDlgViewContext dCryptUIDlgViewContext;
//u32 api
_CancelShutdown dCancelShutdown;
//
_GetPerTcpConnectionEStats dGetPerTcpConnectionEStats;
_GetPerTcp6ConnectionEStats dGetPerTcp6ConnectionEStats;
_GetExtendedTcpTable dGetExtendedTcpTable;
_SetPerTcpConnectionEStats dSetPerTcpConnectionEStats;
//imahlp api
extern fnIMAGELOAD ImageLoad;
extern fnIMAGEUNLOAD ImageUnload;
//winsta api
_WinStationConnectW WinStationConnectW;
_WinStationDisconnect WinStationDisconnect;
_WinStationReset WinStationReset;
_WinStationFreeMemory WinStationFreeMemory;
_WinStationEnumerateW WinStationEnumerateW;
_WinStationQueryInformationW WinStationQueryInformationW;
_MGetCurrentPeb dMGetCurrentPeb;

//Enum apis
extern BOOL MAppVProcessAllWindows();

BOOL CALLBACK lpEnumProcWinsFunc(HWND hWnd, LPARAM lParam)
{
	PEPW_ARG agrs = (PEPW_ARG)lParam;
	DWORD processId;
	DWORD threadId = GetWindowThreadProcessId(hWnd, &processId);
	if (processId == agrs->pid)
	{
		WCHAR windowText[128];
		GetWindowText(hWnd, windowText, 128);
		return agrs->c(hWnd, windowText, agrs->data);
	}
	return TRUE;
}

std::list<PPROCHANDLE_STORAGE> processHandleStorage;

void MFroceKillProcessUser()
{
	if (thisCommandPid > 4)
	{
		if (thisCommandIsVeryImporant && !hTerminateImporantWarnCallBack(thisCommandName, 3))return;
		if (!thisCommandIsVeryImporant && thisCommandIsImporant && !hTerminateImporantWarnCallBack(thisCommandName, 1))return;
		if ((!thisCommandIsVeryImporant && thisCommandIsImporant) || (!thisCommandIsImporant && MShowMessageDialog(hWndMain, (LPWSTR)str_item_kill_ast_content.c_str(), DEFDIALOGGTITLE,
			(LPWSTR)(str_item_kill_ask_start + L" " + thisCommandName + L" " + str_item_kill_ask_end).c_str(), NULL, MB_YESNO) == IDYES))
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
void MKillProcessUser(BOOL ask)
{
	if (thisCommandPid > 4)
	{
		if (thisCommandIsVeryImporant && !hTerminateImporantWarnCallBack(thisCommandName, 3))return;
		if (!thisCommandIsVeryImporant && thisCommandIsImporant && !hTerminateImporantWarnCallBack(thisCommandName, 1))return;
		if ((!thisCommandIsVeryImporant && thisCommandIsImporant) || !ask || isKillingExplorer || (!thisCommandIsImporant && MShowMessageDialog(hWndMain, (LPWSTR)str_item_kill_ast_content.c_str(), DEFDIALOGGTITLE,
			(LPWSTR)(str_item_kill_ask_start + L" " + thisCommandName + L" " + str_item_kill_ask_end).c_str(), NULL, MB_YESNO) == IDYES))
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
			else if (status == STATUS_INVALID_CID || status == STATUS_INVALID_HANDLE)
				MShowErrorMessage((LPWSTR)str_item_invalidproc.c_str(), (LPWSTR)str_item_kill_failed.c_str(), MB_ICONWARNING, MB_OK);
			else ThrowErrorAndErrorCodeX(status, str_item_openprocfailed, (LPWSTR)str_item_kill_failed.c_str());
		}
	}
}
void MKillProcessTreeUser()
{
	if (thisCommandPid > 4 && !isKillingExplorer)
	{
		if (thisCommandIsVeryImporant && !hTerminateImporantWarnCallBack(thisCommandName, 3))return;
		if (!thisCommandIsVeryImporant && thisCommandIsImporant && !hTerminateImporantWarnCallBack(thisCommandName, 1))return;
		if ((!thisCommandIsVeryImporant && thisCommandIsImporant) || (!thisCommandIsImporant && MShowMessageDialog(hWndMain, str_item_killtree_content, DEFDIALOGGTITLE,
			(LPWSTR)(str_item_kill_ask_start + L" " + thisCommandName + L" " + str_item_killtree_end).c_str(), NULL, MB_YESNO) == IDYES))
			MAppMainCall(M_CALLBACK_KILLPROCTREE, (LPVOID)(ULONG_PTR)thisCommandPid, 0);
	}
}
BOOL MKillProcessUser2(HWND hWnd, DWORD pid, BOOL showErr, BOOL ignoreTerminateing)
{
	if (hWnd == hWndMain && pid > 4)
	{
		HANDLE hProcess;
		NTSTATUS status = MOpenProcessNt(pid, &hProcess);
		if (status == STATUS_SUCCESS)
		{
			status = MTerminateProcessNt(pid, hProcess);
			if (status == STATUS_ACCESS_DENIED && showErr)
				MShowErrorMessage((LPWSTR)str_item_access_denied.c_str(), (LPWSTR)str_item_kill_failed.c_str(), MB_ICONERROR, MB_OK);
			else if (status == STATUS_PROCESS_IS_TERMINATING && ignoreTerminateing)
				return TRUE;
			else if (!NT_SUCCESS(status) && showErr)
				ThrowErrorAndErrorCodeX(status, str_item_endprocfailed, (LPWSTR)str_item_kill_failed.c_str());
			else return TRUE;
		}
		else if (status == STATUS_PROCESS_IS_TERMINATING && ignoreTerminateing)
			return TRUE;
		else if (status == STATUS_ACCESS_DENIED && showErr)
			MShowErrorMessage((LPWSTR)str_item_access_denied.c_str(), (LPWSTR)str_item_kill_failed.c_str(), MB_ICONERROR, MB_OK);
		else if ((status == STATUS_INVALID_CID || status == STATUS_INVALID_HANDLE) && showErr)
			MShowErrorMessage((LPWSTR)str_item_invalidproc.c_str(), (LPWSTR)str_item_kill_failed.c_str(), MB_ICONWARNING, MB_OK);
		else if (NT_SUCCESS(status))
			return TRUE;
		else if (showErr)
			ThrowErrorAndErrorCodeX(status, str_item_openprocfailed, (LPWSTR)str_item_kill_failed.c_str());
	}
	return 0;
}
BOOL MDetachFromDebuggerProcess(DWORD pid)
{
	NTSTATUS status;
	HANDLE processHandle;
	HANDLE debugObjectHandle;
	
	if (NT_SUCCESS(status = MOpenProcessNt(	pid, &processHandle)))
	{
		if (NT_SUCCESS(status = MGetProcessDebugObject(processHandle, &debugObjectHandle)))
		{
			ULONG flags;

			// Disable kill-on-close.
			flags = 0;
			NtSetInformationDebugObject(
				debugObjectHandle,
				DebugObjectFlags,
				&flags,
				sizeof(ULONG),
				NULL
			);

			status = NtRemoveProcessDebug(processHandle, debugObjectHandle);

			NtClose(debugObjectHandle);
		}

		NtClose(processHandle);
	}

	if (status == STATUS_PORT_NOT_SET)
	{
		MShowMessageDialog(hWndMain, str_item_deatch_debugger_notdebug, str_item_deatch_debugger_title, L"");
		return FALSE;
	}

	if (!NT_SUCCESS(status))
	{
		MShowErrorMessageWithNTSTATUS(str_item_deatch_debugger_err, str_item_deatch_debugger_title, status);
		return FALSE;
	}

	return TRUE;
}


DWORD WINAPI CreateMiniDumpForProcessThread(LPVOID lpParameter)
{
	HWND hWnd = (HWND)lpParameter;

	WCHAR szFullPath[MAX_PATH];
	WCHAR strBuffer[MAX_PATH];
	WCHAR strDir[MAX_PATH];
	WCHAR strFileName[MAX_PATH];
	if (NT_SUCCESS(MGetProcessImageFileNameWin32(thisCommandhProcess, strBuffer, MAX_PATH)))
	{
		std::wstring *dir = Path::GetDirectoryName(strBuffer);
		std::wstring *fname = Path::GetFileNameWithoutExtension(strBuffer);

		wcscpy_s(strDir, dir->c_str());
		wcscpy_s(strFileName, fname->c_str());

		FreeStringPtr(dir);
		FreeStringPtr(fname);
	}
	else
	{
		GetTempPath(MAX_PATH, strDir);
		swprintf_s(strFileName, L"\\MiniDumpForProcess %d .dmp", thisCommandPid);
	}

	PathYetAnotherMakeUniqueName(szFullPath, strDir, NULL, strFileName);

	HANDLE hDumpFile = CreateFile(szFullPath, GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_WRITE | FILE_SHARE_READ, 0, CREATE_ALWAYS, 0, 0);

	if (INVALID_HANDLE_VALUE != hDumpFile)
	{
		if (!MiniDumpWriteDump(thisCommandhProcess, thisCommandPid, hDumpFile, MiniDumpNormal, NULL, NULL, NULL))
			MShowErrorMessageWithLastErr(str_item_CreateDumpFailed, DEFDIALOGGTITLE, MB_ICONERROR, 0);
		else {
			SetDlgItemText(hWnd, IDC_EDIT, szFullPath);
			SetDlgItemText(hWnd, IDC_STATUS, str_item_CreateDumpSuccess);
			ShowWindow(GetDlgItem(hWnd, IDCANCEL), SW_HIDE);
			ShowWindow(GetDlgItem(hWnd, IDOK), SW_SHOW);
		}

		return MCloseHandle(hDumpFile);
	}
	else {
		SendMessage(hWnd, WM_SYSCOMMAND, SC_CLOSE, NULL);
		MShowErrorMessageWithLastErr(str_item_OpenFileError, str_item_CreateDumpFailed, MB_ICONERROR, 0);
	}

	return 0;
}
INT_PTR CALLBACK CreateMiniDumpForProcessDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_INITDIALOG: {

		break;
	}
	case WM_SHOWWINDOW: {
		if (wParam) 
			CreateThread(NULL, 0, CreateMiniDumpForProcessThread, hDlg, 0, NULL);
		break;
	}
	case WM_SYSCOMMAND: {
		if (wParam == SC_CLOSE) 
			EndDialog(hDlg, IDOK);
		return 0;
	}
	case WM_COMMAND: {
		switch (wParam)
		{
		case IDOK: SendMessage(hDlg, WM_SYSCOMMAND, SC_CLOSE, NULL); break;
		case IDCANCEL: {



			SendMessage(hDlg, WM_SYSCOMMAND, SC_CLOSE, NULL);
			break;
		}
		}
		break;
	}
	}
	return (INT_PTR)FALSE;
}

BOOL MCreateMiniDumpForProcess(DWORD pid)
{
	if (thisCommandhProcess)
		return DialogBoxW(hInstRs, MAKEINTRESOURCE(IDD_CREATEDUMP), hWndMain, CreateMiniDumpForProcessDlgProc) == 0;
	return FALSE;
}

void MProcessHANDLEStorageDestroyItem(DWORD pid) {
	PPROCHANDLE_STORAGE target = NULL;
	for (auto it = processHandleStorage.begin(); it != processHandleStorage.end(); it++)
	{
		if ((*it)->pid == pid) {
			target = *it;
			break;
		}
	}
	if (target) {
		MCloseHandle(target->hProcess);
		processHandleStorage.remove(target);
		MFree(target);
	}
}
void MProcessHANDLEStorageDestroy() {
	for (auto it = processHandleStorage.begin(); it != processHandleStorage.end(); it++)
		MFree(*it);
	processHandleStorage.clear();
}
BOOL MTryGetProcessExitStatus(HANDLE hProcess) {
	NTSTATUS exitStatus = 0;
	NTSTATUS status = MGetProcessExitStatus(hProcess, &exitStatus);
	if (NT_SUCCESS(status))
		return exitStatus == 0;
	else if (status == STATUS_INVALID_HANDLE || status == STATUS_INVALID_CID)
		return FALSE;
	else return TRUE;
}
HANDLE MTryFindOpenedProcessHANDLE(DWORD pid) {
	if(pid<=4 )	return NULL;
	PPROCHANDLE_STORAGE target = NULL;
	for (auto it = processHandleStorage.begin(); it != processHandleStorage.end(); it++)
	{
		if ((*it)->pid == pid) {
			target = *it;
			break;
		}
	}
	if (target)
		return target->hProcess;
	return NULL;
}
HANDLE MTryOpenProcess(DWORD pid) {
	if (pid <= 4)	return NULL;
	HANDLE oldHandle = MTryFindOpenedProcessHANDLE(pid);
	if (oldHandle)
		if (!MTryGetProcessExitStatus(oldHandle))
			MProcessHANDLEStorageDestroyItem(pid);
		else return oldHandle;
	HANDLE hProcess = NULL;
	NTSTATUS status = MOpenProcessNt(pid, &hProcess);
	if (NT_SUCCESS(status) && hProcess) {
		PPROCHANDLE_STORAGE item = (PPROCHANDLE_STORAGE)MAlloc(sizeof(PROCHANDLE_STORAGE));
		item->hProcess = hProcess;
		item->pid = pid;
		processHandleStorage.push_back(item);
		return hProcess;
	}
	return NULL;
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

	if (!LookupPrivilegeValue(NULL, SE_PROF_SINGLE_PROCESS_NAME, &luid))
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
M_API BOOL MUpdateProcessList()
{
	if (current_system_process) MEnumProcessFree();

	DWORD dwSize = 0;
	NTSTATUS status = NtQuerySystemInformation(SystemProcessInformation, NULL, 0, &dwSize);
	if (status == STATUS_INFO_LENGTH_MISMATCH && dwSize > 0)
	{
		current_system_process = (PSYSTEM_PROCESSES)MAlloc(dwSize);
		status = NtQuerySystemInformation(SystemProcessInformation, current_system_process, dwSize, 0);
		if (!NT_SUCCESS(status))
			SetLastError(RtlNtStatusToDosError(status));
		else return TRUE;
	}
	return FALSE;
}
M_API VOID MEnumProcessFree()
{
	if (current_system_process) {
		MFree(current_system_process);
		current_system_process = NULL;
	}
}
M_API BOOL MEnumProcess(EnumProcessCallBack calBack, LPVOID customData)
{
	if (calBack)
	{
		HANDLE hProcess = NULL;
		WCHAR exeFullPath[260];

		MAppVProcessAllWindows();
		if (MUpdateProcessList()) {
			bool done = false;
			int ix = 0;
			for (PSYSTEM_PROCESSES p = current_system_process; !done; p = PSYSTEM_PROCESSES(PCHAR(p) + p->NextEntryOffset))
			{
				memset(exeFullPath, 0, sizeof(exeFullPath));
				hProcess = NULL;
				MGetProcessFullPathEx(static_cast<DWORD>((ULONG_PTR)p->ProcessId), exeFullPath, &hProcess, p->ImageName.Buffer);
				if (!calBack(static_cast<DWORD>((ULONG_PTR)p->ProcessId), static_cast<DWORD>((ULONG_PTR)p->InheritedFromProcessId), p->ImageName.Buffer, exeFullPath, 1, hProcess, p, customData))
					done = true;
				else done = p->NextEntryOffset == 0;
				ix++;
				done = p->NextEntryOffset == 0;
			}
			calBack(ix, 0, NULL, NULL, 0, 0, NULL, customData);
			return TRUE;
		}
	}
	return FALSE;
}
M_API BOOL MEnumProcess2(EnumProcessCallBack2 callBack)
{
	if (callBack)
	{
		MAppVProcessAllWindows();
		if (MUpdateProcessList()) 
		{
			bool done = false;
			for (PSYSTEM_PROCESSES p = current_system_process; !done; p = PSYSTEM_PROCESSES(PCHAR(p) + p->NextEntryOffset))
			{
				callBack(static_cast<DWORD>((ULONG_PTR)p->ProcessId), p);
				done = p->NextEntryOffset == 0;
			}
			return TRUE;
		}
	}		
	return 0;
}
M_API BOOL MUpdateProcess(DWORD pid, EnumProcessCallBack calBack, LPVOID customData)
{
	if (!calBack)return FALSE;
	PSYSTEM_PROCESSES p = MFindProcessInLoadedProcesses(pid);
	if (p != NULL)
	{
	    HANDLE hProcess = 0;
		WCHAR exeFullPath[260];
		memset(exeFullPath, 0, sizeof(exeFullPath));
		MGetProcessFullPathEx(static_cast<DWORD>((ULONG_PTR)p->ProcessId), exeFullPath, &hProcess, p->ImageName.Buffer);
		calBack(static_cast<DWORD>((ULONG_PTR)p->ProcessId), static_cast<DWORD>((ULONG_PTR)p->InheritedFromProcessId), p->ImageName.Buffer, exeFullPath, 1, hProcess, p, customData);
		return TRUE;
	}
	return 0;
}
M_API PSYSTEM_PROCESSES MFindProcessInLoadedProcesses(DWORD pid) {
	bool done = false;
	for (PSYSTEM_PROCESSES p = current_system_process; !done; p = PSYSTEM_PROCESSES(PCHAR(p) + p->NextEntryOffset))
	{
		if (static_cast<DWORD>((ULONG_PTR)p->ProcessId) == pid)
			return p;
		done = p->NextEntryOffset == 0;
	}	
	return 0;
}
M_API BOOL MEnumProcessThreads(PSYSTEM_PROCESSES p, EnumProcessThreadsCallBack callback, LPVOID customData)
{
	if (!callback)return FALSE;
	PSYSTEM_THREADS threads = p->Threads;
	for (ULONG i = 0; i < p->NumberOfThreads; i++)
		if (!callback(static_cast<DWORD>((ULONG_PTR)threads[i].ClientId.UniqueThread), static_cast<DWORD>((ULONG_PTR)threads[i].ClientId.UniqueProcess),
			threads[i].StartAddress, &threads[i], customData)) break;
	return TRUE;
}
M_API BOOL MEnumProcessWindow(DWORD pid, EnumProcessWindowsCallBack callback, LPVOID customData)
{
	if (callback) {
		EPW_ARG agrs = { pid, callback, customData };
		return EnumWindows(lpEnumProcWinsFunc, (LPARAM)&agrs);
	}
	return FALSE;
}
M_API BOOL MEnumProcessModules(DWORD dwPID, EnumProcessModulesCallBack callback, LPVOID customData)
{
	if (callback) {
		HANDLE hProcess;
		NTSTATUS status = MOpenProcessNt(dwPID, &hProcess);
		if (status == STATUS_SUCCESS && hProcess) {

			BOOL bRet = FALSE;

			PPEB pPeb = 0;
			if (NT_SUCCESS(MGetProcessPeb(hProcess, &pPeb) && pPeb != 0))
			{
				PPEB_LDR_DATA pLdr = NULL;

				status = NtReadVirtualMemory(hProcess, PTR_ADD_OFFSET(pPeb, FIELD_OFFSET(PEB, Ldr)), &pLdr, sizeof(PPEB_LDR_DATA), NULL);
				if (!NT_SUCCESS(status)) return FALSE;

				PLIST_ENTRY list_head = (PLIST_ENTRY)PTR_ADD_OFFSET(pLdr, FIELD_OFFSET(PEB_LDR_DATA, InMemoryOrderModuleList));
				PLIST_ENTRY p = list_head;
				LIST_ENTRY thisListEntry;
				status = NtReadVirtualMemory(hProcess, list_head, &thisListEntry, sizeof(LIST_ENTRY), NULL);

				int i = 0;
				WCHAR thisName[MAX_PATH];
				for (p = thisListEntry.Flink; p != list_head; p = thisListEntry.Flink)
				{
					LDR_MODULE thisModule;
					status = NtReadVirtualMemory(hProcess, CONTAINING_RECORD(p, LDR_MODULE, InMemoryOrderModuleList), &thisModule, sizeof(LDR_MODULE), NULL);
					status = NtReadVirtualMemory(hProcess, p, &thisListEntry, sizeof(LIST_ENTRY), NULL);

					if (thisModule.BaseDllName.Buffer != NULL)
					{
						WCHAR baseDllNameBuffer[MAX_PATH] = { 0 };
						status = NtReadVirtualMemory(hProcess, thisModule.BaseDllName.Buffer, &baseDllNameBuffer, sizeof(baseDllNameBuffer), NULL);
						wcscpy_s(thisName, baseDllNameBuffer);

						WCHAR fullDllNameBuffer[MAX_PATH] = { 0 };
						if (thisModule.FullDllName.Buffer != NULL) {
							status = NtReadVirtualMemory(hProcess, thisModule.FullDllName.Buffer, &fullDllNameBuffer, sizeof(fullDllNameBuffer), NULL);
						}

						if (!callback(baseDllNameBuffer, fullDllNameBuffer, thisModule.BaseAddress, thisModule.SizeOfImage, customData))
							break;
						i++;

						if (i > 512)//Too big
							break;
					}
				}
				CloseHandle(hProcess);
				return TRUE;
			}
		}
	}
	return FALSE;
}
M_API BOOL MEnumProcessHandles(DWORD pid, EHCALLBACK callback) {
	return M_EH_EnumProcessHandles(pid, callback);
}

M_CAPI(BOOL) MEnumProcessHotKeys(DWORD pid, EnumProcessHotKeyCallBack callBack)
{
	return M_SU_GetProcessHotKeys(pid, callBack);
}
M_CAPI(BOOL) MEnumProcessTimers(DWORD pid, EnumProcessTimerCallBack callBack)
{
	return M_SU_GetProcessTimers(pid, callBack);
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
		if (StrEqual(pszFullPath, L"")) {
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
	if (hIcon == NULL) hIcon = HIconDef;
	return hIcon;
}
//PE Signature Verify
M_API LONG MVerifyEmbeddedSignature(LPCWSTR pwszSourceFile)
{
	LONG rs = 0;
	LONG lStatus;
	DWORD dwLastError;

	// Initialize the WINTRUST_FILE_INFO structure.

	WINTRUST_FILE_INFO FileData;
	memset(&FileData, 0, sizeof(FileData));
	FileData.cbStruct = sizeof(WINTRUST_FILE_INFO);
	FileData.pcwszFilePath = pwszSourceFile;
	FileData.hFile = NULL;
	FileData.pgKnownSubject = NULL;

	/*
	WVTPolicyGUID specifies the policy to apply on the file
	WINTRUST_ACTION_GENERIC_VERIFY_V2 policy checks:

	1) The certificate used to sign the file chains up to a root
	certificate located in the trusted root certificate store. This
	implies that the identity of the publisher has been verified by
	a certification authority.

	2) In cases where user interface is displayed (which this example
	does not do), WinVerifyTrust will check for whether the
	end entity certificate is stored in the trusted publisher store,
	implying that the user trusts content from this publisher.

	3) The end entity certificate has sufficient permission to sign
	code, as indicated by the presence of a code signing EKU or no
	EKU.
	*/

	GUID WVTPolicyGUID = GUID{ 0x00AAC56B, 0xCD44, 0x11d0, 0x8C, 0xC2, 0x00, 0xC0, 0x4F, 0xC2, 0x95, 0xEE };
	WINTRUST_DATA WinTrustData;

	// Initialize the WinVerifyTrust input data structure.

	// Default all fields to 0.
	memset(&WinTrustData, 0, sizeof(WinTrustData));

	WinTrustData.cbStruct = sizeof(WinTrustData);

	// Use default code signing EKU.
	WinTrustData.pPolicyCallbackData = NULL;

	// No data to pass to SIP.
	WinTrustData.pSIPClientData = NULL;

	// Disable WVT UI.
	WinTrustData.dwUIChoice = WTD_UI_NONE;

	// No revocation checking.
	WinTrustData.fdwRevocationChecks = WTD_REVOKE_NONE;

	// Verify an embedded signature on a file.
	WinTrustData.dwUnionChoice = WTD_CHOICE_FILE;

	// Verify action.
	WinTrustData.dwStateAction = WTD_STATEACTION_VERIFY;

	// Verification sets this value.
	WinTrustData.hWVTStateData = NULL;

	// Not used.
	WinTrustData.pwszURLReference = NULL;

	// This is not applicable if there is no UI because it changes 
	// the UI to accommodate running applications instead of 
	// installing applications.
	WinTrustData.dwUIContext = 0;

	// Set pFile.
	WinTrustData.pFile = &FileData;

	// WinVerifyTrust verifies signatures as specified by the GUID 
	// and Wintrust_Data.
	lStatus = WinVerifyTrust(
		NULL,
		&WVTPolicyGUID,
		&WinTrustData);

	rs = lStatus;
	switch (lStatus)
	{
	case ERROR_SUCCESS:
		/*
		Signed file:
		- Hash that represents the subject is trusted.

		- Trusted publisher without any verification errors.

		- UI was disabled in dwUIChoice. No publisher or
		time stamp chain errors.

		- UI was enabled in dwUIChoice and the user clicked
		"Yes" when asked to install and run the signed
		subject.
		*/
		wprintf_s(L"The file \"%s\" is signed and the signature "
			L"was verified.\n",
			pwszSourceFile);
		break;

	case TRUST_E_NOSIGNATURE:
		// The file was not signed or had a signature 
		// that was not valid.

		// Get the reason for no signature.
		dwLastError = GetLastError();
		if (TRUST_E_NOSIGNATURE == dwLastError ||
			TRUST_E_SUBJECT_FORM_UNKNOWN == dwLastError ||
			TRUST_E_PROVIDER_UNKNOWN == dwLastError)
		{
			// The file was not signed.
			wprintf_s(L"The file \"%s\" is not signed.\n",
				pwszSourceFile);
		}
		else
		{
			// The signature was not valid or there was an error 
			// opening the file.
			wprintf_s(L"An unknown error occurred trying to "
				L"verify the signature of the \"%s\" file.\n",
				pwszSourceFile);
		}

		break;

	case TRUST_E_EXPLICIT_DISTRUST:
		// The hash that represents the subject or the publisher 
		// is not allowed by the admin or user.
		wprintf_s(L"The signature is present, but specifically "
			L"disallowed.\n");
		break;

	case TRUST_E_SUBJECT_NOT_TRUSTED:
		// The user clicked "No" when asked to install and run.
		wprintf_s(L"The signature is present, but not "
			L"trusted.\n");
		break;

	case CRYPT_E_SECURITY_SETTINGS:
		/*
		The hash that represents the subject or the publisher
		was not explicitly trusted by the admin and the
		admin policy has disabled user trust. No signature,
		publisher or time stamp errors.
		*/
		wprintf_s(L"CRYPT_E_SECURITY_SETTINGS - The hash "
			L"representing the subject or the publisher wasn't "
			L"explicitly trusted by the admin and admin policy "
			L"has disabled user trust. No signature, publisher "
			L"or timestamp errors.\n");
		break;

	default:
		// The UI was disabled in dwUIChoice or the admin policy 
		// has disabled user trust. lStatus contains the 
		// publisher or time stamp chain error.
		wprintf_s(L"Error is: 0x%x.\n",
			lStatus);
		break;
	}

	// Any hWVTStateData must be released by a call with close.
	WinTrustData.dwStateAction = WTD_STATEACTION_CLOSE;

	lStatus = WinVerifyTrust(
		NULL,
		&WVTPolicyGUID,
		&WinTrustData);

	return rs;
}
M_API BOOL MShowExeFileSignatureInfo(LPCWSTR pwszSourceFile)
{
	CERT_INFO CertInfo;
	DWORD dwEncoding, dwContentType, dwFormatType;
	HCERTSTORE hStore = NULL;
	HCRYPTMSG hMsg = NULL;
	PCMSG_SIGNER_INFO pSignerInfo = NULL;
	DWORD dwSignerInfo;
	PCCERT_CONTEXT pCertContext = NULL;

	BOOL fResult = CryptQueryObject(CERT_QUERY_OBJECT_FILE,
		pwszSourceFile,
		CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED, CERT_QUERY_FORMAT_FLAG_BINARY,
		0, &dwEncoding, &dwContentType, &dwFormatType, &hStore, &hMsg, NULL);
	if (!fResult) goto RETURN;

	fResult = CryptMsgGetParam(hMsg,
		CMSG_SIGNER_INFO_PARAM,
		0,
		NULL,
		&dwSignerInfo);
	if (!fResult) goto RETURN;
	pSignerInfo = (PCMSG_SIGNER_INFO)MAlloc(dwSignerInfo);
	fResult = CryptMsgGetParam(hMsg,
		CMSG_SIGNER_INFO_PARAM,
		0,
		(PVOID)pSignerInfo,
		&dwSignerInfo);
	if (!fResult) goto RETURN;

	CertInfo.Issuer = pSignerInfo->Issuer;
	CertInfo.SerialNumber = pSignerInfo->SerialNumber;
	pCertContext = CertFindCertificateInStore(hStore,
		ENCODING,
		0,
		CERT_FIND_SUBJECT_CERT,
		(PVOID)&CertInfo,
		NULL);
	if (pCertContext)
	{
		/*
		CRYPTUI_VIEWCERTIFICATE_STRUCT cvs = { 0 };
		cvs.dwSize = sizeof(CRYPTUI_VIEWCERTIFICATE_STRUCT);
		cvs.hwndParent = hWndMain;
		cvs.dwFlags = CRYPTUI_DISABLE_EDITPROPERTIES | CRYPTUI_DISABLE_ADDTOSTORE 
			| CRYPTUI_DISABLE_EXPORT | CRYPTUI_IGNORE_UNTRUSTED_ROOT | CRYPTUI_ONLY_OPEN_ROOT_STORE;
		cvs.pCertContext = pCertContext;
		fResult = dCryptUIDlgViewCertificateW(&cvs, NULL);
		*/
		fResult = dCryptUIDlgViewContext(CERT_STORE_CERTIFICATE_CONTEXT, pCertContext, hWndMain, NULL, 0, 0);
		CertFreeCertificateContext(pCertContext);
	}
	else LogErr(_T("CertFindCertificateInStore failed with %x\n"),	GetLastError());
RETURN:
	if (pSignerInfo)MFree(pSignerInfo);
	if (hStore != NULL) CertCloseStore(hStore, 0);
	if (hMsg != NULL) CryptMsgClose(hMsg);
	return fResult;
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

	BYTE  u8_Buffer[512];
	DWORD u32_ReqLength = 0;

	UNICODE_STRING* pk_Info = &((OBJECT_NAME_INFORMATION*)u8_Buffer)->Name;
	pk_Info->Buffer = 0;
	pk_Info->Length = 0;

	// IMPORTANT: The return value from NtQueryObject is bullshit! (driver bug?)
	// - The function may return STATUS_NOT_SUPPORTED although it has successfully written to the buffer.
	// - The function returns STATUS_SUCCESS although h_File == 0xFFFFFFFF
	NtQueryObject(hFile, ObjectNameInformation, u8_Buffer, sizeof(u8_Buffer), &u32_ReqLength);

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

	//TCHAR szResult[MAX_PATH];
	TCHAR szImagePath[MAX_PATH];
	HANDLE hProcess = MTryOpenProcess(dwPID);
	if (!hProcess) return FALSE;
	if (!NT_SUCCESS(MGetProcessImageFileNameWin32(hProcess, szImagePath, MAX_PATH)))
		return FALSE;
	//if (!MDosPathToNtPath(szImagePath, szResult)) return FALSE;
	//wcscpy_s(outNter, 260, szResult);
	wcscpy_s(outNter, 260, szImagePath);

	if (phandle)*phandle = hProcess;
	else MCloseHandle(hProcess);
	return TRUE;
}
M_API int MGetProcessState(PMPROCESS_ITEM processItem, HWND hWnd)
{
	bool done = false;
	if (processItem && processItem->Data)
	{
		if(IsWindow(hWnd) && IsHungAppWindow(hWnd))
			return 3;
		__try {
			SYSTEM_THREADS systemThread = processItem->Data->Threads[0];
			if (systemThread.ThreadState == THREAD_STATE::StateWait && systemThread.WaitReason == Suspended)
				return 2;
			else return 1;
		}
		__except (EXCEPTION_EXECUTE_HANDLER) {
			return -1;
		}
	}	
	return 0;
}
M_API PSYSTEM_THREADS MGetProcessThreads(DWORD pid)
{

	PSYSTEM_PROCESSES p = MFindProcessInLoadedProcesses(pid);
	if (p != NULL)
		return p->Threads;
	return 0;
}
M_API PSYSTEM_PROCESSES MGetProcessInfo(DWORD pid)
{
	bool done = false;
	//遍历进程列表
	for (PSYSTEM_PROCESSES p = current_system_process; !done;
		p = PSYSTEM_PROCESSES(PCHAR(p) + p->NextEntryOffset))
	{
		if (static_cast<DWORD>((ULONG_PTR)p->ProcessId) == pid)
			return p;
		done = p->NextEntryOffset == 0;
	}
	return 0;
}
M_API BOOL MGetProcessCommandLine(HANDLE handle, LPWSTR l, int maxcount, DWORD pid) {
	if (handle && l) {
		DWORD id = GetProcessId(handle);
		if (id != 0 && id != 4) {
			PUNICODE_STRING commandLine;
			NTSTATUS status = MQueryProcessVariableSize(handle, ProcessCommandLineInformation, (PVOID*)&commandLine);
			if (NT_SUCCESS(status)) {
				wcscpy_s(l, maxcount, commandLine->Buffer);
				MFree(commandLine);
				return TRUE;
			}
		}
		else {
			wcscpy_s(l, maxcount, L"");
			return TRUE;
		}
	}
	else if (l && pid >4)
		return M_SU_GetProcessCommandLine(pid, l);
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
M_API DWORD MGetProcessGdiHandleCount(HANDLE handle) {
	return GetGuiResources(handle, GR_GDIOBJECTS);
}
M_API DWORD MGetProcessUserHandleCount(HANDLE handle) {
	return GetGuiResources(handle, GR_USEROBJECTS);
}
M_API BOOL MGetProcessEprocess(DWORD pid, PPEOCESSKINFO info)
{
	ULONG_PTR outEprocess = 0;
	ULONG_PTR outPeb = 0;
	ULONG_PTR outJob = 0;
	WCHAR imageName[MAX_PATH];
	memset(imageName, 0, sizeof(imageName));
	WCHAR krnpath[MAX_PATH];
	memset(krnpath, 0, sizeof(krnpath));
	WCHAR ntpath[MAX_PATH];
	memset(ntpath, 0, sizeof(ntpath));

	if (M_SU_GetEPROCESS(pid, &outEprocess, &outPeb, &outJob, imageName, krnpath))
	{
#ifdef _X64_
		swprintf_s(info->Eprocess, L"0x%I64X", outEprocess);
		swprintf_s(info->PebAddress, L"0x%I64X", outPeb);
		swprintf_s(info->JobAddress, L"0x%I64X", outJob);
#else
		swprintf_s(info->Eprocess, L"0x%08X", outEprocess);
		swprintf_s(info->PebAddress, L"0x%08X", outPeb);
		swprintf_s(info->JobAddress, L"0x%08X", outJob);
#endif
		wcscpy_s(info->ImageFileName, imageName);
		if (!MDosPathToNtPath(krnpath, ntpath))
			wcscpy_s(info->ImageFullName, krnpath);
		else wcscpy_s(info->ImageFullName, ntpath);
		return TRUE;
}
	return FALSE;
}
M_API ULONG_PTR MGetProcessWorkingSetPrivate(HANDLE hProcess, SIZE_T pageSize)
{
	NTSTATUS status;
	PMEMORY_WORKING_SET_INFORMATION buffer;
	SIZE_T bufferSize;

	bufferSize = sizeof(MEMORY_WORKING_SET_INFORMATION);
	buffer = (PMEMORY_WORKING_SET_INFORMATION)MAlloc(bufferSize);
	memset(buffer, 0, bufferSize);

	status = NtQueryVirtualMemory(hProcess, NULL, MemoryWorkingSetInformation, buffer, bufferSize, NULL);
	if (status == STATUS_INFO_LENGTH_MISMATCH)
	{
		bufferSize = sizeof(buffer->NumberOfEntries) + buffer->NumberOfEntries * sizeof(MEMORY_WORKING_SET_BLOCK);
		MFree(buffer);
		buffer = (PMEMORY_WORKING_SET_INFORMATION)MAlloc(bufferSize);

		status = NtQueryVirtualMemory(hProcess, NULL, MemoryWorkingSetInformation, buffer, bufferSize, NULL);
		if (NT_SUCCESS(status))
		{
			SIZE_T workSetPrivate = 0;
			for (ULONG_PTR i = 0; i < buffer->NumberOfEntries; ++i)
				if (!buffer->WorkingSetInfo[i].Shared) workSetPrivate++;

			workSetPrivate *= pageSize;
			MFree(buffer);
			return workSetPrivate;
		}
		MFree(buffer);
	}
	return 0;
}
M_API DWORD MGetProcessSessionID(PMPROCESS_ITEM processItem)
{
	if (processItem && processItem->Data) return processItem->Data->SessionId;
	return 0;
}
M_API BOOL MGetProcessUserName(HANDLE hProcess, LPWSTR buffer, int maxcount)
{
	if (!buffer) return FALSE;

	BOOL result = FALSE;
	HANDLE hToken;
	if (!OpenProcessToken(hProcess, TOKEN_QUERY, &hToken))
		return FALSE;

	PTOKEN_USER pTokenUser = NULL;
	DWORD dwSize = 0;

	if (!GetTokenInformation(hToken, TokenUser, pTokenUser, dwSize, &dwSize))
	{
		if (GetLastError() != ERROR_INSUFFICIENT_BUFFER)
			return FALSE;
	}

	pTokenUser = NULL;
	pTokenUser = (PTOKEN_USER)MAlloc(dwSize);
	if (pTokenUser == NULL)
		result = FALSE;

	if (!GetTokenInformation(hToken, TokenUser, pTokenUser, dwSize, &dwSize)) {
		if (pTokenUser) MFree(pTokenUser);
		return FALSE;
	}

	PUSERNAME userName = MGetUserNameBySID(pTokenUser->User.Sid);
	if (userName) {
		wcscpy_s(buffer, maxcount, userName->UserName);
		result = TRUE;
	}

	if (pTokenUser) MFree(pTokenUser);

	return result;
}
M_API ULONG MGetProcessThreadsCount(PMPROCESS_ITEM processItem)
{
	if (processItem && processItem->Data) return processItem->Data->NumberOfThreads;
	return 0;
}
M_API ULONG MGetProcessHandlesCount(PMPROCESS_ITEM processItem)
{
	if (processItem && processItem->Data) return processItem->Data->HandleCount;
	return 0;
}
M_API DWORD MGetProcessSessionID(DWORD pid)
{
	DWORD sid = 0;
	ProcessIdToSessionId(pid, &sid);
	return sid;
}
M_API NTSTATUS MGetProcessMappedFileName(HANDLE ProcessHandle, PVOID BaseAddress, LPWSTR OutFileName, int BufferSize)
{
	NTSTATUS status;
	PVOID buffer;
	SIZE_T bufferSize;
	SIZE_T returnLength;
	PUNICODE_STRING unicodeString;

	bufferSize = 0x100;
	buffer = MAlloc(bufferSize);

	status = NtQueryVirtualMemory(
		ProcessHandle,
		BaseAddress,
		MemoryMappedFilenameInformation,
		buffer,
		bufferSize,
		&returnLength
	);

	if (status == STATUS_BUFFER_OVERFLOW)
	{
		MFree(buffer);
		bufferSize = returnLength;
		buffer = MAlloc(bufferSize);

		status = NtQueryVirtualMemory(
			ProcessHandle,
			BaseAddress,
			MemoryMappedFilenameInformation,
			buffer,
			bufferSize,
			&returnLength
		);
	}

	if (!NT_SUCCESS(status))
	{
		MFree(buffer);
		return status;
	}

	unicodeString = (PUNICODE_STRING)buffer;

	wcscpy_s(OutFileName, BufferSize, unicodeString->Buffer);

	MFree(buffer);
	return status;
}
M_API NTSTATUS MGetProcessPeb(HANDLE ProcessHandle, PPEB* pPpeb) {
	ULONG outLength = 0;
	PROCESS_BASIC_INFORMATION basicInfo = { 0 };
	NTSTATUS status = MGetProcessBasicInformation(ProcessHandle, &basicInfo);
	if (NT_SUCCESS(status))
		if(pPpeb) *pPpeb = basicInfo.PebBaseAddress;
	return status;
}
M_API NTSTATUS MGetProcessImageFileNameWin32(HANDLE ProcessHandle,	LPWSTR FileNameBuffer, ULONG FileNameBufferSize)
{
	NTSTATUS status;
	PUNICODE_STRING fileName;

	status = MQueryProcessVariableSize(ProcessHandle, ProcessImageFileNameWin32, (PVOID*)&fileName);

	if (!NT_SUCCESS(status))
		return status;

	wcscpy_s(FileNameBuffer, FileNameBufferSize, fileName->Buffer);

	MFree(fileName);

	return status;
}
M_API NTSTATUS MGetProcessImageFileName(HANDLE ProcessHandle, LPWSTR FileNameBuffer, ULONG FileNameBufferSize)
{
	NTSTATUS status;
	PUNICODE_STRING fileName;

	status = MQueryProcessVariableSize(
		ProcessHandle,
		ProcessImageFileName,
		(PVOID*)&fileName
	);
	if (!NT_SUCCESS(status))
		return status;
	wcscpy_s(FileNameBuffer, FileNameBufferSize, fileName->Buffer);
	MFree(fileName);
	return status;
}
M_API NTSTATUS MGetProcessBasicInformation(HANDLE ProcessHandle, PPROCESS_BASIC_INFORMATION BasicInformation)
{
	return NtQueryInformationProcess(ProcessHandle, ProcessBasicInformation, BasicInformation, sizeof(PROCESS_BASIC_INFORMATION), NULL);
}
M_API NTSTATUS MGetProcessExecuteFlags(HANDLE ProcessHandle, PULONG ExecuteFlags)
{
	return NtQueryInformationProcess(ProcessHandle, ProcessExecuteFlags, ExecuteFlags, sizeof(ULONG), NULL);
}
M_API NTSTATUS MGetProcessIoPriority(HANDLE ProcessHandle, IO_PRIORITY_HINT *IoPriority)
{
	return NtQueryInformationProcess(
		ProcessHandle,
		ProcessIoPriority,
		IoPriority,
		sizeof(IO_PRIORITY_HINT),
		NULL
	);
}
M_API NTSTATUS MGetProcessExitStatus(HANDLE ProcessHandle, NTSTATUS *ExitStatus) {
	ULONG outLength = 0;
	PROCESS_BASIC_INFORMATION basicInfo = { 0 };
	NTSTATUS status = MGetProcessBasicInformation(ProcessHandle, &basicInfo);
	if (NT_SUCCESS(status))
		if (ExitStatus) *ExitStatus = basicInfo.ExitStatus;
	return status;
}
M_API NTSTATUS MGetProcessBasePriority(HANDLE ProcessHandle, PULONG BasePriority)
{
	return NtQueryInformationProcess(
		ProcessHandle,
		ProcessBasePriority,
		BasePriority,
		sizeof(ULONG),
		NULL
	);
}
M_API NTSTATUS MGetProcessAffinityMask(HANDLE ProcessHandle, PULONG_PTR AffinityMask)
{
	NTSTATUS status;
	PROCESS_BASIC_INFORMATION pbi;
	status = MGetProcessBasicInformation(ProcessHandle, &pbi);
	if (NT_SUCCESS(status))
	{
		if (AffinityMask)
			*AffinityMask = pbi.AffinityMask;
	}
	return status;
}
M_API NTSTATUS MGetProcessDebugObject(HANDLE ProcessHandle, PHANDLE DebugObjectHandle)
{
	return NtQueryInformationProcess(
		ProcessHandle,
		ProcessDebugObjectHandle,
		DebugObjectHandle,
		sizeof(HANDLE),
		NULL
	);
}
//Set Process information 
NTSTATUS MSetProcessPriorityClass(HANDLE ProcessHandle, UCHAR PriorityClass)
{
	PROCESS_PRIORITY_CLASS priorityClass;
	priorityClass.Foreground = FALSE;
	priorityClass.PriorityClass = PriorityClass;
	return NtSetInformationProcess(ProcessHandle, ProcessPriorityClass, &priorityClass, sizeof(PROCESS_PRIORITY_CLASS));
}
NTSTATUS MSetProcessAffinityMask(HANDLE ProcessHandle,	ULONG_PTR AffinityMask)
{
	return NtSetInformationProcess(
		ProcessHandle,
		ProcessAffinityMask,
		&AffinityMask,
		sizeof(ULONG_PTR)
	);
}

//User names
std::list<PUSERNAME> allUserNames;
//User name

VOID MUserNameSIDsDeleteAll() {
	for (auto it = allUserNames.begin(); it != allUserNames.end(); it++)
		MFree(*it);
	allUserNames.clear();
}
PUSERNAME MFindUserNameBySID(PSID sid) {
	if (sid)
	{
		for (auto it = allUserNames.begin(); it != allUserNames.end(); it++)
		{
			if ((*it)->Sid == sid)
				return *it;
		}
	}
	return NULL;
}
M_API PUSERNAME MGetUserNameBySID(PSID sid) {
	if (sid)
	{
		PUSERNAME addedItem = MFindUserNameBySID(sid);
		if (addedItem)return addedItem;
		else {
			DWORD dwNameSize = 64;
			DWORD dwDomainSize = 128;
			SID_NAME_USE SNU;

			addedItem = (PUSERNAME)MAlloc(sizeof(USERNAME));
			addedItem->Sid = sid;
			if (LookupAccountSid(NULL, sid, addedItem->UserName, &dwNameSize, addedItem->DomainName, &dwDomainSize, &SNU) != 0)
				allUserNames.push_back(addedItem);
			else {
				MFree(addedItem);
				addedItem = nullptr;
			}
			return addedItem;
		}
	}
	return NULL;
}

M_API PPEB MGetCurrentPeb()
{
	if (dMGetCurrentPeb)
		return (PPEB)dMGetCurrentPeb();
	else {
		PPEB pPeb = NULL;
		HANDLE hProcess = GetCurrentProcess();
		MGetProcessPeb(hProcess, &pPeb);
		return pPeb;
	}
}
M_API PTEB MGetCurrentTeb()
{
	PTEB pTeb = NULL;
	HANDLE hThread = GetCurrentThread();
	MGetThreadTeb(hThread, &pTeb);
	return pTeb;
}

//UWP
/*M_API BOOL MGetUWPPackageId(HANDLE handle, MPerfAndProcessData*data)
{
	if (handle && data)
	{
		UINT32 bufferLength = 0;
		LONG result = dGetPackageId(handle, &bufferLength, nullptr);
		BYTE* buffer = (PBYTE)MAlloc(bufferLength);
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
M_API BOOL MUnInstallUWPApp(LPWSTR name)
{
	return FALSE;
}

//..
M_API BOOL MCloseHandle(HANDLE handle)
{
	return NT_SUCCESS(NtClose(handle));
}
//Process Control
M_API NTSTATUS MSuspendProcessNt(DWORD dwPId, HANDLE handle)
{
	if (dwPId != 0 && dwPId != 4 && dwPId > 0) {
		HANDLE hProcess;
		NTSTATUS rs = MOpenProcessNt(dwPId, &hProcess);
		if (rs == STATUS_SUCCESS) {
			if (hProcess) {
				rs = NtSuspendProcess(hProcess);
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
					M_SU_SuspendProcess(dwPId, &rs);
					if (rs != STATUS_SUCCESS)
						LogErr(L"SuspendProcess failed in kernel : (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
				}
				else LogErr(L"SuspendProcess failed in OpenProcess : (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
			}
		}
		else if (rs == STATUS_ACCESS_DENIED && MCanUseKernel()) {
			LogWarn(L"SuspendProcess failed in OpenProcess : (PID : %d) , Use kernel mode to Suspend it.");
			M_SU_SuspendProcess(dwPId, &rs);
			if (rs != STATUS_SUCCESS)
				LogErr(L"SuspendProcess failed in kernel : (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
		}
		else LogErr(L"SuspendProcess failed in OpenProcess : (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
		return rs;
	}
	else if (handle)
	{
		NTSTATUS  rs = NtSuspendProcess(handle);
		if (rs == 0) return STATUS_SUCCESS;
		else {
			LogErr(L"SuspendProcess failed NTSTATUS : 0x%08X", rs);
			return rs;
		}
	}
	return STATUS_UNSUCCESSFUL;
}
M_API NTSTATUS MResumeProcessNt(DWORD dwPId, HANDLE handle)
{
	if (dwPId != 0 && dwPId != 4 && dwPId > 0) {
		HANDLE hProcess;
		NTSTATUS rs = MOpenProcessNt(dwPId, &hProcess);
		if (rs == STATUS_SUCCESS) {
			if (hProcess)
			{
				rs = NtResumeProcess(hProcess);
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
					M_SU_ResumeProcess(dwPId, &rs);
					if (rs != STATUS_SUCCESS)
						LogErr(L"RusemeProcess failed in kernel : (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
				}
				else LogErr(L"RusemeProcess failed in OpenProcess : (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
			}
		}
		else if (rs == STATUS_ACCESS_DENIED && MCanUseKernel()) {
			LogWarn(L"RusemeProcess failed in OpenProcess : (PID : %d) , Use kernel mode to Ruseme it.");
			M_SU_ResumeProcess(dwPId, &rs);
			if (rs != STATUS_SUCCESS)
				LogErr(L"RusemeProcess failed in kernel : (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
		}
		else LogErr(L"RusemeProcess failed in OpenProcess : (PID : %d) NTSTATUS : 0x%08X", dwPId, rs);
		return rs;
	}
	else if (handle)
	{
		NTSTATUS rs = NtResumeProcess(handle);
		if (rs == 0)return STATUS_SUCCESS;
		else
		{
			LogErr(L"RusemeProcess failed NTSTATUS : 0x%08X", rs);
			return rs;
		}
	}
	return STATUS_UNSUCCESSFUL;
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

	NTSTATUS NtStatus = NtOpenProcess(
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
		NTSTATUS rs = NtTerminateProcess(handle, 0);
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
				rs = NtTerminateProcess(hProcess, 0);
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
M_API BOOL MRunUWPApp(LPWSTR packageName, LPWSTR name)
{
	std::wstring cmdline = FormatString(L"shell:AppsFolder\\%s!%s", packageName, name);
	return ShellExecute(hWndMain, L"open", L"explorer.exe", (LPWSTR)cmdline.c_str(), NULL, 5) > (HINSTANCE)32;
}

//Process Privileges
M_CAPI(BOOL) MEnumProcessPrivileges(DWORD dwId, EnumPrivilegesCallBack callBack)
{
	NTSTATUS status;
	HANDLE hProcess;
	HANDLE hToken;
	if (M_SU_OpenProcess(dwId, &hProcess, &status) && status == STATUS_SUCCESS)
	{
		PTOKEN_PRIVILEGES pTp = NULL;
		DWORD dwNeededSize = 0, dwI = 0;

		if (!OpenProcessToken(hProcess, TOKEN_ALL_ACCESS, &hToken))
		{
			LogErr(L"OpenProcessToken failed pid : %d Error : %d", dwId, GetLastError());
			return 0;
		}
		// 试探一下需要分配多少内存
		GetTokenInformation(hToken, TokenPrivileges, NULL, dwNeededSize, &dwNeededSize);
		// 分配所需内存大小
		pTp = (PTOKEN_PRIVILEGES)MAlloc(dwNeededSize);
		if (!GetTokenInformation(hToken, TokenPrivileges, pTp, dwNeededSize, &dwNeededSize))
		{
			MFree(pTp);
			LogErr(L"GetTokenInformation failed pid : %d Error : %d", dwId, GetLastError());
			return 0;
		}
		else
		{
			/////////////////////////////////////////////////////////
			// 枚举进程权限
			/////////////////////////////////////////////////////////
			for (DWORD i = 0; i < pTp->PrivilegeCount; i++)
			{
				WCHAR *pUidName = NULL;    // 存权限名的指针
				DWORD dwNameLen = 0;    // 权限名字长度
				LookupPrivilegeName(NULL, &pTp->Privileges[i].Luid, NULL, &dwNameLen);
				// 分配需要的内存
				pUidName = (WCHAR *)MAlloc(dwNameLen*sizeof(WCHAR));
				// 获取权限名
				LookupPrivilegeName(NULL, &pTp->Privileges[i].Luid, pUidName, &dwNameLen);
				// 如果该权限是启用状态就记录
				if (pTp->Privileges[i].Attributes == SE_PRIVILEGE_ENABLED)
				{
					callBack(pUidName);
				}
				MFree(pUidName);
			}
		}
		MFree(pTp);
		CloseHandle(hToken);
	}
	return FALSE;
}

extern HWND selectItem4;

void MSetProcessPriorityClassMenuHandler(UCHAR PriorityClass) 
{
	std::wstring str = FormatString(str_item_ChangePriorityAsk, thisCommandName);
	if (MShowMessageDialog(hWndMain, str_item_ChangePriorityContent, DEFDIALOGGTITLE, (LPWSTR)str.c_str(), 0, MB_YESNO) == IDYES)
	{
		NTSTATUS status = MSetProcessPriorityClass(thisCommandhProcess, PriorityClass);
		if (!NT_SUCCESS(status))
			MShowErrorMessageWithNTSTATUS(str_item_set_proc_priority_failed, DEFDIALOGGTITLE, status);
	}
}
void MAppProcPropertyClassHandleWmCommand(WPARAM wParam)
{
	switch (wParam)
	{
	case ID_SETPRIORTY_REALTIME: {
		if (thisCommandProcessPriority != PROCESS_PRIORITY_CLASS_REALTIME)
			MSetProcessPriorityClassMenuHandler(PROCESS_PRIORITY_CLASS_REALTIME);
		break;
	}
	case ID_SETPRIORTY_HIGH: {
		if (thisCommandProcessPriority != PROCESS_PRIORITY_CLASS_HIGH)
			MSetProcessPriorityClassMenuHandler(PROCESS_PRIORITY_CLASS_HIGH);
		break;
	}
	case ID_SETPRIORTY_ABOVENORMAL: {
		if (thisCommandProcessPriority != PROCESS_PRIORITY_CLASS_ABOVE_NORMAL)
			MSetProcessPriorityClassMenuHandler(PROCESS_PRIORITY_CLASS_ABOVE_NORMAL);
		break;
	}
	case ID_SETPRIORTY_NORMAL: {
		if (thisCommandProcessPriority != PROCESS_PRIORITY_CLASS_NORMAL)
			MSetProcessPriorityClassMenuHandler(PROCESS_PRIORITY_CLASS_NORMAL);
		break;
	}
	case ID_SETPRIORTY_BELOWNORMAL: {
		if (thisCommandProcessPriority != PROCESS_PRIORITY_CLASS_BELOW_NORMAL)
			MSetProcessPriorityClassMenuHandler(PROCESS_PRIORITY_CLASS_BELOW_NORMAL);
		break;
	}
	case ID_SETPRIORTY_LOW: {
		MSetProcessPriorityClassMenuHandler(PROCESS_PRIORITY_CLASS_IDLE);
		break;
	}
	default:
		break;
	}
}

//MENU
int MAppWorkShowMenuProcessPrepare(LPWSTR strFilePath, LPWSTR strFileName, DWORD pid, BOOL isImporant, BOOL isVeryImporant)
{
	thisCommandIsVeryImporant = isVeryImporant;
	thisCommandIsImporant = isImporant;
	thisCommandPid = pid;
	if (pid > 0)
	{
		if (!StrEqual(strFilePath, L"") && wcslen(strFilePath) < 260) {
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
int MAppWorkShowMenuProcess(LPWSTR strFilePath, LPWSTR strFileName, DWORD pid, HWND hDlg, HWND selectHWND, int data, int type, int x, int y)
{
	if (pid == 2) pid = 0;

	thisCommandPid = pid;

	HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUTASK));
	int addMenu = 0;

	if (!hroot) return 0;
	if (pid > 0)
	{
		HMENU hpop = GetSubMenu(hroot, 0);

		POINT pt;
		if (x == 0 && y == 0) GetCursorPos(&pt);
		else {
			pt.x = x;
			pt.y = y;
		}

		if (pid == 4) {
			EnableMenuItem(hpop, IDM_SUPROC, MF_DISABLED);
			EnableMenuItem(hpop, IDM_RESPROC, MF_DISABLED);
			EnableMenuItem(hpop, IDM_KILLKERNEL, MF_DISABLED);
			EnableMenuItem(hpop, IDM_KILLPROCTREE, MF_DISABLED);
			EnableMenuItem(hpop, IDM_VTHREAD, MF_DISABLED);
			EnableMenuItem(hpop, IDM_VHANDLES, MF_DISABLED);
			EnableMenuItem(hpop, IDM_VMODULS, MF_DISABLED);
			EnableMenuItem(hpop, IDM_VWINS, MF_DISABLED);
			EnableMenuItem(hpop, IDM_KILL, MF_DISABLED);
			EnableMenuItem(hpop, IDM_SGINED, MF_DISABLED);
			EnableMenuItem(hpop, IDM_VKSTRUCTS, MF_DISABLED);
			EnableMenuItem(hpop, IDM_VHOTKEY, MF_DISABLED);
			EnableMenuItem(hpop, IDM_VTIMER, MF_DISABLED);
			EnableMenuItem(hpop, IDM_DEBUG, MF_DISABLED);

			EnableMenuItem(hpop, 9 + addMenu, MF_BYPOSITION | MF_DISABLED);
			EnableMenuItem(hpop, 10 + addMenu, MF_BYPOSITION | MF_DISABLED);
			EnableMenuItem(hpop, 11 + addMenu, MF_BYPOSITION | MF_DISABLED);
		}
		if (!strFilePath || StrEqual(strFilePath, L"")) {
			EnableMenuItem(hpop, IDM_OPENPATH, MF_DISABLED);
			EnableMenuItem(hpop, IDM_FILEPROP, MF_DISABLED);
		}
		else if (wcslen(strFilePath) < 260) 
		{
			wcscpy_s(thisCommandPath, 260, strFilePath);
			EnableMenuItem(hpop, IDM_OPENPATH, MF_ENABLED);
			EnableMenuItem(hpop, IDM_FILEPROP, MF_ENABLED);
		}
		if (wcslen(strFileName) < 260)
			wcscpy_s(thisCommandName, 260, strFileName);

		if (!can_debug)EnableMenuItem(hpop, IDM_DEBUG, MF_DISABLED);

		if (selectHWND)
		{
			selectItem4 = selectHWND;
			InsertMenu(hpop, 1, MF_BYPOSITION, IDM_SETTO, str_item_set_to);
			addMenu++;
		}

		killUWPCmdSendBack = type == 3;
		killCmdSendBack = type == 2;
		isKillingExplorer = type == 1;

		if (type == 1) {
			MENUITEMINFO info = MENUITEMINFO();
			info.cbSize = sizeof(MENUITEMINFO);
			info.fMask = MIIM_STRING;
			info.dwTypeData = str_item_rebootexplorer;
			SetMenuItemInfo(hpop, IDM_KILL, FALSE, &info);
		}
		else if (type == 2 || type == 3) {
			MENUITEMINFO info = MENUITEMINFO();
			info.cbSize = sizeof(MENUITEMINFO);
			info.fMask = MIIM_STRING;
			info.dwTypeData = str_item_endtask;
			SetMenuItemInfo(hpop, IDM_KILL, FALSE, &info);
			if (type == 3) {
				wcscpy_s(thisCommandUWPName, 260, strFileName);

				EnableMenuItem(hpop, IDM_SUPROC, MF_DISABLED);
				EnableMenuItem(hpop, IDM_RESPROC, MF_DISABLED);
				EnableMenuItem(hpop, IDM_KILLKERNEL, MF_DISABLED);
				EnableMenuItem(hpop, IDM_KILLPROCTREE, MF_DISABLED);
				EnableMenuItem(hpop, IDM_OPENPATH, MF_DISABLED);
				EnableMenuItem(hpop, IDM_VTHREAD, MF_DISABLED);
				EnableMenuItem(hpop, IDM_VHANDLES, MF_DISABLED);
				EnableMenuItem(hpop, IDM_VMODULS, MF_DISABLED);
				EnableMenuItem(hpop, IDM_VWINS, MF_DISABLED);
				EnableMenuItem(hpop, IDM_SGINED, MF_DISABLED);
				EnableMenuItem(hpop, IDM_VKSTRUCTS, MF_DISABLED);
				EnableMenuItem(hpop, IDM_VHOTKEY, MF_DISABLED);
				EnableMenuItem(hpop, IDM_VTIMER, MF_DISABLED);
				EnableMenuItem(hpop, IDM_VPRIVILEGE, MF_DISABLED);
				EnableMenuItem(hpop, IDM_DEBUG, MF_DISABLED);

				EnableMenuItem(hpop, 9 + addMenu, MF_BYPOSITION | MF_DISABLED);
				EnableMenuItem(hpop, 10 + addMenu, MF_BYPOSITION | MF_DISABLED);
				EnableMenuItem(hpop, 11 + addMenu, MF_BYPOSITION | MF_DISABLED);
				EnableMenuItem(hpop, 12 + addMenu, MF_BYPOSITION | MF_DISABLED);
			}
		}

		thisCommandhProcess = MTryFindOpenedProcessHANDLE(pid);

		if (pid > 4 && thisCommandhProcess)
		{
			PROCESS_PRIORITY_CLASS priorityClass = { 0 };
			NTSTATUS status = NtQueryInformationProcess(thisCommandhProcess, ProcessPriorityClass, &priorityClass, sizeof(PROCESS_PRIORITY_CLASS), NULL);
			if (NT_SUCCESS(status))
			{
				thisCommandProcessPriority = priorityClass.PriorityClass;
				HMENU hpopAfs = GetSubMenu(hpop, 10 + addMenu);
				switch (priorityClass.PriorityClass)
				{
				case PROCESS_PRIORITY_CLASS_REALTIME:
					CheckMenuRadioItem(hpopAfs, 0, 5, 0, MF_BYPOSITION);
					break;
				case PROCESS_PRIORITY_CLASS_HIGH:
					CheckMenuRadioItem(hpopAfs, 0, 5, 1, MF_BYPOSITION);
					break;
				case PROCESS_PRIORITY_CLASS_ABOVE_NORMAL:
					CheckMenuRadioItem(hpopAfs, 0, 5, 2, MF_BYPOSITION);
					break;
				case PROCESS_PRIORITY_CLASS_NORMAL:
					CheckMenuRadioItem(hpopAfs, 0, 5, 3, MF_BYPOSITION);
					break;
				case PROCESS_PRIORITY_CLASS_BELOW_NORMAL:
					CheckMenuRadioItem(hpopAfs, 0, 5, 4, MF_BYPOSITION);
					break;
				case PROCESS_PRIORITY_CLASS_IDLE:
					CheckMenuRadioItem(hpopAfs, 0, 5, 5, MF_BYPOSITION);
					break;
				}
			}
			else
			{
				Log(L"NtQueryInformationProcess  ProcessPriorityClass FAILED ! %s", MNtStatusToStr(status));
				EnableMenuItem(hpop, 10 + addMenu, MF_BYPOSITION | MF_DISABLED);
			}
		}
		else {
			EnableMenuItem(hpop, 10 + addMenu, MF_BYPOSITION | MF_DISABLED);
			EnableMenuItem(hpop, 11 + addMenu, MF_BYPOSITION | MF_DISABLED);
			EnableMenuItem(hpop, 12 + addMenu, MF_BYPOSITION | MF_DISABLED);
		}

		TrackPopupMenu(hpop,
			TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
			pt.x,
			pt.y,
			0,
			hWndMain,
			NULL);
	}
	else if (pid == 0)
	{
		thisCommandhProcess = NULL;

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
		EnableMenuItem(hpop, IDM_SGINED, MF_DISABLED);
		EnableMenuItem(hpop, IDM_VKSTRUCTS, MF_DISABLED);
		EnableMenuItem(hpop, IDM_VHOTKEY, MF_DISABLED);
		EnableMenuItem(hpop, IDM_VTIMER, MF_DISABLED);
		EnableMenuItem(hpop, IDM_VPRIVILEGE, MF_DISABLED);
		EnableMenuItem(hpop, IDM_FILEPROP, MF_DISABLED);

		EnableMenuItem(hpop, ID_TASKMENU_SETAFFINITY, MF_DISABLED);

		EnableMenuItem(hpop, 9, MF_BYPOSITION | MF_DISABLED);
		EnableMenuItem(hpop, 10, MF_BYPOSITION | MF_DISABLED);
		EnableMenuItem(hpop, 12, MF_BYPOSITION | MF_DISABLED);

		TrackPopupMenu(hpop,
			TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
			pt.x,
			pt.y,
			0,
			hWndMain,
			NULL);
	}
	DestroyMenu(hroot);
	return 0;
}

extern HINSTANCE hInst;

HINSTANCE hLoader;
HINSTANCE hNtDll;
HINSTANCE hShell32;
HINSTANCE hKernel32;
HINSTANCE hKernelBase;
HINSTANCE hUser32;
HINSTANCE hCryptui;
HINSTANCE hIphlpapi;
HINSTANCE hImghlp;
HINSTANCE hPerfDisk;
HINSTANCE hComBase;
HINSTANCE hClr;
HINSTANCE hGdiPlus;
HINSTANCE hWinsta;

NTSTATUS MReadVirtualMemory(_In_ HANDLE ProcessHandle, _In_opt_ PVOID BaseAddress, _Out_writes_bytes_(BufferSize) PVOID Buffer, _In_ SIZE_T BufferSize, _Out_opt_ PSIZE_T NumberOfBytesRead)
{
	return NtReadVirtualMemory(ProcessHandle, BaseAddress, Buffer, BufferSize, NumberOfBytesRead);
}


//动态加载api
BOOL LoadDll()
{
	hLoader = GetModuleHandle(NULL);
	hNtDll = GetModuleHandle(L"ntdll.dll");
	hShell32 = GetModuleHandle(L"shell32.dll");
	hKernelBase = GetModuleHandle(L"kernelbase.dll");
	hKernel32 = GetModuleHandle(L"kernel32.dll");
	hUser32 = GetModuleHandle(L"user32.dll");
	hCryptui = LoadLibrary(L"Cryptui.dll");
	hIphlpapi = LoadLibrary(L"IPHLPAPI.dll");
	hImghlp = LoadLibrary(L"Imagehlp.dll");
	hPerfDisk = LoadLibrary(L"perfdisk.dll");
	hComBase = LoadLibrary(L"combase.dll");
	hGdiPlus = LoadLibrary(L"GdiPlus.dll");
	hWinsta = LoadLibrary(L"WinSta.dll");
	thisCommandPath = new WCHAR[260];
	thisCommandName = new WCHAR[260];
	thisCommandUWPName = new WCHAR[260];
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

		//加载一些未文档化的函数
		//ntdll

		NtSuspendProcess = (NtSuspendProcessFun)GetProcAddress(hNtDll, "NtSuspendProcess");
		NtResumeProcess = (NtResumeProcessFun)GetProcAddress(hNtDll, "NtResumeProcess");
		NtTerminateProcess = (NtTerminateProcessFun)GetProcAddress(hNtDll, "NtTerminateProcess");
		NtOpenProcess = (NtOpenProcessFun)GetProcAddress(hNtDll, "NtOpenProcess");
		NtOpenThread = (NtOpenThreadFun)GetProcAddress(hNtDll, "NtOpenThread");
		NtQueryInformationThread = (NtQueryInformationThreadFun)GetProcAddress(hNtDll, "NtQueryInformationThread");
		NtResumeThread = (NtResumeThreadFun)GetProcAddress(hNtDll, "NtResumeThread");
		NtTerminateThread = (NtTerminateThreadFun)GetProcAddress(hNtDll, "NtTerminateThread");
		NtSuspendThread = (NtSuspendThreadFun)GetProcAddress(hNtDll, "NtSuspendThread");
		NtQueryObject = (NtQueryObjectFun)GetProcAddress(hNtDll, "NtQueryObject");
		NtUnmapViewOfSection = (NtUnmapViewOfSectionFun)GetProcAddress(hNtDll, "NtUnmapViewOfSection");
		NtQuerySystemInformation = (NtQuerySystemInformationFun)GetProcAddress(hNtDll, "NtQuerySystemInformation");
		NtQueryInformationProcess = (NtQueryInformationProcessFun)GetProcAddress(hNtDll, "NtQueryInformationProcess");
		LdrGetProcedureAddress = (LdrGetProcedureAddressFun)GetProcAddress(hNtDll, "LdrGetProcedureAddress");
		RtlInitAnsiString = (RtlInitAnsiStringFun)GetProcAddress(hNtDll, "RtlInitAnsiString");
		RtlNtStatusToDosError = (RtlNtStatusToDosErrorFun)GetProcAddress(hNtDll, "RtlNtStatusToDosError");
		RtlGetLastWin32Error = (RtlGetLastWin32ErrorFun)GetProcAddress(hNtDll, "RtlGetLastWin32Error");
		NtQueryVirtualMemory = (NtQueryVirtualMemoryFun)GetProcAddress(hNtDll, "NtQueryVirtualMemory");
		NtReadVirtualMemory = (NtReadVirtualMemoryFun)GetProcAddress(hNtDll, "NtReadVirtualMemory");
		NtSetInformationProcess = (NtSetInformationProcessFun)GetProcAddress(hNtDll, "NtSetInformationProcess");
		NtDuplicateObject = (NtDuplicateObjectFun)GetProcAddress(hNtDll, "NtDuplicateObject");
		NtClose = (NtCloseFun)GetProcAddress(hNtDll, "NtClose");
		NtSetInformationDebugObject = (NtSetInformationDebugObjectFun)GetProcAddress(hNtDll, "NtSetInformationDebugObject");
		NtRemoveProcessDebug = (NtRemoveProcessDebugFun)GetProcAddress(hNtDll, "NtRemoveProcessDebug");


		//shell32
		RunFileDlg = (_RunFileDlg)MGetProcedureAddress(hShell32, NULL, 61);
		//k32
		dLoadLibraryA = (fnLoadLibraryA)GetProcAddress(hUser32, "LoadLibraryA");
		dLoadLibraryW = (fnLoadLibraryW)GetProcAddress(hUser32, "LoadLibraryW");
		dIsImmersiveProcess = (_IsImmersiveProcess)GetProcAddress(hUser32, "IsImmersiveProcess");
		dGetPackageFullName = (_GetPackageFullName)GetProcAddress(hKernel32, "GetPackageFullName");
		dGetPackageInfo = (_GetPackageInfo)GetProcAddress(hKernel32, "GetPackageInfo");
		dClosePackageInfo = (_ClosePackageInfo)GetProcAddress(hKernel32, "ClosePackageInfo");
		dOpenPackageInfoByFullName = (_OpenPackageInfoByFullName)GetProcAddress(hKernel32, "OpenPackageInfoByFullName");
		dGetPackageId = (_GetPackageId)GetProcAddress(hKernel32, "GetPackageId");
		dGetModuleFileNameW = (_GetModuleFileNameW)GetProcAddress(hKernel32, "GetModuleFileNameW");
		//
		dCryptUIDlgViewCertificateW = (_CryptUIDlgViewCertificateW)GetProcAddress(hCryptui, "CryptUIDlgViewCertificateW");
		dCryptUIDlgViewContext = (_CryptUIDlgViewContext)GetProcAddress(hCryptui, "CryptUIDlgViewContext");
		//u32 api
		dCancelShutdown = (_CancelShutdown)GetProcAddress(hUser32, "CancelShutdown");
		//
		dGetPerTcpConnectionEStats = (_GetPerTcpConnectionEStats)GetProcAddress(hIphlpapi, "GetPerTcpConnectionEStats");
		dGetExtendedTcpTable = (_GetExtendedTcpTable)GetProcAddress(hIphlpapi, "GetExtendedTcpTable");
		dGetPerTcp6ConnectionEStats = (_GetPerTcp6ConnectionEStats)GetProcAddress(hIphlpapi, "GetPerTcp6ConnectionEStats");
		dSetPerTcpConnectionEStats = (_SetPerTcpConnectionEStats)GetProcAddress(hIphlpapi, "SetPerTcpConnectionEStats");
		//
		ImageLoad = (fnIMAGELOAD)GetProcAddress(hImghlp, "ImageLoad");
		ImageUnload = (fnIMAGEUNLOAD)GetProcAddress(hImghlp, "ImageUnload");

		WinStationConnectW = (_WinStationConnectW)GetProcAddress(hWinsta, "WinStationConnectW");
		WinStationDisconnect = (_WinStationDisconnect)GetProcAddress(hWinsta, "WinStationDisconnect");
		WinStationReset = (_WinStationReset)GetProcAddress(hWinsta, "WinStationReset");
		WinStationFreeMemory = (_WinStationFreeMemory)GetProcAddress(hWinsta, "WinStationFreeMemory");
		WinStationEnumerateW = (_WinStationEnumerateW)GetProcAddress(hWinsta, "WinStationEnumerateW");
		WinStationQueryInformationW = (_WinStationQueryInformationW)GetProcAddress(hWinsta, "WinStationQueryInformationW");

		return TRUE;
	}
}
void FreeDll()
{
	MUserNameSIDsDeleteAll();

	FreeLibrary(hCryptui);
	FreeLibrary(hIphlpapi);
	FreeLibrary(hImghlp);
	FreeLibrary(hWinsta);

	delete thisCommandUWPName;
	delete thisCommandPath;
	delete thisCommandName;
}

VOID MBoom() 
{
	FreeLibrary(hNtDll);
}
BOOL MIsLoadLibrary(ULONG_PTR startAddress)
{
	if (startAddress == (ULONG_PTR)dLoadLibraryA || startAddress == (ULONG_PTR)dLoadLibraryW)
		return TRUE;

	/*UCHAR bytes[16];
	memmove(bytes, (LPVOID)startAddress, sizeof(bytes));

	std::wstring *diastring = nullptr;
	if (M_KDA_DeAssemblier(bytes, startAddress, NULL, sizeof(bytes), &diastring))
	{
		MessageBox(0, diastring->c_str(), L"MIsLoadLibrary", 0);

		delete diastring;
	}*/
	return FALSE;
}
BOOL MIsATrustDll2(LPWSTR fullPath)
{
	if (StrEqual(fullPath, L"C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\clr.dll"))
		return TRUE;
	else if (wcsncmp(fullPath, L"C:\\Windows\\", 11) == 0)
	{
		std::wstring *fname = Path::GetFileName(fullPath);
		if (*fname == L"GdiPlus.dll")
			return TRUE;
	}
	return FALSE;
}
BOOL MIsATrustDll(HMODULE dll)
{
	if (dll == hInst || dll == hLoader
		|| dll == hNtDll || dll == hPerfDisk || dll == hShell32
		|| dll == hComBase || dll == hClr
		|| dll == hGdiPlus)
		return TRUE;
	return FALSE;
}
VOID MAnitInjectLow() {
	if (NtQueryInformationThread)
	{
		HANDLE hThread = GetCurrentThread();
		MEMORY_BASIC_INFORMATION mbi = { 0 };
		ULONG_PTR dwStaAddr = 0;
		ULONG dwReturnLength = 0;
		NtQueryInformationThread(hThread, ThreadQuerySetWin32StartAddress, &dwStaAddr, sizeof(dwStaAddr), &dwReturnLength);
		VirtualQuery((LPVOID)dwStaAddr, &mbi, sizeof(mbi));
		if (!MIsATrustDll((HMODULE)mbi.AllocationBase)) {
			/*
			DWORD tid = GetCurrentThreadId();
			TCHAR modpath[260];
			TCHAR modname[260];
			if (K32GetMappedFileNameW(GetCurrentProcess(), (LPVOID)dwStaAddr, modname, 260) > 0)
				MDosPathToNtPath(modname, modpath);
			
			wchar_t str[300];
#ifdef _AMD64_
			swprintf_s(str, L"An abnormal thread is found. Do you want to terminate it ?\nThread id : %d in 0x%I64X\nAllocationBase : 0x%I64X", tid, dwStaAddr, (ULONG_PTR)mbi.AllocationBase);
#else
			swprintf_s(str, L"An abnormal thread is found. Do you want to terminate it ?\nThread id : %d in 0x%08X\nAllocationBase : 0x%08X", tid, dwStaAddr, (ULONG_PTR)mbi.AllocationBase);
#endif
			*/
			if (mbi.AllocationBase == hKernel32 && MIsLoadLibrary(dwStaAddr))
			{
				NTSTATUS status = NtTerminateThread(hThread, 0);
				if(!NT_SUCCESS(status)) MBoom();
				return;
			}
			/*if (!StrEqual(modpath, L"")) {

				if (MIsATrustDll2(modpath)) return;

				wcscat_s(str, L"\nThread base dll :");
				wcscat_s(str, modpath);
			}
			if (MessageBox(0, str, L"illegal !", MB_YESNO) == IDYES)
				ExitThread(0);*/
		}
	}
}
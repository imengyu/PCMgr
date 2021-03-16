#include "stdafx.h"
#include "vprocx.h"
#include "resource.h"
#include "mapphlp.h"
#include "prochlp.h"
#include "thdhlp.h"
#include "syshlp.h"
#include "fmhlp.h"
#include "ntdef.h"
#include "kernelhlp.h"
#include "lghlp.h"
#include "suact.h"
#include "StringHlp.h"
#include "loghlp.h"
#include <list>

typedef void(__cdecl *EnumWinsCallBack)(HWND hWnd, HWND hWndParent);

DWORD currentPid = 0;
DWORD currentShowThreadPid = 0;
int selectItem2 = 0, selectItem1 = 0, winscount = 0, winicoidx = 0, threadscount = 0;
HWND selectItem3 = 0;

HWND hListModuls = NULL, hListWins = NULL, hListThreads = NULL, hListHandles = NULL;
HWND hListHeader = NULL;
HMODULE selectHModule = NULL;
extern HANDLE hMainDevice;
extern HINSTANCE hInstRs;
extern bool use_apc;
LPWSTR curretProcName = 0;
HIMAGELIST hImgListWinSm;
HCURSOR hCurLoading;

extern NtQueryInformationThreadFun NtQueryInformationThread;
extern NtQueryInformationProcessFun NtQueryInformationProcess;
extern NtReadVirtualMemoryFun NtReadVirtualMemory;

using namespace std;

typedef struct tag_UWPWindow
{
	HWND hWndHost;
	HWND hWndCoreWindow;
	DWORD hostPid;
	BOOL notFoundRealWindow;
}UWPWindow,*PUWPWindow;

list<HWND> *hAllWins = nullptr;
list<PUWPWindow> *hUWPWins = nullptr;
list<HWND> *hUWPBrokedWins = nullptr;
HDESK hDesk = NULL;

void WindowEnumStart() {
	hDesk = OpenDesktop(L"Default", 0, FALSE, DESKTOP_ENUMERATE);
	hAllWins = new list<HWND>();
	hUWPBrokedWins = new list<HWND>();
	hUWPWins = new list<PUWPWindow>();

}
void WindowEnumDestroy() {
	CloseDesktop(hDesk);
	for (auto it = hUWPWins->begin(); it != hUWPWins->end(); it++)
		MFree(*it);
	hUWPWins->clear();
	delete hUWPWins;
	delete hUWPBrokedWins;
	delete hAllWins;
}

extern NtUnmapViewOfSectionFun NtUnmapViewOfSection;
extern EnumWinsCallBack hEnumWinsCallBack;
extern GetWinsCallBack hGetWinsWinsCallBack;

extern BOOL ShowMainCore(HWND hWndParent);

LPARAM GetItemData(HWND hList, int nItem)
{
	LVITEM lvi;
	memset(&lvi, 0, sizeof(LVITEM));
	lvi.iItem = nItem;
	lvi.mask = LVIF_PARAM;
	SendMessage(hList, LVM_GETITEM, 0, (LPARAM)&lvi);
	return (DWORD)lvi.lParam;
}

LPARAM SetItemData(HWND hList, int nItem, LPARAM dwData)
{
	if (IsWindow(hList))
	{
		LVITEM lvi;
		memset(&lvi, 0, sizeof(LVITEM));
		lvi.iItem = nItem;
		SendMessage(hList, LVM_GETITEM, 0, (LPARAM)&lvi);
		lvi.mask = LVIF_PARAM;
		lvi.lParam = dwData;
		SendMessage(hList, LVM_SETITEM, 0, (LPARAM)&lvi);
	}
	return NULL;
}

BOOL FreeLibraryEx()
{
	HANDLE hProcess;
	NTSTATUS status = MOpenProcessNt(currentPid, &hProcess);
	if (status == STATUS_INVALID_HANDLE || status == STATUS_INVALID_CID) {
		MessageBox(NULL, str_item_freeinvproc, str_item_freefailed, MB_OK | MB_ICONWARNING);
	}
	else if (hProcess) {
		TCHAR address[128];
		ListView_GetItemText(hListModuls, selectItem2, 2, address, 128);
		long long moduleBaseAddr = StringHlp::HexStrToLongW(address);
		DWORD rs2 = NtUnmapViewOfSection(hProcess, (PVOID)moduleBaseAddr);
		if (rs2 == 0)
			return TRUE;
		else if (rs2 == STATUS_PROCESS_IS_TERMINATING)
			MessageBox(NULL, str_item_freeinvproc, str_item_freefailed, MB_OK | MB_ICONWARNING);
		else {
			LogErr(L"UnmapView failed %s NTSTATUS : 0x%08X", address, status);
			ThrowErrorAndErrorCodeX(rs2, L"", str_item_freefailed);
		}
	}
	else {
		LogErr(L"OpenProcess %d Error : 0x%08X", currentPid, status);
		ThrowErrorAndErrorCodeX(status, str_item_openprocfailed, str_item_freefailed);
	}
	return FALSE;
}

void KillThreadKernel(bool a = false);

bool KillThread()
{
	HANDLE hThread;
	NTSTATUS NtStatus = MOpenThreadNt(selectItem1, &hThread, currentShowThreadPid);
	if (NtStatus == STATUS_INVALID_HANDLE) {
		MessageBox(NULL, str_item_killinvthread, str_item_killthreaderr, MB_OK);
		return true;
	}
	if (hThread) {
		NtStatus = MTerminateThreadNt(hThread);
		if (NtStatus == STATUS_SUCCESS)
			return true;
		else {
			LogErr(L"TerminateThread Error : 0x%08X", NtStatus);
			ThrowErrorAndErrorCodeX(NtStatus, L"", str_item_killthreaderr);
		}
		return true;
	}
	else {
		LogErr(L"OpenThread Error : 0x%08X", NtStatus);
		if (MCanUseKernel())Log(L"You can try to terminate it in kernel module.");
		ThrowErrorAndErrorCodeX(NtStatus, str_item_openthreaderr, str_item_killthreaderr);
	}
	return false;
}
void KillThreadKernel(bool a)
{
	if (!MCanUseKernel())MessageBox(NULL, str_item_kernelnotload, str_item_killthreaderr, MB_OK | MB_ICONERROR);
	else {
		NTSTATUS status = 0;
		if (!(M_SU_TerminateThreadTID(currentShowThreadPid, 0, &status, use_apc) && (status) == STATUS_SUCCESS))
		{
			if (status == STATUS_INVALID_HANDLE) MessageBox(NULL, str_item_killinvthread, str_item_killthreaderr, MB_OK);
			else if (status == STATUS_ACCESS_DENIED)
			{
				if (!(M_SU_TerminateThreadTID(currentShowThreadPid, 0, &status) && (status) == STATUS_SUCCESS))
					ThrowErrorAndErrorCodeX(status, L"Kernel return 2", str_item_killthreaderr);
			}
			else ThrowErrorAndErrorCodeX(status, L"Kernel return", str_item_killthreaderr);
		}
	}
}
bool SuspendThread()
{
	if (MShowMessageDialog(NULL, str_item_suthreadwarn, str_item_question, L"", MB_ICONWARNING, MB_YESNO) == IDNO)
		return false;
	HANDLE hThread;
	NTSTATUS NtStatus = MOpenThreadNt(selectItem1, &hThread, currentShowThreadPid);
	if (NtStatus == STATUS_INVALID_HANDLE) {
		MessageBox(NULL, str_item_invthread, str_item_suthreaderr, MB_OK);
		return true;
	}
	if (hThread) {
		NtStatus = MSuspendThreadNt(hThread);
		if (NtStatus == STATUS_SUCCESS) {
			return true;
		}
		else {
			LogErr(L"SuspendThread Error : 0x%08X", NtStatus);
			ThrowErrorAndErrorCodeX(NtStatus, L"", str_item_suthreaderr);
		}
	}
	else {
		LogErr(L"OpenThread Error : 0x%08X", NtStatus);
		ThrowErrorAndErrorCodeX(NtStatus, str_item_openthreaderr, str_item_suthreaderr);
	}
	return false;
}
bool ResusemeThread()
{
	HANDLE hThread;
	NTSTATUS NtStatus = MOpenThreadNt(selectItem1, &hThread, currentShowThreadPid);
	if (NtStatus == STATUS_INVALID_HANDLE) {
		MessageBox(NULL, str_item_invthread, str_item_rethreaderr, MB_OK);
		return true;
	}
	if (hThread) {
		NtStatus = MResumeThreadNt(hThread);
		if (NtStatus == STATUS_SUCCESS)
			return true;
		else {
			ThrowErrorAndErrorCodeX(NtStatus, L"", str_item_rethreaderr);
			LogErr(L"ResumeThread Error : 0x%08X", NtStatus);
		}
	}
	else {
		ThrowErrorAndErrorCodeX(NtStatus, str_item_openthreaderr, str_item_rethreaderr);
		LogErr(L"OpenThread Error : 0x%08X", NtStatus);
	}
	return false;
}

BOOL CALLBACK lpEnumFunc3(HWND hWnd, LPARAM lParam);
BOOL CALLBACK lpEnumFunc(HWND hWnd, LPARAM lParam)
{
	DWORD processId;
	DWORD threadId = GetWindowThreadProcessId(hWnd, &processId);
	if (processId == lParam)
	{
		BOOL visible = IsWindowVisible(hWnd);
		LVITEM vitem;
		vitem.mask = LVIF_IMAGE | LVIF_TEXT;
		vitem.iSubItem = 0;
		vitem.iItem = 0;
		vitem.iSubItem = 0;
		HICON hIcon = NULL;
		if (!SendMessageTimeoutW(hWnd, WM_GETICON, 0, 0,
			SMTO_BLOCK | SMTO_ABORTIFHUNG, 500, (PULONG_PTR)&hIcon))
		{
			ImageList_AddIcon(hImgListWinSm, hIcon);
			vitem.iImage = winicoidx;
			winicoidx++;
		}
		else
		{
			if (visible)vitem.iImage = 0;
			else vitem.iImage = 1;
		}

		WCHAR text[260];
		GetWindowText(hWnd, text, 260);
		vitem.pszText = text;
		ListView_InsertItem(hListWins, &vitem);
		vitem.iSubItem++;
		WCHAR handle[14];
#ifdef _AMD64_
		swprintf_s(handle, L"%I64X", (ULONG_PTR)hWnd);
#else
		swprintf_s(handle, L"%08X", (ULONG_PTR)hWnd);
#endif
		vitem.pszText = handle;
		ListView_SetItem(hListWins, &vitem);
		vitem.iSubItem++;
		WCHAR clsmame[128];
		GetClassName(hWnd, clsmame, 128);
		vitem.pszText = clsmame;
		ListView_SetItem(hListWins, &vitem);
		vitem.iSubItem++;
		if (visible) vitem.pszText = str_item_visible;
		else vitem.pszText = L"-";
		ListView_SetItem(hListWins, &vitem);
		vitem.iSubItem++;

		WCHAR threadidstr[16];
		_ltow_s(threadId, threadidstr, 10);
		vitem.pszText = threadidstr;
		ListView_SetItem(hListWins, &vitem);
		winscount++;
	}
	return TRUE;
}
BOOL CALLBACK lpEnumFunc2(HWND hWnd, LPARAM lParam)
{
	if (IsWindowVisible(hWnd))
	{
		wchar_t txtn[50];
		GetWindowText(hWnd, txtn, 50);
		if (StrEqual(txtn, L""))
			return TRUE;

		long l = GetWindowLong(hWnd, GWL_EXSTYLE);
		long ls = GetWindowLong(hWnd, GWL_STYLE);
		wchar_t clsn[50];
		GetClassName(hWnd, clsn, 50);
		if (StrEqual(clsn, L"ApplicationFrameWindow"))
		{
			//This is a uwp host window
			if (((l & WS_EX_APPWINDOW) == WS_EX_APPWINDOW 
				|| (l & WS_EX_OVERLAPPEDWINDOW) == WS_EX_OVERLAPPEDWINDOW
				|| (ls & WS_CAPTION) == WS_CAPTION) && (l & WS_EX_NOACTIVATE) != WS_EX_NOACTIVATE)
			{
				PUWPWindow uwpWindow = (PUWPWindow)MAlloc(sizeof(UWPWindow));
				uwpWindow->hWndHost = hWnd;
				uwpWindow->notFoundRealWindow = TRUE;
				uwpWindow->hostPid = 0;
				EnumChildWindows(hWnd, lpEnumFunc3, (LPARAM)uwpWindow);
				hUWPWins->push_back(uwpWindow);
			}
		}
		else if (StrEqual(clsn, L"Windows.UI.Core.CoreWindow"))
		{
			//This is a broaked uwp window
			if ((ls & WS_VISIBLE) == WS_VISIBLE)
				hUWPBrokedWins->push_back(hWnd);
		}
		else
		{
			if ((l & WS_EX_TOOLWINDOW) != WS_EX_TOOLWINDOW) {
				if ((l & WS_EX_APPWINDOW) == WS_EX_APPWINDOW
					|| (l & WS_EX_OVERLAPPEDWINDOW) == WS_EX_OVERLAPPEDWINDOW)
					hAllWins->push_back(hWnd);
				else if ((ls & WS_CAPTION) == WS_CAPTION
					|| (ls & WS_OVERLAPPED) == WS_OVERLAPPED) {
					hAllWins->push_back(hWnd);
				}
			}
		}
	}
	return TRUE;
}
BOOL CALLBACK lpEnumFunc3(HWND hWnd, LPARAM lParam)
{
	if (lParam)
	{
		WCHAR className[32];
		GetClassName(hWnd, className, 32);
		if (StrEqual(className, L"Windows.UI.Core.CoreWindow"))
		{
			PUWPWindow window = (PUWPWindow)lParam;
			window->hWndCoreWindow = hWnd;
			window->notFoundRealWindow = FALSE;
			GetWindowThreadProcessId(hWnd, &window->hostPid);
			return FALSE;
		}
	}
	return TRUE;
}

void MAppVProcessAllWindowsUWP()
{
	if (hGetWinsWinsCallBack) {
		list<UWPWindow*>::iterator it;
		for (it = hUWPWins->begin(); it != hUWPWins->end(); it++) 
		{
			PUWPWindow window = *it;
			if (window->notFoundRealWindow) 
			{
				WCHAR windowNameThis[64];
				GetWindowText(window->hWndHost, windowNameThis, 64);
				WCHAR windowNameFind[64];
				list<HWND>::iterator iter;
				for (iter = hUWPBrokedWins->begin(); iter != hUWPBrokedWins->end(); iter++)
				{
					GetWindowText((*iter), windowNameFind, 64);
					if (StrEqual(windowNameThis, windowNameFind))
					{
						window->hWndCoreWindow = (*iter);
						GetWindowThreadProcessId(window->hWndCoreWindow, &window->hostPid);
						break;
					}
				}
			}
			hGetWinsWinsCallBack(window->hWndHost, (HWND)(ULONG_PTR)window->hostPid, 1);
		}
	}
}
BOOL MAppVProcessAllWindowsGetProcessWindow(DWORD pid)
{
	if (hGetWinsWinsCallBack) 
	{
		list<HWND>::iterator it;
		for (it = hAllWins->begin(); it != hAllWins->end(); it++)
		{
			DWORD processId = 0;
			GetWindowThreadProcessId(*it, &processId);
			if (processId == pid)
			{
				wchar_t text[512];
				GetWindowText(*it, text, 512);
				hGetWinsWinsCallBack(*it, (HWND)&text, 0);
			}
		}	
		return TRUE;
	}
	return FALSE;
}
BOOL MAppVProcessAllWindowsGetProcessWindow2(DWORD pid)
{
	list<HWND>::iterator it;
	for (it = hAllWins->begin(); it != hAllWins->end(); it++)
	{
		DWORD processId = 0;
		GetWindowThreadProcessId(*it, &processId);
		if (processId == pid)
		{
			wchar_t text[512];
			GetWindowText(*it, text, 512);
			if (hGetWinsWinsCallBack)hGetWinsWinsCallBack(*it, (HWND)&text, 2);
			else return FALSE;
		}
	}
	return TRUE;
}

BOOL MAppVProcessModuls(DWORD dwPID, HWND hDlg, LPWSTR procName)
{
	currentPid = static_cast<DWORD>(dwPID);	curretProcName = procName;
	DialogBoxW(hInstRs, MAKEINTRESOURCE(IDD_VMODULS), hDlg, VModulsDlgProc);
	return TRUE;
}
BOOL MAppVProcessThreads(DWORD dwPID, HWND hDlg, LPWSTR procName)
{
	currentPid = static_cast<DWORD>(dwPID);	curretProcName = procName;
	DialogBoxW(hInstRs, MAKEINTRESOURCE(IDD_VTHEADS), hDlg, VThreadsDlgProc);
	return TRUE;
}
BOOL MAppVProcessWindows(DWORD dwPID, HWND hDlg, LPWSTR procName)
{
	currentPid = static_cast<DWORD>(dwPID);	curretProcName = procName;
	DialogBoxW(hInstRs, MAKEINTRESOURCE(IDD_VWINS), hDlg, VWinsDlgProc);
	return TRUE;
}
BOOL MAppVProcessAllWindows()
{
	for (auto it = hUWPWins->begin(); it != hUWPWins->end(); it++)
		MFree(*it);
	hUWPBrokedWins->clear();
	hUWPWins->clear();
	hAllWins->clear();
	
	if (EnumDesktopWindows(hDesk, lpEnumFunc2, NULL))
	{
		return TRUE;
	}

	return 0;
}

extern PSYSTEM_PROCESSES current_system_process;

BOOL MAppVModuls(DWORD dwPID, HWND hDlg, LPWSTR procName)
{
	BOOL bFound = FALSE;
	HANDLE hProcess;
	HWND htitle = GetDlgItem(hDlg, IDC_TITLE);
	NTSTATUS status = MOpenProcessNt(dwPID, &hProcess);
	if (status == STATUS_INVALID_HANDLE || status == STATUS_INVALID_CID) {
		SetWindowText(htitle, (LPWSTR)str_item_invalidproc.c_str());
		return -1;
	}	
	else if (status == STATUS_ACCESS_DENIED) {
		SetWindowText(htitle, (LPWSTR)str_item_access_denied.c_str());
		return -1;
	}
	else if (NT_SUCCESS(status) && hProcess) {

		BOOL bRet = FALSE;
		BOOL bWow64Proc = FALSE;
		IsWow64Process(hProcess, &bWow64Proc);
#ifdef _AMD64_
		if (!bWow64Proc)
		{
			SetWindowText(htitle, str_item_PleaseEnumIn64);
			return FALSE;
		}
#else
		if(bWow64Proc)
		{
			SetWindowText(htitle, str_item_PleaseEnumIn64);
			return FALSE;
		}
#endif

		PPEB_LDR_DATA pLdr = NULL;
		PPEB pPeb = 0;

		status = MGetProcessPeb(hProcess,&pPeb);

		if (NT_SUCCESS(status) && pPeb != 0)
		{
			status = NtReadVirtualMemory(hProcess, PTR_ADD_OFFSET(pPeb, FIELD_OFFSET(PEB, Ldr)), &pLdr, sizeof(PPEB_LDR_DATA), NULL);
			if (!NT_SUCCESS(status)) return FALSE;

			PLIST_ENTRY list_head = (PLIST_ENTRY)PTR_ADD_OFFSET(pLdr, FIELD_OFFSET(PEB_LDR_DATA, InMemoryOrderModuleList));
			PLIST_ENTRY p = list_head;
			LIST_ENTRY thisListEntry;
			status = NtReadVirtualMemory(hProcess, list_head, &thisListEntry, sizeof(LIST_ENTRY), NULL);

			int i = 0;
			WCHAR thisName[MAX_PATH];
			for (p = thisListEntry.Flink; p != list_head; p= thisListEntry.Flink)
			{
				LDR_MODULE thisModule;
				status = NtReadVirtualMemory(hProcess, CONTAINING_RECORD(p, LDR_MODULE, InMemoryOrderModuleList), &thisModule, sizeof(LDR_MODULE), NULL);
				status = NtReadVirtualMemory(hProcess, p, &thisListEntry, sizeof(LIST_ENTRY), NULL);

				if (thisModule.BaseDllName.Buffer != NULL) {
					WCHAR baseDllNameBuffer[MAX_PATH];
					status = NtReadVirtualMemory(hProcess, thisModule.BaseDllName.Buffer, &baseDllNameBuffer, sizeof(baseDllNameBuffer), NULL);
					wcscpy_s(thisName, baseDllNameBuffer);

					LVITEM vitem;
					vitem.mask = LVIF_TEXT;
					vitem.iSubItem = 0;
					vitem.iItem = 0;
					vitem.iSubItem = 0;
					vitem.lParam = (LPARAM)thisModule.BaseAddress;
					vitem.pszText = thisName;
					ListView_InsertItem(hListModuls, &vitem);

					WCHAR fullDllNameBuffer[MAX_PATH];

					vitem.iSubItem = 1;
					if (thisModule.FullDllName.Buffer != NULL) {
						status = NtReadVirtualMemory(hProcess, thisModule.FullDllName.Buffer, &fullDllNameBuffer, sizeof(fullDllNameBuffer), NULL);
						vitem.pszText = fullDllNameBuffer;
					}
					else vitem.pszText = L"-";
					ListView_SetItem(hListModuls, &vitem);
					vitem.iSubItem = 2;
					TCHAR addr[20];
#ifdef _AMD64_
					swprintf_s(addr, L"0x%I64X", (ULONG_PTR)thisModule.BaseAddress);
#else
					swprintf_s(addr, L"0x%08X", (ULONG_PTR)thisModule.BaseAddress);
#endif
					vitem.pszText = addr;
					ListView_SetItem(hListModuls, &vitem);
					vitem.iSubItem = 3;
					TCHAR sz[20];
					swprintf_s(sz, L"%d", thisModule.SizeOfImage);
					vitem.pszText = sz;
					ListView_SetItem(hListModuls, &vitem);
					vitem.iSubItem = 4;
					if (thisModule.FullDllName.Buffer != NULL) {
						WCHAR company[256] = { 0 };
						if (MGetExeCompany(fullDllNameBuffer, company, 255))
							vitem.pszText = company;
						else vitem.pszText = L"";
					}
					else vitem.pszText = L"";
					ListView_SetItem(hListModuls, &vitem);

					i++;

					if (i > 512)//Too big
						break;
				}
			}

			wstring str = FormatString(str_item_vmodulestitle, procName, dwPID, i);
			SetWindowText(hDlg, str.c_str());
			return TRUE;
		}
		else {
			LogWarn(L"View Modules Failed in MGetProcessPeb  : %s", MNtStatusToStr(status));
			wstring str = FormatString(L"%s : %s \n Error Code : %s", str_item_enum_modulefailed, L"Failed to get peb", MNtStatusToStr(status));
			SetWindowText(htitle, str.c_str());
		}

		MCloseHandle(hProcess);
	}
	else {
		LogWarn(L"View Modules Failed in OpenProcess : %s", MNtStatusToStr(status));
		wstring str = FormatString(L"%s\n%s\nError Code : %s", str_item_enum_modulefailed, str_item_openprocfailed, MNtStatusToStr(status));
		SetWindowText(htitle, str.c_str());
		return 0;
	}
	return FALSE;
}
BOOL MAppVThreads(DWORD dwPID, HWND hDlg, LPWSTR procName)
{
	BOOL bFound = FALSE;
	HWND htitle = GetDlgItem(hDlg, IDC_TITLE);
	threadscount = 0;

	bool done = false;
	for (PSYSTEM_PROCESSES p = current_system_process; !done;
		p = PSYSTEM_PROCESSES(PCHAR(p) + p->NextEntryOffset))
	{
		if (static_cast<DWORD>((ULONG_PTR)p->ProcessId) == dwPID)
		{
			for (ULONG i = 0; i < p->NumberOfThreads; i++)
			{
				SYSTEM_THREADS systemThread = p->Threads[i];
				ULONG_PTR tid = (ULONG_PTR)systemThread.ClientId.UniqueThread;

				NTSTATUS status = STATUS_SUCCESS;
				HANDLE hThread = NULL;
				MOpenThreadNt(static_cast<DWORD>(tid), &hThread, static_cast<DWORD>((ULONG_PTR)systemThread.ClientId.UniqueProcess));

				LVITEM vitem;
				vitem.mask = LVIF_TEXT;
				vitem.iSubItem = 0;
				vitem.iItem = 0;
				vitem.iSubItem = 0;
				wchar_t c[16];
#ifdef _X64_
				swprintf_s(c, L"%I64u", tid);
#else
				swprintf_s(c, L"%lu", tid);
#endif
				vitem.pszText = c;
				ListView_InsertItem(hListThreads, &vitem);
				vitem.iSubItem++;

				ULONG_PTR teb = 0;
				ULONG_PTR ethread = 0;
				if (M_SU_GetETHREAD(static_cast<DWORD>(tid), &ethread, &teb))
				{
					wchar_t x[32];
#ifdef _X64_
					swprintf_s(x, L"0x%I64X", ethread);
#else
					swprintf_s(x, L"0x%08X", ethread);
#endif
					vitem.pszText = x;
				}
				else vitem.pszText = L"-";
				ListView_SetItem(hListThreads, &vitem);
				vitem.iSubItem++;

				if (hThread && teb == 0) {
					status = MGetThreadTeb(hThread, (PTEB*)&teb);
					if (NT_SUCCESS(status))
					{
						TCHAR modname1[32];
#ifdef _X64_
						swprintf_s(modname1, L"0x%I64X", (ULONG_PTR)teb);
#else
						swprintf_s(modname1, L"0x%08X", (ULONG_PTR)teb);
#endif
						vitem.pszText = modname1;
					}
					else {
						TCHAR err[18];
						swprintf_s(err, L"%s", MNtStatusToStr(status));
						vitem.pszText = err;
					}
				}
				else if (teb) {
					TCHAR modname1[32];
#ifdef _X64_
					swprintf_s(modname1, L"0x%I64X", (ULONG_PTR)teb);
#else
					swprintf_s(modname1, L"0x%08X", (ULONG_PTR)teb);
#endif
					vitem.pszText = modname1;
				}
				else vitem.pszText = L"-";
				ListView_SetItem(hListThreads, &vitem);
				vitem.iSubItem++;

				WCHAR basePri[10];
				swprintf_s(basePri, L"%d", systemThread.BasePriority);
				vitem.pszText = basePri;
				ListView_SetItem(hListThreads, &vitem);
				vitem.iSubItem++;

				PVOID startaddr = 0;
				if (hThread)
				{
					status = MGetThreadWin32StartAddress(hThread, &startaddr);
					if (NT_SUCCESS(status)) {
						WCHAR modname1[32];
#ifdef _X64_
						swprintf_s(modname1, L"0x%I64X", (ULONG_PTR)(startaddr));
#else
						swprintf_s(modname1, L"0x%08X", (ULONG_PTR)(startaddr));
#endif
						vitem.pszText = modname1;
					}
					else {
						TCHAR err[18];
						swprintf_s(err, L"%s", MNtStatusToStr(status));
						vitem.pszText = err;
					}
				}
				else vitem.pszText = L"-";
				ListView_SetItem(hListThreads, &vitem);
				vitem.iSubItem++;

				if (hThread && startaddr != 0) {
					HANDLE hProcess = NULL;
					if (MOpenProcessNt(static_cast<DWORD>((ULONG_PTR)systemThread.ClientId.UniqueProcess), &hProcess) == STATUS_SUCCESS)
					{
						WCHAR modpath[260];
						WCHAR modname[260];
						if (MGetProcessMappedFileName(hProcess, startaddr, modname, 260) == STATUS_SUCCESS) {
							MDosPathToNtPath(modname, modpath);
							vitem.pszText = modpath;
						}
						else vitem.pszText = L"-";
					}
					else vitem.pszText = L"-";
					CloseHandle(hProcess);
				}
				else vitem.pszText = L"-";

				ListView_SetItem(hListThreads, &vitem);
				vitem.iSubItem++;
				THREAD_STATE ts = systemThread.ThreadState;
				switch (ts)
				{
				case THREAD_STATE::StateReady:
					vitem.pszText = L"StateReady";
					break;
				case THREAD_STATE::StateRunning:
					vitem.pszText = L"StateRunning";
					break;
				case THREAD_STATE::StateWait:
					vitem.pszText = L"StateWait";
					break;
				case THREAD_STATE::StateTerminated:
					vitem.pszText = L"StateTerminated";
					break;
				case 9:
					vitem.pszText = L"StateWait";
					break;
				default:
					vitem.pszText = L"0";
					break;
				}
				ListView_SetItem(hListThreads, &vitem);
				vitem.iSubItem++;

				WCHAR tswc[16];
				swprintf_s(tswc, L"%d", systemThread.ContextSwitchCount);
				vitem.pszText = tswc;
				ListView_SetItem(hListThreads, &vitem);
				threadscount++;

				if (hThread)CloseHandle(hThread);
			}

			wstring str = FormatString(str_item_vthreadtitle, procName, dwPID, threadscount);
			SetWindowText(hDlg, str.c_str());
			currentShowThreadPid = static_cast<DWORD>(dwPID);

			return TRUE;
		}
		done = p->NextEntryOffset == 0;
	}

	SetWindowText(htitle, (LPWSTR)str_item_invalidproc.c_str());
	LogWarn(L"Not found process : %d", dwPID);
	return 0;

	/*
	NTSTATUS rs = MOpenProcessNt(dwPID, &hProcess);
	if (rs == 0xC0000008 || rs == 0xC000000B) {
		SetWindowText(htitle, (LPWSTR)str_item_invalidproc.c_str());
		return -1;
	}
	else if (rs == STATUS_ACCESS_DENIED) {
		SetWindowText(htitle, (LPWSTR)str_item_access_denied.c_str());
		return -1;
	}
	else if (hProcess && rs == STATUS_SUCCESS) {
		HANDLE hSnapThread = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, static_cast<DWORD>(dwPID));
		if (INVALID_HANDLE_VALUE != hSnapThread)
		{
			THREADENTRY32 th32;
			th32.dwSize = sizeof(THREADENTRY32);
			int i = 0;
			BOOL fOk;
			for (fOk = Thread32First(hSnapThread, &th32); fOk; fOk = Thread32Next(hSnapThread, &th32)) {
				if (th32.th32OwnerProcessID == dwPID)
				{
					
				}
				threadscount = i;
			}

			
			CloseHandle(hSnapThread);

			return TRUE;
		}
		else
		{
			wstring str = FormatString(L"%s\nLast Error : 0x%x", str_item_enum_threadfailed, GetLastError());
			SetWindowText(htitle, str.c_str());
			return FALSE;
		}
	}
	else
	{
		wstring str = FormatString(L"%s , %s\nLast Error : 0x%08x", str_item_enum_threadfailed, str_item_openprocfailed,rs);
		SetWindowText(htitle, str.c_str());
		return FALSE;
	}*/
}
BOOL MAppVWins(DWORD dwPID, HWND hDlg, LPWSTR procName)
{
	winscount = 0; winicoidx = 2;
	ImageList_Destroy(hImgListWinSm);
	hImgListWinSm = ImageList_Create(16, 16, ILC_COLOR32, 1, 0);
	HICON hIcoDefH, hIcoDefS;
	hIcoDefS = LoadIcon(hInstRs, MAKEINTRESOURCE(IDI_ICONACTIVEDWIN));
	hIcoDefH = LoadIcon(hInstRs, MAKEINTRESOURCE(IDI_ICONHIDEDWIN));
	ImageList_AddIcon(hImgListWinSm, hIcoDefS);
	ImageList_AddIcon(hImgListWinSm, hIcoDefH);
	ListView_SetImageList(hListWins, hImgListWinSm, LVSIL_SMALL);
	HWND htitle = GetDlgItem(hDlg, IDC_RESULT);
	if (EnumWindows(lpEnumFunc, dwPID))
	{
		wstring str = FormatString(str_item_vwinstitle, procName, dwPID, winscount);
		SetWindowText(htitle, str.c_str());
		return TRUE;
	}
	else {
		wstring str = FormatString(L"EnumWindows failed\nError Code : %s", GetLastError());
		SetWindowText(htitle, str.c_str());
	}
	return FALSE;
}

bool sortAscending = false;
int oldSortItem = 0;

int CALLBACK Sort_VMODULS(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort)
{
	if (lParam1 == lParam2)
		return -1;
	int cloums = static_cast<int>(lParamSort);
	switch (cloums)
	{
	case 1: {
		wchar_t s1[MAX_PATH];
		wchar_t s2[MAX_PATH];
		ListView_GetItemText(hListModuls, lParam1, cloums, s1, MAX_PATH);
		ListView_GetItemText(hListModuls, lParam2, cloums, s2, MAX_PATH);
		if (sortAscending)
			return wcscmp(s1, s2);
		else return -wcscmp(s1, s2);
	}
	case 0:
	case 2:
	case 3:
	case 4: {
		wchar_t s1[64];
		wchar_t s2[64];
		ListView_GetItemText(hListModuls, lParam1, cloums, s1, 64);
		ListView_GetItemText(hListModuls, lParam2, cloums, s2, 64);
		if (sortAscending)
			return wcscmp(s1, s2);
		else return -wcscmp(s1, s2);
	}
	default:
		break;
	}
	return 0;
}
int CALLBACK Sort_VTHREADS(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort)
{
	if (lParam1 == lParam2)
		return -1;
	int cloums = static_cast<int>(lParamSort);
	switch (cloums)
	{
	case 0:
	case 7: {
		int rs = 0;
		wchar_t s1[64];
		wchar_t s2[64];
		ListView_GetItemText(hListThreads, lParam1, cloums, s1, 64);
		ListView_GetItemText(hListThreads, lParam2, cloums, s2, 64);
		int i1 = _wtoi(s1), i2 = _wtoi(s2);
		if (i1 == i2)return rs;
		if (i1 > i2) rs= 1;
		else rs= -1;
		return sortAscending ? rs : -rs;
	}
	case 5: {
		wchar_t s1[MAX_PATH];
		wchar_t s2[MAX_PATH];
		ListView_GetItemText(hListThreads, lParam1, cloums, s1, MAX_PATH);
		ListView_GetItemText(hListThreads, lParam2, cloums, s2, MAX_PATH);
		if (sortAscending)
			return wcscmp(s1, s2);
		else return -wcscmp(s1, s2);
	}
	case 1:
	case 2:
	case 3:
	case 4:
	case 6: {
		wchar_t s1[64];
		wchar_t s2[64];
		ListView_GetItemText(hListThreads, lParam1, cloums, s1, 64);
		ListView_GetItemText(hListThreads, lParam2, cloums, s2, 64);
		if (sortAscending)
			return wcscmp(s1, s2);
		else return -wcscmp(s1, s2);
	}
	default:
		break;
	}
	return 0;
}
int CALLBACK Sort_VWINS(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort)
{
	if (lParam1 == lParam2)
		return -1;
	wchar_t s1[64];
	wchar_t s2[64];
	int cloums = static_cast<int>(lParamSort);
	ListView_GetItemText(hListWins, lParam1, cloums, s1, 64);
	ListView_GetItemText(hListWins, lParam2, cloums, s2, 64);
	if (sortAscending)
		return wcscmp(s1, s2);
	else return -wcscmp(s1, s2);
}

INT_PTR CALLBACK VWinsDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_INITDIALOG: {
		SendMessage(hDlg, WM_SETICON, ICON_SMALL, (LPARAM)LoadIcon(hInstRs, MAKEINTRESOURCE(IDI_ICONAPP)));
		hListWins = GetDlgItem(hDlg, IDC_WINSLIST);
		LV_COLUMN lvc;
		lvc.mask = LVCF_TEXT | LVCF_WIDTH;
		lvc.pszText = str_item_wndbthread;
		lvc.cx = 60;
		SendMessageW(hListWins, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = str_item_visible;
		lvc.cx = 40;
		SendMessage(hListWins, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = str_item_wndclass;
		lvc.cx = 150;
		SendMessage(hListWins, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = str_item_windowhandle;
		lvc.cx = 85;
		SendMessage(hListWins, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = str_item_windowtext;
		lvc.cx = 180;
		SendMessage(hListWins, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		ListView_SetExtendedListViewStyleEx(hListWins, 0, LVS_EX_FULLROWSELECT | LVS_EX_TWOCLICKACTIVATE);
		MAppVWins(currentPid, hDlg, curretProcName);
		SetWindowTheme(hListWins, L"explorer", NULL);
		ListView_SetExtendedListViewStyleEx(hListWins, 0, LVS_EX_FULLROWSELECT | LVS_EX_TWOCLICKACTIVATE);
		SendMessage(hListWins, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);
		hListHeader = MListViewGetHeaderControl(hListWins);
		break;
	}
	case WM_SYSCOMMAND: {
		if (wParam == SC_CLOSE) {
			EndDialog(hDlg, 0);
		}
		return 0;
	}
	case WM_COMMAND: {
		switch (wParam)
		{
		case ID_WINSMENU_SPYWIN:
			if (hEnumWinsCallBack)
				hEnumWinsCallBack(selectItem3, hDlg);
			break;
		case ID_WINSMENU_DISABLE: {
			if (IsWindow(selectItem3))
				EnableWindow(selectItem3, FALSE);
			break;
		}
		case ID_WINSMENU_ENABLE: {
			if (IsWindow(selectItem3))
				EnableWindow(selectItem3, TRUE);
			break;
		}
		case ID_WINSMENU_SHOWREGION:
			break;
		case ID_WINSMENU_SHOWWND:
			if (IsWindow(selectItem3)) {
				ShowWindow(selectItem3, SW_SHOW);
				ListView_SetItemText(hListWins, ListView_GetSelectionMark(hListWins), 3, str_item_visible);
			}
			break;
		case ID_WINSMENU_HIDEWINDOW:
			if (IsWindow(selectItem3)) {
				ShowWindow(selectItem3, SW_HIDE);
				LPWSTR text = L"-";
				ListView_SetItemText(hListWins, ListView_GetSelectionMark(hListWins), 3, text);
			}
			break;
		case ID_WINSMENU_CLOSE: {
			if (IsWindow(selectItem3))
				CloseWindow(selectItem3);
			break;
		}
		case ID_WINSMENU_END: {
			if (IsWindow(selectItem3)) {
				SendMessage(selectItem3, WM_SYSCOMMAND, SC_CLOSE, NULL);
				WaitForSingleObject(selectItem3, 1000);
				if (IsWindow(selectItem3)) {
					CloseWindow(selectItem3);
					SendMessage(hDlg, WM_COMMAND, 41030, 0);
				}
			}
			break;
		}
		case ID_WINSMENU_MAX: {
			if (IsWindow(selectItem3))
				ShowWindow(selectItem3, SW_MAXIMIZE);
			break;
		}
		case ID_WINSMENU_MIN: {
			if (IsWindow(selectItem3))
				ShowWindow(selectItem3, SW_MINIMIZE);
			break;
		}
		case ID_WINSMENU_TOTOP: {
			if (IsWindow(selectItem3))
				SetForegroundWindow(selectItem3);
			break;
		}
		case ID_WINSMENU_SETTO: {
			if (IsWindow(selectItem3)) {
				SendMessage(selectItem3, WM_SYSCOMMAND, SC_RESTORE, 0);
				SetForegroundWindow(selectItem3);
			}
			break;
		}
		case IDC_REFESH: {
			SetCursor(hCurLoading);
			ListView_DeleteAllItems(hListWins);
			hListWins = GetDlgItem(hDlg, IDC_WINSLIST);
			MAppVWins(currentPid, hDlg, curretProcName);
			break;
		}
		case IDCANCEL:
			SendMessage(hDlg, WM_SYSCOMMAND, SC_CLOSE, NULL);
			break;
		case IDC_CLOSEWINDOW: {
			if (IsWindow(selectItem3)) {
				SendMessage(selectItem3, WM_SYSCOMMAND, SC_CLOSE, NULL);
				WaitForSingleObject(selectItem3, 1000);
				if (IsWindow(selectItem3)) {
					CloseWindow(selectItem3);
					SendMessage(hDlg, WM_COMMAND, 41030, 0);
				}
			}
			break;
		}
		}
		break;
	}
	case WM_SIZE: {
		RECT rc;
		GetClientRect(hDlg, &rc);
		MoveWindow(hListWins, 12, 34, rc.right - rc.left - 24, rc.bottom - rc.top - 74, TRUE);
		MoveWindow(GetDlgItem(hDlg, IDC_CLOSEWINDOW), rc.right - 164, rc.bottom - 33, 75, 23, TRUE);
		MoveWindow(GetDlgItem(hDlg, IDCANCEL), rc.right - 85, rc.bottom - 33, 75, 23, TRUE);
		break;
	}
	case WM_NOTIFY: {
		switch (LOWORD(wParam))
		{
		case IDC_WINSLIST:
			switch (((LPNMHDR)lParam)->code)
			{
			case LVN_KEYDOWN: {
				LPNMLVKEYDOWN lpnmk = (LPNMLVKEYDOWN)lParam;
				if (lpnmk->wVKey == 93)
				{
					WCHAR hwnd[32];
					ListView_GetItemText(hListWins, ListView_GetSelectionMark(hListWins), 1, hwnd, 32);
					selectItem3 = (HWND)LongToHandle(StringHlp::HexStrToIntW(hwnd));
					HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_WINSMENU));
					if (hroot) {
						HMENU hpop = GetSubMenu(hroot, 0);
						POINT pt;
						GetCursorPos(&pt);

						if (IsWindow(selectItem3)) {
							if (IsWindowEnabled(selectItem3))
								EnableMenuItem(hpop, ID_WINSMENU_ENABLE, MF_DISABLED);
							else
								EnableMenuItem(hpop, ID_WINSMENU_DISABLE, MF_DISABLED);
							if (!IsWindowVisible(selectItem3))
								EnableMenuItem(hpop, ID_WINSMENU_HIDEWINDOW, MF_DISABLED);
							else
								EnableMenuItem(hpop, ID_WINSMENU_SHOWWND, MF_DISABLED);
						}
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
				break;
			}
			case LVN_COLUMNCLICK:{
				if (oldSortItem != ((LPNMLISTVIEW)lParam)->iSubItem)
				{
					MListViewSetColumnSortArrow(hListHeader, oldSortItem, false, true);
					sortAscending = true;
					oldSortItem = ((LPNMLISTVIEW)lParam)->iSubItem;
					MListViewSetColumnSortArrow(hListHeader, oldSortItem, sortAscending, false);
				}
				else {
					sortAscending = !sortAscending;
					MListViewSetColumnSortArrow(hListHeader, oldSortItem, sortAscending, false);
				}
				ListView_SortItemsEx(hListWins, Sort_VWINS, ((LPNMLISTVIEW)lParam)->iSubItem);
				break;
			}
			case NM_CLICK: {
				WCHAR hwnd[32];
				ListView_GetItemText(hListWins, ListView_GetSelectionMark(hListWins), 1, hwnd, 32);
				selectItem3 = (HWND)LongToHandle(StringHlp::HexStrToIntW(hwnd));
				break;
			}
			case NM_RCLICK:{
				WCHAR hwnd[32];
				ListView_GetItemText(hListWins, ListView_GetSelectionMark(hListWins), 1, hwnd, 32);
				selectItem3 = (HWND)LongToHandle(StringHlp::HexStrToIntW(hwnd));
				HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_WINSMENU));
				if (hroot) {
					HMENU hpop = GetSubMenu(hroot, 0);
					POINT pt;
					GetCursorPos(&pt);

					if (IsWindow(selectItem3)) {
						if (IsWindowEnabled(selectItem3))
							EnableMenuItem(hpop, ID_WINSMENU_ENABLE, MF_DISABLED);
						else
							EnableMenuItem(hpop, ID_WINSMENU_DISABLE, MF_DISABLED);
						if (!IsWindowVisible(selectItem3))
							EnableMenuItem(hpop, ID_WINSMENU_HIDEWINDOW, MF_DISABLED);
						else
							EnableMenuItem(hpop, ID_WINSMENU_SHOWWND, MF_DISABLED);
					}
					TrackPopupMenu(hpop,
						TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
						pt.x,
						pt.y,
						0,
						hDlg,
						NULL);
					DestroyMenu(hroot);
				}
				break;
			}
			}
		}
		break;
	}
	}
	return (INT_PTR)FALSE;
}
INT_PTR CALLBACK VModulsDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_INITDIALOG: {
		SendMessage(hDlg, WM_SETICON, ICON_SMALL, (LPARAM)LoadIcon(hInstRs, MAKEINTRESOURCE(IDI_ICONAPP)));
		hListModuls = GetDlgItem(hDlg, IDC_MODULLIST);
		LV_COLUMN lvc;
		lvc.mask = LVCF_TEXT | LVCF_WIDTH;
		lvc.pszText = str_item_publisher;
		lvc.cx = 130;
		SendMessage(hListModuls, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = str_item_size;
		lvc.cx = 80;
		SendMessage(hListModuls, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = str_item_address;
		lvc.cx = 80;
		SendMessage(hListModuls, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = str_item_modulepath;
		lvc.cx = 300;
		SendMessageW(hListModuls, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = str_item_modulename;
		lvc.cx = 80;
		SendMessage(hListModuls, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		SetWindowTheme(hListModuls, L"explorer", NULL);
		ListView_SetExtendedListViewStyleEx(hListModuls, 0, LVS_EX_FULLROWSELECT | LVS_EX_TWOCLICKACTIVATE);
		SendMessage(hListModuls, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);
		hListHeader = MListViewGetHeaderControl(hListModuls);
		if (MAppVModuls(currentPid, hDlg, curretProcName) == 1) {
			ShowWindow(hListModuls, SW_SHOW);
			ShowWindow(GetDlgItem(hDlg, IDC_TITLE), SW_HIDE);
		}
		else {
			ShowWindow(hListModuls, SW_HIDE);
			ShowWindow(GetDlgItem(hDlg, IDC_TITLE), SW_SHOW);
		}
		return 0;
	}
	case WM_SYSCOMMAND: {
		if (wParam == SC_CLOSE) {
			EndDialog(hDlg, 0);
		}
		return 0;
	}
	case WM_COMMAND:
		switch (wParam)
		{
		case IDC_REFESH:
			ListView_DeleteAllItems(hListModuls);
			hListModuls = GetDlgItem(hDlg, IDC_MODULLIST);
			if (MAppVModuls(currentPid, hDlg, curretProcName) == 1) {
				ShowWindow(hListModuls, SW_SHOW);
				ShowWindow(GetDlgItem(hDlg, IDC_TITLE), SW_HIDE);
			}
			else {
				ShowWindow(hListModuls, SW_HIDE);
				ShowWindow(GetDlgItem(hDlg, IDC_TITLE), SW_SHOW);
			}
			break;
		case IDCANCEL:
			SendMessage(hDlg, WM_SYSCOMMAND, SC_CLOSE, NULL);
			break;
		case ID_MODULSMENU_FREELIB:
			SetCursor(hCurLoading);
			if (FreeLibraryEx())
				MessageBox(NULL, str_item_freesuccess, L"", MB_OK);
			break;
		case ID_MODULSMENU_FILEPROP: {
			SetCursor(hCurLoading);
			TCHAR path[MAX_PATH];
			ListView_GetItemText(hListModuls, selectItem2, 1, path, MAX_PATH);
			MShowFileProp(path);
			break;
		}
		case ID_MODULSMENU_OPENPATH:
			SetCursor(hCurLoading);
			TCHAR path[MAX_PATH];
			ListView_GetItemText(hListModuls, selectItem2, 1, path, MAX_PATH);
			if (!StrEqual(path, L""))
			{
				std::wstring buf = FormatString(L"/select,%s", path);
				ShellExecuteW(NULL, NULL, L"explorer.exe", buf.c_str(), NULL, SW_SHOWDEFAULT);
			}
			else MessageBox(hDlg, str_item_cantgetpath, L"", MB_ICONERROR | MB_OK);
			break;
		}
		break;
	case WM_SIZE:	{
		RECT rc;
		GetClientRect(hDlg, &rc);
		MoveWindow(hListModuls, 0, 0, rc.right - rc.left, rc.bottom - rc.top, TRUE);
		MoveWindow(GetDlgItem(hDlg, IDC_TITLE), 0, 0, rc.right - rc.left, rc.bottom - rc.top, TRUE);
		break;
	}
	case WM_NOTIFY:
		switch (LOWORD(wParam))
		{
		case IDC_MODULLIST:
			switch (((LPNMHDR)lParam)->code)
			{
			case LVN_KEYDOWN: {
				LPNMLVKEYDOWN lpnmk = (LPNMLVKEYDOWN)lParam;
				if (lpnmk->wVKey == 93)
				{
					selectItem2 = ListView_GetSelectionMark(hListModuls);
					selectHModule = (HMODULE)GetItemData(hListModuls, ListView_GetSelectionMark(hListModuls));
					HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MODULSMENU));
					if (hroot) {
						HMENU hpop = GetSubMenu(hroot, 0);
						POINT pt;
						GetCursorPos(&pt);
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
				break;
			}
			case LVN_COLUMNCLICK: {					
				if (oldSortItem != ((LPNMLISTVIEW)lParam)->iSubItem)
				{
					MListViewSetColumnSortArrow(hListHeader, oldSortItem, false, true);
					sortAscending = true;
					oldSortItem = ((LPNMLISTVIEW)lParam)->iSubItem;
					MListViewSetColumnSortArrow(hListHeader, oldSortItem, sortAscending, false);
				}
				else {
					sortAscending = !sortAscending;
					MListViewSetColumnSortArrow(hListHeader, oldSortItem, sortAscending, false);
				}
				ListView_SortItemsEx(hListModuls, Sort_VMODULS, ((LPNMLISTVIEW)lParam)->iSubItem);
				break;
			}
			case NM_CLICK: {
				selectItem2 = ListView_GetSelectionMark(hListModuls);
				selectHModule = (HMODULE)GetItemData(hListModuls, ListView_GetSelectionMark(hListModuls));
				break;
			}
			case NM_RCLICK: {
				selectItem2 = ListView_GetSelectionMark(hListModuls);
				selectHModule = (HMODULE)GetItemData(hListModuls, ListView_GetSelectionMark(hListModuls));
				HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MODULSMENU));
				if (hroot) {
					HMENU hpop = GetSubMenu(hroot, 0);
					POINT pt;
					GetCursorPos(&pt);
					TrackPopupMenu(hpop,
						TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
						pt.x,
						pt.y,
						0,
						hDlg,
						NULL);
					DestroyMenu(hroot);
				}
				break;
			}
			}
		}
	}
	return (INT_PTR)FALSE;
}
INT_PTR CALLBACK VThreadsDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_INITDIALOG: {
		SendMessage(hDlg, WM_SETICON, ICON_SMALL, (LPARAM)LoadIcon(hInstRs, MAKEINTRESOURCE(IDI_ICONAPP)));
		hListThreads = GetDlgItem(hDlg, IDC_THREADLIST);
		SetWindowTheme(hListThreads, L"explorer", NULL);
		ListView_SetExtendedListViewStyleEx(hListThreads, 0, LVS_EX_FULLROWSELECT | LVS_EX_TWOCLICKACTIVATE);
		SendMessage(hListThreads, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);
		LV_COLUMN lvc;
		lvc.mask = LVCF_TEXT | LVCF_WIDTH;

		lvc.pszText = str_item_contextswitch;
		lvc.cx = 60;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = str_item_state;
		lvc.cx = 160;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = str_item_modulename;
		lvc.cx = 270;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = str_item_entrypoint;
		lvc.cx = 80;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = str_item_proerty;
		lvc.cx = 60;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"Teb";
		lvc.cx = 80;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"ETHREAD";
		lvc.cx = 80;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"ID";
		lvc.cx = 60;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		hListHeader = MListViewGetHeaderControl(hListThreads);
		SendMessage(hDlg, WM_COMMAND, 16765, 0);
		return 0;
	}
	case WM_SYSCOMMAND: {
		if (wParam == SC_CLOSE) {
			EndDialog(hDlg, 0);
		}
		return 0;
	}
	case WM_COMMAND:
		switch (wParam)
		{
		case 16765:
			ListView_DeleteAllItems(hListThreads);
			if (MAppVThreads(currentPid, hDlg, curretProcName) == 1) {
				ShowWindow(hListThreads, SW_SHOW);
				ShowWindow(GetDlgItem(hDlg, IDC_TITLE), SW_HIDE);
			}
			else {
				ShowWindow(hListThreads, SW_HIDE);
				ShowWindow(GetDlgItem(hDlg, IDC_TITLE), SW_SHOW);
			}
			break;
		case IDC_REFESH:
			SetCursor(hCurLoading);
			ListView_DeleteAllItems(hListThreads);
			if (MAppVThreads(currentPid, hDlg, curretProcName) == 1) {
				ShowWindow(hListThreads, SW_SHOW);
				ShowWindow(GetDlgItem(hDlg, IDC_TITLE), SW_HIDE);
			}
			else {
				ShowWindow(hListThreads, SW_HIDE);
				ShowWindow(GetDlgItem(hDlg, IDC_TITLE), SW_SHOW);
			}
			break;
		case IDCANCEL:
			SendMessage(hDlg, WM_SYSCOMMAND, SC_CLOSE, NULL);
			break;
		case ID_THREADMENU_KILLTHREAD:
			if (KillThread())
				SendMessage(hDlg, WM_COMMAND, 16765, 0);
			break;
		case ID_THREADMENU_KILLKERNEL:
			KillThreadKernel();
			SendMessage(hDlg, WM_COMMAND, 16765, 0);
			break;
		case ID_THREADMENU_RESUTHREAD:
			if (ResusemeThread())
				SendMessage(hDlg, WM_COMMAND, 16765, 0);
			break;
		case ID_THREADMENU_SURTHREAD:
			if (SuspendThread())
				SendMessage(hDlg, WM_COMMAND, 16765, 0);
			break;
		case  ID_THREADMENU_FILEPROP: {
			TCHAR path[MAX_PATH];
			ListView_GetItemText(hListThreads, ListView_GetSelectionMark(hListThreads), 5, path, MAX_PATH);
			MShowFileProp(path);
			break;
		}
		case ID_THREADMENU_OPENPATH:
			TCHAR path[MAX_PATH];
			ListView_GetItemText(hListThreads, ListView_GetSelectionMark(hListThreads), 5, path, MAX_PATH);
			if (!StrEqual(path, L""))
			{
				std::wstring buf = FormatString(L"/select,%s", path);
				ShellExecuteW(NULL, NULL, L"explorer.exe", buf.c_str(), NULL, SW_SHOWDEFAULT);
			}
			else MessageBox(hDlg, str_item_cantgetpath, L"", MB_ICONERROR | MB_OK);
			break;
		}
		break;
	case WM_SIZE: {
		RECT rc;
		GetClientRect(hDlg, &rc);
		MoveWindow(hListThreads, 0, 0, rc.right - rc.left, rc.bottom - rc.top, TRUE);
		MoveWindow(GetDlgItem(hDlg, IDC_TITLE), 0, 0, rc.right - rc.left, rc.bottom - rc.top, TRUE);
		break;
	}
	case WM_NOTIFY:
		switch (LOWORD(wParam))
		{
		case IDC_THREADLIST:
			switch (((LPNMHDR)lParam)->code)
			{
			case LVN_KEYDOWN: {
				LPNMLVKEYDOWN lpnmk = (LPNMLVKEYDOWN)lParam;
				if (lpnmk->wVKey == 93)
				{
					TCHAR id[20];
					ListView_GetItemText(hListThreads, ListView_GetSelectionMark(hListThreads), 0, id, 20);
					selectItem1 = _wtoi(id);
					HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_THREADMENU));
					if (hroot) {
						HMENU hpop = GetSubMenu(hroot, 0);
						POINT pt;
						GetCursorPos(&pt);
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
				break;
			}
			case LVN_COLUMNCLICK: {
				if (oldSortItem != ((LPNMLISTVIEW)lParam)->iSubItem)
				{
					MListViewSetColumnSortArrow(hListHeader, oldSortItem, false, true);
					sortAscending = true;
					oldSortItem = ((LPNMLISTVIEW)lParam)->iSubItem;
					MListViewSetColumnSortArrow(hListHeader, oldSortItem, sortAscending, false);
				}
				else {
					sortAscending = !sortAscending;
					MListViewSetColumnSortArrow(hListHeader, oldSortItem, sortAscending, false);
				}
				ListView_SortItemsEx(hListThreads, Sort_VTHREADS, ((LPNMLISTVIEW)lParam)->iSubItem);
				break;
			}
			case NM_CLICK: {
				TCHAR id[20];
				ListView_GetItemText(hListThreads, ListView_GetSelectionMark(hListThreads), 0, id, 20);
				selectItem1 = _wtoi(id);
				break;
			}
			case NM_RCLICK: {
				TCHAR id[20];
				ListView_GetItemText(hListThreads, ListView_GetSelectionMark(hListThreads), 0, id, 20);
				selectItem1 = _wtoi(id);
				HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_THREADMENU));
				if (hroot) {
					HMENU hpop = GetSubMenu(hroot, 0);
					POINT pt;
					GetCursorPos(&pt);
					TrackPopupMenu(hpop,
						TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
						pt.x,
						pt.y,
						0,
						hDlg,
						NULL);
					DestroyMenu(hroot);
				}
				break;
			}

			}
		}
	}
	return (INT_PTR)FALSE;
}
#include "stdafx.h"
#include "vprocx.h"
#include "resource.h"
#include "mapphlp.h"
#include "prochlp.h"
#include "thdhlp.h"
#include "ntdef.h"

#include <list>

typedef void(__stdcall *EnumWinsCallBack)(HWND hWnd, HWND hWndParent);

DWORD currentPid = 0;
int selectItem2 = 0, selectItem1 = 0, currentShowThreadPid = 0, winscount = 0, winicoidx = 0, threadscount = 0;
HWND selectItem3 = 0;

HWND hListModuls = NULL, hListWins = NULL, hListThreads = NULL, hListHandles = NULL;
HMODULE selectHModule = NULL;
extern HANDLE hMainDevice;
extern HINSTANCE hInst;
LPWSTR curretProcName = 0;
HIMAGELIST hImgListWinSm;
HCURSOR hCurLoading;

using namespace std;

list<HWND> *hAllWins = new list<HWND>();

extern NtUnmapViewOfSectionFun NtUnmapViewOfSection;
extern EnumWinsCallBack hEnumWinsCallBack;
extern EnumWinsCallBack hGetWinsWinsCallBack;

extern BOOL ShowMainCore(HWND hWndParent);

DWORD GetPId4Name(const wchar_t *pszProcessName)
{
	DWORD id = 0;
	//获得系统快照句柄 (通俗的讲, 就是得到当前的所有进程)   
	HANDLE hSnapShot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	PROCESSENTRY32 pInfo; //用于保存进程信息的一个数据结构   
	pInfo.dwSize = sizeof(pInfo);
	//从快照中获取进程列表   
	Process32First(hSnapShot, &pInfo); //从第一个进程开始循环   
	do
	{
		//这里的 pszProcessName 为你的进程名称   

		_wcslwr_s(pInfo.szExeFile);
		if (lstrcmp(pInfo.szExeFile, pszProcessName) == 0)
		{
			id = pInfo.th32ProcessID;
			break;
		}
	} while (Process32Next(hSnapShot, &pInfo) != FALSE);
	CloseHandle(hSnapShot);
	return id; //id 就是你要的进程PID 了..   
}

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

void ThrowErrorAndErrorCode(DWORD code, LPWSTR msg, LPWSTR title)
{
	wchar_t errcode[260];
	wsprintf(errcode, L"Code: %d\nNTSTATUS:0x%lX", code, code);

	LPWSTR l = MStrAddW(msg, errcode);
	MessageBox(NULL, l, title, MB_OK | MB_ICONERROR);
	delete l;
}

BOOL FreeLibraryEx()
{
	TCHAR path[MAX_PATH];
	ListView_GetItemText(hListModuls, selectItem2, 0, path, MAX_PATH);
	LPCSTR szModuleName = W2A(path);
	HANDLE hProcess;
	DWORD rs = MOpenProcessNt(currentPid, &hProcess);
	if (rs == -1) {
		MessageBox(NULL, L"无法Free模块，进程已退出。", L"卸载模块错误", MB_OK | MB_ICONWARNING);
	}
	else if (hProcess) {
		TCHAR address[128];
		ListView_GetItemText(hListModuls, selectItem2, 2, address, 128);
		long long moduleBaseAddr = MHexStrToLongW(address);
#ifndef _AMD64_
		DWORD rs2 = NtUnmapViewOfSection(hProcess, (PVOID)moduleBaseAddr);
#else
		long long rs2 = NtUnmapViewOfSection(hProcess, (PVOID)moduleBaseAddr);
#endif
		if (rs2 == 0)
			return TRUE;
		else if (rs2 == 0xC000010A)
			MessageBox(NULL, L"无法Free模块，进程已退出。", L"卸载模块错误", MB_OK | MB_ICONWARNING);
		else {
			ThrowErrorAndErrorCode(rs2, L"无法Free模块，错误代码：", L"卸载模块错误");
		}
	}
	else {
		ThrowErrorAndErrorCode(rs, L"无法打开进程，错误代码：", L"卸载模块错误");
	}
	return FALSE;
}

void KillThreadKernel(bool a = false);

bool KillThread()
{
	if (MShowMessageDialog(NULL, L"那么，确定强制结束线程？", L"警告", L"不建议强制结束线程，因为结束线程极易发生死锁等其他问题，如果您不是在调试程序，请不要使用该方法。", MB_ICONWARNING, MB_YESNO) == IDNO)
		return false;
	HANDLE hThread;
	DWORD rs = MOpenThreadNt(selectItem1, &hThread, currentShowThreadPid);
	if (rs == -1) {
		MessageBox(NULL, L"无法结束线程，无效线程。", L"结束线程", MB_OK);
		return true;
	}
	if (hThread) {
		rs = MTerminateThreadNt(hThread);
		if (rs == 1)
			return true;
		else {
			
		}
		ThrowErrorAndErrorCode(rs, L"无法结束线程。\n错误代码：", L"结束线程");
		return true;
	}
	else {
		
	}
	ThrowErrorAndErrorCode(rs, L"无法打开线程。\n错误代码：", L"结束线程");
	return false;
}

void KillThreadKernel(bool a)
{
	if (!a)
		if (MShowMessageDialog(NULL, L"那么，确定强制结束线程？", L"警告", L"不建议强制结束线程，因为结束线程极易发生死锁等其他问题，如果您不是在调试程序，请不要使用该方法。", MB_ICONWARNING, MB_YESNO) == IDNO)
			return;
	HANDLE hThread;
	int rs = MOpenThreadNt(selectItem1, &hThread, currentShowThreadPid);
	if (rs == -1)
		MessageBox(NULL, L"无法结束线程，无效线程。", L"内核结束线程", MB_OK);
	else {
		MessageBox(NULL, L"无法强制结束线程。\n内核没有启动。", L"内核结束线程", MB_OK | MB_ICONERROR);
	}
}

bool SuspendThread()
{
	if (MShowMessageDialog(NULL, L"", L"警告", L"不建议强制挂起线程，因为挂起线程极易发生死锁等其他问题，如果您不是在调试程序，请不要使用该方法。\n那么，确定强制挂起线程？", MB_ICONWARNING, MB_YESNO) == IDNO)
		return false;
	HANDLE hThread;
	DWORD rs = MOpenThreadNt(selectItem1, &hThread, currentShowThreadPid);
	if (rs == -1) {
		MessageBox(NULL, L"无法挂起线程，无效线程。", L"暂停线程运行", MB_OK);
		return true;
	}
	if (hThread) {
		rs = MSuspendThreadNt(hThread);
		if (rs == 1) {
			return true;
		}
		else ThrowErrorAndErrorCode(rs, L"无法挂起线程，\n错误代码：", L"暂停线程运行");
	}
	else ThrowErrorAndErrorCode(rs, L"无法打开线程。\n错误代码：", L"暂停线程运行");
	return false;
}

bool ResusemeThread()
{
	HANDLE hThread;
	DWORD rs = MOpenThreadNt(selectItem1, &hThread, currentShowThreadPid);
	if (rs == -1) {
		MessageBox(NULL, L"无法取消挂起线程，无效线程。", L"继续线程运行", MB_OK);
		return true;
	}
	if (hThread) {
		rs = MResumeThreadNt(hThread);
		if (rs == 1) {
			return true;
		}
		else ThrowErrorAndErrorCode(rs, L"无法继续线程运行，\n错误代码：", L"暂停线程运行");
	}
	else ThrowErrorAndErrorCode(rs, L"无法打开线程。\n错误代码：", L"继续线程运行");
	return false;
}

BOOL CALLBACK lpEnumFunc(HWND hWnd, LPARAM lParam)
{
	DWORD processId;
	GetWindowThreadProcessId(hWnd, &processId);
	if (processId == lParam)
	{
		BOOL visible = IsWindowVisible(hWnd);
		LVITEM vitem;
		vitem.mask = LVIF_IMAGE | LVIF_TEXT;
		vitem.iSubItem = 0;
		vitem.iItem = 0;
		vitem.iSubItem = 0;
		HICON hIcon = (HICON)SendMessage(hWnd, WM_GETICON, ICON_SMALL, 0);
		if (hIcon)
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
		wsprintf(handle, L"%08X", hWnd);
		vitem.pszText = handle;
		ListView_SetItem(hListWins, &vitem);
		vitem.iSubItem++;
		WCHAR clsmame[128];
		GetClassName(hWnd, clsmame, 128);
		vitem.pszText = clsmame;
		ListView_SetItem(hListWins, &vitem);
		vitem.iSubItem++;
		if (visible)
			vitem.pszText = L"可见";
		else
			vitem.pszText = L"-";
		ListView_SetItem(hListWins, &vitem);
		vitem.iSubItem++;
		vitem.pszText = L"-";
		ListView_SetItem(hListWins, &vitem);
		winscount++;
	}
	return TRUE;
}
BOOL CALLBACK lpEnumFunc2(HWND hWnd, LPARAM lParam)
{
	if (IsWindowVisible(hWnd))
	{
		long l = GetWindowLong(hWnd, GWL_EXSTYLE);
		long ls = GetWindowLong(hWnd, GWL_STYLE);

		wchar_t clsn[50];
		GetClassName(hWnd, clsn, 50);
		if (wcscmp(clsn, L"ApplicationFrameWindow") != 0)
		{
			if ((l & WS_EX_APPWINDOW) == WS_EX_APPWINDOW || (l & WS_EX_WINDOWEDGE) == WS_EX_WINDOWEDGE)
				hAllWins->push_back(hWnd);
			else if ((ls & WS_CAPTION) == WS_CAPTION)
				hAllWins->push_back(hWnd);
		}
	}
	return TRUE;
}

M_API BOOL MAppVProcessAllWindowsGetProcessWindow(DWORD pid)
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
			if (hGetWinsWinsCallBack)hGetWinsWinsCallBack(*it, (HWND)&text);
			else return FALSE;
		}
	}
	return TRUE;
}

M_API BOOL MAppVProcessMsg(DWORD dwPID, HWND hDlg, int type, LPWSTR procName)
{
	currentPid = dwPID;	curretProcName = procName;
	if (type == 1)
		DialogBoxW(hInst, MAKEINTRESOURCE(IDD_VTHEADS), hDlg, VThreadsDlgProc);
	else if (type == 2)
		DialogBoxW(hInst, MAKEINTRESOURCE(IDD_VMODULS), hDlg, VModulsDlgProc);
	else if (type == 3)
		DialogBoxW(hInst, MAKEINTRESOURCE(IDD_VWINS), hDlg, VWinsDlgProc);
	else return FALSE;

	/*MSG msg;
	while (GetMessage(&msg, hWnd, 0, 0))
	{
	TranslateMessage(&msg);
	DispatchMessage(&msg);
	}*/
	return TRUE;
}
M_API BOOL MAppVProcessModuls(DWORD dwPID, HWND hDlg, LPWSTR procName)
{
	currentPid = dwPID;	curretProcName = procName;
	DialogBoxW(hInst, MAKEINTRESOURCE(IDD_VMODULS), hDlg, VModulsDlgProc);
	return TRUE;
}
M_API BOOL MAppVProcessThreads(DWORD dwPID, HWND hDlg, LPWSTR procName)
{
	currentPid = dwPID;	curretProcName = procName;
	DialogBoxW(hInst, MAKEINTRESOURCE(IDD_VTHEADS), hDlg, VThreadsDlgProc);
	return TRUE;
}
M_API BOOL MAppVProcessWindows(DWORD dwPID, HWND hDlg, LPWSTR procName)
{
	currentPid = dwPID;	curretProcName = procName;
	DialogBoxW(hInst, MAKEINTRESOURCE(IDD_VWINS), hDlg, VWinsDlgProc);
	return TRUE;
}
M_API BOOL MAppVProcess(HWND hWndParent)
{
	return ShowMainCore(hWndParent);
}
M_API BOOL MAppVProcessAllWindows()
{
	hAllWins->clear();
	HDESK hDesk = OpenDesktop(L"Default", 0, FALSE, DESKTOP_ENUMERATE);
	if (EnumDesktopWindows(hDesk, lpEnumFunc2, NULL))
	{
		return TRUE;
	}
	return 0;
}

BOOL MAppVModuls(DWORD dwPID, HWND hDlg, LPWSTR procName)
{
	BOOL bFound = FALSE;
	HANDLE hProcess;
	HWND htitle = GetDlgItem(hDlg, IDC_TITLE);
	int rs = MOpenProcessNt(dwPID, &hProcess);
	if (rs == -1) {
		SetWindowText(htitle, L"枚举模块失败。\n无效进程。");
		return -1;
	}
	else if (rs == 1 && hProcess) {

		BOOL bRet = FALSE;
		HANDLE hModuleSnap = NULL;
		MODULEENTRY32 me32 = { 0 };


		hModuleSnap = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, dwPID);
		if (hModuleSnap == INVALID_HANDLE_VALUE) {
			if (GetLastError() == 299) SetWindowText(htitle, L"枚举模块失败。\n无效进程。");
			else SetWindowText(htitle, MStrAddW(MStrAddW(L"枚举模块失败。\n错误代码:", MIntToStrW(GetLastError())), L"。"));
			return FALSE;
		}

		me32.dwSize = sizeof(MODULEENTRY32);
		int i = 0;
		BOOL fOk;
		for (fOk = Module32First(hModuleSnap, &me32); fOk; fOk = Module32Next(hModuleSnap, &me32)) {
			LVITEM vitem;
			vitem.mask = LVIF_TEXT;
			vitem.iSubItem = 0;
			vitem.iItem = 0;
			vitem.iSubItem = 0;
			vitem.lParam = (LPARAM)me32.hModule;
			vitem.pszText = me32.szModule;
			ListView_InsertItem(hListModuls, &vitem);
			TCHAR mpath[MAX_PATH];
			vitem.iSubItem = 1;
			GetModuleFileNameExW(hProcess, me32.hModule, mpath, MAX_PATH);
			vitem.pszText = mpath;
			vitem.pszText = me32.szExePath;
			ListView_SetItem(hListModuls, &vitem);
			vitem.iSubItem = 2;
			TCHAR addr[20];
			wsprintf(addr, L"0x%X", me32.modBaseAddr);
			vitem.pszText = addr;
			ListView_SetItem(hListModuls, &vitem);
			vitem.iSubItem = 3;
			TCHAR sz[20];
			wsprintf(sz, L"%d", me32.modBaseSize);
			vitem.pszText = sz;
			ListView_SetItem(hListModuls, &vitem);
			vitem.iSubItem = 4;
			WCHAR company[256];
			if (MGetExeCompany(mpath, company, 255))
				vitem.pszText = company;
			else vitem.pszText = L"";

			ListView_SetItem(hListModuls, &vitem);

			i++;
		}

		LPWSTR str1 = L"进程 ";
		LPWSTR str2 = MStrAdd(str1, procName);
		LPWSTR str3 = MStrAdd(str2, L"[%d] 的所有模块：%d");
		delete str2;
		wchar_t text[1024];
		wsprintf(text, str3, dwPID, i);
		SetWindowText(hDlg, text);
		delete str3;
		CloseHandle(hModuleSnap);
		return TRUE;
	}
	else {
		SetWindowText(htitle, MStrAddW(L"无法枚举模块，无法打开进程。\n错误码：", MIntToStrW(rs)));
		return 0;
	}
}

BOOL MAppVThreads(DWORD dwPID, HWND hDlg, LPWSTR procName)
{
	BOOL bFound = FALSE;
	HANDLE hProcess;
	HWND htitle = GetDlgItem(hDlg, IDC_TITLE);
	DWORD rs = MOpenProcessNt(dwPID, &hProcess);
	if (rs == -1) {
		SetWindowText(htitle, L"枚举线程失败。\n无效进程。");
		return -1;
	}
	else if (hProcess && rs == 1) {
		HANDLE hSnapThread = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, dwPID);
		if (INVALID_HANDLE_VALUE != hSnapThread)
		{
			THREADENTRY32 th32;
			th32.dwSize = sizeof(THREADENTRY32);
			int i = 0;
			BOOL fOk;
			for (fOk = Thread32First(hSnapThread, &th32); fOk; fOk = Thread32Next(hSnapThread, &th32)) {
				if (th32.th32OwnerProcessID == dwPID)
				{
					LVITEM vitem;
					vitem.mask = LVIF_TEXT;
					vitem.iSubItem = 0;
					vitem.iItem = 0;
					vitem.iSubItem = 0;
					wchar_t c[16];
					wsprintf(c, L"%d", th32.th32ThreadID);
					vitem.pszText = c;
					ListView_InsertItem(hListThreads, &vitem);
					vitem.iSubItem++;
					vitem.pszText = L"-";
					ListView_SetItem(hListThreads, &vitem);

					vitem.iSubItem++;
					LPWSTR result0 = 0;
					if (MGetThreadInfoNt(th32.th32ThreadID, 3, &result0))
						vitem.pszText = result0;
					else 
						vitem.pszText = L"!3";

					ListView_SetItem(hListThreads, &vitem);
					vitem.iSubItem++;

					WCHAR basePri[10];
					wsprintf(basePri, L"%d", th32.tpBasePri);
					vitem.pszText = basePri;
					ListView_SetItem(hListThreads, &vitem);
					vitem.iSubItem++;
					LPWSTR result = 0;
					if (MGetThreadInfoNt(th32.th32ThreadID, 2, &result))
						vitem.pszText = result;
					else vitem.pszText = L"!2";
					ListView_SetItem(hListThreads, &vitem);
					vitem.iSubItem++;
					wchar_t result1[260];
					wchar_t modname[260];
					LPWSTR rs = 0;
					if (MGetThreadInfoNt(th32.th32ThreadID, 1, &rs)) {
						lstrcpy(result1, rs);
						MDosPathToNtPath(result1, modname);
						vitem.pszText = modname;
					}
					else vitem.pszText = L"!1";
					ListView_SetItem(hListThreads, &vitem);
					vitem.iSubItem++;
					THREAD_STATE ts = (THREAD_STATE)MGetThreadState(th32.th32OwnerProcessID, th32.th32ThreadID);
					switch (ts)
					{
					case THREAD_STATE::StateReady:
						vitem.pszText = L"就绪(StateReady)";
						break;
					case THREAD_STATE::StateRunning:
						vitem.pszText = L"运行(StateRunning)";
						break;
					case THREAD_STATE::StateWait:
						vitem.pszText = L"等待(StateWait)";
						break;
					case THREAD_STATE::StateTerminated:
						vitem.pszText = L"终止(StateTerminated)";
						break;
					case 9:
						vitem.pszText = L"等待(StateWait 已挂起)";
						break;
					default:
						vitem.pszText = L"0";
						break;
					}
					ListView_SetItem(hListThreads, &vitem);
					vitem.iSubItem++;
					LPWSTR result4 = 0;
					if (MGetThreadInfoNt(th32.th32ThreadID, 4, &result4))
						vitem.pszText = result4;
					else vitem.pszText = L"!2";
					ListView_SetItem(hListThreads, &vitem);
					i++;
				}
				threadscount = i;
			}

			LPWSTR str1 = L"进程 ";
			LPWSTR str2 = MStrAdd(str1, procName);
			LPWSTR str3 = MStrAdd(str2, L" [%d] 的所有线程：%d");
			delete str2;
			wchar_t text[512];
			wsprintf(text, str3, dwPID, threadscount);
			SetWindowText(hDlg, text);
			delete str3;			
			currentShowThreadPid = dwPID;
			CloseHandle(hSnapThread);

			return TRUE;
		}
		else
		{
			LPWSTR l1 = MIntToStrW(GetLastError());
			LPWSTR l2 = MStrAddW(L"枚举线程失败。\n错误代码:", l1);
			LPWSTR l3 = MStrAddW(l2, L"。");
			SetWindowText(htitle, l3);
			delete l1; delete l2; delete l3;
			return FALSE;
		}
	}
	else
	{
		LPWSTR l1 = MIntToStrW(rs);
		LPWSTR l = MStrAddW(L"枚举线程失败。\n无法打开进程。\n错误代码:", l1);
		SetWindowText(htitle, l);
		delete l1;
		delete l;
		return FALSE;
	}
}

BOOL MAppVWins(DWORD dwPID, HWND hDlg, LPWSTR procName)
{
	winscount = 0; winicoidx = 2;
	ImageList_Destroy(hImgListWinSm);
	hImgListWinSm = ImageList_Create(16, 16, ILC_COLOR32, 1, 0);
	HICON hIcoDefH, hIcoDefS;
	hIcoDefS = LoadIcon(hInst, MAKEINTRESOURCE(IDI_ICONACTIVEDWIN));
	hIcoDefH = LoadIcon(hInst, MAKEINTRESOURCE(IDI_ICONHIDEDWIN));
	ImageList_AddIcon(hImgListWinSm, hIcoDefS);
	ImageList_AddIcon(hImgListWinSm, hIcoDefH);
	ListView_SetImageList(hListWins, hImgListWinSm, LVSIL_SMALL);
	HWND htitle = GetDlgItem(hDlg, IDC_RESULT);
	if (EnumWindows(lpEnumFunc, dwPID))
	{
		LPWSTR str1 = L"进程 ";
		LPWSTR str2 = MStrAdd(str1, procName);
		LPWSTR str3 = MStrAdd(str2, L"[%d] 的所有窗口：%d");
		delete str2;
		wchar_t text[1024];
		wsprintf(text, str3, dwPID, winscount);
		SetWindowText(hDlg, text);
		SetWindowText(htitle, text);
		delete str3;
		return TRUE;
	}
	return FALSE;
}


INT_PTR CALLBACK VWinsDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_INITDIALOG:
		SendMessage(hDlg, WM_SETICON, ICON_SMALL, (LPARAM)LoadIcon(hInst, MAKEINTRESOURCE(IDI_ICONAPP)));
		hListWins = GetDlgItem(hDlg, IDC_WINSLIST);
		LV_COLUMN lvc;
		lvc.mask = LVCF_TEXT | LVCF_WIDTH;
		lvc.pszText = L"所属线程";
		lvc.cx = 60;
		SendMessageW(hListWins, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"可见";
		lvc.cx = 40;
		SendMessage(hListWins, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"窗口类名";
		lvc.cx = 150;
		SendMessage(hListWins, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"窗口句柄";
		lvc.cx = 85;
		SendMessage(hListWins, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"窗口文字";
		lvc.cx = 180;
		SendMessage(hListWins, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		ListView_SetExtendedListViewStyleEx(hListWins, 0, LVS_EX_FULLROWSELECT | LVS_EX_TWOCLICKACTIVATE);
		MAppVWins(currentPid, hDlg, curretProcName);
		SetWindowTheme(hListWins, L"explorer", NULL);
		ListView_SetExtendedListViewStyleEx(hListWins, 0, LVS_EX_FULLROWSELECT | LVS_EX_TWOCLICKACTIVATE);
		SendMessage(hListWins, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);
		break;
	case WM_SYSCOMMAND:
		if (wParam == SC_CLOSE) {
			EndDialog(hDlg, 0);
		}
		return 0;
	case WM_COMMAND:
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
				LPWSTR text = L"可见";
				ListView_SetItemText(hListWins, ListView_GetSelectionMark(hListWins), 3, text);
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
			case LVN_COLUMNCLICK:
			{

			}
			case NM_CLICK: {
				WCHAR hwnd[32];
				ListView_GetItemText(hListWins, ListView_GetSelectionMark(hListWins), 1, hwnd, 32);
				selectItem3 = (HWND)LongToHandle(MHexStrToIntW(hwnd));
				break;
			}
			case NM_RCLICK:
			{
				WCHAR hwnd[32];
				ListView_GetItemText(hListWins, ListView_GetSelectionMark(hListWins), 1, hwnd, 32);
				selectItem3 = (HWND)LongToHandle(MHexStrToIntW(hwnd));
				HMENU hroot = LoadMenu(hInst, MAKEINTRESOURCE(IDR_WINSMENU));
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
	case WM_INITDIALOG:
		SendMessage(hDlg, WM_SETICON, ICON_SMALL, (LPARAM)LoadIcon(hInst, MAKEINTRESOURCE(IDI_ICONAPP)));
		hListModuls = GetDlgItem(hDlg, IDC_MODULLIST);
		LV_COLUMN lvc;
		lvc.mask = LVCF_TEXT | LVCF_WIDTH;
		lvc.pszText = L"文件公司";
		lvc.cx = 130;
		SendMessage(hListModuls, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"大小";
		lvc.cx = 80;
		SendMessage(hListModuls, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"地址";
		lvc.cx = 80;
		SendMessage(hListModuls, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"模块路径";
		lvc.cx = 300;
		SendMessageW(hListModuls, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"模块名";
		lvc.cx = 80;
		SendMessage(hListModuls, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		SetWindowTheme(hListModuls, L"explorer", NULL);
		ListView_SetExtendedListViewStyleEx(hListModuls, 0, LVS_EX_FULLROWSELECT | LVS_EX_TWOCLICKACTIVATE);
		SendMessage(hListModuls, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);
		if (MAppVModuls(currentPid, hDlg, curretProcName) == 1) {
			ShowWindow(hListModuls, SW_SHOW);
			ShowWindow(GetDlgItem(hDlg, IDC_TITLE), SW_HIDE);
		}
		else {
			ShowWindow(hListModuls, SW_HIDE);
			ShowWindow(GetDlgItem(hDlg, IDC_TITLE), SW_SHOW);
		}
		return 0;
	case WM_SYSCOMMAND:
		if (wParam == SC_CLOSE) {
			EndDialog(hDlg, 0);
		}
		return 0;
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
				MessageBox(NULL, L"卸载模块成功。", L"", MB_OK);
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
			if (!MStrEqualW(path, L""))
			{
				LPWSTR lparm = MStrAddW(L"/select,", path);
				ShellExecuteW(NULL, NULL, L"explorer.exe", lparm, NULL, SW_SHOWDEFAULT);
			}
			else MessageBox(hDlg, L"无法获取路径。", L"错误", MB_ICONERROR | MB_OK);
			break;
		}
		break;
	case WM_SIZE:
	{
		RECT rc;
		GetClientRect(hDlg, &rc);
		MoveWindow(hListModuls, 0, 0, rc.right - rc.left, rc.bottom - rc.top, TRUE);
		MoveWindow(GetDlgItem(hDlg, IDC_TITLE), 0, 0, rc.right - rc.left, rc.bottom - rc.top, TRUE);
	}
	break;
	case WM_NOTIFY:
		switch (LOWORD(wParam))
		{
		case IDC_MODULLIST:
			switch (((LPNMHDR)lParam)->code)
			{
			case LVN_COLUMNCLICK:
			{

			}
			case NM_CLICK:
				selectItem2 = ListView_GetSelectionMark(hListModuls);
				selectHModule = (HMODULE)GetItemData(hListModuls, ListView_GetSelectionMark(hListModuls));
				break;
			case NM_RCLICK:
			{
				selectItem2 = ListView_GetSelectionMark(hListModuls);
				selectHModule = (HMODULE)GetItemData(hListModuls, ListView_GetSelectionMark(hListModuls));
				HMENU hroot = LoadMenu(hInst, MAKEINTRESOURCE(IDR_MODULSMENU));
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
	case WM_INITDIALOG:
		SendMessage(hDlg, WM_SETICON, ICON_SMALL, (LPARAM)LoadIcon(hInst, MAKEINTRESOURCE(IDI_ICONAPP)));
		hListThreads = GetDlgItem(hDlg, IDC_THREADLIST);
		SetWindowTheme(hListThreads, L"explorer", NULL);
		ListView_SetExtendedListViewStyleEx(hListThreads, 0, LVS_EX_FULLROWSELECT | LVS_EX_TWOCLICKACTIVATE);
		SendMessage(hListThreads, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);
		LV_COLUMN lvc;
		lvc.mask = LVCF_TEXT | LVCF_WIDTH;

		lvc.pszText = L"切换次数";
		lvc.cx = 60;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"状态";
		lvc.cx = 160;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"模块";
		lvc.cx = 270;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"入口点";
		lvc.cx = 80;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"优先级";
		lvc.cx = 60;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"Teb";
		lvc.cx = 80;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"ETHREAD";
		lvc.cx = 80;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		lvc.pszText = L"线程ID";
		lvc.cx = 60;
		SendMessage(hListThreads, LVM_INSERTCOLUMN, 0, (LPARAM)&lvc);
		SendMessage(hDlg, WM_COMMAND, 16765, 0);
		return 0;
	case WM_SYSCOMMAND:
		if (wParam == SC_CLOSE) {
			EndDialog(hDlg, 0);
		}
		return 0;
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
				MessageBox(NULL, L"刷新成功。", L"", MB_OK);
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
		}
		break;
	case WM_SIZE:
	{
		RECT rc;
		GetClientRect(hDlg, &rc);
		MoveWindow(hListThreads, 0, 0, rc.right - rc.left, rc.bottom - rc.top, TRUE);
		MoveWindow(GetDlgItem(hDlg, IDC_TITLE), 0, 0, rc.right - rc.left, rc.bottom - rc.top, TRUE);
	}
	break;
	case WM_NOTIFY:
		switch (LOWORD(wParam))
		{
		case IDC_THREADLIST:
			switch (((LPNMHDR)lParam)->code)
			{
			case LVN_COLUMNCLICK:
			{

			}
			case NM_CLICK: {
				TCHAR id[20];
				ListView_GetItemText(hListThreads, ListView_GetSelectionMark(hListThreads), 0, id, 20);
				selectItem1 = _wtoi(id);
				break;
			}
			case NM_RCLICK:
			{
				TCHAR id[20];
				ListView_GetItemText(hListThreads, ListView_GetSelectionMark(hListThreads), 0, id, 20);
				selectItem1 = _wtoi(id);
				HMENU hroot = LoadMenu(hInst, MAKEINTRESOURCE(IDR_THREADMENU));
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
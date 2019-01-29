#include "stdafx.h"
#include "mapphlp.h"
#include "resource.h"
#include "prochlp.h"
#include "vprocx.h"
#include "comdlghlp.h"
#include "fmhlp.h"
#include "syshlp.h"
#include "schlp.h"
#include "lghlp.h"
#include "suact.h"
#include "starthlp.h"
#include "settinghlp.h"
#include "kernelhlp.h"
#include "VersionHelpers.h"
#include "PathHelper.h"
#include "StringHlp.h"
#include "loghlp.h"
#include "nthlp.h"
#include "userhlp.h"
#include "thdhlp.h"
#include "pehlp.h"
#include <Windowsx.h>
#include <shellapi.h>
#include <Vsstyle.h>
#include <vssym32.h>
#include <Uxtheme.h>
#include <string.h>
#include <string>
#include <dbghelp.h>
#include <mscoree.h>
#include <Metahost.h>
#include <corerror.h>
#include <comdef.h>
#pragma comment(linker,"\"/manifestdependency:type='win32' \
name = 'Microsoft.Windows.Common-Controls' version = '6.0.0.0' \
processorArchitecture = '*' publicKeyToken = '6595b64144ccf1df' language = '*'\"")

#ifdef _AMD64_
#define GWL_WNDPROC         (-4)
#pragma comment(linker,"/export:MAppWorkCall1=PCMGR64.MAppMainThreadCall")
#else
#pragma comment(linker,"/export:MAppWorkCall1=PCMGR32.MAppMainThreadCall")
#endif
#define WM_S_APPBAR 900
#define WM_S_MESSAGE_EXIT 901
#define WM_S_MESSAGE_ACTIVE 902
#define WM_S_MAINTHREAD_ACT 903

#define ID_AOP 701

//程序主版本
#define APP_VERSION L"1.3.2.6"
#define APP_BULID_DATE L"2019-2-3"

extern HINSTANCE hInst;
extern HINSTANCE hInstRs;
extern HINSTANCE hClr;
extern _CancelShutdown dCancelShutdown;
extern _MGetCurrentPeb dMGetCurrentPeb;

extern NtQuerySystemInformationFun NtQuerySystemInformation;
extern _MGetProcAddressCore MGetProcAddressCore;
extern LPWSTR fmCurrectSelectFilePath0;
extern bool fmMutilSelect;
extern int fmMutilSelectCount;
extern void* clrcreateinstance;
extern bool showConsole;
BOOL appLoadSucessfuly = FALSE;

typedef HRESULT(WINAPI*CLRCreateInstanceFun)(REFCLSID clsid, REFIID riid, LPVOID *ppInterface);

HMENU hMenuMain = NULL;
exitcallback hMainExitCallBack = NULL;
extern HWND hWndMain;

EnumWinsCallBack hEnumWinsCallBack = NULL;
GetWinsCallBack hGetWinsWinsCallBack = NULL;
CLRCreateInstanceFun _CLRCreateInstance = NULL;
WorkerCallBack hWorkerCallBack = NULL;
TerminateImporantWarnCallBack hTerminateImporantWarnCallBack = NULL;
HANDLE hMutex = NULL;

typedef struct tag_SHOWMENUPROC
{
	WCHAR FilePath[MAX_PATH];
	WCHAR FileName[MAX_PATH];
	DWORD Pid;
	HWND hWnd;
	HWND selectedhWnd;
	int data;
	int type;
	int x;
	int y;
}SHOWMENUPROC, *PSHOWMENUPROC;
typedef struct tag_SHOWMENUPROCPPER
{
	WCHAR FilePath[MAX_PATH];
	WCHAR FileName[MAX_PATH];
	DWORD Pid;
	BOOL IsImporant;
	BOOL IsVeryImporant;
}SHOWMENUPROCPPER,*PSHOWMENUPROCPPER;

WCHAR debuggerCommand[MAX_PATH];
WCHAR thisGuid[MAX_PATH];
WCHAR appDir[MAX_PATH];
WCHAR appName[MAX_PATH];
DWORD dwMainAppRet = 0;
extern WCHAR iniPath[MAX_PATH];

_MGetXFun MGetXFun;

int menu_last_x = 0,
menu_last_y = 0;
HMENU hMenuMainFile;
HMENU hMenuMainSet;
HMENU hMenuMainView;
HMENU hMenuMainViewRefeshRate;
HWND selectItem4;

WCHAR ntoskrnlPath[MAX_PATH];
WCHAR cssrssPath[MAX_PATH];
WCHAR systemRoot[MAX_PATH];

int HotKeyId = 0;
bool has_fullscreen_window = false;

bool executeByLoader = false;
extern BOOL killUWPCmdSendBack;
extern BOOL killCmdSendBack;
bool refesh_fast = false;
bool refesh_paused = false;
bool min_hide = false;
bool close_hide = false;
bool top_most = false;
bool main_grouping = false;
bool alwaysOnTop = false;
bool aopTimer = false;

bool can_debug = false;
bool use_apc = false;
int IDC_MAINLIST_HEADER = 0;
WNDPROC procListWndProc = NULL;
WNDPROC procListHeaderWndProc = NULL;
BOOL procListLock = FALSE;
HWND hListHeaderMainProcList;
CLRCreateInstanceFnPtr dCLRCreateInstance;

extern DWORD thisCommandPid;
extern LPWSTR thisCommandPath;
extern LPWSTR thisCommandName;
extern LPWSTR thisCommandUWPName;
extern BOOL thisCommandIsImporant;
extern BOOL thisCommandIsVeryImporant;
extern HANDLE thisCommandhProcess;

bool MLoadApp();
bool MLoadAppBackUp();
LONG WINAPI MUnhandledExceptionFilter(struct _EXCEPTION_POINTERS *lpExceptionInfo);
M_CAPI(BOOL) InstallMouseHook();
M_CAPI(BOOL) UninstallMouseHook();
LRESULT CALLBACK LowLevelMouseProc(INT nCode, WPARAM wParam, LPARAM lParam);

BOOL MAppLoadMenuSettings()
{
	hMenuMainViewRefeshRate = GetSubMenu(hMenuMainView, 1);

	UINT rtIndex = -1;
	WCHAR lastRt[16];
	GetPrivateProfileString(L"AppSetting", L"RefeshTime", L"", lastRt, 16, iniPath);
	if (StrEqual(lastRt, L"Stop")) {
		rtIndex = 2;
		refesh_paused = true;
	}
	else if (StrEqual(lastRt, L"Fast")) {
		rtIndex = 0;
		refesh_paused = false;
		refesh_fast = true;
	}
	else if (StrEqual(lastRt, L"Slow")) {
		rtIndex = 1;
		refesh_paused = false;
		refesh_fast = false;
	}

	if (rtIndex != -1)
		CheckMenuRadioItem(hMenuMainViewRefeshRate, 0, 2, rtIndex, MF_BYPOSITION);

	return TRUE;
}
BOOL MAppShowLoadDrvWarn() {
	if (MShowMessageDialog(hWndMain, str_item_loaddriver_warn, str_item_warn_title, str_item_loaddriver_warn_title, MB_ICONEXCLAMATION, MB_YESNO) == IDNO)
		return FALSE;
	return TRUE;
}
//Worker Calls
BOOL MAppFkillWnd(HWND hWnd) {
	HANDLE hThread;
	DWORD pid = 0, tid = GetWindowThreadProcessId(hWnd, &pid);
	if (pid == GetCurrentProcessId())return TRUE;
	if (NT_SUCCESS(MOpenThreadNt(tid, &hThread, 0)))
		return NT_SUCCESS(MTerminateThreadNt(hThread));
	return FALSE;
}
BOOL MAppStartLoadSomeZZ()
{
	return FreeLibrary(GetModuleHandle(L"mscoree.dll"));
}
BOOL MAppStartGGUID()
{
	wchar_t w1 = 1;
	memset(thisGuid, 0, sizeof(thisGuid));
	for (int i = 0; i < 32; i++)
	{
		w1 = i * 2;
		if (w1 > 16)
			w1 = i / 2 + w1;
		thisGuid[i] = w1;
	}
	return 1;
}
BOOL MAppStartTestZzX(void*v)
{
	USHORT vv[32];
	memcpy_s(vv, sizeof(vv), v, sizeof(vv));
	LPWSTR guid = (LPWSTR)v;

	int ix = guid[1] * guid[2] + guid[3];//14
	int ig = guid[ix] - ix;//21
	int pos = guid[ig] / 2 - guid[1] - guid[1] * guid[2];//16

	ULONG_PTR ptr = 0;
	memcpy_s(&ptr, sizeof(ptr), guid + pos * sizeof(USHORT), sizeof(ULONG_PTR));

	dCLRCreateInstance = (CLRCreateInstanceFnPtr)ptr;

	return executeByLoader;
}
BOOL MAppStartTestZz(void*v) 
{
	LPWSTR vs = (LPWSTR)v;
	int maxlen = vs[4] * 2;
	if ((int)wcslen(vs) >= maxlen)
	{
		if (vs[vs[0]] == vs[8] && vs[7] == maxlen)
		{
			executeByLoader = true;
			return 0;
		}
	}
	if (!executeByLoader)
		MAppStartLoadSomeZZ();
	return TRUE;
}
BOOL MAppStartEnd() {
	if(hMutex) return CloseHandle(hMutex);
	return FALSE;
}
BOOL MAppStartTryActiveLastApp(LPWSTR windowTitle) {
	HWND hWnd = FindWindow(NULL, windowTitle);
	if (IsWindow(hWnd))
	{
		if (SendMessageTimeout(hWnd, WM_S_MESSAGE_ACTIVE, NULL, NULL, SMTO_BLOCK, 500, 0) == 0)
			return FALSE;
		else return TRUE;
	}		
	return FALSE;
}
BOOL MAppKillOld(LPWSTR procName)
{
	BOOL ended = FALSE;
	PROCESSENTRY32 pe;
	HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	pe.dwSize = sizeof(PROCESSENTRY32);
	if (!Process32First(hSnapshot, &pe))
		return 0;
	while (1)
	{
		pe.dwSize = sizeof(PROCESSENTRY32);
		if (Process32Next(hSnapshot, &pe) == FALSE)
			break;
		if (StrEqual(pe.szExeFile, procName))
			if (pe.th32ProcessID != GetCurrentProcessId()) {
				Log(L"Killing old PCMgr process : %d", pe.th32ProcessID);
				ended = MTerminateProcessNt(pe.th32ProcessID, NULL) == STATUS_SUCCESS;
			}
	}
	CloseHandle(hSnapshot);
	return ended;
}
BOOL MAppStartTest()
{
	hMutex = CreateMutex(NULL, false, L"PCMGR");
	if (GetLastError() == ERROR_ALREADY_EXISTS)
	{
		CloseHandle(hMutex);
		return TRUE;
	}
	return FALSE;
}
BOOL MAppStartShowRun2Warn()
{
	WCHAR run2Tilte[16];
	LoadString(hInstRs, IDS_STRING_RUN2TITLE, run2Tilte, 16);
	WCHAR run2Text[32];
	LoadString(hInstRs, IDS_STRING_RUN2TEXT, run2Text, 32);

	WCHAR run2Con[16];
	LoadString(hInstRs, IDS_STRING_CONRUN, run2Con, 16);
	WCHAR run2Can[16];
	LoadString(hInstRs, IDS_STRING_CANRUN, run2Can, 16);
	WCHAR run2Killold[16];
	LoadString(hInstRs, IDS_STRING_KILLOLD, run2Killold, 16);
	WCHAR run2KilloldFailed[32];
	LoadString(hInstRs, IDS_STRING_KILLOLDFAILED, run2KilloldFailed, 32);

	TASKDIALOGCONFIG cfg = { 0 };
	cfg.cbSize = sizeof(TASKDIALOGCONFIG);
	cfg.hInstance = hInst;
	cfg.dwFlags = TDF_USE_COMMAND_LINKS_NO_ICON;
	cfg.pszWindowTitle = DEFDIALOGGTITLE;
	cfg.pszMainInstruction = run2Tilte;
	cfg.pszContent = run2Text;

	TASKDIALOG_BUTTON btn[3] = { 0 };
	btn[0] = TASKDIALOG_BUTTON{ 1, run2Can };
	btn[1] = TASKDIALOG_BUTTON{ 2, run2Con };
	btn[2] = TASKDIALOG_BUTTON{ 3, run2Killold };
	cfg.pButtons = btn;
	cfg.cButtons = 3;

	int selectButton = 0;
	if(SUCCEEDED(TaskDialogIndirect(&cfg, &selectButton, NULL, NULL)))
	{
		if (selectButton == 1)
			return TRUE;
		else if (selectButton == 3)
		{
			if (!MAppKillOld(appName))
				MessageBox(NULL, run2KilloldFailed, DEFDIALOGGTITLE, MB_ICONWARNING);
			MAppStartTest();
		}
	}
	return FALSE;
}
M_API void MAppWorkCall2(UINT msg, WPARAM wParam, LPARAM lParam)
{
	if (!executeByLoader) {
		msg = 0;
		wParam = NULL;
	}
	SendMessage(hWndMain, msg, wParam, lParam);
}
M_API int MAppWorkCall3(int id, HWND hWnd, void*data)
{
	if (!executeByLoader) {
		id = 0;
		data = NULL;
	}
	switch (id)
	{
	case 156: {
		alwaysOnTop = static_cast<int>((ULONG_PTR)data);
		if (top_most) {
			if (alwaysOnTop)
				MAppMainCall(M_CALLBACK_SW_AOP_WND, (LPVOID)TRUE, (LPVOID)0);
			else
				MAppMainCall(M_CALLBACK_SW_AOP_WND, (LPVOID)FALSE, (LPVOID)0);
		}
		CheckMenuItem(hMenuMainSet, IDM_ALWAYSTOP, alwaysOnTop ? MF_CHECKED : MF_UNCHECKED);
		M_CFG_SetConfigBOOL(L"AppSetting", L"AlwaysOnTop", alwaysOnTop);
		hWorkerCallBack(M_CALLBACK_SWITCH_IDM_ALWAYSTOP_SET, (LPVOID)static_cast<ULONG_PTR>(alwaysOnTop), 0);
		break;
	}
	case 157: hWndMain = hWnd; break;
	case 158: return MCreateMiniDumpForProcess((DWORD)(ULONG_PTR)data);
	case 159: return MDetachFromDebuggerProcess((DWORD)(ULONG_PTR)data);
	case 160: {
		ListView_SetExtendedListViewStyleEx(hWnd, 0, LVS_EX_FULLROWSELECT | LVS_EX_TWOCLICKACTIVATE | LVS_EX_HEADERDRAGDROP);
		SendMessage(hWnd, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);
		break;
	}
	case 161: return IsMinimized(hWnd);
	case 162: {
		main_grouping = static_cast<int>((ULONG_PTR)hWnd);
		CheckMenuItem(hMenuMainView, IDM_GROUP, main_grouping ? MF_CHECKED : MF_UNCHECKED);
		M_CFG_SetConfigBOOL(L"AppSetting", L"MainGrouping", main_grouping);
		hWorkerCallBack(M_CALLBACK_SWITCH_MAINGROUP_SET, (LPVOID)static_cast<ULONG_PTR>(main_grouping), 0);
		break;
	}
	case 163: {
		if(refesh_paused)
			hWorkerCallBack(M_CALLBACK_SWITCH_REFESHRATE_SET, (LPVOID)static_cast<ULONG_PTR>(2), 0);
		else if (refesh_fast)
			hWorkerCallBack(M_CALLBACK_SWITCH_REFESHRATE_SET, (LPVOID)static_cast<ULONG_PTR>(0), 0);
		else
			hWorkerCallBack(M_CALLBACK_SWITCH_REFESHRATE_SET, (LPVOID)static_cast<ULONG_PTR>(1), 0);
		return 0;
	}
	case 164: return MAppLoadMenuSettings();
	case 165: {
		ReleaseCapture();
		SendMessage(hWndMain, WM_NCLBUTTONDOWN, HTCAPTION, 0);
		break;
	}
	case 167: {
		MAppWorkCall3(215, hWnd, NULL);
		LONG stytle = GetWindowLong(hWnd, GWL_STYLE);
		stytle ^= WS_CAPTION;
		SetWindowLong(hWnd, GWL_STYLE, stytle);
		break;
	}
	case 168: MAppVPEExp((LPWSTR)data, hWnd); break;
	case 169: MAppVPEImp((LPWSTR)data, hWnd); break;	
	case 170: MUsersSetCurrentSelectUserName((LPWSTR)data); break;
	case 171: {
		NTSTATUS status = MSetProcessAffinityMask((HANDLE)hWnd, (ULONG_PTR)data);
		if (!NT_SUCCESS(status))
			MShowErrorMessageWithNTSTATUS(str_item_set_proc_affinity_failed, DEFDIALOGGTITLE, status);
		break;
	}
	case 172: MShowProgramStats(); break;
	case 173: {
		MAppWorkCall3(216, hWnd, NULL);
		LONG stytle = GetWindowLong(hWnd, GWL_STYLE);
		stytle |= WS_CAPTION;
		SetWindowLong(hWnd, GWL_STYLE, stytle);
		break;
	}
	case 174: MProcessHANDLEStorageDestroyItem((DWORD)(ULONG_PTR)data); break;
	case 175: {
		if (hWnd) {
			MUsersSetCurrentSelect((DWORD)(ULONG_PTR)data);
			HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUUSER));
			if (hroot) {
				HMENU hpop = GetSubMenu(hroot, 0);
				POINT pt;
				if (menu_last_x == 0 && menu_last_y == 0)
					GetCursorPos(&pt);
				else {
					pt.x = menu_last_x;
					pt.y = menu_last_y;
				}

				TrackPopupMenu(hpop,
					TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
					pt.x,
					pt.y,
					0,
					hWnd,
					NULL);
				DestroyMenu(hroot);
			}
		}
		break;
	}
	case 176: MGetXFun(4); break;
	case 177: appLoadSucessfuly = TRUE;	break;
	case 178: SendMessage(hWnd, WM_COMMAND, IDM_KILL, 0); break;
	case 179: {
		ListView_SetExtendedListViewStyleEx(hWnd, 0, LVS_EX_FULLROWSELECT | LVS_EX_TWOCLICKACTIVATE);
		SendMessage(hWnd, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);
		break;
	}
	case 180: return GetCurrentProcessId();
	case 181: {
		SetUnhandledExceptionFilter(NULL);
		SetUnhandledExceptionFilter(MUnhandledExceptionFilter);
		return 1;
	}
	case 182: MSetAsExplorerTheme(hWnd); return 1;
	case 183: {
		hMenuMain = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUMAIN));
		hMenuMainFile = GetSubMenu(hMenuMain, 0);
		hMenuMainSet = GetSubMenu(hMenuMain, 1);
		hMenuMainView = GetSubMenu(hMenuMain, 3);
		if (!MIsRunasAdmin())
			InsertMenu(hMenuMainFile, 1, MF_BYPOSITION, IDM_REBOOT_AS_ADMIN, str_item_rebootasadmin);
		else {
#ifdef _AMD64_
			InsertMenu(hMenuMainFile, 2, MF_BYPOSITION, IDM_UNLOAD_DRIVER, str_item_unloaddriver);
			InsertMenu(hMenuMainFile, 2, MF_BYPOSITION, IDM_LOAD_DRIVER, str_item_loaddriver);
#else
			if (!MIs64BitOS()) 
			{
				InsertMenu(hMenuMainFile, 2, MF_BYPOSITION, IDM_UNLOAD_DRIVER, str_item_unloaddriver);
				InsertMenu(hMenuMainFile, 2, MF_BYPOSITION, IDM_LOAD_DRIVER, str_item_loaddriver);
			}
#endif
		}
		hWndMain = hWnd;
		if (!M_CFG_GetConfigBOOL(L"SimpleView", L"AppSetting", TRUE))
			SetMenu(hWnd, hMenuMain);

		can_debug = MGetDebuggerInformation();
		return 1;
	}
	case 184: {
		if (data) {
			MSCM_SetCurrSelSc((LPWSTR)data);
			HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUSCSMALL));
			if (hroot) {
				HMENU hpop = GetSubMenu(hroot, 0);
				POINT pt;
				if (menu_last_x == 0 && menu_last_y == 0)
					GetCursorPos(&pt);
				else {
					pt.x = menu_last_x;
					pt.y = menu_last_y;
				}
				TrackPopupMenu(hpop,
					TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
					pt.x,
					pt.y,
					0,
					hWnd,
					NULL);
				DestroyMenu(hroot);
			}
		}
		break;
	}
	case 185: ExitWindowsEx(EWX_REBOOT, 0); break;
	case 186: ExitWindowsEx(EWX_LOGOFF, 0); break;
	case 187: ExitWindowsEx(EWX_SHUTDOWN, 0); break;
	case 188: {
		INITCOMMONCONTROLSEX InitCtrls;
		InitCtrls.dwSize = sizeof(InitCtrls);
		InitCtrls.dwICC = ICC_WIN95_CLASSES;
		InitCommonControlsEx(&InitCtrls);
		break;
	}
	case 189: {
		selectItem4 = (HWND)data;
		HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUWINMAIN));
		if (hroot) {
			HMENU hpop = GetSubMenu(hroot, 0);
			POINT pt;
			if (menu_last_x == 0 && menu_last_y == 0)
				GetCursorPos(&pt);
			else {
				pt.x = menu_last_x;
				pt.y = menu_last_y;
			}
			if (selectItem4 == hWndMain)
			{
				EnableMenuItem(hpop, ID_MAINWINMENU_SETTO, MF_DISABLED);
				EnableMenuItem(hpop, ID_MAINWINMENU_BRINGFORNT, MF_DISABLED);
			}
			TrackPopupMenu(hpop,
				TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
				pt.x,
				pt.y,
				0,
				hWnd,
				NULL);
			DestroyMenu(hroot);
		}
		break;
	}
	case 190: SendMessage(hWnd, WM_COMMAND, IDM_KILL, 0); break;
	case 191: MAppRebot(); break;
	case 192: {
		if (SendMessageTimeout((HWND)data, WM_SYSCOMMAND, SC_CLOSE, 0, SMTO_BLOCK, 500, 0) == 0)
			return 1;
		if (IsWindow((HWND)data))
			return 1;
		return 0;
	}
	case 193: {
		int c = static_cast<int>((ULONG_PTR)data);
		switch (c)
		{
		case 0:
			refesh_paused = false;
			refesh_fast = true;
			break;
		case 1:
			refesh_paused = false;
			refesh_fast = false;
			break;
		case 2:
			refesh_paused = true;
			break;
		}
		CheckMenuRadioItem(hMenuMainViewRefeshRate, 0, 2, c, MF_BYPOSITION);
		hWorkerCallBack(M_CALLBACK_SWITCH_REFESHRATE_SET, (LPVOID)static_cast<ULONG_PTR>(c), 0);
		break;
	}
	case 194: {
		top_most = static_cast<int>((ULONG_PTR)data);
		CheckMenuItem(hMenuMainSet, IDM_TOPMOST, top_most ? MF_CHECKED : MF_UNCHECKED);
		M_CFG_SetConfigBOOL(L"AppSetting", L"TopMost", top_most);
		hWorkerCallBack(M_CALLBACK_SWITCH_TOPMOST_SET, (LPVOID)static_cast<ULONG_PTR>(top_most), 0);
		break;
	}
	case 195: {
		close_hide = static_cast<int>((ULONG_PTR)data);
		CheckMenuItem(hMenuMainSet, IDM_CLOSETOHIDE, close_hide ? MF_CHECKED : MF_UNCHECKED);
		M_CFG_SetConfigBOOL(L"AppSetting", L"CloseHideToNotfication", close_hide);
		hWorkerCallBack(M_CALLBACK_SWITCH_CLOSEHIDE_SET, (LPVOID)static_cast<ULONG_PTR>(close_hide), 0);
		break;
	}
	case 196: {
		min_hide = static_cast<int>((ULONG_PTR)data);
		CheckMenuItem(hMenuMainSet, IDM_MINHIDE, min_hide ? MF_CHECKED : MF_UNCHECKED);
		M_CFG_SetConfigBOOL(L"AppSetting", L"MinHide", min_hide);
		hWorkerCallBack(M_CALLBACK_SWITCH_MINHIDE_SET, (LPVOID)static_cast<ULONG_PTR>(min_hide), 0);
		break;
	}
	case 197: if (data) MSCM_SetCurrSelSc((LPWSTR)data); break;
	case 198: selectItem4 = (HWND)data; break;
	case 200: ShowWindow(hWnd, SW_HIDE); break;
	case 201: if (MCanUseKernel()) M_SU_ForceShutdown(); break;
	case 202: if (MCanUseKernel()) M_SU_ForceReboot(); break;
	case 203: M_SU_ProtectMySelf(); break;
	case 204: M_SU_UnProtectMySelf(); break;
	case 205: ShowWindow(hWnd, SW_SHOW); break;
	case 206: use_apc = (BOOL)(ULONG_PTR)data; break;
	case 207: UnregisterHotKey(hWnd, HotKeyId); break;
	case 208: {		
		if (!IsWindowVisible(hWnd))
		ShowWindow(hWnd, SW_SHOW);
		if (IsIconic(hWnd))
			ShowWindow(hWnd, SW_RESTORE);
		if (has_fullscreen_window && !top_most)
			SendMessage(hWnd, WM_COMMAND, IDM_TOPMOST, 0);
		SetForegroundWindow(hWnd);
		break;
	}
	case 209: {
		APPBARDATA abd;
		memset(&abd, 0, sizeof(abd));
		abd.cbSize = sizeof(APPBARDATA);
		abd.hWnd = hWnd;
		abd.uCallbackMessage = WM_S_APPBAR;
		SHAppBarMessage(ABM_NEW, &abd);
		break;
	}
	case 210: EnableWindow(hWnd, FALSE); break;
	case 211: EnableWindow(hWnd, TRUE); break;
	case 212: {
		menu_last_x = (int)(ULONG_PTR)hWnd;
		menu_last_y = (int)(ULONG_PTR)data;
		break;
	}
	case 213: {
		if (IsIconic(hWnd))
			ShowWindow(hWnd, SW_RESTORE);
		SetForegroundWindow(hWnd);
		break;
	}
	case 214: {
		HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUSIMPPROC));
		if (hroot) {
			HMENU hpop = GetSubMenu(hroot, 0);
			POINT pt;
			if (menu_last_x == 0 && menu_last_y == 0)
				GetCursorPos(&pt);
			else {
				pt.x = menu_last_x;
				pt.y = menu_last_y;
			}
			TrackPopupMenu(hpop,
				TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
				pt.x,
				pt.y,
				0,
				hWnd,
				NULL);
			DestroyMenu(hroot);
		}
		break;
	}
	case 215: SetMenu(hWnd, NULL); break;
	case 216: SetMenu(hWnd, hMenuMain); break;
	case 217: MAppVProcessAllWindowsUWP(); break;
	case 218: return MAppVProcessAllWindowsGetProcessWindow((DWORD)(ULONG_PTR)hWnd);
	case 219: return MAppVProcessAllWindowsGetProcessWindow2((DWORD)(ULONG_PTR)hWnd);
	case 220: {
		PSHOWMENUPROCPPER ppr = (PSHOWMENUPROCPPER)hWnd;
		return MAppWorkShowMenuProcessPrepare(ppr->FilePath, ppr->FileName, ppr->Pid, ppr->IsImporant, ppr->IsVeryImporant);
	}
	case 221: {
		PSHOWMENUPROC ppr = (PSHOWMENUPROC)hWnd;
		return MAppWorkShowMenuProcess(ppr->FilePath, ppr->FileName, ppr->Pid, ppr->hWnd, ppr->selectedhWnd, ppr->data, ppr->type, ppr->x, ppr->y);
	}
	case 222: MAppVProcessAllWindows(); break;
	case 223:return aopTimer;
	case 224: {
		MAppWorkCall3(223, hWndMain, NULL);
		break;
	}
	default: break;
	}
	return 0;
}	
M_API void* MAppWorkCall4(int id, void* hWnd, void*data)
{
	switch (id)
	{
	case 95: {
		WCHAR buffer[MAX_PATH];
		wcscpy_s(buffer, L"%SystemRoot%");
		ExpandEnvironmentStrings(buffer, systemRoot, MAX_PATH);
		return systemRoot;
	}
	case 96: {
		WCHAR buffer[MAX_PATH];
		wcscpy_s(buffer, L"%SystemRoot%\\System32\\csrss.exe");
		ExpandEnvironmentStrings(buffer, cssrssPath, MAX_PATH);
		return cssrssPath;
	}
	case 97: {
		WCHAR buffer[MAX_PATH];
		wcscpy_s(buffer, L"%SystemRoot%\\System32\\ntoskrnl.exe");
		ExpandEnvironmentStrings(buffer, ntoskrnlPath, MAX_PATH);
		return ntoskrnlPath;
	}
	case 98: {
		MLoadApp();
		MLoadAppBackUp();
		return 0;
	}
	case 99: return SetParent((HWND)hWnd, (HWND)data);
	case 100: { 

		break;
	}
	case 101:return thisCommandName;
	case 102:return (void*)(ULONG_PTR)MAppFkillWnd((HWND)hWnd);
	case 103: {
		HWND h = (HWND)hWnd;
		LONG exstytle = GetWindowLong(h, GWL_EXSTYLE);
		if ((exstytle & WS_EX_TOPMOST) == WS_EX_TOPMOST) {
			exstytle ^= WS_EX_TOPMOST;
			LONG rs = SetWindowLong(h, GWL_EXSTYLE, exstytle);
			if (rs == 0 && GetLastError() == 5) MAppFkillWnd(h);
			rs = SetWindowLong(h, GWL_STYLE, GetWindowLong(h, GWL_STYLE) | WS_OVERLAPPEDWINDOW);
			if (rs == 0 && GetLastError() == 5) MAppFkillWnd(h);
		}
		break;
	}
	case 104: {
		HWND h = (HWND)hWnd;
		LONG exstytle = GetWindowLong(h, GWL_EXSTYLE);
		if ((exstytle & WS_EX_TOPMOST) == WS_EX_TOPMOST) {
			exstytle ^= WS_EX_TOPMOST;
			LONG rs = SetWindowLong(h, GWL_EXSTYLE, exstytle);
			if (rs == 0 && GetLastError() == 5)
				MAppFkillWnd(h);
		}
		SetForegroundWindow(hWndMain);
		break;
	}
	}
	return 0;
}
M_API void* MAppWorkCall5(int id, void* hWnd, void*data1, void*data2, void*data3)
{
	switch (id)
	{
	case 50: return (LPVOID)(ULONG_PTR)MKillProcessUser2(hWndMain, (DWORD)(ULONG_PTR)data1, (BOOL)(ULONG_PTR)data2, (BOOL)(ULONG_PTR)data3);
	case 51: MoveWindow((HWND)hWnd, 15, 15, (int)(ULONG_PTR)data1, (int)(ULONG_PTR)data2, TRUE); break;
	case 52: MoveWindow((HWND)hWnd, (int)(ULONG_PTR)LOWORD(data1), (int)(ULONG_PTR)HIWORD(data1), (int)(ULONG_PTR)LOWORD(data2), (int)(ULONG_PTR)HIWORD(data2), TRUE); break;
	case 53: return (LPVOID)(ULONG_PTR)MAppWorkShowMenuFM((LPWSTR)data1, (BOOL)(ULONG_PTR)data2, (int)(ULONG_PTR)data3);
	case 54: return (LPVOID)(ULONG_PTR)MAppWorkShowMenuFMF((LPWSTR)data1);
	case 55: return (LPVOID)(ULONG_PTR)MAppRegShowHotKey((HWND)hWnd, (UINT)(ULONG_PTR)data1, (UINT)(ULONG_PTR)data2);
	
	}
	return 0;
}
M_API void MAppHideCos()
{
	ShowWindow(GetConsoleWindow(), SW_HIDE);
}
M_API void*MAppSetCallBack(void * cp, int id)
{
	switch (id)
	{
	case 0:
		return (WNDPROC)MAppWinProc;
		break;
	case 1:
		hMainExitCallBack = (exitcallback)cp;
		break;
	case 2:
		hTerminateImporantWarnCallBack = (TerminateImporantWarnCallBack)cp;
		break;
	case 3:
		hEnumWinsCallBack = (EnumWinsCallBack)cp;
		break;
	case 4:
		hGetWinsWinsCallBack = (GetWinsCallBack)cp;
		break;
	case 5:
		hWorkerCallBack = (WorkerCallBack)cp;
		break;
	default:
		break;
	}
	return NULL;
}
M_API void MAppMainCall(int msg, void* data1, void* data2)
{
	hWorkerCallBack(msg, data1, data2);
}
M_API void MAppSetLanuageItems(int in, int ind, LPWSTR msg, int size)
{
	switch (in)
	{
	case 0:
		MLG_SetLanuageItems_0(ind, msg, size);
		break;
	case 1:
		MLG_SetLanuageItems_1(ind, msg, size);
		break;
	case 2:
		MLG_SetLanuageItems_2(ind, msg, size);
		break;
	default:
		break;
	}
}
int MAppRegShowHotKey(HWND hWnd, UINT vkkey, UINT key)
{	
    HotKeyId = GlobalAddAtom(L"PCMgrHotKey") - 0xC000;
	if (vkkey == 65536)//c# shift
		vkkey = MOD_SHIFT;
	if (vkkey == 262144)//c# alt
		vkkey = MOD_ALT;
	if (vkkey == 91 || vkkey == 92) {//c# lwin rwin
		if (!RegisterHotKey(hWnd, HotKeyId, MOD_WIN, key))
			LogWarn(L"RegisterHotKey failed : %d ", GetLastError());
	}
	else {
		if (!RegisterHotKey(hWnd, HotKeyId, MOD_CONTROL | vkkey, key))
			LogWarn(L"RegisterHotKey failed : %d ", GetLastError());
	}
	return HotKeyId;
}
M_API void MAppSetStartingProgessText(LPWSTR text)
{
	MAppMainCall(M_CALLBACK_UPDATE_LOAD_STATUS, text, NULL);
}
M_API void MAppSet(int id, void*v)
{
	switch (id)
	{
	case 0:
		break;
	case 1:
		MAppStartTestZz(v);
		break;
	case 2:
		MGetProcAddressCore = (_MGetProcAddressCore)v;
		break;
	case 8:
		if (v) {
			MAppStartGGUID();
			*((WCHAR**)v) = thisGuid;
		}
		break;
	case 16: {
		MGetXFun = (_MGetXFun)v;
		break;
	}
	}
}
M_API void MAppTest(int id, void*v) {
	switch (id)
	{
	case 0:
		FreeLibrary(hInst);
		break;
	case 1:
		MessageBox(0, L"Test", L"MAppTest", 0);
		break;
	default:
		break;
	}
}
M_API LRESULT MAppMainThreadCall(WPARAM wParam, LPARAM lParam)
{
	if (!executeByLoader)
		return 0;
	return SendMessage(hWndMain, WM_S_MAINTHREAD_ACT, wParam, lParam);
}
M_API LPWSTR MAppGetName() { return appName; }
M_API LPWSTR MAppGetVersion() { 
	return APP_VERSION; 
}
M_API LPWSTR MAppGetBulidDate() { return APP_BULID_DATE; }

M_CAPI(HMODULE) MAppGetCoreModulHandle() {
#ifdef _X64_
	return GetModuleHandle(L"PCMgr64.dll");
#else
	return GetModuleHandle(L"PCMgr32.dll");
#endif
}

//...
M_API HICON MGetWindowIcon(HWND hWnd)
{
	HICON m_hSmallIcon;
	if (!SendMessageTimeoutW(hWnd, WM_GETICON, 0, 0,
		SMTO_BLOCK | SMTO_ABORTIFHUNG, 1000, (PULONG_PTR)&m_hSmallIcon)
		|| NULL == m_hSmallIcon)
	{
		m_hSmallIcon = (HICON)(LONG_PTR)GetClassLongPtr(hWnd, GCLP_HICONSM);
	}	
	return m_hSmallIcon;
}
M_API BOOL MIsSystemSupport()
{
	return IsWindows7OrGreater();
}

LPWSTR*argsStrs = NULL;
BOOL isRunningMainApp = FALSE;
BOOL clrInited = FALSE;
ICLRGCManager *pGcManager = nullptr;

M_API void MAppMainExit(UINT exitcode)
{
	ExitProcess(exitcode);
}
M_API DWORD MAppMainGetExitCode()
{
	return dwMainAppRet;
}
M_API BOOL MAppMainRun()
{
	BOOL rs = FALSE;

	if (!MAppStartTestZzX(MGetXFun(2)))
	{
		MessageBox(0, L"Only pcmgr can use this function.", L"illegal use", 0);
		return rs;
	}

	dMGetCurrentPeb = (_MGetCurrentPeb)MGetXFun(1);

	isRunningMainApp = TRUE;

	GetModuleFileName(NULL, appDir, MAX_PATH);
	std::wstring *w = Path::GetFileName(appDir);
	wcscpy_s(appName, w->c_str());
	delete w;
	PathRemoveFileSpec(appDir);

	GetModuleFileName(0, iniPath, MAX_PATH);
	PathRenameExtension(iniPath, (LPWSTR)L".ini");

	WCHAR lastWindowTitle[64];
	GetPrivateProfileString(L"AppSetting", L"LastWindowTitle", L"", lastWindowTitle, 64, iniPath);

	WCHAR lastLg[16];
	GetPrivateProfileString(L"AppSetting", L"Lanuage", L"", lastLg, 16, iniPath);
	WCHAR logLev[16];
	GetPrivateProfileString(L"AppSetting", L"LogLevel", L"", logLev, 16, iniPath);

	MLG_SetLanuageItems_NoRealloc();
	MLG_SetLanuageRes(appDir, lastLg);

	if (MAppStartTest())
	{
		if (MAppStartTryActiveLastApp(lastWindowTitle))
			return TRUE;
		else if (MAppStartShowRun2Warn())
			return TRUE;
	}

	M_LOG_Init(M_CFG_GetConfigBOOL(L"ShowDebugWindow", L"Configure", FALSE), 
		M_CFG_GetConfigBOOL(L"LogToFile", L"Configure", FALSE));
	M_LOG_SetLogLevelStr(logLev);

	WCHAR loadFailedText[32];
	LoadString(hInstRs, IDS_STRING_LOADAPP_FAIL, loadFailedText, 32);

	WCHAR mainDllPath[MAX_PATH];
	wcscpy_s(mainDllPath, appDir);
#ifdef _AMD64_
	wcscat_s(mainDllPath, L"\\PCMgrApp64.dll");
#else
	wcscat_s(mainDllPath, L"\\PCMgrApp32.dll");
#endif
	if (!MFM_FileExist(mainDllPath))
	{
		WCHAR mainMissingText[32];
		LoadString(hInstRs, IDS_STRING_MISSING_MAINDLL, mainMissingText, 32);

		MShowErrorMessage(mainMissingText, loadFailedText, MB_ICONERROR);
		LogErr(L"Main Dll missing : %s", mainDllPath);
		return rs;
	}
	else
	{
		ICLRMetaHost *pMetaHost = nullptr;
		ICLRMetaHostPolicy *pMetaHostPolicy = nullptr;
		ICLRRuntimeHost *pRuntimeHost = nullptr;
		ICLRRuntimeInfo *pRuntimeInfo = nullptr;

		HRESULT hr = dCLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&pMetaHost);
		hr = pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo));

		if (FAILED(hr)) {
			MShowErrorMessage(L"Not found .NET framework runtime v4.0.30319.", L"Load app failed", MB_ICONERROR);
			LogErr(L"GetRuntime v4.0.30319 failed HRESULT : 0x%08X", hr);
			goto cleanup;
		}
		hr = pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pRuntimeHost));
		if (FAILED(hr)) {
			MShowErrorMessage(L"Create CLRRuntimeHost Failed.", L"Load app failed", MB_ICONERROR);
			LogErr(L"GetInterface CLSID_CLRRuntimeHost failed HRESULT : 0x%08X", hr);
			goto cleanup;
		}

		clrInited = TRUE;

		ICLRControl *manager = nullptr;
		pRuntimeHost->GetCLRControl(&manager);
		if (SUCCEEDED(hr)) {
			hr = manager->GetCLRManager(IID_ICLRGCManager, (LPVOID*)&pGcManager);
			if (FAILED(hr)) LogWarn2(L"ICLRControl->QueryInterface faild : 0x%X08", hr);
		}
		else LogWarn2(L"GetCLRControl faild HRESULT : 0x%X08", hr);

		hr = pRuntimeHost->Start();
		if (FAILED(hr)) { LogErr(L"Start RuntimeHost failed HRESULT : 0x%08X", hr); goto cleanup; }

		LogInfo(L"Load main app : %s", mainDllPath);
		hr = pRuntimeHost->ExecuteInDefaultAppDomain(mainDllPath, L"PCMgr.Program", L"ProgramEntry", GetCommandLine(), &dwMainAppRet);
		if (FAILED(hr)) {
			LogErr(L"ExecuteInDefaultAppDomain %s failed HRESULT : 0x%08X", mainDllPath, hr);
			if (!appLoadSucessfuly) 
			{
				if (hr == COR_E_DLLNOTFOUND)
				{
					WCHAR mainMissingText[32];
					LoadString(hInstRs, IDS_STRING_MISSINGDLL_HLP, mainMissingText, 32);

					MShowErrorMessage(mainMissingText, loadFailedText, MB_ICONERROR);
				}
				else 
				{
					_com_error err(hr);
					LPCTSTR errMsg = err.ErrorMessage();
					std::wstring hResultSr = FormatString(L"%s\n%s\nHRESULT : 0x%08X", loadFailedText, errMsg, hr);
					MShowErrorMessage((LPWSTR)hResultSr.c_str(), DEFDIALOGGTITLE, MB_ICONERROR);
				}
			}
		}
		else rs = TRUE;
		hr = pRuntimeHost->Stop();

		clrInited = FALSE;
	cleanup:
		if (pRuntimeInfo != nullptr) {
			pRuntimeInfo->Release();
			pRuntimeInfo = nullptr;
		}
		if (pRuntimeHost != nullptr) {
			pRuntimeHost->Release();
			pRuntimeHost = nullptr;
		}
		if (pMetaHost != nullptr) {
			pMetaHost->Release();
			pMetaHost = nullptr;
		}
	}


	MAppStartEnd();

	return rs;
}

//App agrs
M_API int MAppMainGetArgs(LPWSTR cmdline) {
	int argc = 0;
	argsStrs = CommandLineToArgvW(cmdline, &argc);
	return argc;
}
M_API LPWSTR MAppMainGetArgsStr(int index)
{
	return argsStrs[index];
}
M_API void MAppMainGetArgsFreeAll() {
	if (argsStrs) LocalFree(argsStrs);
}

//old init funs
typedef void(*startfun)();

EXTERN_C BOOL STDMETHODCALLTYPE _CorDllMain(HINSTANCE hInst, DWORD dwReason, LPVOID lpReserved);

bool MLoadApp() {
	return _CorDllMain(hInst, 0, 0);
}
bool MLoadAppBackUp()
{
	HMODULE hI = LoadLibrary(L"PCMgrInit32.dll");
	if (hI)
	{
		startfun s = (startfun)GetProcAddress(hI, "MStart");
		if (s) {
			s();
			return true;
		}
	}
	return false;
}

//global ex
M_API void MAppExit() {
	if (hMainExitCallBack)
		hMainExitCallBack();
}
M_API void MAppRebot() {

	TCHAR exeFullPath[MAX_PATH];
	GetModuleFileName(NULL, exeFullPath, MAX_PATH);
	ShellExecute(NULL, L"open", exeFullPath, NULL, NULL, 5);

	MAppStartEnd();

	if (hMainExitCallBack)
		hMainExitCallBack();
}
M_API void MAppRebotAdmin() {

	TCHAR exeFullPath[MAX_PATH];
	GetModuleFileName(NULL, exeFullPath, MAX_PATH);
	if (static_cast<int>((ULONG_PTR)ShellExecute(NULL, L"runas", exeFullPath, NULL, NULL, 5)) > 32) {
		MAppStartEnd();
		if (hMainExitCallBack) 
			hMainExitCallBack();
	}
	else {
		LogWarn(L"Restart app to admin canceled.");
		if (GetLastError() == ERROR_CANCELLED) {
			MShowMessageDialog(hWndMain, str_item_noadmin1, str_item_tip, str_item_noadmin2);
		}
	}
}
M_API void MAppRebotAdmin2(LPWSTR agrs) {

	TCHAR exeFullPath[MAX_PATH];
	GetModuleFileName(NULL, exeFullPath, MAX_PATH);

	if (static_cast<int>((ULONG_PTR)ShellExecute(NULL, L"runas", exeFullPath, agrs, NULL, 5)) > 32)
	{
		MAppStartEnd();
		if (hMainExitCallBack)
			hMainExitCallBack();
	}
	else {
		if (GetLastError() == ERROR_CANCELLED) {
			MShowMessageDialog(hWndMain, str_item_noadmin1, str_item_tip, str_item_noadmin2);
		}
	}
}
M_API void MAppRebotAdmin3(LPWSTR agrs, BOOL *userCanceled) {

	TCHAR exeFullPath[MAX_PATH];
	GetModuleFileName(NULL, exeFullPath, MAX_PATH);

	if (userCanceled)*userCanceled = FALSE;

	if (static_cast<int>((ULONG_PTR)ShellExecute(NULL, L"runas", exeFullPath, agrs, NULL, 5)) > 32)
	{
		MAppStartEnd();
		if (hMainExitCallBack) hMainExitCallBack();
	}
	else {
		if (GetLastError() == ERROR_CANCELLED) {
			if (userCanceled)*userCanceled = TRUE;
		}
	}
}

//Theme
M_API HANDLE MOpenThemeData(HWND hWnd, LPWSTR className)
{
	return OpenThemeData(hWnd, className);
}
M_API void MCloseThemeData(HANDLE hTheme)
{
	if (hTheme != NULL)
		CloseThemeData(hTheme);
}
M_API void MSetAsExplorerTheme(HWND hWnd)
{
	SetWindowTheme(hWnd, L"Explorer", NULL);
}

M_API void MDrawIcon(HICON hIcon, HDC hdc, int x, int y)
{
	DrawIconEx(hdc, x, y, hIcon, 16, 16, 0, NULL, DI_NORMAL);
}

//Theme draw Expand button
M_API void MExpandDrawButton(HANDLE hTheme, HDC hdc, int x, int y, int state, BOOL	on)
{
	RECT rc;
	rc.left = x;
	rc.top = y;
	rc.right = x + 19;
	rc.bottom = y + 21;
	switch (state)
	{
	case M_DRAW_EXPAND_NORMAL:
		DrawThemeBackground(hTheme, hdc, TDLG_EXPANDOBUTTON, on ? TDLGEBS_EXPANDEDNORMAL : TDLGEBS_NORMAL, &rc, &rc);
		break;
	case M_DRAW_EXPAND_HOVER:
		DrawThemeBackground(hTheme, hdc, TDLG_EXPANDOBUTTON, on ? TDLGEBS_EXPANDEDHOVER : TDLGEBS_HOVER, &rc, &rc);
		break;
	case M_DRAW_EXPAND_PRESSED:
		DrawThemeBackground(hTheme, hdc, TDLG_EXPANDOBUTTON, on ? TDLGEBS_EXPANDEDPRESSED : TDLGEBS_PRESSED, &rc, &rc);
		break;
	}
}
//Theme draw Header
M_API void MHeaderDrawItem(HANDLE hTheme, HDC hdc, int x, int y, int w, int h, int state)
{	
	if (hTheme)
	{
		RECT rc;
		rc.left = x;
		rc.top = y;
		rc.right = x + w;
		rc.bottom = y + h;
		switch (state)
		{
		case M_DRAW_HEADER_HOT:
			DrawThemeBackground(hTheme, hdc, HP_HEADERITEM, HIS_HOT, &rc, &rc);
			break;
		case M_DRAW_HEADER_PRESSED:
			DrawThemeBackground(hTheme, hdc, HP_HEADERITEM, HIS_PRESSED, &rc, &rc);
			break;
		case M_DRAW_HEADER_SORTDOWN:
			DrawThemeBackground(hTheme, hdc, HP_HEADERSORTARROW, HSAS_SORTEDDOWN, &rc, &rc);
			break;
		case M_DRAW_HEADER_SORTUP:
			DrawThemeBackground(hTheme, hdc, HP_HEADERSORTARROW, HSAS_SORTEDUP, &rc, &rc);
			break;
		}
	}
}
//Theme draw LISTVIEW Item
M_API void MListDrawItem(HANDLE hTheme, HDC hdc, int x, int y, int w, int h, int state)
{
	RECT rc;
	rc.left = x;
	rc.top = y;
	rc.right = x + w;
	rc.bottom = y + h;
	switch (state)
	{
	case M_DRAW_LISTVIEW_HOT:
		DrawThemeBackground(hTheme, hdc, LVP_LISTITEM, LISS_HOT, &rc, &rc);
		break;
	case M_DRAW_LISTVIEW_SELECT_NOFOCUS:
		DrawThemeBackground(hTheme, hdc, LVP_LISTITEM, LISS_SELECTEDNOTFOCUS, &rc, &rc);
		break;
	case M_DRAW_LISTVIEW_HOT_SELECT:
		DrawThemeBackground(hTheme, hdc, LVP_LISTITEM, LISS_HOTSELECTED, &rc, &rc);
		break;
	case M_DRAW_LISTVIEW_SELECT:
		DrawThemeBackground(hTheme, hdc, LVP_LISTITEM, LISS_SELECTED, &rc, &rc);
		break;
	case M_DRAW_TREEVIEW_GY_OPEN:
		DrawThemeBackground(hTheme, hdc, TVP_GLYPH, GLPS_OPENED, &rc, &rc);
		break;
	case M_DRAW_TREEVIEW_GY_CLOSED:
		DrawThemeBackground(hTheme, hdc, TVP_GLYPH, GLPS_CLOSED, &rc, &rc);
		break;
	case M_DRAW_TREEVIEW_GY_OPEN_HOT:
		DrawThemeBackground(hTheme, hdc, TVP_HOTGLYPH, HGLPS_OPENED, &rc, &rc);
		break;
	case M_DRAW_TREEVIEW_GY_CLOSED_HOT:
		DrawThemeBackground(hTheme, hdc, TVP_HOTGLYPH, HGLPS_CLOSED, &rc, &rc);
		break;
	default:
		break;
	}
}

M_CAPI(void) MListViewSetColumnSortArrow(HWND hListHeader, int index, BOOL isUp, BOOL no) {
	HDITEM item = { 0 };
	item.mask = HDI_FORMAT;
	Header_GetItem(hListHeader, index, &item);
	if (no) {
		if ((item.fmt & HDF_SORTDOWN) == HDF_SORTDOWN) item.fmt ^= HDF_SORTDOWN;
		if ((item.fmt & HDF_SORTUP) == HDF_SORTUP) item.fmt ^= HDF_SORTUP;
	}
	else {
		if (isUp) {
			if ((item.fmt & HDF_SORTDOWN) == HDF_SORTDOWN) item.fmt ^= HDF_SORTDOWN;
			item.fmt |= HDF_SORTUP;
		}
		else {
			if ((item.fmt & HDF_SORTUP) == HDF_SORTUP) item.fmt ^= HDF_SORTUP;
			item.fmt |= HDF_SORTDOWN;
		}
	}
	Header_SetItem(hListHeader, index, &item);
}
M_CAPI(HWND) MListViewGetHeaderControl(HWND hList, BOOL isMain) {
	HWND hListHeader = FindWindowEx(hList, NULL, L"SysHeader32", NULL);
	if (isMain && hListHeader) {
		IDC_MAINLIST_HEADER = GetDlgCtrlID(hListHeader);
		procListHeaderWndProc = (WNDPROC)GetWindowLongPtr(hListHeader, GWL_WNDPROC);
		SetWindowLongPtr(hListHeader, GWL_WNDPROC, (LONG_PTR)MProcListHeaderWinProc);
		hListHeaderMainProcList = hListHeader;
	}
	return hListHeader;
}
M_CAPI(void) MListViewProcListWndProc(HWND hList) {
	procListWndProc = (WNDPROC)GetWindowLongPtr(hList, GWL_WNDPROC);
	SetWindowLongPtr(hList, GWL_WNDPROC, (LONG_PTR)MProcListWinProc);
}
M_CAPI(void) MListViewProcListLock(BOOL lock) {
	procListLock = lock;
}

void MAppWmCommandTools(WPARAM wParam)
{
	switch (wParam)
	{
	case IDC_SOFTACT_SHOWDRIVER_LOADERTOOL: 
		MAppMainCall(M_CALLBACK_LOADDRIVER_TOOL, GetDesktopWindow(), 0);
		break;
	case IDC_SOFTACT_SHOWSPY:
		MAppMainCall(M_CALLBACK_SPY_TOOL, GetDesktopWindow(), 0);
		break;
	case IDC_SOFTACT_SHOWFILETOOL:
		MAppMainCall(M_CALLBACK_FILE_TOOL, 0, 0);
		break;
	default:
		break;
	}
}

void ThrowErrorAndErrorCodeX(NTSTATUS code, LPWSTR msg, LPWSTR title, BOOL ntstatus)
{
	if (ntstatus && STATUS_SUCCESS == code)return;	
	wchar_t errcode[260];
	if (ntstatus) {
		LPWSTR ntstatusstr = MNtStatusToStr(code);
		if (ntstatusstr==0)
		wsprintf(errcode, L"\nCode : %d\nNTSTATUS : 0x%lX", code, code);
		else wsprintf(errcode, L"\nCode : %d\n%s (0x%lX)", code, ntstatusstr, code);
	}
	else wsprintf(errcode, L"\nError Code : %d", code);
	MShowMessageDialog(hWndMain, errcode, title, msg, MB_ICONERROR, MB_OK);
}

//主窗口 WinProc
LRESULT CALLBACK MAppWinProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_QUERYENDSESSION: {
		if (M_CFG_GetConfigBOOL(L"AbortShutdown", L"AppSetting", false))
		{
			dCancelShutdown();
			return TRUE;
		}
		else MAppExit();
		break;
	}
	case WM_S_MAINTHREAD_ACT: {
		switch (wParam)
		{
		case M_MTMSG_COSCLOSE: {
			M_LOG_CloseConsole(FALSE);
			break;
		}
		default:
			break;
		}
		break;
	}
	case WM_S_MESSAGE_EXIT: {
		MAppExit();
		break;
	}
	case WM_S_APPBAR: {
		switch (wParam)
		{
		case ABN_FULLSCREENAPP:
		{
			has_fullscreen_window = (BOOL)lParam;
		}
		break;
		default:
			break;
		}
		break;
	}
	case WM_S_MESSAGE_ACTIVE: {
		MAppWorkCall3(208, hWnd, 0);
		break;
	}
	case WM_COMMAND: {
		switch (wParam)
		{
		case IDM_LOAD_DRIVER: {
			if (MAppShowLoadDrvWarn())
				MAppMainCall(M_CALLBACK_KERNEL_INIT, 0, 0);
			break;
		}
		case IDM_UNLOAD_DRIVER: {
			MUninitKernel();
			break;
		}
		case IDM_TOPMOST: {
			MAppWorkCall3(194, 0, (LPVOID)static_cast<ULONG_PTR>(!top_most));
			break;
		}
		case IDM_ALWAYSTOP: {
			MAppWorkCall3(156, 0, (LPVOID)static_cast<ULONG_PTR>(!alwaysOnTop));
			break;
		}
		case IDM_MINHIDE: {
			MAppWorkCall3(196, 0, (LPVOID)static_cast<ULONG_PTR>(!min_hide));
			break;
		}
		case IDM_CLOSETOHIDE: {
			MAppWorkCall3(195, 0, (LPVOID)static_cast<ULONG_PTR>(!close_hide));
			break;
		}
		case IDM_REFESH_FAST: {
			MAppWorkCall3(193, NULL, (LPVOID)(ULONG_PTR)(0));
			break;
		}
		case IDM_REFESH_PAUSED: {
			MAppWorkCall3(193, NULL, (LPVOID)(ULONG_PTR)2);
			break;
		}
		case IDM_REFESH_SLOW: {
			MAppWorkCall3(193, NULL, (LPVOID)(ULONG_PTR)1);
			break;
		}
		case ID_SIMPPROC_NEWTASK:
		case IDM_RUN: {
			MRunFileDlg(hWnd, NULL, NULL, NULL, NULL, 0);
			break;
		}
		case IDM_REBOOT_AS_ADMIN: {
			MAppRebotAdmin();
			break;
		}
		case IDM_ABOUT: {
			//DialogBox(hInstRs, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
			MAppMainCall(M_CALLBACK_ABOUT, 0, 0);
			break;
		}
		case IDM_HELP: {
			MAppMainCall(M_CALLBACK_SHOW_HELP, 0, 0);
			break;
		}
		case IDM_TEXIT: {
			MAppExit();
			break;
		}
		case IDM_TREBOT: {
			MAppRebot();
			break;
		}
		case IDM_KILL: {
			if (killCmdSendBack)
				MAppMainCall(M_CALLBACK_ENDTASK, (LPVOID)(ULONG_PTR)thisCommandPid, 0);
			else if (killUWPCmdSendBack)
				MAppMainCall(M_CALLBACK_UWPKILL, (LPVOID)thisCommandUWPName, 0);
			else MKillProcessUser(TRUE);
			break;
		}
		case IDM_KILLKERNEL: {
			MFroceKillProcessUser();
			break;
		}
		case IDM_KILLPROCTREE: {
			MKillProcessTreeUser();
			break;
		}
		case IDM_FILEPROP: {
			if (thisCommandPath)
				MShowFileProp(thisCommandPath);
			break;
		}
		case IDM_OPENPATH: {
			if (thisCommandPath)
			{
				std::wstring buf = FormatString(L"/select,%s", thisCommandPath);
				ShellExecuteW(NULL, NULL, L"explorer.exe", buf.c_str(), NULL, SW_SHOWDEFAULT);
			}
			break;
		}
		case IDM_VMODULS: {
			if (thisCommandPid > 4)
				MAppVProcessModuls(thisCommandPid, hWndMain, thisCommandName);
			break;
		}
		case IDM_VTHREAD: {
			if (thisCommandPid > 4)
				MAppVProcessThreads(thisCommandPid, hWndMain, thisCommandName);
			break;
		}
		case IDM_VWINS: {
			if (thisCommandPid > 4)
				MAppVProcessWindows(thisCommandPid, hWndMain, thisCommandName);
			break;
		}
		case IDM_VHANDLES: {
			if (thisCommandPid > 4)
				MAppMainCall(M_CALLBACK_VIEW_HANDLES, (LPVOID)(ULONG_PTR)thisCommandPid, thisCommandName);
			break;
		}
		case IDM_VPRIVILEGE: {
			if (thisCommandPid > 4)
				MAppMainCall(M_CALLBACK_KERNEL_VIELL_PRGV, (LPVOID)(ULONG_PTR)thisCommandPid, thisCommandName);
			break;
		}
		case IDM_SUPROC: {
			if (thisCommandPid > 4)
			{
				if (thisCommandPid == GetCurrentProcessId() && !hTerminateImporantWarnCallBack(0, 5)) break;
				if (thisCommandIsVeryImporant && !hTerminateImporantWarnCallBack(thisCommandName, 4)) break;
				if (!thisCommandIsVeryImporant && thisCommandIsImporant && !hTerminateImporantWarnCallBack(thisCommandName, 2)) break;
				NTSTATUS status = MSuspendProcessNt(thisCommandPid, NULL);
				if (status == STATUS_INVALID_HANDLE) {
					MShowErrorMessage((LPWSTR)str_item_invalidproc.c_str(), (LPWSTR)str_item_op_failed.c_str(), MB_ICONWARNING, MB_OK);
					SendMessage(hWndMain, WM_COMMAND, 41012, 0);
				}
				else if (status == STATUS_ACCESS_DENIED)
					MShowErrorMessage((LPWSTR)str_item_access_denied.c_str(), (LPWSTR)str_item_op_failed.c_str(), MB_ICONERROR, MB_OK);
				else if (status != STATUS_SUCCESS) ThrowErrorAndErrorCodeX(status, str_item_susprocfailed, (LPWSTR)str_item_op_failed.c_str());
			}
			break;
		}
		case IDM_RESPROC: {
			if (thisCommandPid > 4)
			{
				NTSTATUS status = MResumeProcessNt(thisCommandPid, NULL);
				if (status == STATUS_INVALID_HANDLE) {
					MShowErrorMessage((LPWSTR)str_item_invalidproc.c_str(), (LPWSTR)str_item_op_failed.c_str(), MB_ICONWARNING, MB_OK);
					SendMessage(hWndMain, WM_COMMAND, 41012, 0);
				}
				else if (status == STATUS_ACCESS_DENIED)
					MShowErrorMessage((LPWSTR)str_item_access_denied.c_str(), (LPWSTR)str_item_op_failed.c_str(), MB_ICONERROR, MB_OK);
				else if (status != STATUS_SUCCESS)ThrowErrorAndErrorCodeX(status, str_item_resprocfailed, (LPWSTR)str_item_op_failed.c_str());
			}
			break;
		}
		case IDM_VKSTRUCTS: {
			MAppMainCall(M_CALLBACK_VIEW_KSTRUCTS, (LPVOID)(ULONG_PTR)thisCommandPid, thisCommandName);
			break;
		}
		case IDM_VTIMER: {
			if (thisCommandPid > 4)
				MAppMainCall(M_CALLBACK_VIEW_TIMER, (LPVOID)(ULONG_PTR)thisCommandPid, thisCommandName);
			break;
		}
		case IDM_VHOTKEY: {
			if (thisCommandPid > 4)
				MAppMainCall(M_CALLBACK_VIEW_HOTKEY, (LPVOID)(ULONG_PTR)thisCommandPid, thisCommandName);
			break;
		}
		case IDM_DEBUG: {
			if (thisCommandPid > 4)
			{
				std::wstring cmd = FormatString(debuggerCommand, thisCommandPid, thisCommandPid);
				if (!MRunExeWithAgrs((LPWSTR)cmd.c_str(), FALSE, hWndMain))
					MShowErrorMessage((LPWSTR)str_item_op_failed.c_str(), L"", MB_ICONERROR);
			}
			break;
		}
		case IDM_DEATCH_DEBUGGER: {
			if (thisCommandPid > 4)
				MDetachFromDebuggerProcess(thisCommandPid);
			break;
		}
		case IDM_CREATE_MINI_DUMP: {
			if (thisCommandPid > 4)
				MCreateMiniDumpForProcess(thisCommandPid);
			break;
		}
		case IDM_SGINED: {
			if (thisCommandPath)
			{
				if (MFM_FileExist(thisCommandPath))
				{
					if (MGetExeFileTrust(thisCommandPath))
						MAppMainCall(M_CALLBACK_SHOW_TRUSTED_DLG, thisCommandPath, 0);
					else MShowMessageDialog(hWndMain, thisCommandPath, str_item_tip, str_item_filenottrust, 0, 0);
				}
				else MShowMessageDialog(hWndMain, str_item_filenotexist, (LPWSTR)str_item_op_failed.c_str(), L"");
			}
			break;
		}
		case IDM_GROUP: {
			MAppWorkCall3(162, (HWND)(ULONG_PTR)(!main_grouping), 0);
			break;
		}
		case IDM_COLLAPSE: {
			MAppMainCall(M_CALLBACK_COLLAPSE_ALL, 0, 0);
			break;
		}
		case IDM_EXPAND: {
			MAppMainCall(M_CALLBACK_EXPAND_ALL, 0, 0);
			break;
		}
		case IDC_PCMGR_CMD: {
			if (!M_CFG_GetConfigBOOL(L"ShowDebugWindow", L"Configure", FALSE)) {
				M_CFG_SetConfigBOOL(L"ShowDebugWindow", L"Configure", TRUE);
				M_LOG_Close();
				M_LOG_Init(1, 0);
			}
			else if (showConsole) {
				M_LOG_FocusConsoleWindow();
			}
			else M_LOG_Init(1, 0);
			break;
		}
		case IDC_CLOSE_PCMGR_CMD: {
			if (showConsole) {
				M_LOG_CloseConsole(TRUE, TRUE);
				M_CFG_SetConfigBOOL(L"ShowDebugWindow", L"Configure", FALSE);
				showConsole = false;
			}
			break;
		}
		case IDC_SOFTACT_SHOW_KDA: {
			MAppMainCall(M_CALLBACK_KDA, 0, 0);
			break;
		}
		case IDC_PCMGR_KERNEL_TOOL: {
			MAppMainCall(M_CALLBACK_KERNEL_TOOL, 0, 0);
			break;
		}
		case IDC_PCMGR_HOOK_TOOL: {
			MAppMainCall(M_CALLBACK_HOOKS, 0, 0);
			break;
		}
		case IDC_PCMGR_NETMON: {
			MAppMainCall(M_CALLBACK_NETMON, 0, 0);
			break;
		}
		case IDC_PCMGR_REGEDIT: {
			WCHAR pat[MAX_PATH];
			wcscpy_s(pat, appDir);
			wcscat_s(pat, L"\\PCMgrRegedit.exe");
			MRunExe(pat, NULL, FALSE, hWndMain);
			break;
		}
		case IDC_PCMGR_FILEMGR: {
			MAppMainCall(M_CALLBACK_FILEMGR, 0, 0);
			break;
		}
		case IDM_DBGVIEW: {
			if (MCanUseKernel())
				MShowMyDbgView();
			else MShowErrorMessage(L"", L"Kernel not load.");
			break;
		}
		case IDM_RELOADPDB: {
			if (MCanUseKernel())
			{
				MAppMainCall(M_CALLBACK_SHOW_LOAD_STATUS, 0, 0);
				KNTOSVALUE kNtosValue = { 0 };
				MLoadKernelNTPDB(&kNtosValue, M_CFG_GetConfigBOOL(L"UseKrnlPDB", L"Configure", true));
				MAppMainCall(M_CALLBACK_HLDE_LOAD_STATUS, 0, 0);
			}
			else MShowErrorMessage(L"", L"Kernel not load.");
			break;
		}
		case IDM_SETTO:
		case ID_MAINWINMENU_SETTO: {
			if (IsWindow(selectItem4))
			{
				if (IsIconic(selectItem4))
					ShowWindow(selectItem4, SW_RESTORE);
				SetForegroundWindow(selectItem4);
			}
			break;
		}
		case ID_MAINWINMENU_SPYWIN: {
			if (hEnumWinsCallBack)
				hEnumWinsCallBack(selectItem4, hWnd);
			break;
		}
		case ID_MAINWINMENU_END: {
			if (IsWindow(selectItem4))
				SendMessage(selectItem4, WM_SYSCOMMAND, SC_CLOSE, NULL);
			else SendMessage(hWndMain, WM_COMMAND, 41012, 0);
			break;
		}
		case ID_MAINWINMENU_MAX: {
			if (IsWindow(selectItem4))
				ShowWindow(selectItem4, SW_MAXIMIZE);
			break;
		}
		case ID_MAINWINMENU_MIN: {
			if (IsWindow(selectItem4))
				ShowWindow(selectItem4, SW_MINIMIZE);
			break;
		}
		case ID_MAINWINMENU_BRINGFORNT: {
			if (IsWindow(selectItem4))
				SetForegroundWindow(selectItem4);
			break;
		}
		case IDM_CANCELTOP: {
			if (IsWindow(selectItem4))
			{
				LONG exstytle = GetWindowLong(selectItem4, GWL_EXSTYLE);
				if ((exstytle & WS_EX_TOPMOST) == WS_EX_TOPMOST) {
					exstytle ^= WS_EX_TOPMOST;
					LONG rs = SetWindowLong(selectItem4, GWL_EXSTYLE, exstytle);
					if (rs == 0 && GetLastError() == 5) {
						HANDLE hThread;
						if (NT_SUCCESS(MOpenThreadNt(GetWindowThreadProcessId(selectItem4, NULL), &hThread, 0)))
							MTerminateThreadNt(hThread);
					}
				}
				break;
			}
		}
		case IDM_SHOWBORDER: {
			if (IsWindow(selectItem4))
			{
				LONG stytle = GetWindowLong(selectItem4, GWL_STYLE);
				stytle |= WS_OVERLAPPEDWINDOW;
				LONG rs = SetWindowLong(selectItem4, GWL_STYLE, stytle);
				if (rs == 0 && GetLastError() == 5) {
					HANDLE hThread;
					if (NT_SUCCESS(MOpenThreadNt(GetWindowThreadProcessId(selectItem4, NULL), &hThread, 0)))
						MTerminateThreadNt(hThread);
				}
				break;
			}
		}
		case ID_SETPRIORTY_REALTIME:
		case ID_SETPRIORTY_HIGH:
		case ID_SETPRIORTY_ABOVENORMAL:
		case ID_SETPRIORTY_NORMAL:
		case ID_SETPRIORTY_BELOWNORMAL:
		case ID_SETPRIORTY_LOW: {
			MAppProcPropertyClassHandleWmCommand(wParam);
			break;
		}
		case ID_TASKMENU_SETAFFINITY: {
			MAppMainCall(M_CALLBACK_SETAFFINITY, (LPVOID)(ULONG_PTR)thisCommandPid, thisCommandhProcess);
			break;
		}
		case ID_FMMAIN_REFESH: {
			MFM_Refesh();
			break;
		}
		case ID_FMMAIN_OPEN: {
			ShellExecute(hWndMain, L"open", fmCurrectSelectFilePath0, NULL, NULL, 5);
			break;
		}
		case ID_FMMAIN_OPENWAY: {
			LPCSTR s = W2A(fmCurrectSelectFilePath0);
			std::string strCmd = FormatString("rundll32 shell32,OpenAs_RunDLL %s", s);
			WinExec(strCmd.c_str(), SW_SHOWNORMAL);
			delete s;
			break;
		}
		case ID_FMMAIN_DEL: {
			MFM_DelFileToRecBinUser();
			break;
		}
		case ID_FMMAIN_REMOVE: {
			MFM_DelFileForeverUser();
			break;
		}
		case ID_FMMAIN_RENAME: {
			MFM_Recall(9, fmCurrectSelectFilePath0);
			break;
		}
		case ID_FMMAIN_COPYTO: {
			if (!MFM_CopyFileToUser())
				MShowErrorMessage(fmCurrectSelectFilePath0, (LPWSTR)(str_item_cantcopyfile.c_str()), MB_ICONWARNING, MB_OK);
			break;
		}
		case ID_FMMAIN_MOVETO: {
			if (!MFM_MoveFileToUser())
				MShowErrorMessage(fmCurrectSelectFilePath0, (LPWSTR)(str_item_cantmovefile.c_str()), MB_ICONWARNING, MB_OK);
			break;
		}
		case ID_FMMAIN_COPYPATH: {
			MCopyToClipboard(fmCurrectSelectFilePath0, wcslen(fmCurrectSelectFilePath0));
			MFM_SetStatus2(9);
			break;
		}
		case ID_FMMAIN_NEW: {

			break;
		}
		case ID_FMMAIN_SHIWHIDEDFILES: {
			MFM_ReSetShowHiddenFiles();
			MFM_Refesh();
			break;
		}
		case ID_FMMAIN_NEWFOLDER: {
			MFM_Recall(10, fmCurrectSelectFilePath0);
			break;
		}
		case ID_FMMAIN_PROP: {
			if (MFM_FileExist(fmCurrectSelectFilePath0))
				MShowFileProp(fmCurrectSelectFilePath0);
			break;
		}
		case ID_FMMAIN_SELALL: {
			MFM_Recall(11, 0);
			break;
		}
		case ID_FMMAIN_NOSEL: {
			MFM_Recall(12, 0);
			break;
		}
		case ID_FMMAIN_RESEL: {
			MFM_Recall(13, 0);
			break;
		}
		case ID_FMMAIN_CUT: {
			if (MFM_FileExist(fmCurrectSelectFilePath0)) {
				MFM_CopyOrCutFileToClipboard(fmCurrectSelectFilePath0, FALSE);
				MFM_SetStatus2(5);
			}
			break;
		}
		case ID_FMMAIN_PTASE: {
			MFM_RenameFile();
			break;
		}
		case ID_FMMAIN_COPY: {
			if (MFM_FileExist(fmCurrectSelectFilePath0)) {
				MFM_CopyOrCutFileToClipboard(fmCurrectSelectFilePath0, FALSE);
				MFM_SetStatus2(6);
			}
			break;
		}
		case ID_FMMAIN_FORCE_REMOVE: {
			if (MFM_FileExist(fmCurrectSelectFilePath0)) {
				MFM_DeleteFileForce(fmCurrectSelectFilePath0);
			}
			break;
		}
		case ID_FMMAIN_CNECK_USING: {
			if (MFM_FileExist(fmCurrectSelectFilePath0))
				MFM_Recall(20, fmCurrectSelectFilePath0);
			break;
		}
		case ID_FMM_READONLY: {
			if (MFM_FileExist(fmCurrectSelectFilePath0))
				MFM_SetFileArrtibute(fmCurrectSelectFilePath0, FILE_ATTRIBUTE_READONLY);
			break;
		}
		case ID_FMM_HIDDEN: {
			if (MFM_FileExist(fmCurrectSelectFilePath0))
				MFM_SetFileArrtibute(fmCurrectSelectFilePath0, FILE_ATTRIBUTE_HIDDEN);
			break;
		}
		case ID_FMM_SYSTEM: {
			if (MFM_FileExist(fmCurrectSelectFilePath0))
				MFM_SetFileArrtibute(fmCurrectSelectFilePath0, FILE_ATTRIBUTE_SYSTEM);
			break;
		}
		case ID_FMFOLDER_PROP: {
			MFF_ShowFolderProp();
			break;
		}
		case ID_FMFOLDER_COPYPATH: {
			MFF_CopyPath();
			break;
		}
		case ID_FMFOLDER_RENAME: {
			MFF_Remane();
			break;
		}
		case ID_FMFOLDER_REMOVE: {
			MFF_DelForever();
			break;
		}
		case ID_FMFOLDER_DEL: {
			MFF_DelToRecBin();
			break;
		}
		case ID_FMFOLDER_PTASE: {
			MFF_Patse();
			break;
		}
		case ID_FMFOLDER_CUT: {
			MFF_Cut();
			break;
		}
		case ID_FMFOLDER_COPY: {
			MFF_Copy();
			break;
		}
		case ID_FMFOLDER_OPENINEXP: {
			MFF_ShowInExplorer();
			break;
		}
		case ID_FMFOLDER_OPEN: {
			MFF_ShowFolder();
			break;
		}
		case ID_FMFOLDER_FORCE_REMOVE: {
			MFF_ForceDel();
			break;
		}
		case ID_SCMAIN_COPYPATH:
		case ID_SCMAIN_DEL:
		case ID_SCMAIN_DISABLE:
		case ID_SCMAIN_AUTOSTART:
		case ID_SCMAIN_NOAUTOSTART:
		case ID_SCMAIN_REBOOT:
		case ID_SCMAIN_START:
		case ID_SCMAIN_STOP:
		case ID_SCMAIN_REFESH:
		case ID_SCMAIN_RESU:
		case ID_SCMAIN_SUSP:
		case ID_SCSMALL_STOPSC:
		case ID_SCSMALL_REBOOTSC:
		case ID_SCSMALL_GOTOSC: {
			return MSCM_HandleWmCommand(wParam);
		}
		case IDC_MENUSTART_DELREG:
		case IDC_MENUSTART_DELREGANDFILE:
		case IDC_MENUSTART_COPYPATH:
		case IDC_MENUSTART_COPYREGPATH:
		case IDC_MENUSTART_OPENPATH:
		case IDC_MENUSTART_OPENREGPATH: {
			return MSM_HandleWmCommand(wParam);
		}
		case ID_MENUDRIVER_REFESH:
		case ID_MENUDRIVER_DELETE:
		case ID_MENUDRIVER_DELETEREGANDFILE:
		case ID_MENUDRIVER_CHECKVERY:
		case ID_MENUDRIVER_CHECK_ALLVERY:
		case ID_MENUDRIVER_SHOWALLDRIVER:
		case ID_MENUDRIVER_COPYPATH:
		case ID_MENUDRIVER_COPYREG:
		case ID_MENUDRIVER_OPENPATH:
		case ID_MENUDRIVER_SHOWPROP:
		case ID_MENUDRIVER_UNLOAD:
		case ID_MENUDRIVER_START_BOOT:
		case ID_MENUDRIVER_START_SYSTEM:
		case ID_MENUDRIVER_START_AUTO:
		case ID_MENUDRIVER_START_DEMAND:
		case ID_MENUDRIVER_START_DISABLE: {
			return M_SU_EnumKernelModuls_HandleWmCommand(wParam);
		}
		case IDC_SOFTACT_SHOWSPY:
		case IDC_SOFTACT_SHOWFILETOOL:
		case IDC_SOFTACT_SHOWDRIVER_LOADERTOOL: {
			MAppWmCommandTools(wParam);
			break;
		}
		case IDM_START_MHOOK: {
			if(InstallMouseHook()) MessageBox(hWnd, L"Mouse hook started!", L"Tip", 0);
			else {
				LogErr2(L"Install Mouse hook failed, lasterr : %d", GetLastError());
				MessageBox(hWnd, L"Install Mouse hook failed!", L"Tip", 0);
			}
			break;
		}
		case IDM_END_MHOOK: {
			if (UninstallMouseHook())
				MessageBox(hWnd, L"Mouse hook stopped!", L"Tip", 0);
			break;
		}
		case IDM_SE_AOT: {
			if (aopTimer) {
				KillTimer(hWnd, ID_AOP); aopTimer = false;
				MAppMainCall(M_CALLBACK_SW_AOP_WND, (LPVOID)2, (LPVOID)L"Aoptimer stopped!");
			}
			else {
				SetTimer(hWnd, ID_AOP, 10000, NULL); aopTimer = true;
				MAppMainCall(M_CALLBACK_SW_AOP_WND, (LPVOID)2, (LPVOID)L"Aoptimer started!");
			}
			break;
		}
		case IDC_SOFTACT_TEST1: {
			Log(L"IDC_SOFTACT_TEST1");
			ULONG_PTR addrWin32k = 0;
			MGetNtosAndWin32kfullNameAndStartAddress(NULL, 0, NULL, &addrWin32k);
			Log(L"Win32k StartAddress : %X", addrWin32k);
			break;
		}
		case IDC_SOFTACT_TEST2: {
			M_UWP_EnumUWPApplications();
			break;
		}
		case IDC_SOFTACT_TEST3: {
			M_SU_PrintInternalFuns();
			break;
		}
		case ID_SIMPPROC_ENDTASK: {
			MAppMainCall(M_CALLBACK_SIMPLEVIEW_ACT, (LPVOID)1, 0);
			break;
		}
		case ID_SIMPPROC_SETTO: {
			MAppMainCall(M_CALLBACK_SIMPLEVIEW_ACT, 0, 0);
			break;
		}
		case ID_USER_CONNECT:
		case ID_USER_DISCONNECT:
		case ID_USER_LOGOOFF: {
			return MUsersHandleWmCommand(wParam);
		}
		default:
			break;
		}
		break;
	}
	case WM_SYSCOMMAND: {
		if (wParam == SC_MINIMIZE)
		{
			if (min_hide) ShowWindow(hWnd, SW_HIDE);
		}
		break;
	}
	case WM_TIMER: {
		switch (wParam)
		{
		case ID_AOP: {
			if (top_most && alwaysOnTop) {

				POINT  _mousepoint;
				GetCursorPos(&_mousepoint);
				HWND hWndPoint = WindowFromPoint(_mousepoint);
				LONG exstytle = GetWindowLong(hWndPoint, GWL_EXSTYLE);
				if (hWndPoint != hWndMain && (exstytle & WS_EX_TOPMOST) == WS_EX_TOPMOST) {
					//Top and not this window
					DWORD pid = 0, tid = GetWindowThreadProcessId(hWndPoint, &pid);
					if (pid == GetCurrentProcessId()) break;
					//Show a dialog to ask if want to close this window
					MAppMainCall(M_CALLBACK_SW_AOP_WND, (LPVOID)2, (LPVOID)L"AopCheck finished and checked");
					MAppMainCall(M_CALLBACK_CLEAR_ILLEGAL_TOP_WND, hWndPoint, (LPVOID)(ULONG_PTR)tid);
				}
				else if (GetForegroundWindow() != hWndMain) {
					LONG exthis = GetWindowLong(hWndMain, GWL_EXSTYLE);
					if ((exthis & WS_EX_TOPMOST) != WS_EX_TOPMOST)
					{
						exthis |= WS_EX_TOPMOST;
						SetWindowLong(hWndMain, GWL_EXSTYLE, exthis);
						SetForegroundWindow(hWndMain);
					}
					MAppMainCall(M_CALLBACK_SW_AOP_WND, (LPVOID)2, (LPVOID)L"AopCheck finished reactived");
				}
				else {
					LONG exthis = GetWindowLong(hWndMain, GWL_EXSTYLE);
					if ((exthis & WS_EX_TOPMOST) != WS_EX_TOPMOST)
					{
						exthis |= WS_EX_TOPMOST;
						SetWindowLong(hWndMain, GWL_EXSTYLE, exthis);
						SetForegroundWindow(hWndMain);
					}
					MAppMainCall(M_CALLBACK_SW_AOP_WND, (LPVOID)2, (LPVOID)L"AopCheck finished and not checked");
				}
				break;
			}
		default:
			break;
		}
		}
	}
	default:
		break;
	}
	return 0;
}
INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	UNREFERENCED_PARAMETER(lParam);
	switch (message)
	{
	case WM_INITDIALOG:
		SendMessage(hDlg, WM_SETICON, ICON_SMALL, (LPARAM)LoadIcon(hInstRs, MAKEINTRESOURCE(IDI_ICONAPP)));
		return (INT_PTR)TRUE;
	case WM_SYSCOMMAND:
		if (wParam == SC_CLOSE)
		{
			EndDialog(hDlg, LOWORD(wParam));
			return (INT_PTR)TRUE;
		}
		return 0;
	case WM_CTLCOLORSTATIC:
		if ((HWND)lParam == GetDlgItem(hDlg, IDC_TITLE))
		{
			SetBkColor((HDC)wParam, RGB(0xFF, 0xFF, 0xFF));
			HBRUSH hBrush = CreateSolidBrush(RGB(0xFF, 0xFF, 0xFF));
			return (LRESULT)hBrush;
		}
	case WM_CTLCOLORDLG: {
		HBRUSH hBrush = CreateSolidBrush(RGB(0xFF, 0xFF, 0xFF));
		return (LRESULT)hBrush;
	}
	case WM_COMMAND:
		if (LOWORD(wParam) == IDOK)
		{
			EndDialog(hDlg, LOWORD(wParam));
			return (INT_PTR)TRUE;
		}
		break;
	}
	return (INT_PTR)FALSE;
}
LRESULT CALLBACK MProcListWinProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_PAINT: {
		if (procListLock)return 0;
		else return procListWndProc(hWnd, msg, wParam, lParam);
	}
	case WM_NOTIFY: {
		int loword = LOWORD(wParam);
		if (loword == IDC_MAINLIST_HEADER)
		{
			switch (((LPNMHDR)lParam)->code)
			{
			case NM_RCLICK: {
				int index = - 1;
				LPNMHDR lpnmh = (LPNMHDR)lParam;
				DWORD pos = GetMessagePos();

				RECT rcCol = { 0 };
				POINT pt = { GET_X_LPARAM(pos), GET_Y_LPARAM(pos) };
				ScreenToClient(hListHeaderMainProcList, &pt);
				for (int i = 0; Header_GetItemRect(hListHeaderMainProcList, i, &rcCol); i++)
				{
					if (rcCol.left<pt.x && rcCol.right>pt.x
						&& rcCol.top<pt.y && rcCol.bottom>pt.y)
					{
						index = i;
						break;
					}
				}
				if (index >= 0) MAppMainCall(M_CALLBACK_MDETALS_LIST_HEADER_RIGHTCLICK, (LPVOID)(ULONG_PTR)index, 0);
				break;
			}
			}
		}
		break;
	}
	}
	return procListWndProc(hWnd, msg, wParam, lParam);
}
LRESULT CALLBACK MProcListHeaderWinProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_MOUSEMOVE: {
		int index = -1;
		LONG xPos = GET_X_LPARAM(lParam);
		LONG yPos = GET_Y_LPARAM(lParam);
		RECT rcCol = { 0 };
		for (int i = 0; Header_GetItemRect(hListHeaderMainProcList, i, &rcCol); i++)
		{
			if (rcCol.left<xPos && rcCol.right>xPos
				&& rcCol.top<yPos && rcCol.bottom>yPos)
			{
				index = i;
				break;
			}
		}
		if (index >= 0) MAppMainCall(M_CALLBACK_MDETALS_LIST_HEADER_MOUSEMOVE, 
			(LPVOID)(ULONG_PTR)index, 
			(LPVOID)MAKELPARAM(rcCol.left, rcCol.bottom));
		break;
	}
	}
	return procListHeaderWndProc(hWnd, msg, wParam, lParam);
}


//Dialog boxs
int MShowMessageDialog(HWND hwnd, LPWSTR text, LPWSTR title, LPWSTR instruction, int ico, int button)
{
	PCWSTR tico = NULL;
	if (ico == MB_ICONERROR)
		tico = TD_ERROR_ICON;
	else if (ico == MB_ICONWARNING)
		tico = TD_WARNING_ICON;
	else if (ico == MB_ICONASTERISK)
		tico = TD_INFORMATION_ICON;

	TASKDIALOG_COMMON_BUTTON_FLAGS tbtn;
	if (button == MB_OK)
		tbtn = TDCBF_OK_BUTTON;
	else if (button == MB_OKCANCEL)
		tbtn = TDCBF_OK_BUTTON | TDCBF_CANCEL_BUTTON;
	else if (button == MB_YESNO)
		tbtn = TDCBF_YES_BUTTON | TDCBF_NO_BUTTON;
	else if (button == MB_YESNOCANCEL)
		tbtn = TDCBF_YES_BUTTON | TDCBF_NO_BUTTON | TDCBF_CANCEL_BUTTON;
	else if (button == MB_ABORTRETRYIGNORE)
		tbtn = TDCBF_RETRY_BUTTON | TDCBF_CANCEL_BUTTON;

	int result = 0;
	TaskDialog(hwnd, hInstRs, title, instruction, text, tbtn, tico, &result);
	return result;
}
int MShowErrorMessage(LPWSTR text, LPWSTR intr, int ico, int btn)
{
	if (ico == 0)ico = MB_ICONERROR;
	return MShowMessageDialog(hWndMain, text, DEFDIALOGGTITLE, intr, ico, btn);
}
int MShowErrorMessageWithLastErr(LPWSTR text, LPWSTR intr, int ico, int btn)
{
	std::wstring w = text;

	LPVOID buf;
	if (FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM, NULL, GetLastError(), MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPTSTR)&buf, 0, NULL))
	{
		w += w += FormatString(L"\n%s (%d)", (LPWSTR)buf, GetLastError());
		LocalFree(buf);
	}
	else w += FormatString(L"\nLastError : %d", GetLastError());
	return MShowMessageDialog(hWndMain, (LPWSTR)w.c_str(), DEFDIALOGGTITLE, intr, ico, btn);
}
void MShowErrorMessageWithNTSTATUS(LPWSTR msg, LPWSTR title, NTSTATUS status)
{
	ThrowErrorAndErrorCodeX(status, msg, title);

}

//TaskDialog
EXTERN_C M_API HRESULT MTaskDialogIndirect(_In_ const TASKDIALOGCONFIG *pTaskConfig, _Out_opt_ int *pnButton, _Out_opt_ int *pnRadioButton, _Out_opt_ BOOL *pfVerificationFlagChecked)
{
	return TaskDialogIndirect(pTaskConfig, pnButton, pnRadioButton, pfVerificationFlagChecked);
}
EXTERN_C M_API HRESULT MTaskDialog(_In_opt_ HWND hwndOwner, _In_opt_ HINSTANCE hInstance, _In_opt_ PCWSTR pszWindowTitle, _In_opt_ PCWSTR pszMainInstruction, _In_opt_ PCWSTR pszContent, TASKDIALOG_COMMON_BUTTON_FLAGS dwCommonButtons, _In_opt_ PCWSTR pszIcon, _Out_opt_ int *pnButton)
{
	return TaskDialog(hwndOwner, hInstance, pszWindowTitle, pszMainInstruction, pszContent, dwCommonButtons, pszIcon, pnButton);
}

//Crush annd MiniDump

//Create MiniDump file
int GenerateMiniDump(PEXCEPTION_POINTERS pExceptionPointers)
{
	// 创建 dmp 文件件
	TCHAR szFileName[MAX_PATH] = { 0 };
	LPWSTR szVersion = (LPWSTR)L"\\PCMgr";
	SYSTEMTIME stLocalTime;
	GetLocalTime(&stLocalTime);
	wsprintf(szFileName, L"%s%s-%04d%02d%02d-%02d%02d%02d-Crash!.dmp", appDir,
		szVersion, stLocalTime.wYear, stLocalTime.wMonth, stLocalTime.wDay,
		stLocalTime.wHour, stLocalTime.wMinute, stLocalTime.wSecond);

	HANDLE hDumpFile = CreateFile(szFileName, GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_WRITE | FILE_SHARE_READ, 0, CREATE_ALWAYS, 0, 0);

	if (INVALID_HANDLE_VALUE == hDumpFile)
	{
		FLogErr(L"Application crashed!\nDump File Create failed! (ErrorCode: %d)", GetLastError());
		goto QUITCREATEDUMP;
	}

	// 写入 dmp 文件
	MINIDUMP_EXCEPTION_INFORMATION expParam;
	expParam.ThreadId = GetCurrentThreadId();
	expParam.ExceptionPointers = pExceptionPointers;
	expParam.ClientPointers = FALSE;

	MiniDumpWriteDump(GetCurrentProcess(), GetCurrentProcessId(), 	hDumpFile, MiniDumpWithDataSegs, (pExceptionPointers ? &expParam : NULL), NULL, NULL);

	CloseHandle(hDumpFile);

	if (pExceptionPointers) {
		if (pExceptionPointers->ContextRecord&&pExceptionPointers->ExceptionRecord) {
			FLogErr(L"Application crashed!\nA Dump file was created.\n  Dump file: %s\nPlease send this Error Description file to us.\n\
		        Details:  ExceptionAddress:%d\nExceptionCode:%d\nExceptionFlags:%u\
                ContextFlags:%d\n\
				Dr0:%d\n\
				Dr1:%d\n\
				Dr2:%d\n\
				Dr3:%d\n\
				Dr4:%d\n\
				Dr5:%d\n\
				Dr6:%d\n\
				Dr7:%d\n\
				SegGs:%d\n\
				SegFs:%d\n\
				SegEs:%d\n\
				SegDs:%d\n\
				SegCs:%d\n\
				EFlags:%d\n\
				SegSs:%d\n", __LINE__, __FILE__, __FUNCTION__, szFileName,
				pExceptionPointers->ExceptionRecord->ExceptionAddress,
				pExceptionPointers->ExceptionRecord->ExceptionCode,
				pExceptionPointers->ExceptionRecord->ExceptionFlags,
				pExceptionPointers->ContextRecord->ContextFlags,
				pExceptionPointers->ContextRecord->Dr0,
				pExceptionPointers->ContextRecord->Dr1,
				pExceptionPointers->ContextRecord->Dr2,
				pExceptionPointers->ContextRecord->Dr3,
				pExceptionPointers->ContextRecord->Dr6,
				pExceptionPointers->ContextRecord->Dr7,
				pExceptionPointers->ContextRecord->SegGs,
				pExceptionPointers->ContextRecord->SegFs,
				pExceptionPointers->ContextRecord->SegEs,
				pExceptionPointers->ContextRecord->SegDs,
				pExceptionPointers->ContextRecord->SegCs,
				pExceptionPointers->ContextRecord->EFlags,
				pExceptionPointers->ContextRecord->SegSs);
		}

		M_LOG_Close();

		TerminateProcess(GetCurrentProcess(), 0);
	}

	return EXCEPTION_EXECUTE_HANDLER;
QUITCREATEDUMP:
	return EXCEPTION_CONTINUE_EXECUTION;
}
//UnhandledExceptionFilter
LONG WINAPI MUnhandledExceptionFilter(struct _EXCEPTION_POINTERS *lpExceptionInfo)
{
	// 这里做一些异常的过滤或提示
	if (IsDebuggerPresent())
		return EXCEPTION_CONTINUE_SEARCH;
	return GenerateMiniDump(lpExceptionInfo);
}

HHOOK mouse_Hook = NULL;

//Mouse hook
M_CAPI(BOOL) InstallMouseHook()
{
	if (mouse_Hook)  UninstallMouseHook();
	mouse_Hook = SetWindowsHookEx(WH_MOUSE_LL,
		(HOOKPROC)(LowLevelMouseProc), hInst, NULL);
	return(mouse_Hook != NULL);
}
M_CAPI(BOOL) UninstallMouseHook()
{
	BOOL jud = FALSE;
	if (mouse_Hook) {
		jud = UnhookWindowsHookEx(mouse_Hook);
		mouse_Hook = NULL;  //set NULL  
	}

	return jud;
}

LRESULT CALLBACK LowLevelMouseProc(INT nCode, WPARAM wParam, LPARAM lParam)
{
	POINT  _mousepoint;
	MSLLHOOKSTRUCT *pkbhs = (MSLLHOOKSTRUCT *)lParam;
	switch (nCode)
	{
	case HC_ACTION:
	{
		//鼠标左击  
		if ((top_most && alwaysOnTop) && (wParam == WM_MBUTTONUP || wParam == WM_LBUTTONUP || wParam == WM_RBUTTONUP)) {
			//获取鼠标的位置，并进行必要的判断
			LONG exthis = GetWindowLong(hWndMain, GWL_EXSTYLE);
			if ((exthis & WS_EX_TOPMOST) != WS_EX_TOPMOST)
			{
				exthis |= WS_EX_TOPMOST;
				SetWindowLong(hWndMain, GWL_EXSTYLE, exthis);
				SetForegroundWindow(hWndMain);
			}

			GetCursorPos(&_mousepoint);
			HWND hWndPoint = WindowFromPoint(_mousepoint);
			LONG exstytle = GetWindowLong(hWndPoint, GWL_EXSTYLE);
			if (hWndPoint != hWndMain) {
				//Top and not this window
				DWORD pid = 0, tid = GetWindowThreadProcessId(hWndPoint, &pid);
				if (pid == GetCurrentProcessId()) break;
				//Show a dialog to ask if want to close this window
				MAppMainCall(M_CALLBACK_CLEAR_ILLEGAL_TOP_WND, hWndPoint, (LPVOID)(ULONG_PTR)tid);
			}
		}
		break;
	}
	default:   
		break;
	}

	return CallNextHookEx(NULL, nCode, wParam, lParam);
}




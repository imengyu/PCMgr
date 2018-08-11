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
#include "StringHlp.h"
#include "loghlp.h"
#include "nthlp.h"
#include <shellapi.h>
#include <Vsstyle.h>
#include <vssym32.h>
#include <Uxtheme.h>
#include <string.h>
#include <string>
#include <dbghelp.h>
#include <mscoree.h>
#include <Metahost.h>

#pragma comment(linker,"\"/manifestdependency:type='win32' \
name = 'Microsoft.Windows.Common-Controls' version = '6.0.0.0' \
processorArchitecture = '*' publicKeyToken = '6595b64144ccf1df' language = '*'\"")

#define WM_S_APPBAR 900
#define WM_S_MESSAGE_EXIT 901

extern HINSTANCE hInst;
extern HINSTANCE hInstRs;

extern _CancelShutdown dCancelShutdown;

extern LPWSTR fmCurrectSelectFilePath0;
extern bool fmMutilSelect;
extern int fmMutilSelectCount;

typedef HRESULT(WINAPI*CLRCreateInstanceFun)(REFCLSID clsid, REFIID riid, LPVOID *ppInterface);

HMENU hMenuMain;
exitcallback hMainExitCallBack;
extern HWND hWndMain;

EnumWinsCallBack hEnumWinsCallBack;
GetWinsCallBack hGetWinsWinsCallBack;
CLRCreateInstanceFun _CLRCreateInstance;
WorkerCallBack hWorkerCallBack;
TerminateImporantWarnCallBack hTerminateImporantWarnCallBack;
HANDLE hMutex;

WCHAR appDir[MAX_PATH];
DWORD dwMainAppRet = 0;


int menu_last_x = 0,
menu_last_y = 0;
HMENU hMenuMainFile;
HMENU hMenuMainSet;
HMENU hMenuMainView;
HWND selectItem4;

int HotKeyId = 0;
bool has_fullscreen_window = false;

extern BOOL killUWPCmdSendBack;
extern BOOL killCmdSendBack;
bool refesh_fast = false;
bool refesh_paused = false;
bool min_hide = false;
bool close_hide = false;
bool top_most = false;

bool can_debug = false;
bool use_apc = false;

void print(LPWSTR str)
{
	MessageBox(0, str, DEFDIALOGGTITLE, MB_OK);
}

bool MLoadAppBackUp();
LONG WINAPI MUnhandledExceptionFilter(struct _EXCEPTION_POINTERS *lpExceptionInfo);

M_API BOOL MAppStartEnd() {
	return CloseHandle(hMutex);
}
M_API BOOL MAppStartTryCloseLastApp(LPWSTR windowTitle) {
	HWND hWnd = FindWindow(NULL, windowTitle);
	if (IsWindow(hWnd))
	{
		if (SendMessageTimeout(hWnd, WM_S_MESSAGE_EXIT, NULL, NULL, SMTO_BLOCK, 500, 0) == 0)
			return FALSE;
		else return TRUE;
	}		
	return FALSE;
}
M_API BOOL MAppKillOld(LPWSTR procName)
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
		if (MStrEqualW(pe.szExeFile, procName))
			if (pe.th32ProcessID != GetCurrentProcessId()) {
				Log(L"Killing old PCMgr process : %d", pe.th32ProcessID);
				ended = MTerminateProcessNt(pe.th32ProcessID, NULL) == STATUS_SUCCESS;
			}
	}
	CloseHandle(hSnapshot);
	return ended;
}
M_API BOOL MAppStartTest()
{
	hMutex = CreateMutex(NULL, false, L"PCMGR");
	if (GetLastError() == ERROR_ALREADY_EXISTS)
	{
		CloseHandle(hMutex);
		return TRUE;
	}
	return FALSE;
}
M_API void MAppWorkCall2(UINT msg, WPARAM wParam, LPARAM lParam)
{
	SendMessage(hWndMain, msg, wParam, lParam);
}
M_API int MAppWorkCall3(int id, HWND hWnd, void*data)
{
	switch (id)
	{
	case 181: {
		SetUnhandledExceptionFilter(NULL);
		SetUnhandledExceptionFilter(MUnhandledExceptionFilter);
		return 1;
	}
	case 182:
		SetWindowTheme(hWnd, L"Explorer", NULL);
		return 1;
	case 183: {
		hMenuMain = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUMAIN));
		hMenuMainFile = GetSubMenu(hMenuMain, 0);
		hMenuMainSet = GetSubMenu(hMenuMain, 1);
		hMenuMainView = GetSubMenu(hMenuMain, 2);
		if (!MIsRunasAdmin())
			InsertMenu(hMenuMainFile, 1, MF_BYPOSITION, IDM_REBOOT_AS_ADMIN, str_item_rebootasadmin);
		else {
			InsertMenu(hMenuMainFile, 2, MF_BYPOSITION, IDM_UNLOAD_DRIVER, str_item_unloaddriver);
			InsertMenu(hMenuMainFile, 2, MF_BYPOSITION, IDM_LOAD_DRIVER, str_item_loaddriver);
		}
		SetMenu(hWnd, hMenuMain);
		hWndMain = hWnd;

		if (MFM_FileExist(L"C:\\Windows\\System32\\vsjitdebugger.exe"))
			can_debug = true;

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
	case 185:
		ExitWindowsEx(EWX_REBOOT, 0);
		break;
	case 186:
		ExitWindowsEx(EWX_LOGOFF, 0);
		break;
	case 187:
		ExitWindowsEx(EWX_SHUTDOWN, 0);
		break;
	case 188:
		INITCOMMONCONTROLSEX InitCtrls;
		InitCtrls.dwSize = sizeof(InitCtrls);
		InitCtrls.dwICC = ICC_WIN95_CLASSES;
		InitCommonControlsEx(&InitCtrls);
		break;
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
			if (IsWindow(selectItem4)) {
				if (IsWindowEnabled(selectItem4))
					EnableMenuItem(hpop, ID_WINSMENU_ENABLE, MF_DISABLED);
				else
					EnableMenuItem(hpop, ID_WINSMENU_DISABLE, MF_DISABLED);
				if (!IsWindowVisible(selectItem4))
					EnableMenuItem(hpop, ID_WINSMENU_HIDEWINDOW, MF_DISABLED);
				else
					EnableMenuItem(hpop, ID_WINSMENU_SHOWWND, MF_DISABLED);
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
	case 190:
		SendMessage(hWnd, WM_COMMAND, IDM_KILL, 0);
		break;
	case 191:
		MAppRebot();
		break;
	case 192:
		if (SendMessageTimeout((HWND)data, WM_SYSCOMMAND, SC_CLOSE, 0, SMTO_BLOCK, 500, 0) == 0)
			MKillProcessUser(FALSE);
		break;
	case 193: {
		int c = static_cast<int>((ULONG_PTR)data);
		switch (c)
		{
		case 0:
			refesh_paused = true;
			break;
		case 1:
			refesh_paused = false;
			refesh_fast = false;
			break;
		case 2:
			refesh_paused = false;
			refesh_fast = true;
			break;
		}
		HMENU h = GetSubMenu(hMenuMainView, 1);
		CheckMenuItem(h, IDM_REFESH_FAST, (!refesh_paused && refesh_fast) ? MF_CHECKED : MF_UNCHECKED);
		CheckMenuItem(h, IDM_REFESH_PAUSED, refesh_paused ? MF_CHECKED : MF_UNCHECKED);
		CheckMenuItem(h, IDM_REFESH_SLOW, (!refesh_paused && !refesh_fast) ? MF_CHECKED : MF_UNCHECKED);
		hWorkerCallBack(5, (LPVOID)static_cast<ULONG_PTR>(c), 0);
		break;
	}
	case 194:
		top_most = static_cast<int>((ULONG_PTR)data);
		CheckMenuItem(hMenuMainSet, IDM_TOPMOST, top_most ? MF_CHECKED : MF_UNCHECKED);
		hWorkerCallBack(6, (LPVOID)static_cast<ULONG_PTR>(top_most), 0);
		break;
	case 195:
		close_hide = static_cast<int>((ULONG_PTR)data);
		CheckMenuItem(hMenuMainSet, IDM_CLOSETOHIDE, close_hide ? MF_CHECKED : MF_UNCHECKED);
		hWorkerCallBack(6, (LPVOID)static_cast<ULONG_PTR>(close_hide), 0);
		break;
	case 196:
		min_hide = static_cast<int>((ULONG_PTR)data);
		CheckMenuItem(hMenuMainSet, IDM_MINHIDE, min_hide ? MF_CHECKED : MF_UNCHECKED);
		hWorkerCallBack(7, (LPVOID)static_cast<ULONG_PTR>(min_hide), 0);
		break;
	case 197:
		if (data)
			MSCM_SetCurrSelSc((LPWSTR)data);
		break;
	case 198:
		selectItem4 = (HWND)data;
		break;
	case 199:
		wcscpy_s(appDir, (LPWSTR)data);
		break;
	case 200:
		ShowWindow(hWnd, SW_HIDE);
		break;
	case 201:
		if (MCanUseKernel())
			M_SU_ForceShutdown();
		break;
	case 202:
		if (MCanUseKernel())
			M_SU_ForceReboot();
		break;
	case 203:
		M_SU_ProtectMySelf();
		break;
	case 204:
		M_SU_UnProtectMySelf();
		break;
	case 205:
		ShowWindow(hWnd, SW_SHOW);
		break;
	case 206:
		use_apc = (BOOL)(ULONG_PTR)data;
		break;
	case 207:
		UnregisterHotKey(hWnd, HotKeyId);
		break;
	case 208:
		if (!IsWindowVisible(hWnd)) 
			ShowWindow(hWnd, SW_SHOW);
		if (IsIconic(hWnd))
			ShowWindow(hWnd, SW_RESTORE);
		if (has_fullscreen_window)
			SendMessage(hWnd, WM_COMMAND, IDM_TOPMOST, 0);
		SetForegroundWindow(hWnd);
		break;
	case 209:
		APPBARDATA abd;
		memset(&abd, 0, sizeof(abd));
		abd.cbSize = sizeof(APPBARDATA);
		abd.hWnd = hWnd;
		abd.uCallbackMessage = WM_S_APPBAR;
		SHAppBarMessage(ABM_NEW, &abd);
		break;
	case 210:
		EnableWindow(hWnd, FALSE);
		break;
	case 211:
		EnableWindow(hWnd, TRUE);
		break;
	case 212:
		menu_last_x = (int)(ULONG_PTR)hWnd;
		menu_last_y = (int)(ULONG_PTR)data;
		break;
	default:
		return 0;
		break;
	}
	return 0;
}
M_API void MAppHideCos()
{
	ShowWindow(GetConsoleWindow(), SW_HIDE);
}
M_API void* MAppSetCallBack(void * cp, int id)
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
M_API int MAppRegShowHotKey(HWND hWnd, UINT vkkey, UINT key)
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
	MAppMainCall(34, text, NULL);
}

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

M_API BOOL MAppMainCanRun()
{
	if (!MIsSystemSupport()) 
	{
		MPrintErrorMessage((LPWSTR)L"Application not support your Windows, Running this program requires Windows 7 at least.", MB_ICONERROR);
		return FALSE;
	}
	return TRUE;
}
M_API void MAppMainExit(UINT exitcode)
{
	ExitProcess(exitcode);
}
M_API DWORD MAppMainGetExitCode()
{
	return dwMainAppRet;
}
M_API void MAppMainRun()
{
	M_LOG_Init(M_CFG_GetConfigBOOL(L"ShowDebugWindow", L"Configure", FALSE));
	MLG_SetLanuageItems_NoRealloc();

	GetModuleFileName(NULL, appDir, MAX_PATH);
	PathRemoveFileSpec(appDir);

	WCHAR mainDllPath[MAX_PATH];
	wcscpy_s(mainDllPath, appDir);
#ifdef _AMD64_
	wcscat_s(mainDllPath, L"\\PCMgrApp64.dll");
#else
	wcscat_s(mainDllPath, L"\\PCMgrApp32.dll");
#endif
	if (!MFM_FileExist(mainDllPath))
	{
		MShowErrorMessage(L"Not found Main Dll.", L"Load app failed", MB_ICONERROR);
		LogErr(L"Main Dll missing : %s", mainDllPath);
		return;
	}

	typedef HRESULT (WINAPI*_CLRCreateInstance)(REFCLSID clsid, REFIID riid, LPVOID *ppInterface);

	_CLRCreateInstance c = (_CLRCreateInstance)GetProcAddress(GetModuleHandle(L"mscoree.dll"), "CLRCreateInstance");
	if (c)
	{
		ICLRMetaHost *pMetaHost = nullptr;
		ICLRMetaHostPolicy *pMetaHostPolicy = nullptr;
		ICLRRuntimeHost *pRuntimeHost = nullptr;
		ICLRRuntimeInfo *pRuntimeInfo = nullptr;

		HRESULT hr = c(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&pMetaHost);
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

		hr = pRuntimeHost->Start();
		if (FAILED(hr)) { LogErr(L"Start RuntimeHost failed HRESULT : 0x%08X", hr); goto cleanup; }

		LogInfo(L"Load main app : %s", mainDllPath);
		hr = pRuntimeHost->ExecuteInDefaultAppDomain(mainDllPath, L"PCMgr.Program", L"ProgramEntry", GetCommandLine(), &dwMainAppRet);
		if (FAILED(hr)) LogErr(L"ExecuteInDefaultAppDomain %s failed HRESULT : 0x%08X", mainDllPath, hr);
		hr = pRuntimeHost->Stop();

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
	else {
		MShowErrorMessage(L"Not found .NET framework in your computer.", L"Load app failed", MB_ICONERROR);
		LogErr(L"Load mscoree.dll failed !");
	}
}
M_API int MAppMainGetArgs(LPWSTR cmdline) {
	int argc = 0;
	argsStrs = CommandLineToArgvW(cmdline, &argc);

	return argc;
}
M_API LPWSTR MAppMainGetArgsStr(int index)
{
	return argsStrs[index];
}
M_API void MAppMainGetArgsFreAall() {
	if (argsStrs) LocalFree(argsStrs);
}

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

M_API void MAppExit() {
	if (hMainExitCallBack)
		hMainExitCallBack();
}
M_API void MAppRebot() {

	TCHAR exeFullPath[MAX_PATH];
	GetModuleFileName(NULL, exeFullPath, MAX_PATH);
	ShellExecute(NULL, L"open", exeFullPath, NULL, NULL, 5);

	if (hMainExitCallBack)
		hMainExitCallBack();
}
M_API void MAppRebotAdmin() {

	TCHAR exeFullPath[MAX_PATH];
	GetModuleFileName(NULL, exeFullPath, MAX_PATH);
	if (static_cast<int>((ULONG_PTR)ShellExecute(NULL, L"runas", exeFullPath, NULL, NULL, 5)) > 32) {
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
	if (static_cast<int>((ULONG_PTR)ShellExecute(NULL, L"runas", exeFullPath, agrs, NULL, 5)) > 32) {
		if (hMainExitCallBack)
			hMainExitCallBack();
	}
	else {
		if (GetLastError() == ERROR_CANCELLED) {
			MShowMessageDialog(hWndMain, str_item_noadmin1, str_item_tip, str_item_noadmin2);
		}
	}
}
M_API void MListDrawItem(HWND hWnd, HDC hdc, int x, int y, int w, int h, int state)
{
	HTHEME hTheme = NULL;
	RECT rc;
	rc.left = x;
	rc.top = y;
	rc.right = x + w;
	rc.bottom = y + h;
	switch (state)
	{
	case 1:
		hTheme = OpenThemeData(hWnd, L"LISTVIEW");
		DrawThemeBackground(hTheme, hdc, TVP_TREEITEM, LISS_HOTSELECTED, &rc, &rc);
		break;
	case 2:
		hTheme = OpenThemeData(hWnd, L"LISTVIEW");
		DrawThemeBackground(hTheme, hdc, TVP_TREEITEM, LISS_SELECTEDNOTFOCUS, &rc, &rc);
		break;
	case 3:
		hTheme = OpenThemeData(hWnd, L"LISTVIEW");
		DrawThemeBackground(hTheme, hdc, TVP_TREEITEM, LISS_HOT, &rc, &rc);
	case 4:
		hTheme = OpenThemeData(hWnd, L"LISTVIEW");
		DrawThemeBackground(hTheme, hdc, TVP_TREEITEM, LISS_SELECTED, &rc, &rc);
		break;
	case 5:
		hTheme = OpenThemeData(hWnd, L"TREEVIEW");
		DrawThemeBackground(hTheme, hdc, TVP_GLYPH, GLPS_CLOSED, &rc, &rc);
		break;
	case 6:
		hTheme = OpenThemeData(hWnd, L"TREEVIEW");
		DrawThemeBackground(hTheme, hdc, TVP_GLYPH, GLPS_OPENED, &rc, &rc);
		break;
	case 7:
		hTheme = OpenThemeData(hWnd, L"TREEVIEW");
		DrawThemeBackground(hTheme, hdc, TVP_HOTGLYPH, HGLPS_CLOSED, &rc, &rc);
		break;
	case 8:
		hTheme = OpenThemeData(hWnd, L"TREEVIEW");
		DrawThemeBackground(hTheme, hdc, TVP_HOTGLYPH, HGLPS_OPENED, &rc, &rc);
		break;
	default:
		break;
	}
	if (hTheme != NULL)CloseThemeData(hTheme);
}

void MAppWmCommandTools(WPARAM wParam)
{
	switch (wParam)
	{
	case IDC_SOFTACT_SHOWDRIVER_LOADERTOOL: 
		MAppMainCall(16, GetDesktopWindow(), 0);
		break;
	case IDC_SOFTACT_SHOWSPY:
		MAppMainCall(12, GetDesktopWindow(), 0);
		break;
	case IDC_SOFTACT_SHOWFILETOOL:
		MAppMainCall(13, 0, 0);
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
		LPWSTR ntstatusstr = MNtstatusToStr(code);
		if (ntstatusstr==0)
		wsprintf(errcode, L"\nCode : %d\nNTSTATUS : 0x%lX", code, code);
		else wsprintf(errcode, L"\nCode : %d\n%s (0x%lX)", code, ntstatusstr, code);
	}
	else wsprintf(errcode, L"\nError Code : %d", code);
	MShowMessageDialog(hWndMain, errcode, title, msg, MB_ICONERROR, MB_OK);
}

extern DWORD thisCommandPid;
extern LPWSTR thisCommandPath;
extern LPWSTR thisCommandName;
extern LPWSTR thisCommandUWPName;
extern BOOL thisCommandIsImporant;
extern BOOL thisCommandIsVeryImporant;

LRESULT MAppWinProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
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
	case WM_COMMAND:
		switch (wParam)
		{
		case IDM_LOAD_DRIVER: {
			MAppMainCall(22, 0, 0);
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
		case IDM_MINHIDE: {
			MAppWorkCall3(196, 0, (LPVOID)static_cast<ULONG_PTR>(!min_hide));
			break;
		}
		case IDM_CLOSETOHIDE: {
			MAppWorkCall3(195, 0, (LPVOID)static_cast<ULONG_PTR>(!close_hide));
			break;
		}
		case IDM_REFESH_FAST: {
			MAppWorkCall3(193, NULL, (LPVOID)(ULONG_PTR)(2));
			break;
		}
		case IDM_REFESH_PAUSED: {
			MAppWorkCall3(193, NULL, 0);
			break;
		}
		case IDM_REFESH_SLOW: {
			MAppWorkCall3(193, NULL, (LPVOID)(ULONG_PTR)1);
			break;
		}
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
			MAppMainCall(14, 0, 0);
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
				MAppMainCall(15, (LPVOID)(ULONG_PTR)thisCommandPid, 0);
			else if (killUWPCmdSendBack)
				MAppMainCall(37, (LPVOID)thisCommandUWPName, 0);
			else MKillProcessUser(TRUE);
			break;
		}
		case IDM_KILLKERNEL: {
			MFroceKillProcessUser();
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
				MAppMainCall(23, (LPVOID)(ULONG_PTR)thisCommandPid, thisCommandName);
			break;
		}
		case IDM_SUPROC: {
			if (thisCommandPid > 4)
			{
				if (thisCommandIsVeryImporant && !hTerminateImporantWarnCallBack(thisCommandName, 4)) break;
				if (!thisCommandIsVeryImporant && thisCommandIsImporant && !hTerminateImporantWarnCallBack(thisCommandName, 2)) break;
				NTSTATUS status = MSuspendProcessNt(thisCommandPid, NULL);
				if (status == STATUS_INVALID_HANDLE) {
					MShowErrorMessage((LPWSTR)str_item_invalidproc.c_str(), (LPWSTR)str_item_op_failed.c_str(), MB_ICONWARNING, MB_OK);
					SendMessage(hWndMain, WM_COMMAND, 41012, 0);
				}
				else if (status == STATUS_ACCESS_DENIED)
					MShowErrorMessage((LPWSTR)str_item_access_denied.c_str(), (LPWSTR)str_item_kill_failed.c_str(), MB_ICONERROR, MB_OK);
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
			MAppMainCall(27, (LPVOID)(ULONG_PTR)thisCommandPid, thisCommandName);
			break;
		}
		case IDM_VTIMER: {
			if (thisCommandPid > 4)
				MAppMainCall(28, (LPVOID)(ULONG_PTR)thisCommandPid, thisCommandName);
			break;
		}
		case IDM_VHOTKEY: {
			if (thisCommandPid > 4)
				MAppMainCall(29, (LPVOID)(ULONG_PTR)thisCommandPid, thisCommandName);
			break;
		}
		case IDM_DEBUG: {
			if (thisCommandPid > 4)
			{
				std::wstring cmd = FormatString(L"-p %d", thisCommandPid);
				ShellExecute(hWndMain, L"open", L"C:\\Windows\\System32\\vsjitdebugger.exe", (LPWSTR)cmd.c_str(), NULL, 5);
			}
			break;
		}
		case IDM_SGINED: {
			if (thisCommandPath)
			{
				if (MFM_FileExist(thisCommandPath))
				{
					if (MGetExeFileTrust(thisCommandPath))
						MAppMainCall(30, thisCommandPath, 0);
					else MShowMessageDialog(hWndMain, thisCommandPath, str_item_tip, str_item_filenottrust, 0, 0);
				}
				else MShowMessageDialog(hWndMain, str_item_filenotexist, (LPWSTR)str_item_op_failed.c_str(), L"");
			}
			break;
		}
		case IDC_PCMGR_CMD: {
#ifdef _AMD64_
			if(MFM_FileExist(L"PCMgrCmd64.exe"))
			    MFM_OpenFile(L"PCMgrCmd64.exe", hWnd);
#else
			if (MFM_FileExist(L"PCMgrCmd32.exe"))
				MFM_OpenFile(L"PCMgrCmd32.exe", hWnd);
#endif
			break;
		}
		case IDC_SOFTACT_SHOW_KDA: {
			MAppMainCall(32, 0, 0);
			break;
		}
		case ID_MAINWINMENU_SETTO: {
			if (IsWindow(selectItem4))
			{
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
		case IDC_SOFTACT_TEST1: {
			Log(L"Test 1 : %s", L"IDC_SOFTACT_TEST1");
			NTSTATUS status = 0;
			BOOL rs = M_SU_TerminateProcessPIDTest(GetCurrentProcessId(), &status);
			if (status != 0 || !rs) {
				LogErr(L"M_SU_TerminateProcessPIDTest failed : 0x%08X", status);
			}
			break;
		}
		case IDC_SOFTACT_TEST2: {
			Log(L"IDC_SOFTACT_TEST2");
			//M_SU_Test("TestString");
			//M_SU_Test2();
			break;
		}
		case IDC_SOFTACT_TEST3: {
			M_SU_PrintInternalFuns();
			break;
		}
		default:
			break;
		}
		break;
	case WM_SYSCOMMAND:
		if (wParam == SC_MINIMIZE)
		{
			if (min_hide) ShowWindow(hWnd, SW_HIDE);
		}
		break;
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

void MPrintErrorMessage(LPWSTR str, int icon)
{
	MShowErrorMessage(str, L"", icon, MB_OK);
}
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
	return MShowMessageDialog(hWndMain, text, DEFDIALOGGTITLE, intr, ico, btn);
}
int MShowErrorMessageWithLastErr(LPWSTR text, LPWSTR intr, int ico, int btn)
{
	std::wstring w = text;
	w += FormatString(L"\nLastError : %d", GetLastError());
	return MShowMessageDialog(hWndMain, (LPWSTR)w.c_str(), DEFDIALOGGTITLE, intr, ico, btn);
}

EXTERN_C M_API HRESULT MTaskDialogIndirect(_In_ const TASKDIALOGCONFIG *pTaskConfig, _Out_opt_ int *pnButton, _Out_opt_ int *pnRadioButton, _Out_opt_ BOOL *pfVerificationFlagChecked)
{
	return TaskDialogIndirect(pTaskConfig, pnButton, pnRadioButton, pfVerificationFlagChecked);
}
EXTERN_C M_API HRESULT MTaskDialog(_In_opt_ HWND hwndOwner, _In_opt_ HINSTANCE hInstance, _In_opt_ PCWSTR pszWindowTitle, _In_opt_ PCWSTR pszMainInstruction, _In_opt_ PCWSTR pszContent, TASKDIALOG_COMMON_BUTTON_FLAGS dwCommonButtons, _In_opt_ PCWSTR pszIcon, _Out_opt_ int *pnButton)
{
	return TaskDialog(hwndOwner, hInstance, pszWindowTitle, pszMainInstruction, pszContent, dwCommonButtons, pszIcon, pnButton);
}

#pragma region StringHlp

M_API void MConvertStrDel(void*str)
{
	delete str;
}
M_API LPWSTR MConvertLPCSTRToLPWSTR(const char * szString)
{
	size_t dwLen = strlen(szString) + 1;
	int nwLen = MultiByteToWideChar(CP_ACP, 0, szString, static_cast<int>(dwLen), NULL, 0);
	LPWSTR lpszPath = new WCHAR[dwLen];
	MultiByteToWideChar(CP_ACP, 0, szString, static_cast<int>(dwLen), lpszPath, nwLen);
	return lpszPath;
}
M_API LPCSTR MConvertLPWSTRToLPCSTR(const WCHAR * szString)
{
	size_t dwLen = wcslen(szString) + 1;
	char *pChar = new char[dwLen];
	WideCharToMultiByte(CP_ACP, 0, szString, -1,
		pChar, static_cast<int>(dwLen), NULL, NULL);
	return pChar;
}

M_API LPWSTR MStrUpW(const LPWSTR str)
{
	size_t len = wcslen(str) + 1;
	_wcsupr_s((wchar_t *)str, len);
	return str;
}
M_API LPCSTR MStrUpA(const LPCSTR str)
{
	size_t len = strlen(str) + 1;
	_strupr_s((char*)str, len);
	return str;
}

M_API LPWSTR MStrLoW(const LPWSTR str)
{
	size_t len = wcslen(str) + 1;
	_wcslwr_s((wchar_t *)str, len);
	return str;
}
M_API LPCSTR MStrLoA(const LPCSTR str)
{
	size_t len = strlen(str) + 1;
	_strlwr_s((char*)str, len);
	return str;
}

M_API BOOL MStrEqualA(const LPCSTR str1, const LPCSTR str2)
{
	return (strcmp(str1, str2) == 0);
}
M_API BOOL MStrEqualW(const wchar_t* str1, const wchar_t* str2)
{
	return (wcscmp(str1, str2) == 0);
}

M_API LPCSTR MIntToStrA(int i)
{
	int n = 1, i2 = i;
	if (i == 0)
		n = 2;
	else
	{
		while (i2)
		{
			i2 = i2 / 10;
			n++;
		}
		if (i < 0)
			n++;
	}

	char *rs = new char[n];
	_itoa_s(i, rs, n, 10);
	return rs;
}
M_API LPWSTR MIntToStrW(int i)
{
	int n = 1, i2 = i;
	if (i == 0)
		n = 2;
	else
	{
		while (i2)
		{
			i2 = i2 / 10;
			n++;
		}
		if (i < 0)
			n++;
	}

	WCHAR *rs = new WCHAR[n];
	_itow_s(i, rs, n, 10);
	return rs;
}

M_API LPCSTR MLongToStrA(long i)
{
	long n = 1, i2 = i;
	if (i == 0)
		n = 2;
	else
	{
		while (i2)
		{
			i2 = i2 / 10;
			n++;
		}
		if (i < 0)
			n++;
	}

	char *rs = new char[n];
	_ltoa_s(i, rs, n, 10);
	return rs;
}
M_API LPWSTR MLongToStrW(long i)
{
	long n = 1, i2 = i;
	if (i == 0)
		n = 2;
	else
	{
		while (i2)
		{
			i2 = i2 / 10;
			n++;
		}
		if (i < 0)
			n++;
	}

	wchar_t *rs = new wchar_t[n];
	_ltow_s(i, rs, n, 10);
	return rs;
}

M_API int MStrToIntA(char* str)
{
	return atoi(str);
}
M_API int MStrToIntW(LPWSTR str)
{
	return _wtoi(str);
}

M_API DWORD MStrSplitA(char* str, const LPCSTR splitStr, LPCSTR * result, char** lead)
{
	if (str)
	{
		char*p = strtok_s(str, splitStr, lead);
		if (p) {
			*result = p;
			return 1;
		}
		else return 0;
	}
	return 0;
}
M_API DWORD MStrSplitW(LPWSTR str, const LPWSTR splitStr, LPWSTR * result, wchar_t ** lead)
{
	if (str)
	{
		wchar_t*p = wcstok_s(str, splitStr, lead);
		if (p) {
			*result = p;
			return 1;
		}
		else return 0;
	}
	return 0;
}

M_API BOOL MStrContainsA(const LPCSTR str, const LPCSTR testStr, LPCSTR *resultStr)
{
	BOOL result = FALSE;
	const char *rs = strstr(str, testStr);
	if (rs) {
		result = TRUE;
		if (resultStr)*resultStr = rs;
	}
	return result;
}
M_API BOOL MStrContainsW(const LPWSTR str, const LPWSTR testStr, LPWSTR *resultStr)
{
	BOOL result = FALSE;
	const wchar_t *rs = wcsstr(str, testStr);
	if (rs) {
		result = TRUE;
		if (resultStr) *resultStr = (LPWSTR)rs;
	}
	return result;
}

M_API int MHexStrToIntW(wchar_t *s)
{
	size_t i, m = lstrlen(s);
	int temp = 0, n;
	for (i = 0; i<m; i++) {
		if (s[i] >= L'A'&&s[i] <= L'F')
			n = s[i] - L'A' + 10;
		else if (s[i] >= L'a'&&s[i] <= L'f')
			n = s[i] - L'a' + 10;
		else n = s[i] - L'0';
		temp = temp * 16 + n;
	}
	return temp;
}
M_API long long MHexStrToLongW(wchar_t *s)
{
	bool isx = false;
	int len = lstrlen(s);
	for (int i = 0; i<len; i++)
	{
		if (s[i] == 'x' || s[i] == 'X') {
			isx = true;
			break;
		}
	}
	int i, m = isx ? lstrlen(s) - 2 : lstrlen(s), n, w = m;
	long long temp = 0;
	for (i = isx ? 2 : 0; i < m; i++) {
		if (s[i] >= L'A'&&s[i] <= L'F')
			n = s[i] - L'A' + 10;
		else if (s[i] >= L'a'&&s[i] <= L'f')
			n = s[i] - L'a' + 10;
		else n = s[i] - L'0';
		w--;
		temp += static_cast<long long>(pow(16, w) * n);
	}
	return temp;
}
#pragma endregion

int GenerateMiniDump(PEXCEPTION_POINTERS pExceptionPointers);

LONG WINAPI MUnhandledExceptionFilter(struct _EXCEPTION_POINTERS *lpExceptionInfo)
{
	// 这里做一些异常的过滤或提示
	if (IsDebuggerPresent())
		return EXCEPTION_CONTINUE_SEARCH;
	return GenerateMiniDump(lpExceptionInfo);
}
int GenerateMiniDump(PEXCEPTION_POINTERS pExceptionPointers)
{
	// 定义函数指针
	typedef BOOL(WINAPI * MiniDumpWriteDumpT)(
		HANDLE,
		DWORD,
		HANDLE,
		MINIDUMP_TYPE,
		PMINIDUMP_EXCEPTION_INFORMATION,
		PMINIDUMP_USER_STREAM_INFORMATION,
		PMINIDUMP_CALLBACK_INFORMATION
		);
	// 从 "DbgHelp.dll" 库中获取 "MiniDumpWriteDump" 函数
	MiniDumpWriteDumpT pfnMiniDumpWriteDump = NULL;
	HMODULE hDbgHelp = LoadLibrary(L"DbgHelp.dll");
	if (NULL == hDbgHelp)
	{
		return EXCEPTION_CONTINUE_EXECUTION;
	}
	pfnMiniDumpWriteDump = (MiniDumpWriteDumpT)GetProcAddress(hDbgHelp, "MiniDumpWriteDump");

	if (NULL == pfnMiniDumpWriteDump)
	{
		FreeLibrary(hDbgHelp);
		return EXCEPTION_CONTINUE_EXECUTION;
	}
	// 创建 dmp 文件件
	TCHAR szFileName[MAX_PATH] = { 0 };
	LPWSTR szVersion = (LPWSTR)L"\\PCMgr";
	SYSTEMTIME stLocalTime;
	GetLocalTime(&stLocalTime);
	wsprintf(szFileName, L"%s%s-%04d%02d%02d-%02d%02d%02d-Crush.dmp", appDir,
		szVersion, stLocalTime.wYear, stLocalTime.wMonth, stLocalTime.wDay,
		stLocalTime.wHour, stLocalTime.wMinute, stLocalTime.wSecond);
	MessageBox(0, szFileName, L"szFileName", 0);

	HANDLE hDumpFile = CreateFile(szFileName, GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_WRITE | FILE_SHARE_READ, 0, CREATE_ALWAYS, 0, 0);

	if (INVALID_HANDLE_VALUE == hDumpFile)
	{
		//WCHAR err[200];
		//wsprintf(err, L"-errreport %s %s %s", L"0", L"未知错误", szFileName);
		//ShellExecute(NULL, L"open", static_AppLoader->GetAppFullPath(), err, NULL, SW_SHOWNORMAL);
		M_LOG_Error_ForceFile(L"Application crashed!\nDump File Create failed! (ErrorCode: %d)", GetLastError());
		FreeLibrary(hDbgHelp);
		return EXCEPTION_CONTINUE_EXECUTION;

	}
	// 写入 dmp 文件
	MINIDUMP_EXCEPTION_INFORMATION expParam;
	expParam.ThreadId = GetCurrentThreadId();
	expParam.ExceptionPointers = pExceptionPointers;
	expParam.ClientPointers = FALSE;
	pfnMiniDumpWriteDump(GetCurrentProcess(), GetCurrentProcessId(),
		hDumpFile, MiniDumpWithDataSegs, (pExceptionPointers ? &expParam : NULL), NULL, NULL);
	// 释放文件
	CloseHandle(hDumpFile);
	FreeLibrary(hDbgHelp);
	if (pExceptionPointers) {
		if (pExceptionPointers->ContextRecord&&pExceptionPointers->ExceptionRecord) {
			M_LOG_Error_ForceFile(L"Application crashed!\nA Dump file was created.\n  Dump file: %s\nPlease send this Error Description file to us.\n\
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
}


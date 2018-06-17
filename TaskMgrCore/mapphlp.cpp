#include "stdafx.h"
#include "mapphlp.h"
#include "resource.h"
#include "prochlp.h"
#include "vprocx.h"
#include "VersionHelpers.h"
#include <shellapi.h>
#include <Vsstyle.h>
#include <vssym32.h>
#include <Uxtheme.h>
#include <string.h>
#include <string>
#include <metahost.h>
#include <mscoree.h>

extern HINSTANCE hInst;

typedef HRESULT(WINAPI*CLRCreateInstanceFun)(REFCLSID clsid, REFIID riid, LPVOID *ppInterface);

HMENU hMenuMain;
exitcallback hMainExitCallBack;
extern HWND hWndMain;
extern void ThrowErrorAndErrorCode(DWORD code, LPWSTR msg, LPWSTR title);
taskdialogcallback hMainTaskDialogCallBack;
EnumWinsCallBack hEnumWinsCallBack;
EnumWinsCallBack hGetWinsWinsCallBack;
CLRCreateInstanceFun _CLRCreateInstance;
HWND selectItem4;

void print(LPWSTR str)
{
	MessageBox(0, str, DEFDIALOGGTITLE, MB_OK);
}
bool MLoadAppBackUp();

M_API int MAppWorkCall3(int id, HWND hWnd, void*data)
{
	switch (id)
	{
	case 182:
		SetWindowTheme(hWnd, L"Explorer", NULL);
		return 1;
	case 183:
		hMenuMain = LoadMenu(hInst, MAKEINTRESOURCE(IDR_MENUMAIN));
		SetMenu(hWnd, hMenuMain);
		hWndMain = hWnd;
		return 1;
	case 184:
		if (MIs64BitOS())
		{
			if (MessageBox(0, L"检测到您在64位系统中运行32位的Task Manager，这将导致某些功能不可用。\n您还要继续运行软件吗？", L"Task Manager 信息", MB_YESNO | MB_ICONWARNING) == IDNO)
				MAppExit();
		}
		break;
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
		HMENU hroot = LoadMenu(hInst, MAKEINTRESOURCE(IDR_MENUWINMAIN));
		if (hroot) {
			HMENU hpop = GetSubMenu(hroot, 0);
			POINT pt;
			GetCursorPos(&pt);
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
	default:
		return 0;
		break;
	}
	return 0;
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
		hMainTaskDialogCallBack = (taskdialogcallback)cp;
		break;
	case 3:
		hEnumWinsCallBack = (EnumWinsCallBack)cp;
		break;
	case 4:
		hGetWinsWinsCallBack = (EnumWinsCallBack)cp;
		break;
	default:
		break;
	}
	return NULL;
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
M_API BOOL MLoadMainApp()
{
	HMODULE hMscoree = LoadLibrary(L"MSCOREE.DLL");
	if (hMscoree)
	{
		_CLRCreateInstance = (CLRCreateInstanceFun)GetProcAddress(hMscoree, "CLRCreateInstance");

		ICLRMetaHost        *pMetaHost = nullptr;
		ICLRMetaHostPolicy  *pMetaHostPolicy = nullptr;
		ICLRRuntimeHost     *pRuntimeHost = nullptr;
		ICLRRuntimeInfo     *pRuntimeInfo = nullptr;

		HRESULT hr = _CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&pMetaHost);
		if (FAILED(hr))
		{
			if (hr == 0x80004001)
				MessageBox(0, L"无法运行程序，因为您的计算机上没有安装.NET Framework 4.0 。\n百度“.NET Framework 4.0”就可以下载安装了。", DEFDIALOGGTITLE, MB_ICONERROR | MB_OK);
			goto CLEAR;
		}
		hr = pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo));
		if (FAILED(hr))
		{
			MessageBox(0, L"无法运行程序，因为您的计算机上没有安装.NET Framework 4.0 。\n百度“.NET Framework 4.0”就可以下载安装了。", DEFDIALOGGTITLE, MB_ICONERROR | MB_OK);
			goto CLEAR;
		}
		hr = pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pRuntimeHost));
		hr = pRuntimeHost->Start();

		DWORD dwRet = 0;
		hr = pRuntimeHost->ExecuteInDefaultAppDomain(L"PCMgrCore32.dll",
			L"TaskMgr.Program",
			L"EntryPoint",
			L"",
			&dwRet);

		hr = pRuntimeHost->Stop();

		CLEAR:
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
	else MessageBox(0, L"无法运行程序，因为您的计算机上没有安装.NET Framework 2.0 。\n百度“.NET Framework 2.0”就可以下载安装了。", DEFDIALOGGTITLE, MB_ICONERROR | MB_OK);
	return true;
}

typedef void(*startfun)();

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

M_API bool MIs64BitOS()
{
	typedef void (WINAPI *LPFN_PGNSI)(LPSYSTEM_INFO);
	bool bRetVal = false;
	SYSTEM_INFO si = { 0 };
	LPFN_PGNSI pGNSI = (LPFN_PGNSI)GetProcAddress(GetModuleHandle(L"kernel32.dll"), "GetNativeSystemInfo");
	if (pGNSI == NULL)
	{
		return false;
	}
	pGNSI(&si);
	if (si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64 ||
		si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_IA64)
		bRetVal = true;
	return bRetVal;
}

M_API void MAppExit() {
	if (hMainExitCallBack)
		hMainExitCallBack();
}
M_API void MAppRebot() {

	TCHAR exeFullPath[256];
	GetModuleFileName(NULL, exeFullPath, MAX_PATH);
	ShellExecute(NULL, L"open", exeFullPath, NULL, NULL, 5);

	if (hMainExitCallBack)
		hMainExitCallBack();
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
M_API bool MGetPrivileges()
{
	HANDLE hToken;
	TOKEN_PRIVILEGES tp;
	TOKEN_PRIVILEGES oldtp;
	DWORD dwSize = sizeof(TOKEN_PRIVILEGES);
	LUID luid;
	TOKEN_PRIVILEGES tkp = { 0 };

	ZeroMemory(&tp, sizeof(tp));
	if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken)) {
		if (GetLastError() == ERROR_CALL_NOT_IMPLEMENTED) return true;
		else return false;
	}
	if (!LookupPrivilegeValue(NULL, SE_SHUTDOWN_NAME, &luid))
	{
		CloseHandle(hToken);
		return false;
	}
	tp.PrivilegeCount = 1;
	tp.Privileges[0].Luid = luid;
	tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	if (!AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), &oldtp, &dwSize)) {
		CloseHandle(hToken);
		return false;
	}
	if (!LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &luid))
	{
		CloseHandle(hToken);
		return false;
	}
	tp.PrivilegeCount = 1;
	tp.Privileges[0].Luid = luid;
	tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	if (!AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), &oldtp, &dwSize)) {
		CloseHandle(hToken);
		return false;
	}
	if (!LookupPrivilegeValue(NULL, SE_LOAD_DRIVER_NAME, &luid))
	{
		CloseHandle(hToken);
		return false;
	}
	tp.PrivilegeCount = 1;
	tp.Privileges[0].Luid = luid;
	tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	if (!AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), &oldtp, &dwSize)) {
		CloseHandle(hToken);
		return false;
	}

	CloseHandle(hToken);
	return true;
}
M_API VOID MShowFileProp(LPWSTR file)
{
	SHELLEXECUTEINFO info = { 0 };
	info.cbSize = sizeof(SHELLEXECUTEINFO);
	info.lpVerb = L"properties";
	info.lpFile = file;
	info.nShow = SW_SHOW;
	info.fMask = SEE_MASK_INVOKEIDLIST;
	ShellExecuteEx(&info);
}

void ThrowErrorAndErrorCodeX(DWORD code, LPWSTR msg, LPWSTR title)
{
	wchar_t errcode[260];
	wsprintf(errcode, L"\nCode: %d\nNTSTATUS:0x%lX", code, code);
	MShowMessageDialog(hWndMain, errcode, title, msg, MB_ICONERROR, MB_OK);
}

extern DWORD thisCommandPid;
extern LPWSTR thisCommandPath;
extern LPWSTR thisCommandName;

LRESULT MAppWinProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_COMMAND:
		switch (wParam)
		{
		case IDM_ABOUT:
			DialogBox(hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
			break;
		case IDM_TEXIT:
			MAppExit();
			break;
		case IDM_TREBOT:
			MAppRebot();
			break;
		case IDM_KILL:
			if (thisCommandPid > 4)
			{
				LPWSTR t1 = MStrAdd(L"你希望结束 ", thisCommandName);
				LPWSTR t2 = MStrAdd(t1, L" 吗？");
				if (MShowMessageDialog(hWndMain, ENDTASKASKTEXT, DEFDIALOGGTITLE, t2, NULL, MB_YESNO) == IDYES)
				{
					HANDLE hProcess;
					int rs = MOpenProcessNt(thisCommandPid, &hProcess);
					if (rs == 1)
					{
						rs = MTerminateProcessNt(hProcess);
						if (rs == 0xC0000022)
							MShowErrorMessage(L"拒绝访问。", L"无法结束进程", MB_ICONERROR, MB_OK);
						else if (rs != 1)
							ThrowErrorAndErrorCodeX(rs, L"无法结束进程，错误代码：", L"无法结束进程");
						else SendMessage(hWndMain, WM_COMMAND, 41012, 0);
					}
					else if (rs == 0xC0000022)
						MShowErrorMessage(L"拒绝访问。", L"无法结束进程", MB_ICONERROR, MB_OK);
					else if (rs == -1) {
						MShowErrorMessage(L"无效进程。", L"无法结束进程", MB_ICONWARNING, MB_OK);
						SendMessage(hWndMain, WM_COMMAND, 41012, 0);
					}
					else ThrowErrorAndErrorCodeX(rs, L"无法打开进程，错误代码：", L"无法结束进程"); 
				}
				delete t1;
				delete t2;
			}
			break;
		case IDM_FILEPROP:
			if (thisCommandPath)
				MShowFileProp(thisCommandPath);
			else MShowErrorMessage(L"无法获取路径。", L"无法完成该操作", MB_ICONERROR, MB_OK);
			break;
		case IDM_OPENPATH: {
			if (thisCommandPath)
			{
				LPWSTR lparm = MStrAddW(L"/select,", thisCommandPath);
				ShellExecuteW(NULL, NULL, L"explorer.exe", lparm, NULL, SW_SHOWDEFAULT);
				delete lparm;
			}
			else MShowErrorMessage(L"无法获取路径。", L"无法完成该操作", MB_ICONERROR, MB_OK);
			break;
		}
		case IDM_VMODULS:
			if (thisCommandPid > 4)
				MAppVProcessModuls(thisCommandPid, hWndMain, thisCommandName);
			break;
		case IDM_VTHREAD:
			if (thisCommandPid > 4)
				MAppVProcessThreads(thisCommandPid, hWndMain, thisCommandName);
			break;
		case IDM_VWINS:
			if (thisCommandPid > 4)
				MAppVProcessWindows(thisCommandPid, hWndMain, thisCommandName);
			break;
		case IDM_SUPROC:
			if (thisCommandPid > 4)
			{
				int rs = MSuspendTaskNt(thisCommandPid);
				if (rs == -1) {
					MShowErrorMessage(L"无效进程。", L"无法完成该操作", MB_ICONWARNING, MB_OK);
					SendMessage(hWndMain, WM_COMMAND, 41012, 0);
				}
				else if (rs != 1)
					ThrowErrorAndErrorCodeX(rs, L"无法暂停进程运行进程，错误代码：", L"无法完成该操作");
				else SendMessage(hWndMain, WM_COMMAND, 41012, 0);
			}
			break;
		case IDM_RESPROC:
			if (thisCommandPid > 4)
			{
				int rs = MRusemeTaskNt(thisCommandPid);
				if (rs == -1) {
					MShowErrorMessage(L"无效进程。", L"无法完成该操作", MB_ICONWARNING, MB_OK);
					SendMessage(hWndMain, WM_COMMAND, 41012, 0);
				}
				else if (rs != 1)
					ThrowErrorAndErrorCodeX(rs, L"无法继续进程运行进程，错误代码：", L"无法完成该操作");
				else SendMessage(hWndMain, WM_COMMAND, 41012, 0);
			}
			break;
		case ID_MAINWINMENU_SETTO:
			if (IsWindow(selectItem4))
			{
				ShowWindow(selectItem4, SW_RESTORE);
				SetForegroundWindow(selectItem4);
			}
			break;
		case ID_MAINWINMENU_SPYWIN:
			if (hEnumWinsCallBack)
				hEnumWinsCallBack(selectItem4, hWnd);
			break;
		case ID_MAINWINMENU_END:
			if (IsWindow(selectItem4))
				SendMessage(selectItem4, WM_SYSCOMMAND, SC_CLOSE, NULL);
			else SendMessage(hWndMain, WM_COMMAND, 41012, 0);
			break;
		case ID_MAINWINMENU_MAX:
			if (IsWindow(selectItem4))
				ShowWindow(selectItem4, SW_MAXIMIZE);
			break;
		case ID_MAINWINMENU_MIN:
			if (IsWindow(selectItem4))
				ShowWindow(selectItem4, SW_MINIMIZE);
			break;
		case ID_MAINWINMENU_BRINGFORNT:
			if (IsWindow(selectItem4))
				SetForegroundWindow(selectItem4);
			break;
		default:
			break;
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
		SendMessage(hDlg, WM_SETICON, ICON_SMALL, (LPARAM)LoadIcon(hInst, MAKEINTRESOURCE(IDI_ICONAPP)));
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
	MShowErrorMessage(L"", str, icon, MB_OK);
}
int MShowMessageDialog(HWND hwnd, LPWSTR text, LPWSTR title, LPWSTR apptl, int ico, int button)
{
	if (hMainTaskDialogCallBack)
		return hMainTaskDialogCallBack(hwnd, text, title, apptl, ico, button);
	return 0;
}
int MShowErrorMessage(LPWSTR text, LPWSTR intr, int ico, int btn)
{
	return MShowMessageDialog(hWndMain, text, DEFDIALOGGTITLE, intr, ico, btn);
}

#pragma region StringHlp

M_API LPWSTR MConvertLPCSTRToLPWSTR(const char * szString)
{
	int dwLen = strlen(szString) + 1;
	int nwLen = MultiByteToWideChar(CP_ACP, 0, szString, dwLen, NULL, 0);
	LPWSTR lpszPath = new WCHAR[dwLen];
	MultiByteToWideChar(CP_ACP, 0, szString, dwLen, lpszPath, nwLen);
	return lpszPath;
}
M_API LPCSTR MConvertLPWSTRToLPCSTR(const WCHAR * szString)
{
	int dwLen = wcslen(szString) + 1;
	char *pChar = new char[dwLen];
	WideCharToMultiByte(CP_ACP, 0, szString, -1,
		pChar, dwLen, NULL, NULL);
	return pChar;
}

M_API LPWSTR MStrUpW(const LPWSTR str)
{
	int len = wcslen(str) + 1;
	_wcsupr_s((wchar_t *)str, len);
	return str;
}
M_API LPCSTR MStrUpA(const LPCSTR str)
{
	int len = strlen(str) + 1;
	_strupr_s((char*)str, len);
	return str;
}

M_API LPWSTR MStrLoW(const LPWSTR str)
{
	int len = wcslen(str) + 1;
	_wcslwr_s((wchar_t *)str, len);
	return str;
}
M_API LPCSTR MStrLoA(const LPCSTR str)
{
	int len = strlen(str) + 1;
	_strlwr_s((char*)str, len);
	return str;
}

M_API LPWSTR MStrAddW(const LPWSTR str1, const LPWSTR str2)
{
	int strlen1 = wcslen(str1);
	int strlen2 = wcslen(str2);
	if (strlen2 == 0)
		return str1;
	if (strlen1 == 0)
		return str2;
	WCHAR *result = new WCHAR[strlen1 + strlen2 + 1];
	wcscpy_s(result, strlen1 + strlen2 + 1, str1);
	wcscat_s(result, strlen1 + strlen2 + 1, str2);
	return result;
}
M_API LPCSTR MStrAddA(const LPCSTR str1, const LPCSTR str2)
{
	size_t strlen1 = strlen(str1);
	size_t strlen2 = strlen(str2);
	if (strlen2 == 0)
		return str1;
	if (strlen1 == 0)
		return str2;
	size_t strlen = strlen1 + strlen2 + 1;
	CHAR *result = new CHAR[strlen];
	strcpy_s(result, strlen, str1);
	strcat_s(result, strlen, str2);
	return result;
}

M_API BOOL MStrEqualA(const LPCSTR str1, const LPCSTR str2)
{
	return (strcmp(str1, str2) == 0);
}
M_API BOOL MStrEqualW(const LPWSTR str1, const LPWSTR str2)
{
	return (lstrcmp(str1, str2) == 0);
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
	for (int i = 0; i<sizeof(s) / sizeof(s[0]); i++)
	{
		if (s[i] == 'x' || s[i] == 'X') {
			isx = true;
			break;
		}
	}
	int i, m = isx ? lstrlen(s) - 2 : lstrlen(s), n;
	long long temp = 0;
	for (i = isx ? 2 : 0; i < m; i++) {
		if (s[i] >= L'A'&&s[i] <= L'F')
			n = s[i] - L'A' + 10;
		else if (s[i] >= L'a'&&s[i] <= L'f')
			n = s[i] - L'a' + 10;
		else n = s[i] - L'0';
		temp = temp * 16 + n;
	}
	return temp;
}
#pragma endregion




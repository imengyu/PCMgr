#include "stdafx.h"
#include "starthlp.h"
#include "resource.h"
#include "fmhlp.h"
#include "lghlp.h"
#include "loghlp.h"
#include "mapphlp.h"
#include "reghlp.h"
#include "StringHlp.h"

extern HINSTANCE hInstRs;
extern HWND hWndMain;

void MSM_OpenAndEnum(HKEY rootkey, LPWSTR path, LPWSTR type, EnumStartupsCallBack callBack)
{
	HKEY hKEY;
	if (RegOpenKeyEx(rootkey, path, 0, KEY_READ, &hKEY) == ERROR_SUCCESS)
	{
		DWORD dwIndex = 0;  // 注册表项的键值索引 
		TCHAR valueName[MAX_PATH] = { 0 }; //保存项下键的名称 
		DWORD length = MAX_PATH;   // 保存返回读取的字符长度 
		BYTE keyData[MAX_PATH] = { 0 }; //保存键的值 
		DWORD lengthData = MAX_PATH;  //保存值得长度 
		DWORD dwType = REG_SZ;	      //键值的类型 
		while (RegEnumValue(hKEY, dwIndex, valueName, &length, 0, &dwType, keyData, &lengthData) == ERROR_SUCCESS)
		{
			if (dwType == REG_SZ || dwType == REG_EXPAND_SZ) {
				TCHAR dwValue[256];
				DWORD dwSzType = REG_SZ;
				DWORD dwSize = sizeof(dwValue);
				if (RegQueryValueEx(hKEY, valueName, 0, &dwSzType, (LPBYTE)&dwValue, &dwSize) == ERROR_SUCCESS) {
					callBack(valueName, type, dwValue, rootkey, path, valueName);
				}LogErr(L"Query key value (%s\\%s) failed in RegQueryValueEx : %d", MREG_ROOTKEYToStr(rootkey), path, GetLastError());
			}
			dwIndex++;
			length = MAX_PATH;
			lengthData = MAX_PATH;
		}
		RegCloseKey(hKEY);
	}
	else LogErr(L"Enum child key (%s\\%s) failed in RegOpenKeyEx : %d", MREG_ROOTKEYToStr(rootkey), path, GetLastError());
}
void MSM_OpenAndEnumWithClsid(HKEY rootkey, LPWSTR path, LPWSTR type, EnumStartupsCallBack callBack)
{
	HKEY hKEY;
	if (RegOpenKeyEx(rootkey, path, 0, KEY_READ, &hKEY) == ERROR_SUCCESS)
	{
		DWORD dwIndex = 0;  // 注册表项的键值索引 
		TCHAR valueName[MAX_PATH] = { 0 }; //保存项下键的名称 
		DWORD length = MAX_PATH;   // 保存返回读取的字符长度 
		BYTE keyData[MAX_PATH] = { 0 }; //保存键的值 
		DWORD lengthData = MAX_PATH;  //保存值得长度 
		DWORD dwType = REG_SZ;	      //键值的类型 
		while (RegEnumValue(hKEY, dwIndex, valueName, &length, 0, &dwType, keyData, &lengthData) == ERROR_SUCCESS)
		{
			if (dwType == REG_SZ || dwType == REG_EXPAND_SZ) {
				if (valueName[0] != L'{') {
					TCHAR dwValue[256];
					DWORD dwSzType = REG_SZ;
					DWORD dwSize = sizeof(dwValue);
					if (RegQueryValueEx(hKEY, valueName, 0, &dwSzType, (LPBYTE)&dwValue, &dwSize) == ERROR_SUCCESS)
					{
						HKEY csidkey = MREG_CLSIDToHKEYInprocServer32(rootkey, dwValue);
						if (csidkey != NULL) {
							TCHAR dwValueInprocServer32AppPath[256];
							DWORD dwSizeInprocServer32AppPath = sizeof(dwValueInprocServer32AppPath);
							if (RegQueryValueEx(hKEY, L"", 0, &dwSzType, (LPBYTE)&dwValueInprocServer32AppPath, &dwSizeInprocServer32AppPath) == ERROR_SUCCESS)
								callBack(valueName, type, dwValueInprocServer32AppPath, rootkey, path, valueName);
							else callBack(valueName, type, dwValue, rootkey, path, valueName);
							RegCloseKey(csidkey);
						}
						else callBack(valueName, type, dwValue, rootkey, path, valueName);
					}LogErr(L"Query key value (%s\\%s) failed in RegQueryValueEx : %d", MREG_ROOTKEYToStr(rootkey), path, GetLastError());
				}
			}
			dwIndex++;
			length = MAX_PATH;
			lengthData = MAX_PATH;
		}
		RegCloseKey(hKEY);
	}
	else LogErr(L"Enum child key (%s\\%s) failed in RegOpenKeyEx : %d", MREG_ROOTKEYToStr(rootkey), path, GetLastError());
}

//SOFTWARE\\Classes\\CLSID\\

void MSM_EnumHC_RUN(EnumStartupsCallBack callBack)
{
	MSM_OpenAndEnum(HKEY_LOCAL_MACHINE, TEXT("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"), L"HKLM Run", callBack);
	MSM_OpenAndEnum(HKEY_CURRENT_USER, TEXT("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"), L"HKCU Run", callBack);
}
void MSM_EnumHC_KnowDlls(EnumStartupsCallBack callBack)
{
	MSM_OpenAndEnum(HKEY_LOCAL_MACHINE, TEXT("SYSTEM\\CurrentControlSet\\Control\\Session Manager\\KnownDLLs"), L"KnownDLLs", callBack);
}
void MSM_EnumHC_RightMenus(EnumStartupsCallBack callBack)
{
	MSM_OpenAndEnumWithClsid(HKEY_LOCAL_MACHINE, TEXT("SOFTWARE\\Classes\\*\\shellex\\ContextMenuHandlers"), L"RightMenu1", callBack);
	MSM_OpenAndEnumWithClsid(HKEY_CURRENT_USER, TEXT("SOFTWARE\\Classes\\*\\shellex\\ContextMenuHandlers"), L"RightMenu2", callBack);
}
void MSM_EnumHC_Prints(EnumStartupsCallBack callBack)
{

}

TCHAR selectedFilePath[MAX_PATH];
HKEY selectedRootkey;
TCHAR selectedKey[MAX_PATH];
TCHAR selectedValue[MAX_PATH];

void MSM_DelSelectedReg()
{

}
void MSM_DelSelectedFile()
{

}

M_CAPI(VOID) MEnumStartups(EnumStartupsCallBack callBack) {
	if (callBack)
	{
		MSM_EnumHC_RUN(callBack);
		MSM_EnumHC_RightMenus(callBack);
		MSM_EnumHC_KnowDlls(callBack);
		MSM_EnumHC_Prints(callBack);
	}
}
M_CAPI(VOID) MStartupsMgr_ShowMenu(HKEY rootkey, LPWSTR path, LPWSTR filepath, LPWSTR regvalue)
{
	HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUSTART));
	if (hroot) {
		HMENU hpop = GetSubMenu(hroot, 0);
		POINT pt;
		GetCursorPos(&pt);

		if (!filepath || MStrEqualW(filepath, L"")) {
			EnableMenuItem(hpop, IDC_MENUSTART_COPYPATH, MF_DISABLED);
			EnableMenuItem(hpop, IDC_MENUSTART_OPENPATH, MF_DISABLED);
		}
		else wcscpy_s(selectedFilePath, filepath);

		selectedRootkey = rootkey;
		wcscpy_s(selectedKey, path);
		wcscpy_s(selectedValue, regvalue);

		TrackPopupMenu(hpop,
			TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
			pt.x, pt.y, 0, hWndMain, NULL);
		DestroyMenu(hroot);
	}
}

LRESULT MSM_HandleWmCommand(WPARAM wParam)
{
	switch (wParam)
	{
	case IDC_MENUSTART_DELREG: {
		if (MessageBox(hWndMain, str_item_delscask, str_item_question, MB_ICONWARNING | MB_YESNO) == IDYES)
			MSM_DelSelectedReg();
		break;
	}
	case IDC_MENUSTART_DELREGANDFILE: {
		if (MessageBox(hWndMain, str_item_delsc2ask, str_item_question, MB_ICONWARNING | MB_YESNO) == IDYES)
		{
			MSM_DelSelectedReg();
			MSM_DelSelectedFile();
		}
		break;
	}
	case IDC_MENUSTART_COPYPATH: {
		if (wcslen(selectedFilePath) > 0 || !MStrEqualW(selectedFilePath, L""))
			MCopyToClipboard(selectedFilePath, wcslen(selectedFilePath));
		break;
	}
	case IDC_MENUSTART_COPYREGPATH: {
		std::wstring path2 = FormatString(L"%s\\%s\\%s", MREG_ROOTKEYToStr(selectedRootkey), selectedKey, selectedValue);
		MCopyToClipboard(path2.c_str(), path2.size());
		break;
	}
	case IDC_MENUSTART_OPENPATH: {
		if (wcslen(selectedFilePath) > 0 || !MStrEqualW(selectedFilePath, L""))
			MFM_ShowInExplorer(selectedFilePath);
		break;
	}
	default:
		break;
	}
	return 0;
}
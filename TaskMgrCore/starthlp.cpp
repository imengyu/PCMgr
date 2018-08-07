#include "stdafx.h"
#include "starthlp.h"
#include "resource.h"
#include "fmhlp.h"
#include "lghlp.h"
#include "loghlp.h"
#include "mapphlp.h"
#include "reghlp.h"
#include "StringHlp.h"
#include <list>

using namespace std;

extern HINSTANCE hInstRs;
extern HWND hWndMain;

void MSM_OpenAndEnumKeyValues(HKEY rootkey, LPWSTR path, LPWSTR type, EnumStartupsCallBack callBack,BOOL isKnowDLLs=FALSE)
{
	HKEY hKEY;
	DWORD err = RegOpenKeyEx(rootkey, path, 0, KEY_READ, &hKEY);
	if (err == ERROR_SUCCESS)
	{
		DWORD dwIndex = 0;  // 注册表项的键值索引 
		TCHAR valueName[MAX_PATH] = { 0 }; //保存项下键的名称 
		DWORD length = MAX_PATH;   // 保存返回读取的字符长度 
		BYTE keyData[MAX_PATH] = { 0 }; //保存键的值 
		DWORD lengthData = MAX_PATH;  //保存值得长度 
		DWORD dwType = REG_SZ;	      //键值的类型 
		while (RegEnumValue(hKEY, dwIndex, valueName, &length, 0, &dwType, keyData, &lengthData) == ERROR_SUCCESS)
		{
			if (isKnowDLLs && MStrEqualW(valueName, L"DllDirectory")) { dwIndex++; continue; }
			if (dwType == REG_SZ || dwType == REG_EXPAND_SZ) {
				TCHAR dwValue[256];
				DWORD dwSzType = REG_SZ;
				DWORD dwSize = sizeof(dwValue);
				err = RegQueryValueEx(hKEY, valueName, 0, &dwSzType, (LPBYTE)&dwValue, &dwSize);
				if (RegQueryValueEx(hKEY, valueName, 0, &dwSzType, (LPBYTE)&dwValue, &dwSize) == ERROR_SUCCESS) {
					callBack(valueName, type, dwValue, rootkey, path, valueName);
				}else LogErr(L"Query key value (%s\\%s) failed in RegQueryValueEx : %d", MREG_ROOTKEYToStr(rootkey), path, err);
			}
			dwIndex++;
			length = MAX_PATH;
			lengthData = MAX_PATH;
		}
		RegCloseKey(hKEY);
	}
	else LogErr(L"Enum child key (%s\\%s) failed in RegOpenKeyEx : %d", MREG_ROOTKEYToStr(rootkey), path, err);
}
void MSM_OpenAndEnumWithClsid(HKEY rootkey, LPWSTR path, LPWSTR type, EnumStartupsCallBack callBack)
{
	HKEY hKEY;
	DWORD err = RegOpenKeyEx(rootkey, path, 0, KEY_READ, &hKEY);
	if (err == ERROR_SUCCESS)
	{
		//HKEY_LOCAL_MACHINE\SOFTWARE\Classes\*\shellex\ContextMenuHandlers\WinRAR
		DWORD dwIndex = 0;
		TCHAR valueName[MAX_PATH] = { 0 }; //键值名称
		DWORD length = MAX_PATH;  
		DWORD dwType = REG_SZ;

		list<wstring> childKeys;	
		err = RegEnumKeyEx(hKEY, dwIndex, valueName, &length, NULL, NULL, NULL, NULL);
		while (err == ERROR_SUCCESS)
		{
			childKeys.push_back(wstring(valueName));
			dwIndex++;
			length = MAX_PATH;
			err = RegEnumKeyEx(hKEY, dwIndex, valueName, &length, NULL, NULL, NULL, NULL);
		}

		list<wstring>::iterator itor;
		for (itor = childKeys.begin(); itor != childKeys.end(); itor++)
		{
			LPWSTR keyName = (LPWSTR)(*itor).c_str();
			if (keyName[0] == L'{' && keyName[wcslen(keyName) - 1] == L'}') {

				HKEY hKeyChild;
				wstring childKeyPath = FormatString(L"%s\\%s", path, keyName);
				err = RegOpenKeyEx(rootkey, (LPWSTR)childKeyPath.c_str(), 0, KEY_READ, &hKeyChild);
				if (err == ERROR_SUCCESS)
				{
					TCHAR displayNameValue[MAX_PATH] = { 0 };
					DWORD displayNameValueLength = MAX_PATH;
					DWORD displayNameValueType = REG_SZ;
					err = RegQueryValueEx(hKeyChild, NULL, 0, &displayNameValueType, (LPBYTE)&displayNameValue, &displayNameValueLength);

					//Is a clsid
					HKEY hKeyIs32 = MREG_CLSIDToHKEYInprocServer32(rootkey, keyName);
					if (hKeyIs32)
					{
						TCHAR defKeyValueForPath[MAX_PATH] = { 0 };
						DWORD defKeyValueForPathLength = MAX_PATH;
						DWORD defKeyValueForPathType = REG_SZ;
						err = RegQueryValueEx(hKeyIs32, NULL, 0, &defKeyValueForPathType, (LPBYTE)&defKeyValueForPath, &defKeyValueForPathLength);
						if (err == ERROR_SUCCESS) {
							TCHAR realPath[MAX_PATH];
							if (ExpandEnvironmentStrings(defKeyValueForPath, realPath, MAX_PATH))
								callBack(displayNameValue, type, realPath, rootkey, path, keyName);
							else callBack(displayNameValue, type, defKeyValueForPath, rootkey, path, keyName);
						}
						else {
							callBack(displayNameValue, type, L"", rootkey, path, keyName);
							LogErr(L"Query key value (%s\\%s\\%s) failed in RegQueryValueEx : %d", MREG_ROOTKEYToStr(rootkey), path, valueName, err);
						}
					}
				}
			}
			else {
				LPWSTR displayNameValue = keyName;
				//Not cls id
				TCHAR clsidValue[MAX_PATH] = { 0 };
				DWORD clsidValueLength = MAX_PATH;
				DWORD clsidValueType = REG_SZ;

				HKEY hKeyChild;
				wstring childKeyPath = FormatString(L"%s\\%s", path, keyName);
				err = RegOpenKeyEx(rootkey, (LPWSTR)childKeyPath.c_str(), 0, KEY_READ, &hKeyChild);
				if (err == ERROR_SUCCESS)
				{
					err = RegQueryValueEx(hKeyChild, NULL, 0, &clsidValueType, (LPBYTE)&clsidValue, &clsidValueLength);
					if (err == ERROR_SUCCESS)
					{
						//Test value is clsid
						if (clsidValue[0] == L'{' && clsidValue[wcslen(clsidValue) - 1] == L'}') {
							//Is a clsid
							HKEY hKeyIs32 = MREG_CLSIDToHKEYInprocServer32(rootkey, clsidValue);
							if (hKeyIs32)
							{
								TCHAR defKeyValueForPath[MAX_PATH] = { 0 };
								DWORD defKeyValueForPathLength = MAX_PATH;
								DWORD defKeyValueForPathType = REG_SZ;
								err = RegQueryValueEx(hKeyIs32, NULL, 0, &defKeyValueForPathType, (LPBYTE)&defKeyValueForPath, &defKeyValueForPathLength);
								if (err == ERROR_SUCCESS) {
									TCHAR realPath[MAX_PATH];
									if(ExpandEnvironmentStrings(defKeyValueForPath, realPath, MAX_PATH))
										callBack(displayNameValue, type, realPath, rootkey, path, keyName);
									else callBack(displayNameValue, type, defKeyValueForPath, rootkey, path, keyName);
								}
								else {
									callBack(displayNameValue, type, L"", rootkey, path, keyName);
									LogErr(L"Query key value (%s\\%s\\%s) failed in RegQueryValueEx : %d", MREG_ROOTKEYToStr(rootkey), path, valueName, err);
								}
							}
						}
					}
					else LogErr(L"Query key value (%s\\%s) failed in RegQueryValueEx : %d", MREG_ROOTKEYToStr(rootkey), path, err);
				}
			}
		}
		RegCloseKey(hKEY);
	}
	else LogErr(L"Enum child key (%s\\%s) failed in RegOpenKeyEx : %d", MREG_ROOTKEYToStr(rootkey), path, err);
}

//SOFTWARE\\Classes\\CLSID\\

void MSM_EnumHC_RUN(EnumStartupsCallBack callBack)
{
	MSM_OpenAndEnumKeyValues(HKEY_LOCAL_MACHINE, TEXT("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"), L"HKLM Run", callBack);
	MSM_OpenAndEnumKeyValues(HKEY_CURRENT_USER, TEXT("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"), L"HKCU Run", callBack);
}
void MSM_EnumHC_KnowDlls(EnumStartupsCallBack callBack)
{
	MSM_OpenAndEnumKeyValues(HKEY_LOCAL_MACHINE, TEXT("SYSTEM\\CurrentControlSet\\Control\\Session Manager\\KnownDLLs"), L"KnownDLLs", callBack, TRUE);
}
void MSM_EnumHC_RightMenus(EnumStartupsCallBack callBack)
{
	MSM_OpenAndEnumWithClsid(HKEY_LOCAL_MACHINE, TEXT("SOFTWARE\\Classes\\*\\shellex\\ContextMenuHandlers\\"), L"RightMenu1", callBack);
	MSM_OpenAndEnumWithClsid(HKEY_CURRENT_USER, TEXT("SOFTWARE\\Classes\\*\\shellex\\ContextMenuHandlers\\"), L"RightMenu2", callBack);
}
void MSM_EnumHC_Prints(EnumStartupsCallBack callBack)
{

}

TCHAR selectedFilePath[MAX_PATH];
HKEY selectedRootkey;
TCHAR selectedKey[MAX_PATH];
TCHAR selectedValue[MAX_PATH];
DWORD selectedId = 0;

void MSM_DelSelectedReg()
{
	if (!MStrEqualW(selectedKey, L""))
	{
		if (!MREG_DeleteKeyValue(selectedRootkey, selectedKey, selectedValue)) {
			LogWarn(L"Delete reg key %s\\%s\\%s failed : %d", MREG_ROOTKEYToStr(selectedRootkey), selectedKey, selectedValue, GetLastError());
			MessageBox(hWndMain, L"Failed delete reg", (LPWSTR)str_item_op_failed.c_str(), MB_OK);
		}
		else MAppMainCall(26, (LPVOID)(ULONG_PTR)selectedId, 0);
	}
}
void MSM_DelSelectedFile()
{
	if (!MStrEqualW(selectedFilePath, L""))
	{
		if (MFM_FileExist(selectedFilePath))
			if (!MFM_DeleteDirOrFile(selectedFilePath))
				LogWarn(L"Delete file %s failed : %d", selectedFilePath, GetLastError());
	}
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
M_CAPI(VOID) MStartupsMgr_ShowMenu(HKEY rootkey, LPWSTR path, LPWSTR filepath, LPWSTR regvalue, DWORD id, int x, int y)
{
	selectedId = id;
	HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUSTART));
	if (hroot) {
		HMENU hpop = GetSubMenu(hroot, 0);

		POINT pt;
		if (x == 0 && y == 0)
			GetCursorPos(&pt);
		else {
			pt.x = x;
			pt.y = y;
		}

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
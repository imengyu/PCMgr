#include "stdafx.h"
#include "settinghlp.h"
#include "mapphlp.h"
#include "PathHelper.h"

WCHAR iniPath[MAX_PATH];

void M_CFG_Init() {
	WCHAR szFileFullPath[MAX_PATH];
	GetModuleFileName(NULL, szFileFullPath, MAX_PATH);
	std::wstring*exename = Path::GetFileNameWithoutExtension(szFileFullPath);
	std::wstring*dir = Path::GetDirectoryName(szFileFullPath);
	std::wstring inipath = *dir + L"\\" + *exename + L".ini";
	wcscpy_s(iniPath, inipath.c_str());
	delete dir;	delete exename;
}

M_CAPI(LPWSTR) M_CFG_GetCfgFilePath() {
	return iniPath;
}
M_CAPI(BOOL) M_CFG_GetConfigBOOL(LPWSTR configkey, LPWSTR configSection, BOOL defaultValue)
{
	BOOL rs = defaultValue;

	WCHAR temp[32];
	if (GetPrivateProfileString(configSection, configkey, defaultValue ? L"TRUE" : L"FALSE", temp, 32, iniPath) > 0)
		defaultValue = MStrEqualW(temp, L"1") || MStrEqualW(temp, L"TRUE") || MStrEqualW(temp, L"True");
	return defaultValue;
}
M_CAPI(BOOL) M_CFG_SetConfigBOOL(LPWSTR configkey, LPWSTR configSection, BOOL value)
{
	return WritePrivateProfileStringW(configSection, configkey, value ? L"TRUE" : L"FALSE", iniPath);
}
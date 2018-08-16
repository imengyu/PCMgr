#include "stdafx.h"
#include "reghlp.h"
#include "loghlp.h"
#include "StringHlp.h"
#include <string>

BOOL MREG_ForceDeleteServiceRegkey(LPWSTR lpszDriverName)
{
	BOOL rs = FALSE;
	wchar_t regPath[MAX_PATH];
	wsprintf(regPath, L"SYSTEM\\CurrentControlSet\\services\\%s", lpszDriverName);
	rs = MREG_DeleteKey(HKEY_LOCAL_MACHINE, regPath);

	if (!rs)LogErr(L"RegDeleteTree failed : %d in delete key HKEY_LOCAL_MACHINE\\%s", GetLastError(), regPath);
	else Log(L"Service Key deleted : HKEY_LOCAL_MACHINE\\%s", regPath);

	wchar_t regName[MAX_PATH];
	wcscpy_s(regName, lpszDriverName);
	_wcsupr_s(regName);
	wsprintf(regPath, L"SYSTEM\\CurrentControlSet\\Enum\\Root\\LEGACY_%s", regName);
	rs = MREG_DeleteKey(HKEY_LOCAL_MACHINE, regPath);

	if (!rs) {
		LogWarn(L"RegDeleteTree failed : %d in delete key HKEY_LOCAL_MACHINE\\%s", GetLastError(), regPath);
		rs = TRUE;
	}
	else Log(L"Service Key deleted : HKEY_LOCAL_MACHINE\\%s", regPath);

	return rs;
}
//获取服务的注册表键值路径
M_CAPI(BOOL) MREG_GetServiceReg(LPWSTR servicName, LPWSTR buf, size_t size)
{
	std::wstring s = FormatString(L"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\services\\%s", servicName);
	if (buf && size > s.size())
	{
		wcscpy_s(buf, size, s.c_str());
		return TRUE;
}
	return 0;
}
//尝试转换CLSID
M_CAPI(HKEY) MREG_CLSIDToHKEY(HKEY hRootKey, LPWSTR clsid)
{
	HKEY hKEY;
	std::wstring path; path += L"SOFTWARE\\Classes\\CLSID\\"; path += clsid;
#ifdef _AMD64_
	DWORD err = RegOpenKeyEx(hRootKey, (LPWSTR)path.c_str(), 0, KEY_READ, &hKEY);
#else
	DWORD err = RegOpenKeyEx(hRootKey, (LPWSTR)path.c_str(), 0, KEY_WOW64_64KEY | KEY_READ, &hKEY);
#endif
	if (err != ERROR_SUCCESS)
		LogErr(L"MREG_CLSIDToHKEY err : %d key : %s\\%s", err, MREG_ROOTKEYToStr(hRootKey), path.c_str());
	return hKEY;
}
//尝试转换CLSID并打开InprocServer32值
M_CAPI(HKEY) MREG_CLSIDToHKEYInprocServer32(HKEY hRootKey, LPWSTR clsid)
{
	HKEY hKEY;
	std::wstring path; path += L"SOFTWARE\\Classes\\CLSID\\"; path += clsid; path += L"\\InprocServer32";
#ifdef _AMD64_
	DWORD err = RegOpenKeyEx(hRootKey, (LPWSTR)path.c_str(), 0, KEY_READ, &hKEY);
#else
	DWORD err = RegOpenKeyEx(hRootKey, (LPWSTR)path.c_str(), 0, KEY_WOW64_64KEY | KEY_READ, &hKEY);
#endif
	if (err != ERROR_SUCCESS) 
		LogErr(L"MREG_CLSIDToHKEYInprocServer32 err : %d key : %s\\%s", err, MREG_ROOTKEYToStr(hRootKey), path.c_str());
	return hKEY;
}
//根注册表键值转字符串
M_CAPI(LPWSTR) MREG_ROOTKEYToStr(HKEY hRootKey)
{
	switch ((ULONG_PTR)hRootKey)
	{
	case HKEY_CLASSES_ROOT:return L"HKEY_CLASSES_ROOT";
	case HKEY_CURRENT_USER:return L"HKEY_CURRENT_USER";
	case HKEY_LOCAL_MACHINE:return L"HKEY_LOCAL_MACHINE";
	case HKEY_USERS:return L"HKEY_USERS";
	case HKEY_PERFORMANCE_DATA:return L"HKEY_PERFORMANCE_DATA";
	case HKEY_PERFORMANCE_TEXT:return L"HKEY_PERFORMANCE_TEXT";
	case HKEY_PERFORMANCE_NLSTEXT:return L"HKEY_PERFORMANCE_NLSTEXT";
	case HKEY_CURRENT_CONFIG:return L"";
	case HKEY_DYN_DATA:return L"HKEY_DYN_DATA";
	case HKEY_CURRENT_USER_LOCAL_SETTINGS:return L"HKEY_CURRENT_USER_LOCAL_SETTINGS";
	}
	return L"";
}
//删除注册表键以及子键
M_CAPI(BOOL) MREG_DeleteKey(HKEY hRootKey, LPWSTR path) {

	DWORD lastErr = RegDeleteTree(hRootKey, path);
	if (lastErr == ERROR_SUCCESS || lastErr == ERROR_FILE_NOT_FOUND)
		return TRUE;
	else
	{
		SetLastError(lastErr);
		return 0;
	}
}
//删除注册表项下的子值
M_CAPI(BOOL) MREG_DeleteKeyValue(HKEY hRootKey, LPWSTR path, LPWSTR value) {

	DWORD lastErr = RegDeleteKeyValueW(hRootKey, path, value);
	if (lastErr == ERROR_SUCCESS || lastErr == ERROR_FILE_NOT_FOUND)
		return TRUE;
	else
	{
		SetLastError(lastErr);
		return 0;
	}
}
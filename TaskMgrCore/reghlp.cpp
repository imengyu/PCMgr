#include "stdafx.h"
#include "reghlp.h"
#include "loghlp.h"
#include "StringHlp.h"
#include <string>
#include <prsht.h>

WCHAR lastErrStr[256];
DWORD lastErr = 0;

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
	case HKEY_CURRENT_CONFIG:return L"HKEY_CURRENT_CONFIG";
	case HKEY_DYN_DATA:return L"HKEY_DYN_DATA";
	case HKEY_CURRENT_USER_LOCAL_SETTINGS:return L"HKEY_CURRENT_USER_LOCAL_SETTINGS";
	}
	return L"";
}
//注册表值类型转字符串
M_CAPI(LPWSTR) MREG_RegTypeToStr(DWORD regType)
{
	switch (regType)
	{
	case REG_NONE:return L"REG_NONE";
	case REG_SZ:return L"REG_SZ";
	case REG_EXPAND_SZ:return L"REG_EXPAND_SZ";
	case REG_BINARY:return L"REG_BINARY";
	case REG_DWORD:return L"REG_DWORD";
	case REG_DWORD_BIG_ENDIAN:return L"REG_DWORD_BIG_ENDIAN";
	case REG_LINK:return L"REG_LINK";
	case REG_MULTI_SZ:return L"REG_MULTI_SZ";
	case REG_RESOURCE_LIST:return L"REG_RESOURCE_LIST";
	case REG_FULL_RESOURCE_DESCRIPTOR:return L"REG_FULL_RESOURCE_DESCRIPTOR";
	case REG_RESOURCE_REQUIREMENTS_LIST:return L"REG_RESOURCE_REQUIREMENTS_LIST";
	case REG_QWORD:return L"REG_QWORD";
	}
	return L"";
}

M_CAPI(HKEY) MREG_GetROOTKEY(int i) {
	switch (i)
	{
	case 0:return HKEY_CLASSES_ROOT;
	case 1:return HKEY_CURRENT_USER;
	case 2:return HKEY_LOCAL_MACHINE;
	case 3:return HKEY_USERS;
	case 4:return HKEY_PERFORMANCE_DATA;
	case 5:return HKEY_PERFORMANCE_TEXT;
	case 6:return HKEY_PERFORMANCE_NLSTEXT;
	case 7:return HKEY_CURRENT_CONFIG;
	case 8:return HKEY_DYN_DATA;
	case 9:return HKEY_CURRENT_USER_LOCAL_SETTINGS;
	}
	return NULL;
}
//删除注册表键以及子键
M_CAPI(BOOL) MREG_DeleteKey(HKEY hRootKey, LPWSTR path) {

	lastErr = RegDeleteTree(hRootKey, path);
	if (lastErr == ERROR_SUCCESS || lastErr == ERROR_FILE_NOT_FOUND)
		return TRUE;
	else
	{
		SetLastError(lastErr);
		return 0;
	}
}
//重命名注册表项
M_CAPI(BOOL) MREG_RenameKey(HKEY hRootKey, LPWSTR path, LPWSTR newName)
{
	DWORD ret= RegRenameKey(hRootKey, path, newName);
	if (ret != ERROR_SUCCESS)
	{
		LogErr(L"Rename key (%s\\%s) failed in RegRenameKey : %d", MREG_ROOTKEYToStr(hRootKey), path, ret);
		lastErr = ret;
		return FALSE;
	}	
	return TRUE;
}
//重命名注册表值
M_CAPI(BOOL) MREG_RenameValue(HKEY hRootKey, LPWSTR path, LPWSTR ValueName, LPWSTR newValueName)
{
	HKEY hKey;
	lastErr = RegOpenKeyEx(hRootKey, path, 0, KEY_READ, &hKey);
	if (lastErr == ERROR_SUCCESS)
	{
		DWORD dwType = 0;
		DWORD dwSize = 0;
		BYTE*data = nullptr;
		lastErr = RegGetValue(hRootKey, path, ValueName, RRF_RT_ANY, &dwType, data, &dwSize);
		if (lastErr == ERROR_MORE_DATA)
		{
			data = new BYTE[dwSize];
			lastErr = RegGetValue(hRootKey, path, ValueName, RRF_RT_ANY, &dwType, data, &dwSize);
			
			if (lastErr == ERROR_SUCCESS)
				lastErr = RegDeleteKeyValueW(hRootKey, path, ValueName);
		}

		if (lastErr == ERROR_SUCCESS)
			lastErr = RegSetValueEx(hKey, newValueName, 0, dwType, data, dwSize);
		return lastErr == ERROR_SUCCESS;
	}
	return 0;
}

M_CAPI(LPWSTR) MREG_GetLastErrString() {
	LPVOID buf;
	if (FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER
		| FORMAT_MESSAGE_FROM_SYSTEM,
		NULL,
		lastErr,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&buf,
		0,
		NULL)) {
		
		wcscpy_s(lastErrStr, (LPWSTR)buf);

		LocalFree(buf);
	}
	return lastErrStr;
}
M_CAPI(DWORD) MREG_GetLastErr() {
	
	return lastErr;
}

//CreatePropertySheetPageW
M_CAPI(BOOL) MREG_ShowKeyPrivilege(HKEY hRootKey, LPWSTR path) {
	HKEY hKey;
	lastErr = RegOpenKeyEx(hRootKey, path, 0, KEY_READ, &hKey);
	if (lastErr == ERROR_SUCCESS)
	{

	}
	return FALSE;
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
//创建注册表子项
M_CAPI(BOOL) MREG_CreateSubKey(HKEY hRootKey, LPWSTR path, LPWSTR newKeyName)
{
	HKEY hKey;
	std::wstring s(path);
	s += L"\\";
	s += newKeyName;
	lastErr = RegCreateKey(hRootKey, s.c_str(), &hKey);
	return lastErr == ERROR_SUCCESS;
}
//创建注册表项键值
M_CAPI(BOOL) MREG_CreateValue(HKEY hRootKey, LPWSTR path, LPWSTR newValueName, DWORD type)
{
	HKEY hKey;
	lastErr = RegOpenKeyEx(hRootKey, path, 0, KEY_READ, &hKey);
	if (lastErr == ERROR_SUCCESS)
	{
		lastErr = RegSetValueEx(hKey, newValueName, 0, type, NULL, 0);
		return lastErr == ERROR_SUCCESS;
	}
	return FALSE;
}
//创建注册表项键值字符串
M_CAPI(BOOL) MREG_SetValueSZ(HKEY hRootKey, LPWSTR path, LPWSTR vlueName, DWORD type, LPWSTR data, DWORD cb)
{
	HKEY hKey;
	lastErr = RegOpenKeyEx(hRootKey, path, 0, KEY_READ, &hKey);
	if (lastErr == ERROR_SUCCESS)
	{
		lastErr = RegSetValueEx(hKey, vlueName, 0, type, (LPBYTE)data, cb);
		return lastErr == ERROR_SUCCESS;
	}
	return FALSE;
}
M_CAPI(BOOL) MREG_SetValueDWORD(HKEY hRootKey, LPWSTR path, LPWSTR vlueName, DWORD type, DWORD data)
{
	HKEY hKey;
	lastErr = RegOpenKeyEx(hRootKey, path, 0, KEY_READ, &hKey);
	if (lastErr == ERROR_SUCCESS)
	{
		lastErr = RegSetValueEx(hKey, vlueName, 0, type, (LPBYTE)&data, sizeof(DWORD));
		return lastErr == ERROR_SUCCESS;
	}
	return FALSE;
}
M_CAPI(BOOL) MREG_SetValueQWORD(HKEY hRootKey, LPWSTR path, LPWSTR vlueName, DWORD type, UINT64 data)
{
	HKEY hKey;
	lastErr = RegOpenKeyEx(hRootKey, path, 0, KEY_READ, &hKey);
	if (lastErr == ERROR_SUCCESS)
	{
		lastErr = RegSetValueEx(hKey, vlueName, 0, type, (LPBYTE)&data, sizeof(UINT64));
		return lastErr == ERROR_SUCCESS;
	}
	return FALSE;
}

M_CAPI(DWORD) MREG_GetChildKeyCount(HKEY hKey) {
	DWORD rs = 0;

	DWORD dwSubKeyCnt;          // 子键的数量  
	DWORD dwSubKeyNameMaxLen;   // 子键名称的最大长度(不包含结尾的null字符)  
	DWORD dwKeyValueCnt;        // 键值项的数量  
	DWORD dwKeyValueNameMaxLen; // 键值项名称的最大长度(不包含结尾的null字符)  
	DWORD dwKeyValueDataMaxLen; // 键值项数据的最大长度(in bytes)  
	if (RegQueryInfoKey(
		hKey,
		NULL,
		NULL,
		NULL,
		&dwSubKeyCnt,
		&dwSubKeyNameMaxLen,
		NULL,
		&dwKeyValueCnt,
		&dwKeyValueNameMaxLen,
		&dwKeyValueDataMaxLen,
		NULL,
		NULL) == ERROR_SUCCESS)
		rs = dwSubKeyCnt;
	return rs;
}
//枚举注册表下的所有子值
M_CAPI(BOOL) MREG_EnumKeyVaules(HKEY hRootKey, LPWSTR path, ENUMKEYVALECALLBACK callBack)
{
	if (!callBack)
		return FALSE;

	BOOL continueEnum = TRUE;
	HKEY hKey;
	lastErr = RegOpenKeyEx(hRootKey, path, 0, KEY_READ, &hKey);
	if (lastErr == ERROR_SUCCESS)
	{
		DWORD dwSubKeyCnt;          // 子键的数量  
		DWORD dwSubKeyNameMaxLen;   // 子键名称的最大长度(不包含结尾的null字符)  
		DWORD dwKeyValueCnt;        // 键值项的数量  
		DWORD dwKeyValueNameMaxLen; // 键值项名称的最大长度(不包含结尾的null字符)  
		DWORD dwKeyValueDataMaxLen; // 键值项数据的最大长度(in bytes)  
		lastErr = RegQueryInfoKey(
			hKey,
			NULL,
			NULL,
			NULL,
			&dwSubKeyCnt,
			&dwSubKeyNameMaxLen,
			NULL,
			&dwKeyValueCnt,
			&dwKeyValueNameMaxLen,
			&dwKeyValueDataMaxLen,
			NULL,
			NULL);

		DWORD dwIndex = 0; 
		WCHAR* valueName = new WCHAR[dwKeyValueNameMaxLen + 1];
		BYTE*keyData = new BYTE[dwKeyValueDataMaxLen + 1];

		DWORD length = dwKeyValueNameMaxLen + 1;
		DWORD lengthData = dwKeyValueDataMaxLen + 1;
		DWORD dwType = 0;//键值的类型 

		while (continueEnum && lastErr == ERROR_SUCCESS)
		{
			lastErr = RegEnumValue(hKey, dwIndex, valueName, &length, 0, &dwType, keyData, &lengthData);
			if (lastErr != ERROR_SUCCESS)
				break;

			bool success = false;
			bool useBuffer = false;
			bool useBufferStr = false;
			TCHAR baniryFormatBuffer[8];
			TCHAR szValue[256];
			DWORD dwSize = 0;
			BYTE*buffer = NULL;

			if (dwType == REG_SZ || dwType == REG_EXPAND_SZ)
			{
				dwSize = sizeof(szValue);
				lastErr = RegQueryValueEx(hKey, valueName, 0, &dwType, (LPBYTE)&szValue, &dwSize);
				if (lastErr == ERROR_SUCCESS)
					success = true;
				else if (lastErr == ERROR_MORE_DATA && dwSize > 0) {
					useBuffer = true;
					buffer = (LPBYTE)malloc(dwSize);
					useBufferStr = true;
					memset(szValue, 0, sizeof(szValue));

					lastErr = RegQueryValueEx(hKey, valueName, 0, &dwType, buffer, &dwSize);
					if (lastErr == ERROR_SUCCESS)
						success = true;
				}
			}
			else if (dwType == REG_MULTI_SZ)
			{
				lastErr = RegQueryValueEx(hKey, valueName, 0, &dwType, (LPBYTE)&szValue, &dwSize);
				if (lastErr == ERROR_MORE_DATA && dwSize > 0)
				{
					useBuffer = true;
					buffer = (LPBYTE)malloc(dwSize);
					memset(szValue, 0, sizeof(szValue));

					lastErr = RegQueryValueEx(hKey, valueName, 0, &dwType, buffer, &dwSize);
					if (lastErr == ERROR_SUCCESS)
					{
						TCHAR *strBuffer = (TCHAR *)buffer;
						for (UINT i = 0; i < dwSize - 1; i += sizeof(TCHAR))
						{
							if (strBuffer[i] == L'\0')
								strBuffer[i] = L' ';
						}
						success = true;

						useBufferStr = true;
					}
				}
			}
			else if (dwType == REG_DWORD)
			{
				DWORD dwValue = 0;
				dwSize = sizeof(dwValue);
				lastErr = RegQueryValueEx(hKey, valueName, 0, &dwType, (LPBYTE)&dwValue, &dwSize);
				if (lastErr == ERROR_SUCCESS)
				{
					swprintf_s(szValue, L"0x%08x (%u)", dwValue, dwValue);
					success = true;
				}
			}
			else if (dwType == REG_QWORD)
			{
				UINT64 qwValue = 0;
				dwSize = sizeof(qwValue);
				lastErr = RegQueryValueEx(hKey, valueName, 0, &dwType, (LPBYTE)&qwValue, &dwSize);
				if (lastErr == ERROR_SUCCESS)
				{
					swprintf_s(szValue, L"0x%I64x (%llu)", qwValue, qwValue);
					success = true;
				}
			}
			else if (dwType == REG_BINARY) 
			{
				lastErr = RegQueryValueEx(hKey, valueName, 0, &dwType, (LPBYTE)&szValue, &dwSize);
				if (lastErr == ERROR_MORE_DATA && dwSize > 0)
				{
					useBuffer = true;
					buffer = (LPBYTE)malloc(dwSize);
					memset(szValue, 0, sizeof(szValue));

					lastErr = RegQueryValueEx(hKey, valueName, 0, &dwType, buffer, &dwSize);
					if (lastErr == ERROR_SUCCESS)
					{
						size_t len = 0;
						for (UINT i = 0; i < dwSize; i++)
						{
							swprintf_s(baniryFormatBuffer, L"%02X ", buffer[i]);
							wcscat_s(szValue, baniryFormatBuffer);

							len = wcslen(szValue);
							if (len > 250)
								break;
						}

						success = true;
					}
				}
			}

			if (success)
				continueEnum = callBack(hRootKey, path, valueName, dwType, dwSize, useBufferStr ? (LPWSTR)buffer : szValue, dwIndex, dwKeyValueCnt);
			else LogErr(L"Query key value (%s\\%s\\%s) failed in RegQueryValueEx : %d", MREG_ROOTKEYToStr(hRootKey), path, valueName, lastErr);
			
			if (useBuffer && buffer)
			{
				free(buffer);
				buffer = NULL;
			}

			dwIndex++;
			length = MAX_PATH;
			lengthData = MAX_PATH;
		}
		RegCloseKey(hKey);

		delete valueName;
		delete keyData;

		return TRUE;
	}
	else LogErr(L"Enum child key (%s\\%s) failed in RegOpenKeyEx : %d", MREG_ROOTKEYToStr(hRootKey), path, lastErr);
	
	return FALSE;
}
//枚举注册表下的所有子键
M_CAPI(BOOL) MREG_EnumKeys(HKEY hRootKey, LPWSTR path, ENUMKEYSCALLBACK callBack)
{
	if (!callBack)
		return FALSE;
	HKEY hKEY;
	lastErr = RegOpenKeyEx(hRootKey, path, 0, KEY_READ, &hKEY);
	if (lastErr == ERROR_SUCCESS)
	{
		std::wstring pp(path); 
		if (pp != L"")
			pp += L"\\";

		DWORD dwSubKeyCnt;          // 子键的数量  
		DWORD dwSubKeyNameMaxLen;   // 子键名称的最大长度(不包含结尾的null字符)  
		DWORD dwKeyValueCnt;        // 键值项的数量  
		DWORD dwKeyValueNameMaxLen; // 键值项名称的最大长度(不包含结尾的null字符)  
		DWORD dwKeyValueDataMaxLen; // 键值项数据的最大长度(in bytes)  
		lastErr = RegQueryInfoKey(
			hKEY,
			NULL,
			NULL,
			NULL,
			&dwSubKeyCnt,
			&dwSubKeyNameMaxLen,
			NULL,
			&dwKeyValueCnt,
			&dwKeyValueNameMaxLen,
			&dwKeyValueDataMaxLen,
			NULL,
			NULL);

		DWORD dwChilds = dwSubKeyCnt;
		DWORD dwIndex = 0;
		WCHAR* valueName = new WCHAR[dwSubKeyNameMaxLen + 1];
		DWORD length = dwSubKeyNameMaxLen + 1;

		lastErr = RegEnumKeyEx(hKEY, dwIndex, valueName, &length, NULL, NULL, NULL, NULL);
		while (lastErr == ERROR_SUCCESS)
		{
			BOOL hasChild = TRUE; 
			/*
			HKEY hKeyChild;
			err = RegOpenKeyEx(hRootKey, (pp + valueName).c_str(), 0, KEY_READ, &hKeyChild);
			if (err == ERROR_SUCCESS) 
			{
				hasChild = MREG_GetChildKeyCount(hKeyChild) > 0;
				RegCloseKey(hKeyChild);
			}
			*/
			if (!callBack(hRootKey, path, valueName, hasChild, dwIndex, dwChilds))
				break;

			dwIndex++;
			length = MAX_PATH;
			lastErr = RegEnumKeyEx(hKEY, dwIndex, valueName, &length, NULL, NULL, NULL, NULL);
		}
		RegCloseKey(hKEY);

		delete valueName;

		return TRUE;
	}
	else LogErr(L"Enum child key (%s\\%s) failed in RegOpenKeyEx : %d", MREG_ROOTKEYToStr(hRootKey), path, lastErr);
	return FALSE;
}

M_CAPI(BOOL) MREG_IsCurrentIEVersionOK(DWORD ver, LPWSTR currentProcessName) {
	BOOL ok = FALSE;
	HKEY hKey = NULL;
#ifdef _X64_
	lastErr = RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Microsoft\\Internet Explorer\\MAIN\\FeatureControl\\FEATURE_BROWSER_EMULATION", 0, KEY_READ, &hKey);
	if (lastErr == ERROR_SUCCESS) {
		DWORD dwValue;
		DWORD dwSize = sizeof(dwValue);
		DWORD dwType = REG_DWORD;

		if (RegQueryValueEx(hKey, currentProcessName, 0, &dwType, (LPBYTE)&dwValue, &dwSize) == ERROR_SUCCESS)
			if (dwValue >= ver) ok = TRUE;
		RegCloseKey(hKey);
}
	lastErr = RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", 0, KEY_READ, &hKey);
	if (lastErr == ERROR_SUCCESS) {
		DWORD dwValue;
		DWORD dwSize = sizeof(dwValue);
		DWORD dwType = REG_DWORD;

		if (RegQueryValueEx(hKey, currentProcessName, 0, &dwType, (LPBYTE)&dwValue, &dwSize) == ERROR_SUCCESS)
			if (dwValue >= ver) ok = TRUE;
		RegCloseKey(hKey);
}
#else
	lastErr = RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", 0, KEY_READ | KEY_WRITE, &hKey);
	if (lastErr == ERROR_SUCCESS) {
		DWORD dwValue;
		DWORD dwSize = sizeof(dwValue);
		DWORD dwType = REG_DWORD;

		if (RegQueryValueEx(hKey, currentProcessName, 0, &dwType, (LPBYTE)&dwValue, &dwSize) == ERROR_SUCCESS)
			if (dwValue >= ver) ok = TRUE;
		RegCloseKey(hKey);
	}
#endif
	return ok;
}
M_CAPI(BOOL) MREG_SetCurrentIEVersion(DWORD ver, LPWSTR currentProcessName) {
	HKEY hKey;
#ifdef _X64_
	lastErr = RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Microsoft\\Internet Explorer\\MAIN\\FeatureControl\\FEATURE_BROWSER_EMULATION", 0, KEY_READ | KEY_WRITE, &hKey);
	if (lastErr == ERROR_SUCCESS)
	{
		lastErr = RegSetValueEx(hKey, currentProcessName, 0, REG_DWORD, (LPBYTE)&ver, sizeof(DWORD));
		if (lastErr != ERROR_SUCCESS)
			LogErr2(L"RegSetValueEx failed : %d", lastErr);
		return lastErr == ERROR_SUCCESS;
	}
	else LogErr2(L"RegOpenKeyEx failed : %d", lastErr);
	RegCloseKey(hKey);
	lastErr = RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", 0, KEY_READ | KEY_WRITE, &hKey);
	if (lastErr == ERROR_SUCCESS)
	{
		lastErr = RegSetValueEx(hKey, currentProcessName, 0, REG_DWORD, (LPBYTE)&ver, sizeof(DWORD));
		if (lastErr != ERROR_SUCCESS)
			LogErr2(L"RegSetValueEx failed : %d", lastErr);
		return lastErr == ERROR_SUCCESS;
	}
	else LogErr2(L"RegOpenKeyEx failed : %d", lastErr);
	RegCloseKey(hKey);
#else
	lastErr = RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", 0, KEY_READ | KEY_WRITE, &hKey);
	if (lastErr == ERROR_SUCCESS)
	{
		lastErr = RegSetValueEx(hKey, currentProcessName, 0, REG_DWORD, (LPBYTE)&ver, sizeof(DWORD));
		if (lastErr != ERROR_SUCCESS)
			LogErr2(L"RegSetValueEx failed : %d", lastErr);
		return lastErr == ERROR_SUCCESS;
	}
	else LogErr2(L"RegOpenKeyEx failed : %d", lastErr);
	RegCloseKey(hKey);
#endif
	return FALSE;
}
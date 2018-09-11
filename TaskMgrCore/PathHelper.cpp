#include "stdafx.h"
#include "PathHelper.h"
#include <shlwapi.h>

#define DirectorySeparatorChar L'\\'
#define AltDirectorySeparatorChar  L'/'
#define VolumeSeparatorChar  L':'

Path::Path()
{
}
Path::~Path()
{
}

bool Path::RemoveQuotes(LPWSTR pathBuffer, size_t bufferSize)
{
	if (pathBuffer[0] == L'\"')
	{
		size_t size = wcslen(pathBuffer);
		if (pathBuffer[size - 1] == L'\"')
		{
			for (size_t i = 1; i < size -1 && i < bufferSize; i++) {
				pathBuffer[i - 1] = pathBuffer[i];
			}
			pathBuffer[size - 1] = L'\0';
			pathBuffer[size - 2] = L'\0';
			return true;
		}
	}	
	return false;
}
bool Path::IsValidateFolderFileName(std::wstring * path)
{
	bool ret = true;
	size_t u32Length = 0, u32Index = 0;
	wchar_t u8SpecialChar[] = { '\\','<','>','(',')','[',']','&',':',',','/','|','?','*' };
	wchar_t u8CtrlCharBegin = 0x0, u8CtrlCharEnd = 0x31;

	LPWSTR pName = (LPWSTR)path->c_str();
	if (pName == NULL)
		ret = false;
	else
	{
		u32Length = wcslen(pName);
		if (u32Length >= MAX_PATH)
			ret = false;
	}

	for (u32Index = 0; (u32Index < u32Length) && (ret == 0);
		u32Index++)
	{
		if (u8CtrlCharBegin <= pName[u32Index] && pName[u32Index] <= u8CtrlCharEnd)
			ret = false;
		else if (wcschr(u8SpecialChar, pName[u32Index]) != NULL)
			ret = false;
	}
	return ret;
}
bool Path::CheckInvalidPathChars(std::wstring * path)
{
	for (size_t i = 0; i < path->size(); i++)
	{
		int num = (int)(*path)[i];
		if (num == 34 || num == 60 || num == 62 || num == 124 || num < 32)
		{
			return true;
		}
	}
	return false;
}
std::wstring * Path::GetExtension(std::wstring * path)
{
	if (path == nullptr)
		return nullptr;
	if(Path::CheckInvalidPathChars(path)) return nullptr;
	size_t length = path->size();
	size_t num = length;
	while (--num >= 0)
	{
		wchar_t c = (*path)[num];
		if (c == L'.')
		{
			if (num != length - 1)
			{
				std::wstring *rs = new std::wstring(path->substr(num, length - num));
				return rs;
			}
			return nullptr;
		}
		else if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar || c == VolumeSeparatorChar)
		{
			break;
		}
	}
	return nullptr;
}
bool Path::IsPathRooted(std::wstring * path1)
{
	if (path1 != nullptr)
	{
		std::wstring path = *path1;
		if (Path::CheckInvalidPathChars(path1)) return false;
		size_t length = path.size();
		if ((length >= 1 && (path[0] == DirectorySeparatorChar || path[0] == AltDirectorySeparatorChar)) || (length >= 2 && path[1] == VolumeSeparatorChar))
		{
			return true;
		}
	}
	return false;
}
bool Path::HasExtension(std::wstring * path)
{
	if (path != nullptr)
	{
		if(Path::CheckInvalidPathChars(path)) 	return false;
		size_t num = path->size();
		while (--num >= 0)
		{
			wchar_t c = (*path)[num];
			if (c == L'.')
			{
				return num != path->size() - 1;
			}
			if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar || c == VolumeSeparatorChar)
			{
				break;
			}
		}
	}
	return false;
}
std::wstring * Path::GetFileNameWithoutExtension(std::wstring * path)
{
	path = Path::GetFileName(path);
	if (path == NULL)
	{
		return NULL;
	}
	size_t length;
	if ((length = path->find_last_of(L'.')) == -1)
	{
		return path;
	}
	std::wstring *rs = new std::wstring(path->substr(0, length));
	return rs;
}
std::wstring * Path::GetFileName(std::wstring * path)
{
	if (path != NULL)
	{
		if (Path::CheckInvalidPathChars(path))return NULL;
		size_t length = path->size();
		size_t num = length;
		while (--num >= 0)
		{
			wchar_t c = (*path)[num];
			if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar || c == VolumeSeparatorChar)
			{
				std::wstring *rs = new std::wstring(path->substr(num + 1, length - num - 1));
				return rs;
			}
		}
	}
	return path;
}
std::wstring * Path::GetDirectoryName(std::wstring * path)
{
	if (path != nullptr)
	{
		TCHAR exeFullPath[MAX_PATH];
		wcscpy_s(exeFullPath, path->c_str());
		PathRemoveFileSpec(exeFullPath);
		std::wstring *rs = new std::wstring(exeFullPath);
		return rs;
	}
	return nullptr;
}

std::wstring * Path::GetFileNameWithoutExtension(LPWSTR path)
{
	std::wstring cc(path);
	return GetFileNameWithoutExtension(&cc);
}
std::wstring * Path::GetExtension(LPWSTR path)
{
	std::wstring cc(path);
	return GetExtension(&cc);
}
bool Path::IsPathRooted(LPWSTR path)
{
	std::wstring cc(path);
	return IsPathRooted(&cc);
}
bool Path::HasExtension(LPWSTR path)
{
	std::wstring cc(path);
	return HasExtension(&cc);
}
bool Path::CheckInvalidPathChars(LPWSTR path)
{
	std::wstring cc(path);
	return CheckInvalidPathChars(&cc);
}
std::wstring * Path::GetFileName(LPWSTR path)
{
	std::wstring cc(path);
	return GetFileName(&cc);
}
std::wstring * Path::GetDirectoryName(LPWSTR path)
{
	std::wstring cc(path);
	return GetDirectoryName(&cc);
}

bool Path::IsValidateFolderFileName(std::string * path)
{
	bool ret = true;
	size_t u32Length = 0, u32Index = 0;
	char u8SpecialChar[] = { '\\','<','>','(',')','[',']','&',':',',','/','|','?','*' };
	char u8CtrlCharBegin = 0x0, u8CtrlCharEnd = 0x31;

	LPCSTR pName = (LPCSTR)path->c_str();
	if (pName == NULL)
		ret = false;
	else
	{
		u32Length = strlen(pName);
		if (u32Length >= MAX_PATH)
			ret = false;
	}

	for (u32Index = 0; (u32Index < u32Length) && (ret == 0);
		u32Index++)
	{
		if (u8CtrlCharBegin <= pName[u32Index] && pName[u32Index] <= u8CtrlCharEnd)
			ret = false;
		else if (strchr(u8SpecialChar, pName[u32Index]) != NULL)
			ret = false;
	}
	return ret;
}
bool Path::CheckInvalidPathChars(std::string * path)
{
	for (size_t i = 0; i < path->size(); i++)
	{
		int num = (int)(*path)[i];
		if (num == 34 || num == 60 || num == 62 || num == 124 || num < 32)
		{
			return true;
		}
	}
	return false;
}
std::string * Path::GetExtension(std::string * path)
{
	if (path == nullptr)
		return nullptr;
	if (Path::CheckInvalidPathChars(path)) return nullptr;
	size_t length = path->size();
	size_t num = length;
	while (--num >= 0)
	{
		wchar_t c = (*path)[num];
		if (c == L'.')
		{
			if (num != length - 1)
			{
				std::string *rs = new std::string(path->substr(num, length - num));
				return rs;
			}
			return nullptr;
		}
		else if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar || c == VolumeSeparatorChar)
		{
			break;
		}
	}
	return nullptr;
}
bool Path::IsPathRooted(std::string * path1)
{
	if (path1 != nullptr)
	{
		std::string path = *path1;
		if (Path::CheckInvalidPathChars(path1)) return false;
		size_t length = path.size();
		if ((length >= 1 && (path[0] == DirectorySeparatorChar || path[0] == AltDirectorySeparatorChar)) || (length >= 2 && path[1] == VolumeSeparatorChar))
		{
			return true;
		}
	}
	return false;
}
bool Path::HasExtension(std::string * path)
{
	if (path != nullptr)
	{
		if (Path::CheckInvalidPathChars(path)) 	return false;
		size_t num = path->size();
		while (--num >= 0)
		{
			wchar_t c = (*path)[num];
			if (c == L'.')
			{
				return num != path->size() - 1;
			}
			if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar || c == VolumeSeparatorChar)
			{
				break;
			}
		}
	}
	return false;
}
std::string * Path::GetFileNameWithoutExtension(std::string * path)
{
	path = Path::GetFileName(path);
	if (path == NULL)
	{
		return NULL;
	}
	size_t length;
	if ((length = path->find_last_of(L'.')) == -1)
	{
		return path;
	}
	std::string *rs = new std::string(path->substr(0, length));
	return rs;
}
std::string * Path::GetFileName(std::string * path)
{
	if (path != NULL)
	{
		if (Path::CheckInvalidPathChars(path))return NULL;
		size_t length = path->size();
		size_t num = length;
		while (--num >= 0)
		{
			wchar_t c = (*path)[num];
			if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar || c == VolumeSeparatorChar)
			{
				std::string *rs = new std::string(path->substr(num + 1, length - num - 1));
				return rs;
			}
		}
	}
	return path;
}
std::string * Path::GetDirectoryName(std::string * path)
{
	if (path != nullptr)
	{
		CHAR exeFullPath[MAX_PATH];
		strcpy_s(exeFullPath, path->c_str());
		PathRemoveFileSpecA(exeFullPath);
		std::string *rs = new std::string(exeFullPath);
		return rs;
	}
	return nullptr;
}

std::string * Path::GetFileNameWithoutExtension(LPCSTR path)
{
	std::string cc(path);
	return GetFileNameWithoutExtension(&cc);
}
std::string * Path::GetExtension(LPCSTR path)
{
	std::string cc(path);
	return GetExtension(&cc);
}
bool Path::IsPathRooted(LPCSTR path)
{
	std::string cc(path);
	return IsPathRooted(&cc);
}
bool Path::HasExtension(LPCSTR path)
{
	std::string cc(path);
	return HasExtension(&cc);
}
bool Path::CheckInvalidPathChars(LPCSTR path)
{
	std::string cc(path);
	return CheckInvalidPathChars(&cc);
}
std::string * Path::GetFileName(LPCSTR path)
{
	std::string cc(path);
	return GetFileName(&cc);
}
std::string * Path::GetDirectoryName(LPCSTR path)
{
	std::string cc(path);
	return GetDirectoryName(&cc);
}

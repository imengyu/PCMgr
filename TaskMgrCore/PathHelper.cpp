#include "stdafx.h"
#include "PathHelper.h"

#define DirectorySeparatorChar L'\\'
#define AltDirectorySeparatorChar  L'/'
#define VolumeSeparatorChar  L':'

Path::Path()
{
}
Path::~Path()
{
}

bool Path::IsValidateFolderFileName(std::wstring * path)
{
	bool ret = true;
	unsigned int u32Length = 0, u32Index = 0;
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
		if (u8CtrlCharBegin <= pName[u32Index] <= u8CtrlCharEnd)
			ret = false;
		else if (wcschr(u8SpecialChar, pName[u32Index]) != NULL)
			ret = false;
	}
	return ret;
}
bool Path::CheckInvalidPathChars(std::wstring * path)
{
	for (UINT i = 0; i < path->size(); i++)
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
	int length = path->size();
	int num = length;
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
		int length = path.size();
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
		int num = path->size();
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
	int length;
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
		int length = path->size();
		int num = length;
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

		if (path->find_last_of('\\') != -1) {
			wchar_t *p = wcsrchr(exeFullPath, '\\');
			if (p) *p = 0x00;
		}
		else	if (path->find_last_of('//') != -1) {
			wchar_t *p = wcsrchr(exeFullPath, '//');
			if(p) *p = 0x00;
		}
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

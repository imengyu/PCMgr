#pragma once
#include "stdafx.h"
#include <string>

class M_API Path
{
public:
	Path();
	~Path();

	static std::wstring *GetFileNameWithoutExtension(std::wstring * path);
	static std::wstring *GetExtension(std::wstring* path);
	static bool IsPathRooted(std::wstring * path1);
	static bool HasExtension(std::wstring * path);
	static bool CheckInvalidPathChars(std::wstring * path);
	static std::wstring *GetFileName(std::wstring * path);
	static std::wstring *GetDirectoryName(std::wstring * path);
	static bool IsValidateFolderFileName(std::wstring * path);

	static std::wstring *GetFileNameWithoutExtension(LPWSTR path);
	static std::wstring *GetExtension(LPWSTR path);
	static bool IsPathRooted(LPWSTR path1);
	static bool HasExtension(LPWSTR path);
	static bool CheckInvalidPathChars(LPWSTR path);
	static std::wstring *GetFileName(LPWSTR path);
	static std::wstring *GetDirectoryName(LPWSTR path);
};



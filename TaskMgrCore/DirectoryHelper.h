#pragma once
#include "stdafx.h"
#include <string>

class M_API Directory
{
public:
	Directory();
	~Directory();

	static bool Create(std::wstring* path);
	static bool Create(WCHAR * path);
	static bool Exists(std::wstring* path);
	static bool Exists(WCHAR * path);
	static bool Delete(std::wstring* path);
	static bool Delete(WCHAR * path);
};


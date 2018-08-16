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

	static bool Create(std::string* path);
	static bool Create(CHAR * path);
	static bool Exists(std::string* path);
	static bool Exists(CHAR * path);
	static bool Delete(std::string* path);
	static bool Delete(CHAR * path);
};


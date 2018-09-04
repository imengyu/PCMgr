#pragma once
#include "stdafx.h"

class MK_API MDriverLoader
{
public:
	MDriverLoader();
	~MDriverLoader();

	static bool LoadDriver(const wchar_t* lpszDriverServiceName, const wchar_t* driverFilePath);
	static bool UnLoadDriver(const wchar_t* lpszDriverServiceName);
};


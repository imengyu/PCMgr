#include "stdafx.h"
#include "MDriverLoader.h"

#include "..\TaskMgrCore\reghlp.h"
#include "..\TaskMgrCore\kernelhlp.h"

MDriverLoader::MDriverLoader()
{
}
MDriverLoader::~MDriverLoader()
{
}

bool MDriverLoader::LoadDriver(const wchar_t * lpszDriverServiceName, const wchar_t * driverFilePath)
{
	return MLoadKernelDriver((LPWSTR)lpszDriverServiceName, (LPWSTR)driverFilePath, NULL);
}
bool MDriverLoader::UnLoadDriver(const wchar_t * lpszDriverServiceName)
{
	return MUnLoadKernelDriver((LPWSTR)lpszDriverServiceName);
}

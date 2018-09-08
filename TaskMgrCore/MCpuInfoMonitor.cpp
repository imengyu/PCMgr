#include "stdafx.h"
#include "MCpuInfoMonitor.h"

extern UINT CpuCount;

PSYSTEM_LOGICAL_PROCESSOR_INFORMATION buffer = NULL;
PSYSTEM_LOGICAL_PROCESSOR_INFORMATION ptr = NULL;

DWORD numaNodeCount = 0;
DWORD processorL1CacheCount = 0;
DWORD processorL2CacheCount = 0;
DWORD processorL3CacheCount = 0;
DWORD processorPackageCount = 0;

void MCpuInfoMonitor::FreeCpuInfos()
{
	if (buffer)
		delete buffer;
}
DWORD MCpuInfoMonitor::GetCpuL1Cache()
{
	return processorL1CacheCount;
}
DWORD MCpuInfoMonitor::GetCpuL2Cache()
{
	return processorL2CacheCount;
}
DWORD MCpuInfoMonitor::GetCpuL3Cache()
{
	return processorL3CacheCount;
}
DWORD MCpuInfoMonitor::GetCpuPackage()
{
	return processorPackageCount;
}
DWORD MCpuInfoMonitor::GetCpuNodeCount()
{
	return numaNodeCount;
}
BOOL MCpuInfoMonitor::GetCpuInfos() {
	PCACHE_DESCRIPTOR Cache;
	DWORD returnLength = 0;
	DWORD byteOffset = 0;

	GetLogicalProcessorInformation(NULL, &returnLength);
	buffer = (PSYSTEM_LOGICAL_PROCESSOR_INFORMATION)malloc(returnLength);
	if (GetLogicalProcessorInformation(buffer, &returnLength))
	{
		ptr = buffer;
		while (byteOffset + sizeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION) <= returnLength)
		{
			switch (ptr->Relationship)
			{
			case RelationNumaNode: numaNodeCount++; break;
			case RelationCache: {
				Cache = &ptr->Cache;
				if (Cache->Level == 1)
					processorL1CacheCount += Cache->Size;
				else if (Cache->Level == 2)
					processorL2CacheCount += Cache->Size;
				else if (Cache->Level == 3)
					processorL3CacheCount += Cache->Size;
				break;
			}
			case RelationProcessorPackage: processorPackageCount++; break;
			}
			byteOffset += sizeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION);
			ptr++;
		}
		return TRUE;
	}
	return 0;
}
BOOL MCpuInfoMonitor::GetCpuName(LPWSTR buf, int size)
{
	HKEY hKey;
	if (ERROR_SUCCESS == RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0", 0, KEY_READ, &hKey))
	{
		WCHAR szValue[64];//长整型数据，如果是字符串数据用char数组  
		DWORD dwSize = sizeof(szValue);
		DWORD dwType = REG_SZ;

		if (RegQueryValueExW(hKey, L"ProcessorNameString", 0, &dwType, (LPBYTE)&szValue, &dwSize) == ERROR_SUCCESS)
		{
			wcscpy_s(buf, size, szValue);
			RegCloseKey(hKey);
			return TRUE;
		}
	}
	RegCloseKey(hKey);
	return 0;
}
int MCpuInfoMonitor::GetCpuFrequency()	//获取CPU主频
{
	HKEY hKey;
	if (ERROR_SUCCESS == RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0", 0, KEY_READ, &hKey))
	{
		DWORD dwValue;
		DWORD dwSize = sizeof(dwValue);
		DWORD dwType = REG_SZ;

		if (RegQueryValueEx(hKey, L"~MHz", 0, &dwType, (LPBYTE)&dwValue, &dwSize) == ERROR_SUCCESS) {
			RegCloseKey(hKey);
			return static_cast<int>(dwValue);
		}
	}
	RegCloseKey(hKey);
	return 0;
}
int MCpuInfoMonitor::GetProcessorNumber()
{
	return (int)CpuCount;
}
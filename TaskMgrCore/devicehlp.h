#pragma once
#include "stdafx.h"


//物理磁盘信息
struct MDevicePhysicalDisk
{
	WCHAR Name[64];
	WCHAR Model[64];
	UINT64 Size;
	UINT32 Index;
	WCHAR SizeStr[64];
};
//内存信息
struct MDeviceMemory
{
	WCHAR Name[64];
	WCHAR Model[64];
	UINT64 Size;
	UINT32 Speed;
};

M_CAPI(BOOL) MDEVICE_Init();
M_CAPI(void) MDEVICE_UnInit();

M_CAPI(BOOL) MDEVICE_GetLogicalDiskInfo();
M_CAPI(BOOL) MDEVICE_DestroyLogicalDiskInfo();
M_CAPI(UINT) MDEVICE_GetLogicalDiskInfoSize();
M_CAPI(BOOL) MDEVICE_GetLogicalDiskInfoItem(int index, LPWSTR nameBuffer, LPWSTR modelBuffer, UINT*outIndex, UINT64*outSize, LPWSTR sizeBuffer);
M_CAPI(DWORD) MDEVICE_GetPhysicalDriveFromPartitionLetter(CHAR letter);
M_CAPI(BOOL) MDEVICE_GetIsSystemDisk(LPCSTR perfStr);


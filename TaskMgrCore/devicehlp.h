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
	UINT16 FormatFactor;
	WCHAR DeviceLocator[32];
	UINT64 Size;
	UINT32 Speed;
	UINT32 SMBIOSMemoryType;
	UINT64 Capacity;
};
//网卡信息
struct MDeviceNetworkAdapter
{
	WCHAR Description[64];
	WCHAR IPAddressV4[32];
	WCHAR IPAddressV6[64];
	UINT16 StatusInfo;
	BOOL PhysicalAdapter;
	BOOL Enabled;
};

//初始化
M_CAPI(BOOL) MDEVICE_Init();
//释放
M_CAPI(void) MDEVICE_UnInit();

//获取磁盘信息，使用完后请调用 MDEVICE_DestroyLogicalDiskInfo 释放
M_CAPI(BOOL) MDEVICE_GetLogicalDiskInfo();
//释放磁盘信息
M_CAPI(BOOL) MDEVICE_DestroyLogicalDiskInfo();
//获取当前共有几个磁盘
M_CAPI(UINT) MDEVICE_GetLogicalDiskInfoSize();
//获取磁盘信息
//    index：磁盘序号
//    [OUT] nameBuffer：磁盘名称（64）
//    [OUT] modelBuffer：磁盘型号（64）
//    [OUT] outIndex：磁盘序号
//    [OUT] outSize：磁盘大小
//    [OUT] sizeBuffer：大小字符串（outSize可能不准）（64）
M_CAPI(BOOL) MDEVICE_GetLogicalDiskInfoItem(int index, LPWSTR nameBuffer, LPWSTR modelBuffer, UINT*outIndex, UINT64*outSize, LPWSTR sizeBuffer);
//盘符转为物理磁盘序号
M_CAPI(DWORD) MDEVICE_GetPhysicalDriveFromPartitionLetter(CHAR letter);
//获取是否是系统磁盘
//    perfStr：c:
M_CAPI(BOOL) MDEVICE_GetIsSystemDisk(LPCSTR perfStr);


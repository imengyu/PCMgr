#pragma once
#include "stdafx.h"

#ifdef _AMD64_
typedef struct tag_KERNEL_MODULE
{
	WCHAR BaseDllName[64];
	WCHAR FullDllPath[260];
	ULONG_PTR EntryPoint;
	ULONG SizeOfImage;
	ULONG_PTR DriverObject;
	ULONG_PTR Base;
	ULONG Order;
}KERNEL_MODULE, *PKERNEL_MODULE;
#else
typedef struct tag_KERNEL_MODULE
{
	WCHAR BaseDllName[64];
	WCHAR FullDllPath[260];
	ULONG EntryPoint;
	ULONG SizeOfImage;
	ULONG_PTR DriverObject;
	ULONG_PTR Base;
	ULONG Order;
}KERNEL_MODULE, *PKERNEL_MODULE;
#endif
#pragma once
#include "stdafx.h"

//反汇编引擎（基于开源 capstone 反汇编引擎）
//    buf：原始代码
//    startaddress：起始地址（在真实的机器上的地址）
//    callback：回调（DACALLBACK）
//    size：原始代码大小
//    x86Orx64：TRUE为x86 FALSE为x64 
M_CAPI(BOOL) M_KDA_Dec(PUCHAR buf, ULONG_PTR startaddress, LPVOID callback, ULONG_PTR size, BOOL x86Orx64);

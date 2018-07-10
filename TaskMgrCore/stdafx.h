// stdafx.h : 标准系统包含文件的包含文件，
// 或是经常使用但不常更改的
// 特定于项目的包含文件
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // 从 Windows 头中排除极少使用的资料
// Windows 头文件: 
#include <windows.h>

#define M_API __declspec(dllexport)
#define M_CAPI(x) extern "C" M_API x

#define DEFDIALOGGTITLE L"PC Manager"
#define ENDTASKASKTEXT L"如果某个打开的程序与此进程关联，则会关闭此程序并且将丢失所有未保存的数据。如果结束某个系统进程，则可能导致系统不稳定。你确定要继续吗 ?"

// TODO:  在此处引用程序需要的其他头文件

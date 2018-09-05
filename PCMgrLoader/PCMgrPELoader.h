#pragma once
#include "stdafx.h"

typedef struct _UNICODE_STRING {
	USHORT Length;
	USHORT MaximumLength;
#ifdef MIDL_PASS
	[size_is(MaximumLength / 2), length_is((Length) / 2)] USHORT * Buffer;
#else // MIDL_PASS
	_Field_size_bytes_part_(MaximumLength, Length) PWCH   Buffer;
#endif // MIDL_PASS
} UNICODE_STRING, *PUNICODE_STRING;

typedef struct _PEB_LDR_DATA {
	BYTE       Reserved1[8];
	PVOID      Reserved2[3];
	LIST_ENTRY InMemoryOrderModuleList;
} PEB_LDR_DATA, *PPEB_LDR_DATA;

typedef struct _LDR_MODULE {
	LIST_ENTRY              InLoadOrderModuleList;//代表按加载顺序构成的模块链表
	LIST_ENTRY              InMemoryOrderModuleList;//代表按内存顺序构成的模块链表
	LIST_ENTRY            InInitializationOrderModuleList;//代表按初始化顺序构成的模块链表
	PVOID                   BaseAddress;//该模块的基地址
	PVOID                   EntryPoint;//该模块的入口
	ULONG                   SizeOfImage;//该模块的影像大小
	UNICODE_STRING          FullDllName;//包含路径的模块名
	UNICODE_STRING          BaseDllName;//不包含路径的模块名
	ULONG                   Flags;
	SHORT                   LoadCount;//该模块的引用计数
	SHORT                   TlsIndex;
	HANDLE                  SectionHandle;
	ULONG                   CheckSum;
	ULONG                   TimeDateStamp;
} LDR_MODULE, *PLDR_MODULE;

typedef struct _PEB {
	BYTE                          Reserved1[2];
	BYTE                          BeingDebugged;
	BYTE                          Reserved2[1];
	PVOID                         Reserved3[2];
	PPEB_LDR_DATA                 Ldr;
} PEB, *PPEB;

PVOID MGetK32ModuleHandle();
PVOID MGetProcAddress(HMODULE hModule, LPCSTR lpProcName);
PVOID MGetK32ModuleGetProcAddress(HMODULE hK32);
VOID MGetModuleHandles();
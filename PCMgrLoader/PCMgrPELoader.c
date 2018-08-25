#include "stdafx.h"
#include "PCMgrPELoader.h"
#include "PCMgrDyFuns.h"
#include "mcrt.h"



UINT dotdll[] = { '.','d','l','l','\0', };
UINT gpa[] = { 'G','e','t','P','r','o','c','A','d','d','r','e','s','s','\0','\0', };
USHORT k32name[] = { L'k',L'e',L'r',L'n',L'e',L'l',L'3',L'2',L'.',L'd',L'l',L'l',L'\0', };
USHORT ntdllname[] = { L'n',L't',L'd',L'l',L'l',L'.',L'd',L'l',L'l',L'\0', };

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

extern PPEB MGetCurrentPeb();

extern HMODULE hKernel32;
extern HMODULE hNtdll;

PVOID MGetProcAddress(HMODULE hModule, LPCSTR lpProcName)
{
	char strdotdll[5];
	m_copyto_strarray(strdotdll, dotdll, 5);

	UINT i = 0;
	char *pRet = NULL;

	PIMAGE_DOS_HEADER pDosHeader = (PIMAGE_DOS_HEADER)hModule;
	PIMAGE_NT_HEADERS pNtHeader = (PIMAGE_NT_HEADERS)((ULONG_PTR)hModule + pDosHeader->e_lfanew);
	PIMAGE_OPTIONAL_HEADER pOptionalHeader = (PIMAGE_OPTIONAL_HEADER)((PBYTE)hModule + pDosHeader->e_lfanew + offsetof(IMAGE_NT_HEADERS, OptionalHeader));
	PIMAGE_EXPORT_DIRECTORY pExportDirectory = (PIMAGE_EXPORT_DIRECTORY)((PBYTE)hModule + pOptionalHeader->DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress);
	LPCSTR lpstrLibraryName = (LPCSTR)hModule + pExportDirectory->Name;
	PDWORD aryAddressOfFunctions = (PDWORD)((PBYTE)hModule + pExportDirectory->AddressOfFunctions);
	PDWORD aryAddressOfNames = (PDWORD)((PBYTE)hModule + pExportDirectory->AddressOfNames);
	LPWORD aryAddressOfNameOrdinals = (LPWORD)((PBYTE)hModule + pExportDirectory->AddressOfNameOrdinals);
	DWORD dwNumberOfNames = pExportDirectory->NumberOfNames;
	DWORD dwBase = pExportDirectory->Base;

	DWORD dwExportRVA = pNtHeader->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress;
	DWORD dwExportSize = pNtHeader->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].Size;

	//这个是查一下是按照什么方式（函数名称or函数序号）来查函数地址的
	ULONG_PTR dwName = (ULONG_PTR)lpProcName;
	if ((dwName & 0xFFFF0000) == 0)
		goto xuhao;

	for (i = 0; i < dwNumberOfNames; i++)
	{
		char *strFunction = (char *)(aryAddressOfNames[i] + (ULONG_PTR)hModule);
		if (m_strcmp(strFunction, (char *)lpProcName) == 0)
		{
			pRet = (char *)(aryAddressOfFunctions[aryAddressOfNameOrdinals[i]] + (ULONG_PTR)hModule);
			goto _exit11;
		}
	}
	//这个是通过以序号的方式来查函数地址的
xuhao:
	if (dwName < dwBase || dwName > dwBase + pExportDirectory->NumberOfFunctions - 1)
		return 0;
	pRet = (char *)(aryAddressOfFunctions[dwName - dwBase] + (ULONG_PTR)hModule);
_exit11:
	//判断得到的地址有没有越界
	if ((ULONG_PTR)pRet<dwExportRVA + (ULONG_PTR)hModule || (ULONG_PTR)pRet > dwExportRVA + (ULONG_PTR)hModule + dwExportSize)
		return (PVOID)pRet;

	char pTempDll[MAX_PATH];
	m_memset(pTempDll, 0, sizeof(pTempDll));
	char pTempFuction[64];
	m_memset(pTempFuction, 0, sizeof(pTempFuction));

	m_strcpy(pTempDll, pRet);
	char *p = m_strchr(pTempDll, '.');
	if (!p)
		return (PVOID)pRet;
	*p = 0;
	m_strcpy(pTempFuction, p + 1);
	m_strcat(pTempDll, strdotdll);

	HMODULE h = _LoadLibraryA(pTempDll);
	if (h == NULL)
		return (PVOID)pRet;
	return MGetProcAddress(h, pTempFuction);
}
PVOID MGetK32ModuleHandle()
{
	PPEB pPeb = MGetCurrentPeb();
	PPEB_LDR_DATA pLdr = pPeb->Ldr;

	PLIST_ENTRY list_head = &pLdr->InMemoryOrderModuleList;
	PLIST_ENTRY p = &pLdr->InMemoryOrderModuleList;

	WCHAR thisName[MAX_PATH];
	WCHAR k32NameName[13];
	m_copyto_wcsarray(k32NameName, k32name, 13);

	for (p = list_head->Flink; p != list_head; p = p->Flink) {
		PLDR_MODULE thisModule = CONTAINING_RECORD(p, LDR_MODULE, InMemoryOrderModuleList);
		if (thisModule->BaseDllName.Buffer != NULL) {
			m_wcscpy(thisName, thisModule->BaseDllName.Buffer);
			m_wcslwr(thisName);
			if (m_wcscmp(thisName, k32NameName) == 0)
				return thisModule->BaseAddress;
		}
	}
	//return GetModuleHandle(L"KERNEL32.DLL");
	return 0;
}
PVOID MGetK32ModuleGetProcAddress(HMODULE hK32)
{
	char strgpa[15];
	m_copyto_strarray(strgpa, gpa, 15);
	return MGetProcAddress(hK32, strgpa);
}
VOID MGetModuleHandles()
{
	PPEB pPeb = MGetCurrentPeb();
	PPEB_LDR_DATA pLdr = pPeb->Ldr;

	PLIST_ENTRY list_head = &pLdr->InMemoryOrderModuleList;
	PLIST_ENTRY p = &pLdr->InMemoryOrderModuleList;

	WCHAR thisName[MAX_PATH];

	WCHAR k32NameName[13];
	m_copyto_wcsarray(k32NameName, k32name, 13);
	WCHAR ntdllNameName[10];
	m_copyto_wcsarray(ntdllNameName, ntdllname, 10);

	for (p = list_head->Flink; p != list_head; p = p->Flink) {
		PLDR_MODULE thisModule = CONTAINING_RECORD(p, LDR_MODULE, InMemoryOrderModuleList);
		if (thisModule->BaseDllName.Buffer != NULL) {
			m_wcscpy(thisName, thisModule->BaseDllName.Buffer);
			m_wcslwr(thisName);
			if (m_wcscmp(thisName, k32NameName) == 0)
				hKernel32 = thisModule->BaseAddress;
			else if (m_wcscmp(thisName, ntdllNameName) == 0)
				hNtdll = thisModule->BaseAddress;
		}
	}
}

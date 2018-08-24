#include "stdafx.h"
#include "DyFuns.h"
#include "PCMgrPELoader.h"
#include "mcrt.h"

fnExitProcess _ExitProcess;
fnLoadLibraryA _LoadLibraryA;
fnLoadLibraryW _LoadLibraryW;
fnRtlGetVersion RtlGetVersion;
fnMessageBoxW _MessageBoxW;

_MAppMainGetExitCode MAppMainGetExitCode;
_MAppMainRun MAppMainRun;
_MAppSet MAppSet;

WCHAR*usedguid = 0;
bool basedllfailed = false;

HMODULE hMain;
HMODULE hKernel32;
HMODULE hUser32;
HMODULE hNtdll;

void show_err(const wchar_t* err);

const wchar_t* mainDllName = 0;

char _funNames[] = {
	'%','%', '%','\0',//3
	'L','o','a','d','L','i','b','r','a','r','y','A','\0',//16
	'\r','\r', '^','#','\0',//21
	'*','?','a',//24
	'N','T','D','L','L','.','D','L','L','\0',//34
	'M','e','s','s','a','g','e','B','o','x','W','\0'//46
};
char _funNames2[] = {
	'w','?','z','%', '#','?','\0',//6
	'L','o','a','d','L','i','b','r','a','r','y','W','\0',//19
	'*','@',//21
	'U','S','E','R','3','2','.','D','L','L','\0',//32
	'R','t','l','G','e','t','V','e','r','s','i','o','n','\0'//46
};
char _funNames3[] = {
	'\0','\0','\0','\0','\0','\0','\0',//6
	0x36,0x16,0x26,0x56,'W',0x66,'r','d','i','o',0x76,'\0',//18
	'M','A','p','p','S','e','t','\0',//26
	'M','A','p','p','M','a','i','n','R','u','n','\0',//38
	'M','A','p','p','M','a','i','n','G','e','t','E','x','i','t','C','o','d','e','\0',//58
	'E','x','i','t','P','r','o','c','e','s','s','\0'
};

typedef void( *voidfun)();

voidfun DllMonStart;

bool LoadDyamicFuns()
{
	ULONG_PTR funNames = (ULONG_PTR)_funNames;
	ULONG_PTR funNames2 = (ULONG_PTR)_funNames2;
	ULONG_PTR funNames3 = (ULONG_PTR)_funNames3;

	hKernel32 = (HMODULE)MGetK32ModuleHandle();

	_ExitProcess = (fnExitProcess)MGetProcAddress(hKernel32, (LPCSTR)(funNames3 + 59));
	_LoadLibraryA = (fnLoadLibraryA)MGetProcAddress(hKernel32, (LPCSTR)(funNames + 4));
	_LoadLibraryW = (fnLoadLibraryW)MGetProcAddress(hKernel32, (LPCSTR)(funNames2 + 7));

	hNtdll = _LoadLibraryA((LPCSTR)(funNames + 25));
	hUser32 = _LoadLibraryA((LPCSTR)(funNames2 + 22));

	RtlGetVersion = (fnRtlGetVersion)MGetProcAddress(hNtdll, (LPCSTR)(funNames2 + 33));//RtlGetVersion
	_MessageBoxW = (fnMessageBoxW)MGetProcAddress(hUser32, (LPCSTR)(funNames + 35));//MessageBoxW

#ifdef _AMD64_
	char mainDllName[] = { '\0','\0','\0','\0','P','C','M','G','R','6','4','.','D','L','L','\0', };
#else
	char mainDllName[] = { '?','?','\0','\0','P','C','M','G','R','3','2','.','D','L','L','\0', };
#endif
	hMain = _LoadLibraryA(mainDllName + 4);
	if (!hMain) { basedllfailed = true; return true; }

	MAppSet = (_MAppSet)MGetProcAddress(hMain, (LPCSTR)(funNames3 + 19));
	if (!MAppSet)return FALSE;
	MAppSet(2, MGetProcAddress);
	MAppSet(8, &usedguid);

	int old = usedguid[8];
	usedguid[8] = usedguid[old];
	usedguid[0] = old;
	usedguid[old / 2 - 1] = old;

	MAppMainGetExitCode = (_MAppMainGetExitCode)MGetProcAddress(hMain, (LPCSTR)(funNames3 + 39));
	MAppMainRun = (_MAppMainRun)MGetProcAddress(hMain, (LPCSTR)(funNames3 + 27));

	//DllMonStart = (voidfun)MGetProcAddress(_LoadLibraryA("E:\\主数据库\\编程\\方案V5\\DllTest\\Debug\\DllMon.dll"), "DllMonStart");
	//DllMonStart();

	return (MAppMainRun != NULL && MAppMainGetExitCode != NULL);
}

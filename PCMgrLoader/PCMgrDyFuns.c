#include "stdafx.h"
#include "PCMgrDyFuns.h"
#include "PCMgrPELoader.h"
#include "mcrt.h"

fnExitProcess _ExitProcess;
fnLoadLibraryA _LoadLibraryA;
fnLoadLibraryW _LoadLibraryW;
fnRtlGetVersion RtlGetVersion;
fnMessageBoxW _MessageBoxW;
fnGetProcAddress _GetProcAddress;
fnGetModuleHandleA _GetModuleHandleA;

_MAppMainGetExitCode MAppMainGetExitCode;
_MAppMainRun MAppMainRun;
_MAppSet MAppSet;

WCHAR*usedguid = 0;
BOOL basedllfailed = FALSE;

HMODULE hMain;
HMODULE hKernel32;
HMODULE hUser32;
HMODULE hNtdll;

UINT _funNames[] = {
	'%','%', '%','\0',//3
	'L','o','a','d','L','i','b','r','a','r','y','A','\0',//16
	'\r','\r', '^','#','\0',//21
	'*','?','a',//24
	'n','t','d','l','l','.','d','l','l','\0',//34
	'M','e','s','s','a','g','e','B','o','x','W','\0'//46
};
UINT _funNames2[] = {
	'w','?','z','%', '#','?','\0',//6
	'L','o','a','d','L','i','b','r','a','r','y','W','\0',//19
	'*','@',//21
	'U','s','e','r','3','2','.','d','l','l','\0',//32
	'R','t','l','G','e','t','V','e','r','s','i','o','n','\0',//46
	'G','e','t','M','o','d','u','l','e','H','a','n','d','l', 'e','A','\0'//63
};
UINT _funNames3[] = {
	'\0','\0','\0','\0','\0','\0','\0',//6
	0x36,0x16,0x26,0x56,'W',0x66,'r','d','i','o',0x76,'\0',//18
	'M','A','p','p','S','e','t','\0',//26
	'M','A','p','p','M','a','i','n','R','u','n','\0',//38
	'M','A','p','p','M','a','i','n','G','e','t','E','x','i','t','C','o','d','e','\0',//58
	'E','x','i','t','P','r','o','c','e','s','s','\0'
};

#ifdef _AMD64_
UINT mainDllName[] = { '\0','\0','\0','\0','P','C','M','g','r','6','4','.','d','l','l','\0', };
#else
UINT mainDllName[] = { '?','?','\0','\0','P','C','M','g','r','3','2','.','d','l','l','\0', };
#endif

BOOL MLoadDyamicFuns()
{
	char funNames[47];
	m_copyto_strarray(funNames, _funNames, 47);
	char funNames2[63];
	m_copyto_strarray(funNames2, _funNames2, 63);
	char funNames3[71];
	m_copyto_strarray(funNames3, _funNames3, 71);

	//hKernel32 = (HMODULE)MGetK32ModuleHandle();
	MGetModuleHandles();

	if (hKernel32 == NULL) _ExitProcess(0);

	_GetProcAddress = (fnGetProcAddress)MGetK32ModuleGetProcAddress(hKernel32);
	_GetModuleHandleA = (fnGetModuleHandleA)_GetProcAddress(hKernel32, (LPCSTR)(funNames2 + 47));
	_ExitProcess = (fnExitProcess)_GetProcAddress(hKernel32, (LPCSTR)(funNames3 + 59));

	if (_ExitProcess == 0)_ExitProcess(0);

	_LoadLibraryA = (fnLoadLibraryA)_GetProcAddress(hKernel32, (LPCSTR)(funNames + 4));
	_LoadLibraryW = (fnLoadLibraryW)_GetProcAddress(hKernel32, (LPCSTR)(funNames2 + 7));

	//hNtdll = _GetModuleHandleA((LPCSTR)(funNames + 25));
	hUser32 = _LoadLibraryA((LPCSTR)(funNames2 + 22));

	RtlGetVersion = (fnRtlGetVersion)_GetProcAddress(hNtdll, (LPCSTR)(funNames2 + 33));//RtlGetVersion
	_MessageBoxW = (fnMessageBoxW)_GetProcAddress(hUser32, (LPCSTR)(funNames + 35));//MessageBoxW

	char strmainDllName[16];
	m_copyto_strarray(strmainDllName, mainDllName, 16);

	hMain = _LoadLibraryA((LPCSTR)(strmainDllName + 4));
	if (!hMain) { basedllfailed = TRUE; return TRUE; }

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

	return (MAppMainRun != NULL && MAppMainGetExitCode != NULL);
}

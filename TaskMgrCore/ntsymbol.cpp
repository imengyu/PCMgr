#include "stdafx.h"
#include "ntsymbol.h"
#include "loghlp.h"
#include "fmhlp.h"
#include "ioctls.h"
#include "sysstructs.h"
#include "mapphlp.h"
#include <psapi.h>

HANDLE hProcess;
char* url = "http://msdl.microsoft.com/download/symbols";
extern HANDLE hKernelDevice;

BOOLEAN InitSymHandler()
{
	char Path[MAX_PATH] = { 0 };
	char SymSrvPath[MAX_PATH] = { 0 };
	char FileName[MAX_PATH] = { 0 };
	char SymPath[MAX_PATH * 2] = { 0 };

	if (!GetCurrentDirectoryA(MAX_PATH, Path))
	{
		LogErr2(L"Cannot get current directory \n");
		return FALSE;
	}

	strcpy_s(SymSrvPath, Path);
	strcat_s(SymSrvPath, "\\symsrv.dll");
	if (!LoadLibraryA(SymSrvPath))
		LogErr(L"LoadLibrary %s failed : %d", SymSrvPath, GetLastError());

	strcpy_s(FileName, Path);
	strcat_s(FileName, "\\symsrv.yes");

	hProcess = GetCurrentProcess();

	SymSetOptions(SYMOPT_DEFERRED_LOADS | SYMOPT_EXACT_SYMBOLS | SYMOPT_CASE_INSENSITIVE | SYMOPT_UNDNAME | SYMOPT_LOAD_ANYTHING);

	strcat_s(Path, "\\symbols*");
	strcpy_s(SymPath, "SRV*");
	strcat_s(SymPath, Path);
	strcat_s(SymPath, url);

	BOOL rs = SymInitialize(hProcess, SymPath, FALSE);
	if(!rs) LogErr(L"SymInitialize failed : %d", GetLastError());
	return rs;
}
BOOLEAN LoadSymModule(char* ImageName, DWORD ModuleBase)
{
	DWORD64 tmp;
	CHAR szFile[MAX_PATH],
		SymFile[MAX_PATH];
	MODULEINFO ModInfo;
	HMODULE hDll = LoadLibraryExA(ImageName, NULL, DONT_RESOLVE_DLL_REFERENCES);
	if (!hDll)
	{
		LogErr(L"Cannot load library %hs, error: %d \n", ImageName, GetLastError());
		return FALSE;
	}

	GetModuleFileNameA(hDll, szFile, sizeof(szFile) / sizeof(szFile[0]));
	GetModuleInformation(hProcess, hDll, &ModInfo, sizeof(ModInfo));
	if (!SymGetSymbolFile(hProcess, NULL, szFile, sfPdb, SymFile, MAX_PATH, SymFile, MAX_PATH))
	{
		LogErr(L"Cannot get symbol file of %hs  (%hs), error: %d \n", ImageName, szFile, GetLastError());
		return FALSE;
	}
	FreeLibrary(hDll);

	tmp = SymLoadModule64(hProcess, NULL, szFile, NULL, (DWORD64)ModuleBase, ModInfo.SizeOfImage);
	if (!tmp)
	{
		LogErr(L"Cannot load module (SymLoadModule64) , error : %d \n", GetLastError());
		return FALSE;
	}

	return TRUE;
}

ULONG_PTR ntosModuleBase = 0;
SYMBOL_INFO eprocessSymbolInfo;

BOOLEAN MEnumSyms(ULONG_PTR ModuleBase, PSYM_ENUMERATESYMBOLS_CALLBACK EnumRoutine, PVOID Context)
{
	BOOLEAN bEnum;
	ntosModuleBase = ModuleBase;

	bEnum = SymEnumSymbols(hProcess, ModuleBase, NULL, EnumRoutine, Context);
	if (!bEnum) LogErr(L"SymEnumSymbols failed , error: %d \n", GetLastError());


	MKEnumSymStructs(ModuleBase, "!_EPROCESS", (PSYM_ENUMERATESYMBOLS_CALLBACK)CALLBACKMEnumSymStruct_EPROCESS_Routine, NULL);
	MKEnumSymStructs(ModuleBase, "!_ETHREAD", (PSYM_ENUMERATESYMBOLS_CALLBACK)CALLBACKMEnumSymStruct_ETHREAD_Routine, NULL);
	
	return bEnum;
}
BOOLEAN MEnumSymsClear() {
	return SymCleanup(GetCurrentProcess());
}

ULONG_PTR Off_EPROCESS_RundownProtectOffest;
ULONG_PTR Off_EPROCESS_FlagsOffest;
ULONG_PTR Off_EPROCESS_ThreadListHeadOffest;

ULONG_PTR Off_ETHREAD_TcbOffest;
ULONG_PTR Off_ETHREAD_CrossThreadFlagsOffest;

BOOLEAN CALLBACK  CALLBACKMEnumSymStruct_EPROCESS_Routine(PSYMBOL_INFO psi, ULONG SymSize, PVOID Context)
{
	if (strcmp(psi->Name, "_EPROCESS") == 0)
		MKEnumSymStructOffests(psi, ntosModuleBase, (MENUMSTRUCTOFFEST_CALLBACK)CALLBACKMEnumSymStruct_Off_EPROCESS_Routine, NULL);
	return TRUE;
}
BOOLEAN CALLBACK  CALLBACKMEnumSymStruct_ETHREAD_Routine(PSYMBOL_INFO psi, ULONG SymSize, PVOID Context)
{
	if (strcmp(psi->Name, "_ETHREAD") == 0)
		MKEnumSymStructOffests(psi, ntosModuleBase, (MENUMSTRUCTOFFEST_CALLBACK)CALLBACKMEnumSymStruct_Off_ETHREAD_Routine, NULL);
	return TRUE;
}
BOOL CALLBACKMEnumSymStruct_Off_EPROCESS_Routine(_In_ LPCSTR structName, _In_ LPWSTR mumberName, _In_opt_ DWORD mumberOffest)
{
	if (MStrEqual(mumberName, L"RundownProtect"))
		Off_EPROCESS_RundownProtectOffest = mumberOffest;
	else 	if (MStrEqual(mumberName, L"ThreadListHead"))
		Off_EPROCESS_ThreadListHeadOffest = mumberOffest;
	else 	if (MStrEqual(mumberName, L"Flags"))
		Off_EPROCESS_FlagsOffest = mumberOffest;
	
	return TRUE;
}
BOOL CALLBACKMEnumSymStruct_Off_ETHREAD_Routine(_In_ LPCSTR structName, _In_ LPWSTR mumberName, _In_opt_ DWORD mumberOffest)
{
	if (MStrEqual(mumberName, L"Tcb"))
		Off_ETHREAD_TcbOffest = mumberOffest;
	else 	if (MStrEqual(mumberName, L"CrossThreadFlags"))
		Off_ETHREAD_CrossThreadFlagsOffest = mumberOffest;

	return TRUE;
}

M_CAPI(BOOLEAN) MKSymInit(char* ImageName, ULONG_PTR ModuleBase) {
	if (!LoadSymModule(ImageName, ModuleBase))
		return FALSE;
	return TRUE;
}
M_CAPI(BOOLEAN) MKEnumAllSym(ULONG_PTR ModuleBase, PSYM_ENUMERATESYMBOLS_CALLBACK callback, PVOID Context)
{
	BOOLEAN bEnum;
	bEnum = SymEnumSymbols(hProcess, ModuleBase, NULL, callback, Context);
	if (!bEnum) LogErr(L"SymEnumSymbols failed , error: %d \n", GetLastError());
	return bEnum;
}
M_CAPI(BOOLEAN) MKEnumSymStructs(ULONG_PTR ModuleBase, LPCSTR structName, PSYM_ENUMERATESYMBOLS_CALLBACK callBack, PVOID Context)
{
	return SymEnumTypesByName(hProcess, ModuleBase, structName, callBack, Context);
}
M_CAPI(BOOLEAN) MKEnumSymStructOffests(PSYMBOL_INFO psi, ULONG_PTR ModuleBase, MENUMSTRUCTOFFEST_CALLBACK callBack, PVOID Context)
{
	DWORD typeIndex = psi->TypeIndex;
	DWORD dwChildrenCount = 0;
	if (!SymGetTypeInfo(hProcess, ModuleBase, typeIndex, TI_GET_CHILDRENCOUNT, &dwChildrenCount))
	{
		LogErr(L"SymGetTypeInfo(TI_GET_CHILDRENCOUNT) failed : %d", GetLastError());
		return FALSE;
	}
	TI_FINDCHILDREN_PARAMS* childs= (TI_FINDCHILDREN_PARAMS*)new char[sizeof(TI_FINDCHILDREN_PARAMS) + sizeof(DWORD) * dwChildrenCount];;
	childs->Start = 0;
	childs->Count = dwChildrenCount;
	if (!SymGetTypeInfo(hProcess, ModuleBase, typeIndex, TI_FINDCHILDREN, childs))
	{
		LogErr(L"SymGetTypeInfo(TI_FINDCHILDREN) failed : %d", GetLastError());
		delete childs;
		return FALSE;
	}

	for (UINT i = 0; i < dwChildrenCount; i++)
	{
		LPWSTR symbolName = NULL;
		BOOL b = SymGetTypeInfo(hProcess, ModuleBase, childs->ChildId[i], TI_GET_SYMNAME, &symbolName);
		if(!b) LogWarn(L"SymGetTypeInfo(TI_GET_SYMNAME) failed : %d", GetLastError());

		if (symbolName)
		{
			DWORD mumberOffest = 0;
			if (SymGetTypeInfo(hProcess, ModuleBase, childs->ChildId[i], TI_GET_OFFSET, &mumberOffest))
			{
				callBack(psi->Name, symbolName, mumberOffest);
			}
			LocalFree(symbolName);
		}
	}
	delete childs;
	return TRUE;
}


ULONG_PTR PspTerminateThreadByPointer_;
ULONG_PTR PspExitThread_;
ULONG_PTR PsGetNextProcessThread_;
ULONG_PTR PsTerminateProcess_;
ULONG_PTR PsGetNextProcess_;
ULONG_PTR KeForceResumeThread_;


BOOLEAN CALLBACK MEnumSymRoutine(PSYMBOL_INFO psi, ULONG SymSize, PVOID Context)
{
	if (strcmp(psi->Name, "PspTerminateThreadByPointer") == 0)
		PspTerminateThreadByPointer_ = (ULONG_PTR)psi->Address;
	else if (strcmp(psi->Name, "PspExitThread") == 0)
		PspExitThread_ = (ULONG_PTR)psi->Address;
	else if (strcmp(psi->Name, "PsGetNextProcessThread") == 0)
		PsGetNextProcessThread_ = (ULONG_PTR)psi->Address;
	else if (strcmp(psi->Name, "PsTerminateProcess") == 0)
		PsTerminateProcess_ = (ULONG_PTR)psi->Address;
	else if (strcmp(psi->Name, "PsGetNextProcess") == 0)
		PsGetNextProcess_ = (ULONG_PTR)psi->Address;
	else if (strcmp(psi->Name, "KeForceResumeThread") == 0)
		KeForceResumeThread_ = (ULONG_PTR)psi->Address;
	//else if (strcmp(psi->Name, "") == 0)
	return TRUE;
}

BOOL MSendAllSymAddressToDriver() {
	DWORD ReturnLength = 0;
	NTOS_PDB_DATA inputBuffer = { 0 };

	inputBuffer.StructOffestData.EPROCESS_RundownProtectOffest = Off_EPROCESS_RundownProtectOffest;
	inputBuffer.StructOffestData.EPROCESS_ThreadListHeadOffest = Off_EPROCESS_ThreadListHeadOffest;
	inputBuffer.StructOffestData.EPROCESS_FlagsOffest = Off_EPROCESS_FlagsOffest;

	inputBuffer.StructOffestData.ETHREAD_TcbOffest = Off_ETHREAD_TcbOffest;
	inputBuffer.StructOffestData.ETHREAD_CrossThreadFlagsOffest = Off_ETHREAD_CrossThreadFlagsOffest;

	inputBuffer.PsGetNextProcessThread_ = PsGetNextProcessThread_;
	inputBuffer.PsGetNextProcess_ = PsGetNextProcess_;
	inputBuffer.PspExitThread_ = PspExitThread_;
	inputBuffer.PsTerminateProcess_ = PsTerminateProcess_;
	inputBuffer.PspTerminateThreadByPointer_ = PspTerminateThreadByPointer_;
	inputBuffer.KeForceResumeThread_ = KeForceResumeThread_;
	if (DeviceIoControl(hKernelDevice, CTL_KERNEL_INIT_WITH_PDB_DATA, &inputBuffer, sizeof(inputBuffer), NULL, 0, &ReturnLength, NULL))
		return TRUE;
	LogErr(L"MSendAllSymAddressToDriver error : %d", GetLastError());
	return FALSE;
}
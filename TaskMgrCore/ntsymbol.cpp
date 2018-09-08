#include "stdafx.h"
#include "ntsymbol.h"
#include "loghlp.h"
#include "fmhlp.h"
#include "ioctls.h"
#include "sysstructs.h"
#include "sysfuns.h"
#include "cscall.h"
#include "mapphlp.h"
#include "DirectoryHelper.h"
#include "PathHelper.h"
#include "StringHlp.h"
#include <psapi.h>

HANDLE hProcess;
CHAR* url = "http://msdl.microsoft.com/download/symbols";
extern HANDLE hKernelDevice;
ULONG_PTR ntosModuleBase = 0;
ULONG_PTR win32kModuleBase = 0;
SYMBOL_INFO eprocessSymbolInfo;
CHAR SymPathDir[MAX_PATH] = { 0 };

fnIMAGELOAD ImageLoad;
fnIMAGEUNLOAD ImageUnload;

BOOLEAN InitSymHandler()
{
	char Path[MAX_PATH] = { 0 };
	char SymSrvPath[MAX_PATH] = { 0 };
	char SymPath[MAX_PATH * 2] = { 0 };

	if (!GetCurrentDirectoryA(MAX_PATH, Path))
	{
		LogErr2(L"Cannot get current directory");
		return FALSE;
	}

	strcpy_s(SymSrvPath, Path);
	strcat_s(SymSrvPath, "\\symsrv.dll");
	if (!LoadLibraryA(SymSrvPath))
		LogErr(L"LoadLibrary %s failed : %d", SymSrvPath, GetLastError());

	hProcess = GetCurrentProcess();

	SymSetOptions(SYMOPT_DEFERRED_LOADS | SYMOPT_EXACT_SYMBOLS | SYMOPT_CASE_INSENSITIVE | SYMOPT_UNDNAME | SYMOPT_LOAD_ANYTHING);

	strcat_s(Path, "\\symbols*");
	strcpy_s(SymPathDir, Path);
	strcpy_s(SymPath, "SRV*");
	strcat_s(SymPath, Path);
	strcat_s(SymPath, url);

	BOOL rs = SymInitialize(hProcess, SymPath, FALSE);
	if(!rs) LogErr2(L"SymInitialize failed : %d", GetLastError());
	return rs;
}
BOOLEAN LoadSymModule(char* ImageName, ULONG_PTR ModuleBase)
{
	BOOL rs = FALSE;
	DWORD64 tmp;
	CHAR SymFileOrginalExe[MAX_PATH];
	CHAR SymFile[MAX_PATH];
	BOOL useOrginalExe = TRUE;

	memset(SymFileOrginalExe, 0, sizeof(SymFileOrginalExe));
	memset(SymFile, 0, sizeof(SymFile));

	PLOADED_IMAGE ImageInfo = ImageLoad(ImageName, NULL);
	if (!ImageInfo)
	{
		LogErr(L"Cannot load library %hs, (ImageLoad) error: %d", ImageName, GetLastError());
		return rs;
	}

	if (!StrEqualAnsi(ImageName, "ntoskrnl.exe") &&!StrEqualAnsi(ImageName, "ntkrnlpa.exe"))
	{
		CHAR ModuleSymPathDir[MAX_PATH] = { 0 };
		strcpy_s(ModuleSymPathDir, SymPathDir);
		ModuleSymPathDir[strlen(ModuleSymPathDir) - 1] = '\0';
		strcat_s(ModuleSymPathDir, "\\");
		strcat_s(ModuleSymPathDir, ImageName);
		if (!Directory::Exists(ModuleSymPathDir))
			Directory::Create(ModuleSymPathDir);

		CHAR ModuleSymPathDir2Size[MAX_PATH] = { 0 };
		sprintf_s(ModuleSymPathDir2Size, "%X", ImageInfo->SizeOfImage);
		CHAR ModuleSymPathDir2[MAX_PATH] = { 0 };
		strcpy_s(ModuleSymPathDir2, ModuleSymPathDir);
		strcat_s(ModuleSymPathDir2, "\\");
		strcat_s(ModuleSymPathDir2, ModuleSymPathDir2Size);
		if (!Directory::Exists(ModuleSymPathDir2))
			Directory::Create(ModuleSymPathDir2);

		CHAR ModuleSymPathFile[MAX_PATH] = { 0 };
		strcpy_s(ModuleSymPathFile, ModuleSymPathDir2);
		strcat_s(ModuleSymPathFile, "\\");
		strcat_s(ModuleSymPathFile, ImageName);
		if (!MFM_FileExistA(ModuleSymPathFile)) {
			useOrginalExe = !CopyFileA(ImageInfo->ModuleName, ModuleSymPathFile, TRUE);
			Log(L"Copy File %hs to %hs : %s", ImageInfo->ModuleName, ModuleSymPathFile, useOrginalExe ? L"" : L"");
			if(useOrginalExe) Log2(L"CopyFile failed : %d", GetLastError());
		}
		else useOrginalExe = FALSE;
		strcpy_s(SymFileOrginalExe, ModuleSymPathFile);
	}

	LPCSTR targerFile = useOrginalExe ? ImageInfo->ModuleName : SymFileOrginalExe;

	if (!SymGetSymbolFile(hProcess, NULL, targerFile, sfPdb, SymFile, MAX_PATH, SymFile, MAX_PATH))
	{
		LogErr2(L"Cannot get symbol file of %hs  (%hs), error: %d", ImageName, targerFile, GetLastError());
		MAppMainCall(M_CALLBACK_SHOW_NOPDB_WARN, ImageName, 0);
		ImageUnload(ImageInfo);
		return rs;
	}

	tmp = SymLoadModule64(hProcess, ImageInfo->hFile, targerFile, NULL, (ULONG_PTR)ModuleBase, ImageInfo->SizeOfImage);
	if (!tmp) LogErr2(L"Cannot load module (SymLoadModule64) , error : %d", GetLastError());
	else rs = TRUE;

	ImageUnload(ImageInfo);

	return rs;
}

ULONG_PTR Off_EPROCESS_RundownProtectOffest;
ULONG_PTR Off_EPROCESS_FlagsOffest;
ULONG_PTR Off_EPROCESS_ThreadListHeadOffest;
ULONG_PTR Off_EPROCESS_SeAuditProcessCreationInfoOffest;

ULONG_PTR Off_ETHREAD_TcbOffest;
ULONG_PTR Off_ETHREAD_CrossThreadFlagsOffest;

ULONG_PTR Off_PEB_LdrOffest;
ULONG_PTR Off_PEB_ProcessParametersOffest;

ULONG_PTR Off_RTL_USER_PROCESS_PARAMETERS_CommandLineOffest;

BOOL CALLBACKMEnumSymStruct_Off_RTL_USER_PROCESS_PARAMETERS_Routine(_In_ LPCSTR structName, _In_ LPWSTR mumberName, _In_opt_ DWORD mumberOffest)
{
	if (StrEqual(mumberName, L"CommandLine"))
		Off_RTL_USER_PROCESS_PARAMETERS_CommandLineOffest = mumberOffest;
	return TRUE;
}
BOOL CALLBACKMEnumSymStruct_Off_PEB_Routine(_In_ LPCSTR structName, _In_ LPWSTR mumberName, _In_opt_ DWORD mumberOffest)
{
	if (StrEqual(mumberName, L"Ldr"))
		Off_PEB_LdrOffest = mumberOffest;
	if (StrEqual(mumberName, L"ProcessParameters"))
		Off_PEB_ProcessParametersOffest = mumberOffest;
	return TRUE;
}
BOOL CALLBACKMEnumSymStruct_Off_EPROCESS_Routine(_In_ LPCSTR structName, _In_ LPWSTR mumberName, _In_opt_ DWORD mumberOffest)
{
	if (StrEqual(mumberName, L"RundownProtect"))
		Off_EPROCESS_RundownProtectOffest = mumberOffest;
	else 	if (StrEqual(mumberName, L"ThreadListHead"))
		Off_EPROCESS_ThreadListHeadOffest = mumberOffest;
	else 	if (StrEqual(mumberName, L"Flags"))
		Off_EPROCESS_FlagsOffest = mumberOffest;
	else 	if (StrEqual(mumberName, L"SeAuditProcessCreationInfo"))
		Off_EPROCESS_SeAuditProcessCreationInfoOffest = mumberOffest;
	return TRUE;
}
BOOL CALLBACKMEnumSymStruct_Off_ETHREAD_Routine(_In_ LPCSTR structName, _In_ LPWSTR mumberName, _In_opt_ DWORD mumberOffest)
{
	if (StrEqual(mumberName, L"Tcb"))
		Off_ETHREAD_TcbOffest = mumberOffest;
	else 	if (StrEqual(mumberName, L"CrossThreadFlags"))
		Off_ETHREAD_CrossThreadFlagsOffest = mumberOffest;

	return TRUE;
}

BOOLEAN CALLBACK  CALLBACKMEnumSymStruct_RTL_USER_PROCESS_PARAMETERS_Routine(PSYMBOL_INFO psi, ULONG SymSize, PVOID Context)
{
	if (strcmp(psi->Name, "_RTL_USER_PROCESS_PARAMETERS") == 0)
		MKEnumSymStructOffests(psi, ntosModuleBase, (MENUMSTRUCTOFFEST_CALLBACK)CALLBACKMEnumSymStruct_Off_RTL_USER_PROCESS_PARAMETERS_Routine, NULL);
	return TRUE;
}
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
BOOLEAN CALLBACK  CALLBACKMEnumSymStruct_PEB_Routine(PSYMBOL_INFO psi, ULONG SymSize, PVOID Context)
{
	if (strcmp(psi->Name, "_PEB") == 0)
		MKEnumSymStructOffests(psi, ntosModuleBase, (MENUMSTRUCTOFFEST_CALLBACK)CALLBACKMEnumSymStruct_Off_PEB_Routine, NULL);
	return TRUE;
}

M_CAPI(BOOLEAN) MKSymInit(char* ImageName, ULONG_PTR ModuleBase) 
{
	return LoadSymModule(ImageName, ModuleBase);
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



BOOLEAN MEnumNTOSSyms(ULONG_PTR ModuleBase, PSYM_ENUMERATESYMBOLS_CALLBACK EnumRoutine, PVOID Context)
{
	BOOLEAN bEnum;
	ntosModuleBase = ModuleBase;

	bEnum = SymEnumSymbols(hProcess, ModuleBase, NULL, EnumRoutine, Context);
	if (!bEnum) LogErr(L"SymEnumSymbols failed , error: %d \n", GetLastError());


	bEnum = MKEnumSymStructs(ModuleBase, "!_EPROCESS", (PSYM_ENUMERATESYMBOLS_CALLBACK)CALLBACKMEnumSymStruct_EPROCESS_Routine, NULL);
	bEnum = MKEnumSymStructs(ModuleBase, "!_ETHREAD", (PSYM_ENUMERATESYMBOLS_CALLBACK)CALLBACKMEnumSymStruct_ETHREAD_Routine, NULL);
	bEnum = MKEnumSymStructs(ModuleBase, "!_PEB", (PSYM_ENUMERATESYMBOLS_CALLBACK)CALLBACKMEnumSymStruct_PEB_Routine, NULL);
	bEnum = MKEnumSymStructs(ModuleBase, "!_RTL_USER_PROCESS_PARAMETERS", (PSYM_ENUMERATESYMBOLS_CALLBACK)CALLBACKMEnumSymStruct_RTL_USER_PROCESS_PARAMETERS_Routine, NULL);

	return bEnum;
}
BOOLEAN MEnumWIN32KSyms(ULONG_PTR ModuleBase, PSYM_ENUMERATESYMBOLS_CALLBACK EnumRoutine, PVOID Context)
{
	BOOLEAN bEnum;
	win32kModuleBase = ModuleBase;
	bEnum = SymEnumSymbols(hProcess, ModuleBase, NULL, EnumRoutine, Context);
	if (!bEnum) LogErr(L"SymEnumSymbols failed , error: %d \n", GetLastError());

	return bEnum;
}
BOOLEAN MEnumSymsClear() {
	return SymCleanup(GetCurrentProcess());
}

ULONG_PTR PspTerminateThreadByPointer_ = 0;
ULONG_PTR PspExitThread_ = 0;
ULONG_PTR PsGetNextProcessThread_ = 0;
ULONG_PTR PsTerminateProcess_ = 0;
ULONG_PTR PsGetNextProcess_ = 0;
ULONG_PTR KeForceResumeThread_ = 0;

ULONG_PTR _gptmrFirst = 0;
ULONG_PTR _gphkFirst = 0;

BOOLEAN CALLBACK MEnumSymWIN32KRoutine(PSYMBOL_INFO psi, ULONG SymSize, PVOID Context)
{
	if (strcmp(psi->Name, "_gptmrFirst") == 0)
		_gptmrFirst = (ULONG_PTR)psi->Address;
	else if (strcmp(psi->Name, "_gphkFirst") == 0)
		_gphkFirst = (ULONG_PTR)psi->Address;
	return TRUE;
}
BOOLEAN CALLBACK MEnumSymNTOSRoutine(PSYMBOL_INFO psi, ULONG SymSize, PVOID Context)
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

BOOL MSendAllSymAddressToDriver() 
{
	DWORD ReturnLength = 0;
	NTOS_PDB_DATA inputBuffer = { 0 };

	inputBuffer.StructOffestData.EPROCESS_RundownProtectOffest = Off_EPROCESS_RundownProtectOffest;
	inputBuffer.StructOffestData.EPROCESS_ThreadListHeadOffest = Off_EPROCESS_ThreadListHeadOffest;
	inputBuffer.StructOffestData.EPROCESS_FlagsOffest = Off_EPROCESS_FlagsOffest;
	inputBuffer.StructOffestData.EPROCESS_SeAuditProcessCreationInfoOffest = Off_EPROCESS_SeAuditProcessCreationInfoOffest;

	inputBuffer.StructOffestData.ETHREAD_TcbOffest = Off_ETHREAD_TcbOffest;
	inputBuffer.StructOffestData.ETHREAD_CrossThreadFlagsOffest = Off_ETHREAD_CrossThreadFlagsOffest;

	inputBuffer.StructOffestData.PEB_LdrOffest = Off_PEB_LdrOffest;
	inputBuffer.StructOffestData.PEB_ProcessParametersOffest = Off_PEB_ProcessParametersOffest;

	inputBuffer.StructOffestData.RTL_USER_PROCESS_PARAMETERS_CommandLineOffest = Off_RTL_USER_PROCESS_PARAMETERS_CommandLineOffest;

	inputBuffer.Win32KData._gphkFirst = _gphkFirst;
	inputBuffer.Win32KData._gptmrFirst = _gptmrFirst;

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
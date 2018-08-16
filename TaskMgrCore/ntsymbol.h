#pragma once
#include "stdafx.h"
#include <DbgHelp.h>

typedef BOOL (*MENUMSTRUCTOFFEST_CALLBACK)(_In_ LPCSTR structName,_In_ LPWSTR mumberName,	_In_opt_ DWORD mumberOffest);

BOOL CALLBACKMEnumSymStruct_Off_EPROCESS_Routine(_In_ LPCSTR structName, _In_ LPWSTR mumberName, _In_opt_ DWORD mumberOffest);
BOOL CALLBACKMEnumSymStruct_Off_ETHREAD_Routine(LPCSTR structName, LPWSTR mumberName, DWORD mumberOffest);

BOOLEAN CALLBACK  CALLBACKMEnumSymStruct_EPROCESS_Routine(PSYMBOL_INFO psi, ULONG SymSize, PVOID Context);
BOOLEAN CALLBACK CALLBACKMEnumSymStruct_ETHREAD_Routine(PSYMBOL_INFO psi, ULONG SymSize, PVOID Context);
BOOLEAN CALLBACK MEnumSymNTOSRoutine(PSYMBOL_INFO psi, ULONG SymSize, PVOID Context);
BOOLEAN CALLBACK MEnumSymWIN32KRoutine(PSYMBOL_INFO psi, ULONG SymSize, PVOID Context);

BOOL MSendAllSymAddressToDriver();
BOOLEAN InitSymHandler();
BOOLEAN MEnumNTOSSyms(ULONG_PTR ModuleBase, PSYM_ENUMERATESYMBOLS_CALLBACK EnumRoutine, PVOID Context);
BOOLEAN MEnumWIN32KSyms(ULONG_PTR ModuleBase, PSYM_ENUMERATESYMBOLS_CALLBACK EnumRoutine, PVOID Context);
BOOLEAN MEnumSymsClear();


M_CAPI(BOOLEAN) MKSymInit(char * ImageName, ULONG_PTR ModuleBase);
M_CAPI(BOOLEAN) MKEnumAllSym(ULONG_PTR ModuleBase, PSYM_ENUMERATESYMBOLS_CALLBACK callback, PVOID Context);
M_CAPI(BOOLEAN) MKEnumSymStructs(ULONG_PTR ModuleBase, LPCSTR structName, PSYM_ENUMERATESYMBOLS_CALLBACK callBack, PVOID Context);
M_CAPI(BOOLEAN) MKEnumSymStructOffests(PSYMBOL_INFO psi, ULONG_PTR ModuleBase, MENUMSTRUCTOFFEST_CALLBACK callBack, PVOID Context);

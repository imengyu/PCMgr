#pragma once
#include "Driver.h"

NTSTATUS KxGetFunctions(PWINVERS parm);

VOID KxGetFunctionsFormPDBData(PNTOS_PDB_DATA data);

VOID KxGetStructOffestsFormPDBData(PNTOS_EPROCESS_OFF_DATA data);

VOID KxPrintInternalFuns();

VOID KxPrintInternalOffests();

NTSTATUS KxLoadStructOffests(PWINVERS parm);

ULONG_PTR KxSearchFeatureCodeForAddress(ULONG_PTR StartAddress, PUCHAR FeatureCode, int FeatureCodeSize, int Search_MaxLength);


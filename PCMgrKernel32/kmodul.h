#pragma once
#include "Driver.h"

NTSTATUS KxUnLoadDrvObjectByDrvObject(wchar_t * pszDrvName, ULONG_PTR * pDrvObject);

NTSTATUS KxGetDrvObjectByName(wchar_t * pszDrvName, ULONG_PTR* pDrvObject);

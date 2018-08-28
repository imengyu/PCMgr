#pragma once
#include "stdafx.h"


LPVOID MAlloc(SIZE_T size);
LPVOID MRealloc(LPVOID ptr, SIZE_T size);;
VOID MFree(LPVOID ptr);
VOID MForceGC();
VOID MForceGC(LONG Generation);
VOID MShowProgramStats();
#pragma once
#include "Driver.h"


VOID KxForceReBoot(void);

VOID KxForceShutdown(void);

BOOLEAN KxDasm(ULONG_PTR address, ULONG_PTR offest, PUCHAR buf);

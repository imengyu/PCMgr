#pragma once
#include "stdafx.h"

typedef struct _MSINGLE_DELTA
{
	float Value;
	float Delta;
} MSINGLE_DELTA, *PMSINGLE_DELTA;

typedef struct _MUINT32_DELTA
{
	ULONG Value;
	ULONG Delta;
} MUINT32_DELTA, *PMUINT32_DELTA;

typedef struct _MUINT64_DELTA
{
	ULONG64 Value;
	ULONG64 Delta;
} MUINT64_DELTA, *PMUINT64_DELTA;

typedef struct _MUINTPTR_DELTA
{
	ULONG_PTR Value;
	ULONG_PTR Delta;
} MUINTPTR_DELTA, *PMUINTPTR_DELTA;

#define MInitializeDelta(DltMgr) \
    ((DltMgr)->Value = 0, (DltMgr)->Delta = 0)

#define MUpdateDelta(DltMgr, NewValue) \
    ((DltMgr)->Delta = (NewValue) - (DltMgr)->Value, \
    (DltMgr)->Value = (NewValue), (DltMgr)->Delta)
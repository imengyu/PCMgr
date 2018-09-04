#pragma once
#include "driver.h"
#include "..\FileAccess\fsppv.h"

VOID KxInitPkv();
VOID KxUnInitPkv();

BOOLEAN KxIsPathInProtectList(LPCWSTR path, PMFS_PROTECT *outProtect);
BOOLEAN KxIsPathInProtect(PUNICODE_STRING path, BOOLEAN isDir, PMFS_PROTECT *outProtect);
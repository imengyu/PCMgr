#pragma once
#include "stdafx.h"

PVOID MGetK32ModuleHandle();
PVOID MGetProcAddress(HMODULE hModule, LPCSTR lpProcName);
PVOID MGetK32ModuleGetProcAddress(HMODULE hK32);
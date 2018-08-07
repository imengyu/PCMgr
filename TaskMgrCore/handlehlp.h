#pragma once
#include "stdafx.h"

typedef void(*EHCALLBACK)(VOID* handle, LPWSTR type, LPWSTR name, LPWSTR address, LPWSTR objaddr, int refcount, int typeindex);
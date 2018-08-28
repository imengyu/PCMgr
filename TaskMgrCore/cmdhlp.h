#pragma once
#include "stdafx.h"

M_CAPI(int) MAppConsoleInit();

BOOL MStartRunCmdThread();
BOOL MStopRunCmdThread();

bool MRunCmdWithString(char*maxbuf);

int MAppCmdRunner(BOOL isMain);

int MPrintMumberWithLen(DWORD n, size_t len);
int MPrintStrWithLenW(LPWSTR s, size_t len);
int MPrintStrWithLenA(LPCSTR s, size_t len);
void MPrintSuccess();
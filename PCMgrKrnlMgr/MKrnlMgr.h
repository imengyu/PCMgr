#pragma once
#include "stdafx.h"

class MK_API MKrnlMgr
{
public:
	MKrnlMgr();
	~MKrnlMgr();

	static bool UnInitKernel();
	static bool InitKernel(LPCWSTR currentPath);
	static bool KernelInited();
	static bool KernelNeed64();
	static bool KernelCanUse();

	static HANDLE DriverHandle;
private:
	static bool KernelInitHandle();
};


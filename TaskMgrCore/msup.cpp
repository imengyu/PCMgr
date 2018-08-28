#include "stdafx.h"
#include "msup.h"
#include "resource.h"
#include "psapi.h"
#include <mscoree.h>
#include <Metahost.h>

extern HINSTANCE hInst;
extern HWND hWndMain;
extern BOOL clrInited;
extern ICLRGCManager *pGcManager;

ULONGLONG allAllocedMemoryHistory = 0;
ULONGLONG allAllocedMemory = 0;
ULONGLONG allFreedMemory = 0;

LPVOID MAlloc(SIZE_T size) {
	allAllocedMemoryHistory += size;
	allAllocedMemory += size;
	if (allAllocedMemoryHistory > 0xffffffff)allAllocedMemoryHistory = 0;
	return malloc(size);
}
LPVOID MRealloc(LPVOID ptr, SIZE_T size) {
	if (ptr) {	
		size_t oldSize = _msize(ptr);
		if (oldSize > size) {
			allAllocedMemory -= (oldSize - size);
			allAllocedMemoryHistory -= (oldSize - size);
		}
		else {
			allAllocedMemory += (size - oldSize);
			allAllocedMemoryHistory += (size - oldSize);
		}
		return realloc(ptr, size);
	}
	return 0;
}
VOID MFree(LPVOID ptr) {
	if (ptr) {
		allAllocedMemory -= _msize(ptr);
		allFreedMemory += _msize(ptr);
		if (allFreedMemory > 0xffffffff)allAllocedMemoryHistory = 0;
		free(ptr);
	}
}

VOID MGetMemStatitics(HWND hDlg)
{
	WCHAR memstr[512];

	PROCESS_MEMORY_COUNTERS meminfo = { 0 };
	meminfo.cb = sizeof(PROCESS_MEMORY_COUNTERS);
	GetProcessMemoryInfo(GetCurrentProcess(), &meminfo, meminfo.cb);

	swprintf_s(memstr,
		L"PageFaultCount : %u\nPagefileUsage : %u\nPeakPagefileUsage : %u\nPeakWorkingSetSize : %u\
        \nQuotaNonPagedPoolUsage : %u\nQuotaPagedPoolUsage : %u\nQuotaPeakNonPagedPoolUsage : %u\
		\nQuotaPeakPagedPoolUsage : %u\nWorkingSetSize : %u\n",
		meminfo.PageFaultCount,
		meminfo.PagefileUsage,
		meminfo.PeakPagefileUsage, meminfo.PeakWorkingSetSize,
		meminfo.QuotaNonPagedPoolUsage,
		meminfo.QuotaPagedPoolUsage,
		meminfo.QuotaPeakNonPagedPoolUsage,
		meminfo.QuotaPeakPagedPoolUsage,
		meminfo.WorkingSetSize);

	SetDlgItemText(hDlg, IDC_STAT_MEM, memstr);

	WCHAR memstr2[64];
	swprintf_s(memstr2, L"%llu K", allAllocedMemoryHistory / 1024);
	SetDlgItemText(hDlg, IDC_STAT_AC_MEM, memstr2);
	swprintf_s(memstr2, L"%llu K", allFreedMemory / 1024);
	SetDlgItemText(hDlg, IDC_STAT_FREEMEM, memstr2);
	swprintf_s(memstr2, L"%llu K", (allAllocedMemory) / 1024);
	SetDlgItemText(hDlg, IDC_STAT_INUSEMEM, memstr2);
}
VOID MGetGCStatitics(HWND hDlg)
{
	if (clrInited && pGcManager)
	{
		COR_GC_STATS GCStats;
		GCStats.Flags = COR_GC_COUNTS | COR_GC_MEMORYUSAGE;
		if (SUCCEEDED(pGcManager->GetStats(&GCStats)))
		{
			WCHAR memstr[512];
			swprintf_s(memstr, L"Gen0HeapSizeKBytes : %u K\nGen1HeapSizeKBytes : %u K\nGen2HeapSizeKBytes : %u K\n\
KBytesPromotedFromGen0 : %u K\nKBytesPromotedFromGen1 : %u K\nLargeObjectHeapSizeKBytes : %u K\n\
CommittedKBytes : %u K\nReservedKBytes : %u K",
				GCStats.Gen0HeapSizeKBytes,
				GCStats.Gen1HeapSizeKBytes,
				GCStats.Gen2HeapSizeKBytes,
				GCStats.KBytesPromotedFromGen0,
				GCStats.KBytesPromotedFromGen1,
				GCStats.LargeObjectHeapSizeKBytes,
				GCStats.CommittedKBytes,
				GCStats.ReservedKBytes);
			SetDlgItemText(hDlg, IDC_STAT_GC, memstr);
		}
	}
}

INT_PTR CALLBACK StatiticsDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_TIMER: {
		switch (wParam)
		{
		case 1: 
			SendMessage(hDlg, WM_COMMAND, IDC_BTN_REFESH, NULL);
			break;
		default:
			break;
		}
		break;
	}
	case WM_INITDIALOG: {
		SendMessage(hDlg, WM_SETICON, ICON_SMALL, (LPARAM)LoadIcon(hInst, MAKEINTRESOURCE(IDI_ICONAPP)));
		SendMessage(hDlg, WM_COMMAND, IDC_BTN_REFESH, NULL);
		SetTimer(hDlg, 1, 1000, NULL);
		break;
	}
	case WM_SYSCOMMAND: {
		if (wParam == SC_CLOSE) {
			EndDialog(hDlg, 0);
		}
		return 0;
	}
	case WM_COMMAND: {
		switch (wParam)
		{
		case IDC_FORCEGC: {
			MForceGC();
			break;
		}
		case IDC_BTN_REFESH: {
			MGetMemStatitics(hDlg);
			MGetGCStatitics(hDlg);
			break;
		}
		case IDOK:
		case IDCANCEL:
			SendMessage(hDlg, WM_SYSCOMMAND, SC_CLOSE, NULL);
			break;
		}
		break;
	}
	}
	return (INT_PTR)FALSE;
}

VOID MForceGC() {
	//if (clrInited && pGcManager)
	//	pGcManager->Collect(-1);
}
VOID MForceGC(LONG Generation) {
	if (clrInited && pGcManager)
		pGcManager->Collect(Generation);
}
VOID MShowProgramStats()
{
	DialogBoxW(hInst, MAKEINTRESOURCE(IDD_STATS), hWndMain, StatiticsDlgProc);
}

#include "stdafx.h"
#include "pehlp.h"
#include "resource.h"
#include "vprocx.h"
#include "fmhlp.h"
#include "comdlghlp.h"
#include "mapphlp.h"
#include "StringHlp.h"
#include "PathHelper.h"
#include <dbghelp.h>

extern HINSTANCE hInst;

BOOL vImportTables = FALSE;
BOOL vExportTables = FALSE;
HWND hListTables;
HWND hListTree;

WCHAR currentOpenPEFile[MAX_PATH];

VOID AddAStringItem(HWND hList, LPWSTR str)
{
	LVITEM li = { 0 };
	li.mask = LVIF_TEXT;
	li.iItem = ListView_GetItemCount(hList);
	li.pszText = L"";
	li.cchTextMax = 1;
	ListView_InsertItem(hList, &li);
	li.iSubItem = 1;
	li.pszText = str;
	li.cchTextMax = static_cast<int>(wcslen(str) + 1);
	ListView_SetItem(hList, &li);
}
VOID Add2StringItem(HWND hList, LPWSTR str, LPWSTR str2) {
	LVITEM li = { 0 };
	li.iItem = ListView_GetItemCount(hList);
	li.mask = LVIF_TEXT;
	li.pszText = str;
	li.cchTextMax = static_cast<int>(wcslen(str) + 1);
	ListView_InsertItem(hList, &li);
	li.iSubItem = 1;
	li.pszText = str2;
	li.cchTextMax = static_cast<int>(wcslen(str2) + 1);
	ListView_SetItem(hList, &li);
}

DWORD RvaToOffset(PIMAGE_NT_HEADERS pImageNtHeaders, DWORD dwRva)
{
	PIMAGE_SECTION_HEADER pImageSectionHeader;
	DWORD dwCount;
	DWORD dwFileOffset;
	pImageSectionHeader = IMAGE_FIRST_SECTION(pImageNtHeaders);
	dwFileOffset = dwRva;
	for (dwCount = 0; dwCount < pImageNtHeaders->FileHeader.NumberOfSections; dwCount++)
	{
		if (dwRva >= pImageSectionHeader[dwCount].VirtualAddress && dwRva < (pImageSectionHeader[dwCount].VirtualAddress + pImageSectionHeader[dwCount].SizeOfRawData))
		{
			dwFileOffset -= pImageSectionHeader[dwCount].VirtualAddress;
			dwFileOffset += pImageSectionHeader[dwCount].PointerToRawData;
			return dwFileOffset;
		}
	}
	return 0;
}

VOID OpenPEFile(HWND hDlg)
{
	ListView_DeleteAllItems(hListTables);
	if (M_DLG_ChooseFileSingal(hDlg, NULL, L"Choose a PE File", L"PE文件\0*.exe;*.dll\0所有文件\0*.*\0", NULL, L".exe", currentOpenPEFile, MAX_PATH))
	{ 
		if (MFM_FileExist(currentOpenPEFile))
			AddAStringItem(hListTables, L"File opened");
		else AddAStringItem(hListTables, L"File was not exist");
	}
}
VOID LoadImportTables(HWND hDlg)
{
	ListView_DeleteAllItems(hListTables);
	if (StrEqual(currentOpenPEFile, L"")) {
		AddAStringItem(hListTables, L"Please open a PE File first");
		return;
	}
	if (!MFM_FileExist(currentOpenPEFile))
	{
		AddAStringItem(hListTables, L"File was not exist");
		return;
	}

	HANDLE hFile = NULL;
	HANDLE hFileMapping = NULL;
	LPBYTE lpBaseAddress = NULL;

	hFile = CreateFile(currentOpenPEFile, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hFile == INVALID_HANDLE_VALUE)
	{
		WCHAR str[32];
		swprintf_s(str, L"Create file failed : %d", GetLastError());
		AddAStringItem(hListTables, str);

		return;
	}

	hFileMapping = CreateFileMapping(hFile, NULL, PAGE_READONLY, 0, 0, NULL);
	if (hFileMapping == NULL || hFileMapping == INVALID_HANDLE_VALUE)
	{
		WCHAR str[55];
		swprintf_s(str, L"Could not create file mapping object (%d)", GetLastError());
		AddAStringItem(hListTables, str);

		goto UNMAP_AND_EXIT;
	}

	lpBaseAddress = (LPBYTE)MapViewOfFile(hFileMapping, FILE_MAP_READ, 0, 0, 0);
	if (lpBaseAddress == NULL)
	{
		WCHAR str[32];
		swprintf_s(str, L"Could not map view of file (%d)", GetLastError());
		AddAStringItem(hListTables, str);
		goto UNMAP_AND_EXIT;
	}

	PIMAGE_DOS_HEADER pDosHeader = (PIMAGE_DOS_HEADER)lpBaseAddress;
	PIMAGE_NT_HEADERS pNtHeaders = (PIMAGE_NT_HEADERS)(lpBaseAddress + pDosHeader->e_lfanew);

	DWORD Rva_import_table = pNtHeaders->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress;
	if (Rva_import_table == 0)
	{
		AddAStringItem(hListTables, L"No import table!"); 
		goto UNMAP_AND_EXIT;
	}

	PIMAGE_IMPORT_DESCRIPTOR pImportTable = (PIMAGE_IMPORT_DESCRIPTOR)ImageRvaToVa(pNtHeaders, lpBaseAddress, Rva_import_table, NULL);
	IMAGE_IMPORT_DESCRIPTOR null_iid;
	IMAGE_THUNK_DATA null_thunk;
	memset(&null_iid, 0, sizeof(null_iid));
	memset(&null_thunk, 0, sizeof(null_thunk));

	int i, j;
	for (i = 0; memcmp(pImportTable + i, &null_iid, sizeof(null_iid)) != 0; i++)
	{
		LPCSTR szDllName = (LPCSTR)ImageRvaToVa(pNtHeaders, lpBaseAddress, pImportTable[i].Name, NULL);
		PIMAGE_THUNK_DATA32 pThunk = (PIMAGE_THUNK_DATA32)ImageRvaToVa(pNtHeaders, lpBaseAddress, pImportTable[i].OriginalFirstThunk, NULL);

		for (j = 0; memcmp(pThunk + j, &null_thunk, sizeof(null_thunk)) != 0; j++)
		{
			if (pThunk[j].u1.AddressOfData & IMAGE_ORDINAL_FLAG32)
			{
				WCHAR msg[32];
				WCHAR number[32];
				swprintf_s(msg, L"#%d %hs", j, szDllName);
				swprintf_s(number, L"%ld", pThunk[j].u1.AddressOfData & 0xffff);
				Add2StringItem(hListTables, msg, number);
			}
			else
			{
				PIMAGE_IMPORT_BY_NAME pFuncName = (PIMAGE_IMPORT_BY_NAME)ImageRvaToVa(pNtHeaders, lpBaseAddress, pThunk[j].u1.AddressOfData, NULL);

				WCHAR msg[32];
				WCHAR number[64];
				swprintf_s(msg, L"#%d %hs", j, szDllName);
				swprintf_s(number, L"%hs (%ld)", pFuncName->Name, pFuncName->Hint);
				Add2StringItem(hListTables, msg, number);
			}
		}
	}
UNMAP_AND_EXIT:
	if (lpBaseAddress) UnmapViewOfFile(lpBaseAddress);
	if (hFileMapping) CloseHandle(hFileMapping);
	if (hFile) CloseHandle(hFile);
}
VOID LoadExportTables(HWND hDlg)
{
	ListView_DeleteAllItems(hListTables);
	if (StrEqual(currentOpenPEFile, L"")) {
		AddAStringItem(hListTables, L"Please open a PE File first");
		return;
	}
	if (!MFM_FileExist(currentOpenPEFile))
	{
		AddAStringItem(hListTables, L"File was not exist");
		return;
	}

	HANDLE hFile = NULL;
	HANDLE hFileMapping = NULL;
	LPBYTE lpBaseAddress = NULL;

	hFile = CreateFile(currentOpenPEFile, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hFile == INVALID_HANDLE_VALUE)
	{
		WCHAR str[32];
		swprintf_s(str, L"Create file failed : %d", GetLastError());
		AddAStringItem(hListTables, str);
		return;
	}
	hFileMapping = CreateFileMapping(hFile, NULL, PAGE_READONLY, 0, 0, NULL);
	if (hFileMapping == NULL || hFileMapping == INVALID_HANDLE_VALUE)
	{
		WCHAR str[64];
		swprintf_s(str, L"Could not create file mapping object (%d)", GetLastError());
		AddAStringItem(hListTables, str);
		goto UNMAP_AND_EXIT;
	}

	lpBaseAddress = (LPBYTE)MapViewOfFile(hFileMapping, FILE_MAP_READ, 0, 0, 0);
	if (lpBaseAddress == NULL)
	{
		WCHAR str[64];
		swprintf_s(str, L"Could not map view of file (%d)", GetLastError());
		AddAStringItem(hListTables, str);
		goto UNMAP_AND_EXIT;
	}

	PIMAGE_DOS_HEADER pImageDOSHeader;
	PIMAGE_NT_HEADERS pImageNTHeader;
	PIMAGE_EXPORT_DIRECTORY pImageExportDirectory;
	DWORD dwCount;
	DWORD dwCount2;
	DWORD dwFileOffset;
	DWORD dwOrdinals;
	DWORD dwFunctions;
	char *szFunctionName;
	DWORD dwNames;
	PDWORD dwName;
	PDWORD dwFunction;
	PWORD dwOrdinal;

	pImageDOSHeader = (PIMAGE_DOS_HEADER)lpBaseAddress;
	if (pImageDOSHeader->e_magic != IMAGE_DOS_SIGNATURE)
		goto UNMAP_AND_EXIT;
	pImageNTHeader = (PIMAGE_NT_HEADERS)((PUCHAR)lpBaseAddress + pImageDOSHeader->e_lfanew);
	if (pImageNTHeader->Signature != IMAGE_NT_SIGNATURE)
		goto UNMAP_AND_EXIT;
	if (!(pImageNTHeader->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress))
	{
		AddAStringItem(hListTables, L"No export function!");
		goto UNMAP_AND_EXIT;
	}
	//导出表文件偏移
	dwFileOffset = RvaToOffset(pImageNTHeader, pImageNTHeader->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress);
	pImageExportDirectory = (PIMAGE_EXPORT_DIRECTORY)((PUCHAR)lpBaseAddress + dwFileOffset);
	dwCount = pImageExportDirectory->NumberOfFunctions;
	dwOrdinals = RvaToOffset(pImageNTHeader, pImageExportDirectory->AddressOfNameOrdinals);
	dwFunctions = RvaToOffset(pImageNTHeader, pImageExportDirectory->AddressOfFunctions);
	dwNames = RvaToOffset(pImageNTHeader, pImageExportDirectory->AddressOfNames);
	for (dwCount2 = 0; dwCount2 < dwCount; dwCount2++)
	{
		dwOrdinal = (PWORD)((PUCHAR)lpBaseAddress + dwOrdinals + dwCount2 * 2); // 地址
		dwFunction = (PDWORD)((PUCHAR)lpBaseAddress + dwFunctions + dwCount2 * 4); // 地址
		dwName = (PDWORD)((PUCHAR)lpBaseAddress + dwNames + dwCount2 * 4); //地址
		szFunctionName = ((PCHAR)lpBaseAddress + RvaToOffset(pImageNTHeader, *dwName));

		WCHAR number[32];
		WCHAR fun[256];

		swprintf_s(number, L"#%d 0x%04X", *dwOrdinal, *dwFunction);
		if (dwCount2 == *dwOrdinal) 
			swprintf_s(fun, L"%hs", szFunctionName);

		Add2StringItem(hListTables, number, fun);
	}

UNMAP_AND_EXIT:
	if (lpBaseAddress) UnmapViewOfFile(lpBaseAddress);
	if (hFileMapping) CloseHandle(hFileMapping);
	if (hFile) CloseHandle(hFile);
}
VOID LoadPEInfo(HWND hDlg)
{
	ListView_DeleteAllItems(hListTables);
	if (StrEqual(currentOpenPEFile, L"")) {
		AddAStringItem(hListTables, L"Please open a PE File first");
		return;
	}
	if (!MFM_FileExist(currentOpenPEFile))
	{
		AddAStringItem(hListTables, L"File was not exist");
		return;
	}


}

INT_PTR CALLBACK VPEDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_INITDIALOG: {
		SendMessage(hDlg, WM_SETICON, ICON_SMALL, (LPARAM)LoadIcon(hInst, MAKEINTRESOURCE(IDI_ICONAPP)));
		hListTables = GetDlgItem(hDlg, IDC_TABLES);
		hListTree = GetDlgItem(hDlg, IDC_TREEITEMS);
		LV_COLUMN lvc;
		lvc.mask = LVCF_TEXT | LVCF_WIDTH | LVCF_SUBITEM;
		lvc.pszText = L"Dll";
		lvc.cx = 110;
		lvc.iSubItem = 0;
		ListView_InsertColumn(hListTables, 0, (LPARAM)&lvc);
		lvc.pszText = L"Function name";
		lvc.cx = 250;
		lvc.iSubItem = 1;
		ListView_InsertColumn(hListTables, 1, (LPARAM)&lvc);

		ListView_SetExtendedListViewStyleEx(hListTables, 0, LVS_EX_FULLROWSELECT | LVS_EX_TWOCLICKACTIVATE);
		SendMessage(hListTables, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);

		TVINSERTSTRUCT tvs;
		tvs.item.mask = TVIF_TEXT;
		tvs.hInsertAfter = TVI_LAST;
		tvs.item.pszText = L"Open PE File";
		tvs.hParent = NULL;
		SendMessage(hListTree, TVM_INSERTITEM, 0, (LPARAM)&tvs);
		tvs.item.mask = TVIF_TEXT;
		tvs.hInsertAfter = TVI_LAST;
		tvs.item.pszText = L"PE Info";
		tvs.hParent = NULL;
		SendMessage(hListTree, TVM_INSERTITEM, 0, (LPARAM)&tvs);
		tvs.item.mask = TVIF_TEXT;
		tvs.hInsertAfter = TVI_LAST;
		tvs.item.pszText = L"PE Import Table";
		tvs.hParent = NULL;	
		SendMessage(hListTree, TVM_INSERTITEM, 0, (LPARAM)&tvs);
		tvs.item.mask = TVIF_TEXT;
		tvs.hInsertAfter = TVI_LAST;
		tvs.item.pszText = L"PE Export Table";
		tvs.hParent = NULL;
		SendMessage(hListTree, TVM_INSERTITEM, 0, (LPARAM)&tvs);
		tvs.item.mask = TVIF_TEXT;
		tvs.hInsertAfter = TVI_LAST;
		tvs.item.pszText = L"Test";
		tvs.hParent = NULL;
		SendMessage(hListTree, TVM_INSERTITEM, 0, (LPARAM)&tvs);

		if (!StrEmepty(currentOpenPEFile)) 
		{
			std::wstring w = FormatString(L"View %s", currentOpenPEFile);
			SetWindowText(hDlg, w.c_str());

			if (vImportTables)
				LoadImportTables(hDlg);
			else if (vExportTables)
				LoadExportTables(hDlg);
		}

		break;
	}
	case WM_SYSCOMMAND: {
		if (wParam == SC_CLOSE) {
			EndDialog(hDlg, 0);
		}
		return 0;
	}
	case WM_SIZE: {
		RECT rc;
		GetClientRect(hDlg, &rc);
		MoveWindow(hListTables, 222, 0, rc.right - rc.left - 222, rc.bottom - rc.top, TRUE);
		MoveWindow(hListTree, 0, 0, 222, rc.bottom - rc.top, TRUE);
		break;
	}
	case WM_NOTIFY: {
		LPNMHDR lpnmh = (LPNMHDR)lParam;
		if (NM_CLICK == lpnmh->code)
		{
			DWORD dwPos = GetMessagePos();
			POINT pt;
			pt.x = LOWORD(dwPos);
			pt.y = HIWORD(dwPos);
			ScreenToClient(lpnmh->hwndFrom, &pt);
			TVHITTESTINFO ht = { 0 };
			ht.pt = pt;
			ht.flags = TVHT_ONITEM;
			HTREEITEM hItem = TreeView_HitTest(lpnmh->hwndFrom, &ht);
			TVITEM ti = { 0 };
			ti.mask = TVIF_HANDLE | TVIF_TEXT;
			TCHAR buf[32] = { 0 };
			ti.cchTextMax = 32;
			ti.pszText = buf;
			ti.hItem = hItem;
			TreeView_GetItem(lpnmh->hwndFrom, &ti);
			if (StrEqual(buf, L"PE Import Table"))
				LoadImportTables(hDlg);
			else if (StrEqual(buf, L"PE Export Table"))
				LoadExportTables(hDlg);
			else if (StrEqual(buf, L"Open PE File"))
				OpenPEFile(hDlg);
			else if (StrEqual(buf, L"PE Info"))
				LoadPEInfo(hDlg);
			
			//else if (StrEqual(buf, L"Test"))
			//	Add2StringItem(hListTables, L"Test", L"This is a test item");
		}
	}
	break;
	}
	return (INT_PTR)FALSE;
}


VOID MAppVPE(LPWSTR pszFile, HWND hWnd)
{

	vImportTables = FALSE;
	vExportTables = FALSE;
	if (!StrEmepty(pszFile))  wcscpy_s(currentOpenPEFile, pszFile);
	DialogBoxW(hInst, MAKEINTRESOURCE(IDD_PEVIEW), hWnd, VPEDlgProc);
}
VOID MAppVPEExp(LPWSTR pszFile, HWND hWnd)
{
	vImportTables = FALSE;
	vExportTables = TRUE;
	wcscpy_s(currentOpenPEFile, pszFile);
	DialogBoxW(hInst, MAKEINTRESOURCE(IDD_PEVIEW), hWnd, VPEDlgProc);
}
VOID MAppVPEImp(LPWSTR pszFile, HWND hWnd)
{
	vExportTables = FALSE;
	vImportTables = TRUE;
	wcscpy_s(currentOpenPEFile, pszFile);
	DialogBoxW(hInst, MAKEINTRESOURCE(IDD_PEVIEW), hWnd, VPEDlgProc);
}
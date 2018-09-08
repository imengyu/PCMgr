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

BOOL MAppVPECheck(LPBYTE lpBaseAddress) {
	//check MZ signature
	BOOL bMZ = FALSE;
	BYTE *bMZsig = (BYTE*)lpBaseAddress;
	if ('M' == *bMZsig)
	{
		bMZsig++;
		if ('Z' == *bMZsig)
		{
			bMZ = TRUE;
		}
	}

	//check PE signature
	BOOL bPE = FALSE;
	BYTE  *bPEoffset = (BYTE*)lpBaseAddress + sizeof(IMAGE_DOS_HEADER) - 4;
	BYTE *bPEsig = (BYTE*)lpBaseAddress + (*bPEoffset);
	if ('P' == *bPEsig)
	{
		bPEsig++;
		if ('E' == *bPEsig)
			bPE = TRUE;
	}
	return (bMZ && bPE);
}

VOID AddAStringItem(HWND hList, LPWSTR str)
{
	LVITEM li = { 0 };
	li.mask = LVIF_TEXT;
	li.iItem = ListView_GetItemCount(hList);
	li.iSubItem = 0;
	li.pszText = L"";
	li.cchTextMax = 1;
	ListView_InsertItem(hList, &li);
	li.iItem = 0;
	li.iSubItem = 1;
	li.pszText = str;
	li.cchTextMax = static_cast<int>(wcslen(str) + 1);
	ListView_SetItem(hList, &li);
}
VOID Add2StringItem(HWND hList, LPWSTR str, LPWSTR str2) {
	LVITEM li = { 0 };
	li.mask = LVIF_TEXT;
	li.pszText = str;
	li.cchTextMax = static_cast<int>(wcslen(str) + 1);
	ListView_InsertItem(hList, &li);
	li.iSubItem = 1;
	li.pszText = str2;
	li.cchTextMax = static_cast<int>(wcslen(str2) + 1);
	ListView_SetItem(hList, &li);
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

	HANDLE hFile = CreateFile(currentOpenPEFile, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hFile == INVALID_HANDLE_VALUE)
	{
		WCHAR err[32]; swprintf_s(err, L"Create file failed : %d", GetLastError());
		AddAStringItem(hListTables, err);
		return;
	}

	HANDLE hFileMapping = CreateFileMapping(hFile, NULL, PAGE_READONLY, 0, 0, NULL);
	if (hFileMapping == NULL || hFileMapping == INVALID_HANDLE_VALUE)
	{
		WCHAR err[64]; swprintf_s(err, L"Could not create file mapping object (%d)", GetLastError());
		AddAStringItem(hListTables, err);
		CloseHandle(hFile);
		return;
	}

	LPBYTE lpBaseAddress = (LPBYTE)MapViewOfFile(hFileMapping, FILE_MAP_READ, 0, 0, 0);
	if (lpBaseAddress == NULL)
	{
		WCHAR err[64]; swprintf_s(err, L"Could not map view of file (%d)", GetLastError());
		AddAStringItem(hListTables, err);
		CloseHandle(hFileMapping);
		CloseHandle(hFile);
		return;
	}
	if (!MAppVPECheck(lpBaseAddress)) {
		AddAStringItem(hListTables, L"This is not a valid PE file.");
		goto UNMAP_AND_EXIT;
	}

	PIMAGE_DOS_HEADER pDosHeader = (PIMAGE_DOS_HEADER)lpBaseAddress;
	PIMAGE_NT_HEADERS pNtHeaders = (PIMAGE_NT_HEADERS)(lpBaseAddress + pDosHeader->e_lfanew);

	//导入表的rva：0x2a000;
	DWORD Rva_import_table = pNtHeaders->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress;
	if (Rva_import_table == 0)
	{
		AddAStringItem(hListTables, L"No import table!");
		goto UNMAP_AND_EXIT;
	}

	//这个虽然是内存地址，但是减去文件开头的地址，就是文件地址了
	//这个地址可以直接从里面读取你想要的东西了
	PIMAGE_IMPORT_DESCRIPTOR pImportTable = (PIMAGE_IMPORT_DESCRIPTOR)ImageRvaToVa(
		pNtHeaders,
		lpBaseAddress,
		Rva_import_table,
		NULL
	);

	//减去内存映射的首地址，就是文件地址了。。（很简单吧）
	//printf("FileAddress Of ImportTable: %p\n", ((DWORD)pImportTable - (DWORD)lpBaseAddress));

	//现在来到了导入表的面前：IMAGE_IMPORT_DESCRIPTOR 数组（以0元素为终止）
	//定义表示数组结尾的null元素！
	IMAGE_IMPORT_DESCRIPTOR null_iid;
	IMAGE_THUNK_DATA null_thunk;
	memset(&null_iid, 0, sizeof(null_iid));
	memset(&null_thunk, 0, sizeof(null_thunk));

	//每个元素代表了一个引入的DLL。
	int i, j;
	for (i = 0; memcmp(pImportTable + i, &null_iid, sizeof(null_iid)) != 0; i++)
	{		
		//拿到了DLL的名字
		//LPCSTR: 就是 const char*
		LPCSTR szDllName = (LPCSTR)ImageRvaToVa(
			pNtHeaders, lpBaseAddress,
			pImportTable[i].Name, //DLL名称的RVA
			NULL);

		//现在去看看从该DLL中引入了哪些函数
		//我们来到该DLL的 IMAGE_TRUNK_DATA 数组（IAT：导入地址表）前面
		PIMAGE_THUNK_DATA32 pThunk = (PIMAGE_THUNK_DATA32)ImageRvaToVa(
			pNtHeaders, lpBaseAddress,
			pImportTable[i].OriginalFirstThunk, //【注意】这里使用的是OriginalFirstThunk
			NULL);

		for (j = 0; memcmp(pThunk + j, &null_thunk, sizeof(null_thunk)) != 0; j++)
		{
			//这里通过RVA的最高位判断函数的导入方式，
			//如果最高位为1，按序号导入，否则按名称导入
			if (pThunk[j].u1.AddressOfData & IMAGE_ORDINAL_FLAG32)
			{
				WCHAR msg[32];
				WCHAR number[32];
				swprintf_s(msg, L"#%d %hs", j, szDllName);
				swprintf_s(number, L"%ld",pThunk[j].u1.AddressOfData & 0xffff);
				Add2StringItem(hListTables, msg, number);
			}
			else
			{
				//按名称导入，我们再次定向到函数序号和名称
				//注意其地址不能直接用，因为仍然是RVA！
				PIMAGE_IMPORT_BY_NAME pFuncName = (PIMAGE_IMPORT_BY_NAME)ImageRvaToVa(
					pNtHeaders, lpBaseAddress,
					pThunk[j].u1.AddressOfData,
					NULL);

				WCHAR msg[32];
				WCHAR number[64];
				swprintf_s(msg, L"#%d %hs", j, szDllName);
				swprintf_s(number, L"%hs (%ld)", pFuncName->Name, pFuncName->Hint);
				Add2StringItem(hListTables, msg, number);
			}
		}
	}
UNMAP_AND_EXIT:
	UnmapViewOfFile(lpBaseAddress);
	CloseHandle(hFileMapping);
	CloseHandle(hFile);
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

	HANDLE hFile = CreateFile(currentOpenPEFile, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hFile == INVALID_HANDLE_VALUE)
	{
		WCHAR err[32]; swprintf_s(err, L"Create file failed : %d", GetLastError());
		AddAStringItem(hListTables, err);
		return;
	}

	HANDLE hFileMapping = CreateFileMapping(hFile, NULL, PAGE_READONLY, 0, 0, NULL);
	if (hFileMapping == NULL || hFileMapping == INVALID_HANDLE_VALUE)
	{
		WCHAR err[64]; swprintf_s(err, L"Could not create file mapping object (%d)", GetLastError());
		AddAStringItem(hListTables, err);
		CloseHandle(hFile);
		return;
	}

	//内存映射文件的基址
	LPBYTE lpBaseAddress = (LPBYTE)MapViewOfFile(hFileMapping, FILE_MAP_READ, 0, 0, 0);
	if (lpBaseAddress == NULL)
	{
		WCHAR err[64]; swprintf_s(err, L"Could not map view of file (%d)", GetLastError());
		AddAStringItem(hListTables, err);
		CloseHandle(hFileMapping);
		CloseHandle(hFile);
		return;
	}
	if (!MAppVPECheck(lpBaseAddress)) {
		AddAStringItem(hListTables, L"This is not a valid PE file.");
		goto UNMAP_AND_EXIT;
	}

	PIMAGE_DOS_HEADER pDosHeader = (PIMAGE_DOS_HEADER)lpBaseAddress;
	PIMAGE_NT_HEADERS pNtHeader = (PIMAGE_NT_HEADERS)((ULONG_PTR)lpBaseAddress + pDosHeader->e_lfanew);
	PIMAGE_OPTIONAL_HEADER pOptionalHeader = (PIMAGE_OPTIONAL_HEADER)((PBYTE)lpBaseAddress + pDosHeader->e_lfanew + offsetof(IMAGE_NT_HEADERS, OptionalHeader));
	PIMAGE_EXPORT_DIRECTORY pExportDirectory = (PIMAGE_EXPORT_DIRECTORY)((PBYTE)lpBaseAddress + pOptionalHeader->DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress);
	PDWORD aryAddressOfNames = (PDWORD)((PBYTE)lpBaseAddress + pExportDirectory->AddressOfNames);
	DWORD dwNumberOfNames = pExportDirectory->NumberOfNames;
	if (dwNumberOfNames == 0)AddAStringItem(hListTables, L"This PE File has not export table");
	else {
		for (UINT i = 0; i < dwNumberOfNames; i++)
		{
			char *strFunction = (char *)(aryAddressOfNames[i] + (ULONG_PTR)lpBaseAddress);
			WCHAR msg[32];
			WCHAR number[32];
			swprintf_s(msg, L"#%d", i);
			swprintf_s(number, L"%hs", strFunction);
			Add2StringItem(hListTables, msg, number);
		}
	}

UNMAP_AND_EXIT:
	UnmapViewOfFile(lpBaseAddress);
	CloseHandle(hFileMapping);
	CloseHandle(hFile);
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

		std::wstring w = FormatString(L"View %s", currentOpenPEFile);
		SetWindowText(hDlg, w.c_str());

		if (vImportTables)
			LoadImportTables(hDlg);
		else if (vExportTables)
			LoadExportTables(hDlg);

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
	wcscpy_s(currentOpenPEFile, pszFile);
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
#include "stdafx.h"
#include "comdlghlp.h"
#include <CommDlg.h>
#include <Shlobj.h>
#include <Shobjidl.h>

BOOL MChooseFileSingal(HWND hWnd, LPWSTR startDir, LPWSTR title, LPWSTR fileFilter, LPWSTR fileName, LPWSTR defExt, LPWSTR*strrs, size_t bufsize)
{
	if (strrs) {
		OPENFILENAME ofn;
		TCHAR szFile[MAX_PATH];
		if (fileName != 0 && wcslen(fileName) != 0)
			wcscpy_s(szFile, fileName);
		ZeroMemory(&ofn, sizeof(OPENFILENAME));
		ofn.lStructSize = sizeof(OPENFILENAME);
		ofn.hwndOwner = hWnd;
		ofn.lpstrFile = szFile;
		ofn.lpstrFile[0] = '\0';
		ofn.nMaxFile = sizeof(szFile);
		ofn.nFilterIndex = 1;
		ofn.lpstrFilter = fileFilter;
		ofn.lpstrDefExt = defExt;
		ofn.lpstrFileTitle = title;
		ofn.nMaxFileTitle = 0;
		ofn.lpstrInitialDir = startDir;
		ofn.Flags = OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST;
		if (GetOpenFileName(&ofn))
		{
			//显示选择的文件。 szFile
			wcscpy_s(*strrs, bufsize, szFile);
			return TRUE;
		}
	}
	return 0;
}
BOOL MChooseDir(HWND hWnd, LPWSTR startDir, LPWSTR title, LPWSTR*strrs, size_t bufsize)
{
	if (strrs) {
		IFileDialog *pfd = NULL;
		HRESULT hr = CoCreateInstance(CLSID_FileOpenDialog,
			NULL,
			CLSCTX_INPROC_SERVER,
			IID_PPV_ARGS(&pfd));
		if (SUCCEEDED(hr))
		{
			DWORD dwFlags;
			if (startDir != 0 && wcslen(startDir) != 0)
			{
				IShellItem*item_folder;
				LPITEMIDLIST idl;
				DWORD atts = 0;
				if (SHILCreateFromPath(startDir, &idl, &atts) == 0)
				{
					if (SHCreateShellItem(NULL, NULL, idl, &item_folder) == 0)
						pfd->SetFolder(item_folder);
				}
			}
			hr = pfd->GetOptions(&dwFlags);
			hr = pfd->SetOptions(dwFlags | FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM);
			if (title != 0) hr = pfd->SetTitle(title);
			hr = pfd->Show(hWnd);
			if (hr == ERROR_CANCELLED)
				return FALSE;
			IShellItem*item;			
			hr = pfd->GetResult(&item);
			LPWSTR pathrs;
			item->GetDisplayName(SIGDN_FILESYSPATH, &pathrs);
			wcscpy_s(*strrs, bufsize, pathrs);
			return TRUE;
		}
	}
	return FALSE;
}
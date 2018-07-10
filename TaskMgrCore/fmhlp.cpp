#include "stdafx.h"
#include "fmhlp.h"
#include "StringHlp.h"
#include "resource.h"
#include "comdlghlp.h"
#include "PathHelper.h"
#include "mapphlp.h"

extern HINSTANCE hInst;
extern HWND hWndMain;

LPWSTR fmCurrectSelectFilePath0;
bool fmMutilSelect = false;
int fmMutilSelectCount = 0;
bool fmShowHiddenFile = false;
LPWSTR fmCurrectSelectFolderPath0;

MFCALLBACK mfmain_callback;
HICON hiconFolder = NULL, hiconMyComputer = NULL;
LPWSTR strMyComputer = NULL;

// 获取文件图标
M_API HICON MFM_GetFileIcon(LPWSTR extention, LPWSTR s, int count)
{
	HICON icon = NULL;
	if (extention && wcslen(extention))
	{
		SHFILEINFO info;
		if (SHGetFileInfo(extention,
			FILE_ATTRIBUTE_NORMAL,
			&info,
			sizeof(info),
			SHGFI_SYSICONINDEX | SHGFI_ICON | (count == 0 ? 0 : SHGFI_TYPENAME) | SHGFI_USEFILEATTRIBUTES))
		{
			icon = info.hIcon;
			if (count != 0) wcscpy_s(s, count, info.szTypeName);
		}
	}
	return icon;
}
// 获取文件夹图标
M_API HICON MFM_GetFolderIcon()
{
	if (hiconFolder == NULL) {
		SHFILEINFOA info;
		if (SHGetFileInfoA("folder",
			FILE_ATTRIBUTE_DIRECTORY,
			&info,
			sizeof(info),
			SHGFI_SYSICONINDEX | SHGFI_ICON | SHGFI_USEFILEATTRIBUTES))
			hiconFolder = info.hIcon;
	}
	return hiconFolder;
}
M_API HICON MFM_GetMyComputerIcon()
{
	if (hiconMyComputer == NULL) {
		SHFILEINFOA info;
		LPITEMIDLIST pidl=0;
		SHGetSpecialFolderLocation(0, CSIDL_DRIVES, &pidl);
		if (SHGetFileInfoA((LPCSTR)pidl, FILE_ATTRIBUTE_NORMAL, &info,
			sizeof(info), SHGFI_PIDL | SHGFI_ICON | SHGFI_USEFILEATTRIBUTES)) 
			hiconMyComputer = info.hIcon;
		IMalloc*im = 0;
		SHGetMalloc(&im);
		if (im) {
			im->Free(pidl);
			im->Release();
		}
	}
	return hiconMyComputer;
}
M_API LPWSTR MFM_GetMyComputerName()
{
	if (strMyComputer == NULL) {
		SHFILEINFO info;
		LPITEMIDLIST pidl = 0;
		SHGetSpecialFolderLocation(0, CSIDL_DRIVES, &pidl);
		if (SHGetFileInfo((LPWSTR)pidl, FILE_ATTRIBUTE_NORMAL, &info,
			sizeof(info), SHGFI_PIDL | SHGFI_DISPLAYNAME | SHGFI_USEFILEATTRIBUTES))
			strMyComputer = info.szDisplayName;
		IMalloc*im = 0;
		SHGetMalloc(&im);
		if (im) {
			im->Free(pidl);
			im->Release();
		}
	}
	return strMyComputer;
}

M_API VOID MShowFileProp(LPWSTR file)
{
	SHELLEXECUTEINFO info = { 0 };
	info.cbSize = sizeof(SHELLEXECUTEINFO);
	info.hwnd = hWndMain;
	info.lpVerb = L"properties";
	info.lpFile = file;
	info.nShow = SW_SHOW;
	info.fMask = SEE_MASK_INVOKEIDLIST;
	ShellExecuteEx(&info);
}
M_API BOOL MCopyToClipboard(const WCHAR* pszData, const int nDataLen)
{
	if (OpenClipboard(NULL))
	{
		EmptyClipboard();
		HGLOBAL clipbuffer;
		WCHAR *buffer;
		clipbuffer = ::GlobalAlloc(GMEM_DDESHARE, (nDataLen + 1) * sizeof(WCHAR));
		buffer = (WCHAR*)GlobalLock(clipbuffer);
		wcscpy_s(buffer, nDataLen + 1, pszData);
		GlobalUnlock(clipbuffer);
		SetClipboardData(CF_UNICODETEXT, clipbuffer);
		CloseClipboard();
		return TRUE;
	}
	return FALSE;
}

M_API void MFM_GetRoots()
{
	DWORD dwLen = GetLogicalDriveStrings(0, NULL);
	WCHAR * pszDriver = new WCHAR[dwLen];
	WCHAR * pszDriverOld = pszDriver;
	GetLogicalDriveStrings(dwLen, pszDriver);

	while (*pszDriver != '\0') {
		int a = wcslen(pszDriver);
		DWORD serialNumber, maxComponentLength, fsFlags;

		TCHAR szFileSystem[16] = { 0 };
		TCHAR szVolumeName[32] = { 0 };
		TCHAR szRoot[4] = { 0 };

		wcscpy_s(szRoot, pszDriver);

		if (!GetVolumeInformationW(
			szRoot,
			szVolumeName,
			sizeof(szVolumeName),
			&serialNumber,
			&maxComponentLength,
			&fsFlags,
			szFileSystem,
			sizeof(szFileSystem))) {
			continue;
		}

		std::wstring w = FormatString(L"%s/%s (%s)", szVolumeName, szFileSystem, szRoot);
		mfmain_callback(2, (LPVOID)w.c_str(), (LPVOID)szRoot);

		pszDriver += a + 1;
	}
	delete pszDriverOld;
}
M_API void MFM_SetCallBack(MFCALLBACK cp)
{
	mfmain_callback = cp;
}
M_API BOOL MFM_GetFolders(LPWSTR path)
{
	if (_waccess_s(path, 0) == 0)
	{
		WIN32_FIND_DATA FindData;
		HANDLE hError;
		int FileCount = 0;
		// 构造路径
		WCHAR FullPathName[MAX_PATH];
		wcscpy_s(FullPathName, path);
		wcscat_s(FullPathName, L"\\*.*");
		hError = FindFirstFile(FullPathName, &FindData);
		if (hError == INVALID_HANDLE_VALUE) {
			mfmain_callback(3, 0, (LPVOID)-1);
			return 0;
		}
		while (FindNextFile(hError, &FindData))
		{
			// 过虑.和..
			if (wcscmp(FindData.cFileName, L".") == 0
				|| wcscmp(FindData.cFileName, L"..") == 0)
				continue;
			if (!fmShowHiddenFile && FindData.dwFileAttributes & FILE_ATTRIBUTE_HIDDEN)
				continue;
			if (FindData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
				mfmain_callback(3, FindData.cFileName, path);
		}
		return TRUE;
	}
	else mfmain_callback(3, 0, (LPVOID)-1);
	return 0;
}
M_API BOOL MFM_OpenFile(LPWSTR path, HWND hWnd)
{
	return ShellExecute(hWnd, L"open", path, NULL, NULL, 5) != NULL;
}
M_API BOOL MFM_ReUpdateFile(LPWSTR fullPath, LPWSTR dirPath)
{
	WIN32_FIND_DATA FindData;
	HANDLE hError;
	hError = FindFirstFile(fullPath, &FindData);
	if (hError == INVALID_HANDLE_VALUE)
		return 0;
	if (FindData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
		mfmain_callback(5, FindData.cFileName, dirPath);
	else mfmain_callback(6, &FindData, dirPath);
	return 0;
}
M_API BOOL MFM_UpdateFile(LPWSTR fullPath, LPWSTR dirPath)
{
	if (_waccess_s(fullPath, 0) == 0)
	{
		WIN32_FIND_DATA FindData;
		HANDLE hError;
		hError = FindFirstFile(fullPath, &FindData);
		if (hError == INVALID_HANDLE_VALUE)
			return 0;
		if (!(FindData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY))
			mfmain_callback(26, &FindData, dirPath);
	}
	return 0;
}
M_API BOOL MFM_GetFiles(LPWSTR path)
{
	if (_waccess_s(path, 0) == 0)
	{
		WIN32_FIND_DATA FindData;
		HANDLE hError;
		int FileCount = 0;
		// 构造路径
		WCHAR FullPathName[MAX_PATH];
		wcscpy_s(FullPathName, path);
		wcscat_s(FullPathName, L"\\*.*");
		hError = FindFirstFile(FullPathName, &FindData);
		if (hError == INVALID_HANDLE_VALUE) {
			mfmain_callback(6, path, (LPVOID)-1);
			return 0;
		}
		while (FindNextFile(hError, &FindData))
		{
			// 过虑.和..
			if (wcscmp(FindData.cFileName, L".") == 0)
				continue;
			else if (wcscmp(FindData.cFileName, L"..") == 0) {
				mfmain_callback(7, L"..", path);
				continue;
			}
			if (!fmShowHiddenFile && FindData.dwFileAttributes & FILE_ATTRIBUTE_HIDDEN)
				continue;
			if (FindData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
				mfmain_callback(5, FindData.cFileName, path);
			else mfmain_callback(6, &FindData, path);
		}
		return TRUE;
	}
	else mfmain_callback(6, 0, (LPVOID)-1);
	return 0;
}
M_API BOOL MFM_GetFileTime(FILETIME *ft, LPWSTR s, int count)
{
	SYSTEMTIME time = { 0 };
	if (FileTimeToSystemTime(ft, &time))
	{
		std::wstring w = FormatString(L"%d-%d-%d %d:%d:%d", time.wYear, time.wMonth, time.wDay, time.wHour, time.wMinute, time.wSecond);
		wcscpy_s(s, count, w.c_str());
		return 1;
	}
	return 0;
}
M_API BOOL MFM_GetFileAttr(DWORD att, LPWSTR s, int count, BOOL*hiddenout)
{
	if (att)
	{
		bool readonly = att & FILE_ATTRIBUTE_READONLY;
		bool hidden = att & FILE_ATTRIBUTE_HIDDEN;
		bool system = att & FILE_ATTRIBUTE_SYSTEM;

		std::wstring w;
		if (readonly) w += L"只读 ";
		if (hidden) w += L"隐藏 ";
		if (system) w += L"系统文件 ";
		wcscpy_s(s, count, w.c_str());
		*hiddenout = hidden;
		return 1;
	}
	return 0;
}
M_API void MFM_Refesh()
{
	mfmain_callback(8, 0, 0);
}
M_API void MFM_Recall(int id, LPWSTR path)
{
	mfmain_callback(id, path, 0);
}
M_API int MFM_CopyOrCutFileToClipboard(LPWSTR szFileName, BOOL isCopy)
{
	UINT uDropEffect;
	HGLOBAL hGblEffect;
	LPDWORD lpdDropEffect;
	DROPFILES stDrop;
	HGLOBAL hGblFiles;
	LPSTR lpData;
	uDropEffect = RegisterClipboardFormat(L"Preferred DropEffect");
	hGblEffect = GlobalAlloc(GMEM_ZEROINIT | GMEM_MOVEABLE | GMEM_DDESHARE, sizeof(DWORD));
	lpdDropEffect = (LPDWORD)GlobalLock(hGblEffect);
	*lpdDropEffect = isCopy ? DROPEFFECT_COPY : DROPEFFECT_MOVE;//复制; 剪贴则用DROPEFFECT_MOVE
	GlobalUnlock(hGblEffect);
	stDrop.pFiles = sizeof(DROPFILES);
	stDrop.pt.x = 0;
	stDrop.pt.y = 0;
	stDrop.fNC = FALSE;
	stDrop.fWide = FALSE;
	hGblFiles = GlobalAlloc(GMEM_ZEROINIT | GMEM_MOVEABLE | GMEM_DDESHARE, sizeof(DROPFILES) + wcslen(szFileName) + 2);
	lpData = (LPSTR)GlobalLock(hGblFiles);	
	memcpy_s(lpData, GlobalSize(lpData), &stDrop, sizeof(DROPFILES));
	wcscpy_s((LPWSTR)lpData + sizeof(DROPFILES), GlobalSize(lpData) - sizeof(DROPFILES), szFileName);
	GlobalUnlock(hGblFiles);
	OpenClipboard(NULL);
	EmptyClipboard();
	SetClipboardData(CF_HDROP, hGblFiles);
	SetClipboardData(uDropEffect, hGblEffect);
	CloseClipboard();
	return 1;
}
M_API void MFM_SetStatus(LPWSTR st)
{
	mfmain_callback(14, st, 0);
}
M_API void MFM_SetStatus2(int st)
{
	mfmain_callback(15, (VOID*)st, 0);
}
M_API BOOL MFM_IsValidateFolderFileName(wchar_t *pName)
{
	std::wstring w(pName);
	return Path::IsValidateFolderFileName(&w);
}
M_API BOOL MFM_CreateDir(wchar_t *path)
{
	return CreateDirectory(path, NULL);
}
M_API BOOL MFM_DeleteDirOrFile(wchar_t *path)
{
	if (MFM_IsPathDir(path))
	{
		return MFM_DeleteDir(path);
	}
	else
	{
		return DeleteFile(path);
	}
}
M_API BOOL MFM_DeleteDir(const wchar_t* szFileDir)
{
	std::wstring strDir = szFileDir;
	if (strDir.at(strDir.length() - 1) != '\\')
		strDir += '\\';
	WIN32_FIND_DATA wfd;
	HANDLE hFind = FindFirstFile((strDir + L"*.*").c_str(), &wfd);
	if (hFind == INVALID_HANDLE_VALUE)
		return false;
	do
	{
		if (wfd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
		{
			if (_wcsicmp(wfd.cFileName, L".") != 0 &&
				_wcsicmp(wfd.cFileName, L"..") != 0)
				MFM_DeleteDir((strDir + wfd.cFileName).c_str());
		}
		else DeleteFile((strDir + wfd.cFileName).c_str());
	} while (FindNextFile(hFind, &wfd));
	FindClose(hFind);
	RemoveDirectory(szFileDir);
	return true;
}
M_API BOOL MFM_IsPathDir(const wchar_t* path) {
	struct _stat buf = { 0 };
	_wstat(path, &buf);
	return buf.st_mode & _S_IFDIR;
}
M_API LPWSTR MFM_GetSeledItemPath(int index)
{
	return (LPWSTR)mfmain_callback(16, (VOID*)index, 0);
}
M_API void MFM_GetSeledItemFree(void* v)
{
	mfmain_callback(17, v, 0);
}
M_API BOOL MFM_GetShowHiddenFiles()
{
	fmShowHiddenFile = (BOOL)mfmain_callback(18, 0, 0);
	return fmShowHiddenFile;
}
M_API void MFM_SetShowHiddenFiles(BOOL b)
{
	fmShowHiddenFile = b;
}
void MFM_ReSetShowHiddenFiles()
{
	fmShowHiddenFile = !fmShowHiddenFile;

}
/*
LPWSTR buf;
for (int i = 0; i < fmMutilSelectCount; i++)
{
if (i == 0)buf = fmCurrectSelectFilePath0;
else buf = MFM_GetSeledItemPath(i);

if (i != 0)MFM_GetSeledItemFree(buf);
}
*/

BOOL MFM_RenameFile() {
	return 0;
}
BOOL MFM_MoveFileToUser()
{
	WCHAR targetDir[MAX_PATH];
	if (MChooseDir(hWndMain, NULL, L"选择移动目标文件夹", (LPWSTR*)&targetDir, sizeof(targetDir)))
	{
		if (fmMutilSelect) {
			std::wstring paths(fmCurrectSelectFilePath0);
			std::wstring target_paths(targetDir);
			target_paths += L'\\';
			target_paths += fmCurrectSelectFilePath0;
			LPWSTR buf;
			for (int i = 1; i < fmMutilSelectCount; i++)
			{
				buf = MFM_GetSeledItemPath(i);
				target_paths += L'\0';
				target_paths += targetDir;
				target_paths += L'\\';
				target_paths += buf;
				paths += L'\0';
				paths += buf;
				MFM_GetSeledItemFree(buf);
			}
			SHFILEOPSTRUCT FileOp;
			FileOp.hwnd = hWndMain;
			FileOp.wFunc = FO_MOVE; 
			FileOp.pFrom = paths.c_str();
			FileOp.pTo = target_paths.c_str();
			FileOp.fFlags = FOF_NOCONFIRMMKDIR;
			FileOp.hNameMappings = NULL;
			FileOp.lpszProgressTitle = L"正在移动文件...";
			return SHFileOperation(&FileOp);
		}
		else {
			std::wstring *w = Path::GetFileName((LPWSTR)fmCurrectSelectFilePath0);
			*w = targetDir + (L"\\" + *w);
			delete w;
			return MoveFile(fmCurrectSelectFilePath0, w->c_str());
		}
	}
	else return 1; 
	return 0;
}
BOOL MFM_CopyFileToUser()
{
	WCHAR targetDir[MAX_PATH];
	if (MChooseDir(hWndMain, NULL, L"选择复制目标文件夹", (LPWSTR*)&targetDir, sizeof(targetDir)))
	{
		if (fmMutilSelect) {
			std::wstring paths(fmCurrectSelectFilePath0);
			std::wstring target_paths(targetDir);
			target_paths += L'\\';
			target_paths += fmCurrectSelectFilePath0;
			LPWSTR buf;
			for (int i = 1; i < fmMutilSelectCount; i++)
			{
				buf = MFM_GetSeledItemPath(i);
				target_paths += L'\0';
				target_paths += targetDir;
				target_paths += L'\\';
				target_paths += buf;
				paths += L'\0';
				paths += buf;
				MFM_GetSeledItemFree(buf);
			}
			SHFILEOPSTRUCT FileOp;
			FileOp.hwnd = hWndMain;
			FileOp.wFunc = FO_COPY;
			FileOp.pFrom = paths.c_str();
			FileOp.pTo = target_paths.c_str();
			FileOp.fFlags = FOF_NOCONFIRMMKDIR;
			FileOp.hNameMappings = NULL;
			FileOp.lpszProgressTitle = L"正在复制文件...";
			return SHFileOperation(&FileOp);
		}
		else {
			std::wstring *w = Path::GetFileName((LPWSTR)fmCurrectSelectFilePath0);
			*w = targetDir + (L"\\" + *w);
			bool replace = false;
			if (MShowMessageDialog(hWndMain, (LPWSTR)w->c_str(), L"文件已经存在，覆盖吗？", L"复制文件疑问", MB_ICONEXCLAMATION, MB_YESNO) == IDOK)
				replace = true;
			BOOL rs = CopyFile(fmCurrectSelectFilePath0, w->c_str(), !replace);
			delete w;
			if (!rs)
			{
				if (GetLastError() == ERROR_ALREADY_EXISTS)
				{
					MShowErrorMessage(L"文件已经存在", L"复制文件错误", MB_ICONEXCLAMATION, 0);
					return TRUE;
				}
			}
			return rs;
		}
	}
	else return 1;
	return 0;
}
BOOL MFM_DelFileToRecBinUser()
{
	if (fmMutilSelect) {
		if (MShowMessageDialog(hWndMain, fmCurrectSelectFilePath0, L"删除确认", (LPWSTR)FormatString(L"真的要删除这 %d 个文件？").c_str(), MB_ICONEXCLAMATION, MB_YESNO) == IDYES)
		{
			std::wstring paths;
			LPWSTR buf;
			for (int i = 0; i < fmMutilSelectCount; i++)
			{
				if (i == 0)buf = fmCurrectSelectFilePath0;
				else buf = MFM_GetSeledItemPath(i);
				paths += L'\0';
				paths += buf;
				if (i != 0)MFM_GetSeledItemFree(buf);
			}
			SHFILEOPSTRUCT FileOp;//定义SHFILEOPSTRUCT结构对象;
			FileOp.hwnd = hWndMain;
			FileOp.wFunc = FO_DELETE; //执行文件删除操作;
			FileOp.pFrom = paths.c_str();
			FileOp.pTo = paths.c_str();
			FileOp.fFlags = FOF_ALLOWUNDO;//此标志使删除文件备份到Windows回收站
			FileOp.hNameMappings = NULL;
			FileOp.lpszProgressTitle = L"删除文件...";
			return SHFileOperation(&FileOp);
		}
		else return 1;
	}
	else {
		if (_waccess(fmCurrectSelectFilePath0, 0) == 0) {
			if (MShowMessageDialog(hWndMain, fmCurrectSelectFilePath0, L"删除确认" , L"真的要删除这个文件？", MB_ICONEXCLAMATION, MB_YESNO) == IDYES)
			{
				SHFILEOPSTRUCT FileOp;//定义SHFILEOPSTRUCT结构对象;
				FileOp.hwnd = hWndMain;
				FileOp.wFunc = FO_DELETE; //执行文件删除操作;
				FileOp.pFrom = fmCurrectSelectFilePath0;
				FileOp.pTo = fmCurrectSelectFilePath0;
				FileOp.fFlags = FOF_ALLOWUNDO;//此标志使删除文件备份到Windows回收站
				FileOp.hNameMappings = NULL;
				FileOp.lpszProgressTitle = L"删除文件...";
				return SHFileOperation(&FileOp);
			}
			else return 1;
		}
	}
	return 0;
}
BOOL MFM_DelFileBinUser()
{
	if (fmMutilSelect) {
		if (MShowMessageDialog(hWndMain, fmCurrectSelectFilePath0, (LPWSTR)FormatString(L"真的要删除这 %d 个文件？", fmMutilSelectCount).c_str(), L"删除确认", MB_ICONEXCLAMATION, MB_YESNO) == IDYES)
		{
			LPWSTR buf;
			for (int i = 0; i < fmMutilSelectCount; i++)
			{
				if (i == 0)buf = fmCurrectSelectFilePath0;
				else buf = MFM_GetSeledItemPath(i);

				if (!DeleteFile(buf)) MShowErrorMessage(buf, (LPWSTR)FormatString(L"删除文件失败\nLastError : %d", GetLastError()).c_str(), MB_ICONEXCLAMATION, 0);

				if (i != 0)MFM_GetSeledItemFree(buf);
			}
		}
		else return 1;
	}
	else {
		if (_waccess(fmCurrectSelectFilePath0, 0) == 0) {
			if (MShowMessageDialog(hWndMain, fmCurrectSelectFilePath0, L"真的要删除这个文件？", L"删除确认", MB_ICONEXCLAMATION, MB_YESNO) == IDYES)
				if (!MFM_DeleteDirOrFile(fmCurrectSelectFilePath0))
					MShowErrorMessage(fmCurrectSelectFilePath0, (LPWSTR)FormatString(L"删除文件失败\nLastError : %d", GetLastError()).c_str(), MB_ICONEXCLAMATION, 0);
		}
	}
	return 1;
}
void MFF_ShowFolderProp() {
	if (_waccess(fmCurrectSelectFolderPath0, 0) == 0)
		MShowFileProp(fmCurrectSelectFolderPath0);
}
void MFF_ShowInExplorer() {
	if (_waccess(fmCurrectSelectFolderPath0, 0) == 0)
		ShellExecute(hWndMain, L"open", fmCurrectSelectFolderPath0, 0, 0, 5);
}
BOOL MFF_DelToRecBin() {
	if (_waccess(fmCurrectSelectFolderPath0, 0) == 0)
	{
		if (MShowMessageDialog(hWndMain, fmCurrectSelectFilePath0, L"删除确认", L"真的要删除这个文件夹？", MB_ICONEXCLAMATION, MB_YESNO) == IDYES)
		{
			SHFILEOPSTRUCT FileOp;//定义SHFILEOPSTRUCT结构对象;
			FileOp.hwnd = hWndMain;
			FileOp.wFunc = FO_DELETE; //执行文件删除操作;
			FileOp.pFrom = fmCurrectSelectFolderPath0;
			FileOp.pTo = fmCurrectSelectFolderPath0;
			FileOp.fFlags = FOF_ALLOWUNDO;//此标志使删除文件备份到Windows回收站
			FileOp.hNameMappings = NULL;
			FileOp.lpszProgressTitle = L"删除文件...";
			return SHFileOperation(&FileOp);
		}
		else return 1;
	}
	return 0;
}
BOOL MFF_Del() {
	if (_waccess(fmCurrectSelectFolderPath0, 0) == 0)
	{
		if (MShowMessageDialog(hWndMain, fmCurrectSelectFilePath0, L"删除确认", L"真的要删除这个文件夹？", MB_ICONEXCLAMATION, MB_YESNO) == IDYES)
		{
			SHFILEOPSTRUCT FileOp;//定义SHFILEOPSTRUCT结构对象;
			FileOp.hwnd = hWndMain;
			FileOp.wFunc = FO_DELETE; //执行文件删除操作;
			FileOp.pFrom = fmCurrectSelectFolderPath0;
			FileOp.pTo = 0;
			FileOp.hNameMappings = NULL;
			FileOp.lpszProgressTitle = L"删除文件...";
			return SHFileOperation(&FileOp);
		}
		else return 1;
	}	
	return 0;
}
void MFF_Copy() {
	MFM_CopyOrCutFileToClipboard(fmCurrectSelectFolderPath0, 1);
	MFM_SetStatus2(11);
}
void MFF_CopyPath() {
	MCopyToClipboard(fmCurrectSelectFolderPath0, wcslen(fmCurrectSelectFolderPath0));
	MFM_SetStatus2(9);
}
void MFF_Patse() {

}
void MFF_Cut() {
	MFM_CopyOrCutFileToClipboard(fmCurrectSelectFolderPath0, 0);
	MFM_SetStatus2(10);
}
void MFF_Remane() {

}
void MFF_ShowFolder()
{
	MFM_Recall(19, fmCurrectSelectFolderPath0);
}
M_API int MAppWorkShowMenuFM(LPWSTR strFilePath, BOOL mutilSelect, int selectCount)
{
	fmMutilSelectCount = selectCount;
	fmMutilSelect = mutilSelect;
	fmCurrectSelectFilePath0 = strFilePath;
	HMENU hroot = LoadMenu(hInst, MAKEINTRESOURCE(IDR_MENUFMMAIN));
	if (hroot) {
		HMENU hpop = GetSubMenu(hroot, 0);
		DWORD attr = GetFileAttributesW(strFilePath);
		CheckMenuItem(hpop, ID_FMM_READONLY, (attr & FILE_ATTRIBUTE_READONLY) ? MF_CHECKED : MF_UNCHECKED);
		CheckMenuItem(hpop, ID_FMM_HIDDEN, (attr & FILE_ATTRIBUTE_HIDDEN) ? MF_CHECKED : MF_UNCHECKED);
		CheckMenuItem(hpop, ID_FMM_SYSTEM, (attr & FILE_ATTRIBUTE_SYSTEM) ? MF_CHECKED : MF_UNCHECKED);

		if ((!mutilSelect && MFM_IsPathDir(strFilePath)) || mutilSelect)
			EnableMenuItem(hpop, ID_FMMAIN_OPENWAY, MF_DISABLED);

		CheckMenuItem(hpop, ID_FMMAIN_SHIWHIDEDFILES, fmShowHiddenFile ? MF_CHECKED : MF_UNCHECKED);

		POINT pt;
		GetCursorPos(&pt);
		TrackPopupMenu(hpop,
			TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
			pt.x,
			pt.y,
			0,
			hWndMain,
			NULL);

		DestroyMenu(hroot);
	}
	return 0;
}
M_API int MAppWorkShowMenuFMF(LPWSTR strfolderPath)
{
	fmCurrectSelectFolderPath0 = strfolderPath;
	if (wcscmp(fmCurrectSelectFolderPath0, L"\\\\") != 0 && wcscmp(fmCurrectSelectFolderPath0, L"mycp") != 0) {
		HMENU hroot = LoadMenu(hInst, MAKEINTRESOURCE(IDR_MENUFMFOLDER));
		if (hroot) {
			HMENU hpop = GetSubMenu(hroot, 0);

			if (wcslen(fmCurrectSelectFolderPath0) == 3 &&
				fmCurrectSelectFolderPath0[0] >= L'A'&& fmCurrectSelectFolderPath0[0] <= L'Z'&&
				fmCurrectSelectFolderPath0[2] == L'\\')
			{
				EnableMenuItem(hpop, ID_FMFOLDER_REMOVE, MF_DISABLED);
				EnableMenuItem(hpop, ID_FMFOLDER_DEL, MF_DISABLED);
				EnableMenuItem(hpop, ID_FMFOLDER_CUT, MF_DISABLED);
				EnableMenuItem(hpop, ID_FMFOLDER_COPY, MF_DISABLED);
			}

			POINT pt;
			GetCursorPos(&pt);
			TrackPopupMenu(hpop,
				TPM_LEFTALIGN | TPM_TOPALIGN | TPM_RIGHTBUTTON,
				pt.x,
				pt.y,
				0,
				hWndMain,
				NULL);
			DestroyMenu(hroot);
		}
	}
	return 0;
}
#include "stdafx.h"
#include "fmhlp.h"
#include "StringHlp.h"
#include "resource.h"
#include "prochlp.h"
#include "ntdef.h"
#include "comdlghlp.h"
#include "PathHelper.h"
#include "mapphlp.h"
#include "suact.h"
#include "lghlp.h"
#include "loghlp.h"

extern NtQuerySystemInformationFun NtQuerySystemInformation;

extern HINSTANCE hInstRs;
extern HWND hWndMain;

LPWSTR fmCurrectSelectFilePath0;
bool fmMutilSelect = false;
int fmMutilSelectCount = 0;
bool fmShowHiddenFile = false;
LPWSTR fmCurrectSelectFolderPath0;
UINT deletedFileCount = 0;
UINT needDeletedFileCount = 0;

MFCALLBACK mfmain_callback;
HICON hiconFolder = NULL, hiconMyComputer = NULL;
LPWSTR strMyComputer = NULL;

typedef void(__cdecl*MFUSEINGCALLBACK)(SYSTEM_HANDLE_TABLE_ENTRY_INFO handle, DWORD dwpid, LPWSTR value, int fileType, LPWSTR exepath);

M_CAPI (BOOL) MFM_EnumFileHandles(const WCHAR* pszFilePath, MFUSEINGCALLBACK callBack)
{
	if (!callBack) { SetLastError(ERROR_INVALID_PARAMETER); return FALSE; }

	PSYSTEM_HANDLE_INFORMATION pSysHandleInformation = new SYSTEM_HANDLE_INFORMATION;
	DWORD size = sizeof(SYSTEM_HANDLE_INFORMATION);
	DWORD needed = 0;
	NTSTATUS status = NtQuerySystemInformation(SystemHandleInformation, pSysHandleInformation, size, &needed);
	if (!NT_SUCCESS(status))
	{
		if (0 == needed)
		{
			delete pSysHandleInformation;
			SetLastError(ERROR_SHARING_VIOLATION);
			return FALSE;// some other error
		}
		// The previously supplied buffer wasn't enough.
		delete pSysHandleInformation;
		size = needed + 1024;
		pSysHandleInformation = (PSYSTEM_HANDLE_INFORMATION)new BYTE[size];
		status = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS(SystemHandleInformation), pSysHandleInformation, size, &needed);
		if (!NT_SUCCESS(status))
		{
			// some other error so quit.
			delete pSysHandleInformation;
			SetLastError(ERROR_SHARING_VIOLATION);
			return FALSE;
		}
	}

	// iterate over every handle
	for (DWORD i = 0; i < pSysHandleInformation->NumberOfHandles; i++)
	{
		//if (pSysHandleInformation->Handles[i].dwProcessId == GetCurrentProcessId())
		//{
			WCHAR strNtPath[MAX_PATH];
			WCHAR strDosPath[MAX_PATH];
			HANDLE hDup = (HANDLE)pSysHandleInformation->Handles[i].HandleValue;
			MGetNtPathFromHandle(hDup, strNtPath, MAX_PATH);
			MNtPathToDosPath(strNtPath, strDosPath, MAX_PATH);
			if (MStrEqualW(strDosPath, pszFilePath))
			{
				WCHAR strValue[16];
				wsprintf(strValue, L"0x%x", pSysHandleInformation->Handles[i].HandleValue);

				WCHAR exeFullPath[MAX_PATH] = { 0 };
				HANDLE hProcess;
				MGetProcessFullPathEx(pSysHandleInformation->Handles[i].HandleValue, exeFullPath, &hProcess, L"");

				callBack(pSysHandleInformation->Handles[i], pSysHandleInformation->Handles[i].HandleValue, strValue,
					pSysHandleInformation->Handles[i].ObjectTypeIndex, exeFullPath);

				//now we can close file open by another process
				//do rename or delete file again
				//EnableTokenPrivilege(SE_DEBUG_NAME);
				//CloseHandleWithProcess(pSysHandleInformation->Handles[i]);
			}
		//}
	}

	delete pSysHandleInformation;
	return TRUE;
}
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
	else SetLastError(ERROR_INVALID_PARAMETER);
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
M_API BOOL MCopyToClipboard(const WCHAR* pszData, const size_t nDataLen)
{
	if (OpenClipboard(NULL))
	{
		EmptyClipboard();
		HGLOBAL clipbuffer;
		WCHAR *buffer;
		clipbuffer = GlobalAlloc(GMEM_DDESHARE, (nDataLen + 1) * sizeof(WCHAR));
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
		size_t a = wcslen(pszDriver);
		DWORD serialNumber, maxComponentLength, fsFlags;

		if (!lstrcmpi(pszDriver, L"A:\\") || !lstrcmpi(pszDriver, L"B:\\")) {
			pszDriver += a + 1;
			continue;
		}

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
			pszDriver += a + 1;
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
			if (MStrEqualW(FindData.cFileName, L".") || MStrEqualW(FindData.cFileName, L".."))
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
			if (MStrEqualW(FindData.cFileName, L"."))
				continue;
			else if (MStrEqualW(FindData.cFileName, L"..")) {
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
	mfmain_callback(15, (VOID*)(ULONG_PTR)st, 0);
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
M_API UINT MFM_CalcFileCount(const wchar_t* szFileDir)
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
				_wcsicmp(wfd.cFileName, L"..") != 0) {
				MFM_CalcFileCount((strDir + wfd.cFileName).c_str());
				needDeletedFileCount++;
			}
		}
		else {
			if (_wcsicmp(wfd.cFileName, L".") != 0 &&
				_wcsicmp(wfd.cFileName, L"..") != 0)
				needDeletedFileCount++;
		}

	} while (FindNextFile(hFind, &wfd));
	FindClose(hFind);
	return needDeletedFileCount;
}
M_API BOOL MFM_DeleteDir(const wchar_t* szFileDir)
{
	size_t strsize = wcslen(szFileDir) + 1;
	LPWSTR path = (LPWSTR)malloc(strsize * sizeof(wchar_t));
	wcscpy_s(path, strsize, szFileDir);
	HANDLE hThread = CreateThread(NULL, 0, MFM_DeleteDirThread, (LPVOID)path, 0, 0);
	if (hThread)return TRUE;
	return FALSE;
}
M_API BOOL MFM_IsPathDir(const wchar_t* path) {
	struct _stat buf = { 0 };
	_wstat(path, &buf);
	return buf.st_mode & _S_IFDIR;
}
M_API LPWSTR MFM_GetSeledItemPath(int index)
{
	return (LPWSTR)mfmain_callback(16, (VOID*)(ULONG_PTR)index, 0);
}
M_API void MFM_GetSeledItemFree(void* v)
{
	mfmain_callback(17, v, 0);
}
M_API BOOL MFM_GetShowHiddenFiles()
{
	fmShowHiddenFile = static_cast<BOOL>((ULONG_PTR)mfmain_callback(18, 0, 0));
	return fmShowHiddenFile;
}
M_API BOOL MFM_FileExist(const wchar_t* path)
{
	if(_waccess(path, 0)==0)
		return TRUE;
	return 0;
}
M_API void MFM_SetShowHiddenFiles(BOOL b)
{
	fmShowHiddenFile = b;
}
M_API BOOL MFM_DeleteDirOrFileForce(const wchar_t* szFileDir)
{
	if (MFM_IsPathDir(szFileDir))
		return MFM_DeleteDirForce(szFileDir);
	else return MFM_DeleteFileForce(szFileDir);
}
M_API BOOL MFM_DeleteDirForce(const wchar_t* szFileDir)
{


	return 0;
}
M_API BOOL MFM_DeleteFileForce(const wchar_t* szFileDir)
{
	if (DeleteFile(szFileDir))
		return TRUE;



	return 0;
}
M_API BOOL MFM_SetFileArrtibute(const wchar_t* szFileDir, DWORD attr)
{
	DWORD old_attr = GetFileAttributes(szFileDir);
	if ((old_attr & attr) == attr)return TRUE;
	old_attr |= attr;
	return SetFileAttributes(szFileDir, old_attr);
}
M_API BOOL MFM_FillData(const wchar_t* szFileDir, BOOL force, UINT fileSize)
{
	HANDLE hFile;
	if (M_SU_CreateFile(szFileDir, GENERIC_WRITE | GENERIC_READ, 0, OPEN_EXISTING, &hFile))
	{
		LPVOID buffer = malloc(fileSize);
		memset(buffer, 0, fileSize);
		DWORD written = 0;
		SetFilePointer(hFile, 0, 0, FILE_BEGIN);
		BOOL rs = WriteFile(hFile, buffer, fileSize, &written, NULL);
		if(!rs)LogWarn(L"WriteFile failed (%d)", GetLastError());
		SetEndOfFile(hFile);
		CloseHandle(hFile);
		return rs;
	}
	else LogWarn(L"M_SU_CreateFile failed (%s)", szFileDir);
	return 0;
}
M_API BOOL MFM_EmeptyFile(const wchar_t* szFileDir, BOOL force)
{
	HANDLE hFile;
	if (M_SU_CreateFile(szFileDir, GENERIC_WRITE | GENERIC_READ, 0, OPEN_EXISTING, &hFile))
	{
		SetFilePointer(hFile, 0, 0, FILE_BEGIN);
		SetEndOfFile(hFile);
		CloseHandle(hFile);
		return TRUE;
	}
	else LogWarn(L"M_SU_CreateFile failed (%s)", szFileDir);
	return 0;
}
M_API BOOL MFM_GetFileInformationString(const wchar_t* szFile, LPWSTR strbuf, UINT bufsize)
{
	if (MFM_FileExist(szFile))
	{
		struct _stat buf = { 0 };
		if (_wstat(szFile, &buf) == 0)
		{
			wchar_t timebuf[26];
			wchar_t strbufc[64];
			swprintf_s(strbufc, L"File size : %ld\n", buf.st_size);
			wcscat_s(strbuf, bufsize, strbufc);
			swprintf_s(strbufc, L"Driver : %c:\n", buf.st_dev + 'A');
			wcscat_s(strbuf, bufsize, strbufc);
			if (!_wctime_s(timebuf, 26, &buf.st_mtime)) {
				swprintf_s(strbufc, L"Last modified time : %s\n", timebuf);
				wcscat_s(strbuf, bufsize, strbufc);
			}
			if (!_wctime_s(timebuf, 26, &buf.st_atime)) {
				swprintf_s(strbufc, L"Last access time : %s\n", timebuf);
				wcscat_s(strbuf, bufsize, strbufc);
			}
			if (!_wctime_s(timebuf, 26, &buf.st_ctime)) {
				swprintf_s(strbufc, L"Last write time : %s\n", timebuf);
				wcscat_s(strbuf, bufsize, strbufc);
			}

			return TRUE;
		}
		else LogWarn(L"_wstat failed (%s)", szFile);
	}
	else {
		swprintf_s(strbuf, bufsize, L"File not exist.");
		LogWarn(L"File not exist. (%s)", szFile);
		return TRUE;
	}
	return 0;
}
M_API BOOL MFM_ShowInExplorer(const wchar_t* szFile)
{
	std::wstring path2 = FormatString(L"/select, %s", szFile);
	return (ULONG_PTR)ShellExecute(hWndMain, L"open", L"explorer.exe", path2.c_str(), NULL, 5) >= 32;
}

DWORD WINAPI MFM_DeleteDirThread(LPVOID lpThreadParameter)
{
	needDeletedFileCount = 0;
	MAppMainCall(21, 0, 0);
	MFM_CalcFileCount((LPWSTR)lpThreadParameter);

	deletedFileCount = 0;
	MAppMainCall(18, 0, 0);
	MFM_DeleteDirInnern((LPWSTR)lpThreadParameter);
	MAppMainCall(19, 0, 0);

	free(lpThreadParameter);
	return 0;
}
BOOL MFM_DeleteDirInnern(const wchar_t* szFileDir)
{
	std::wstring strDir;
	strDir = szFileDir;
	if (strDir.at(strDir.length() - 1) != '\\')
		strDir += '\\';
	WIN32_FIND_DATA wfd;
	HANDLE hFind = FindFirstFile((strDir + L"*.*").c_str(), &wfd);
	if (hFind == INVALID_HANDLE_VALUE)
		return FALSE;
	do
	{
		WCHAR path[MAX_PATH];
		wcscpy_s(path, strDir.c_str());
		wcscat_s(path, wfd.cFileName);
		if (wfd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
		{
			if (_wcsicmp(wfd.cFileName, L".") != 0 &&
				_wcsicmp(wfd.cFileName, L"..") != 0)
				MFM_DeleteDirInnern(path);
		}
		else {

			MAppMainCall(20, (void*)path, (void*)(ULONG_PTR)((int)((double)deletedFileCount / (double)needDeletedFileCount) * 100));

			if (!DeleteFile(path)) {
				DWORD lasterr = GetLastError();
				if (lasterr == ERROR_ACCESS_DENIED)
				{
					if (MShowMessageDialog(hWndMain, path, (LPWSTR)str_item_access_denied.c_str(), str_item_delfailed, 0, MB_RETRYCANCEL) == IDRETRY)
					{
						if(!MFM_DeleteFileForce(path))
							LogWarn(L"Deleting file : %s failed, (use force delete)", path);
					}
				}
				LogWarn(L"Deleting file : %s failed, last error : %d", path, lasterr);
			}

			deletedFileCount++;
		}
	} while (FindNextFile(hFind, &wfd));
	FindClose(hFind);

	if(!RemoveDirectory(strDir.c_str()))
		LogWarn(L"Deleting directory : %s failed, last error : %d", strDir.c_str(), GetLastError());

	return TRUE;
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
	if (MChooseDir(hWndMain, NULL, (LPWSTR)str_item_choose_target_dir.c_str(), (LPWSTR*)&targetDir, sizeof(targetDir)))
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
			FileOp.lpszProgressTitle = str_item_moveing;
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
	if (MChooseDir(hWndMain, NULL, (LPWSTR)str_item_choose_target_dir.c_str(), (LPWSTR*)&targetDir, sizeof(targetDir)))
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
			FileOp.lpszProgressTitle = str_item_copying;
			return SHFileOperation(&FileOp);
		}
		else {
			std::wstring *w = Path::GetFileName((LPWSTR)fmCurrectSelectFilePath0);
			*w = targetDir + (L"\\" + *w);
			bool replace = false;
			if (MShowMessageDialog(hWndMain, (LPWSTR)w->c_str(), str_item_fileexisted_ask, str_item_question, MB_ICONEXCLAMATION, MB_YESNO) == IDOK)
				replace = true;
			BOOL rs = CopyFile(fmCurrectSelectFilePath0, w->c_str(), !replace);
			delete w;
			if (!rs)
			{
				if (GetLastError() == ERROR_ALREADY_EXISTS)
				{
					MShowErrorMessage(str_item_fileexisted, (LPWSTR)(str_item_cantcopyfile.c_str()), MB_ICONEXCLAMATION, 0);
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
		if (MShowMessageDialog(hWndMain, fmCurrectSelectFilePath0, (LPWSTR)FormatString(str_item_ask2).c_str(), str_item_delsure, MB_ICONEXCLAMATION, MB_YESNO) == IDYES)
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
			FileOp.lpszProgressTitle = str_item_deling;
			return SHFileOperation(&FileOp);
		}
		else return 1;
	}
	else {
		if (MFM_FileExist(fmCurrectSelectFilePath0)) {
			if (MShowMessageDialog(hWndMain, fmCurrectSelectFilePath0, str_item_ask1, str_item_delsure, MB_ICONEXCLAMATION, MB_YESNO) == IDYES)
			{
				SHFILEOPSTRUCT FileOp;//定义SHFILEOPSTRUCT结构对象;
				FileOp.hwnd = hWndMain;
				FileOp.wFunc = FO_DELETE; //执行文件删除操作;
				FileOp.pFrom = fmCurrectSelectFilePath0;
				FileOp.pTo = fmCurrectSelectFilePath0;
				FileOp.fFlags = FOF_ALLOWUNDO;//此标志使删除文件备份到Windows回收站
				FileOp.hNameMappings = NULL;
				FileOp.lpszProgressTitle = str_item_deling;
				return SHFileOperation(&FileOp);
			}
			else return 1;
		}
	}
	return 0;
}
BOOL MFM_DelFileForeverUser()
{
	if (fmMutilSelect) {
		if (MShowMessageDialog(hWndMain, fmCurrectSelectFilePath0, (LPWSTR)FormatString(str_item_ask2, fmMutilSelectCount).c_str(), str_item_delsure, MB_ICONEXCLAMATION, MB_YESNO) == IDYES)
		{
			bool showstatus = false;
			if (fmMutilSelectCount > 32) {
				showstatus = true;
				needDeletedFileCount = fmMutilSelectCount;
				deletedFileCount = 0;
				MAppMainCall(18, 0, 0);
			}
			LPWSTR buf;
			for (int i = 0; i < fmMutilSelectCount; i++)
			{
				if (i == 0)buf = fmCurrectSelectFilePath0;
				else buf = MFM_GetSeledItemPath(i);

				if (showstatus) MAppMainCall(20, (void*)buf, (void*)(ULONG_PTR)((int)((double)deletedFileCount / (double)needDeletedFileCount) * 100));
				if (!DeleteFile(buf)) MShowErrorMessageWithLastErr(buf, str_item_delfailed, MB_ICONEXCLAMATION, 0);

				if (i != 0)MFM_GetSeledItemFree(buf);
			}
			if (showstatus)MAppMainCall(19, 0, 0);
		}
		else return 1;
	}
	else {
		if (MFM_FileExist(fmCurrectSelectFilePath0)) {
			if (MShowMessageDialog(hWndMain, fmCurrectSelectFilePath0, str_item_delsure, str_item_ask1, MB_ICONEXCLAMATION, MB_YESNO) == IDYES)
				if (!MFM_DeleteDirOrFile(fmCurrectSelectFilePath0))
					MShowErrorMessageWithLastErr(fmCurrectSelectFilePath0, str_item_delfailed, MB_ICONEXCLAMATION, 0);
		}
	}
	return 1;
}
void MFF_ShowFolderProp() {
	if (MFM_FileExist(fmCurrectSelectFolderPath0))
		MShowFileProp(fmCurrectSelectFolderPath0);
}
void MFF_ShowInExplorer() {
	if (MFM_FileExist(fmCurrectSelectFolderPath0))
		ShellExecute(hWndMain, L"open", fmCurrectSelectFolderPath0, 0, 0, 5);
}
BOOL MFF_DelToRecBin() {
	if (MFM_FileExist(fmCurrectSelectFolderPath0))
	{
		if (MShowMessageDialog(hWndMain, fmCurrectSelectFilePath0, str_item_ask3, str_item_delsure,  MB_ICONEXCLAMATION, MB_YESNO) == IDYES)
		{
			SHFILEOPSTRUCT FileOp;//定义SHFILEOPSTRUCT结构对象;
			FileOp.hwnd = hWndMain;
			FileOp.wFunc = FO_DELETE; //执行文件删除操作;
			FileOp.pFrom = fmCurrectSelectFolderPath0;
			FileOp.pTo = fmCurrectSelectFolderPath0;
			FileOp.fFlags = FOF_ALLOWUNDO;//此标志使删除文件备份到Windows回收站
			FileOp.hNameMappings = NULL;
			FileOp.lpszProgressTitle = str_item_deling;
			return SHFileOperation(&FileOp);
		}
		else return 1;
	}
	return 0;
}
BOOL MFF_DelForever() {
	if (MFM_FileExist(fmCurrectSelectFolderPath0))
	{
		if (MShowMessageDialog(hWndMain, fmCurrectSelectFilePath0, str_item_delsure, str_item_ask3, MB_ICONEXCLAMATION, MB_YESNO) == IDYES)
		{
			/*SHFILEOPSTRUCT FileOp;//定义SHFILEOPSTRUCT结构对象;
			FileOp.hwnd = hWndMain;
			FileOp.wFunc = FO_DELETE; //执行文件删除操作;
			FileOp.pFrom = fmCurrectSelectFolderPath0;
			FileOp.pTo = 0;
			FileOp.hNameMappings = NULL;
			FileOp.lpszProgressTitle = str_item_deling;
			return SHFileOperation(&FileOp);*/
			return MFM_DeleteDir(fmCurrectSelectFolderPath0);
		}
		else return 1;
	}	
	return 0;
}
BOOL MFF_ForceDel()
{
	if (MFM_FileExist(fmCurrectSelectFolderPath0))
	{
		if (MShowMessageDialog(hWndMain, fmCurrectSelectFilePath0, str_item_ask3, str_item_delsure, MB_ICONEXCLAMATION, MB_YESNO) == IDYES)
			return MFM_DeleteDirForce(fmCurrectSelectFolderPath0);
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
	HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUFMMAIN));
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
	if (!MStrEqualW(fmCurrectSelectFolderPath0, L"\\\\") && !MStrEqualW(fmCurrectSelectFolderPath0, L"mycp")) {
		HMENU hroot = LoadMenu(hInstRs, MAKEINTRESOURCE(IDR_MENUFMFOLDER));
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
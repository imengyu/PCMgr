#include "stdafx.h"
#include "mapphlp.h"
#include "cmdhlp.h"
#include "comdlghlp.h"
#include "loghlp.h"
#include "prochlp.h"
#include "thdhlp.h"
#include "loghlp.h"
#include "pehlp.h"
#include "syshlp.h"
#include "suact.h"
#include "nthlp.h"
#include "fmhlp.h"
#include "StringHlp.h"
#include "StringSplit.h"

#include <shellapi.h>
#include <locale>

using namespace std;

extern HINSTANCE hInst;
extern HINSTANCE hLoader;

BOOL cmdThreadCanRun = FALSE;
BOOL cmdThreadRunning = FALSE;
HANDLE hCmdThread = NULL;

DWORD WINAPI MConsoleThread(LPVOID lpParameter)
{
	Sleep(1000);
	return MAppCmdRunner(TRUE);
}

#define CMD_CASE(cmd, go) else if ((*cmds)[0] == cmd) go(cmds, size)
#define CMD_CASE_CAN_EXIT(cmd, go) else if ((*cmds)[0] == cmd) {if(go(cmds, size))return true;}

bool MRunCmd(vector<string> * cmds);

vector<string> *MAppConsoleInitCommandLine() 
{
	int argc = 0;
	LPWSTR*argsStrs = CommandLineToArgvW(GetCommandLineW(), &argc);
	if (argsStrs) {
		if (argc > 1) {
			vector<string> *cmdArray = new vector<string>();
			for (int i = 1; i < argc; i++)
			{
				LPCSTR str = W2A(argsStrs[i]);
				cmdArray->push_back(string(str));
				delete str;
			}
			return cmdArray;
		}
		LocalFree(argsStrs);
		return nullptr;
	}
	return nullptr;
}
M_CAPI(int) MAppConsoleInit()
{
	setlocale(LC_ALL, "chs");
	cmdThreadCanRun = TRUE;
	M_LOG_Init_InConsole();

	printf_s("PCMgr Command Line Tool\n");
	printf_s("Version : 1.0.0.1\n\n");
	MGetWindowsBulidVersion();
	printf_s("\n");

	vector<string> * cmds = MAppConsoleInitCommandLine();
	if (cmds) MRunCmd(cmds);
	delete(cmds);

	int rs = MAppCmdRunner(FALSE);
	M_LOG_Close_InConsole();
	ExitProcess(rs);
	return rs;
}

int MAppCmdRunner(BOOL isMain) 
{
REENTER:
	printf_s("\n>");

	char maxbuf[260];
	gets_s(maxbuf);

	char *buf = maxbuf;
	if (maxbuf[0] == '>')
		buf = maxbuf + 1;

	if (MRunCmdWithString(maxbuf))
	{
		if(isMain) MAppMainThreadCall(M_MTMSG_COSCLOSE, 0);
		return 0;
	}

	if (cmdThreadCanRun)
		goto REENTER;

	return 0;
}

BOOL MStartRunCmdThread()
{
	if (!cmdThreadRunning)
	{
		cmdThreadCanRun = TRUE;
		hCmdThread = CreateThread(NULL, NULL, MConsoleThread, NULL, NULL, NULL);
		cmdThreadRunning = TRUE;
		return cmdThreadRunning;
	}
	return FALSE;
}
BOOL MStopRunCmdThread()
{
	if (cmdThreadRunning)
	{
		cmdThreadCanRun = FALSE;
		if (hCmdThread)
		{
			DWORD dw = WaitForSingleObject(hCmdThread, 100);
			if (dw == WAIT_TIMEOUT) {
				if (NT_SUCCESS(MTerminateThreadNt(hCmdThread)))
					LogInfo(L"RunCmdThread Terminated.");
				else LogWarn(L"RunCmdThread Terminate failed!");
			}
			if (hCmdThread) { CloseHandle(hCmdThread); hCmdThread = 0; }
			cmdThreadRunning = FALSE;
			return 1;
		}
		cmdThreadRunning = FALSE;
	}
	return FALSE;
}

int MPrintMumberWithLen(DWORD n, size_t len)
{
	char munbuf[16];
	sprintf_s(munbuf, "%d", n);
	size_t munstrlen = strlen(munbuf);
	if (munstrlen < len)
	{
		size_t outlen = len - munstrlen;
		for (size_t i = 0; i < outlen; i++)putchar(' ');
		for (size_t i = 0; i < 16 && i + outlen < len; i++)
			putchar(munbuf[i]);
		return static_cast<int>(outlen);
	}
	else printf(munbuf);
	return static_cast<int>(len);
}
int MPrintStrWithLenW(LPWSTR s, size_t len)
{
	if (s != NULL) {
		wprintf_s(L"%s", s);
		size_t slen = wcslen(s);
		if (slen > 0)
		{
			if (len > (size_t)slen) {
				size_t soutlen = len - (size_t)slen;
				for (size_t i = 0; i < soutlen; i++)
					putchar(' ');
			}
			return static_cast<int>(len - slen);
		}
	}
	return 0;
}
int MPrintStrWithLenA(LPCSTR s, size_t len)
{
	if (s != NULL) {
		printf_s("%s", s);
		size_t slen = strlen(s);
		if (slen > 0)
		{
			if (len > (size_t)slen) {
				size_t soutlen = len - (size_t)slen;
				for (size_t i = 0; i < soutlen; i++)
					putchar(' ');
				return static_cast<int>(len - slen);
			}
			return 0;
		}
	}
	return 0;
}
void MPrintSuccess() {
	printf("Success.\n");
}

void __cdecl MEnumProcessCallBack(DWORD pid, DWORD parentid, LPWSTR exename, LPWSTR exefullpath, int tp, HANDLE hProcess)
{
	if (tp) 
	{
		MPrintMumberWithLen(pid, 5);
		printf(" ");
		MPrintMumberWithLen(parentid, 5);
		printf("        ");//6
		MPrintStrWithLenW(exename, 32);
		wprintf_s(L"   %s\n", exefullpath);
	}
}

void MRunCmd_RunAsProgram(vector<string>* cmds, int size) {
	BOOL fileexists = TRUE;
	LPWSTR wagrs = NULL;
	string parms;
	for (int i = 1; i < size; i++) {
		if (i != 1)parms += " ";
		parms += (*cmds)[i];
	}
	if (parms != "") wagrs = MConvertLPCSTRToLPWSTR((LPCSTR)parms.c_str());

	WCHAR targetPath[MAX_PATH];
	LPWSTR wmaxpath = MConvertLPCSTRToLPWSTR((LPCSTR)("%SystemRoot%\\system32\\" + (*cmds)[0]).c_str());
	if (ExpandEnvironmentStrings(wmaxpath, targetPath, MAX_PATH))
	{
		if (PathIsExe(targetPath))
			MFM_RunExe(wmaxpath, wagrs, GetConsoleWindow());
		else
		{
			LPWSTR wmaxpath2 = MConvertLPCSTRToLPWSTR((LPCSTR)("%SystemRoot%\\system32\\" + (*cmds)[0] + ".exe").c_str());
			if (ExpandEnvironmentStrings(wmaxpath2, targetPath, MAX_PATH))
			{
				if (MFM_FileExist(targetPath))
					MFM_RunExe(targetPath, wagrs, GetConsoleWindow());
				else  fileexists = FALSE;		
			}
			MConvertStrDel(wmaxpath2);

			if (!fileexists) {
				LPWSTR wmaxpath3 = MConvertLPCSTRToLPWSTR((LPCSTR)("%Path%\\" + (*cmds)[0]).c_str());
				if (ExpandEnvironmentStrings(wmaxpath3, targetPath, MAX_PATH))
				{
					if (MFM_FileExist(targetPath))
						MFM_RunExe(targetPath, wagrs, GetConsoleWindow());
					else  fileexists = FALSE;
				}
				MConvertStrDel(wmaxpath3);
			}

			if (!fileexists) {
				LPWSTR wmaxpath4 = MConvertLPCSTRToLPWSTR((LPCSTR)("%Path%\\" + (*cmds)[0] + ".exe").c_str());
				if (ExpandEnvironmentStrings(wmaxpath4, targetPath, MAX_PATH))
				{
					if (MFM_FileExist(targetPath))
						MFM_RunExe(targetPath, wagrs, GetConsoleWindow());
					else fileexists = FALSE;
				}
				MConvertStrDel(wmaxpath4);
			}
		}
	}
	else fileexists = FALSE;
	MConvertStrDel(wmaxpath);
	if (wagrs) MConvertStrDel(wagrs);

	if (!fileexists) printf("Unknow cmd : %s\n", (*cmds)[0].c_str());
}
void MRunCmd_TaskList(vector<string>* cmds, int size)
{
	wprintf_s(L"PID     ParentPID ProcessName                          FullPath\n");
	MEnumProcess((EnumProcessCallBack)MEnumProcessCallBack, NULL);
}
void MRunCmd_Help(vector<string>* cmds, int size) {
	printf("Help : \n");
	printf("    tasklist : list all running process\n");
	printf("    taskkill pid [force] [useApc] : kill a running process use process id\n            force : Want to use kernel force kill process\n            useApc : When force kill , should use APC to terminate threads\n");
	printf("    tasksuspend pid [force] : suspend process use process id\n            force : Want to use kernel force suspend process\n");
	printf("    taskresume pid [force] : resume process use process id\n            force : Want to use kernel force resume process\n");
	printf("    toadmin : run pcmgr as adminstrator\n");

}
void MRunCmd_TaskKill(vector<string>* cmds, int size)
{
	if (size < 2) { printf("Please enter pid.\n"); return; }
	DWORD pid = static_cast<DWORD>(atoll((*cmds)[1].c_str()));
	if (pid > 4) {
		if (size > 2 && (*cmds)[2] == "force")
		{
			BOOL useApc = FALSE;
			if (size > 3 && (*cmds)[3] == "apc")useApc = TRUE;
			NTSTATUS status = 0;
			if (M_SU_TerminateProcessPID(pid, 0, &status, useApc) && NT_SUCCESS(status))
				MPrintSuccess();
			else wprintf(L"TerminateProcess Failed %s\n", MNtStatusToStr(status));
		}
		else
		{
			HANDLE hProcess;
			NTSTATUS status = MOpenProcessNt(pid, &hProcess);
			if (status == STATUS_SUCCESS)
			{
				status = MTerminateProcessNt(0, hProcess);
				if (NT_SUCCESS(status)) printf("Success.\n");
				else wprintf(L"TerminateProcess Failed %s\n", MNtStatusToStr(status));
			}
			else wprintf(L"TerminateProcess Failed %s\n", MNtStatusToStr(status));
		}
	}
	else printf("Invalid pid.\n");
}
void MRunCmd_ThreadKill(vector<string>* cmds, int size)
{
	if (size < 2) { printf("Please enter tid.\n"); return; }
	DWORD tid = static_cast<DWORD>(atoll((*cmds)[1].c_str()));
	NTSTATUS status = 0;
	if (size > 2 && (*cmds)[2] == "force")
	{
		BOOL useApc = FALSE;
		if (size > 3 && (*cmds)[3] == "apc")useApc = TRUE;
		if (!(M_SU_TerminateThreadTID(tid, 0, &status, useApc) && NT_SUCCESS(status)))
			wprintf(L"TerminateThread Failed %s\n", MNtStatusToStr(status));
	}
	else {
		HANDLE hThread;
		DWORD NtStatus = MOpenThreadNt(tid, &hThread, tid);
		if (NT_SUCCESS(status)) {
			NtStatus = MTerminateThreadNt(hThread);
			if (NtStatus == STATUS_SUCCESS)
				printf("Success.\n");
			else wprintf(L"TerminateThread Failed %s\n", MNtStatusToStr(status));
		}
		else wprintf(L"Failed : OpenThread : %s\n", MNtStatusToStr(status));
	}
}
void MRunCmd_TaskSuspend(vector<string>* cmds, int size)
{
	if (size < 2) { printf("Please enter pid.\n"); return; }
	DWORD pid = static_cast<DWORD>(atoll((*cmds)[1].c_str()));
	if (pid > 4) {
		NTSTATUS status = MSuspendProcessNt(pid, 0);
		if (status == STATUS_SUCCESS)
			MPrintSuccess();
		else wprintf(L"Failed : SuspendProcess : %s\n", MNtStatusToStr(status));
	}
	else printf("Invalid pid.\n");
}
void MRunCmd_TaskResume(vector<string>* cmds, int size)
{
	if (size < 2) { printf("Please enter pid.\n"); return; }
	DWORD pid = static_cast<DWORD>(atoll((*cmds)[1].c_str()));
	if (pid > 4) {
		NTSTATUS status = MResumeProcessNt(pid, 0);
		if (status == STATUS_SUCCESS)
			MPrintSuccess();
		else wprintf(L"Failed : SuspendProcess :%s\n", MNtStatusToStr(status));
	}
	else printf("Invalid pid.\n");
}
void MRunCmd_VExp(vector<string>* cmds, int size)
{
	WCHAR fileName[MAX_PATH];
	if (size >= 2)
	{
		LPWSTR str = A2W((*cmds)[1].c_str());
		wcscpy_s(fileName, str);
		delete(str);
	}
	if (size < 2 && !MChooseFileSingal(GetConsoleWindow(), NULL, L"Choose a PE File", L"PE文件\0*.exe;*.dll\0所有文件\0*.*\0", NULL, L".exe", fileName, MAX_PATH))
	{
		printf("Please enter file name.\n"); 
		return;
	}

	wprintf(fileName);
	printf("\n");

	HMODULE hModule = LoadLibrary(fileName);
	if (!hModule || hModule == INVALID_HANDLE_VALUE)
	{
		wprintf_s(L"LoadLibrary failed : %d\n", GetLastError());
		return;
	}

	PIMAGE_DOS_HEADER pDosHeader = (PIMAGE_DOS_HEADER)hModule;
	PIMAGE_NT_HEADERS pNtHeader = (PIMAGE_NT_HEADERS)((ULONG_PTR)hModule + pDosHeader->e_lfanew);
	PIMAGE_OPTIONAL_HEADER pOptionalHeader = (PIMAGE_OPTIONAL_HEADER)((PBYTE)hModule + pDosHeader->e_lfanew + offsetof(IMAGE_NT_HEADERS, OptionalHeader));
	PIMAGE_EXPORT_DIRECTORY pExportDirectory = (PIMAGE_EXPORT_DIRECTORY)((PBYTE)hModule + pOptionalHeader->DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress);
	PDWORD aryAddressOfNames = (PDWORD)((PBYTE)hModule + pExportDirectory->AddressOfNames);
	DWORD dwNumberOfNames = pExportDirectory->NumberOfNames;
	if (dwNumberOfNames == 0)wprintf_s(L"This PE File has not export table.\n");
	else {
		for (UINT i = 0; i < dwNumberOfNames; i++)
		{
			char *strFunction = (char *)(aryAddressOfNames[i] + (ULONG_PTR)hModule);
			wprintf(L"\n#"); MPrintMumberWithLen(i, 3);
			printf("    ");
			printf(strFunction);
		}
	}
	printf("\n");
UNMAP_AND_EXIT:
	{
		if (hModule != hInst && hModule != hLoader)
			FreeLibrary(hModule);
	}
}
void MRunCmd_VImp(vector<string>* cmds, int size)
{
	WCHAR fileName[MAX_PATH];
	if (size >= 2)
	{
		LPWSTR str = A2W((*cmds)[1].c_str());
		wcscpy_s(fileName, str);
		delete(str);
	}
	if (size < 2 && !MChooseFileSingal(GetConsoleWindow(), NULL, L"Choose a PE File", L"PE文件\0*.exe;*.dll\0所有文件\0*.*\0", NULL, L".exe", fileName, MAX_PATH))
	{
		printf("Please enter file name.\n"); return;
	}
	
	wprintf(fileName);
	printf("\n");

	HANDLE hFile = CreateFile(fileName, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hFile == INVALID_HANDLE_VALUE)
	{
		wprintf_s(L"Create file failed : %d", GetLastError());
		return;
	}

	HANDLE hFileMapping = CreateFileMapping(hFile, NULL, PAGE_READONLY, 0, 0, NULL);
	if (hFileMapping == NULL || hFileMapping == INVALID_HANDLE_VALUE)
	{
		wprintf_s(L"Could not create file mapping object (%d)", GetLastError());
		CloseHandle(hFile);
		return;
	}

	LPBYTE lpBaseAddress = (LPBYTE)MapViewOfFile(hFileMapping, FILE_MAP_READ, 0, 0, 0);
	if (lpBaseAddress == NULL)
	{
		wprintf_s(L"Could not map view of file (%d)", GetLastError());
		CloseHandle(hFileMapping);
		CloseHandle(hFile);
		return;
	}

	PIMAGE_DOS_HEADER pDosHeader = (PIMAGE_DOS_HEADER)lpBaseAddress;
	PIMAGE_NT_HEADERS pNtHeaders = (PIMAGE_NT_HEADERS)(lpBaseAddress + pDosHeader->e_lfanew);

	DWORD Rva_import_table = pNtHeaders->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress;
	if (Rva_import_table == 0)
	{
		wprintf(L"No import table!");
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
				wprintf(L"\n#"); MPrintMumberWithLen(j, 3);
				printf("    ");
				MPrintStrWithLenA(szDllName, 26);

				wprintf_s(L"    %ld", pThunk[j].u1.AddressOfData & 0xffff);
			}
			else
			{
				PIMAGE_IMPORT_BY_NAME pFuncName = (PIMAGE_IMPORT_BY_NAME)ImageRvaToVa(pNtHeaders, lpBaseAddress, pThunk[j].u1.AddressOfData, NULL);

				wprintf(L"\n#"); MPrintMumberWithLen(j, 3);
				printf("    ");
				MPrintStrWithLenA(szDllName, 26);
				printf("    %s (%ld)", pFuncName->Name, pFuncName->Hint);
			}
		}
	}
	printf("\n");
UNMAP_AND_EXIT:
	UnmapViewOfFile(lpBaseAddress);
	CloseHandle(hFileMapping);
	CloseHandle(hFile);
}


bool MRunCmdWithString(char*maxbuf) {
	string cmd(maxbuf);
	vector<string> cmds;
	SplitString(cmd, cmds, " ");
	return MRunCmd(&cmds);
}
bool MRunCmd(vector<string> * cmds)
{
	int size = static_cast<int>(cmds->size());
	if (size >= 1)
	{
		if ((*cmds)[0] == "exit") return true;
		else if ((*cmds)[0] == "cls") system("cls");
		CMD_CASE("help", MRunCmd_Help);
		CMD_CASE("?", MRunCmd_Help);
		CMD_CASE("tasklist", MRunCmd_TaskList);
		CMD_CASE("taskkill", MRunCmd_TaskKill);
		CMD_CASE("tasksuspend", MRunCmd_TaskSuspend);
		CMD_CASE("taskresume", MRunCmd_TaskResume);
		CMD_CASE("vexp", MRunCmd_VExp);
		CMD_CASE("vimp", MRunCmd_VImp);
		else MRunCmd_RunAsProgram(cmds, size);
	}
	return false;
}


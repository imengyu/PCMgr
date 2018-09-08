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
#include "msup.h"
#include "StringHlp.h"
#include "StringSplit.h"

#include <shellapi.h>
#include <locale>
#include <list>

#include "..\PCMgrCmdRunner\PCMgrCmdRunnerEntry.h"


extern HINSTANCE hInst;
extern HINSTANCE hLoader;

typedef struct tag_COMMAND {
	string cmd;
	MCMD_HANDLER handler;
	MCMD_HANDLER_NO_PARARM handler_no_pararm;
	MCMD_HANDLER_NO_RETURN handler_no_return;
	bool hasPararm;
	bool hasReturn;
}COMMAND,*PCOMMAND;

list<PCOMMAND> *allCommands;
MCmdRunner *staticCmdRunner;

M_CAPI(MCmdRunner*) MGetStaticCmdRunner() {
	return staticCmdRunner;
}

MCmdRunner::MCmdRunner()
{
	allCommands = new list<PCOMMAND>();
}
MCmdRunner::~MCmdRunner()
{
	for each (PCOMMAND var in *allCommands)
		free(var);
	allCommands->clear();
	delete allCommands;
}

bool IsCommandRegistered(const char * cmd, PCOMMAND * outCmd)
{
	for each (PCOMMAND var in (*allCommands))
	{
		if (var->cmd == cmd) {
			if (outCmd)*outCmd = var;
			return true;
		}
	}
	return NULL;
}
bool CallCommand(PCOMMAND cmd, vector<string>* cmds, int size)
{
	if (cmd->hasPararm) {
		if (cmd->hasReturn)
			return cmd->handler(cmds, size);
		else {
			cmd->handler_no_return(cmds, size);
			return  false;
		}
	}
	else return cmd->handler_no_pararm();
}

bool MCmdRunner::RegisterCommand(const char * cmd, MCMD_HANDLER handler)
{
	if (IsCommandRegistered(cmd) == NULL) {
		PCOMMAND cmdItem = new COMMAND();
		cmdItem->hasPararm = true;
		cmdItem->hasReturn = true;
		cmdItem->handler_no_pararm = NULL;
		cmdItem->handler_no_return = NULL;
		cmdItem->handler = handler;
		cmdItem->cmd = cmd;
		allCommands->push_back(cmdItem);
	}
	return false;
}
bool MCmdRunner::RegisterCommandNoParam(const char * cmd, MCMD_HANDLER_NO_PARARM handler)
{
	if (IsCommandRegistered(cmd) == NULL) {
		PCOMMAND cmdItem = new COMMAND();
		cmdItem->hasPararm = false;
		cmdItem->hasReturn = true;
		cmdItem->handler_no_pararm = handler;
		cmdItem->handler_no_return = NULL;
		cmdItem->handler = NULL;
		cmdItem->cmd = cmd;
		allCommands->push_back(cmdItem);
	}
	return false;
}
bool MCmdRunner::RegisterCommandNoReturn(const char * cmd, MCMD_HANDLER_NO_RETURN handler)
{
	if (IsCommandRegistered(cmd) == NULL) {
		PCOMMAND cmdItem = new COMMAND();
		cmdItem->hasPararm = true;
		cmdItem->hasReturn = false;
		cmdItem->handler_no_pararm = NULL;
		cmdItem->handler = NULL;
		cmdItem->handler_no_return = handler;
		cmdItem->cmd = cmd;
		allCommands->push_back(cmdItem);
	}
	return false;
}
bool MCmdRunner::UnRegisterCommand(const char * cmd)
{
	PCOMMAND cmdItem = 0;
	if (::IsCommandRegistered(cmd, &cmdItem) != NULL) {
		free(cmdItem);
		allCommands->remove(cmdItem);
	}
	return false;
}
bool MCmdRunner::IsCommandRegistered(const char * cmd)
{
	PCOMMAND cmdItem = 0;
	return ::IsCommandRegistered(cmd, &cmdItem);
}

#define CMD_CASE_NOARG(cmd, go) else if ((*cmds)[0] == cmd) go()
#define CMD_CASE(cmd, go) else if ((*cmds)[0] == cmd) go(cmds, size)
#define CMD_CASE_CAN_EXIT(cmd, go) else if ((*cmds)[0] == cmd) {if(go(cmds, size))return true;}

M_CAPI(int) MAppConsoleInit()
{
	return MAppCmdStart();
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

void MRunCmd_VExp(vector<string>* cmds, int size)
{
	WCHAR fileName[MAX_PATH];
	if (size >= 2)
	{
		LPWSTR str = A2W((*cmds)[1].c_str());
		wcscpy_s(fileName, str);
		delete(str);
	}
	if (size < 2 && !M_DLG_ChooseFileSingal(GetConsoleWindow(), NULL, L"Choose a PE File", L"PE文件\0*.exe;*.dll\0所有文件\0*.*\0", NULL, L".exe", fileName, MAX_PATH))
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
	//UNMAP_AND_EXIT:
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
	if (size < 2 && !M_DLG_ChooseFileSingal(GetConsoleWindow(), NULL, L"Choose a PE File", L"PE文件\0*.exe;*.dll\0所有文件\0*.*\0", NULL, L".exe", fileName, MAX_PATH))
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
void MRunCmd_FGc() {
	MForceGC();
	MPrintSuccess();
}

bool MCmdRunner::MRunCmdWithString(char*maxbuf) {
	string cmd(maxbuf);
	vector<string> cmds;
	SplitString(cmd, cmds, " ");
	return MRunCmd(&cmds, maxbuf);
}
bool MCmdRunner::MRunCmd(vector<string> * cmds, LPCSTR oldCmd)
{
	PCOMMAND registeredCmd = NULL;
	//PCMgrCmd32
	int size = static_cast<int>(cmds->size());
	if (size >= 1)
	{
		if ((*cmds)[0] == "exit") return true;
		else if ((*cmds)[0] == "cls") system("cls");
		CMD_CASE("vexp", MRunCmd_VExp);
		CMD_CASE("vimp", MRunCmd_VImp);
		CMD_CASE_NOARG("vstat", MShowProgramStats);
		CMD_CASE_NOARG("gc", MRunCmd_FGc);
		else if (::IsCommandRegistered((*cmds)[0].c_str(), &registeredCmd))
			return CallCommand(registeredCmd, cmds, size);
		else system(oldCmd);
	}
	return false;
}


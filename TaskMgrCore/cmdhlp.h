#pragma once
#include "stdafx.h"
#include <string>
#include <vector>

using namespace std;

typedef void(_cdecl* MCMD_HANDLER_NO_RETURN)(vector<string>* cmds, int size);
typedef bool(_cdecl* MCMD_HANDLER)(vector<string>* cmds, int size);
typedef bool(_cdecl* MCMD_HANDLER_NO_PARARM)();

M_CAPI(int) MAppConsoleInit();

M_CAPI(int) MPrintMumberWithLen(DWORD n, size_t len);
M_CAPI(int) MPrintStrWithLenW(LPWSTR s, size_t len);
M_CAPI(int) MPrintStrWithLenA(LPCSTR s, size_t len);
M_CAPI(void) MPrintSuccess();

class M_API MCmdRunner
{
public:
	MCmdRunner();
	~MCmdRunner();

	bool RegisterCommand(const char*cmd, MCMD_HANDLER handler);
	bool RegisterCommandNoParam(const char*cmd, MCMD_HANDLER_NO_PARARM handler);
	bool RegisterCommandNoReturn(const char*cmd, MCMD_HANDLER_NO_RETURN handler);
	
	bool UnRegisterCommand(const char*cmd);
	bool IsCommandRegistered(const char*cmd);

	bool MRunCmdWithString(char*maxbuf);
	bool MRunCmd(vector<string> * cmds, LPCSTR oldCmd);
private:

};

M_CAPI(MCmdRunner*) MGetStaticCmdRunner();

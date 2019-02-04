#include "stdafx.h"
#include <string>
#include <vector>
#include <Psapi.h>
#include <TlHelp32.h>

using namespace std;

typedef void (*COS_FOR_ALLOC_EXIT_CALLBACK)();

#define M_CMD_HANDLER(name)void name(vector<string>* cmds, int size)
#define M_CMD_HANDLER_WITH_RTN(name,rtn) rtn name(vector<string>* cmds, int size)

CMD_CAPI(VOID) MInitAllCmd();
CMD_CAPI(BOOL) MStartRunCmdThread(COS_FOR_ALLOC_EXIT_CALLBACK callback);
CMD_CAPI(BOOL) MStopRunCmdThread();
CMD_CAPI(int) MAppCmdStart();
CMD_CAPI(BOOL) MAppCmdCanRun();
CMD_CAPI(BOOL) MAppCmdRunOne(BOOL isMain, char* cmd);
CMD_CAPI(VOID) MAppCmdOnExit();

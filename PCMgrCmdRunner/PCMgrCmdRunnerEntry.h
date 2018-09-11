#include "stdafx.h"
#include <string>
#include <vector>
#include <Psapi.h>
#include <TlHelp32.h>

using namespace std;

#define M_CMD_HANDLER(name)void name(vector<string>* cmds, int size)
#define M_CMD_HANDLER_WITH_RTN(name,rtn) rtn name(vector<string>* cmds, int size)

CMD_CAPI(VOID) MInitAllCmd();
CMD_CAPI(BOOL) MStartRunCmdThread();
CMD_CAPI(BOOL) MStopRunCmdThread();
CMD_CAPI(int) MAppCmdStart();

M_CMD_HANDLER(MRunCmd_TaskList);
M_CMD_HANDLER(MRunCmd_ScList);
M_CMD_HANDLER(MRunCmd_ScStop);
M_CMD_HANDLER(MRunCmd_ScStart);
M_CMD_HANDLER(MRunCmd_ScPause);
M_CMD_HANDLER(MRunCmd_ScCon);
M_CMD_HANDLER(MRunCmd_Help);
M_CMD_HANDLER(MRunCmd_TaskKill);
M_CMD_HANDLER(MRunCmd_TaskKillName);
M_CMD_HANDLER(MRunCmd_ThreadKill);
M_CMD_HANDLER(MRunCmd_TaskSuspend);
M_CMD_HANDLER(MRunCmd_TaskResume);
M_CMD_HANDLER(MRunCmd_RunUWP);
M_CMD_HANDLER(MRunCmd_StopUWP);
M_CMD_HANDLER(MRunCmd_Vsign);
M_CMD_HANDLER(MRunCmd_DeatchDebugger);
M_CMD_HANDLER(MRunCmd_CreateMiniDump);
M_CMD_HANDLER(MRunCmd_VModule);
M_CMD_HANDLER(MRunCmd_VThread);
M_CMD_HANDLER(MRunCmd_LoadDrv);
M_CMD_HANDLER(MRunCmd_UnLoadDrv);
M_CMD_HANDLER(MRunCmd_Su);
M_CMD_HANDLER(MRunCmd_QSu);
M_CMD_HANDLER(MRunCmd_Test);
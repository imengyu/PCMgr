#include "stdafx.h"
#include <string>
#include <vector>

using namespace std;

#define M_CMD_HANDLER(name)void name(vector<string>* cmds, int size)
#define M_CMD_HANDLER_WITH_RTN(name,rtn) rtn name(vector<string>* cmds, int size)

CMD_CAPI(VOID) MInitAllCmd();
CMD_CAPI(BOOL) MStartRunCmdThread();
CMD_CAPI(BOOL) MStopRunCmdThread();
CMD_CAPI(int) MAppCmdStart();

M_CMD_HANDLER(MRunCmd_TaskList);
M_CMD_HANDLER(MRunCmd_Help);
M_CMD_HANDLER(MRunCmd_TaskKill);
M_CMD_HANDLER(MRunCmd_ThreadKill);
M_CMD_HANDLER(MRunCmd_TaskSuspend);
M_CMD_HANDLER(MRunCmd_TaskResume);
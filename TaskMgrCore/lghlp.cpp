#include "stdafx.h"
#include "lghlp.h"
#include "syshlp.h"
#include "mapphlp.h"
#include <string>
#include "StringHlp.h"

typedef LPWSTR(*LanuageItems_CallBack)(LPWSTR name);

std::wstring str_item_kill_ask_start;
std::wstring str_item_kill_ask_end;
std::wstring str_item_kill_ast_content;
std::wstring str_item_kill_failed;
std::wstring str_item_access_denied;
std::wstring str_item_op_failed;
std::wstring str_item_invalidproc;
std::wstring str_item_cantcopyfile;
std::wstring str_item_cantmovefile;
std::wstring str_item_choose_target_dir;

void MLG_Startup() {
	str_item_access_denied = L"拒绝访问";
	str_item_invalidproc = L"无效进程";
	str_item_op_failed = L"操作失败";
}
void MLG_SetLanuageItems_0(int id, LPWSTR msg, int size)
{
	switch (id)
	{
	case 0:str_item_kill_ask_start = msg; break;
	case 1:str_item_kill_ask_end = msg; break;
	case 2:str_item_kill_ast_content = msg; break;
	case 3:str_item_kill_failed = msg; break;
	case 4:str_item_access_denied = msg; break;
	case 5:str_item_op_failed = msg; break;
	case 6:str_item_invalidproc = msg; break;
	case 7:str_item_cantcopyfile = msg; break;
	case 8:str_item_cantmovefile = msg; break;
	case 9:str_item_choose_target_dir = msg; break;
	}
}

LanuageItems_CallBack callBack;

LPWSTR str_item_copying;
LPWSTR str_item_moveing;
LPWSTR str_item_fileexisted;
LPWSTR str_item_fileexisted_ask;
LPWSTR str_item_question;
LPWSTR str_item_tip;
LPWSTR str_item_delsure;
LPWSTR str_item_ask1;
LPWSTR str_item_ask2;
LPWSTR str_item_ask3;
LPWSTR str_item_deling;
LPWSTR str_item_noadmin1;
LPWSTR str_item_noadmin2;
LPWSTR str_item_delfailed;
LPWSTR str_item_systemidleproc = L"Idle Process";
LPWSTR str_item_endprocfailed;
LPWSTR str_item_openprocfailed = L"打开进程失败";
LPWSTR str_item_susprocfailed;
LPWSTR str_item_resprocfailed;
LPWSTR str_item_rebootasadmin;
LPWSTR str_item_visible;
LPWSTR str_item_cantgetpath;
LPWSTR str_item_freesuccess;
LPWSTR str_item_proerty;
LPWSTR str_item_entrypoint;
LPWSTR str_item_modulename = L"模块名称";
LPWSTR str_item_state;
LPWSTR str_item_contextswitch;
LPWSTR str_item_modulepath = L"模块路径";
LPWSTR str_item_address = L"基地址";
LPWSTR str_item_size = L"大小";
LPWSTR str_item_publisher = L"发布者";
LPWSTR str_item_windowtext;
LPWSTR str_item_windowhandle;
LPWSTR str_item_wndclass;
LPWSTR str_item_wndbthread;
LPWSTR str_item_vwinstitle;
LPWSTR str_item_vmodulestitle = L"进程 %s [%d] 的所有模块：%d";
LPWSTR str_item_vthreadtitle;
LPWSTR str_item_enum_modulefailed = L"枚举模块失败";
LPWSTR str_item_enum_threadfailed;
LPWSTR str_item_freeinvproc;
LPWSTR str_item_freefailed;
LPWSTR str_item_killthreaderr;
LPWSTR str_item_killinvthread;
LPWSTR str_item_openthreaderr;
LPWSTR str_item_suthreaderr;
LPWSTR str_item_rethreaderr;
LPWSTR str_item_invthread;
LPWSTR str_item_suthreadwarn;
LPWSTR str_item_kernelnotload;

LPWSTR str_item_delscask;
LPWSTR str_item_delsc2ask;
LPWSTR str_item_endtask;
LPWSTR str_item_rebootexplorer;
LPWSTR str_item_loaddriver;
LPWSTR str_item_unloaddriver;
LPWSTR str_item_filenotexist;
LPWSTR str_item_filetrusted;
LPWSTR str_item_filenottrust;
LPWSTR str_item_opensc_err;
LPWSTR str_item_delsc_err;
LPWSTR str_item_setscstart_err;

#define HASSTR(x) x=(LPWSTR)malloc(size*sizeof(WCHAR));wcscpy_s(x, size, msg)

extern HINSTANCE hInst;
HINSTANCE hInstRs = NULL;
BOOL lgRealloc = TRUE;

M_CAPI(void) MLG_SetLanuageItems_NoRealloc() {
	lgRealloc = FALSE;
}
M_CAPI(void) MLG_SetLanuageRes(LPWSTR appstarppath, LPWSTR name)
{
	if (!MStrEqualW(name, L"zh") && !MStrEqualW(name, L"zh-CN"))
	{
#if _X64_
		std::wstring  s = FormatString(L"%s\\%s\\PCMgrApp64.resource2.dll", appstarppath, name);
#else
		std::wstring  s = FormatString(L"%s\\%s\\PCMgrApp32.resource2.dll", appstarppath, name);
#endif
		hInstRs = LoadLibrary((LPWSTR)s.c_str());
		if(!hInstRs) hInstRs = hInst;
	}
	else hInstRs = hInst;
	lgRealloc = TRUE;
}
M_CAPI(void) MLG_SetLanuageItems_CallBack(LanuageItems_CallBack c)
{
	callBack = c;
}
LPWSTR MLG_GetLanuageItem(LPWSTR name)
{
	return callBack(name);
}
void MLG_SetLanuageItems_Destroy()
{
	if (!lgRealloc)return;
	delete str_item_copying;
	delete str_item_moveing;
	delete str_item_fileexisted;
	delete str_item_fileexisted_ask;
	delete str_item_question;
	delete str_item_tip;
	delete str_item_delsure;
	delete str_item_ask1;
	delete str_item_ask2;
	delete str_item_ask3;
	delete str_item_deling;
	delete str_item_noadmin1;
	delete str_item_noadmin2;
	delete str_item_delfailed;
	delete str_item_systemidleproc;
	delete str_item_endprocfailed;
	delete str_item_openprocfailed;
	delete str_item_susprocfailed;
	delete str_item_resprocfailed;
	delete str_item_rebootasadmin;
	delete str_item_visible;
	delete str_item_cantgetpath;
	delete str_item_freesuccess;
	delete str_item_proerty;
	delete str_item_entrypoint;
	delete str_item_modulename;
	delete str_item_state;
	delete str_item_contextswitch;
	delete str_item_modulepath;
	delete str_item_address;
	delete str_item_size;
	delete str_item_publisher;
	delete str_item_windowtext;
	delete str_item_windowhandle;
	delete str_item_wndclass;
	delete str_item_wndbthread;
	delete str_item_vwinstitle;
	delete str_item_vmodulestitle;
	delete str_item_vthreadtitle;
	delete str_item_enum_modulefailed;
	delete str_item_enum_threadfailed;
	delete str_item_freeinvproc;
	delete str_item_freefailed;
	delete str_item_killthreaderr;
	delete str_item_killinvthread;
	delete str_item_openthreaderr;
	delete str_item_suthreaderr;
	delete str_item_rethreaderr;
	delete str_item_invthread;
	delete str_item_suthreadwarn;
	delete str_item_kernelnotload;
	delete str_item_delscask;
	delete str_item_delsc2ask;
	delete str_item_endtask;
	delete str_item_rebootexplorer;
	delete str_item_loaddriver;
	delete str_item_unloaddriver;
	delete str_item_filenotexist;
	delete str_item_filetrusted;
	delete str_item_filenottrust;
	delete str_item_opensc_err;
	delete str_item_delsc_err;
	delete str_item_setscstart_err;
}
void MLG_SetLanuageItems_1(int id, LPWSTR msg, int size)
{
	switch (id)
	{
	case 0: HASSTR(str_item_copying); break;
	case 1: HASSTR(str_item_moveing); break;
	case 2: HASSTR(str_item_fileexisted); break;
	case 3: HASSTR(str_item_fileexisted_ask); break;
	case 4: HASSTR(str_item_question); break;
	case 5: HASSTR(str_item_tip); break;
	case 6: HASSTR(str_item_delsure); break;
	case 7: HASSTR(str_item_ask1); break;
	case 8: HASSTR(str_item_ask2); break;
	case 9: HASSTR(str_item_ask3); break;
	case 10: HASSTR(str_item_deling); break;
	case 11: HASSTR(str_item_noadmin1); break;
	case 12: HASSTR(str_item_noadmin2); break;
	case 13: HASSTR(str_item_delfailed); break;
	case 14: HASSTR(str_item_systemidleproc); break;
	case 15: HASSTR(str_item_endprocfailed); break;
	case 16: HASSTR(str_item_openprocfailed); break;
	case 17: HASSTR(str_item_susprocfailed); break;
	case 18: HASSTR(str_item_resprocfailed); break;
	case 19: HASSTR(str_item_rebootasadmin); break;
	case 20: HASSTR(str_item_visible); break;
	case 21: HASSTR(str_item_cantgetpath); break;
	case 22: HASSTR(str_item_freesuccess); break;
	case 23: HASSTR(str_item_proerty); break;
	case 24: HASSTR(str_item_entrypoint); break;
	case 25: HASSTR(str_item_modulename); break;
	case 26: HASSTR(str_item_state); break;
	case 27: HASSTR(str_item_contextswitch); break;
	case 28: HASSTR(str_item_modulepath); break;
	case 29: HASSTR(str_item_address); break;
	case 30: HASSTR(str_item_size); break;
	case 31: HASSTR(str_item_publisher); break;
	case 32: HASSTR(str_item_windowtext); break;
	case 33: HASSTR(str_item_windowhandle); break;
	case 34: HASSTR(str_item_wndclass); break;
	case 35: HASSTR(str_item_wndbthread); break;
	case 36: HASSTR(str_item_vwinstitle); break;
	case 37: HASSTR(str_item_vmodulestitle); break;
	case 38: HASSTR(str_item_vthreadtitle); break;
	case 39: HASSTR(str_item_enum_modulefailed); break;
	case 40: HASSTR(str_item_enum_threadfailed); break;
	case 41: HASSTR(str_item_freeinvproc); break;
	case 42: HASSTR(str_item_freefailed); break;
	case 43: HASSTR(str_item_killthreaderr); break;
	case 44: HASSTR(str_item_killinvthread); break;
	case 45: HASSTR(str_item_openthreaderr); break;
	case 46: HASSTR(str_item_suthreaderr); break;
	case 47: HASSTR(str_item_rethreaderr); break;
	case 48: HASSTR(str_item_invthread); break;
	case 49: HASSTR(str_item_suthreadwarn); break;
	case 50: HASSTR(str_item_kernelnotload); break;
	}
}
void MLG_SetLanuageItems_2(int id, LPWSTR msg, int size)
{
	switch (id)
	{
	case 0: HASSTR(str_item_delscask); break;
	case 1: HASSTR(str_item_delsc2ask); break;
	case 2: HASSTR(str_item_endtask); break;
	case 3: HASSTR(str_item_rebootexplorer); break;
	case 4: HASSTR(str_item_loaddriver); break;
	case 5: HASSTR(str_item_unloaddriver); break;
	case 6: HASSTR(str_item_filenotexist); break;
	case 7: HASSTR(str_item_filetrusted); break;
	case 8: HASSTR(str_item_filenottrust); break;
	case 9: HASSTR(str_item_opensc_err); break;
	case 10: HASSTR(str_item_delsc_err); break;
	case 11: HASSTR(str_item_setscstart_err); break;
		
	}

}
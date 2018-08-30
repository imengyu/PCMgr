#pragma once
#include "stdafx.h"
#include <string>

//语言模块
//所有字符串都在此加载

//设置退出时语言资源回收
M_CAPI(void) MLG_SetLanuageItems_CanRealloc();

//设置退出时语言资源不回收
M_CAPI(void) MLG_SetLanuageItems_NoRealloc();
//设置菜单的语言资源
//    appstarppath：当前程序启动目录
//    name：语言资源名称（zh/en）
M_CAPI(void) MLG_SetLanuageRes(LPWSTR appstarppath, LPWSTR name);

extern std::wstring str_item_kill_ask_start;
extern std::wstring str_item_kill_ask_end;
extern std::wstring str_item_kill_ast_content;
extern std::wstring str_item_kill_failed;
extern std::wstring str_item_access_denied;
extern std::wstring str_item_op_failed;
extern std::wstring str_item_invalidproc;
extern std::wstring str_item_cantcopyfile;
extern std::wstring str_item_cantmovefile;
extern std::wstring str_item_choose_target_dir;

void MLG_Startup();

void MLG_SetLanuageItems_0(int id, LPWSTR msg, int size);
void MLG_SetLanuageItems_Destroy();
void MLG_SetLanuageItems_1(int id, LPWSTR msg, int size);
void MLG_SetLanuageItems_2(int id, LPWSTR msg, int size);


extern LPWSTR str_item_copying;
extern LPWSTR str_item_moveing;
extern LPWSTR str_item_fileexisted;
extern LPWSTR str_item_fileexisted_ask;
extern LPWSTR str_item_question;
extern LPWSTR str_item_tip;
extern LPWSTR str_item_delsure;
extern LPWSTR str_item_ask1;
extern LPWSTR str_item_ask2;
extern LPWSTR str_item_ask3;
extern LPWSTR str_item_deling;
extern LPWSTR str_item_noadmin1;
extern LPWSTR str_item_noadmin2;
extern LPWSTR str_item_delfailed;
extern LPWSTR str_item_systemidleproc;
extern LPWSTR str_item_endprocfailed;
extern LPWSTR str_item_openprocfailed;
extern LPWSTR str_item_susprocfailed;
extern LPWSTR str_item_resprocfailed;
extern LPWSTR str_item_rebootasadmin;
extern LPWSTR str_item_visible;
extern LPWSTR str_item_cantgetpath;
extern LPWSTR str_item_freesuccess;
extern LPWSTR str_item_proerty;
extern LPWSTR str_item_entrypoint;
extern LPWSTR str_item_modulename;
extern LPWSTR str_item_state;
extern LPWSTR str_item_contextswitch;
extern LPWSTR str_item_modulepath;
extern LPWSTR str_item_address;
extern LPWSTR str_item_size;
extern LPWSTR str_item_publisher;
extern LPWSTR str_item_windowtext;
extern LPWSTR str_item_windowhandle;
extern LPWSTR str_item_wndclass;
extern LPWSTR str_item_wndbthread;
extern LPWSTR str_item_vwinstitle;
extern LPWSTR str_item_vmodulestitle;
extern LPWSTR str_item_vthreadtitle;
extern LPWSTR str_item_enum_modulefailed;
extern LPWSTR str_item_enum_threadfailed;
extern LPWSTR str_item_freeinvproc;
extern LPWSTR str_item_freefailed;
extern LPWSTR str_item_killthreaderr;
extern LPWSTR str_item_killinvthread;
extern LPWSTR str_item_openthreaderr;
extern LPWSTR str_item_suthreaderr;
extern LPWSTR str_item_rethreaderr;
extern LPWSTR str_item_invthread; 
extern LPWSTR str_item_suthreadwarn;
extern LPWSTR str_item_kernelnotload;

extern LPWSTR str_item_delscask;
extern LPWSTR str_item_delsc2ask;
extern LPWSTR str_item_endtask;
extern LPWSTR str_item_rebootexplorer;
extern LPWSTR str_item_loaddriver;
extern LPWSTR str_item_unloaddriver;
extern LPWSTR str_item_filenotexist;
extern LPWSTR str_item_filetrusted;
extern LPWSTR str_item_filenottrust;
extern LPWSTR str_item_opensc_err;
extern LPWSTR str_item_delsc_err;
extern LPWSTR str_item_setscstart_err;
extern LPWSTR str_item_set_to;
extern LPWSTR str_item_killtree_end;
extern LPWSTR str_item_killtree_content;
extern LPWSTR str_item_want_disconnectuser;
extern LPWSTR str_item_want_logoffuser;
extern LPWSTR str_item_please_enter_password;
extern LPWSTR str_item_conss_failed;
extern LPWSTR str_item_conect_ss;
extern LPWSTR str_item_disconect_ss;
extern LPWSTR str_item_disconss_failed;
extern LPWSTR str_item_logoff_ss;
extern LPWSTR str_item_logoff_ssfailed;
extern LPWSTR str_item_set_proc_priority_failed;
extern LPWSTR str_item_set_proc_affinity_failed;
extern LPWSTR str_item_warn_title;
extern LPWSTR str_item_loaddriver_warn;
extern LPWSTR str_item_loaddriver_warn_title;








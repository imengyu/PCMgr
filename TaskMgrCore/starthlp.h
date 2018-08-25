#pragma once
#include "stdafx.h"
#include "reghlp.h"

//枚举开机启动项回调
typedef void(__cdecl*EnumStartupsCallBack)(
	LPWSTR dspName, //开机启动项名称
	LPWSTR type, //动项类别
	LPWSTR path, //可执行文件路径
	HKEY regrootpath, //根注册表
	LPWSTR regpath,//注册表路径
	LPWSTR regvalue//注册表路径对应键值名称
	);

//枚举开机启动项
//    callBack：回调
M_CAPI(VOID) MEnumStartups(EnumStartupsCallBack callBack);

LRESULT MSM_HandleWmCommand(WPARAM wParam);

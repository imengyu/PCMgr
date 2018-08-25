#pragma once
#include "stdafx.h"

//服务信息暂存
typedef struct _SERVICE_STORAGE {
	LPWSTR                    lpServiceName;//服务名称
	LPENUM_SERVICE_STATUS_PROCESS lpSvc;//状态
	WCHAR    ServiceImagePath[MAX_PATH];//对应的映像路径
	BOOL  DriverServiceFounded;
	SC_HANDLE ServiceHandle;//服务句柄
	DWORD ServiceStartType;//服务启动信息
} SERVICE_STORAGE, *LPSERVICE_STORAGE;

//枚举服务回调
typedef void(__cdecl*EnumServicesCallBack)(LPWSTR dspName, LPWSTR scName,
	DWORD scType, DWORD currentState, DWORD dwProcessId,	BOOL sysSc, DWORD dwStartType,
	LPWSTR lpBinaryPathName, LPWSTR lpLoadOrderGroup);

//更改服务的启动类型
//    scname：服务名称
//    type：启动类型
//    errText：错误文字
M_CAPI(BOOL) MSCM_ChangeScStartType(LPWSTR scname, DWORD type, LPWSTR errText);
//更改服务的状态
//    scname：服务名称
//    targetStatus：目标状态（SERVICE_STOPPED/SERVICE_RUNNING/SERVICE_PAUSED）
//    targetCtl：SERVICE_CONTROL_STOP/SERVICE_CONTROL_PAUSE /SERVICE_CONTROL_CONTINUE/SERVICE_CONTROL_INTERROGATE/SERVICE_CONTROL_SHUTDOWN
//    errText：错误文字
M_CAPI(BOOL) MSCM_ControlSc(LPWSTR scname, DWORD targetStatus, DWORD targetCtl, LPWSTR errText);

M_CAPI(void) MSCM_SetCurrSelSc(LPWSTR scname);

LRESULT MSCM_HandleWmCommand(WPARAM wParam);

//初始化，返回是否成功
//  首先调用此函数
M_CAPI(BOOL) MSCM_Init();
//退出释放资源
M_CAPI(void) MSCM_Exit();

//获取一个服务所对应的服务组，仅对svchost承载的系统服务有效
//    path：服务的完整命令行
M_CAPI(LPWSTR) MSCM_GetScGroup(LPWSTR path);

//检查 fileName 对应的驱动是不是在服务中注册
//    fileName：需要检查的驱动
//    [OUT] outName：输出服务名称（缓冲区大小260）
//    [OUT] pScInfo：输出服务信息
M_CAPI(BOOL) MSCM_CheckDriverServices(LPWSTR fileName, LPWSTR outName, LPSERVICE_STORAGE*pScInfo);
//枚举驱动的服务
M_CAPI(BOOL) MSCM_EnumDriverServices();
//枚举所有Win32服务
//    callback：回调
M_CAPI(BOOL) MEnumServices(EnumServicesCallBack callback);
//删除服务
//    scname：服务名称
//    errText：错误文字
M_CAPI(BOOL) MSCM_DeleteService(LPWSTR scname, LPWSTR errText);

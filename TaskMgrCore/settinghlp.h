#pragma once
#include "stdafx.h"

//初始化设置读取
void M_CFG_Init();

//读取BOOL设置
M_CAPI(BOOL) M_CFG_GetConfigBOOL(LPWSTR configkey, LPWSTR configSection, BOOL defaultValue);
//写入BOOL设置
M_CAPI(BOOL) M_CFG_SetConfigBOOL(LPWSTR configkey, LPWSTR configSection, BOOL value);

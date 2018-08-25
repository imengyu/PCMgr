#pragma once
#include "stdafx.h"

//∂¡»°BOOL…Ë÷√£®ini£©
M_CAPI(BOOL) M_CFG_GetConfigBOOL(LPWSTR configkey, LPWSTR configSection, BOOL defaultValue);
//–¥»ÎBOOL£®ini£©
M_CAPI(BOOL) M_CFG_SetConfigBOOL(LPWSTR configkey, LPWSTR configSection, BOOL value);

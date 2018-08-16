#pragma once
#include "stdafx.h"

//强制删除驱动服务的注册表
M_CAPI(BOOL) MREG_ForceDeleteServiceRegkey(LPWSTR lpszDriverName);
//获取服务的注册表键值路径
M_CAPI(BOOL) MREG_GetServiceReg(LPWSTR servicName, LPWSTR buf, size_t size);
//尝试转换CLSID
M_CAPI(HKEY) MREG_CLSIDToHKEY(HKEY hRootKey, LPWSTR clsid);
//尝试转换CLSID并打开InprocServer32值
M_CAPI(HKEY) MREG_CLSIDToHKEYInprocServer32(HKEY hRootKey, LPWSTR clsid);
//根注册表键值转字符串
M_CAPI(LPWSTR) MREG_ROOTKEYToStr(HKEY hRootKey);
//删除注册表键以及子键
M_CAPI(BOOL) MREG_DeleteKey(HKEY hRootKey, LPWSTR path);
//删除注册表项下的子值
M_CAPI(BOOL) MREG_DeleteKeyValue(HKEY hRootKey, LPWSTR path, LPWSTR value);

#pragma once
#include "stdafx.h"

//枚举注册表某键所有子值回调
typedef BOOL(*ENUMKEYVALECALLBACK)(HKEY hRootKey, LPWSTR path, LPWSTR valueName, DWORD dataType, DWORD dataSize, LPWSTR dataSample, DWORD index, DWORD allCount);
//枚举注册表某键所有子键回调
typedef BOOL(*ENUMKEYSCALLBACK)(HKEY hRootKey, LPWSTR path, LPWSTR childKeyName, BOOL hasChild, DWORD index, DWORD allCount);

//重命名注册表项
M_CAPI(BOOL) MREG_RenameKey(HKEY hRootKey, LPWSTR path, LPWSTR newName);
//强制删除驱动服务的注册表
M_CAPI(BOOL) MREG_ForceDeleteServiceRegkey(LPWSTR lpszDriverName);
//获取服务的注册表键值路径
//    servicName：服务名称
//    [OUT] buf：输出注册表键值路径
//    size：buf缓冲区大小（字符）
M_CAPI(BOOL) MREG_GetServiceReg(LPWSTR servicName, LPWSTR buf, size_t size);
//尝试转换CLSID
M_CAPI(HKEY) MREG_CLSIDToHKEY(HKEY hRootKey, LPWSTR clsid);
//尝试转换CLSID并打开InprocServer32值
M_CAPI(HKEY) MREG_CLSIDToHKEYInprocServer32(HKEY hRootKey, LPWSTR clsid);
//根注册表键值转字符串
M_CAPI(LPWSTR) MREG_ROOTKEYToStr(HKEY hRootKey);
//注册表值类型转字符串
M_CAPI(LPWSTR) MREG_RegTypeToStr(DWORD regType);
//删除注册表键以及子键
M_CAPI(BOOL) MREG_DeleteKey(HKEY hRootKey, LPWSTR path);
//删除注册表项下的子值
M_CAPI(BOOL) MREG_DeleteKeyValue(HKEY hRootKey, LPWSTR path, LPWSTR value);
//枚举注册表某键下的所有子值
M_CAPI(BOOL) MREG_EnumKeyVaules(HKEY hRootKey, LPWSTR path, ENUMKEYVALECALLBACK callBack);
//枚举注册表某键下的所有子键
M_CAPI(BOOL) MREG_EnumKeys(HKEY hRootKey, LPWSTR path, ENUMKEYSCALLBACK callBack);


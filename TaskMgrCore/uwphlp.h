#pragma once
#include "stdafx.h"
#include <appmodel.h>
#include <appxpackaging.h>

typedef struct tag_UWP_PACKAGE_APP_INFO
{
	WCHAR DisplayName[128];
	WCHAR AppUserModelId[128];
}UWP_PACKAGE_APP_INFO,*PUWP_PACKAGE_APP_INFO;
typedef struct tag_UWP_PACKAGE_INFO
{
	WCHAR AppUserModelId[128];
	WCHAR AppPackageFamilyName[128];
	WCHAR AppPackageFullName[128];

	WCHAR InstallPath[MAX_PATH];
	WCHAR IconPath[MAX_PATH];
	WCHAR DisplayName[128];
	WCHAR PublisherDisplayName[128];

	UINT ApplicationsCount;
	tag_UWP_PACKAGE_APP_INFO Applications[16];
}UWP_PACKAGE_INFO,*PUWP_PACKAGE_INFO;
typedef struct tag_UWP_PACKAGE_INFO_EX
{
	WCHAR AppUserModelId[128];
	WCHAR AppPackageFamilyName[128];
	WCHAR AppPackageFullName[128];

	WCHAR InstallPath[MAX_PATH];
	WCHAR IconPath[MAX_PATH];
	WCHAR DisplayName[128];

	int IconBackgroundColor;
}UWP_PACKAGE_INFO_EX, *PUWP_PACKAGE_INFO_EX;

#define DEFPROPERTYKEY(name, g1, g2, g3, b1,b2,b3,b4,b5,b6,b7,b8, pid) const PROPERTYKEY name = { { (long)g1, (short)g2, (short)g3, { b1,b2,b3,b4,b5,b6,b7,b8 } }, pid }


M_CAPI(BOOL) M_UWP_IsInited();
M_CAPI(BOOL) M_UWP_Init();
M_CAPI(BOOL) M_UWP_UnInit();
M_CAPI(BOOL) M_UWP_RunUWPApp(LPWSTR strAppUserModelId, PDWORD pdwProcessId);
M_CAPI(BOOL) M_UWP_KillUWPApplication(LPWSTR packageName);
M_CAPI(BOOL) M_UWP_UnInstallUWPApplication(LPWSTR packageName);
M_CAPI(BOOL) M_UWP_EnumUWPApplications();

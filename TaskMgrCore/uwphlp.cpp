#include "stdafx.h"
#include "uwphlp.h"
#include "fmhlp.h"
#include "loghlp.h"
#include "StringHlp.h"
#include <Shlobj.h>
#include <Shobjidl.h>
#include <list>
#include <string>  

extern HWND hWndMain;

IApplicationActivationManager *applicationActivationManager = nullptr;
IPackageDebugSettings *packageDebugSettings = nullptr;
IAppxFactory*appxFactory = nullptr;
BOOL uwpInited = FALSE;
std::list<PUWP_PACKAGE_INFO_EX> uwpPackages;

DEFPROPERTYKEY(PKEY_AppUserModel_ID, 0x9F4C2855, 0x9F79, 0x4B39, 0xA8, 0xD0, 0xE1, 0xD4, 0x2D, 0xE1, 0xD5, 0xF3, 0x05);
DEFPROPERTYKEY(PKEY_App_Immersive, 0x9F4C2855, 0x9F79, 0x4B39, 0xA8, 0xD0, 0xE1, 0xD4, 0x2D, 0xE1, 0xD5, 0xF3, 0x0E);
DEFPROPERTYKEY(PKEY_AppUserModel_InstallPath, 0x9F4C2855, 0x9F79, 0x4B39, 0xA8, 0xD0, 0xE1, 0xD4, 0x2D, 0xE1, 0xD5, 0xF3, 0x0F);
DEFPROPERTYKEY(PKEY_AppUserModel_PackageFamilyName, 0x9F4C2855, 0x9F79, 0x4B39, 0xA8, 0xD0, 0xE1, 0xD4, 0x2D, 0xE1, 0xD5, 0xF3, 0x11);
DEFPROPERTYKEY(PKEY_AppUserModel_PackageFullName, 0x9F4C2855, 0x9F79, 0x4B39, 0xA8, 0xD0, 0xE1, 0xD4, 0x2D, 0xE1, 0xD5, 0xF3, 0x15);
DEFPROPERTYKEY(PKEY_AppName, 0xB725F130, 0x47EF, 0x101A, 0xA5, 0xF1, 0x02, 0x60, 0x8C, 0x9E, 0xEB, 0xAC, 0x0A);
DEFPROPERTYKEY(PKEY_SmallLogoPath, 0x86D40B4D, 0x9069, 0x443C, 0x81, 0x9A, 0x2A, 0x54, 0x09, 0x0D, 0xCC, 0xEC, 0x02);

HRESULT M_GetIsImmersiveApp(IPropertyStore *pPropertyStore, BOOL *isImmersiveApp);
HRESULT M_IPropertyStore_GetString(REFPROPERTYKEY key, IPropertyStore *pPropertyStore, std::wstring*str);
void M_UWP_ReadUWPApplicationProperty(IPropertyStore *pPropertyStore);
void M_UWP_ClearAllCache();

M_CAPI(BOOL) M_UWP_IsInited()
{
	return uwpInited;
}
M_CAPI(BOOL) M_UWP_Init()
{
	HRESULT hr;
	// Instantiate IApplicationActivationManager
	hr = CoCreateInstance(CLSID_ApplicationActivationManager,
		NULL,
		CLSCTX_LOCAL_SERVER,
		IID_PPV_ARGS(&applicationActivationManager));
	if (!SUCCEEDED(hr))
		LogErr2(L"CoCreateInstance failed : 0x%08X", hr);

	hr = CoCreateInstance(CLSID_PackageDebugSettings,
		NULL,
		CLSCTX_INPROC_SERVER,
		IID_PPV_ARGS(&packageDebugSettings));
	if (!SUCCEEDED(hr))
		LogErr2(L"CoCreateInstance failed : 0x%08X", hr);

	hr = CoCreateInstance(CLSID_AppxFactory, NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&appxFactory));
	if (!SUCCEEDED(hr))
		LogErr2(L"CoCreateInstance failed : 0x%08X", hr);

	uwpInited = TRUE;

	return SUCCEEDED(hr);
}
M_CAPI(BOOL) M_UWP_UnInit()
{
	if (applicationActivationManager != nullptr)
		applicationActivationManager->Release();
	if (packageDebugSettings != nullptr)
		packageDebugSettings->Release();
	if (appxFactory != nullptr)
		appxFactory->Release();

	appxFactory = nullptr;
	packageDebugSettings = nullptr;
	applicationActivationManager = nullptr;

	M_UWP_ClearAllCache();

	uwpInited = FALSE;

	return TRUE;
}
M_CAPI(BOOL) M_UWP_ReadUWPPackage(LPWSTR installDir, UWP_PACKAGE_INFO*outInfo)
{
	UWP_PACKAGE_INFO info = { 0 };
	BOOL result = FALSE;
	HRESULT hr;
	WCHAR appxManifestPath[MAX_PATH];
	wcscpy_s(appxManifestPath, installDir);
	wcscat_s(appxManifestPath, L"\\AppXManifest.xml");

	if (!outInfo || !appxFactory)return FALSE;

	if (MFM_FileExist(appxManifestPath))
	{
		IStream*appxManifestStream = NULL;
		IAppxManifestReader*appxManifestReader = nullptr;
		IAppxManifestProperties *appxManifestProperties = nullptr;
		IAppxManifestApplicationsEnumerator *appxManifestApplicationsEnumerator = nullptr;

		hr = SHCreateStreamOnFileEx(appxManifestPath, STGM_READ, FILE_ATTRIBUTE_NORMAL, FALSE, NULL, &appxManifestStream);
		if (!SUCCEEDED(hr))
		{
			LogErr2(L"SHCreateStreamOnFileEx failed : 0x%08X", hr);
			goto CLEANUP;
		}

		hr = appxFactory->CreateManifestReader(appxManifestStream, &appxManifestReader);
		if (!SUCCEEDED(hr))
		{
			LogErr2(L"IAppxManifestReader->CreateManifestReader failed : 0x%08X", hr);
			goto CLEANUP;
		}

		hr = appxManifestReader->GetProperties(&appxManifestProperties);
		if (SUCCEEDED(hr))
		{
			//Read all  Manifest Properties

			LPWSTR displayName = NULL;
			LPWSTR outLogo = NULL;
			LPWSTR publisherDisplayName = NULL;

			hr = appxManifestProperties->GetStringValue(L"Logo", &outLogo);
			if (SUCCEEDED(hr))
			{
				wcscpy_s(info.IconPath, outLogo);
				CoTaskMemFree(outLogo);
			}
			else LogErr2(L"IAppxManifestProperties->GetStringValue(L\"Logo\") failed : 0x%08X", hr);
			hr = appxManifestProperties->GetStringValue(L"DisplayName", &displayName);
			if (SUCCEEDED(hr))
			{
				wcscpy_s(info.DisplayName, displayName);
				CoTaskMemFree(displayName);
			}
			else LogErr2(L"IAppxManifestProperties->GetStringValue(\"DisplayName\") failed : 0x%08X", hr);
			hr = appxManifestProperties->GetStringValue(L"PublisherDisplayName", &publisherDisplayName);
			if (SUCCEEDED(hr))
			{
				wcscpy_s(info.PublisherDisplayName, publisherDisplayName);
				CoTaskMemFree(publisherDisplayName);
			}
			else LogErr2(L"IAppxManifestProperties->GetStringValue(\"PublisherDisplayName\") failed : 0x%08X", hr);
		}
		else LogErr2(L"IAppxManifestReader->GetProperties failed : 0x%08X", hr);

		hr = appxManifestReader->GetApplications(&appxManifestApplicationsEnumerator);
		if (SUCCEEDED(hr))
		{
			//Read for all applications in this package

			int index = 0;
			BOOL hasNext = TRUE;
			appxManifestApplicationsEnumerator->GetHasCurrent(&hasNext);
			while (hasNext)
			{
				if (index >= 16) break;
				BOOL hasCurrent = FALSE;
				IAppxManifestApplication*currrentApp = NULL;

				hr = appxManifestApplicationsEnumerator->GetHasCurrent(&hasCurrent);
				if (!SUCCEEDED(hr) || !hasCurrent) break;
				hr = appxManifestApplicationsEnumerator->GetCurrent(&currrentApp);
				if (SUCCEEDED(hr))
				{
					LPWSTR displayName = NULL;
					LPWSTR userModuleId = NULL;
					if (SUCCEEDED(currrentApp->GetAppUserModelId(&userModuleId))) {
						wcscpy_s(info.Applications[index].AppUserModelId, userModuleId);
						CoTaskMemFree(userModuleId);
					}
					if (SUCCEEDED(currrentApp->GetStringValue(L"DisplayName", &displayName))) {
						wcscpy_s(info.Applications[index].DisplayName, userModuleId);
						CoTaskMemFree(displayName);
					}
					currrentApp->Release();
				}

				appxManifestApplicationsEnumerator->MoveNext(&hasNext);
				index++;
			}
			appxManifestApplicationsEnumerator->Release();

			info.ApplicationsCount = index;
		}
		else LogErr2(L"IAppxManifestReader->GetApplications failed : 0x%08X", hr);

		memcpy_s(outInfo, sizeof(UWP_PACKAGE_INFO), &info, sizeof(info));

		result = TRUE;

	CLEANUP:

		if (appxManifestProperties != nullptr) appxManifestProperties->Release();
		if (appxManifestReader != nullptr) appxManifestReader->Release();
		if (appxManifestStream != nullptr) appxManifestStream->Release();
	}
	return result;
}
M_CAPI(BOOL) M_UWP_RunUWPApp(LPWSTR strAppUserModelId, PDWORD pdwProcessId)
{
	if (applicationActivationManager != nullptr) {
		// This call ensures that the app is launched as the foreground window
		HRESULT	 hrResult = CoAllowSetForegroundWindow(applicationActivationManager, NULL);
		// Launch the app
		if (SUCCEEDED(hrResult)) {
			hrResult = applicationActivationManager->ActivateApplication(strAppUserModelId,
				NULL,
				AO_NONE,
				pdwProcessId);
			if (!SUCCEEDED(hrResult))
			{
				LogErr2(L"Failed with HRESULT : 0x%08X .", hrResult);
				return FALSE;
			}
			return TRUE;
		}
	}
	return FALSE;
}
M_CAPI(BOOL) M_UWP_KillUWPApplication(LPWSTR packageName)
{
	if (!StrEmepty(packageName) && packageDebugSettings != nullptr)
	{
		HRESULT hr = packageDebugSettings->TerminateAllProcesses(packageName);
		if (!SUCCEEDED(hr))
		{
			LogErr2(L"Failed with HRESULT : 0x%08X .", hr);
			return FALSE;
		}
		return TRUE;
	}
	else LogErr2(L"Invalid parameter.");
	return FALSE;
}
M_CAPI(BOOL) M_UWP_UnInstallUWPApplication(LPWSTR packageName)
{

	return 0;
}
M_CAPI(BOOL) M_UWP_EnumUWPApplications()
{
	//从任务管理器(1803版本)反编译
	//.text:004B0F07 ; int __thiscall WdcAppHistoryMonitor::_ReconcileImmersiveApplications(WdcAppHistoryMonitor *this)
	//.text:004B0F07 ? _ReconcileImmersiveApplications@WdcAppHistoryMonitor@@AAEJXZ proc near
	//伪代码：
	/*
	v9 = SHGetKnownFolderItem(FOLDERID_AppsFolder, 0x4000, 0, &_GUID_43826d1e_e718_42ee_bc55_a1e261c37bfe, &v31);
	v6 = v9;
	if (v9 < 0)
	{
		v10 = v9;
		v11 = GetCurrentThreadId();
		WdcDebugMessage(
			"base\\diagnosis\\pdui\\atm\\apphistorymonitor.cpp",
			"WdcAppHistoryMonitor::_ReconcileImmersiveApplications",
			1224,
			L"%d FAIL: 0x%08x",
			v11,
			v10);
		goto LABEL_49;
	}
	v12 = (*(int(__thiscall **)(_DWORD, int, _DWORD, const GUID *, GUID *, int *))(*(_DWORD *)v31 + 12))(
		*(_DWORD *)(*(_DWORD *)v31 + 12),
		v31,
		0,
		&BHID_SFObject,
		&_GUID_000214e6_0000_0000_c000_000000000046,
		&v35);
	v6 = v12;
	if (v12 < 0)
	{
		v13 = v12;
		v14 = GetCurrentThreadId();
		WdcDebugMessage(
			"base\\diagnosis\\pdui\\atm\\apphistorymonitor.cpp",
			"WdcAppHistoryMonitor::_ReconcileImmersiveApplications",
			1227,
			L"%d FAIL: 0x%08x",
			v14,
			v13);
		goto LABEL_49;
	}
	v15 = (*(int(__thiscall **)(_DWORD, int, _DWORD, signed int, int *))(*(_DWORD *)v35 + 16))(
		*(_DWORD *)(*(_DWORD *)v35 + 16),
		v35,
		0,
		64,
		&v36);
	v6 = v15;
	if (v15 < 0)
	{
		v16 = v15;
		v17 = GetCurrentThreadId();
		WdcDebugMessage(
			"base\\diagnosis\\pdui\\atm\\apphistorymonitor.cpp",
			"WdcAppHistoryMonitor::_ReconcileImmersiveApplications",
			1230,
			L"%d FAIL: 0x%08x",
			v17,
			v16);
		goto LABEL_49;
	}
	while (!(*(int(__thiscall **)(_DWORD, int, signed int, LPVOID *, _DWORD))(*(_DWORD *)v36 + 12))(
		*(_DWORD *)(*(_DWORD *)v36 + 12),
		v36,
		1,
		&pv,
		0))
	{
		v32 = 0;
		v6 = (*(int(__thiscall **)(_DWORD, int, LPVOID, _DWORD, GUID *, int *))(*(_DWORD *)v35 + 20))(
			*(_DWORD *)(*(_DWORD *)v35 + 20),
			v35,
			pv,
			0,
			&_GUID_bc110b6d_57e8_4148_a9c6_91015ab2f3a5,
			&v32);
		if (v6 >= 0)
		{
			v33 = 0;
			v6 = (*(int(__thiscall **)(_DWORD, int, _DWORD, _DWORD, GUID *, struct IPropertyStore **))(*(_DWORD *)v32 + 12))(
				*(_DWORD *)(*(_DWORD *)v32 + 12),
				v32,
				0,
				0,
				&_GUID_886d8eeb_8cf2_4446_8d02_cdba1dbdcf99,
				&v33);
			if (v6 >= 0)
				WdcAppHistoryMonitor::_AddImmersiveApplication(v1, v33);
			Microsoft::WRL::ComPtr<IAppxManifestProperties>::InternalRelease((int *)&v33);
		}
		CoTaskMemFree(pv);
		Microsoft::WRL::ComPtr<IEnumIDList>::~ComPtr<IEnumIDList>(&v32);
	}
	*/

	M_UWP_ClearAllCache();

	IShellItem *pShellItem = NULL;
	IShellFolder *pShellFolder = NULL;
	IEnumIDList *pEnumIDList = NULL;
	IPropertyStore *pPropertyStore = NULL;
	IPropertyStoreFactory *pPropertyStoreFactory = NULL;
	LPITEMIDLIST pv = NULL;

	HRESULT hr = S_OK;
	hr = SHGetKnownFolderItem(FOLDERID_AppsFolder, KF_FLAG_DONT_VERIFY, 0, IID_PPV_ARGS(&pShellItem));
	if (FAILED(hr))
	{
		LogErr2(L"SHGetKnownFolderItem failed : 0x%08x", hr);
		goto ERRAND_EXIT;
	}
	hr = pShellItem->BindToHandler(0, BHID_SFObject, IID_PPV_ARGS(&pShellFolder));
	if (FAILED(hr))
	{
		LogErr2(L"pShellItem->BindToHandler failed : 0x%08x", hr);
		goto ERRAND_EXIT;
	}
	hr = pShellFolder->EnumObjects(NULL, SHCONTF_NONFOLDERS, &pEnumIDList);
	if (FAILED(hr))
	{
		LogErr2(L"pShellFolder->EnumObjects failed : 0x%08x", hr);
		goto ERRAND_EXIT;
	}
	while (!pEnumIDList->Next(1, &pv, NULL))
	{
		pPropertyStoreFactory = NULL;
		hr = pShellFolder->BindToObject(pv, 0, IID_PPV_ARGS(&pPropertyStoreFactory));
		if (SUCCEEDED(hr))
		{
			pPropertyStore = NULL;
			hr = pPropertyStoreFactory->GetPropertyStore(GPS_DEFAULT, 0, IID_PPV_ARGS(&pPropertyStore));
			if (SUCCEEDED(hr))
				M_UWP_ReadUWPApplicationProperty(pPropertyStore);
			pPropertyStore->Release();
		}
		CoTaskMemFree(pv);
	}
ERRAND_EXIT:
	if (pEnumIDList) pEnumIDList->Release();
	if (pShellFolder) pShellFolder->Release();
	if (pShellItem) pShellItem->Release();
	return SUCCEEDED(hr);
}
M_CAPI(BOOL) M_UWP_GetUWPApplicationAt(size_t i, PUWP_PACKAGE_INFO_EX buffer) {
	if (i >= 0 && i < uwpPackages.size()) {
		std::list<PUWP_PACKAGE_INFO_EX>::iterator iter(uwpPackages.begin());
		std::advance(iter, i);
		memcpy_s(buffer, sizeof(UWP_PACKAGE_INFO_EX), *iter, sizeof(UWP_PACKAGE_INFO_EX));
		return TRUE;
	}
	return 0;
}
M_CAPI(int) M_UWP_GetUWPApplicationsCount() { return static_cast<int>(uwpPackages.size()); }

void M_UWP_ClearAllCache() {
	for (auto it = uwpPackages.begin(); it != uwpPackages.end(); it++)
		free(*it);
	uwpPackages.clear();
}
void M_UWP_ReadUWPApplicationProperty(IPropertyStore *pPropertyStore)
{
	HRESULT hr = S_OK;

	/*
	DWORD count = 0;
	pPropertyStore->GetCount(&count);
	for (DWORD i = 0; i < count; i++) {

		PROPERTYKEY key = { 0 };
		PROPVARIANT *pv = (PROPVARIANT*)CoTaskMemAlloc(sizeof(PROPVARIANT));

		hr = pPropertyStore->GetAt(i, &key);
		if (SUCCEEDED(hr)) {
			hr = pPropertyStore->GetValue(key, pv);
			if (SUCCEEDED(hr)) {
				if ((pv->vt  & VT_BSTR) == VT_BSTR)
					Log(L"VT_BSTR %X-%X-%X-%02X%02X%02X%02X%02X%02X%02X%02X %X : %s",
						key.fmtid.Data1, key.fmtid.Data2, key.fmtid.Data3,
						key.fmtid.Data4[0], key.fmtid.Data4[1], key.fmtid.Data4[2], key.fmtid.Data4[3],
						key.fmtid.Data4[4], key.fmtid.Data4[5], key.fmtid.Data4[6], key.fmtid.Data4[7],
						key.pid, pv->bstrVal);
				else if ((pv->vt  & VT_UI4) == VT_UI4)
					Log(L"VT_UI4 %X-%X-%X-%02X%02X%02X%02X%02X%02X%02X%02X  %X : %d", 
						key.fmtid.Data1, key.fmtid.Data2, key.fmtid.Data3, 
						key.fmtid.Data4[0], key.fmtid.Data4[1], key.fmtid.Data4[2], key.fmtid.Data4[3], 
						key.fmtid.Data4[4], key.fmtid.Data4[5], key.fmtid.Data4[6], key.fmtid.Data4[7], 
						key.pid, pv->uiVal);
				else if ((pv->vt  & VT_UI8) == VT_UI8)
					Log(L"VT_UI8 %X-%X-%X-%02X%02X%02X%02X%02X%02X%02X%02X  %X : %d",
						key.fmtid.Data1, key.fmtid.Data2, key.fmtid.Data3,
						key.fmtid.Data4[0], key.fmtid.Data4[1], key.fmtid.Data4[2], key.fmtid.Data4[3],
						key.fmtid.Data4[4], key.fmtid.Data4[5], key.fmtid.Data4[6], key.fmtid.Data4[7],
						key.pid, pv->ulVal);
				else if ((pv->vt  & VT_UI1) == VT_UI1)
					Log(L"VT_UI1 %X-%X-%X-%02X%02X%02X%02X%02X%02X%02X%02X  %X : %d",
						key.fmtid.Data1, key.fmtid.Data2, key.fmtid.Data3,
						key.fmtid.Data4[0], key.fmtid.Data4[1], key.fmtid.Data4[2], key.fmtid.Data4[3],
						key.fmtid.Data4[4], key.fmtid.Data4[5], key.fmtid.Data4[6], key.fmtid.Data4[7],
						key.pid, pv->bVal);
				else Log(L"VT_UI4 %X-%X-%X-%02X%02X%02X%02X%02X%02X%02X%02X  %X : Unknow type %d",
					key.fmtid.Data1, key.fmtid.Data2, key.fmtid.Data3,
					key.fmtid.Data4[0], key.fmtid.Data4[1], key.fmtid.Data4[2], key.fmtid.Data4[3],
					key.fmtid.Data4[4], key.fmtid.Data4[5], key.fmtid.Data4[6], key.fmtid.Data4[7],
					key.pid, pv->vt);
			}
		}
		CoTaskMemFree(pv);
	}
	Log(L"==================");
	return;



 [LOG] ==================
 [LOG] VT_BSTR B725F130-47EF-101A-A5F102608C9EEBAC A : 网易云音乐
 [LOG] VT_BSTR 86D40B4D-9069-443C-819A2A54090DCCEC 2 : Assets\Logos\Square44x44Logo.png
 [LOG] VT_UI4 86D40B4D-9069-443C-819A2A54090DCCEC  4 : 30720
 [LOG] VT_UI4 86D40B4D-9069-443C-819A2A54090DCCEC  5 : 65535
 [LOG] VT_BSTR 86D40B4D-9069-443C-819A2A54090DCCEC B : 网易云音乐
 [LOG] VT_BSTR 86D40B4D-9069-443C-819A2A54090DCCEC C : Assets\Logos\Square150x150Logo.png
 [LOG] VT_BSTR 86D40B4D-9069-443C-819A2A54090DCCEC D : Assets\Logos\Wide310x150Logo.png
 [LOG] VT_UI4 86D40B4D-9069-443C-819A2A54090DCCEC  E : 1185
 [LOG] VT_BSTR 86D40B4D-9069-443C-819A2A54090DCCEC 13 : Assets\Logos\Square310x310Logo.png
 [LOG] VT_BSTR 86D40B4D-9069-443C-819A2A54090DCCEC 14 : Assets\Logos\Square71x71Logo.png
 [LOG] VT_UI4 DED77B3-C614-456C-AE5B285B38D7B01B  7 : 0
 [LOG] VT_UI8 446D16B1-8DAD-4870-A748402EA43D788C  64 : 2071313852
 [LOG] VT_BSTR 9F4C2855-9F79-4B39-A8D0E1D42DE1D5F3 5 : 1F8B0F94.122165AE053F_j2p0p5q0044a6!App
 [LOG] VT_UI4 9F4C2855-9F79-4B39-A8D0E1D42DE1D5F3  E : 1
 [LOG] VT_BSTR 9F4C2855-9F79-4B39-A8D0E1D42DE1D5F3 F : C:\Program Files\WindowsApps\1F8B0F94.122165AE053F_1.4.1.0_x64__j2p0p5q0044a6
 [LOG] VT_BSTR 9F4C2855-9F79-4B39-A8D0E1D42DE1D5F3 11 : 1F8B0F94.122165AE053F_j2p0p5q0044a6
 [LOG] VT_BSTR 9F4C2855-9F79-4B39-A8D0E1D42DE1D5F3 15 : 1F8B0F94.122165AE053F_1.4.1.0_x64__j2p0p5q0044a6
 [LOG] ==================

	*/

	BOOL isImmersiveApp = FALSE;
	hr = M_GetIsImmersiveApp(pPropertyStore, &isImmersiveApp);
	if (!SUCCEEDED(hr)) {
		LogErr2(L"M_GetIsImmersiveApp failed : 0x%08x", hr);
		return;
	}
	if (isImmersiveApp) {
		std::wstring buffer;
		hr = M_IPropertyStore_GetString(PKEY_AppUserModel_ID, pPropertyStore, &buffer);
		if (SUCCEEDED(hr)) {

			UWP_PACKAGE_INFO_EX *info = (PUWP_PACKAGE_INFO_EX)malloc(sizeof(UWP_PACKAGE_INFO_EX));
			memset(info, 0, sizeof(UWP_PACKAGE_INFO_EX));
			wcscpy_s(info->AppUserModelId, buffer.c_str());

			if (SUCCEEDED(M_IPropertyStore_GetString(PKEY_AppUserModel_PackageFamilyName, pPropertyStore, &buffer)))
				wcscpy_s(info->AppPackageFamilyName, buffer.c_str());
			if (SUCCEEDED(M_IPropertyStore_GetString(PKEY_AppUserModel_PackageFullName, pPropertyStore, &buffer)))
				wcscpy_s(info->AppPackageFullName, buffer.c_str());
			if (SUCCEEDED(M_IPropertyStore_GetString(PKEY_AppUserModel_InstallPath, pPropertyStore, &buffer)))
				wcscpy_s(info->InstallPath, buffer.c_str());
			if (SUCCEEDED(M_IPropertyStore_GetString(PKEY_AppName, pPropertyStore, &buffer)))
				wcscpy_s(info->DisplayName, buffer.c_str());
			if (SUCCEEDED(M_IPropertyStore_GetString(PKEY_SmallLogoPath, pPropertyStore, &buffer)))
				wcscpy_s(info->IconPath, buffer.c_str());

			uwpPackages.push_back(info);
		}
		else LogErr2(L"M_IPropertyStore_GetString(PKEY_AppUserModel_ID, pPropertyStore, &_AppUserModel_ID) failed : 0x%08x", hr);
	}
}

HRESULT M_GetIsImmersiveApp(IPropertyStore *pPropertyStore, BOOL *isImmersiveApp) {
	HRESULT hr = S_OK;
	if (pPropertyStore) {
		PROPVARIANT *pv = (PROPVARIANT*)CoTaskMemAlloc(sizeof(PROPVARIANT));
		hr = pPropertyStore->GetValue(PKEY_App_Immersive, pv);
		if (SUCCEEDED(hr)) {
			if ((pv->vt  & VT_UI4) == VT_UI4)
				*isImmersiveApp = pv->uiVal == TRUE;
			else hr = E_FAIL;
		}
		CoTaskMemFree(pv);
	}
	return hr;
}
HRESULT M_IPropertyStore_GetString(REFPROPERTYKEY key, IPropertyStore *pPropertyStore, std::wstring*str) {
	HRESULT hr = S_OK;
	if (pPropertyStore) {
		PROPVARIANT *pv = (PROPVARIANT*)CoTaskMemAlloc(sizeof(PROPVARIANT));
		hr = pPropertyStore->GetValue(key, pv);
		if (SUCCEEDED(hr)) {
			if ((pv->vt  & VT_BSTR) == VT_BSTR)
				*str = pv->bstrVal;
			else hr = E_FAIL;
		}
		CoTaskMemFree(pv);
	}
	return hr;
}
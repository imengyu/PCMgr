#include "stdafx.h"
#include "uwphlp.h"
#include "fmhlp.h"
#include "loghlp.h"
#include "StringHlp.h"
#include <Shlobj.h>
#include <Shobjidl.h>

extern HWND hWndMain;

struct UWP_PACKAGE_APP_INFO
{
	WCHAR DisplayName[128];
	WCHAR AppUserModelId[128];
};
struct UWP_PACKAGE_INFO
{
	WCHAR DisplayName[128];
	WCHAR PublisherDisplayName[128];
	WCHAR Logo[128];
	UINT ApplicationsCount;
	UWP_PACKAGE_APP_INFO Applications[16];
};


IApplicationActivationManager *applicationActivationManager = nullptr;
IPackageDebugSettings *packageDebugSettings = nullptr;
IAppxFactory*appxFactory = nullptr;
BOOL uwpInited = FALSE;

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
	if(applicationActivationManager != nullptr)
		applicationActivationManager->Release();
	if (packageDebugSettings != nullptr)
		packageDebugSettings->Release();
	if (appxFactory != nullptr)
		appxFactory->Release();

	appxFactory = nullptr;
	packageDebugSettings = nullptr;
	applicationActivationManager = nullptr;

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
				wcscpy_s(info.Logo, outLogo);
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
	else 
		LogErr2(L"Invalid parameter.");
	return FALSE;
}
M_CAPI(BOOL) M_UWP_UnInstallUWPApplication(LPWSTR packageName)
{

	return 0;
}
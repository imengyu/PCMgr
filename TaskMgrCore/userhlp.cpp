#include "stdafx.h"
#include "userhlp.h"
#include "loghlp.h"
#include "resource.h"
#include "lghlp.h"
#include "mapphlp.h"
#include "StringHlp.h"
#include <Lm.h>

extern HINSTANCE hInstRs;
extern HWND hWndMain;

extern _WinStationConnectW WinStationConnectW;
extern _WinStationDisconnect WinStationDisconnect;
extern _WinStationReset WinStationReset;
extern _WinStationFreeMemory WinStationFreeMemory;
extern _WinStationEnumerateW WinStationEnumerateW;
extern _WinStationQueryInformationW WinStationQueryInformationW;

DWORD selectSessionId = 0;
WCHAR currentUserName[32];
WCHAR currentEnteredUserPassword[32];

LPWSTR MGetUserPrev(DWORD n)
{
	switch (n)
	{
	case 0:
		return L"Guest";
		break;
	case 1:
		return L"User";
		break;
	case 2:
		return L"Administrator";
		break;
	default:
		return L"Unknown";
		break;
	}
}

M_CAPI(int) M_User_EnumUsers(EnumUsersCallBack callBack, LPVOID customData)
{
	if (!callBack) return FALSE;

	PSESSIONIDW sessions;
	ULONG numberOfSessions;

	if (WinStationEnumerateW(NULL, &sessions, &numberOfSessions))
	{
		for (UINT i = 0; i < numberOfSessions; i++)
		{
			WINSTATIONINFORMATION winStationInfo;
			ULONG returnLength;

			if (!WinStationQueryInformationW(
				NULL,
				sessions[i].SessionId,
				WinStationInformation,
				&winStationInfo,
				sizeof(WINSTATIONINFORMATION),
				&returnLength
			))
			{
				winStationInfo.Domain[0] = 0;
				winStationInfo.UserName[0] = 0;
			}

			if (winStationInfo.Domain[0] == 0 || winStationInfo.UserName[0] == 0)
			{
				// Probably the Services or RDP-Tcp session.
				continue;
			}

			if (!callBack(winStationInfo.UserName, sessions[i].SessionId, winStationInfo.LogonId, winStationInfo.Domain, NULL))
				break;
		}

		WinStationFreeMemory(sessions);
	}

	return 0;
}

void MUsersSetCurrentSelect(DWORD sessionId) {
	selectSessionId = sessionId;
}
void MUsersSetCurrentSelectUserName(LPWSTR userName) {
	wcscpy_s(currentUserName, userName);
}

M_CAPI(BOOL) M_User_GetUserInfo(LPWSTR userName , LPWSTR userIcoPath, LPWSTR userFullName, int max2) {
	PUSER_INFO_10 userInfo = NULL;
	if (NetUserGetInfo(NULL, userName, 0xA, (LPBYTE*)&userInfo) == NERR_Success) {
		wcscpy_s(userFullName, max2, userInfo->usri10_full_name);
		return TRUE;
	}
	return FALSE;
}

INT_PTR CALLBACK ConnectUserDlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_INITDIALOG: {
		SendMessage(hDlg, WM_SETICON, ICON_SMALL, (LPARAM)LoadIcon(hInstRs, MAKEINTRESOURCE(IDI_ICONAPP)));
		SetDlgItemText(hDlg, IDC_USERNAME, currentUserName);
		break;
	}
	case WM_SYSCOMMAND: {
		if (wParam == SC_CLOSE) {
			EndDialog(hDlg, 0);
		}
		return 0;
	}
	case WM_COMMAND: {
		switch (wParam)
		{
		case IDOK:
			GetDlgItemText(hDlg, IDC_EDIT, currentEnteredUserPassword, 32);
			if (StrEqual(currentEnteredUserPassword, L"")) {
				MessageBox(hWndMain, str_item_please_enter_password, str_item_conect_ss, 0);
				break;
			}
			EndDialog(hDlg, IDOK);
			break;
		case IDCANCEL:
			SendMessage(hDlg, WM_SYSCOMMAND, SC_CLOSE, NULL);
			break;
		}
		break;
	}
	}
	return (INT_PTR)FALSE;
}

LRESULT MUsersHandleWmCommand(WPARAM wParam)
{
	switch (wParam)
	{
	case ID_USER_CONNECT: {
		// Try once with no password.
		if (WinStationConnectW(NULL, selectSessionId, -1, L"", TRUE))
			return TRUE;
		if (DialogBoxW(hInstRs, MAKEINTRESOURCE(IDD_CONNECTSESS), hWndMain, ConnectUserDlgProc) == IDOK)
		{
			if (!WinStationConnectW(NULL, selectSessionId, -1, currentEnteredUserPassword, TRUE)) {
				DWORD lastErr = GetLastError();
				if (lastErr == ERROR_CTX_WINSTATION_ACCESS_DENIED) 
					MShowErrorMessage(L"The requested session access is denied.", str_item_conss_failed, MB_ICONERROR);
				else if (lastErr == ERROR_CTX_WINSTATIONS_DISABLED)
					MShowErrorMessage(L"The session you want to connect is disabled.", str_item_conss_failed, MB_ICONERROR);
				else MShowErrorMessageWithLastErr(str_item_conss_failed, str_item_conect_ss, MB_ICONERROR, MB_OK);
			}
		}
		break;
	}
	case ID_USER_DISCONNECT: {
		if (MessageBox(hWndMain, str_item_want_disconnectuser, str_item_disconect_ss, MB_YESNO | MB_ICONASTERISK) == IDYES)
		{
			if (!WinStationDisconnect(NULL, selectSessionId, FALSE)) {
				DWORD lastErr = GetLastError();
				if (lastErr == ERROR_CTX_WINSTATION_ACCESS_DENIED)
					MShowErrorMessage(L"The requested session access is denied.", str_item_conss_failed, MB_ICONERROR);
				else MShowErrorMessageWithLastErr(str_item_disconss_failed, str_item_disconect_ss, MB_ICONERROR, MB_OK);
			}
		}
		break;
	}
	case ID_USER_LOGOOFF: {
		if (MessageBox(hWndMain, str_item_want_logoffuser, str_item_logoff_ss, MB_YESNO | MB_ICONWARNING) == IDYES)
		{
			if (!WinStationReset(NULL, selectSessionId, FALSE))
			{
				DWORD lastErr = GetLastError();
				if (lastErr == ERROR_CTX_WINSTATION_ACCESS_DENIED)
					MShowErrorMessage(L"The requested session access is denied.", str_item_conss_failed, MB_ICONERROR);
				else MShowErrorMessageWithLastErr(str_item_logoff_ssfailed, str_item_logoff_ss, MB_ICONERROR, MB_OK);
			}
		}
		break;
	}
	default:
		break;
	}
	return 0;
}
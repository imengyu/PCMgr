#include "stdafx.h"
#include "nthlp.h"
#include "msup.h"
#include <stdlib.h>

WCHAR ntStatusStrBuf[20];

extern NtQueryInformationProcessFun NtQueryInformationProcess;

NTSTATUS MQueryProcessVariableSize(_In_ HANDLE ProcessHandle,_In_ PROCESSINFOCLASS ProcessInformationClass,	_Out_ PVOID *Buffer)
{
	NTSTATUS status;
	PVOID buffer;
	ULONG returnLength = 0;

	status = NtQueryInformationProcess(
		ProcessHandle,
		ProcessInformationClass,
		NULL,
		0,
		&returnLength
	);

	if (status != STATUS_BUFFER_OVERFLOW && status != STATUS_BUFFER_TOO_SMALL && status != STATUS_INFO_LENGTH_MISMATCH)
		return status;

	buffer = MAlloc(returnLength);
	status = NtQueryInformationProcess(
		ProcessHandle,
		ProcessInformationClass,
		buffer,
		returnLength,
		&returnLength
	);

	if (NT_SUCCESS(status))
	{
		*Buffer = buffer;
	}
	else
	{
		MFree(buffer);
	}

	return status;
}
LPWSTR MNtStatusToStr(NTSTATUS status) {
	switch (status)
	{
	case STATUS_SUCCESS:
		return L"STATUS_SUCCESS";
	case STATUS_UNSUCCESSFUL:
		return L"STATUS_UNSUCCESSFUL";
	case STATUS_NOT_IMPLEMENTED:
		return L"STATUS_NOT_IMPLEMENTED";
	case STATUS_INVALID_INFO_CLASS:
		return L"STATUS_INVALID_INFO_CLASS";
	case STATUS_INFO_LENGTH_MISMATCH:
		return L"STATUS_INFO_LENGTH_MISMATCH";
	case STATUS_BUFFER_OVERFLOW:
		return L"STATUS_BUFFER_OVERFLOW";
	case STATUS_ACCESS_DENIED:
		return L"STATUS_ACCESS_DENIED";
	case STATUS_BUFFER_TOO_SMALL:
		return L"STATUS_BUFFER_TOO_SMALL";
	case STATUS_PROCESS_IS_TERMINATING:
		return L"STATUS_PROCESS_IS_TERMINATING";
	case STATUS_INVALID_HANDLE:
		return L"STATUS_INVALID_HANDLE";
	case STATUS_INVALID_CID:
		return L"STATUS_INVALID_CID";
	case STATUS_THREAD_IS_TERMINATING:
		return L"STATUS_THREAD_IS_TERMINATING";
	case STATUS_INVALID_PARAMETER:
		return L"STATUS_INVALID_PARAMETER";
	case STATUS_NOT_SUPPORTED:
		return L"STATUS_NOT_SUPPORTED";
	default:
		swprintf_s(ntStatusStrBuf, L"0x%08X", status);
		break;
	}
	return ntStatusStrBuf;
}
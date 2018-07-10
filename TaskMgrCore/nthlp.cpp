#include "stdafx.h"
#include "nthlp.h"
#include <stdlib.h>


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

	buffer = malloc(returnLength);
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
		free(buffer);
	}

	return status;
}
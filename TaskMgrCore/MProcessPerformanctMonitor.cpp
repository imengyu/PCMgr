#include "stdafx.h"
#include "MProcessPerformanctMonitor.h"
#include "MSystemPerformanctMonitor.h"

extern NtQuerySystemInformationFun NtQuerySystemInformation;


//Public interface
//=====================================================

SIZE_T MProcessPerformanctMonitor::GetProcessPrivateWoringSet(PMPROCESS_ITEM processItem)
{
	if (processItem && processItem->Data) return (SIZE_T)processItem->Data->WorkingSetPrivateSize.QuadPart;
	return 0;
}
SIZE_T MProcessPerformanctMonitor::GetProcessIOSpeed(PMPROCESS_ITEM processItem)
{
	if (processItem && processItem->PerfData)
	{
		DWORD interval = static_cast<DWORD>(MSystemPerformanctMonitor::UpdateTimeInterval / 10000000);
		if (interval <= 0)interval = 1;

		return static_cast<DWORD>((
			(processItem->PerfData->IoReadDelta.Delta + 
			processItem->PerfData->IoWriteDelta.Delta +
			processItem->PerfData->IoOtherDelta.Delta)) / interval);
	}
	return 0;
}

double MProcessPerformanctMonitor::GetProcessCpuUseAgeKernel(PMPROCESS_ITEM processItem)
{
	if (processItem && processItem->PerfData) return  processItem->PerfData->CpuKernelUsage;
	return 0;
}
double MProcessPerformanctMonitor::GetProcessCpuUseAgeUser(PMPROCESS_ITEM processItem)
{
	if (processItem && processItem->PerfData) return  processItem->PerfData->CpuUserUsage;
	return 0;
}
double MProcessPerformanctMonitor::GetProcessCpuUseAge(PMPROCESS_ITEM processItem)
{
	if (processItem && processItem->PerfData&& processItem->Data) 
	{
		return  (processItem->PerfData->CpuUserUsage + processItem->PerfData->CpuKernelUsage) * 100.0;
	}
	return 0;
}

ULONGLONG MProcessPerformanctMonitor::GetProcessCpuTime(PMPROCESS_ITEM processItem)
{
	if (processItem && processItem->Data) return (ULONGLONG)(processItem->Data->KernelTime.QuadPart + processItem->Data->UserTime.QuadPart);
	return -1;
}
ULONGLONG MProcessPerformanctMonitor::GetProcessCycle(PMPROCESS_ITEM processItem) {
	if (processItem && processItem->Data) return (ULONGLONG)(processItem->Data->CycleTime);
	return -1;
}

SIZE_T MProcessPerformanctMonitor::GetProcessMemoryInfo(PMPROCESS_ITEM processItem, int col)
{
	if (processItem && processItem->Data) {
		switch (col) {
		case M_GET_PROCMEM_WORKINGSET:
			return (SIZE_T)processItem->Data->VmCounters.WorkingSetSize;
		case M_GET_PROCMEM_WORKINGSETPRIVATE:
			return (SIZE_T)processItem->Data->WorkingSetPrivateSize.QuadPart;
		case M_GET_PROCMEM_WORKINGSETSHARE:
			return (SIZE_T)(processItem->Data->VmCounters.WorkingSetSize - (SIZE_T)processItem->Data->WorkingSetPrivateSize.QuadPart);
		case M_GET_PROCMEM_PEAKWORKINGSET:
			return (SIZE_T)processItem->Data->VmCounters.PeakWorkingSetSize;
		case M_GET_PROCMEM_COMMITEDSIZE:
			return (SIZE_T)processItem->Data->VmCounters.VirtualSize;
		case M_GET_PROCMEM_NONPAGEDPOOL:
			return (SIZE_T)processItem->Data->VmCounters.QuotaNonPagedPoolUsage;
		case M_GET_PROCMEM_PAGEDPOOL:
			return (SIZE_T)processItem->Data->VmCounters.QuotaPagedPoolUsage;
		case M_GET_PROCMEM_PAGEDFAULT:
			return (SIZE_T)processItem->Data->VmCounters.PageFaultCount;
		}
	}
	return 0;
}
ULONGLONG MProcessPerformanctMonitor::GetProcessIOInfo(PMPROCESS_ITEM processItem, int col)
{
	if (processItem && processItem->Data) {
		switch (col)
		{
		case M_GET_PROCIO_READ:
			return processItem->Data->IoCounters.ReadOperationCount;
		case M_GET_PROCIO_WRITE:
			return processItem->Data->IoCounters.WriteOperationCount;
		case M_GET_PROCIO_OTHER:
			return processItem->Data->IoCounters.OtherOperationCount;
		case M_GET_PROCIO_READ_BYTES:
			return processItem->Data->IoCounters.ReadTransferCount;
		case M_GET_PROCIO_WRITE_BYTES:
			return processItem->Data->IoCounters.WriteTransferCount;
		case M_GET_PROCIO_OTHER_BYTES:
			return processItem->Data->IoCounters.OtherTransferCount;
		default:
			break;
		}
	}
	return 0;
}

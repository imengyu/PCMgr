#include "stdafx.h"
#include "perfhlp.h"
#include "ntdef.h"
#include <Psapi.h>

int cpu_Count = 0;
__int64 LastTime;
__int64 TimeInterval;
FILETIME CreateTime;
FILETIME ExitTime;



int GetProcessNumber()
{
	SYSTEM_INFO info;
	GetSystemInfo(&info);
	cpu_Count = static_cast<int>(info.dwNumberOfProcessors);
	return (int)info.dwNumberOfProcessors;
}
__int64 FileTimeToInt64(const FILETIME& time)
{
	ULARGE_INTEGER tt;  //64位无符号整型值
	tt.LowPart = time.dwLowDateTime;
	tt.HighPart = time.dwHighDateTime;
	return(tt.QuadPart);  //返回整型值
}

M_CAPI(MPerfAndProcessData*) MPERF_PerfDataCreate()
{
	return new MPerfAndProcessData();
}
M_CAPI(void) MPERF_PerfDataDestroy(MPerfAndProcessData*data)
{
	if (data) {
		if (data->packageId)
			delete data->packageId;
		delete data;
	}
}
M_CAPI(void) MPERF_CpuTimeUpdate()
{
	FILETIME now;
	GetSystemTimeAsFileTime(&now);
	__int64 nowu = FileTimeToInt64(now);;
	TimeInterval = nowu - LastTime;
	LastTime = nowu;
}
M_CAPI(double) MPERF_GetProcessCpuUseAge(HANDLE hProcess, MPerfAndProcessData*data)
{
	if (hProcess && data)
	{
		FILETIME KernelTime;
		FILETIME UserTime;
		if (GetProcessTimes(hProcess, &CreateTime, &ExitTime, &KernelTime, &UserTime))
		{
			data->LastCpuTime = data->NowCpuTime;
			data->NowCpuTime = (FileTimeToInt64(KernelTime) + FileTimeToInt64(UserTime)) / cpu_Count;
			if (TimeInterval == 0) return 0;
			__int64 i1 = (data->NowCpuTime - data->LastCpuTime) / 1000;
			double rs = static_cast<double>((double)i1 / (double)(TimeInterval / 100000));
			return (rs < 0.1 && rs>0.05) ? 0.1 : rs;
		}
	}
	return -1;
}
M_CAPI(DWORD) MPERF_GetProcessRam(HANDLE hProcess)
{
	if (hProcess)
	{
		PROCESS_MEMORY_COUNTERS mpc;
		if (GetProcessMemoryInfo(hProcess, &mpc, sizeof(mpc)))
			return mpc.WorkingSetSize;
	}
	return 0;
}
M_CAPI(DWORD) MPERF_GetProcessDiskRate(HANDLE hProcess, MPerfAndProcessData*data)
{
	if (hProcess && data)
	{
		IO_COUNTERS io_counter;
		if (GetProcessIoCounters(hProcess, &io_counter))
		{
			ULONGLONG outRead = io_counter.ReadTransferCount - data->LastRead;
			ULONGLONG outWrite = io_counter.WriteTransferCount - data->LastWrite;

			data->LastRead = io_counter.ReadTransferCount;
			data->LastWrite = io_counter.WriteTransferCount;

			return static_cast<DWORD>((outRead + outWrite) / 1024);
		}
	}
	return 0;
}

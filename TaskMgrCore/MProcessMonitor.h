#pragma once
#include "stdafx.h"
#include "prochlp.h"
#include <list>
#include <vector>
#include "MProcessPerformanctMonitor.h"

//无效进程删除回调
typedef void(*ProcessMonitorRemoveItemCallBack)(DWORD pid);
//新进程添加回调
typedef void(*ProcessMonitorNewItemCallBack)(DWORD pid, DWORD parentid, LPWSTR exename, LPWSTR exefullpath, HANDLE hProcess, PMPROCESS_ITEM processItem);
//刷新没有的进程回调
//    返回：返回 FALSE 则发送 NewItemCallBack
typedef BOOL(*ProcessMonitorUpdateNotIncludeItemCallBack)(DWORD pid);

//进程监视器
class M_API MProcessMonitor
{	
protected:
	MProcessMonitor();
public:
	virtual ~MProcessMonitor();

	//枚举所有进程
	virtual bool EnumAllProcess() { return false; };
	//刷新进程
	virtual bool RefeshAllProcess() { return false; };
	//刷新未存在的进程
	virtual bool RefeshAllProcessNotInclude() { return false; };

	//Static fun

	//创建 ProcessMonitor ，请不要直接使用 new MProcessMonitor，请使用此方法创建 MProcessMonitor
	//    removeItemCallBack：删除无效进程回调
	//    newItemCallBack：新进程出现回调
	//    updateNotIncludeItemCallBack：可选，RefeshAllProcessNotInclude使用此回调
	static MProcessMonitor *CreateProcessMonitor(ProcessMonitorRemoveItemCallBack removeItemCallBack, ProcessMonitorNewItemCallBack newItemCallBack, ProcessMonitorUpdateNotIncludeItemCallBack updateNotIncludeItemCallBack);
	//删除 ProcessMonitor，对于使用 CreateProcessMonitor 返回的 MProcessMonitor 请使用此方法释放
	static void DestroyProcessMonitor(MProcessMonitor *monitor);
	
	static BOOL EnumAllProcess(MProcessMonitor *monitor);
	static BOOL RefeshAllProcess(MProcessMonitor *monitor);
	static BOOL RefeshAllProcessNotInclude(MProcessMonitor *monitor);
};

typedef struct tag_PID_ITEM {

}PID_ITEM,*PPID_ITEM;
typedef struct tag_STG_PID_ITEM {
	DWORD Pid;
	struct tag_STG_PID_ITEM*Next;
}STG_PID_ITEM, *PSTG_PID_ITEM;

//进程监视器 Internal
class MProcessMonitorCore : public MProcessMonitor
{
public:
	MProcessMonitorCore(ProcessMonitorRemoveItemCallBack removeItemCallBack, ProcessMonitorNewItemCallBack newItemCallBack, ProcessMonitorUpdateNotIncludeItemCallBack updateNotIncludeItemCallBack);
	~MProcessMonitorCore();

	//枚举所有进程
	bool EnumAllProcess() override;
	//刷新进程
	bool RefeshAllProcess() override;
	//刷新未存在的进程
	bool RefeshAllProcessNotInclude() override;
private:
	//释放进程数据
	void FreeProcessBuffer();
	//清空有效PID
	void ClearVaildPids();

	//刷新有效PID
	void RefreshVaildPids();
	//刷新进程数据
	bool RefreshProcessBuffer();
	//刷新所有进程项目
	void RefreshAllProcessItem();

	void AddValidPid(DWORD pid);
	void RemoveValidPid(DWORD pid);

	//有效的PID
	STG_PID_ITEM validPids = { 0 };
	PSTG_PID_ITEM validPidsEnd;
	//所有进程项目
	PSYSTEM_PROCESSES* processesStorage = NULL;
	DWORD processCount = 0;

	PSYSTEM_PROCESSES FindProcess(DWORD pid);
	//清空所有进程项目
	VOID FreeAllProcessItems();

	MPROCESS_ITEM allProcessItems = { 0 };
	PMPROCESS_ITEM allProcessItemsEnd = NULL;

	CRITICAL_SECTION cs;

	//回调
	ProcessMonitorRemoveItemCallBack RemoveItemCallBack;
	ProcessMonitorNewItemCallBack NewItemCallBack;
	ProcessMonitorUpdateNotIncludeItemCallBack UpdateNotIncludeItemCallBack;

	PSYSTEM_PROCESSES currentProcessBuffer = NULL;
};
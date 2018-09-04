#pragma once
#ifdef FILEACCESS_USER
#include <Windows.h>

typedef struct _CLIENT_ID
{
	PVOID UniqueProcess;
	PVOID UniqueThread;
} CLIENT_ID, *PCLIENT_ID;

#else
#include "..\FileAccessKrnl\driver.h"
#endif

//文件夹保护类型
typedef enum MFS_PROTECT_TYPE{
	PROTECT_TYPE_DIR_ONLY,//仅此目录
    PROTECT_TYPE_DIR_AND_CHILD_DIR,//包括此目录以及其子目录
	PROTECT_TYPE_DIR_ALL,//包括此目录以及其子目录以及其所有文件
};
//保护等级
typedef enum MFS_PROTECT_LEVEL {
	PROTECT_LEVEL_NOACCESS,//拒绝访问
	PROTECT_LEVEL_ONLY_READ,//只读
	PROTECT_LEVEL_ASK_FOR_PERMIT,//请求许可，成功后会保存许可
	PROTECT_LEVEL_ONLY_LOG,//记录
};
//许可请求类型
typedef enum MFS_PERMIT_TYPE {
	PERMIT_REQUEST_AS_USER,//对用户进行许可
	PERMIT_REQUEST_FOR_APPLICATION,//对应用进行许可
	PERMIT_REQUEST_EVERY_TIME,//每次都要请求
};

typedef struct tag_MFS_THREAD
{
	struct tag_MFS_THREAD*GlNext;
	struct tag_MFS_THREAD*GlPrv;
	struct tag_MFS_THREAD*Next;
	struct tag_MFS_THREAD*Prv;

	CLIENT_ID ThreadId;
}MFS_THREAD,*PMFS_THREAD;
//程序令牌
typedef struct tag_MFS_APP_TOKEN 
{
	struct tag_MFS_APP_TOKEN*Next;
	struct tag_MFS_APP_TOKEN*Prv;

	WCHAR Path[260];//路径
	PMFS_THREAD Threads;//线程暂存

}MFS_APP_TOKEN,*PMFS_APP_TOKEN;

//保护
typedef struct tag_MFS_PROTECT
{
	struct tag_MFS_PROTECT*Next;
	struct tag_MFS_PROTECT*Prv;

	BOOLEAN IsDirectory;
	WCHAR Path[260];//路径
	MFS_PROTECT_TYPE ProtectType;//文件夹保护类型
	MFS_PROTECT_LEVEL ProtectLevel = PROTECT_LEVEL_ASK_FOR_PERMIT;//保护等级	
	MFS_PERMIT_TYPE PermitRequsetType;//许可请求类型

	BOOLEAN AllowedUser;//用户是否许可
	PMFS_APP_TOKEN AllowedApplication;//许可的程序

}MFS_PROTECT,*PMFS_PROTECT;
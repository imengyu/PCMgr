#pragma once
#ifdef FILEACCESS_USER
#include <Windows.h>
#else
#endif

typedef struct tag_MFS_INIT_USER_EVENTS {
	HANDLE UserReqPermitCallBackEvent;

}MFS_INIT_USER_EVENTS,*PMFS_INIT_USER_EVENTS;
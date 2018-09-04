#include "fsppvk.h"

PMFS_PROTECT Protects = NULL;
PMFS_THREAD ThreadsStorage = NULL;

VOID KxInitPkv() 
{
	Protects = (PMFS_PROTECT)ExAllocatePool(NonPagedPool, sizeof(PMFS_PROTECT));
	memset(Protects, 0, sizeof(PMFS_PROTECT));

	ThreadsStorage = (PMFS_THREAD)ExAllocatePool(NonPagedPool, sizeof(PMFS_THREAD));
	memset(ThreadsStorage, 0, sizeof(PMFS_THREAD));


}
VOID KxUnInitPkv()
{
	PMFS_PROTECT ptr = Protects;
	if (ptr->Next != NULL) {
		do {
			PMFS_PROTECT ptr_next = ptr->Next;
			ExFreePool(ptr);
			ptr = ptr_next;
		} while (ptr != NULL);
	}
	else ExFreePool(ptr);

	PMFS_THREAD ptrTs = ThreadsStorage;
	if (ptrTs->GlNext != NULL) {
		do {
			PMFS_THREAD ptr_next = ptrTs->GlNext;
			ExFreePool(ptrTs);
			ptrTs = ptr_next;
		} while (ptrTs != NULL);
	}
	else ExFreePool(ptrTs);
}

BOOLEAN KxIsPathInProtectList(LPCWSTR path, PMFS_PROTECT *outProtect) {
	PMFS_PROTECT ptr = Protects;
	if (ptr->Next != NULL) {
		do {
			PMFS_PROTECT ptr_next = ptr->Next;
			if (wcscmp(ptr->Path, path) == 0) {
				if (outProtect)
					*outProtect = ptr;
				return TRUE;
			}
			ptr = ptr_next;
		} while (ptr != NULL);
	}
	return FALSE;
}
BOOLEAN KxIsPathInProtect(PUNICODE_STRING path, BOOLEAN isDir, PMFS_PROTECT *outProtect)
{
	if (path == NULL || path->Buffer == NULL)
		return FALSE;

	PMFS_PROTECT ptr = Protects;
	if (ptr->Next != NULL) 
	{
		do {
			PMFS_PROTECT ptr_next = ptr->Next;
			size_t len = wcslen(ptr->Path);
			if (isDir)//目标操作路径是目录
			{				
				//完全比较
				int cmp2 = wcscmp(ptr->Path, path->Buffer);
				if (cmp2 == 0)
				{
					*outProtect = ptr;
					return TRUE;
				}
				//部分比较
				int cmp1 = wcsncmp(ptr->Path, path->Buffer, len);
				if (cmp1 == 0)
				{
					if (cmp2 != 0 && ptr->ProtectType == PROTECT_TYPE_DIR_ONLY)
						continue;//只有保护目录下所有文件或文件夹才检测，这里返回

					*outProtect = ptr;
					return TRUE;
				}
			}
			else if(ptr->IsDirectory && ptr->ProtectType == PROTECT_TYPE_DIR_ALL)//只有保护目录下所有文件才检测
			{
				//目标操作路径是文件
				if (wcsncmp(ptr->Path, path->Buffer, len) == 0)
				{
					*outProtect = ptr;
					return TRUE;
				}
			}
			else if (wcscmp(ptr->Path, path->Buffer) == 0)
			{
				*outProtect = ptr;
				return TRUE;
			}		

			ptr = ptr_next;
		} while (ptr != NULL);
	}
	return FALSE;
}

BOOLEAN KxAddProtect(LPCWSTR path)
{
	if (!KxIsPathInProtectList(path, NULL))
	{

	}
	return FALSE;
}
BOOLEAN KxRemoveProtect(LPCWSTR path)
{
	PMFS_PROTECT protect = NULL;
	if (KxIsPathInProtectList(path, &protect) && protect)
	{
		if (protect->Prv) protect->Prv->Next = protect->Next;
		if (protect->Next) protect->Next->Prv = protect->Prv;

		ExFreePool(protect);
	}
	return FALSE;
}



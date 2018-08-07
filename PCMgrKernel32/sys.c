#include "sys.h"


VOID KxForceReBoot(void)
{
	//重启计算机(强制)
	typedef void(__fastcall*FCRB)(void);
	/*
	mov al,0FEH
	out 64h,al
	ret
	*/
	FCRB fcrb = NULL;
	UCHAR shellcode[] = "\xB0\xFE\xE6\x64\xC3";
	fcrb = (FCRB)ExAllocatePool(NonPagedPool, sizeof(shellcode));
	memcpy(fcrb, shellcode, sizeof(shellcode));
	fcrb();
	return;
}
VOID KxForceShutdown(void)
{
	//关闭计算机(强制)
	typedef void(__fastcall*FCRB)(void);

	/*
	mov ax,2001h
	mov dx,1004h
	out dx,ax
	retn
	*/
	FCRB fcrb = NULL;
	UCHAR shellcode[] = "\x66\xB8\x01\x20\x66\xBA\x04\x10\x66\xEF\xC3";
	fcrb = (FCRB)ExAllocatePool(NonPagedPool, sizeof(shellcode));
	memcpy(fcrb, shellcode, sizeof(shellcode));
	fcrb();

}

BOOLEAN KxDasm(ULONG_PTR address, ULONG_PTR offest, PUCHAR buf) 
{
	PUCHAR Scan = (PUCHAR)address;
	ULONG_PTR curcodeptr = address + offest;
	if (MmIsAddressValid((PVOID)curcodeptr) != FALSE)
	{
		*buf = Scan[offest];
		return TRUE;
	}
	return FALSE;
}
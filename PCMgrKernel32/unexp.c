#include "unexp.h"

extern PsResumeProcess_ _PsResumeProcess;

PspTerminateThreadByPointer_ PspTerminateThreadByPointer = 0;
PspExitThread_ PspExitThread = 0;
PsGetNextProcessThread_ PsGetNextProcessThread = 0;
PsTerminateProcess_ PsTerminateProcess = 0;
PsGetNextProcess_ PsGetNextProcess = 0;
KeForceResumeThread_ KeForceResumeThread = 0;

//搜索地址
ULONG_PTR KxSearchFeatureCodeForAddress(ULONG_PTR StartAddress, PUCHAR FeatureCode, int FeatureCodeSize, int Search_MaxLength)
{
	PUCHAR Start_scan = (PUCHAR)StartAddress;
	BOOLEAN IsTrue = FALSE;
	ULONG_PTR Real_address = 0;
	int i, fi;

	for (i = 0; i < Search_MaxLength; i++)
	{
		if (Start_scan[i] == FeatureCode[0])
		{
			for (fi = 0; fi < FeatureCodeSize; fi++)
			{
				if (Start_scan[i + fi] != FeatureCode[fi])
				{
					//任何一个字节不同,都标记失败,跳出
					IsTrue = FALSE;
					break;
				}
				else
				{
					if (fi == FeatureCodeSize - 1)
					{
						IsTrue = TRUE;
						//指定长度的字节都相同.认为找到了
					}
				}
			}

			if (IsTrue == TRUE)
			{
				Real_address = (ULONG_PTR)&Start_scan[i];
				break;
			}
		}
	}
	return Real_address;
}
//获取 PspTerminateThreadByPointer 32/64 所有Win
//0 : 32 0x50 64 0xA0
//0x50,0xe8 (push eax call XXXX) / 0xA0,0xe8 (mov r8b call XXXX)
ULONG_PTR KxGetPspTerminateThreadByPointerAddressX_7Or8Or10(UCHAR FeatureCode0)
{
	//摘抄的代码
	UNICODE_STRING  FunName;
	UCHAR FeatureCode[2] = { 0x00,0xe8 };//特征码
	FeatureCode[0] = FeatureCode0;
	ULONG_PTR   FeatureAddress = 0; //找到的特征码的首地址
	ULONG_PTR   FunAddress = 0;
	ULONG_PTR   RetAddress = 0;//最终返回
	ULONG_PTR   FunAdd = 0;

	RtlInitUnicodeString(&FunName, L"PsTerminateSystemThread");
	FunAdd = (ULONG_PTR)MmGetSystemRoutineAddress(&FunName);

	//首先获取PsTerminateSystemThread这个函数地址.其实这个就是直接调用的PspTerminateThreadByPointer
	//这个函数本身非常短小,所以通过搜索特征.定位call 然后将call的地址拿出来并进行运算即可
	FeatureAddress = KxSearchFeatureCodeForAddress(FunAdd, FeatureCode, 2, 0x27);
	//PsTerminateSystemThread函数需要搜索的长度0x27
	if (FeatureAddress != 0) {
		RtlCopyMemory(&FunAddress, (ULONG_PTR*)(FeatureAddress + 2), 4);

		//目标地址=下条指令的地址+机器码E8后面所跟的32位数
		//注意，机器码E8后面所跟的32位数 是 little-endian 格式的，低8位在前，高8位在后
		RetAddress = FunAddress + (FeatureAddress + 2 + 4);
	}
	return RetAddress;
}

//获取 PspExitThread 32/64 所有Win
ULONG_PTR KxGetPspExitThread_32_64() {
	ULONG_PTR i = 0;
	ULONG_PTR callcode = 0;
	ULONG_PTR  curcodeptr = 0;
	ULONG_PTR RetAddress = 0;
	ULONG_PTR AddressPTTBP = (ULONG_PTR)PspTerminateThreadByPointer;//PspTerminateThreadByPointer地址
	PUCHAR Scan = (PUCHAR)AddressPTTBP;

	if (AddressPTTBP != 0) 
	{
		//差不多搜索0x60吧
		for (i = 1; i < 0x60; i++)
		{
			curcodeptr = (ULONG_PTR)&Scan[i];
			if (MmIsAddressValid((PVOID)curcodeptr))
			{
				if (Scan[i] == 0xe8/*call immed16 定位位置 (一共 5 字节) */
					&& Scan[i + 5] == 0xcc/*int 3*/)//经过反汇编，每个系统call PspExitThread后面都是int 3，
					//而前面的指令不一样，，所以使用此方法最保险
				{
					//目标地址=机器码+5+原始地址
					RtlMoveMemory(&callcode, (PVOID)(curcodeptr + 1), 4);//跳转地址
					RetAddress = (ULONG_PTR)((curcodeptr/*机器码*/ + 5 + callcode) & 0x0000ffffffff);
					break;
				}
			}
		}
	}
	else KdPrint(("AddressPTTBP == 0!"));
	return RetAddress;
}

//搜索 PsGetNextProcessThread X
ULONG_PTR KxGetPsGetNextProcessThread_32Or64_X(ULONG TWO_CALL_OFF)
{
	ULONG_PTR RetAddress = 0;
	ULONG_PTR PsResumeProcessAddress = (ULONG_PTR)_PsResumeProcess;//在PsResumeProcess搜索

	ULONG_PTR i = 0;
	ULONG_PTR callcode = 0;
	ULONG_PTR  curcodeptr = 0;

	PUCHAR Scan = (PUCHAR)PsResumeProcessAddress;

	//搜索0x50
	for (i = 1; i < 0x50; i++)
	{
		if (MmIsAddressValid((PVOID)(PsResumeProcessAddress + i)) && MmIsAddressValid((PVOID)(ULONG_PTR)(PsResumeProcessAddress + i + TWO_CALL_OFF)))
		{
			curcodeptr = (ULONG_PTR)&Scan[i];
			if (Scan[i] == 0xe8/*call immed16*/
				&& Scan[i + TWO_CALL_OFF] == 0xe8/*call immed16*/)
			{
				curcodeptr = (ULONG_PTR)&Scan[i + TWO_CALL_OFF];
				//目标地址=机器码+5+原始地址
				RtlMoveMemory(&callcode, (PVOID)(curcodeptr + 1), 4);//跳转地址
				RetAddress = (ULONG_PTR)((curcodeptr/*机器码*/ + 5 + callcode) & 0x0000ffffffff);
				break;
			}
		}
	}

	return RetAddress;
}

//获取一些未导出函数
NTSTATUS KxGetFunctions(PWINVERS parm)
{
	NTSTATUS status = STATUS_SUCCESS;

#ifdef _AMD64_
	PspTerminateThreadByPointer = (PspTerminateThreadByPointer_)KxGetPspTerminateThreadByPointerAddressX_7Or8Or10(0xA0);
#else
	PspTerminateThreadByPointer = (PspTerminateThreadByPointer_)KxGetPspTerminateThreadByPointerAddressX_7Or8Or10(0x50);
#endif

	PspExitThread = (PspExitThread_)KxGetPspExitThread_32_64();

	if (parm->VerSimple == 7)//Win7
	{
#ifdef _AMD64_//两个CALL 的间隔
		PsGetNextProcessThread = (PsGetNextProcessThread_)KxGetPsGetNextProcessThread_32Or64_X(0xB);
#else
		PsGetNextProcessThread = (PsGetNextProcessThread_)KxGetPsGetNextProcessThread_32Or64_X(0x9);
#endif
		
	}
	else if (parm->VerSimple == 10)//Win10
	{
#ifdef _AMD64_//两个CALL 的间隔
		PsGetNextProcessThread = (PsGetNextProcessThread_)KxGetPsGetNextProcessThread_32Or64_X(0xE);
#else
		PsGetNextProcessThread = (PsGetNextProcessThread_)KxGetPsGetNextProcessThread_32Or64_X(0x9);
#endif
	}
	else if (parm->VerSimple == 8)//Win8
	{

	}
	else if (parm->VerSimple == 81)//Win8.1
	{

	}

	KxPrintInternalFuns();

	return status;
}
//从pdb获取一些未导出函数
VOID KxGetFunctionsFormPDBData(PNTOS_PDB_DATA data) {
	PspExitThread = (PspExitThread_)data->PspExitThread_;
	PspTerminateThreadByPointer = (PspTerminateThreadByPointer_)data->PspTerminateThreadByPointer_;
	PsGetNextProcessThread = (PsGetNextProcessThread_)data->PsGetNextProcessThread_;
	PsGetNextProcess = (PsGetNextProcess_)data->PsGetNextProcess_;
	PsTerminateProcess = (PsTerminateProcess_)data->PsTerminateProcess_;
	KeForceResumeThread = (KeForceResumeThread_)data->KeForceResumeThread_;

	KxPrintInternalFuns();
}

ULONG_PTR EPROCESS_ThreadListHead_Offest;
ULONG_PTR EPROCESS_RundownProtect_Offest;
ULONG_PTR EPROCESS_Flags_Offest;
ULONG_PTR ETHREAD_Tcb_Offest;
ULONG_PTR ETHREAD_CrossThreadFlags_Offest;

VOID KxGetStructOffestsFormPDBData(PNTOS_EPROCESS_OFF_DATA data) {

	EPROCESS_ThreadListHead_Offest = data->EPROCESS_ThreadListHeadOffest;
	EPROCESS_RundownProtect_Offest = data->EPROCESS_RundownProtectOffest;
	EPROCESS_Flags_Offest = data->EPROCESS_FlagsOffest;
	ETHREAD_Tcb_Offest = data->ETHREAD_TcbOffest;
	ETHREAD_CrossThreadFlags_Offest = data->ETHREAD_CrossThreadFlagsOffest;
	KxPrintInternalOffests();
}

VOID KxPrintInternalFuns() 
{
#ifdef _AMD64_
	KdPrint(("PspTerminateThreadByPointer : 0x%I64x", PspTerminateThreadByPointer));
	KdPrint(("PspExitThread : 0x%I64x", PspExitThread));
	KdPrint(("PsGetNextProcessThread : 0x%I64x", PsGetNextProcessThread));
	KdPrint(("PsTerminateProcess : 0x%I64x", PsTerminateProcess));
	KdPrint(("PsGetNextProcess : 0x%I64x", PsGetNextProcess));
	KdPrint(("KeForceResumeThread : 0x%I64x", KeForceResumeThread));
#else
	KdPrint(("PspTerminateThreadByPointer : 0x%08x", PspTerminateThreadByPointer));
	KdPrint(("PspExitThread : 0x%08x", PspExitThread));
	KdPrint(("PsGetNextProcessThread : 0x%08x", PsGetNextProcessThread));
	KdPrint(("PsTerminateProcess : 0x%08x", PsTerminateProcess));
	KdPrint(("PsGetNextProcess : 0x%08x", PsGetNextProcess));
	KdPrint(("KeForceResumeThread : 0x%08x", KeForceResumeThread));
#endif
}
VOID KxPrintInternalOffests()
{
#ifdef _AMD64_
	KdPrint(("_EPROCESS->ThreadListHead : +0x%I64x", EPROCESS_ThreadListHead_Offest));
	KdPrint(("_EPROCESS->RundownProtect : +0x%I64x", EPROCESS_RundownProtect_Offest));
	KdPrint(("_EPROCESS->Flags : +0x%I64x", EPROCESS_Flags_Offest));
	KdPrint(("_ETHREAD->Tcb : +0x%I64x", ETHREAD_Tcb_Offest));
	KdPrint(("_ETHREAD->CrossThreadFlags : +0x%I64x", ETHREAD_CrossThreadFlags_Offest));
#else
	KdPrint(("_EPROCESS->ThreadListHead : +0x%08x", EPROCESS_ThreadListHead_Offest));
	KdPrint(("_EPROCESS->RundownProtect : +0x%08x", EPROCESS_RundownProtect_Offest));
	KdPrint(("_EPROCESS->Flags : +0x%08x", EPROCESS_Flags_Offest));
	KdPrint(("_ETHREAD->Tcb : +0x%08x", ETHREAD_Tcb_Offest));
	KdPrint(("_ETHREAD->CrossThreadFlags : +0x%08x", ETHREAD_CrossThreadFlags_Offest));
#endif
}



//根据传入版本选择一些结构的偏移
NTSTATUS KxLoadStructOffests(PWINVERS parm)
{
	NTSTATUS status = STATUS_SUCCESS;
	if (parm->VerSimple == 10) {//win10
#ifdef _AMD64_
		EPROCESS_ThreadListHead_Offest = 0x030;//kprocess 10-17134
		EPROCESS_RundownProtect_Offest = 0x2f8;
#else
		//EPROCESS_RundownProtect_Offest = 0x000;
#endif
	}
	else if (parm->VerSimple == 81)//Win8.1
	{

	}
	else if (parm->VerSimple == 8)//Win8
	{

	}
	else if (parm->VerSimple == 7)//Win7
	{

#ifdef _AMD64_
		//EPROCESS_RundownProtect_Offest = 0x000;
#else
		EPROCESS_RundownProtect_Offest = 0x0b0;
#endif
		
	}
	KxPrintInternalOffests();
	return status;
}
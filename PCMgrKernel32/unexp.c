#include "unexp.h"

PspTerminateThreadByPointer_ PspTerminateThreadByPointer;
PspExitThread_ PspExitThread;

ULONG KxGetWin10Ver() {
	return 0;
}
NTSTATUS KxGetFunctions(ULONG parm)
{
	NTSTATUS status = STATUS_SUCCESS;

	if (parm == 7 || parm == 8) {
		PspTerminateThreadByPointer = (PspTerminateThreadByPointer_)KxGetPspTerminateThreadByPointerAddress78();
		PspExitThread = (PspExitThread_)KxGetPspExitThreadAddress78();

		KdPrint(("PspTerminateThreadByPointer : 0x%08x", PspTerminateThreadByPointer));
		KdPrint(("PspExitThread : 0x%08x", PspExitThread));
	}
	else {
		ULONG w10ver = KxGetWin10Ver();
		KdPrint(("Win10 Ver : %d", w10ver));


	}

	return status;
}

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
ULONG_PTR KxGetPspTerminateThreadByPointerAddress78()
{
	UNICODE_STRING  FunName;
	UCHAR FeatureCode[2] = { 0x50,0xe8 };//特征码push    eax call XXXX
	ULONG_PTR   FeatureAddress = 0; //找到的特征码的首地址
	ULONG_PTR   FunAddress = 0;
	ULONG_PTR   RetAddress = 0;//最终返回
	ULONG_PTR   FunAdd = 0;

	RtlInitUnicodeString(&FunName, L"PsTerminateSystemThread");
	FunAdd = (ULONG_PTR)MmGetSystemRoutineAddress(&FunName);

	KdPrint(("PsTerminateSystemThread : %08X", FunAdd));
	//首先获取PsTerminateSystemThread这个函数地址.其实这个就是直接调用的PspTerminateThreadByPointer
	//这个函数本身非常短小,所以通过搜索特征.定位call 然后将call的地址拿出来并进行运算即可

	FeatureAddress = KxSearchFeatureCodeForAddress(FunAdd, FeatureCode, 2, 0x27);
	//PsTerminateSystemThread函数需要搜索的长度0x27

	KdPrint(("FeatureAddress : %08X", FeatureAddress));

	RtlCopyMemory(&FunAddress, (ULONG_PTR*)(FeatureAddress + 2), 4);

	//目标地址=下条指令的地址+机器码E8后面所跟的32位数
	//注意，机器码E8后面所跟的32位数 是 little-endian 格式的，低8位在前，高8位在后
	RetAddress = FunAddress + (FeatureAddress + 2 + 4);

	return RetAddress;
}
ULONG_PTR KxGetPspExitThreadAddress78()
{
	ULONG_PTR Address = 0; //找到的特征码的首地址
	ULONG_PTR PTTBPAddress = (ULONG_PTR)PspTerminateThreadByPointer;
	ULONG_PTR RetAddress = 0;//最终返回
	if (PTTBPAddress != 0) {
		for (int i = 1; i < 0xff; i++)
		{
			if (MmIsAddressValid((PVOID)(PTTBPAddress + i)) != FALSE)
			{
				//目标地址-原始地址-5=机器码
				//目标地址=机器码+5+原始地址
				if (*(BYTE *)(PTTBPAddress + i + 1) == 0xCC)//int 3
				{
					RtlMoveMemory(&Address, (PVOID)(PTTBPAddress + i), 4);
					RetAddress = (ULONG_PTR)Address + 5 + PTTBPAddress + i;
				}
			}
		}
	}
	return RetAddress;
}

ULONG_PTR KxGetPspTerminateThreadByPointerAddress10(ULONG ver)
{
	return 0;
}
ULONG_PTR KxGetPspExitThreadAddress10(ULONG ver)
{
	return 0;
}
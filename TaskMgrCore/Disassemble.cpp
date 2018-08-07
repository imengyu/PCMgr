/*

		Written by AlexLong

		Bug Report: 
		Email:	33445621@163.com
		MSN:	33445621@163.com

		All rights reserved
*/

#include "stdafx.h"
#include "Disassemble.h"

/************************************************************************/
/* 1个opcode指令表                                                      */
/************************************************************************/
DWORD OneOpCodeMapTable[256]=
{
	/*		0			1			2			3			4							5			6			7			8				9			A			B			C			D			E			F		*/
	/*0*/	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,						Imm66,		OneByte,	OneByte,	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,		Imm66,		OneByte,	TwoOpCode0F,		
	/*1*/	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,						Imm66,		OneByte,	OneByte,	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,		Imm66,		OneByte,	OneByte,		
	/*2*/	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,						Imm66,		PreSegment,	OneByte,	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,		Imm66,		PreSegment,	OneByte,		
	/*3*/	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,						Imm66,		PreSegment,	OneByte,	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,		Imm66,		PreSegment,	OneByte,		
	/*4*/	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,					OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,		
	/*5*/	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,					OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,		
	/*6*/	OneByte,	OneByte,	ModRM+Mxx,	ModRM,		PreSegment,					PreSegment,		PreOperandSize66,PreAddressSize67,Imm66,Imm66+ModRM,Imm8,	Imm8+ModRM,	OneByte+StringInstruction,	OneByte+StringInstruction,	OneByte+StringInstruction,	OneByte+StringInstruction,
	/*7*/	Imm8,		Imm8,		Imm8,		Imm8,		Imm8,						Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,
	/*8*/	Group,		Group,		Group,		Group,		ModRM,						ModRM,ModRM,ModRM,	ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM+Mxx,		ModRM,		Group,
	/*9*/	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,					OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	Imm16+Imm66,OneByte,	OneByte,	OneByte,	OneByte,	OneByte,
	/*A*/	Addr67,		Addr67,		Addr67,		Addr67,		OneByte+StringInstruction,	OneByte+StringInstruction,	OneByte+StringInstruction,	OneByte+StringInstruction,	Imm8,		Imm66,		OneByte+StringInstruction,	OneByte+StringInstruction,	OneByte+StringInstruction,	OneByte+StringInstruction,	OneByte+StringInstruction,	OneByte+StringInstruction,
	/*B*/	Imm8,		Imm8,		Imm8,		Imm8,		Imm8,						Imm8,		Imm8,		Imm8,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,
	/*C*/	Group,		Group,		Imm16,		OneByte,	ModRM+Mxx,					ModRM+Mxx,		Group,Group,Imm16+Imm8,OneByte,Imm16,OneByte,	OneByte,	Imm8,		OneByte,	OneByte,
	/*D*/	Group,		Group,		Group,		Group,		Imm8,						Imm8,		OneByte,	OneByte,	FPUOpCode,		FPUOpCode,		FPUOpCode,		FPUOpCode,		FPUOpCode,		FPUOpCode,		FPUOpCode,		FPUOpCode,
	/*E*/	Imm8,		Imm8,		Imm8,		Imm8,		Imm8,						Imm8,		Imm8,		Imm8,		Imm66,		Imm66,		Imm66+Imm16,Imm8,		OneByte,	OneByte,	OneByte,	OneByte,
	/*F*/	PreLockF0,	OneByte,	PreRep,		PreRep,		OneByte,					OneByte,	Group,Group,OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	Group,Group
};

/************************************************************************/
/* 2个opcode指令表                                                      */
/************************************************************************/
DWORD TwoOpCodeMapTable[256]=
{
	/*		0			1			2			3			4			5			6			7			8				9			A			B			C			D			E			F		*/
	/*0*/	Group,Group,ModRM,		ModRM,		Reserved,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	Reserved,	OneByte,	Reserved,	ModRM,		Reserved,	Reserved,		
	/*1*/	ModRM,		ModRM,		ModRM+Mxx+Uxx,		ModRM+Mxx+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+Uxx+Mxx+MustNoF2,		ModRM+Mxx+MustNoF2+MustNoF3,		Group,Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	ModRM,		
	/*2*/	ModRM+Rxx,		ModRM+Rxx,		ModRM+Rxx,		ModRM+Rxx,		Reserved,	Reserved,	Reserved,	Reserved,	ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM,		ModRM+Mxx,		ModRM,		ModRM,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,
	/*3*/	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	Reserved,	Reserved,	ThreeOpCode0F38,Reserved,ThreeOpCode0F3A,ModRM,	ModRM,		ModRM,		ModRM,		ModRM,
	/*4*/	ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,
	/*5*/	ModRM+Uxx+MustNoF2+MustNoF3,	ModRM,		ModRM+MustNo66+MustNoF2,		ModRM+MustNo66+MustNoF2,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM,		ModRM,		ModRM,		ModRM+MustNoF2,		ModRM,		ModRM,		ModRM,		ModRM,
	/*6*/	ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3+MustHave66,		ModRM+MustNoF2+MustNoF3+MustHave66,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2,
	/*7*/	ModRM+Imm8,	Group,		Group,		Group,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNo66+MustNoF2+MustNoF3,ModRM+MustNo66+MustNoF2+MustNoF3,		ModRM+MustNo66+MustNoF2+MustNoF3,		Reserved,	Reserved,	ModRM+MustNoF3+MustHavePrefix,		ModRM+MustNoF3+MustHavePrefix,		ModRM+MustNoF2,		ModRM+MustNoF2,
	/*8*/	Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,
	/*9*/	ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,
	/*A*/	OneByte,	OneByte,	OneByte,	ModRM,		ModRM+Imm8,	ModRM,		Reserved,	Reserved,	OneByte,	OneByte,	OneByte,	ModRM,		ModRM+Imm8,	ModRM,		Group,ModRM,
	/*B*/	ModRM,		ModRM,		ModRM+Mxx,		ModRM,		ModRM+Mxx,		ModRM+Mxx,		ModRM,		ModRM,		Reserved+MustNo66,	Group,Group,ModRM,ModRM,		ModRM,		ModRM,		ModRM,
	/*C*/	ModRM+MustNo66+MustNoF2+MustNoF3,		ModRM+MustNo66+MustNoF2+MustNoF3,		ModRM+Imm8,	ModRM+Mxx+MustNo66+MustNoF2+MustNoF3,		ModRM+Rxx+Mxx+Imm8+MustNoF2+MustNoF3,	ModRM+Imm8+Uxx+Nxx+MustNoF2+MustNoF3,	ModRM+Imm8+MustNoF2+MustNoF3,	ModRM+Group,OneByte+MustNo66,	OneByte+MustNo66,	OneByte+MustNo66,	OneByte+MustNo66,	OneByte+MustNo66,	OneByte+MustNo66,	OneByte+MustNo66,	OneByte+MustNo66,
	/*D*/	ModRM+MustNoF3+MustHavePrefix,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustHavePrefix,		ModRM+Uxx+Nxx+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,
	/*E*/	ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustHavePrefix,		ModRM+Mxx+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,
	/*F*/	ModRM+Mxx+MustHaveF2+MustNo66+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+Uxx+Nxx+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		Reserved,
};

/************************************************************************/
/* 3个opcode指令表A(0F 38)                                              */
/************************************************************************/
DWORD ThreeOpCodeMapTable0F38[256]=
{
	/*		0			1			2			3			4			5			6			7			8				9			A			B			C			D			E			F		*/
	/*0*/	ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		Reserved,	Reserved,	Reserved,	Reserved,		
	/*1*/	ModRM+MustHave66+MustNoF2+MustNoF3,		Reserved,	Reserved,	Reserved,	ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		Reserved,	ModRM+MustHave66+MustNoF2+MustNoF3,		Reserved,	Reserved,	Reserved,	Reserved,	ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		ModRM+MustNoF2+MustNoF3,		Reserved,		
	/*2*/	ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		Reserved,	Reserved,	ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+Mxx+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		Reserved,	Reserved,	Reserved,	Reserved,	
	/*3*/	ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		Reserved,	ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,
	/*4*/	ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*5*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*6*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*7*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*8*/	ModRM+Mxx+MustHave66+MustNoF2+MustNoF3,		ModRM+Mxx+MustHave66+MustNoF2+MustNoF3,		Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*9*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*A*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*B*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*C*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*D*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,		ModRM+MustHave66+MustNoF2+MustNoF3,
	/*E*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*F*/	ModRM+Mxx+MustNoF3,	ModRM+Mxx+MustNoF3,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
};

/************************************************************************/
/* 3个opcode指令表B(0F 3A)                                              */
/************************************************************************/
DWORD ThreeOpCodeMapTable0F3A[256]=
{
	/*		0			1			2			3			4			5			6			7			8				9			A			B			C			D			E			F		*/
	/*0*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustNoF2+MustNoF3+Imm8,
	/*1*/	Reserved,	Reserved,	Reserved,	Reserved,	ModRM+Rxx+Mxx+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+Rxx+Mxx+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*2*/	ModRM+Rxx+Mxx+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+Uxx+Mxx+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*3*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*4*/	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	Reserved,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,		Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*5*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*6*/	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*7*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*8*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*9*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*A*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*B*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*C*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*D*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	ModRM+MustHave66+MustNoF2+MustNoF3+Imm8,
	/*E*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
	/*F*/	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,	Reserved,
};

/************************************************************************/
/* Group指令表,共17组，每组8个                                          */
/************************************************************************/
DWORD GroupOpCodeMapTable[17][8]=
{
	/*					0				1				2				3				4				5			6				7		*/
	/*1  80-83		*/	{ModRM,			ModRM,			ModRM,			ModRM,			ModRM,			ModRM,		ModRM,			ModRM},
	/*1A 8F			*/	{ModRM,			Reserved,		Reserved,		Reserved,		Reserved,		Reserved,	Reserved,		Reserved},
	/*2  C0 C1 D0-D3*/	{ModRM,			ModRM,			ModRM,			ModRM,			ModRM,			ModRM,		ModRM,			ModRM},
	/*3  F6 F7		*/	{ModRM,			Reserved,		ModRM,			ModRM,			ModRM,			ModRM,		ModRM,			ModRM},
	/*4  FE			*/	{ModRM,			ModRM,			Reserved,		Reserved,		Reserved,		Reserved,	Reserved,		Reserved},
	/*5  FF			*/	{ModRM,			ModRM,			ModRM,			ModRM,			ModRM,			ModRM,		ModRM,			Reserved},
	/*6  0F 00		*/	{ModRM+Mxx+Rxx,	ModRM+Mxx+Rxx,	ModRM,			ModRM,			ModRM,			ModRM,		Reserved,		Reserved},
	/*7  0F 01		*/	{ModRM,			ModRM,			ModRM+Mxx,		ModRM+Mxx,		ModRM+Mxx+Rxx,	Reserved,	ModRM,			ModRM+Mxx},
	/*8  0F BA		*/	{Reserved,		Reserved,		Reserved,		Reserved,		ModRM,			ModRM,		ModRM,			ModRM},
	/*9	 0F C7		*/	{Reserved,		ModRM+Mxx,		Reserved,		Reserved,		Reserved,		Reserved,	ModRM+Mxx,		ModRM+Mxx},
	/*10 0F B9		*/	{OneByte,		OneByte,		OneByte,		OneByte,		OneByte,		OneByte,	OneByte,		OneByte},
	/*11 C6 C7		*/	{ModRM,			Reserved,		Reserved,		Reserved,		Reserved,		Reserved,	Reserved,		Reserved},
	/*12 0F 71		*/	{Reserved,		Reserved,		ModRM+Imm8+Uxx,	Reserved,		ModRM+Imm8+Uxx,	Reserved,	ModRM+Imm8+Uxx,	Reserved},
	/*13 0F 72		*/	{Reserved,		Reserved,		ModRM+Imm8+Uxx,	Reserved,		ModRM+Imm8+Uxx,	Reserved,	ModRM+Imm8+Uxx,	Reserved},
	/*14 0F 73		*/	{Reserved,		Reserved,		ModRM+Imm8+Uxx,	ModRM+Imm8+Uxx,	Reserved,		Reserved,	ModRM+Imm8+Uxx,	ModRM+Imm8+Uxx},
	/*15 0F AE		*/	{ModRM+Mxx,		ModRM+Mxx,		ModRM+Mxx,		ModRM+Mxx,		Reserved,		ModRM+Uxx,	ModRM+Uxx,		ModRM},
	/*16 0F 18		*/	{ModRM+Mxx,		ModRM+Mxx,		ModRM+Mxx,		ModRM+Mxx,		Reserved,		Reserved,	Reserved,		Reserved}
};

/************************************************************************/
/* FPU指令表,共40组，每组16个                                          */
/************************************************************************/
DWORD FpuOpCodeMapTable[5*8][16]=
{
	/*D8 ModRM 00-BF*/	{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
	/*D8 ModRM C0-FF*/	{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
	/*D9 ModRM 00-BF*/	{ModRM,		Reserved,	ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
	/*D9 ModRM C0-FF*/	{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{ModRM,		Reserved,	Reserved,	Reserved,	Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
						{ModRM,		ModRM,		Reserved,	Reserved,	ModRM,ModRM,Reserved,Reserved,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,Reserved},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
	/*DA ModRM 00-BF*/	{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
	/*DA ModRM C0-FF*/	{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{Reserved,	Reserved,	Reserved,	Reserved,	Reserved,Reserved,Reserved,Reserved,Reserved,ModRM,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
						{Reserved,	Reserved,	Reserved,	Reserved,	Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
	/*DB ModRM 00-BF*/	{ModRM,		ModRM,		ModRM,		ModRM,		Reserved,ModRM,Reserved,ModRM,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
	/*DB ModRM C0-FF*/	{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{Reserved,	Reserved,	ModRM,		ModRM,		Reserved,Reserved,Reserved,Reserved,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
	/*DC ModRM 00-BF*/	{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
	/*DC ModRM C0-FF*/	{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{Reserved,	Reserved,	Reserved,	Reserved,	Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
	/*DD ModRM 00-BF*/	{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,Reserved,ModRM,ModRM,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
	/*DD ModRM C0-FF*/	{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{Reserved,	Reserved,	Reserved,	Reserved,	Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
	/*DE ModRM 00-BF*/	{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
	/*DE ModRM C0-FF*/	{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{Reserved,	Reserved,	Reserved,	Reserved,	Reserved,Reserved,Reserved,Reserved,Reserved,ModRM,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
	/*DF ModRM 00-BF*/	{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
	/*DF ModRM C0-FF*/	{Reserved,	Reserved,	Reserved,	Reserved,	Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
						{Reserved,	Reserved,	Reserved,	Reserved,	Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved},
						{ModRM,		Reserved,	Reserved,	Reserved,	Reserved,Reserved,Reserved,Reserved,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM,ModRM},
						{ModRM,		ModRM,		ModRM,		ModRM,		ModRM,ModRM,ModRM,ModRM,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved,Reserved}
};		
		
void GetGroupSignature(IN BYTE* &pCurOpcode,IN PINSTRUCTION &pInstruction,OUT DWORD &dwSignature)
{
	BYTE modrm = *pCurOpcode;
	BYTE mod = (modrm >> 6) & 3;
	BYTE regopcode = (modrm >> 3) & 7;
	BYTE rm = modrm & 7;

	switch(pInstruction->Opcode1 == 0x0F ? *(pCurOpcode-2) : *(pCurOpcode-1))	//由于pCurOpcode已经指向modrm，所以如是opcode1表的Group则-1即为opcode，否则就是opcode2表中的Group，所以需要-2,OF XX,*(pCurOpcode-2) == 0x0F
	{
	case 0x80: case 0x82: case 0x83: case 0xC0: case 0xC1: 	//GROUP1 GROUP2,因为0xC0 0xC1的全部标志与GROUP1全部相同，所以写在这了。省地方^_^
		dwSignature = GroupOpCodeMapTable[0][regopcode] + Imm8;
		break;
	case 0x81: 	//GROUP1
		dwSignature = GroupOpCodeMapTable[0][regopcode] + Imm66;
		break;
	case 0x8F:	//GROUP1A
		dwSignature = GroupOpCodeMapTable[1][regopcode];
		break;
	case 0xD0: case 0xD1: case 0xD2: case 0xD3:	//GROUP2
		dwSignature = GroupOpCodeMapTable[2][regopcode];
		break;
	case 0xF6:	//GROUP3
		{
			if (regopcode == 0)
			{
				dwSignature = GroupOpCodeMapTable[3][regopcode] + Imm8;
			}
			else
				dwSignature = GroupOpCodeMapTable[3][regopcode];
		}
		break;
	case 0xF7:	//GROUP3
		{
			if (regopcode == 0)
			{
				dwSignature = GroupOpCodeMapTable[3][regopcode] + Imm66;
			}
			else
				dwSignature = GroupOpCodeMapTable[3][regopcode];
		}
		break;
	case 0xFE:	//GROUP4
		dwSignature = GroupOpCodeMapTable[4][regopcode];
		break;
	case 0xFF:	//GROUP5
		dwSignature = GroupOpCodeMapTable[5][regopcode];
		break;
	case 0xC6:	////GROUP11
		dwSignature = GroupOpCodeMapTable[11][regopcode] + Imm8;
		break;
	case 0xC7:	//GROUP11
		dwSignature = GroupOpCodeMapTable[11][regopcode] + Imm66;
		break;
	case 0x0F:
		{
			switch(*(pCurOpcode-1))
			{
			case 0x00:	//GROUP6
				dwSignature = GroupOpCodeMapTable[6][regopcode];
				break;
			case 0x01:	//GROUP7
				{
					dwSignature = GroupOpCodeMapTable[7][regopcode];
					if (regopcode == 0)		//特殊情况特殊对待
					{
						if (mod == 3)
						{
							if (rm != 1 || rm != 2 || rm != 3 || rm != 4)
							{
								dwSignature = Reserved;
							}
						}
					}
					if (regopcode == 1)
					{
						if (mod == 3)
						{
							if (rm != 0 || rm != 1)
							{
								dwSignature = Reserved;
							}
						}
					}
				}
				break;
			case 0xBA:	//GROUP8
				dwSignature = GroupOpCodeMapTable[8][regopcode];
				break;
			case 0xC7:	//GROUP9
				dwSignature = GroupOpCodeMapTable[9][regopcode];
				break;
			case 0xB9:	//GROUP10
				dwSignature = GroupOpCodeMapTable[10][regopcode];
				break;
			case 0x71:	//GROUP12
				dwSignature = GroupOpCodeMapTable[12][regopcode];
				break;
			case 0x72:	//GROUP13
				dwSignature = GroupOpCodeMapTable[13][regopcode];
				break;
			case 0x73:	//GROUP14
				dwSignature = GroupOpCodeMapTable[14][regopcode];
				break;
			case 0xAE:	//GROUP15
				dwSignature = GroupOpCodeMapTable[15][regopcode];
				break;
			case 0x18:	//GROUP16
				dwSignature = GroupOpCodeMapTable[16][regopcode];
				break;
			}
		}
		break;
	}
}

void GetFPUSignature(IN BYTE* &pCurOpcode,IN PINSTRUCTION &pInstruction,OUT DWORD &dwSignature)
{
	BYTE modrm = *pCurOpcode;
	BYTE regopcode = (modrm >> 3) & 7;
	
	int index = pInstruction->Opcode1 & 0x07;
	int col = (modrm >> 4) & 0x03;
	int row = modrm & 0x0F;
	
	if (modrm < 0xC0)
	{
	//	sprintf(strMnemonic,"%s",FpuMnemonic[5*index][regopcode]);
		dwSignature = FpuOpCodeMapTable[5*index][regopcode];
	}
	else
	{
		//sprintf(strMnemonic,"%s",FpuMnemonic[5*index + col +1][row]);
		dwSignature = FpuOpCodeMapTable[5*index + col +1][row];
	}
}

void DecodeImm66(IN BYTE* &pCurOpcode,OUT PINSTRUCTION &pInstruction,IN DWORD dwSignature)
{
	//存在0x66前缀时
	if (pInstruction->OperandPrefix == 0x66)
	{
		pInstruction->Immediate.ImmWord = *(WORD*)pCurOpcode;
		pCurOpcode+=2;
	}
	else//不存在0x66前缀时
	{
		pInstruction->Immediate.ImmDword = *(DWORD*)pCurOpcode;
		pCurOpcode+=4;
	}
}

void DecodeImm8(IN BYTE* &pCurOpcode,OUT PINSTRUCTION &pInstruction,IN DWORD dwSignature)
{
	pInstruction->Immediate.ImmByte = *pCurOpcode++;
}

/************************************************************************/
/* 处理Imm16的情况，在这里处理了Imm16与Imm8、Imm66重叠情况              */
/************************************************************************/
void DecodeImm16(IN BYTE* &pCurOpcode,OUT PINSTRUCTION &pInstruction,IN DWORD dwSignature)
{
	//处理Imm16与Imm8重叠的时候
	if (dwSignature & Imm8)
	{
		pInstruction->Displacement.DispWord = *(WORD*)pCurOpcode;
		pCurOpcode+=2;
		pInstruction->Immediate.ImmByte = *pCurOpcode++;
	}
	
	//处理Imm16与Imm66重叠的时候
	if (dwSignature & Imm66)
	{
		if (pInstruction->OperandPrefix == 0x66)
		{
			pInstruction->Displacement.DispWord = *(WORD*)pCurOpcode;
			pCurOpcode+=2;
			pInstruction->Immediate.ImmWord = *(WORD*)pCurOpcode;
			pCurOpcode+=2;
		}
		else
		{
			pInstruction->Displacement.DispDword = *(DWORD*)pCurOpcode;
			pCurOpcode+=4;
			pInstruction->Immediate.ImmWord = *(WORD*)pCurOpcode;
			pCurOpcode+=2;
		}
	}
	else //处理不与任何标志重叠的时候
	{
		pInstruction->Immediate.ImmWord = *(WORD*)pCurOpcode;
		pCurOpcode+=2;
	}
}

/************************************************************************/
/* 处理ModRM的情况，在这里处理了ModRM与Imm8、Imm66重叠情况              */
/************************************************************************/
void DecodeModRM(IN BYTE* &pCurOpcode,OUT PINSTRUCTION &pInstruction,IN DWORD dwSignature)
{
	BYTE modrm = *pCurOpcode++;
	BYTE mod = modrm & 0xC0;
	BYTE regopcode = modrm & 0x38;
	BYTE rm = modrm & 0x07;
	pInstruction->Modrm = modrm;

	//最高2位不是11
	if (mod !=0xC0)
	{
		//有0x67前缀
		if (pInstruction->AddressPrefix == 0x67)	//存在0x67即从32位转到16位，查询16位ModRM表
		{
			//mod == 00
			if (mod == 0x00)
			{
				if (rm == 0x06)
				{
					pInstruction->Displacement.DispWord = *(WORD*)pCurOpcode;
					pCurOpcode+=2;
				}
			}
			
			//mod == 01
			if (mod == 0x40)
			{
				pInstruction->Displacement.DispByte = *pCurOpcode++;
			}
			
			//mod ==10
			if (mod == 0x80)
			{
				pInstruction->Displacement.DispWord = *(WORD*)pCurOpcode;
				pCurOpcode+=2;
			}
		}
		else	//没有0x67前缀,查询32位ModRM表
		{
			//mod == 00
			if (mod == 0x00)
			{
				if (rm == 0x04)
				{
					pInstruction->SIB = *pCurOpcode++;
					//处理SIB.Base=101时的情况
					if ((pInstruction->SIB & 0x07) == 0x05)
					{
						pInstruction->Displacement.DispDword = *(DWORD*)pCurOpcode;
						pCurOpcode+=4;
					}
				}
				if (rm == 0x05)
				{
					pInstruction->Displacement.DispDword = *(DWORD*)pCurOpcode;
					pCurOpcode+=4;
				}
			}

			//mod == 01
			if (mod == 0x40)
			{
				if (rm == 0x04)
				{
					pInstruction->SIB = *pCurOpcode++;
					pInstruction->Displacement.DispByte = *pCurOpcode++;
				}
				else
				{
					pInstruction->Displacement.DispByte = *pCurOpcode++;
				}
			}

			//mod ==10
			if (mod == 0x80)
			{
				if (rm == 0x04)
				{
					pInstruction->SIB = *pCurOpcode++;
					pInstruction->Displacement.DispDword = *(DWORD*)pCurOpcode;
					pCurOpcode+=4;
				}
				else
				{
					pInstruction->Displacement.DispDword = *(DWORD*)pCurOpcode;
					pCurOpcode+=4;
				}
			}
		}
	}
	//处理ModRM与Imm8重叠的时候
	if (dwSignature & Imm8)
	{
		pInstruction->Immediate.ImmByte = *pCurOpcode++;
	}
	//处理ModRM与Imm66重叠的时候
	if (dwSignature & Imm66)
	{
		//存在0x66前缀时
		if (pInstruction->OperandPrefix == 0x66)
		{
			pInstruction->Immediate.ImmWord = *(WORD*)pCurOpcode;
			pCurOpcode+=2;
		}
		else//不存在0x66前缀时
		{
			pInstruction->Immediate.ImmDword = *(DWORD*)pCurOpcode;
			pCurOpcode+=4;
		}
	}
}

int Dissassemble(IN BYTE* pOpcode,OUT PINSTRUCTION pInstruction)//,OUT char *pstrInstruction)
{
	BYTE* pCurOpcode = pOpcode;
	DWORD dwflag = 0;

	memset(pInstruction,0,sizeof(INSTRUCTION));//将结构重新填充为0，否则影响下条指令的解码
/************************************************************************/
/*解码前缀指令                                                          */
/************************************************************************/
_CheckPrefix:
	BYTE opcode = *pCurOpcode++;
	DWORD dwSignature = OneOpCodeMapTable[opcode];	//取opcode1表中的标志

	if (dwSignature & Prefix)
	{
		//出现重复同组前缀
		if (dwflag & dwSignature)
		{
			pInstruction->dwInstructionLen = 1;	
			return 1;	//返回指令长度为1，只输入一个前缀指令
		}
		//保存标志
		dwflag |= dwSignature;
		if (dwSignature & PreSegment)
		{
			pInstruction->SegmentPrefix = opcode;
		}
		if (dwSignature & PreOperandSize66)
		{
			pInstruction->OperandPrefix = opcode;
		}
		if (dwSignature & PreAddressSize67)
		{
			pInstruction->AddressPrefix = opcode;
		}
		if (dwSignature & PreLockF0)
		{
			pInstruction->RepeatPrefix = opcode;
		}
		if (dwSignature & PreRep)
		{
			pInstruction->RepeatPrefix = opcode;
		}

		goto _CheckPrefix;
	}

	//保存opcode,同时也处理了OneByte即1字节opcode的情况
	pInstruction->Opcode1 = opcode;

/************************************************************************/
/* 以下是取opcode2 opcode38/3A Group FPU表的标志，opcode1表标志上面已取过了 */
/************************************************************************/

	if (dwSignature & TwoOpCode0F)	//取opcode2 opcode38/3A 表标记
	{
		//保存opcode1与opcode2，也是处理了OneByte情况
		pInstruction->Opcode1 = opcode;
		pInstruction->Opcode2 = *pCurOpcode++;
		dwSignature = TwoOpCodeMapTable[pInstruction->Opcode2];
		
		if (dwSignature & ThreeOpCode0F38)	//取opcode38表中的标记
		{
			pInstruction->Opcode3 = *pCurOpcode++;
			dwSignature = ThreeOpCodeMapTable0F38[pInstruction->Opcode3];
		}

		if (dwSignature & ThreeOpCode0F3A)	//取opcode3A表中的标记
		{
			pInstruction->Opcode3 = *pCurOpcode++;
			dwSignature = ThreeOpCodeMapTable0F3A[pInstruction->Opcode3];
		}
	}

	if (dwSignature & Group)	//取opcode1 opcode2表中的Group的标记
	{
		GetGroupSignature(pCurOpcode,pInstruction,dwSignature);
	}

	if (dwSignature & FPUOpCode)	//取FPU表中的标志
	{
		GetFPUSignature(pCurOpcode,pInstruction,dwSignature);
	}
	//处理比较特殊的指令0xD6(有0xF3前缀时mod必须为11) 0xF0(无前缀与前缀为0x66时有Mxx标志，前缀为F2与66 F2 时，无Mxx标志
	if (pInstruction->RepeatPrefix == 0xF3 && pInstruction->Opcode2 == 0xD6)
	{
		dwSignature += Uxx;
	}
	if (pInstruction->RepeatPrefix == 0xF2 && (pInstruction->Opcode3 == 0xF0 || pInstruction->Opcode3 == 0xF1))
	{
		dwSignature -= Mxx;
	}

//以下开始处理取到的标记
/************************************************************************/
/* 处理Imm16的情况，在这里处理了Imm16与Imm8、Imm66重叠情况              */
/************************************************************************/

	if (dwSignature & Imm16)	//0x66、0x67前缀不影响指令长度，无论是否存在都取一个WORD立即数
	{
		DecodeImm16(pCurOpcode,pInstruction,dwSignature);
	}


/************************************************************************/
/* 处理Addr67的情况                                                     */
/************************************************************************/

	if (dwSignature & Addr67)//只有0x67影响指令长度
	{
		if (pInstruction->AddressPrefix == 0x67)
		{
			pInstruction->Displacement.DispWord = *(WORD*)pCurOpcode;
			pCurOpcode+=2;
		}
		else
		{
			pInstruction->Displacement.DispDword = *(DWORD*)pCurOpcode;
			pCurOpcode+=4;
		}
	}

/************************************************************************/
/* 处理ModRM的情况，在这里处理了ModRM与Imm8、Imm66重叠情况              */
/************************************************************************/
	
	if (dwSignature & ModRM)
	{
		DecodeModRM(pCurOpcode,pInstruction,dwSignature);
	}

/************************************************************************/
/* 处理Imm8的情况                                                       */
/************************************************************************/

	if (dwSignature & Imm8)//0x66、0x67前缀不影响指令长度，无论0x66、0x67前缀是否存在，指令都是取一个byte立即数
	{
		if (dwSignature & ModRM || dwSignature & Imm16)
		{//Imm8与Imm16重叠的情况已经在DecodeImm16里面处理了，与ModRM重叠已经DecodeModRM中处理
		}
		else
		{
			DecodeImm8(pCurOpcode,pInstruction,dwSignature);
		}
	}

/************************************************************************/
/*处理Imm66的情况                                                       */
/************************************************************************/

	if (dwSignature & Imm66)//只有0x66影响指令长度
	{
		if (dwSignature & ModRM || dwSignature & Imm16)
		{//Imm66与Imm16重叠的情况已经在DecodeImm16里面处理了，与ModRM重叠已经DecodeModRM中处理
		}
		else
		{
			DecodeImm66(pCurOpcode,pInstruction,dwSignature);
		}
	}

/************************************************************************/
/* 处理MustHave66标志                                                   */
/************************************************************************/

	if (dwSignature & MustHave66)	//opcode38/3A表里有很多必须有0x66前缀的指令
	{
		if (pInstruction->OperandPrefix != 0x66)
		{
			pInstruction->dwInstructionLen = 1;
			return 1;
		}
	}

/************************************************************************/
/* 处理MustHaveF2标志		                                            */
/************************************************************************/

	if (dwSignature & MustHaveF2)	//在opcode2表中有一个必须有F2的指令，是opcode2表中的F0
	{
		if (pInstruction->RepeatPrefix != 0xF2)
		{
			pInstruction->dwInstructionLen = 1;
			return 1;
		}
	}

/************************************************************************/
/* 处理MustHavePrefix标志		                                        */
/************************************************************************/
	
	if (dwSignature & MustHavePrefix)	//在opcode2表中有一个必须有F2的指令，是opcode2表中的F0
	{
		if (pInstruction->RepeatPrefix == 0 && pInstruction->OperandPrefix == 0)
		{
			pInstruction->dwInstructionLen = 1;
			return 1;
		}
	}

/************************************************************************/
/* 处理MustNo66标志                                                     */
/************************************************************************/

	if (dwSignature & MustNo66)	//不能有0x66前缀的指令
	{
		if (pInstruction->OperandPrefix == 0x66)
		{
			pInstruction->dwInstructionLen = 1;
			return 1;
		}
	}

/************************************************************************/
/* 处理MustNoF2标志                                                     */
/************************************************************************/

	if (dwSignature & MustNoF2)	//不能有0xF2前缀的指令
	{
		if (pInstruction->RepeatPrefix == 0xF2)
		{
			pInstruction->dwInstructionLen = 1;
			return 1;
		}
	}

/************************************************************************/
/* 处理MustNoF3标志                                                     */
/************************************************************************/
	
	if (dwSignature & MustNoF3)	//不能有0xF3前缀的指令
	{
		if (pInstruction->RepeatPrefix == 0xF3)
		{
			pInstruction->dwInstructionLen = 1;
			return 1;
		}
	}

/************************************************************************/
/* 处理StringMnemonic标志                                               */
/************************************************************************/
	
	if (!(dwSignature & StringInstruction) && pInstruction->Opcode1 != 0x0F)	//如果没有StringInstruction标记
	{
		if (pInstruction->RepeatPrefix > 0)	//如果有前缀指令0xF2或0xF3
		{
			pInstruction->dwInstructionLen = 1;
			return 1;	
		}
	}

/************************************************************************/
/* 处理Reserved指令，一律返回1                                          */
/************************************************************************/

	if (dwSignature & Reserved)
	{
		pInstruction->dwInstructionLen = 1;
		return 1;
	}
	
/************************************************************************/
/* 处理Uxx标志                                                          */
/************************************************************************/

	if (dwSignature & Uxx)
	{
		BYTE modrm = pInstruction->Modrm;
		BYTE mod = (modrm >> 6) & 3;
		if (!(dwSignature & Mxx))	//如果Uxx与Mxx没有重叠，即只有Uxx标志的话，必须mod == 11
		{
			if (mod != 3)	//mod != 11
			{
				pInstruction->dwInstructionLen = 1;
				return 1;
			}
		}
	}

/************************************************************************/
/* 处理Rxx标志                                                          */
/************************************************************************/
	
	if (dwSignature & Rxx)		//Rxx实际作用和Uxx是一样的，与所可以写到一个标志中URNxx
	{
		BYTE modrm = pInstruction->Modrm;
		BYTE mod = (modrm >> 6) & 3;
		if (!(dwSignature & Mxx))	//如果Rxx与Mxx没有重叠，即只有Rxx标志的话，必须mod == 11
		{
			if (mod != 3)	//mod != 11
			{
				pInstruction->dwInstructionLen = 1;
				return 1;
			}
		}
	}

/************************************************************************/
/* 处理Mxx标志                                                          */
/************************************************************************/
	
	if (dwSignature & Mxx)
	{
		BYTE modrm = pInstruction->Modrm;
		BYTE mod = (modrm >> 6) & 3;
		if (!(dwSignature & Uxx || dwSignature & Rxx))	//如果Mxx与Uxx或Rxx没有重叠，即只有Mxx标志的话，必须mod != 11
		{
			if (mod == 3)	//mod == 11
			{
				pInstruction->dwInstructionLen = 1;
				return 1;
			}
		}
	}

	//保存指令长度
	pInstruction->dwInstructionLen = (DWORD)(pCurOpcode-pOpcode);
	//把指令拷贝一份存入pInstruction->InstructionBuf
	memcpy(pInstruction->InstructionBuf,pOpcode,pInstruction->dwInstructionLen);

	return (int)(pCurOpcode-pOpcode);	//返回指令长度
}



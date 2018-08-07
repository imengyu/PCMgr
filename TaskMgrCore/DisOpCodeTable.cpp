#include "stdafx.h"
#include "Disassemble.h"

DWORD OneOpCodeMapTable[256]=
{
	/*		0			1			2			3			4			5			6			7			8				9			A			B			C			D			E			F		*/
	/*0*/	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,		Imm66,		OneByte,	OneByte,	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,		Imm66,		OneByte,	TwoOpCode0F,		
	/*1*/	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,		Imm66,		OneByte,	OneByte,	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,		Imm66,		OneByte,	OneByte,		
	/*2*/	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,		Imm66,		PreSegment,	OneByte,	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,		Imm66,		PreSegment,	OneByte,		
	/*3*/	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,		Imm66,		PreSegment,	OneByte,	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,		Imm66,		PreSegment,	OneByte,		
	/*4*/	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,		
	/*5*/	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,		
	/*6*/	OneByte,	OneByte,	ModRM,		ModRM,		PreSegment,	PreSegment,	PreOperandSize66,PreAddressSize67,Imm66,	Imm66+ModRM,Imm8,		Imm8+ModRM,	OneByte,	OneByte,	OneByte,	OneByte,
	/*7*/	Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,
	/*8*/	Imm8+ModRM,	Imm66+ModRM,Imm8+ModRM,	Imm8+ModRM,	ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,
	/*9*/	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	Imm16+Imm66,OneByte,	OneByte,	OneByte,	OneByte,	OneByte,
	/*A*/	Addr67,		Addr67,		Addr67,		Addr67,		OneByte,	OneByte,	OneByte,	OneByte,	Imm8,		Imm66,		OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,
	/*B*/	Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,		Imm66,
	/*C*/	Imm8+ModRM,	Imm8+ModRM,	Imm16,		OneByte,	ModRM,		ModRM,		Imm8+ModRM,	Imm66+ModRM,Imm16+Imm8,	OneByte,	Imm16,		OneByte,	OneByte,	Imm8,		OneByte,	OneByte,
	/*D*/	ModRM,		ModRM,		ModRM,		ModRM,		Imm8,		Imm8,		Reserve,	OneByte,	ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,		ModRM,
	/*E*/	Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm8,		Imm66,		Imm66,		Imm66+Imm16,Imm8,		OneByte,	OneByte,	OneByte,	OneByte,
	/*F*/	PreLockF0,	Reserve,	PreRep,		PreRep,		OneByte,	OneByte,	ModRM,		ModRM,		OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	OneByte,	ModRM,		ModRM
};
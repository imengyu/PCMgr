/*

		Written by AlexLong

		Bug Report: 
		Email:	33445621@163.com
		MSN:	33445621@163.com

		All rights reserved
*/


#include "stdafx.h"

typedef struct  _INSTRUCTION
{
	BYTE	RepeatPrefix;	//重复指令前缀
	BYTE	SegmentPrefix;	//段前缀
	BYTE	OperandPrefix;	//操作数大小前缀0x66
	BYTE	AddressPrefix;	//地址大小前缀0x67

	BYTE	Opcode1;		//opcode1
	BYTE	Opcode2;		//opcode2
	BYTE	Opcode3;		//opcode3
	
	BYTE	Modrm;			//modrm
	
	BYTE	SIB;			//sib
	
	union					//displacement联合体
	{
		BYTE	DispByte;
		WORD	DispWord;
		DWORD	DispDword;
	}Displacement;

	union					//immediate联合体
	{
		BYTE	ImmByte;
		WORD	ImmWord;
		DWORD	ImmDword;
	}Immediate;
	
	BYTE	InstructionBuf[32];	//保存指令代码
	DWORD	dwInstructionLen;	//返回指令长度
		
}INSTRUCTION,*PINSTRUCTION;
 
#define Reserve             0x00000000
#define ModRM				0x00000001		//含有ModRM
#define Imm8				0x00000002		//后面跟着1字节立即数
#define Imm16				0x00000004		//后面跟着2字节立即数
#define Imm66				0x00000008		//后面跟着立即数（Immediate），立即数长度得看是否有0x66前缀
#define Addr67				0x00000010		//后面跟着偏移量（Displacement），偏移量长度得看是否有0x67前缀
#define OneByte				0x00000020		//只有1个字节，这1个字节独立成一个指令
#define TwoOpCode0F			0x00000040		//0x0F，2个opcode
#define ThreeOpCode0F38		0x00000080		//0x0F38，3个opcode
#define ThreeOpCode0F3A		0x00000100		//0x0F3A，3个opcode
#define Group				0x00000200		//Group表opcode
#define Reserved			0x00000400		//保留
#define MustHave66			0x00000800		//必须有0x66前缀,只在opcode38/3A表中有这样的指令
#define MustHaveF2			0x00001000		//目前只有一个指令是必须有0xF2前缀的：0FF0
#define MustHavePrefix		0x00002000		//必须有前缀
#define MustNo66			0x00004000		//必须没有0x66前缀,在扫描指令时出现66则取标志指令看是否有此标志，有则直接返回1，说明此66前缀是多余的
#define MustNoF2			0x00008000		//意义同上
#define MustNoF3			0x00010000		//意义同上

#define StringInstruction	0x00020000		//指令重复前缀0xF2 0xF3(REPNE REP/REPE)在Opcode1表中只能与下面7组字符串指令组合，
											// 		0xA4: 0xA5:		MOVS
											// 		0xA6: 0xA7:		CMPS
											// 		0xAE: 0xAF:		SCAS
											// 		0xAC: 0xAD:		LODS
											// 		0xAA: 0xAB:		STOS
											// 		0x6C: 0x6D:		INS
											// 		0x6E: 0x6F:		OUTS
#define Uxx					0x00040000		//rm用于寻XMM，只能在mod==11时才可以解码，可能的opcode: 66 0F 50/C5/D7/F7	F2 OF D6
#define Nxx					0x00080000		//rm用于寻MMX，只能在mod==11时才可以解码，可能的opcode: OF C5/D7/F7			F3 OF D6
											//Nxx没用，因为在Uxx里面已经全部包括了Nxx的可能 
#define Mxx					0x00100000		//mod != 11时才可解码
#define Rxx					0x00200000		//mod == 11时才可解码

#define PreSegment			0x00400000		//段前缀
#define	PreOperandSize66	0x00800000		//指令大小前缀0x66
#define PreAddressSize67	0x01000000		//地址大小前缀0x67
#define PreLockF0			0x02000000		//锁前缀0xF0
#define PreRep				0x04000000		//重复前缀
#define Prefix				(PreSegment+PreOperandSize66+PreAddressSize67+PreLockF0+PreRep)

#define FPUOpCode			0x08000000		//FPU

//Private
void DecodeImm66(IN BYTE* &pCurOpcode,OUT PINSTRUCTION &pInstruction,IN DWORD dwSignature);
void DecodeImm8(IN BYTE* &pCurOpcode,OUT PINSTRUCTION &pInstruction,IN DWORD dwSignature);
void DecodeImm16(IN BYTE* &pCurOpcode,OUT PINSTRUCTION &pInstruction,IN DWORD dwSignature);
void DecodeModRM(IN BYTE* &pCurOpcode,OUT PINSTRUCTION &pInstruction,IN DWORD dwSignature);

//Public
int  Dissassemble(IN BYTE* pOpcode,OUT PINSTRUCTION pInstruction);










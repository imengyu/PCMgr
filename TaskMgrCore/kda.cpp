#include "stdafx.h"
#include "kda.h"
#include "suact.h"
#include "loghlp.h"
#include "mapphlp.h"
#include <capstone/platform.h>
#include <capstone/capstone.h>

//基于开源 capstone 反汇编引擎

static csh csh_handle;

M_CAPI(BOOL) M_KDA_Dec(PUCHAR buf, ULONG_PTR startaddress, LPVOID callback, ULONG_PTR size, BOOL x86Orx64)
{
	DACALLBACK dacallback = (DACALLBACK)callback;
	ULONG_PTR curaddress = startaddress;
	WCHAR barinystr[64];
	WCHAR addressstr[32];
	WCHAR shellstr[32];
	WCHAR shellstr2[64];
	WCHAR shellbarinystrbuf[3];
	memset(barinystr, 0, sizeof(barinystr));
	memset(addressstr, 0, sizeof(addressstr));
	memset(shellstr, 0, sizeof(shellstr));
	memset(shellstr2, 0, sizeof(shellstr2));
	memset(shellbarinystrbuf, 0, sizeof(shellbarinystrbuf));

	ULONG_PTR address = startaddress;

	cs_err err = cs_open(CS_ARCH_X86, x86Orx64 ? CS_MODE_32 : CS_MODE_64, &csh_handle);
	if (err) { LogErr(L"Failed on cs_open() with error returned: %u\n", err); return FALSE; }

	cs_insn *insn;
	size_t count = cs_disasm(csh_handle, buf, size, address, 0, &insn);
	if (count) {
		size_t j;
		for (j = 0; j < count; j++) {
			memset(barinystr, 0, sizeof(barinystr));
			memset(addressstr, 0, sizeof(addressstr));
			memset(shellstr, 0, sizeof(shellstr));
			memset(shellstr2, 0, sizeof(shellstr2));

			swprintf_s(addressstr, L"0x%I64X", insn[j].address);
			swprintf_s(shellstr, L"%hs", insn[j].mnemonic);
			swprintf_s(shellstr2, L"%hs", insn[j].op_str);

			for (int h = 0; h < insn[j].size; h++)
			{
				memset(shellbarinystrbuf, 0, sizeof(shellbarinystrbuf));
				swprintf_s(shellbarinystrbuf, L"%02X", insn[j].bytes[h]);
				wcscat_s(barinystr, shellbarinystrbuf);
			}
			dacallback(curaddress, addressstr, shellstr, barinystr, shellstr2);
		}
		//printf("0x%" PRIx64 ":\n", insn[j - 1].address + insn[j - 1].size);
		// free memory allocated by cs_disasm()
		cs_free(insn, count);
		return TRUE;
	}
	else LogErr(L"Failed to disasm given code!\n");
	return FALSE;
}
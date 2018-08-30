#pragma once
#include "stdafx.h"
#include <stdio.h>  
#include <stdlib.h>  
#include <string>  
#include <vector>

//×Ö·û´®ÊÇ·ñÏàµÈ
#define MStrEqual MStrEqualW
//Õ­×Ö·û×ªÎª¿í×Ö·û
#define A2W MConvertLPCSTRToLPWSTR
//¿í×Ö·û×ªÎªÕ­×Ö·û
#define W2A MConvertLPWSTRToLPCSTR

EXTERN_C M_API void MConvertStrDel(void * str);

//Õ­×Ö·û×ªÎª¿í×Ö·û
EXTERN_C M_API LPWSTR MConvertLPCSTRToLPWSTR(const char * szString);
//¿í×Ö·û×ªÎªÕ­×Ö·û
EXTERN_C M_API LPCSTR MConvertLPWSTRToLPCSTR(const WCHAR * szString);
//×Ö·û´®ÊÇ·ñÏàµÈ
EXTERN_C M_API BOOL MStrEqualA(const LPCSTR str1, const LPCSTR str2);
//×Ö·û´®ÊÇ·ñÏàµÈ
EXTERN_C M_API BOOL MStrEqualW(const wchar_t* str1, const wchar_t* str2);

std::string & FormatString(std::string & _str, const char * _Format, ...);

std::wstring & FormatString(std::wstring & _str, const wchar_t * _Format, ...);

std::wstring FormatString(const wchar_t * format, ...);

std::wstring FormatString(const wchar_t *_Format, va_list marker);

std::string FormatString(const char *_Format, va_list marker);

std::string FormatString(const char * format, ...);

#define FormatStringPtr FormatStringPtrW

EXTERN_C M_API std::string* FormatStringPtr2A(std::string *_str, const char * _Format, ...);
EXTERN_C M_API std::wstring * FormatStringPtr2W(std::wstring *_str, const wchar_t * _Format, ...);
EXTERN_C M_API std::wstring * FormatStringPtrW(const wchar_t *format, ...);
EXTERN_C M_API std::string *FormatStringPtrA(const char *format, ...);

EXTERN_C M_API void FormatStringPtrDel(void * ptr);

M_CAPI(wchar_t*) Utf8ToUnicode(const char* szU8);
M_CAPI(char*) UnicodeToAnsi(const wchar_t* szStr);
M_CAPI(char*) UnicodeToUtf8(const wchar_t* unicode);
M_CAPI(wchar_t*) AnsiToUnicode(const char* szStr);

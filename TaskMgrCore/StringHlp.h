#pragma once
#include "stdafx.h"
#include <stdio.h>  
#include <stdlib.h>  
#include <string>  
#include <vector>

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

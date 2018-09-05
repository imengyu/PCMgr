#pragma once
#include "stdafx.h"

wchar_t* __cdecl m_wcscpy(wchar_t* const destination, wchar_t const* source);
wchar_t * __cdecl m_wcslwr(wchar_t * wsrc);
int __cdecl m_wcscmp(wchar_t const* a, wchar_t const* b);
char * __cdecl m_strcat(char * dst, const char * src);
char * __cdecl m_strcpy(char * dst, const char * src);
int __cdecl m_strcmp(const char * src, const char * dst);
char * __cdecl m_strchr(const char * string, int ch);
char * m_memset(char *dst, char value, unsigned int count);
void * m_memcpy(void * dst, void * src, size_t count);

void m_copyto_wcsarray(wchar_t *dst, unsigned short *source, int maxlen);
void m_copyto_strarray(char *dst, unsigned int *source, int maxlen);
#include "stdafx.h"
#include "mcrt.h"

wchar_t* __cdecl m_wcscpy(wchar_t* const destination, wchar_t const* source)
{
	wchar_t* destination_it = destination;
	while ((*destination_it++ = *source++) != '\0') {}

	return destination;
}
wchar_t * __cdecl m_wcslwr(wchar_t * wsrc)
{
	wchar_t * p;
	/* validation section */
	if (wsrc != NULL) {

		for (p = wsrc; *p; ++p)
		{
			if (L'A' <= *p && *p <= L'Z')
				*p += (wchar_t)L'a' - (wchar_t)L'A';
		}
	}
	return(wsrc);
}
int __cdecl m_wcscmp(wchar_t const* a, wchar_t const* b)
{
	int result = 0;
	while ((result = (int)(*a - *b)) == 0 && *b)
		++a, ++b;

	if (result < 0)
		return -1;

	else if (result > 0)
		return 1;

	return 0;
}

char * __cdecl m_strcat(char * dst, const char * src)
{
	char * cp = dst;

	while (*cp)
		cp++;                   /* find end of dst */

	while ((*cp++ = *src++) != '\0');       /* Copy src to end of dst */

	return(dst);                  /* return dst */

}
char * __cdecl m_strcpy(char * dst, const char * src)
{
	char * cp = dst;

	while ((*cp++ = *src++) != '\0')
		;               /* Copy src over dst */

	return(dst);
}
int __cdecl m_strcmp(const char * src, const char * dst)
{
	int ret = 0;
	while ((ret = *(unsigned char *)src - *(unsigned char *)dst) == 0 && *dst)
		++src, ++dst;
	if (ret < 0)
		ret = -1;
	else if (ret > 0)
		ret = 1;
	return(ret);
}
char * __cdecl m_strchr(const char * string,	int ch)
{
	while (*string && *string != (char)ch)
		string++;

	if (*string == (char)ch)
		return((char *)string);
	return(NULL);
}
char * m_memset(char *dst, char value, unsigned int count)
{
	char *start = dst;
	while (count--)
		*dst++ = value;
	return(start);
}
void * m_memcpy(void * dst, void * src, size_t count)
{
	void * ret = dst;
	while (count--)
		*((LPBYTE)dst)++ = *((LPBYTE)src)++;

	return(ret);
}
void m_copyto_wcsarray(wchar_t *dst, unsigned short *source, int maxlen)
{
	for (int i = 0; i < maxlen; i++)
		dst[i] = (wchar_t)source[i];
}
void m_copyto_strarray(char *dst, unsigned int *source, int maxlen)
{
	for (int i = 0; i < maxlen; i++)
		dst[i] = (char)source[i];
}
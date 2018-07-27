#include "stdafx.h"
#include "StringHlp.h"

std::string & FormatString(std::string & _str, const char * _Format, ...) {
	std::string tmp;

	va_list marker = NULL;
	va_start(marker, _Format);

	size_t num_of_chars = _vscprintf(_Format, marker);

	if (num_of_chars > tmp.capacity()) {
		tmp.resize(num_of_chars + 1);
	}

	vsprintf_s((char *)tmp.data(), tmp.capacity(), _Format, marker);

	va_end(marker);

	_str = tmp.c_str();
	return _str;
}

std::wstring & FormatString(std::wstring & _str, const wchar_t * _Format, ...) {
	std::wstring tmp;
	va_list marker = NULL;
	va_start(marker, _Format);
	size_t num_of_chars = _vscwprintf(_Format, marker);

	if (num_of_chars > tmp.capacity()) {
		tmp.resize(num_of_chars + 1);
	}
	vswprintf_s((wchar_t *)tmp.data(), tmp.capacity(), _Format, marker);
	va_end(marker);
	_str = tmp.c_str();
	return _str;
}


std::wstring FormatString(const wchar_t *_Format, va_list marker)
{
	std::wstring tmp;
	size_t num_of_chars = _vscwprintf(_Format, marker);

	if (num_of_chars > tmp.capacity()) {
		tmp.resize(num_of_chars + 1);
	}
	vswprintf_s((wchar_t *)tmp.data(), tmp.capacity(), _Format, marker);
	std::wstring  _str = tmp.c_str();
	return _str;
}

std::string FormatString(const char *_Format, va_list marker)
{
	std::string tmp;
	size_t num_of_chars = _vscprintf(_Format, marker);

	if (num_of_chars > tmp.capacity()) {
		tmp.resize(num_of_chars + 1);
	}

	vsprintf_s((char *)tmp.data(), tmp.capacity(), _Format, marker);
	std::string _str = tmp.c_str();
	return _str;
}

std::wstring FormatString(const wchar_t *_Format, ...)
{
	std::wstring tmp;
	va_list marker = NULL;
	va_start(marker, _Format);
	size_t num_of_chars = _vscwprintf(_Format, marker);

	if (num_of_chars > tmp.capacity()) {
		tmp.resize(num_of_chars + 1);
	}
	vswprintf_s((wchar_t *)tmp.data(), tmp.capacity(), _Format, marker);
	va_end(marker);
	std::wstring  _str = tmp.c_str();
	return _str;
}

std::string FormatString(const char *_Format, ...)
{
	std::string tmp;

	va_list marker = NULL;
	va_start(marker, _Format);

	size_t num_of_chars = _vscprintf(_Format, marker);

	if (num_of_chars > tmp.capacity()) {
		tmp.resize(num_of_chars + 1);
	}

	vsprintf_s((char *)tmp.data(), tmp.capacity(), _Format, marker);

	va_end(marker);

	std::string _str = tmp.c_str();
	return _str;
}

std::string*FormatStringPtr2A(std::string *_str, const char * _Format, ...) {
	std::string tmp;

	va_list marker = NULL;
	va_start(marker, _Format);

	size_t num_of_chars = _vscprintf(_Format, marker);

	if (num_of_chars > tmp.capacity()) {
		tmp.resize(num_of_chars + 1);
	}

	vsprintf_s((char *)tmp.data(), tmp.capacity(), _Format, marker);

	va_end(marker);

	*_str = tmp.c_str();
	return _str;
}

std::wstring *FormatStringPtr2W(std::wstring *_str, const wchar_t * _Format, ...) {
	std::wstring tmp;
	va_list marker = NULL;
	va_start(marker, _Format);
	size_t num_of_chars = _vscwprintf(_Format, marker);

	if (num_of_chars > tmp.capacity()) {
		tmp.resize(num_of_chars + 1);
	}
	vswprintf_s((wchar_t *)tmp.data(), tmp.capacity(), _Format, marker);
	va_end(marker);
	*_str = tmp.c_str();
	return _str;
}

std::wstring *FormatStringPtrW(const wchar_t *_Format, ...)
{
	std::wstring tmp;
	va_list marker = NULL;
	va_start(marker, _Format);
	size_t num_of_chars = _vscwprintf(_Format, marker);

	if (num_of_chars > tmp.capacity()) {
		tmp.resize(num_of_chars + 1);
	}
	vswprintf_s((wchar_t *)tmp.data(), tmp.capacity(), _Format, marker);
	va_end(marker);
	std::wstring *_str = new std::wstring();
	*_str = tmp.c_str();
	return _str;
}

std::string *FormatStringPtrA(const char *_Format, ...)
{
	std::string tmp;

	va_list marker = NULL;
	va_start(marker, _Format);

	size_t num_of_chars = _vscprintf(_Format, marker);

	if (num_of_chars > tmp.capacity()) {
		tmp.resize(num_of_chars + 1);
	}

	vsprintf_s((char *)tmp.data(), tmp.capacity(), _Format, marker);

	va_end(marker);

	std::string *_str = new std::string();
	*_str = tmp.c_str();
	return _str;
}

void FormatStringPtrDel(void *ptr)
{
	if (ptr)
		delete ptr;
}
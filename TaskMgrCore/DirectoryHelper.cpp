#include "stdafx.h"
#include "DirectoryHelper.h"



Directory::Directory()
{
}
Directory::~Directory()
{
}

bool Directory::Exists(std::wstring * path)
{
	if (path != nullptr)
	{
		if (_waccess(path->c_str(), 0) == 0)
			return true;
	}
	return false;
}
bool Directory::Exists(WCHAR * path)
{
	if (path != nullptr)
	{
		if (_waccess(path, 0) == 0)
			return true;
	}
	return false;
}
bool Directory::Delete(std::wstring * path)
{
	if (Exists(path))  return RemoveDirectory(path->c_str());
	return false;
}
bool Directory::Delete(WCHAR * path)
{
	if (Exists(path))  return RemoveDirectory(path);
	return false;
}
bool Directory::Create(std::wstring * path)
{
	if (path != nullptr)
	{
		if (_waccess(path->c_str(), 0) == 0)
			return true;
		else {
			return CreateDirectory(path->c_str(), 0);
		}
	}
	return false;
}
bool Directory::Create(WCHAR * path)
{
	if (path != nullptr)
	{
		if (_waccess(path, 0) == 0)
			return true;
		else
			return CreateDirectory(path, 0);
	}
	return false;
}
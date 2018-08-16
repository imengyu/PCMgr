#include "stdafx.h"
#include "DirectoryHelper.h"
#include "fmhlp.h"


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
		if (MFM_FileExist(path->c_str()))
			return true;
	}
	return false;
}
bool Directory::Exists(WCHAR * path)
{
	if (path != nullptr)
	{
		if (MFM_FileExist(path))
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
		if (MFM_FileExist(path->c_str()))
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
		if (MFM_FileExist(path))
			return true;
		else
			return CreateDirectory(path, 0);
	}
	return false;
}

bool Directory::Exists(std::string * path)
{
	if (path != nullptr)
	{
		if (MFM_FileExistA(path->c_str()))
			return true;
	}
	return false;
}
bool Directory::Exists(CHAR * path)
{
	if (path != nullptr)
	{
		if (MFM_FileExistA(path))
			return true;
	}
	return false;
}
bool Directory::Delete(std::string * path)
{
	if (Exists(path))  return RemoveDirectoryA(path->c_str());
	return false;
}
bool Directory::Delete(CHAR * path)
{
	if (Exists(path))  return RemoveDirectoryA(path);
	return false;
}
bool Directory::Create(std::string * path)
{
	if (path != nullptr)
	{
		if (MFM_FileExistA(path->c_str()))
			return true;
		else {
			return CreateDirectoryA(path->c_str(), 0);
		}
	}
	return false;
}
bool Directory::Create(CHAR * path)
{
	if (path != nullptr)
	{
		if (MFM_FileExistA(path))
			return true;
		else
			return CreateDirectoryA(path, 0);
	}
	return false;
}
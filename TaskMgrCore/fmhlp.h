#pragma once
#include "stdafx.h"
#include <ShlObj.h>
#include <ShlGuid.h>
#include <shellapi.h>
#include <shlwapi.h>

typedef void* (__cdecl*MFCALLBACK)(int msg, void* lParam, void* wParam);

//显示文件属性对话框
//    LPWSTR file：文件路径
EXTERN_C M_API VOID MShowFileProp(LPWSTR file);
//复制字符串到剪贴板中
//    const WCHAR * pszData：需要复制的字符串
//    const size_t nDataLen ：需要复制的字符串字符个数（包括\0）
EXTERN_C M_API BOOL MCopyToClipboard(const WCHAR * pszData, const size_t nDataLen);
//复制字符串到剪贴板中
//    const WCHAR * pszData：需要复制的字符串
EXTERN_C M_API BOOL MCopyToClipboard2(const WCHAR * pszData);

//获取文件扩展名对应的图标以及文件扩展名说明文字
//    LPWSTR extention：扩展名（.txt）
//    LPWSTR s：输出文件扩展名说明文字字符串缓冲区
//    int count：输出字符串缓冲区文字个数
EXTERN_C M_API HICON MFM_GetFileIcon(LPWSTR extention, LPWSTR s, int count);
//获取文件夹的对应图标
EXTERN_C M_API HICON MFM_GetFolderIcon();
//获取”此电脑“的对应图标
EXTERN_C M_API HICON MFM_GetMyComputerIcon();
EXTERN_C M_API void MFM_GetRoots();
EXTERN_C M_API void MFM_SetCallBack(MFCALLBACK cp);
EXTERN_C M_API BOOL MFM_GetFolders(LPWSTR path);
//运行一个exe
//    LPWSTR path：文件路径
//    LPWSTR cmd：附加参数
//    HWND hWnd：调用者窗口句柄
EXTERN_C M_API BOOL MFM_RunExe(LPWSTR path, LPWSTR cmd, HWND hWnd);
//打开一个文件
//    LPWSTR path：文件路径
//    HWND hWnd：调用者窗口句柄
EXTERN_C M_API BOOL MFM_OpenFile(LPWSTR path, HWND hWnd);
EXTERN_C M_API BOOL MFM_ReUpdateFile(LPWSTR fullPath, LPWSTR dirPath);
EXTERN_C M_API BOOL MFM_UpdateFile(LPWSTR fullPath, LPWSTR dirPath);
EXTERN_C M_API BOOL MFM_GetFiles(LPWSTR path);
//获取文件使用时间以及已经格式化以后的字符串
EXTERN_C M_API BOOL MFM_GetFileTime(FILETIME * ft, LPWSTR s, int count);
//获取文件属性格式化以后的字符串
//    DWORD att：输入属性
//    LPWSTR s：输出文件扩展名说明文字字符串缓冲区
//    int count：输出字符串缓冲区文字个数
//    BOOL*hiddenout：输出是否有隐藏标识
EXTERN_C M_API BOOL MFM_GetFileAttr(DWORD att, LPWSTR s, int count, BOOL*hiddenout);
//快速打开一个文件
EXTERN_C M_API BOOL MFM_OpenAFile(LPWSTR path);

//反馈回C#界面刷新的回调
EXTERN_C M_API void MFM_Refesh();
//反馈回C#界面的回调
EXTERN_C M_API void MFM_Recall(int id, LPWSTR path);
EXTERN_C M_API void MFM_SetStatus(LPWSTR st);
EXTERN_C M_API void MFM_SetStatus2(int st);

//复制或剪切一个文件到剪贴板
//    LPWSTR szFileName：文件路径
//    BOOL isCopy：TRUE为复制FALSE为剪切
EXTERN_C M_API int MFM_CopyOrCutFileToClipboard(LPWSTR szFileName, BOOL isCopy);
//验证输入的字符串是否可以是有效的文件名
EXTERN_C M_API BOOL MFM_IsValidateFolderFileName(wchar_t * pName);
//创建目录
EXTERN_C M_API BOOL MFM_CreateDir(wchar_t * path);
//删除一个文件夹或者是文件
EXTERN_C M_API BOOL MFM_DeleteDirOrFile(wchar_t * path);
//计算此文件夹下所有文件的个数
EXTERN_C M_API UINT MFM_CalcFileCount(const wchar_t * szFileDir);
//删除目录下的所有文件夹以及文件
EXTERN_C M_API BOOL MFM_DeleteDir(const wchar_t * szFileDir);
//验证此路径是一个文件还是文件夹，是文件夹则返回TRUE
EXTERN_C M_API BOOL MFM_IsPathDir(const wchar_t * path);

BOOL MFM_RenameFile();
BOOL MFM_MoveFileToUser();
BOOL MFM_CopyFileToUser();
BOOL MFM_DelFileToRecBinUser();
BOOL MFM_DelFileForeverUser();
void MFF_ShowFolderProp();
void MFF_CopyPath();
void MFF_ShowInExplorer();
BOOL MFF_DelToRecBin();
BOOL MFF_DelForever();
BOOL MFF_ForceDel();
void MFF_Copy();
void MFF_Patse();
void MFF_Cut();
void MFF_Remane();
void MFF_ShowFolder();

EXTERN_C M_API LPWSTR MFM_GetSeledItemPath(int index);
EXTERN_C M_API void MFM_GetSeledItemFree(void* v);
EXTERN_C M_API BOOL MFM_GetShowHiddenFiles();

//检查文件或文件夹是否存在
EXTERN_C M_API BOOL MFM_FileExist(const wchar_t * path);
//检查文件或文件夹是否存在
EXTERN_C M_API BOOL MFM_FileExistA(const char * path);

EXTERN_C M_API void MFM_SetShowHiddenFiles(BOOL b);

//强制删除文件或文件夹
EXTERN_C M_API BOOL MFM_DeleteDirOrFileForce(const wchar_t * szFileDir);
//强制删除文件夹
EXTERN_C M_API BOOL MFM_DeleteDirForce(const wchar_t * szFileDir);
//强制删除文件
EXTERN_C M_API BOOL MFM_DeleteFileForce(const wchar_t * szFileDir);
//设置文件属性
//    szFileDir：文件路径
//    attr：需要添加进去的属性
EXTERN_C M_API BOOL MFM_SetFileArrtibute(const wchar_t * szFileDir, DWORD attr);
//移除文件属性
//    szFileDir：文件路径
//    attr：需要从中移除的属性
EXTERN_C M_API BOOL MFM_RemoveFileArrtibute(const wchar_t * szFileDir, DWORD attr);
//把文件填充 00
//    szFile：文件路径
//    force：是否强制
//    fileSize：需要填充的大小
EXTERN_C M_API BOOL MFM_FillData(const wchar_t* szFile, BOOL force, UINT fileSize);
//清空文件
//    force：是否强制
EXTERN_C M_API BOOL MFM_EmeptyFile(const wchar_t* szFile, BOOL force);
EXTERN_C M_API BOOL MFM_GetFileInformationString(const wchar_t * szFile, LPWSTR strbuf, UINT bufsize);
//打开文件资源管理器并定位到 szFile 指定的路径
EXTERN_C M_API BOOL MFM_ShowInExplorer(const wchar_t * szFile);
//获取”此电脑“的名字（在win7显示”计算机“而win10显示”此电脑“）
EXTERN_C M_API LPWSTR MFM_GetMyComputerName();

DWORD WINAPI MFM_DeleteDirThread(LPVOID lpThreadParameter);
BOOL MFM_DeleteDirInnern(const wchar_t * szFileDir);
void MFM_ReSetShowHiddenFiles();

//菜单函数

EXTERN_C M_API int MAppWorkShowMenuFM(LPWSTR strFilePath, BOOL mutilSelect, int selectCount);
EXTERN_C M_API int MAppWorkShowMenuFMF(LPWSTR strfolderPath);




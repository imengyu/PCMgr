#pragma once
#include "stdafx.h"

//保存文件对话框
//    hWnd：父窗口
//    startDir：起始目录
//    title：对话框标题
//    fileFilter：文件筛选器，比如：文本文件\0*.txt\0所有文件\0*.*\0
//    fileName：默认文件名字
//    defExt：默认文件扩展名
//    [OUT] strrs：输出选择的文件字符串缓冲区
//    bufsize：选择的文件字符串缓冲区字符最大个数
M_CAPI(BOOL) M_DLG_SaveFileSingal(HWND hWnd, LPWSTR startDir, LPWSTR title, LPWSTR fileFilter, LPWSTR fileName, LPWSTR defExt, LPWSTR  strrs, size_t bufsize);

//选择文件对话框
//    hWnd：父窗口
//    startDir：起始目录
//    title：对话框标题
//    fileFilter：文件筛选器，比如：文本文件\0*.txt\0所有文件\0*.*\0
//    fileName：默认文件名字
//    defExt：默认文件扩展名
//    [OUT] strrs：输出选择的文件字符串缓冲区
//    bufsize：选择的文件字符串缓冲区字符最大个数
M_CAPI(BOOL) M_DLG_ChooseFileSingal(HWND hWnd, LPWSTR startDir, LPWSTR title, LPWSTR fileFilter, LPWSTR fileName, LPWSTR defExt, LPWSTR strrs, size_t bufsize);
//选择文件夹对话框
//    hWnd：父窗口
//    startDir：起始目录
//    title：对话框标题
//    [OUT] strrs：输出选择的目录字符串缓冲区
//    bufsize：选择的目录字符串缓冲区字符最大个数
M_CAPI(BOOL) M_DLG_ChooseDir(HWND hWnd, LPWSTR startDir, LPWSTR title, LPWSTR strrs, size_t bufsize);
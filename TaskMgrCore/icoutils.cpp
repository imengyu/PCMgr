#include "stdafx.h"
#include "icoutils.h"
#include <Uxtheme.h>

HICON MGetShieldIcon(VOID)
{
	static HICON shieldIcon = NULL;

	if (!shieldIcon)
		 LoadIconMetric(NULL, IDI_SHIELD,  LIM_SMALL, &shieldIcon);
	return shieldIcon;
}
HICON MGetShieldIcon2(VOID)
{
	static HICON shieldIcon2 = NULL;
	if (!shieldIcon2)
		LoadIconWithScaleDown(NULL, IDI_SHIELD, 16, 16, &shieldIcon2);
	return shieldIcon2;
}
HBITMAP MCreateBitmap32(HDC hdc, ULONG Width, ULONG Height, PVOID *Bits)
{
	BITMAPINFO bitmapInfo;

	memset(&bitmapInfo, 0, sizeof(BITMAPINFO));
	bitmapInfo.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
	bitmapInfo.bmiHeader.biPlanes = 1;
	bitmapInfo.bmiHeader.biCompression = BI_RGB;

	bitmapInfo.bmiHeader.biWidth = Width;
	bitmapInfo.bmiHeader.biHeight = Height;
	bitmapInfo.bmiHeader.biBitCount = 32;

	return CreateDIBSection(hdc, &bitmapInfo, DIB_RGB_COLORS, Bits, NULL, 0);
}
HBITMAP MIconToBitmap(HICON Icon, ULONG Width, ULONG Height)
{
	HBITMAP bitmap;
	RECT iconRectangle;
	HDC screenHdc;
	HDC hdc;
	HBITMAP oldBitmap;
	BLENDFUNCTION blendFunction = { AC_SRC_OVER, 0, 255, AC_SRC_ALPHA };
	BP_PAINTPARAMS paintParams = { sizeof(paintParams) };
	HDC bufferHdc;
	HPAINTBUFFER paintBuffer;

	iconRectangle.left = 0;
	iconRectangle.top = 0;
	iconRectangle.right = Width;
	iconRectangle.bottom = Height;


	screenHdc = GetDC(NULL);
	hdc = CreateCompatibleDC(screenHdc);
	bitmap = MCreateBitmap32(screenHdc, Width, Height, NULL);
	ReleaseDC(NULL, screenHdc);
	oldBitmap = (HBITMAP)SelectObject(hdc, bitmap);

	paintParams.dwFlags = BPPF_ERASE;
	paintParams.pBlendFunction = &blendFunction;

	paintBuffer = BeginBufferedPaint(hdc, &iconRectangle, BPBF_DIB, &paintParams, &bufferHdc);
	DrawIconEx(bufferHdc, 0, 0, Icon, Width, Height, 0, NULL, DI_NORMAL);
	// If the icon did not have an alpha channel, we need to convert the buffer to PARGB.
	// PhpConvertToPArgb32IfNeeded(paintBuffer, hdc, Icon, Width, Height);
	// This will write the buffer contents to the destination bitmap.
	EndBufferedPaint(paintBuffer, TRUE);

	SelectObject(hdc, oldBitmap);
	DeleteDC(hdc);

	return bitmap;
}
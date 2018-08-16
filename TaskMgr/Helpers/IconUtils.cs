using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace PCMgr.Helpers
{
    class IconUtils
    {
        /// <summary>
        /// 图片缩放
        /// </summary>
        /// <param name="bmp">图片</param>
        /// <param name="width">目标宽度，若为0，表示宽度按比例缩放</param>
        /// <param name="height">目标长度，若为0，表示长度按比例缩放</param>
        public static Bitmap GetThumbnail(Bitmap bmp, int width, int height)
        {
            if (width == 0)
            {
                width = height * bmp.Width / bmp.Height;
            }
            if (height == 0)
            {
                height = width * bmp.Height / bmp.Width;
            }

            Image imgSource = bmp;
            Bitmap outBmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(outBmp);
            g.Clear(Color.Transparent);
            // 设置画布的描绘质量   
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(imgSource, new Rectangle(0, 0, width, height + 1), 0, 0, imgSource.Width, imgSource.Height, GraphicsUnit.Pixel);
            g.Dispose();
            imgSource.Dispose();
            bmp.Dispose();
            return outBmp;
        }
        /// <summary>
        /// 转换Image为Icon
        /// </summary>
        /// <param name="image">要转换为图标的Image对象</param>
        /// <param name="nullTonull">当image为null时是否返回null。false则抛空引用异常</param>
        /// <exception cref="ArgumentNullException" />
        public static Icon ConvertToIcon(Image image, bool nullTonull = false)
        {
            if (image == null)
            {
                if (nullTonull) { return null; }
                throw new ArgumentNullException("image");
            }
            using (MemoryStream msImg = new MemoryStream() , msIco = new MemoryStream())
            {
                image.Save(msImg, ImageFormat.Png);
                using (var bin = new BinaryWriter(msIco))
                {
                    //写图标头部
                    bin.Write((short)0);      //0-1保留
                    bin.Write((short)1);      //2-3文件类型。1=图标, 2=光标
                    bin.Write((short)1);      //4-5图像数量（图标可以包含多个图像）

                    bin.Write((byte)image.Width); //6图标宽度
                    bin.Write((byte)image.Height); //7图标高度
                    bin.Write((byte)0);      //8颜色数（若像素位深>=8，填0。这是显然的，达到8bpp的颜色数最少是256，byte不够表示）
                    bin.Write((byte)0);      //9保留。必须为0
                    bin.Write((short)0);      //10-11调色板
                    bin.Write((short)32);     //12-13位深
                    bin.Write((int)msImg.Length); //14-17位图数据大小
                    bin.Write(22);         //18-21位图数据起始字节

                    //写图像数据
                    bin.Write(msImg.ToArray());

                    bin.Flush();
                    bin.Seek(0, SeekOrigin.Begin);
                    return new Icon(msIco);
                }
            }
        }
    }
}

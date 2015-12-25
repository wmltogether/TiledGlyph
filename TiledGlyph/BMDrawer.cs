using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpFont;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TiledGlyph
{
    class BMDrawer
    {
        GlobalSettings gSettings = new GlobalSettings();
        string fontName = GlobalSettings.fFontName;

        private int fontHeight = GlobalSettings.iFontHeight;
        private int tile_height = GlobalSettings.iTileHeight;
        private int tile_width = GlobalSettings.iTileWidth;
        private int image_width = GlobalSettings.iImageWidth;
        private int image_height = GlobalSettings.iImageHeight;
        private string fTextStrings = GlobalSettings.fTextStrings;
        private enum render_mode
        {
            freetype_normal = 0 ,
            freetype_nearestneighbor = 1,
            freetype_drawtwice = 2
        };
        string grender_mode = Enum.GetName(typeof(render_mode) , GlobalSettings.iGRenderMode);
        private Color penColor = GlobalSettings.cPenColor;


        uint uchar2code(string current_char)
        {
            uint ucode = (uint)Char.ConvertToUtf32(current_char, 0);
            return ucode;
        }
        public Bitmap[] DrawMultiImages(string fTextStrings)
        {
            StringReader fReader = new StringReader(fTextStrings);
            int img_nums;
            int chars_per_page = (image_width / tile_width) * (image_height / tile_height);
            if (fTextStrings.Length < (chars_per_page))
            {
                img_nums = 1;
            }
            else
            {
                img_nums = (fTextStrings.Length / chars_per_page);
                if ((fTextStrings.Length % chars_per_page) > 0)
                {
                    img_nums = (fTextStrings.Length / chars_per_page) + 1;
                }
                
            }
            Bitmap[] multi = new Bitmap[img_nums];
            for (int i = 0; i < img_nums; i++)
                {
                    int pos = 0;
                    string currentString;
                    if (i != img_nums - 1)
                    {
                        char[] buffer = new char[chars_per_page];
                        fReader.Read(buffer, pos, chars_per_page);
                        currentString = new string(buffer);
                        pos += currentString.Length;
                    }
                    else
                    {
                        currentString = fReader.ReadToEnd();
                    }

                Bitmap currentbmp = test_draw(currentString);
                multi[i] = currentbmp;
                }
            return multi;
        }


        public static Bitmap kPasteImage(Bitmap bmp, int newW, int newH ,int kx ,int ky)
        {
            //插值缩放算法
            try
            {
                Bitmap b = new Bitmap(newW, newH);
                Graphics g = Graphics.FromImage(b);

                g.DrawImageUnscaled(bmp, kx, ky);
                g.Dispose();

                return b;
            }
            catch
            {
                return null;
            }
        }
        public static Bitmap kResizeImage(Bitmap bmp, int newW, int newH)
        {
            //插值缩放算法
            try
            {
                Bitmap b = new Bitmap(newW, newH);
                Graphics g = Graphics.FromImage(b);

                // 插值算法的质量
                //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                g.DrawImage(bmp, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
                g.Dispose();

                return b;
            }
            catch
            {
                return null;
            }
        }

        public Bitmap test_draw(string teststrings)
        {
            teststrings = teststrings.Replace("\n" , "");
            teststrings = teststrings.Replace("\r", "");
            Library library = new Library();
            Face face = library.NewFace(fontName, 0);
            
            Bitmap bmp = new Bitmap((int)Math.Ceiling((double)image_width), (int)Math.Ceiling((double)image_height));
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            //g.Clear(Color.Black);
            int x=0, y=0;
            for (int i = 0; i < teststrings.ToCharArray().Length; i++)
            {
                string currentChar0 = teststrings.ToCharArray()[i].ToString();          
                uint glyphIndex = face.GetCharIndex(uchar2code(currentChar0));
                
                face.SetCharSize(0, this.fontHeight, 0, 72);
                face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
                face.Glyph.RenderGlyph(RenderMode.Normal);
                if (this.fontHeight < 14)
                {
                    face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
                    face.Glyph.RenderGlyph(RenderMode.Normal);
                }
                //获取字符对齐
                float left = (float)face.Glyph.Metrics.HorizontalBearingX;
                float right = (float)face.Glyph.Metrics.HorizontalBearingX + (float)face.Glyph.Metrics.Width;
                float top = (float)face.Glyph.Metrics.HorizontalBearingY;
                float bottom = (float)face.Glyph.Metrics.HorizontalBearingY + (float)face.Glyph.Metrics.Height;
                float FHT = this.fontHeight;
                int FHD = (int)Math.Ceiling(FHT);
                int kx = x + face.Glyph.BitmapLeft;
                int ky = (int)Math.Round((float)y + (FHD - face.Glyph.BitmapTop));

                
                //选择渲染模式（1倍 or 2倍）
                if (this.grender_mode == "freetype_nearestneighbor")
                { 
                    face.SetCharSize(0, this.fontHeight *2, 0, 72);
                    face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
                    face.Glyph.RenderGlyph(RenderMode.Normal);
                    FTBitmap ftbmp = face.Glyph.Bitmap;
                    if (ftbmp.Width == 0)
                    {
                        x += this.tile_width;
                        if (x + this.tile_width > this.image_width)
                        {
                            x = 0;
                            y += this.tile_height;
                        }
                        continue;
                    }

                    Bitmap tmpBmp = ftbmp.ToGdipBitmap(this.penColor);

                    tmpBmp = kPasteImage(tmpBmp, tile_width * 2, tile_height * 2, (int)face.Glyph.BitmapLeft ,
                        (int)Math.Round(((float)this.fontHeight * 2 - face.Glyph.BitmapTop)));

                    Bitmap cBmp = kResizeImage(tmpBmp, tmpBmp.Width / 2 , tmpBmp.Height/2);
                    g.DrawImageUnscaled(cBmp, x + GlobalSettings.relativePositionX, y + GlobalSettings.relativePositionY);
                }
                else if (this.grender_mode == "freetype_drawtwice")
                {
                    FTBitmap ftbmp = face.Glyph.Bitmap;
                    if (ftbmp.Width == 0)
                    {
                        x += this.tile_width;
                        if (x + this.tile_width > this.image_width)
                        {
                            x = 0;
                            y += this.tile_height;
                        }
                        continue;
                    }
                    Bitmap cBmp = ftbmp.ToGdipBitmap(this.penColor);

                    g.DrawImageUnscaled(cBmp, kx + GlobalSettings.relativePositionX, ky + GlobalSettings.relativePositionY);
                    g.DrawImageUnscaled(cBmp, kx + GlobalSettings.relativePositionX, ky + GlobalSettings.relativePositionY);//draw twice
                    cBmp.Dispose();

                }
                else
                {
                    FTBitmap ftbmp = face.Glyph.Bitmap;
                    if (ftbmp.Width == 0)
                    {
                        x += this.tile_width;
                        if (x + this.tile_width > this.image_width)
                        {
                            x = 0;
                            y += this.tile_height;
                        }
                        continue;
                    }
                    Bitmap cBmp = ftbmp.ToGdipBitmap(this.penColor);
                    g.DrawImageUnscaled(cBmp, kx + GlobalSettings.relativePositionX, ky + GlobalSettings.relativePositionY);
                    cBmp.Dispose();
                }
                    /*
                    Stroker stroker = new Stroker(library);
                    stroker.Set(1, StrokerLineCap.Round, StrokerLineJoin.Round, 0);
                    face.LoadGlyph(glyphIndex, LoadFlags.NoBitmap, LoadTarget.Normal);
                    face.Glyph.RenderGlyph(RenderMode.Normal);
                    Glyph glyph = face.Glyph.GetGlyph();
                    if (face.Glyph.Format != GlyphFormat.Outline)
                        throw new InvalidCastException("The glyph's format is not GlyphFormat.Outline.");
                    Glyph new_glyph = glyph.StrokeBorder(stroker,false,true);
                    BitmapGlyph bg = new_glyph.ToBitmapGlyph();
                    FTBitmap ft2 = bg.Bitmap;
                    Bitmap ft2Bmp = ft2.ToGdipBitmap(Color.Black);
                    g.DrawImageUnscaled(ft2Bmp, kx, ky);
                    g.DrawImageUnscaled(cBmp, kx, ky);
                     */



                x += this.tile_width;
                if (x + this.tile_width > this.image_width)
                {
                    x = 0;
                    y += this.tile_height;
                }


            }
            g.Dispose();

            return bmp;

        }
    }
}

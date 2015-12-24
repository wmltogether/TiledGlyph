using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace TiledGlyph
{
    public class GlobalSettings
    {
        public static int iFontHeight = 20;
        public static int iTileHeight = 24;
        public static int iTileWidth = 24;
        public static int iImageHeight = 512;
        public static int iImageWidth = 512;
        public static int iGRenderMode = 0;
        public static int iImageCount = 1;
        public static Color cBgColor = Color.Transparent;
        public static Color cPenColor = Color.White;
        public static string fTextStrings = "";
        public static string fFontName = "Font/NotoSansCJKsc-Regular.otf";
        public static bool bUseUnlimitHeight = false;
        public static ImageFormat globalSaveFmt = ImageFormat.Png;


        public static int relativePositionX = 0;
        public static int relativePositionY = 0;
    }
}

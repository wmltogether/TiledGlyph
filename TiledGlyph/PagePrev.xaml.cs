using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Threading;
using System.Threading;
using System.IO;

namespace TiledGlyph
{
    /// <summary>
    /// PagePrev.xaml 的交互逻辑
    /// </summary>
    public partial class PagePrev : UserControl
    {
        public PagePrev()
        {
            InitializeComponent();
        }
        private bool checkBeforeRender(ref string message)
        {
            bool result = true;
            int currentTileWidth = GlobalSettings.iTileHeight;
            int currentTileHeight = GlobalSettings.iTileHeight;
            int currentImageHeight = GlobalSettings.iImageHeight;
            int currentImageWidth = GlobalSettings.iImageWidth;
            System.Drawing.Color currentBgColor = GlobalSettings.cBgColor;
            System.Drawing.Color currentPenColor = GlobalSettings.cPenColor;
            bool bUseUHeight = GlobalSettings.bUseUnlimitHeight;
            if (bUseUHeight)
            {
                int currentStringLength = characterTextBox.Text.Length;
                currentImageHeight = (currentStringLength / (currentImageWidth / currentTileWidth)) * currentTileHeight;
                if ((currentStringLength % (currentImageWidth / currentTileWidth)) != 0)
                {
                    currentImageHeight += currentTileHeight;
                }
                GlobalSettings.iImageHeight = currentImageHeight;
            }

            if (GlobalSettings.iFontHeight < 8)
            {
                result = false;
                message = "Error: Font Size too small.";
            }
            if ((currentImageHeight == 0) || (currentImageWidth == 0))
            {
                result = false;
                message = "Error: Image can't set to zero";
            }
            if ((currentImageHeight < currentTileHeight) || (currentImageWidth < currentTileWidth))
            {
                result = false;
                message = "Error: Image too small or Tile too large.";
            }
            if (currentBgColor == currentPenColor)
            {
                result = false;
                message = "Error: Pen Color can't be equal to background color";
            }
            if (!File.Exists(GlobalSettings.fFontName))
            {
                result = false;
                message = "Error: true type font font not found";
            }
            return result;


        }
        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            bool result = checkBeforeRender(ref message);
            if (!result)
            {
                MessageBox.Show(message);
                return;
            }
            AsynDisplayImage();
        }
        private void AsynDisplayImage()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.SystemIdle,
                    new Action(() =>
                    {
                        //新开一个线程池执行draw
                        BMDrawer bmd = new BMDrawer();
                        string teststrings = characterTextBox.Text;

                        Bitmap bmp = bmd.test_draw(teststrings);
                        BitmapImage cbmp = BitmapToBitmapImage(bmp);
                        image.Height = cbmp.Height;
                        image.Width = cbmp.Width;
                        image.Source = cbmp;
                        if (checkboxShowLine.IsChecked == true)
                        {
                            DrawingVisual drawingVisual = new DrawingVisual();
                            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                            {
                                //
                                // ... draw on the drawingContext
                                //
                                System.Windows.Media.Pen pen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red, (double)0.3f);
                                drawingContext.DrawImage(image.Source, new Rect(0, 0, image.Source.Width, image.Source.Height));
                                int x_lines = (int)Math.Round((float)(int)image.Width / (float)GlobalSettings.iTileWidth);

                                int y_lines = (int)Math.Round((float)(int)image.Height / (float)GlobalSettings.iTileHeight); ;
                                for (int i = 0; i < x_lines + 1; i++)
                                {
                                    drawingContext.DrawLine(pen, new System.Windows.Point(i * GlobalSettings.iTileWidth, 0),
                                        new System.Windows.Point(i * GlobalSettings.iTileWidth, (float)(int)image.Height));
                                }
                                for (int i = 0; i < y_lines + 1; i++)
                                {
                                    drawingContext.DrawLine(pen, new System.Windows.Point(0, i * GlobalSettings.iTileHeight),
                                        new System.Windows.Point((float)(int)image.Width, i * GlobalSettings.iTileHeight));
                                }
                                drawingContext.Close();
                                RenderTargetBitmap nbmp = new RenderTargetBitmap((int)image.Width, (int)image.Height, 96.0, 96.0, PixelFormats.Default);
                                nbmp.Render(drawingVisual);
                                image.Source = nbmp;
                            }
                        }

                    })
               );
            });
        }

        private BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bmp)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }

        private void checkboxShowLine_Unchecked(object sender, RoutedEventArgs e)
        {
            AsynDisplayImage();
        }

        private void checkboxShowLine_Checked(object sender, RoutedEventArgs e)
        {
            AsynDisplayImage();
        }
    }
}

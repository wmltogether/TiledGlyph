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

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
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
                                System.Windows.Media.Pen pen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red, (double)0.5f);
                                drawingContext.DrawImage(image.Source, new Rect(0, 0, image.Source.Width, image.Source.Height));
                                int x_lines = (int)Math.Round((float)image.Source.Width / (float)GlobalSettings.iTileWidth);

                                int y_lines = (int)Math.Round((float)image.Source.Height / (float)GlobalSettings.iTileHeight); ;
                                for (int i = 0; i < x_lines + 1; i++)
                                {
                                    drawingContext.DrawLine(pen, new System.Windows.Point(i * GlobalSettings.iTileWidth, 0),
                                        new System.Windows.Point(i * GlobalSettings.iTileWidth, image.Source.Height));
                                }
                                for (int i = 0; i < y_lines + 1; i++)
                                {
                                    drawingContext.DrawLine(pen, new System.Windows.Point(0, i * GlobalSettings.iTileHeight),
                                        new System.Windows.Point(image.Source.Height, i * GlobalSettings.iTileHeight));
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

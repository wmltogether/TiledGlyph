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
                        image.Source = cbmp;
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
    }
}

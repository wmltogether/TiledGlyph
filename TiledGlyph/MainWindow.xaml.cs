using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Windows.Threading;
using System.Threading;
using FirstFloor.ModernUI.Windows.Controls;
namespace TiledGlyph
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// 这个项目只能用不高于vs2013的版本打开，因为freetype6.dll的编译问题
    /// </summary>

    public partial class MainWindow : ModernWindow
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string path);
        public MainWindow()
        {

            int p = (int)Environment.OSVersion.Platform;
            if (p != 4 && p != 6 && p != 128)
            {
                //Thanks StackOverflow! http://stackoverflow.com/a/2594135/1122135
                string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                path = System.IO.Path.Combine(path, IntPtr.Size == 8 ? "x64" : "x86");
                if (!SetDllDirectory(path))
                    throw new System.ComponentModel.Win32Exception();
                Console.WriteLine(path);
            }
            InitializeComponent();
        }
        /*
        private void testButton_Click(object sender, RoutedEventArgs e)
        {

            AsynDisplayImage();
            //bmp.Save("./test.png", System.Drawing.Imaging.ImageFormat.Png);
 

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
         */
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
using Microsoft.Win32;
using Xceed.Wpf.Toolkit;
using Ookii.Dialogs.Wpf;
using System.Threading;
using System.Windows.Threading;
using System.Drawing;

namespace TiledGlyph
{
    /// <summary>
    /// PageCharacter.xaml 的交互逻辑
    /// </summary>
    public partial class PageCharacter : UserControl
    {
        public PageCharacter()
        {
            InitializeComponent();
        }
        private string readStringFromFile(string fName)
        {
            string str = System.IO.File.ReadAllText(fName,
                EncodingType.GetType(fName));
            str = str.Replace("\r", "");
            str = str.Replace("\n", "");
            str = str.Replace("\t", "");
            return str;
        }
        private void buttonLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            string fName;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "";
            openFileDialog.Filter = "Text File|*.txt";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog().Value)
            {
                fName = openFileDialog.FileName;

                characterTextBox.Text = readStringFromFile(fName);
                stringinfotextblock.Text = String.Format("{0} Characters Loaded.", characterTextBox.Text.Length);
            }

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
            if ((currentImageHeight == 0) || (currentImageWidth == 0)){
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
        private void buttonSaveImage_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            bool checkResult = checkBeforeRender(ref message);
            if (checkResult == false)
            {
                System.Windows.MessageBox.Show(message);
                return;
            }
            System.Drawing.Imaging.ImageFormat fmt = GlobalSettings.globalSaveFmt;
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderBrowserDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog().Value)
            {
                string saveFolderName = folderBrowserDialog.SelectedPath;
                //DO image saving
                if (GlobalSettings.bUseUnlimitHeight)
                {
                    AsynSaveSingleImage(saveFolderName, fmt);
                }
                else
                {
                    AsynSaveMultiImage(saveFolderName, fmt);
                }
                
                System.Windows.MessageBox.Show("Image Saved!");
            }
            
        }
        private void AsynSaveSingleImage(string saveFolderName, System.Drawing.Imaging.ImageFormat fmt)
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
                        if (teststrings.Length > 0)
                        {
                            Bitmap dest = bmd.test_draw(teststrings);
                            dest.Save(string.Format("{0}\\font.{1}", saveFolderName, fmt.ToString()), fmt);
                            dest.Dispose();
                        }
                    })
               );
            });
        }

        private void AsynSaveMultiImage(string saveFolderName, System.Drawing.Imaging.ImageFormat fmt)
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

                        Bitmap[] bmp = bmd.DrawMultiImages(teststrings);

                        if (bmp.Length > 0)
                        {
                            for (int i=0; i < bmp.Length; i++)
                            {
                                Bitmap dest = bmp[i];
                                dest.Save(string.Format("{0}\\font.{1}.{2}", saveFolderName, i, fmt.ToString()), fmt);
                                dest.Dispose();
                            }
                        }
                        
                    })
               );
            });
        }
    }
}

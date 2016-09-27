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
    /// 
    public static class tmps 
    {
        public static string tmpstr;
    };
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
                tmps.tmpstr = readStringFromFile(fName);
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
                        string teststrings = tmps.tmpstr;
                        if (teststrings.Length > 0)
                        {
                            Bitmap tmpBmp = bmd.test_draw(teststrings);
                            Bitmap dest = new Bitmap(tmpBmp.Width, tmpBmp.Height);
                            if (GlobalSettings.bOptmizeAlpha == true)
                            {
                                System.Drawing.Color pixel;
                                for (int x = 0; x < tmpBmp.Width; x++)
                                    for (int y = 0; y < tmpBmp.Height; y++)
                                    {
                                        pixel = tmpBmp.GetPixel(x, y);
                                        int r, g, b, a, Result = 0;
                                        r = pixel.R;
                                        g = pixel.G;
                                        b = pixel.B;
                                        a = pixel.A;
                                        Result = Math.Max(Math.Max(r * a / 255, g * a / 255), b * a / 255);
                                        dest.SetPixel(x, y, System.Drawing.Color.FromArgb(Result, 255, 255, 255));

                                    }
                            }
                            else
                            {
                                dest = tmpBmp;
                            }

                            dest.Save(string.Format("{0}\\font.{1}", saveFolderName, fmt.ToString()), fmt);
                            dest.Dispose();
                            tmpBmp.Dispose();

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
                        string teststrings = tmps.tmpstr;

                        Bitmap[] bmp = bmd.DrawMultiImages(teststrings);

                        if (bmp.Length > 0)
                        {
                            for (int i=0; i < bmp.Length; i++)
                            {
                                Bitmap tmpBmp = bmp[i];
                                Bitmap dest = new Bitmap(tmpBmp.Width, tmpBmp.Height);
                                if (GlobalSettings.bOptmizeAlpha == true)
                                {
                                    System.Drawing.Color pixel;
                                    for (int x = 0; x < tmpBmp.Width; x++)
                                        for (int y = 0; y < tmpBmp.Height; y++)
                                        {
                                            pixel = tmpBmp.GetPixel(x, y);
                                            int r, g, b , a, Result = 0;
                                            r = pixel.R;
                                            g = pixel.G;
                                            b = pixel.B;
                                            a = pixel.A;
                                            Result = Math.Max(Math.Max(r * a / 255, g * a / 255), b * a / 255);
                                            dest.SetPixel(x, y, System.Drawing.Color.FromArgb(Result,255,255,255));

                                        }
                                }
                                else
                                {
                                    Graphics draw = null;
                                    draw = Graphics.FromImage(dest);
                                    draw.DrawImage(tmpBmp, 0, 0);
                                }
                                string fot_name = System.IO.Path.GetFileName(GlobalSettings.fFontName);
                                string name = string.Format("{0}\\{1}_{2}.{3}", saveFolderName, fot_name, i, fmt.ToString());
                                dest.Save(name, fmt);
                                dest.Dispose();
                                tmpBmp.Dispose();
                            }
                        }
                        
                    })
               );
            });
        }

        private void buttonSaveTable_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            bool checkResult = checkBeforeRender(ref message);
            if (checkResult == false)
            {
                System.Windows.MessageBox.Show(message);
                return;
            }
            SaveFileDialog savefiledialog = new SaveFileDialog();
            savefiledialog.RestoreDirectory = true;
            savefiledialog.Filter = "Font Binary（*.bin）|*.bin|Excel csv File(*.csv)|*.csv|BMFont fnt File(*.fnt)|*.fnt";
            if (savefiledialog.ShowDialog() == true)
            {
                string saveFileName = savefiledialog.FileName;
                int currentIndex = savefiledialog.FilterIndex;
                FileStream fs;
                switch (currentIndex)
                {
                    case 1:
                        fs = new FileStream(saveFileName, FileMode.Create, FileAccess.Write);
                        BinaryWriter baseStream = new BinaryWriter(fs);
                        BMDrawer bmd = new BMDrawer();
                        Table.XYWH[] XYWHS = bmd.GetXYWHTable(characterTextBox.Text);
                        baseStream.BaseStream.WriteByte(0x46);
                        baseStream.BaseStream.WriteByte(0x4E);
                        baseStream.BaseStream.WriteByte(0x54);
                        baseStream.BaseStream.WriteByte(0x42);
                        baseStream.Write(BitConverter.GetBytes(XYWHS.Length));
                        foreach (var v in XYWHS)
                        {

                        baseStream.Write(BitConverter.GetBytes(v.charid));
                        baseStream.Write(BitConverter.GetBytes(v.x_pos));
                        baseStream.Write(BitConverter.GetBytes(v.y_pos));
                        baseStream.Write(BitConverter.GetBytes(v.c_width));
                        baseStream.Write(BitConverter.GetBytes(v.c_height));
                        baseStream.Write(BitConverter.GetBytes(v.page_num));
                        }
                        baseStream.Close();
                        fs.Close();
                        break;
                    case 2:
                        fs = new FileStream(saveFileName, FileMode.Create, FileAccess.Write);
                        StreamWriter sw = new StreamWriter(fs);
                        BMDrawer bmd1 = new BMDrawer();
                        Table.XYWH[] XYWHS1 = bmd1.GetXYWHTable(characterTextBox.Text);
                        foreach (var v in XYWHS1)
                        {
                            sw.WriteLine(string.Format("{0},{1},{2},{3},{4},{5}" , v.charid , v.x_pos , v.y_pos ,
                                                                                   v.c_width , v.c_height , v.page_num));
                        }
                        sw.Close();
                        fs.Close();
                        break;
                    case 3:
                        // bmfont fnt fine
                        fs = new FileStream(saveFileName, FileMode.Create, FileAccess.Write);
                        StreamWriter sw1 = new StreamWriter(fs);
                        BMDrawer bmd2 = new BMDrawer();
                        Table.XYWH[] XYWHS2 = bmd2.GetXYWHTable(characterTextBox.Text);
                        //write fnt header
                        int lineHeight = GlobalSettings.iTileHeight,
                            baseHeight = GlobalSettings.iTileHeight, 
                            scaleW = GlobalSettings.iImageWidth, 
                            scaleH = GlobalSettings.iImageWidth, 
                            pages = (int)XYWHS2[XYWHS2.Length - 1].page_num;
                        string bmfont_name = System.IO.Path.GetFileName(GlobalSettings.fFontName);
                        sw1.WriteLine(string.Format("info face=\"{0}\" bold=0 italic=0 charset=\"\" unicode=1 stretchH=100 smooth=1 aa=4 padding=0,0,0,0 spacing=0,0 outline=0", bmfont_name));
                        sw1.WriteLine(string.Format("common lineHeight={0} base={1} scaleW={2} scaleH={3} pages={4} packed=0 alphaChnl=1 redChnl=0 greenChnl=0 blueChnl=0",
                                        lineHeight,
                                        baseHeight,
                                        scaleW,
                                        scaleH,
                                        pages)
                                        
                                        );
                        
                        sw1.WriteLine(string.Format("page id=0 file=\"{0}_0.png\"" , bmfont_name));
                        sw1.WriteLine(string.Format("chars count={0}", XYWHS2.Length));
                        foreach (var v in XYWHS2) {
                            sw1.WriteLine(string.Format("char id={0}    x={1}     y={2}    width={3}     height={4}     xoffset={5}    yoffset={6}    xadvance={7}    page={8}  chnl=15",

                                            v.charid , 
                                            v.x_pos , 
                                            v.y_pos , 
                                            v.c_width , 
                                            v.c_height , 
                                            0, 
                                            0 ,
                                            v.c_width ,
                                            v.page_num)
                                            );
                        }

                        sw1.Close();
                        fs.Close();


                        break;

                    default:
                        break;
                }
                
            }
            


        }

        private void characterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            tmps.tmpstr = characterTextBox.Text;
        }
    }
}

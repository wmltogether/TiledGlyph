using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;


namespace TiledGlyph
{
    /// <summary>
    /// PageSettings.xaml 的交互逻辑
    /// </summary>
    public partial class PageSettings : UserControl
    {
        public PageSettings()
        {
            InitializeComponent();
        }

        private bool checkNumbic(string str){
            Regex r = new Regex(@"^[0-9]*$");
            if (!r.IsMatch(str))
            {
                return false;
            }
            return true;
        }

        /*
        * 
        * 
        */
        private void textboxTileWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tc = textboxTileWidth.Text.Trim();
            if (!checkNumbic(tc))
            {
                textboxTileWidth.Text = "16";
                return;
            }
            int tileWidth = int.Parse(tc);
            if (tileWidth > 128)
            {
                textboxTileWidth.Text = "16";
                return;
            }
            GlobalSettings.iTileWidth = tileWidth;
        }

        private void textboxTileHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tc = textboxTileHeight.Text.Trim();
            if (!checkNumbic(tc))
            {
                textboxTileHeight.Text = "16";
                return;
            }
            int tileHeight = int.Parse(tc);
            if (tileHeight > 128)
            {
                textboxTileHeight.Text = "16";
                return;
            }
            GlobalSettings.iTileHeight = tileHeight;
        }

        private void textboxImageWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tc = textboxImageWidth.Text.Trim();
            if (!checkNumbic(tc))
            {
                textboxImageWidth.Text = "512";
                return;
            }
            int imageWidth = int.Parse(tc);
            if (imageWidth < 16)
            {
                textboxImageWidth.Text = "512";
                return;
            }
            GlobalSettings.iImageWidth = imageWidth;
        }

        private void textboxImageHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tc = textboxImageHeight.Text.Trim();
            if (!checkNumbic(tc))
            {
                textboxImageHeight.Text = "512";
                return;
            }
            int imageHeight = int.Parse(tc);
            if (imageHeight < 16)
            {
                textboxImageHeight.Text = "512";
                return;
            }
            GlobalSettings.iImageHeight = imageHeight;
        }

        private void textboxFontHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tc = textboxFontHeight.Text.Trim();
            if (!checkNumbic(tc))
            {
                textboxFontHeight.Text = "16";
                return;
            }
            int fontHeight = int.Parse(tc);
            if (fontHeight > 128)
            {
                textboxFontHeight.Text = "16";
                return;
            }
            GlobalSettings.iFontHeight = fontHeight;
        }

        private void comboboxRenderMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int currentMode = comboboxRenderMode.SelectedIndex;
            GlobalSettings.iGRenderMode = currentMode;           
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {

            GlobalSettings.iTileWidth = int.Parse(textboxTileWidth.Text.Trim());
            GlobalSettings.iTileHeight = int.Parse(textboxTileHeight.Text.Trim());
            GlobalSettings.iFontHeight = int.Parse(textboxFontHeight.Text.Trim());
            GlobalSettings.iGRenderMode = comboboxRenderMode.SelectedIndex;
            GlobalSettings.iImageWidth = int.Parse(textboxImageWidth.Text.Trim());
            GlobalSettings.iImageHeight = int.Parse(textboxImageHeight.Text.Trim());
            GlobalSettings.fFontName = textbox_FontName.Text;
            

        }

        private void penColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            string currentColor = penColorPicker.SelectedColor.ToString();
            if (currentColor != null)
            {
                string c = currentColor;
                textboxPenBrushColor.Text = c;
                System.Drawing.ColorConverter colConvert = new System.Drawing.ColorConverter();
                System.Drawing.Color color = (System.Drawing.Color)colConvert.ConvertFromString(c);
                GlobalSettings.cPenColor = color;
            } 

        }

        private void bgColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            string currentColor = bgColorPicker.SelectedColor.ToString();
            if (currentColor != null)
            {
                string c = currentColor;
                textboxBgBrushColor.Text = c;
                System.Drawing.ColorConverter colConvert = new System.Drawing.ColorConverter();
                System.Drawing.Color color = (System.Drawing.Color)colConvert.ConvertFromString(c);
                GlobalSettings.cBgColor = color;
            } 
        }

        private void checkboxUseUHeight_Checked(object sender, RoutedEventArgs e)
        {
            if (checkboxUseUHeight.IsChecked == true){
                GlobalSettings.bUseUnlimitHeight = true;
            }
        }
        private void checkboxUseUHeight_UnChecked(object sender, RoutedEventArgs e)
        {
            if (checkboxUseUHeight.IsChecked == false)
            {
                GlobalSettings.bUseUnlimitHeight = false;
            }
            
        }

        private void buttonChooseFont_Click(object sender, RoutedEventArgs e)
        {
            string fName;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "";
            openFileDialog.Filter = "True Type Font|*.ttf;*.otf";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog().Value)
            {
                fName = openFileDialog.FileName;
                textbox_FontName.Text = fName;
                GlobalSettings.fFontName = textbox_FontName.Text;
            }

        }

        private void comboboxSaveImageType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int currentMode = comboboxRenderMode.SelectedIndex;
            if (currentMode == 0)
            {
                GlobalSettings.globalSaveFmt = System.Drawing.Imaging.ImageFormat.Png;
            }
            if (currentMode == 1)
            {
                GlobalSettings.globalSaveFmt = System.Drawing.Imaging.ImageFormat.Bmp;
            }
        }

        private void textboxRPositionX_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tc = textboxRPositionX.Text.Trim();
            if (!checkNumbic(tc))
            {
                textboxRPositionX.Text = "0";
                return;
            }
            int RPositionX = int.Parse(tc);
            if (RPositionX > GlobalSettings.iFontHeight)
            {
                textboxRPositionX.Text = "0";
                return;
            }
            GlobalSettings.relativePositionX = RPositionX;
        }

        private void textboxRpositionY_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tc = textboxRpositionY.Text.Trim();
            if (!checkNumbic(tc))
            {
                textboxRpositionY.Text = "0";
                return;
            }
            int RPositionY = int.Parse(tc);
            if (RPositionY > GlobalSettings.iFontHeight)
            {
                textboxRpositionY.Text = "0";
                return;
            }
            GlobalSettings.relativePositionY = RPositionY;
        }


    }
}

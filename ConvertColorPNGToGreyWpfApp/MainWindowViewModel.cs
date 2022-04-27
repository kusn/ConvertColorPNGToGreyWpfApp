using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace ConvertColorPNGToGreyWpfApp
{
    public class MainWindowViewModel : ViewModel
    {
        private PixelFormat pxf = PixelFormat.Format24bppRgb;
        private Bitmap _bmp;
        private BitmapImage _colorBitmapImage;
        private BitmapImage _grayBitmapImage;
        private string _originalFileName;
        private string _newFileName;

        public BitmapImage ColoBitmapImage
        {
            get => _colorBitmapImage;
            set => Set(ref _colorBitmapImage, value);
        }

        public BitmapImage GrayBitmapImage
        {
            get => _grayBitmapImage;
            set => Set(ref _grayBitmapImage, value);
        }

        public MainWindowViewModel()
        {
            
        }
        
        private LambdaCommand _openImageCommand;
        public LambdaCommand OpenImageCommand => _openImageCommand
            ??= new LambdaCommand(OnOpenImageCommandExecuted, CanOpenImageCommandExecute);
        private void OnOpenImageCommandExecuted(object p)
        {

            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                _originalFileName = ofd.FileName;
                ColoBitmapImage = new BitmapImage(new Uri(_originalFileName));
                _bmp = new Bitmap(ofd.FileName);
            }
        }
        private bool CanOpenImageCommandExecute(object p) => true;

        private LambdaCommand _convertImageCommand;

        public LambdaCommand ConvertImageCommand => _convertImageCommand
            ??= new LambdaCommand(OnConvertImageCommandExecuted, CanConvertImageCommandExecute);

        private void OnConvertImageCommandExecuted(object p)
        {
            MakeGray(_bmp);
        }
        private bool CanConvertImageCommandExecute(object parameter) => true;


        private void MakeGray(Bitmap bmp)
        {
            PixelFormat pxf = PixelFormat.Format24bppRgb;
            
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);

            IntPtr ptr = bmpData.Scan0;

            int numBytes = bmpData.Stride * bmp.Height;
            int widthBytes = bmpData.Stride;
            byte[] rgbValues = new byte[numBytes];

            Marshal.Copy(ptr, rgbValues, 0, numBytes);

            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {

                int value = rgbValues[counter] + rgbValues[counter + 1] + rgbValues[counter + 2];
                byte color_b = 0;

                color_b = Convert.ToByte(value / 3);


                rgbValues[counter] = color_b;
                rgbValues[counter + 1] = color_b;
                rgbValues[counter + 2] = color_b;

            }
            Marshal.Copy(rgbValues, 0, ptr, numBytes);

            bmp.UnlockBits(bmpData);
            _newFileName = "GrayBitmapImage.png";
            if (!File.Exists(_newFileName))
                File.Create(_newFileName);
            bmp.Save(_newFileName, ImageFormat.Png);
            var dir = Directory.GetCurrentDirectory();
            GrayBitmapImage = new BitmapImage(new Uri(dir + "\\" + _newFileName));
        }
    }
}

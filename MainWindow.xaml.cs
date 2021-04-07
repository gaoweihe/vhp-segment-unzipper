using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace vhp_segmentunzipper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Controls.TextBox inputFilePathTextBox;
        private System.Windows.Controls.TextBox outputFilePathTextBox;
        private System.Windows.Controls.Image displayImage;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InputFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog inputOfd = new System.Windows.Forms.OpenFileDialog();
            if (inputOfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                inputFilePathTextBox.Text = inputOfd.FileName;
            }
        }

        private void InputFilePathTextBox_Initialized(object sender, EventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            this.inputFilePathTextBox = textBox;
        }

        private void OutputFilePathTextBox_Initialized(object sender, EventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            this.outputFilePathTextBox = textBox;
        }

        private void OutputFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog outputOfd = new System.Windows.Forms.OpenFileDialog();
            if (outputOfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                outputFilePathTextBox.Text = outputOfd.FileName;
            }
        }

        private void UnzipButton_Click(object sender, RoutedEventArgs e)
        {
            FileInfo inputFi = new FileInfo(inputFilePathTextBox.Text);
            long inputFileSize = inputFi.Length;
            FileStream inputFs = new FileStream(inputFilePathTextBox.Text, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader inputBr = new BinaryReader(inputFs);
            byte[] inputBuffer = inputBr.ReadBytes((int)inputFileSize);
            short[] tempBuffer = new short[2048 * 1216 * 2];
            byte[] outputBuffer = new byte[1760 * 1024 * 1];
            int currNum = 0;
            int x0 = 0;
            int y0 = 0;
            int x1 = 0;
            int y1 = 0;
            long cnt = 0;
            long cntVal = 0;
            // currPt = pInBuff + 4;
            // shortiPt = pTempBuff;
            long currPt = 4;
            long shortiPt = 0;
            for (cnt = 0; cnt < 1760 * 1024; cnt++)
            {
                if (cntVal <= 0)
                {
                    currNum = inputBuffer[currPt];
                    currPt++;
                    currNum = currNum * 0x100 + inputBuffer[currPt];
                    currPt++;
                    cntVal = inputBuffer[currPt];
                    currPt++;
                    cntVal = cntVal * 0x100 + inputBuffer[currPt];
                    currPt++;
                    cntVal = cntVal * 0x100 + inputBuffer[currPt];
                    currPt++;
                }
                if (cntVal > 0)
                {
                    tempBuffer[shortiPt] = (short)currNum;
                    shortiPt++;
                    cntVal--;
                }
            }
            x0 = 0;
            x1 = 1760 - 1;
            y0 = 0;
            y1 = 1024 - 1;
            // currPt = pOutBuff;
            currPt = 0;
            for (cnt = 0; cnt < y1; cnt++)
            {
                shortiPt = (1023 - cnt - y0) * 1760 + x0;
                for (cntVal = 0; cntVal < x1; cntVal++)
                {
                    outputBuffer[currPt] = (byte)(tempBuffer[shortiPt] % 0x100);
                    currPt++;
                    shortiPt++;
                }
                outputBuffer[currPt] = (byte)(tempBuffer[shortiPt] % 0x100);
                currPt++;
            }

            inputBr.Close();
            inputFs.Close();

            FileStream outputFs = new FileStream(outputFilePathTextBox.Text, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            BinaryWriter outputBw = new BinaryWriter(outputFs);
            outputBw.Write(outputBuffer, 0, 1760 * 1024 * 1);
            outputBw.Close();
            outputFs.Close();

            Bitmap bitmap = new Bitmap(1760, 1024);
            for (int y = 0; y < 1024; y++)
            {
                for (int x = 0; x < 1760; x++)
                {
                    int rOffset = 0;
                    int gOffset = 85;
                    int bOffset = 170;
                    byte currOrganNum = outputBuffer[y * 1760 + x];
                    int r = rOffset + currOrganNum;
                    r = r % 255;
                    int g = gOffset + currOrganNum;
                    g = g % 255;
                    int b = bOffset + currOrganNum;
                    b = b % 255;
                    System.Drawing.Color currColor = System.Drawing.Color.FromArgb(r, g, b);
                    bitmap.SetPixel(x, y, currColor);
                }
            }
            bitmap.Save(@"D:\test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            BitmapImage bitmapImage = new BitmapImage();
            MemoryStream ms = new MemoryStream();
            //byte[] bytes = ms.GetBuffer();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            displayImage.Source = bitmapImage;
        }

        private void DisplayImage_Initialized(object sender, EventArgs e)
        {
            this.displayImage = sender as System.Windows.Controls.Image;
        }
    }
}

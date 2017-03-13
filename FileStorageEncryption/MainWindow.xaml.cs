using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace FileStorageEncryption
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string files = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                //Multiselect = true
            };
            if (ofd.ShowDialog() == true)
            {
                /*foreach (string fileName in ofd.FileNames)
                {
                    files += $"{fileName}, ";
                }*/
                files = ofd.FileName;
                openFileTextBox.Text = ofd.FileName;
            }
        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            FileEncryptionAndDecryption.Encrypt(files, "outputFile.kil",passwordTextBox.Text);
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            FileEncryptionAndDecryption.Decrypt(files,passwordTextBox.Text);
        }        
        
    }

}

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
using System.ComponentModel;

namespace FileStorageEncryption
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {

        string[] files;
        string _outputFolderPathForEncryption = "";
        string _outputFolderPathForDecryption = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(addFileButton))
            {
                OpenFileDialog ofd = new OpenFileDialog()
                {
                    Multiselect = true
                };
                if (ofd.ShowDialog() == true)
                {
                    files = ofd.FileNames;
                    string names = "";
                    foreach (string fileName in ofd.FileNames)
                    {
                        names += $"{fileName}, ";
                    }
                    openFileTextBox.Text = names;
                }
            }
            else if (sender.Equals(addFileButton_Decrypt))
            {
                OpenFileDialog ofd = new OpenFileDialog()
                {
                    Multiselect = false
                };
                if (ofd.ShowDialog() == true)
                {
                    openFileTextBox_Decrypt.Text = ofd.FileName;
                }
            }
        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            string fileFormatted = "";
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                if (i >= files.Length - 1)
                {
                    fileFormatted += file;
                }
                else
                    fileFormatted += file + ",";
            }           
            FileEncryptionAndDecryption.Encrypt(fileFormatted, _outputFolderPathForEncryption + "/Encrypted101.kil", passwordTextBox.Text);
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            //Only one file is needed            
            FileEncryptionAndDecryption.Decrypt(openFileTextBox_Decrypt.Text, _outputFolderPathForDecryption, passwordTextBox_Decrypt.Text);
        }

        private void OutFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (sender.Equals(outputFileButton))
                {
                    _outputFolderPathForEncryption = fbd.SelectedPath;
                    outputFileTextBox.Text = fbd.SelectedPath;
                }
                else if(sender.Equals(outputFileButton_Decrypt))
                {
                    _outputFolderPathForDecryption = fbd.SelectedPath;
                    outputFileTextBox_Decrypt.Text = fbd.SelectedPath;
                }
            }
        }
    }

}

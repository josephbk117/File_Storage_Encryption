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
        public MainWindow()
        {
            InitializeComponent();      

        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
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

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            string fileFormatted = "";
            for(int i=0;i<files.Length;i++)
            {
                string file = files[i];
                if (i >= files.Length - 1)
                {
                    fileFormatted += file;
                }
                else
                    fileFormatted += file + ",";
            }
            Console.WriteLine("Formated file = " + fileFormatted);
            FileEncryptionAndDecryption.Encrypt(fileFormatted, "outputFile.kil",passwordTextBox.Text);
            
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            //Only one file is needed
            FileEncryptionAndDecryption.Decrypt(files[0],passwordTextBox.Text);
        }        
        
    }

}

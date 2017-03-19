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
            AddToDockList("bhyb");
            AddToDockList("poplol");
        }

        private void AddToDockList(string fileName)
        {
            DockPanel pan = new DockPanel();
            DockPanel.SetDock(pan, Dock.Top);
            Label lbl1 = new Label();
            lbl1.Content = fileName;
            lbl1.HorizontalAlignment = HorizontalAlignment.Left;
            DockPanel.SetDock(lbl1, Dock.Left);
            pan.Children.Add(lbl1);
            Label lbl2 = new Label();
            lbl2.Content = "OMOMOMOM";
            lbl2.HorizontalAlignment = HorizontalAlignment.Right;
            DockPanel.SetDock(lbl2, Dock.Right);
            pan.Children.Add(lbl2);
            dockList.Children.Add(pan);
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
                    Filter = "(.KIL)|*.kil"                  
                };                
                if (ofd.ShowDialog() == true)
                {
                    openFileTextBox_Decrypt.Text = ofd.FileName;
                }
            }
        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            if(!ValidateEncryptButton())
            {
                return;
            }
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
            FileEncryptionAndDecryption.Encrypt(fileFormatted, _outputFolderPathForEncryption, passwordTextBox.Text);
        }

        private bool ValidateEncryptButton()
        {
            if (string.IsNullOrWhiteSpace(openFileTextBox.Text))
            {
                MessageBox.Show("There are no files to encrypt", "Error", MessageBoxButton.OK);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(outputFileTextBox.Text))
            {
                MessageBox.Show("There is no output file specified", "Error", MessageBoxButton.OK);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(passwordTextBox.Text) || passwordTextBox.Text == "password")
            {
                MessageBox.Show("No valid password provided", "Error", MessageBoxButton.OK);
                return false;
            }
            return true;
        }
        private bool ValidateDecryptButton()
        {

            if (string.IsNullOrWhiteSpace(openFileTextBox_Decrypt.Text))
            {
                MessageBox.Show("There are no files to decrypt", "Error", MessageBoxButton.OK);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(outputFileTextBox_Decrypt.Text))
            {
                MessageBox.Show("There is no output folder specified", "Error", MessageBoxButton.OK);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(passwordTextBox_Decrypt.Text) || passwordTextBox.Text == "password")
            {
                MessageBox.Show("No valid password provided", "Error", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            //Only one file is needed         
            if(!ValidateDecryptButton())
            {
                return;
            }
            FileEncryptionAndDecryption.Decrypt(openFileTextBox_Decrypt.Text, _outputFolderPathForDecryption, passwordTextBox_Decrypt.Text);
        }

        private void OutFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(outputFileButton))
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "(.KIL)|*.kil";
                if (sfd.ShowDialog() == true)
                {
                    _outputFolderPathForEncryption = sfd.FileName;
                    outputFileTextBox.Text = sfd.FileName;
                }
            }
            else if (sender.Equals(outputFileButton_Decrypt))
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _outputFolderPathForDecryption = fbd.SelectedPath;
                    outputFileTextBox_Decrypt.Text = fbd.SelectedPath;
                }
            }
        }
    }

}

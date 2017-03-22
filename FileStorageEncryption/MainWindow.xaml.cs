using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FileStorageEncryption
{
    //TODO:Use seperate bg workers for enc and decr and set up diff bg work completed and progress changed
    public partial class MainWindow : Window
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private static List<ProgressDisplay> progressDisplayList = new List<ProgressDisplay>();
        private static BackgroundWorker bgw_encryption = new BackgroundWorker();
        private static BackgroundWorker bgw_decryption = new BackgroundWorker();

        string[] files;
        string _outputFolderPathForEncryption = "";
        string _outputFolderPathForDecryption = "";

        public static bool IsAssociated()
        {
            return (Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.kil", false) == null);
        }
        public static void Associate()
        {
            RegistryKey fileReg = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.kil");
            RegistryKey appReg = Registry.CurrentUser.CreateSubKey("Software\\Classes\\Applications\\FileStorageEncryption.exe");
            RegistryKey appAssoc = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.kil");
            fileReg.CreateSubKey("DefaultIcon").SetValue("", "C:\\Users\\josep_000\\Pictures\\encryptProj.ico");
            fileReg.CreateSubKey("PerceivedType").SetValue("", "Text");

            Console.WriteLine("App location = " + Environment.GetCommandLineArgs()[0]);

            appReg.CreateSubKey("shell\\open\\command").SetValue("", "\"" + Environment.GetCommandLineArgs()[0] + "\" %1");
            appReg.CreateSubKey("shell\\edit\\command").SetValue("", "\"" + Environment.GetCommandLineArgs()[0] + "\" %1");
            appReg.CreateSubKey("DefaultIcon").SetValue("", "C:\\Users\\josep_000\\Pictures\\encryptProj.ico");

            appAssoc.CreateSubKey("UserChoice").SetValue("Progid", "Applications\\FileStorageEncryption.exe");
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        public MainWindow()
        {
            if (IsAssociated())
            {
                Associate();
            }
            else
            {
                Console.WriteLine("Has been associated");
            }
            InitializeComponent();
            bgw_encryption.WorkerReportsProgress = true;
            bgw_encryption.ProgressChanged += Bgw_EncryptionProgressChanged;
            bgw_encryption.RunWorkerCompleted += BgwEncryptionWorkDone;
            bgw_decryption.RunWorkerCompleted += BgwDecryptionWorkDone;
        }

        private void BgwDecryptionWorkDone(object sender, RunWorkerCompletedEventArgs e)
        {
            //TODO : Delete file
            MessageBox.Show("Sucessfully decrypted the file", "Decryption Done", MessageBoxButton.OK, MessageBoxImage.Information);
            if(!keepFileCheckBox.IsChecked.Value)
            {
                System.IO.File.Delete(openFileTextBox_Decrypt.Text);
            }
        }

        private void BgwEncryptionWorkDone(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Sucessfully encrypted the files", "Encryption Done", MessageBoxButton.OK, MessageBoxImage.Information);

            dockList.Children.Clear();
            progressDisplayList.Clear();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            String[] args = Environment.GetCommandLineArgs();
            int argCount = args.Length;

            if (argCount > 1)
            {
                openFileTextBox_Decrypt.Text = args[1];
                tabControl.SelectedIndex = 1;
            }

        }


        private static void Bgw_EncryptionProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //TODO gives number of files completed     

            for (int i = 0; i < e.ProgressPercentage; i++)
            {
                progressDisplayList[i].SetProgress(100);
            }
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
                        progressDisplayList.Add(new ProgressDisplay(dockList, fileName));
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
            if (!ValidateEncryptButton())
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
            if (!FileEncryptionAndDecryption.Encrypt(fileFormatted, _outputFolderPathForEncryption, passwordTextBox.Text, bgw_encryption))
            {
                MessageBox.Show("Currenlty Decrypting , Wait Till Process Is Finished", "Task Running", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
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
            else if (string.IsNullOrWhiteSpace(passwordTextBox_Decrypt.Text) || passwordTextBox_Decrypt.Text == "password")
            {
                MessageBox.Show("No valid password provided", "Error", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            //Only one file is needed         
            if (!ValidateDecryptButton())
            {
                return;
            }
            if (!FileEncryptionAndDecryption.Decrypt(openFileTextBox_Decrypt.Text, _outputFolderPathForDecryption, passwordTextBox_Decrypt.Text, bgw_decryption))
            {
                MessageBox.Show("Currenlty Encrypting , Wait Till Process Is Finished", "Task Running", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
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

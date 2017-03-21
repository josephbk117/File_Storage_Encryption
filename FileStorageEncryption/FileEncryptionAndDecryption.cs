using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

public static class FileEncryptionAndDecryption
{

    private static BackgroundWorker bgw;


    private static void Bgw_DoEncryptionWork(object sender, DoWorkEventArgs e)
    {
        string[] argValues = (string[])e.Argument;

        string[] inputFiles = argValues[0].Split(',');
        string outputFile = argValues[1];
        string password = argValues[2];


        string hashedPassword = GenerateHash(password);

        List<byte[]> allBuffers = new List<byte[]>();

        int completedCount = 0;
        foreach (string inputFile in inputFiles)
        {
            FileStream fs = new FileStream(inputFile, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            int fileSize = (int)fs.Length;

            string formattedName = inputFile.Substring(inputFile.LastIndexOf('\\') + 1);//hello.cs - size : 8
            byte[] buffer = new byte[fileSize + formattedName.Length + 1 + 4];// 1 for file name size, 4 for file size
            byte[] temp = br.ReadBytes(fileSize);

            //Put in meta data of file size , first 4 bytes
            byte[] sizeBytes = BitConverter.GetBytes(fileSize);

            for (int i = 0; i < sizeBytes.Length; i++)
            {
                buffer[i] = sizeBytes[i];
            }

            buffer[4] = (byte)formattedName.Length; //File name length

            for (int i = 5; i < formattedName.Length + 5; i++)
            {
                buffer[i] = Convert.ToByte(formattedName[i - 5]);
            }
            //Buffer is filled with meta data now .. now add the data
            for (int i = formattedName.Length + 5; i < buffer.Length; i++)
            {
                int offset = i - (formattedName.Length + 5);
                buffer[i] = CorrectEncryptByteValue(temp[offset] + 3);
                buffer[i] = (byte)(buffer[i] ^ hashedPassword[offset % hashedPassword.Length]);
            }

            br.Close();
            fs.Close();

            allBuffers.Add(buffer);
            //Gives the number that has been completed

            bgw.ReportProgress(++completedCount);
        }

        int totalSize = 0;

        //The main buffer over here
        FileStream fr = new FileStream(outputFile, FileMode.Create);
        BinaryWriter wr = new BinaryWriter(fr);

        for (int i = 0; i < allBuffers.Count; i++)
        {
            wr.Write(allBuffers[i]);
            wr.Flush();
            totalSize += allBuffers[i].Length;
        }

        wr.Close();
        fr.Close();
        wr.Dispose();
        fr.Dispose();
    }

    public static bool Encrypt(string inputFiles, string outputFile, string password, BackgroundWorker _bgw)
    {
        if (bgw == null)
        {
            bgw = _bgw;
        }
        if (!bgw.IsBusy)
        {
            bgw.DoWork += Bgw_DoEncryptionWork;
            string[] vals = { inputFiles, outputFile, password };
            bgw.RunWorkerAsync(vals);
            return true;
        }
        else
            return false;

    }
    public static bool Decrypt(string inputFile, string outputFolderPath, string password, BackgroundWorker _bgw)
    {
        if (bgw == null)
        {
            bgw = _bgw;
        }
        if (!bgw.IsBusy)
        {
            bgw.DoWork += Bgw_DoDecryptionWork;
            string[] vals = { inputFile, outputFolderPath, password };
            bgw.RunWorkerAsync(vals);
            return true;
        }
        else
            return false;
    }

    private static void Bgw_DoDecryptionWork(object sender, DoWorkEventArgs e)
    {
        string[] argValues = (string[])e.Argument;

        string inputFile = argValues[0];
        string outputFolderPath = argValues[1];
        string password = argValues[2];


        string hashedPassword = GenerateHash(password);
        FileStream fs = new FileStream(inputFile, FileMode.Open);
        BinaryReader br = new BinaryReader(fs);

        byte[] buffer = br.ReadBytes((int)fs.Length);
        //------------From here on should be block by block basis----------//
        //------------Find partition points----------// 
        List<int> partitionIndexes = new List<int>();

        int p = 0;
        Console.WriteLine("Buffer length = " + buffer.Length);
        while (p < buffer.Length)
        {
            byte[] byteValue = { buffer[p], buffer[p + 1], buffer[p + 2], buffer[p + 3] };//-get file size 4 bytes
            int _fileSize = BitConverter.ToInt32(byteValue, 0);

            int _fileNameLength = (int)buffer[p + 4];                                     //-get file name length - 1byte

            string _fileName = "";

            for (int i = p + 5; i < _fileNameLength + p + 5; i++)
            {
                _fileName += Convert.ToChar(buffer[i]);
            }
            byte[] _data = new byte[_fileSize];

            int k = 0;
            for (int i = p + _fileNameLength + 5; i < p + _fileSize + 5; i++)
            {
                _data[k] = (byte)(buffer[i] ^ hashedPassword[k % hashedPassword.Length]);
                _data[k] = CorrectDecryptByteValue(_data[k] - 3);
                k++;
            }

            FileStream _fr = new FileStream(outputFolderPath + "/" + _fileName, FileMode.Create);
            BinaryWriter _wr = new BinaryWriter(_fr);
            _wr.Write(_data);

            _wr.Close();
            _fr.Close();

            //Move to next index
            p = p + _fileSize + _fileNameLength + 5;
        }

        br.Close();
        fs.Close();
    }

    private static byte CorrectEncryptByteValue(int value)
    {
        if (value > 255)
        {
            value = (value - 1) % 255;
        }
        else if (value < 0)
        {
            value = 255 + value;
        }
        return (byte)value;
    }
    private static byte CorrectDecryptByteValue(int value)
    {
        if (value < 0)
        {
            value = 255 + (value + 1);
        }
        return (byte)value;
    }
    private static string GenerateHash(string password)
    {
        MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(password);
        byte[] hash = md5.ComputeHash(inputBytes);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("X2"));
        }
        return sb.ToString();
    }
}

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

public static class FileEncryptionAndDecryption
{

    private static BackgroundWorker bgw = new BackgroundWorker();


    private static void Bgw_DoWork(object sender, DoWorkEventArgs e)
    {
        string[] vals = (string[])e.Argument;
        string password = vals[2];
        string[] inputFiles = vals[0].Split(',');

        foreach (string file in inputFiles)
        {
            Console.WriteLine(" Checking File : " + file);
        }

        string outputFile = vals[1];
        string hashedPassword = GenerateHash(password);

        List<byte[]> allBuffers = new List<byte[]>();

        //TODO : Add new meta data in front : 1st - 4 bytes ,size of file in bytes
        //TODO : Add size of file name as 1 byte right after that
        //TODO : Add Encrypted file data till end
        //TODO : Set next 4 bytes to get file size of next file, 1 byte for file name size then keep adding

        //For Debugging
        int prevEndIndex = 0;
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
            Console.WriteLine("Size bytes are : "); // 0-3 filled with file size
            for (int i = 0; i < sizeBytes.Length; i++)
            {
                buffer[i] = sizeBytes[i];
                Console.Write(" " + buffer[i]);
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

            //Test decryption
            byte[] sizeVal = { buffer[0], buffer[1], buffer[2], buffer[3] };
            int fileSizeT = BitConverter.ToInt32(sizeVal, 0);
            int fileNameSizeT = (int)buffer[4];
            string fileNameT = "";
            for (int i = 5; i < fileNameSizeT + 5; i++)
            {
                fileNameT += (char)buffer[i];
            }

            Console.WriteLine(" META DATA : file size = " + fileSizeT + ",File name size = " + fileNameSizeT + ", name size from text = " + fileNameT.Length + ", File name = "
                + fileNameT);            
            Console.WriteLine("Started index of file = " + prevEndIndex);
            prevEndIndex = prevEndIndex + (4 + 1 + formattedName.Length + fileSize);

            allBuffers.Add(buffer);
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
        Console.WriteLine("Total size = " + totalSize);

        wr.Close();
        fr.Close();
        wr.Dispose();
        fr.Dispose();
    }

    public static void Encrypt(string inputFiles, string outputFile, string password)
    {
        bgw.DoWork += Bgw_DoWork;
        string[] vals = { inputFiles, outputFile, password };
        bgw.RunWorkerAsync(vals);
    }

    public static void Decrypt(string inputFile, string outputFolderPath,string password)
    {
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
            Console.WriteLine("Decrypt File Size = " + _fileSize);
            int _fileNameLength = (int)buffer[p + 4];                                     //-get file name length - 1byte
            Console.WriteLine("Decrypt file name length  = " + _fileNameLength);
            string _fileName = "";

            for (int i = p+5; i < _fileNameLength+p+5; i++)
            {
                _fileName += Convert.ToChar(buffer[i]);
            }
            byte[] _data = new byte[_fileSize];

            int k = 0;
            for (int i = p + _fileNameLength + 5; i < p + _fileSize+5; i++)
            {
                _data[k] = (byte)(buffer[i] ^ hashedPassword[k % hashedPassword.Length]);
                _data[k] = CorrectDecryptByteValue(_data[k] - 3);
                k++;
            }

            FileStream _fr = new FileStream(outputFolderPath+"/"+_fileName, FileMode.Create);
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

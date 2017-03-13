using System;
using System.IO;

public static class FileEncryptionAndDecryption
{    
    
    public static void Encrypt(string inputFile,string outputFile,string password)
    { 
        FileStream fs = new FileStream(inputFile, FileMode.Open);
        BinaryReader br = new BinaryReader(fs);

        string formattedName = inputFile.Substring(inputFile.LastIndexOf('\\') + 1);//hello.cs - size : 8
        byte[] buffer = new byte[(int)fs.Length + formattedName.Length + 1];
        byte[] temp = br.ReadBytes((int)fs.Length);

        //Put in meta data name of file in first bytes of file name length and first byte is length of name
        buffer[0] = (byte)formattedName.Length;

        for (int i = 1; i <= formattedName.Length; i++)
        {
            buffer[i] = Convert.ToByte(formattedName[i - 1]);
        }
        //Buffer is filled with meta data now .. now add the data

        for (int i = formattedName.Length + 1; i < buffer.Length; i++)
        {
            int offset = i - (formattedName.Length + 1);
            buffer[i] = CorrectEncryptByteValue(temp[offset] + 3);
            buffer[i] = (byte)(buffer[i] ^ password[offset % password.Length]);            
        }
        FileStream fr = new FileStream(outputFile, FileMode.Create);
        BinaryWriter wr = new BinaryWriter(fr);

        wr.Write(buffer);

        wr.Close();
        fr.Close();

        br.Close();
        fs.Close();
    }
    public static void Decrypt(string inputFile,string password)
    {
        FileStream fs = new FileStream(inputFile, FileMode.Open);
        BinaryReader br = new BinaryReader(fs);

        byte[] buffer = br.ReadBytes((int)fs.Length);
        int nameSize = buffer[0];
        string fileName = "";

        for (int i = 1; i <= nameSize; i++)
        {
            fileName += Convert.ToChar(buffer[i]);
        }
        byte[] data = new byte[buffer.Length - nameSize];
        int j = 0;
        for (int i = nameSize + 1; i < buffer.Length; i++)
        {
            data[j] = (byte)(buffer[i] ^ password[j % password.Length]);
            data[j] = CorrectDecryptByteValue(data[j] - 3);
            j++;
        }

        FileStream fr = new FileStream(fileName, FileMode.Create);
        BinaryWriter wr = new BinaryWriter(fr);
        wr.Write(data);

        wr.Close();
        fr.Close();

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
}

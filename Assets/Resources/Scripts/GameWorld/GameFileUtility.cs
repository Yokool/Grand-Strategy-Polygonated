using System.IO;

public static class GameFileUtility
{

    public static byte[] ReadFile(FileInfo fileInfo)
    {
        FileStream fileStream = fileInfo.OpenRead();

        byte[] data = new byte[fileStream.Length];

        fileStream.Read(data, 0, data.Length);

        fileStream.Close();

        return data;

    }
}

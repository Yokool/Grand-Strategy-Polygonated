using System;
using System.IO;
using UnityEngine;

public static class GameTexture2DUtility
{

    public static Texture2D FileToT2D(FileInfo fileInfo)
    {
        Texture2D texture2D = new Texture2D(0, 0);

        texture2D.LoadImage(GameFileUtility.ReadFile(fileInfo));

        return texture2D;
    }

    public static void IterateOverT2D(Texture2D texture2D, Action<Texture2D, Color32, int, int> consumer)
    {
        int height = texture2D.height;
        int width = texture2D.width;

        for (int y = 0; y < height; ++y)
        {
            for(int x = 0; x < width; ++x)
            {
                consumer(texture2D, texture2D.GetPixel(x, y), x, y);
            }
        }

    }

}
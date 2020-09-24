using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
[RequireComponent(typeof(Grid))]
public class GameWorld : MonoBehaviour
{

    private GameWorld instance;
    public GameWorld INSTANCE => instance;


    private Tilemap terrainTilemap;
    private TilemapRenderer terrainTilemapRenderer;

    private Tile[,] tilesByLoc;
    
    public Tile GetTileByLoc(int x, int y)
    {
        return tilesByLoc[x, y];
    }


    private void Awake()
    {

        

        if (instance != null)
        {
            Debug.LogWarning("Tried to create a second instance of BitmapPolygonConverter.");
            Destroy(gameObject);
            return;
        }

        terrainTilemap = GetComponent<Tilemap>();
        terrainTilemapRenderer = GetComponent<TilemapRenderer>();

        instance = this;


        CreateWorld(new FileInfo(@"C:\Users\ederm\Desktop\f.png"));

    }

    
    private void CreateWorld(FileInfo fileInfo)
    {
        InitializeTilemap(fileInfo);
    }

    private void InitializeTilemap(FileInfo fileInfo)
    {
        Texture2D texture2D = GameTexture2DUtility.FileToT2D(fileInfo);

        CreateTileLocArrayByDimension(texture2D.width, texture2D.height);

        CustomPerlinNoise customPerlinNoise = new CustomPerlinNoise();

        for (int y = 0; y < texture2D.height; ++y)
        {
            for (int x = 0; x < texture2D.width; ++x)
            {


                Color32 pixelColor = texture2D.GetPixel(x, y);


                if (GameTileDatabase.ContainsColor(pixelColor))
                {
                    CreateTerrainTile(x, y, pixelColor);
                }
                
                float perlinNoise = customPerlinNoise.PerlinNoise(x * 0.1f, y * 0.1f);
                perlinNoise = ((perlinNoise + 1) * 0.5f);

                float perlinNoise2 = customPerlinNoise.PerlinNoise((x + 1000) * 0.05f, (y + 1000) * 0.05f);
                perlinNoise2 = ((perlinNoise2 + 1) * 0.5f) * 0.5f;

                float perlinNoise3 = customPerlinNoise.PerlinNoise((x + 10000) * 0.025f, (y + 10000) * 0.025f);
                perlinNoise3 = ((perlinNoise3 + 1) * 0.5f) * 0.25f;

                perlinNoise = (perlinNoise + perlinNoise2 + perlinNoise3);

                TileData tileData = tilesByLoc[x, y].TileData;
                tileData.People = (long)(100000f * perlinNoise);
                tilesByLoc[x, y].TileData = tileData;
                
            }
        }

        
        for (int y = 0; y < texture2D.height; ++y)
        {
            for(int x = 0; x < texture2D.width; ++x)
            {
                terrainTilemap.SetTileFlags(new Vector3Int(x, y, 0), TileFlags.None);
                terrainTilemap.SetColor(new Vector3Int(x, y, 0), Color.Lerp(Color.black, Color.white, tilesByLoc[x, y].TileData.People / 100000f));
                //Debug.Log(terrainTilemap.GetColor(new Vector3Int(x, y, 0)));
            }
        }
        

    }

    private void CreateTerrainTile(int x, int y, Color pixelColor)
    {
        TileBlueprint tileBlueprint = GameTileDatabase.GetBlueprintByColor(pixelColor);
        TileBase tileBaseSpriteFromBlueprint = tileBlueprint.AssociatedTileSprite;

        Vector3Int tilePosition = new Vector3Int(x, y, 0);

        terrainTilemap.SetTile(tilePosition, tileBaseSpriteFromBlueprint);

        Tile tile = new Tile();
        tilesByLoc[x, y] = tile;
        tile.TileData = tileBlueprint.TileData;
    }

    private void CreateTileLocArrayByDimension(int width, int height)
    {
        tilesByLoc = new Tile[width, height];
    }


}

public static class GameTileDatabase
{

    private static List<TileBlueprint> allGameTiles;
    public static List<TileBlueprint> AllGameTiles => allGameTiles;

    public static void _Initialize()
    {

        allGameTiles = new List<TileBlueprint>();


        allGameTiles.Add(new TileBlueprint(new Color32(96, 31, 31, 1), AllGameTileBases.MudLandsTile, new TileData(0L, 10L)));
        allGameTiles.Add(new TileBlueprint(new Color32(255, 191, 0, 1), AllGameTileBases.SandTile, new TileData(0L, 500L)));
        allGameTiles.Add(new TileBlueprint(new Color32(158, 236, 219, 1), AllGameTileBases.SnowTile, new TileData(0L, 50L)));
        allGameTiles.Add(new TileBlueprint(new Color32(90, 90, 90, 1), AllGameTileBases.MountainsTile, new TileData(0L, 100L)));
        allGameTiles.Add(new TileBlueprint(new Color32(107, 206, 50, 1), AllGameTileBases.PlainsTile, new TileData(0L, 100000L)));
        allGameTiles.Add(new TileBlueprint(new Color32(178, 136, 117, 1), AllGameTileBases.HillsTile, new TileData(0L, 6000L)));
        allGameTiles.Add(new TileBlueprint(new Color32(19, 44, 105, 1), AllGameTileBases.DeepWaterTile, new TileData(0L, 0L)));
        allGameTiles.Add(new TileBlueprint(new Color32(66, 131, 191, 1), AllGameTileBases.WaterTile, new TileData(0L, 0L)));
        allGameTiles.Add(new TileBlueprint(new Color32(204, 242, 255, 1), AllGameTileBases.ShallowWaterTile, new TileData(0L, 0)));
        allGameTiles.Add(new TileBlueprint(new Color32(36, 96, 22, 1), AllGameTileBases.ForestTile, new TileData(0L, 2000L)));
        
    }

    public static bool ContainsColor(Color color)
    {

        for(int i = 0; i < AllGameTiles.Count; ++i)
        {
            TileBlueprint tileBlueprint = allGameTiles[i];
            if (ColorUtility.ColorEquals(color, tileBlueprint.ImageColor))
            {
                return true;
            }

        }

        return false;

    }

    public static TileBlueprint GetBlueprintByColor(Color32 color)
    {
        for (int i = 0; i < AllGameTiles.Count; ++i)
        {
            TileBlueprint tileBlueprint = allGameTiles[i];
            if (ColorUtility.ColorEquals(color, tileBlueprint.ImageColor))
            {
                return tileBlueprint;
            }

        }

        return null;
    }


}

public class CustomPerlinNoise
{


    private static readonly int[] permutation = new int[256]{ 151,160,137,91,90,15,
    131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
    190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
    88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
    77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
    102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
    135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
    5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
    223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
    129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
    251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
    49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
    138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
    };

    public float PerlinNoise(float x, float y)
    {

        int cubeX = (int)x;
        int cubeY = (int)y;

        float dX = x - cubeX;
        float dY = y - cubeY;


        Vector2Int locAA = new Vector2Int(cubeX, cubeY);
        Vector2Int locBA = new Vector2Int(cubeX + 1, cubeY);
        Vector2Int locAB = new Vector2Int(cubeX, cubeY + 1);
        Vector2Int locBB = new Vector2Int(cubeX + 1, cubeY + 1);

        int hashAA = permutation[permutation[((locAA.x % 256) + permutation[locAA.y % 256]) % 256]];
        int hashBA = permutation[permutation[((locBA.x % 256) + permutation[locBA.y % 256]) % 256]];
        int hashAB = permutation[permutation[((locAB.x % 256) + permutation[locAB.y % 256]) % 256]];
        int hashBB = permutation[permutation[((locBB.x % 256) + permutation[locBB.y % 256]) % 256]];

        Vector2 pseudoRandomVectorAA = GetRandomCornerVector(hashAA);
        Vector2 pseudoRandomVectorBA = GetRandomCornerVector(hashBA);
        Vector2 pseudoRandomVectorAB = GetRandomCornerVector(hashAB);
        Vector2 pseudoRandomVectorBB = GetRandomCornerVector(hashBB);

        Vector2 differenceVectorAA = new Vector2(x, y) - locAA;
        Vector2 differenceVectorBA = new Vector2(x, y) - locBA;
        Vector2 differenceVectorAB = new Vector2(x, y) - locAB;
        Vector2 differenceVectorBB = new Vector2(x, y) - locBB;
        
        float dotAA = Vector2.Dot(differenceVectorAA, pseudoRandomVectorAA);
        float dotBA = Vector2.Dot(differenceVectorBA, pseudoRandomVectorBA);
        float dotAB = Vector2.Dot(differenceVectorAB, pseudoRandomVectorAB);
        float dotBB = Vector2.Dot(differenceVectorBB, pseudoRandomVectorBB);

        float lerpBottom = Mathf.Lerp(dotAA, dotBA, Fade(dX));
        float lerpTop = Mathf.Lerp(dotAB, dotBB, Fade(dX));

        float finalLerp = Mathf.Lerp(lerpBottom, lerpTop, Fade(dY));

        return finalLerp;

    }

    private static float Fade(float t)
    {
        return 6 * t * t * t * t * t - 15 * t * t * t * t + 10 * t * t * t;
    }

    private Vector2 GetRandomCornerVector(int hash)
    {
        switch (hash % 8)
        {
            case 0:
                return new Vector2(1f, 0f);
            case 1:
                return new Vector2(-1f, 0f);
            case 2:
                return new Vector2(0f, 1f);
            case 3:
                return new Vector2(0f, -1f);
            case 4:
                return new Vector2(1f, 1.41421356237f);
            case 5:
                return new Vector2(1f, -1.41421356237f);
            case 6:
                return new Vector2(-1f, 1.41421356237f);
            case 7:
                return new Vector2(-1f, -1.41421356237f);
        }

        Debug.Log("Something went wrong.");
        return Vector2.zero;

    }

}

public static class ColorUtility
{

    public static bool ColorEquals(Color32 c1, Color32 c2)
    {
        return (c1.r == c2.r) && (c1.g == c2.g) && (c1.b == c2.b);
    }

}

public class Tile
{

    public TileData TileData
    {
        get;
        set;
    }


}

public struct TileData
{

    private long people;

    public long People
    {
        get
        {
            return people;
        }
        set
        {
            people = MathL.ClampL(value, 0L, MaxPeople);
        }
    }

    public long MaxPeople
    {
        get;
        set;
    }

    public TileData(long People, long MaxPeople)
    {
        people = 0L;
        this.MaxPeople = MaxPeople;
        this.People = People;
    }


}

public static class MathL
{

    public static long ClampL(long value, long min, long max)
    {

        if(value > max)
        {
            value = max;
        }
        else if(value < min)
        {
            value = min;
        }

        return value;

    }

}

public class TileBlueprint
{
    public Color32 ImageColor
    {
        get;
    }

    public TileBase AssociatedTileSprite
    {
        get;
    }

    public TileData TileData
    {
        get;
        set;
    }

    public TileBlueprint(Color32 ImageColor, TileBase AssociatedTileSprite, TileData TileData)
    {
        this.ImageColor = ImageColor;
        this.AssociatedTileSprite = AssociatedTileSprite;
        this.TileData = TileData;
    }

}

public enum Goods
{
    GRAIN,
    FRUIT
}

public static class AllGameTileBases
{


    public static TileBase MudLandsTile
    {
        get;
        private set;
    }

    public static TileBase PlainsTile
    {
        get;
        private set;
    }

    public static TileBase SandTile
    {
        get;
        private set;
    }

    public static TileBase SnowTile
    {
        get;
        private set;
    }

    public static TileBase DeepWaterTile
    {
        get;
        private set;
    }

    public static TileBase WaterTile
    {
        get;
        private set;
    }

    public static TileBase ShallowWaterTile
    {
        get;
        private set;
    }

    public static TileBase HillsTile
    {
        get;
        private set;
    }

    public static TileBase MountainsTile
    {
        get;
        private set;
    }

    public static TileBase ForestTile
    {
        get;
        private set;
    }

    public static void _Load()
    {

        MudLandsTile = Resources.Load<TileBase>("WorldTiles/MudTile");
        SnowTile = Resources.Load<TileBase>("WorldTiles/SnowTile");
        MountainsTile = Resources.Load<TileBase>("WorldTiles/MountainsTile");
        SandTile = Resources.Load<TileBase>("WorldTiles/SandTile");
        PlainsTile = Resources.Load<TileBase>("WorldTiles/PlainsTile");
        HillsTile = Resources.Load<TileBase>("WorldTiles/HillsTile");
        ShallowWaterTile = Resources.Load<TileBase>("WorldTiles/ShallowWaterTile");
        WaterTile = Resources.Load<TileBase>("WorldTiles/WaterTile");
        DeepWaterTile = Resources.Load<TileBase>("WorldTiles/DeepWaterTile");
        ForestTile = Resources.Load<TileBase>("WorldTiles/ForestTile");

    }

}

public static class OrderedRuntimeInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void AfterAssembliesLoaded()
    {
        AllGameTileBases._Load();
        GameTileDatabase._Initialize();
    }

}


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

public static class GameTexture2DUtility
{

    public static Texture2D FileToT2D(FileInfo fileInfo)
    {
        Texture2D texture2D = new Texture2D(0, 0);

        texture2D.LoadImage(GameFileUtility.ReadFile(fileInfo));

        return texture2D;
    }

}
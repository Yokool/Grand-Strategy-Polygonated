using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
[RequireComponent(typeof(Grid))]
public class GameWorld : MonoBehaviour
{

    private static GameWorld instance;
    public static GameWorld INSTANCE => instance;


    private Tilemap terrainTilemap;
    private TilemapRenderer terrainTilemapRenderer;

    [SerializeField]
    private int worldWidth;
    [SerializeField]
    private int worldHeight;

    public int GetWorldWidth()
    {
        return worldWidth;
    }

    public int GetRealWorldWidth()
    {
        return worldWidth - 1;
    }

    public int GetWorldHeight()
    {
        return worldHeight;
    }

    public int GetRealWorldHeight()
    {
        return worldHeight - 1;
    }

    [SerializeField]
    private int voronoiSeedCount;

    [SerializeField]
    private int shiftNormalizationCount;

    [SerializeField]
    private int seedMinDistanceFromEachOther;

    private void Awake()
    {
        instance = this;
        terrainTilemap = GetComponent<Tilemap>();
        terrainTilemapRenderer = GetComponent<TilemapRenderer>();
        CreateWorld();
    }

    private void CreateWorld()
    {
        List<VoronoiSeed> seeds = new List<VoronoiSeed>();
        CreateProvinceBackground(seeds);
        PushSeeds(seeds);
        GenerateProvincesForVoronoiSeeds(seeds);
        AssignIndividualTilesToProvinces(seeds);
        GenerateWorldContent(seeds);

    }

    private void CreateProvinceBackground(List<VoronoiSeed> seeds)
    {
        //Debug.Log("Creating seeds");
        for (int i = 0; i < voronoiSeedCount; ++i)
        {
            int xLoc = UnityEngine.Random.Range(0, worldWidth);
            int yLoc = UnityEngine.Random.Range(0, worldHeight);
            seeds.Add(new VoronoiSeed(xLoc, yLoc));
            //Debug.Log("Created a seed at location x: " + seeds[i].GetX() + " y: " + seeds[i].GetY());
        }

    }

    private void PushSeeds(List<VoronoiSeed> seeds)
    {
        //Debug.Log("Pushing seeds");

        for(int i = 0; i < shiftNormalizationCount; ++i)
        {
            for (int j = 0; j < voronoiSeedCount; ++j)
            {
                VoronoiSeed seed = seeds[j];
                seed.PushAway(seeds, seedMinDistanceFromEachOther);
            }
        }

    }

    private void GenerateProvincesForVoronoiSeeds(List<VoronoiSeed> seeds)
    {
        ProvinceDirector cachedDirector = new ProvinceDirector();

        for(int i = 0; i < voronoiSeedCount; ++i)
        {
            GenerateAProvinceForVoronoiSeed(cachedDirector, seeds[i]);
        }

        
    }

    private void GenerateAProvinceForVoronoiSeed(ProvinceDirector cachedDirector, VoronoiSeed seed)
    {
        Province generatedProvince = cachedDirector.GenerateRandomProvince();
        seed.SetProvinceToGenerateFor(generatedProvince);
    }


    private void AssignIndividualTilesToProvinces(List<VoronoiSeed> seeds)
    {

        for(int y = 0; y < worldHeight; ++y)
        {
            for(int x = 0; x < worldWidth; ++x)
            {

                int smallestDistance = int.MaxValue;

                VoronoiSeed smallestDistanceSeed = null;

                for(int i = 0; i < voronoiSeedCount; ++i)
                {
                    VoronoiSeed iteratedSeed = seeds[i];

                    int distance = Math.Max(Math.Abs(x - iteratedSeed.GetX()), Math.Abs(y - iteratedSeed.GetY()));

                    if(distance < smallestDistance)
                    {
                        smallestDistance = distance;
                        smallestDistanceSeed = iteratedSeed;
                        smallestDistanceSeed.AddTileInDistance(new Vector2Int(x, y));
                    }

                }


            }
        }

    }

    private void GenerateWorldContent(List<VoronoiSeed> seeds)
    {

        for(int i = 0; i < voronoiSeedCount; ++i)
        {
            VoronoiSeed seed = seeds[i];
            Province province = seed.GetProvinceToGenerateFor();
            TerrainType provinceTerrainType = province.GetTerrainType();
            List<Vector2Int> allTilesInDistance = seed.GetAllTilesInDistance();

            for (int j = 0; j < allTilesInDistance.Count; ++j)
            {
                Vector2Int tileInDistance = allTilesInDistance[j];
                terrainTilemap.SetTile(new Vector3Int(tileInDistance.x, tileInDistance.y, 0), GameTileSprites.GetSpriteFromTileID(provinceTerrainType));
            }

        }

    }

}

public class Province
{

    private TerrainType terrainType;
    public TerrainType GetTerrainType()
    {
        return terrainType;
    }

    public void SetTerrainType(TerrainType terrainType)
    {
        this.terrainType = terrainType;
    }

}

public class ProvinceBuilder
{

    private Province product;

    public ProvinceBuilder()
    {
        Reset();
    }

    private void Reset()
    {
        product = new Province();
    }

    public Province GetProduct()
    {
        Province result = product;
        Reset();
        return result;
    }

    public void BuildTerrainTypeRandomly()
    {
        product.SetTerrainType(EnumRandomGen.GenerateRandomEnum<TerrainType>());
    }

}

public class ProvinceDirector
{
    private ProvinceBuilder provinceBuilder;

    public ProvinceDirector()
    {
        provinceBuilder = new ProvinceBuilder();
    }

    public Province GenerateRandomProvince()
    {
        provinceBuilder.BuildTerrainTypeRandomly();
        return provinceBuilder.GetProduct();
    }

}

public class VoronoiSeed
{
    private int x;
    private int y;

    private List<Vector2Int> tilesInDistance = new List<Vector2Int>();

    public void AddTileInDistance(Vector2Int tileLoc)
    {
        tilesInDistance.Add(tileLoc);
    }

    public List<Vector2Int> GetAllTilesInDistance()
    {
        return tilesInDistance;
    }


    private Province provinceToGenerateFor;
    public Province GetProvinceToGenerateFor()
    {
        return provinceToGenerateFor;
    }

    public void SetProvinceToGenerateFor(Province provinceToGenerateFor)
    {
        this.provinceToGenerateFor = provinceToGenerateFor;
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }

    public void SetX(int x)
    {
        this.x = x;
    }

    public void SetY(int y)
    {
        this.y = y;
    }

    public int DistanceTo(VoronoiSeed other)
    {
        return Math.Max(Math.Abs(other.GetX() - GetX()), Math.Abs(other.GetY() - GetY()));
    }

    public Vector2Int GetDifference(VoronoiSeed other)
    {
        return new Vector2Int(other.GetX() - GetX(), other.GetY() - GetY());
    }

    public void PushAway(List<VoronoiSeed> allSeeds, int distanceLimit)
    {
        for (int i = 0; i < allSeeds.Count; ++i)
        {
            VoronoiSeed otherSeed = allSeeds[i];

            if (otherSeed == this)
            {
                continue;
            }

            int distance = DistanceTo(otherSeed);


            if (distance < distanceLimit)
            {
                Vector2Int difference = GetDifference(otherSeed);
                int xDiff = UnityEngine.Mathf.Clamp(difference.x, -1, 1);
                int yDiff = UnityEngine.Mathf.Clamp(difference.y, -1, 1);
                xDiff *= -1;
                yDiff *= -1;

                otherSeed.SetX(otherSeed.GetX() + xDiff);
                otherSeed.SetY(otherSeed.GetY() + yDiff);

                otherSeed.ValidateSeedPositionToWorld();

            }

        }

    }

    public void ValidateSeedPositionToWorld()
    {
        SetX(UnityEngine.Mathf.Clamp(GetX(), 0, GameWorld.INSTANCE.GetRealWorldWidth()));
        SetY(UnityEngine.Mathf.Clamp(GetY(), 0, GameWorld.INSTANCE.GetRealWorldHeight()));
    }

    public VoronoiSeed(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

}

public enum TerrainType
{
    MUD_LANDS,
    SAND,
    SNOW,
    MOUNTAINS,
    PLAINS,
    HILLS,
    DEEP_WATER,
    WATER,
    SHALLOW_WATER,
    FOREST

}

public static class EnumRandomGen
{

    public static T GenerateRandomEnum<T>() where T : struct, IConvertible
    {
        Array enumValues = Enum.GetValues(typeof(T));
        return (T)enumValues.GetValue(UnityEngine.Random.Range(0, enumValues.Length));
    }

}

public static class TerrainTypeData
{
    private static Dictionary<TerrainType, double> terrainTypeToDefaultHumidity = new Dictionary<TerrainType, double>();

    
    public static void _Init()
    {
        terrainTypeToDefaultHumidity[TerrainType.MUD_LANDS] = 0.12;
        terrainTypeToDefaultHumidity[TerrainType.SAND] = 0.08;
        terrainTypeToDefaultHumidity[TerrainType.SNOW] = 0.08;
        terrainTypeToDefaultHumidity[TerrainType.MOUNTAINS] = 0.04;
        terrainTypeToDefaultHumidity[TerrainType.PLAINS] = 0.9;
        terrainTypeToDefaultHumidity[TerrainType.HILLS] = 0.5;
        terrainTypeToDefaultHumidity[TerrainType.DEEP_WATER] = 0.0;
        terrainTypeToDefaultHumidity[TerrainType.WATER] = 0.0;
        terrainTypeToDefaultHumidity[TerrainType.SHALLOW_WATER] = 0.0;
        terrainTypeToDefaultHumidity[TerrainType.FOREST] = 0.4;

        Validate();
    }

    private static void Validate()
    {
        Array terrainTypes = Enum.GetValues(typeof(TerrainType));

        foreach(TerrainType terrainType in terrainTypes)
        {
            if (!terrainTypeToDefaultHumidity.ContainsKey(terrainType))
            {
                Debug.LogWarning($"{typeof(TerrainTypeData)}: Validation for {terrainType} failed.");
            }
        }

    }

    public static double GetTerrainTypeDefaultHumidity(TerrainType terrainType)
    {
        return terrainTypeToDefaultHumidity[terrainType];
    }

}
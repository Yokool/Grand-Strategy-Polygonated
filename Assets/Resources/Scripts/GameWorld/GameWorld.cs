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


    private List<Province> worldProvinces;

    public List<Province> GetWorldProvinces()
    {
        return worldProvinces;
    }

    public void AddProvince(Province province)
    {
        worldProvinces.Add(province);
    }

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
    private int pushCount;

    [SerializeField]
    private int seedMinDistanceFromEachOther;


    private void Awake()
    {
        instance = this;
        worldProvinces = new List<Province>();
        terrainTilemap = GetComponent<Tilemap>();
        terrainTilemapRenderer = GetComponent<TilemapRenderer>();
        CreateWorld();
    }

    public void RecalculateProvinceBorderTiles()
    {
        for (int i = 0; i < worldProvinces.Count; ++i)
        {
            Province provinceToRecalculateFor = worldProvinces[i];
            List<Vector2Int> provinceTiles = provinceToRecalculateFor.GetProvinceTiles();

            for (int j = 0; j < provinceTiles.Count; ++j)
            {

                Vector2Int checkedTile = provinceTiles[j];

                // Check tiles in a square around
                // NOTE: Less readable, more optimized.
                // NOTE: Possible code duplication, if you want move to method
                Vector2Int N = new Vector2Int(checkedTile.x, checkedTile.y + 1);


                if (!provinceTiles.Contains(N))
                {
                    provinceToRecalculateFor.AddProvinceBorderTile(checkedTile);
                    continue;
                }
                
                Vector2Int E = new Vector2Int(checkedTile.x + 1, checkedTile.y);

                if (!provinceTiles.Contains(E))
                {
                    provinceToRecalculateFor.AddProvinceBorderTile(checkedTile);
                    continue;
                }

                Vector2Int S = new Vector2Int(checkedTile.x, checkedTile.y - 1);

                if (!provinceTiles.Contains(S))
                {
                    provinceToRecalculateFor.AddProvinceBorderTile(checkedTile);
                    continue;
                }

                Vector2Int W = new Vector2Int(checkedTile.x - 1, checkedTile.y);

                if (!provinceTiles.Contains(W))
                {
                    provinceToRecalculateFor.AddProvinceBorderTile(checkedTile);
                    continue;
                }

                Vector2Int NE = new Vector2Int(checkedTile.x + 1, checkedTile.y + 1);

                if (!provinceTiles.Contains(NE))
                {
                    provinceToRecalculateFor.AddProvinceBorderTile(checkedTile);
                    continue;
                }

                Vector2Int SE = new Vector2Int(checkedTile.x + 1, checkedTile.y - 1);

                if (!provinceTiles.Contains(SE))
                {
                    provinceToRecalculateFor.AddProvinceBorderTile(checkedTile);
                    continue;
                }

                Vector2Int NW = new Vector2Int(checkedTile.x - 1, checkedTile.y + 1);

                if (!provinceTiles.Contains(NW))
                {
                    provinceToRecalculateFor.AddProvinceBorderTile(checkedTile);
                    continue;
                }

                Vector2Int SW = new Vector2Int(checkedTile.x - 1, checkedTile.y - 1);

                if (!provinceTiles.Contains(SW))
                {
                    provinceToRecalculateFor.AddProvinceBorderTile(checkedTile);
                    continue;
                }


                

            }

        }
    }

    public void RecalculateProvinceNeighbours()
    {

        for (int i = 0; i < worldProvinces.Count; ++i)
        {
            Province provinceToRecalculateFor = worldProvinces[i];

            

        }

    }

    private void CreateWorld()
    {
        long time = System.Diagnostics.Stopwatch.GetTimestamp();

        List<VoronoiSeed> seeds = new List<VoronoiSeed>();

        SowVoronoiSeeds(seeds);
        PushSeeds(seeds);
        ValidateVoronoiSeeds(seeds);
        GenerateProvincesForVoronoiSeeds(seeds);


        AssignIndividualTilesToProvinces(seeds);
        GenerateWorldTiles(seeds);

        RecalculateProvinceBorderTiles();
        
        // TEST CODE:
        /*
        for (int i = 0; i < worldProvinces.Count; ++i)
        {
            Province p = worldProvinces[i];
            
            List<Vector2Int> borderTiles = p.GetProvinceBorderTiles();
            
            Color32 c = new Color32((byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), 255);
            List<Vector2Int> provinceTiles = p.GetProvinceTiles();
            for (int j = 0; j < provinceTiles.Count; ++j)
            {
                Vector2Int tile = provinceTiles[j];
                terrainTilemap.SetTileFlags(new Vector3Int(tile.x, tile.y, 0), TileFlags.None);
                terrainTilemap.SetColor(new Vector3Int(tile.x, tile.y, 0), c);
                
            }
            
            terrainTilemap.SetTileFlags(new Vector3Int(p.GetCenterTileLoc().x, p.GetCenterTileLoc().y, 0), TileFlags.None);
            terrainTilemap.SetColor(new Vector3Int(p.GetCenterTileLoc().x, p.GetCenterTileLoc().y, 0), new Color(0, 0, 0, 1f));
            

            for (int j = 0; j < borderTiles.Count; ++j)
            {
                Vector2Int borderTile = borderTiles[j];
                terrainTilemap.SetTileFlags(new Vector3Int(borderTile.x, borderTile.y, 0), TileFlags.None);
                terrainTilemap.SetColor(new Vector3Int(borderTile.x, borderTile.y, 0), new Color(50f, 50f, 50f, 0.5f));
            }
            
        }
        */
        time = System.Diagnostics.Stopwatch.GetTimestamp() - time;
        Debug.Log("TIME TAKEN: " + time / 1_000_000.0);
    }

    private void ValidateVoronoiSeeds(List<VoronoiSeed> seeds)
    {
        for (int i = 0; i < seeds.Count; ++i)
        {
            VoronoiSeed seed = seeds[i];

            for (int j = 0; j < seeds.Count; ++j)
            {
                VoronoiSeed otherSeed = seeds[j];
                if (seed == otherSeed)
                {
                    continue;
                }

                if (seed.GetX() == otherSeed.GetX() && seed.GetY() == otherSeed.GetY())
                {
                    seeds.RemoveAt(j);
                }

            }
        }
    }

    private void SowVoronoiSeeds(List<VoronoiSeed> seeds)
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

        for(int i = 0; i < pushCount; ++i)
        {
            for (int j = 0; j < seeds.Count; ++j)
            {
                VoronoiSeed seed = seeds[j];
                seed.PushAway(seeds, seedMinDistanceFromEachOther);
            }
        }

    }

    private void GenerateProvincesForVoronoiSeeds(List<VoronoiSeed> seeds)
    {
        ProvinceDirector cachedDirector = new ProvinceDirector();

        for(int i = 0; i < seeds.Count; ++i)
        {
            GenerateProvinceForVoronoiSeed(cachedDirector, seeds[i]);
        }

        
    }

    private void GenerateProvinceForVoronoiSeed(ProvinceDirector cachedDirector, VoronoiSeed seed)
    {
        // CHANGE TO RANDOM
        Province generatedProvince = cachedDirector.GenerateRandomProvince();
        AddProvince(generatedProvince);
        seed.SetProvinceToGenerateFor(generatedProvince);

        // The center of the voronoi seeds also constitutes as the center of the province
        generatedProvince.SetCenterTileLoc(new Vector2Int(seed.GetX(), seed.GetY()));

        //Debug.Log("Set a center for: x: " + generatedProvince.GetCenterTileLoc().x + " y: " + generatedProvince.GetCenterTileLoc().y);

    }


    private void AssignIndividualTilesToProvinces(List<VoronoiSeed> seeds)
    {

        for(int y = 0; y < worldHeight; ++y)
        {
            for(int x = 0; x < worldWidth; ++x)
            {

                int smallestDistance = int.MaxValue;

                VoronoiSeed smallestDistanceSeed = null;

                for(int i = 0; i < seeds.Count; ++i)
                {
                    VoronoiSeed iteratedSeed = seeds[i];

                    int distance = Math.Max(Math.Abs(x - iteratedSeed.GetX()), Math.Abs(y - iteratedSeed.GetY()));

                    if(distance < smallestDistance)
                    {
                        smallestDistance = distance;
                        smallestDistanceSeed = iteratedSeed;
                    }

                }
                smallestDistanceSeed.AddTileInDistance(new Vector2Int(x, y));

            }
        }

    }

    private void GenerateWorldTiles(List<VoronoiSeed> seeds)
    {

        for(int i = 0; i < seeds.Count; ++i)
        {
            VoronoiSeed seed = seeds[i];
            
            Province linkedVoronoiProvince = seed.GetProvinceToGenerateFor();
            TerrainType provinceTerrainType = linkedVoronoiProvince.GetTerrainType();

            List<Vector2Int> allTilesInDistanceOfSeed = seed.GetAllTilesInDistance();
            
            linkedVoronoiProvince.AddAllProvinceTiles(allTilesInDistanceOfSeed);

            for (int j = 0; j < allTilesInDistanceOfSeed.Count; ++j)
            {
                Vector2Int tileInDistance = allTilesInDistanceOfSeed[j];
                terrainTilemap.SetTile(new Vector3Int(tileInDistance.x, tileInDistance.y, 0), GameTileSprites.GetSpriteFromTileID(provinceTerrainType));

            }

        }

    }

}

public class Province
{

    private TerrainType terrainType;

    private Vector2Int centerTileLoc;

    private List<Vector2Int> provinceTiles = new List<Vector2Int>();

    private List<Province> provinceNeighbours = new List<Province>();
    private List<Vector2Int> provinceBorderTiles = new List<Vector2Int>();

    public void AddAllProvinceTiles(List<Vector2Int> provinceTiles)
    {
        this.provinceTiles.AddRange(provinceTiles);
    }

    public void AddProvinceTile(Vector2Int provinceTile)
    {
        this.provinceTiles.Add(provinceTile);
    }

    public List<Vector2Int> GetProvinceTiles()
    {
        return this.provinceTiles;
    }

    public void ClearProvinceNeighbours()
    {
        this.provinceNeighbours.Clear();
    }

    public void ClearBorderTiles()
    {
        this.provinceBorderTiles.Clear();
    }

    public void AddProvinceNeighbour(Province province)
    {
        this.provinceNeighbours.Add(province);
    }

    public void RemoveProvinceNeighbour(Province province)
    {
        this.provinceNeighbours.Remove(province);
    }

    public List<Province> GetProvinceNeighbours()
    {
        return provinceNeighbours;
    }

    public void AddProvinceBorderTile(Vector2Int tile)
    {
        provinceBorderTiles.Add(tile);
    }

    public void RemoveProvinceBorderTile(Vector2Int tile)
    {
        provinceBorderTiles.Remove(tile);
    }

    public List<Vector2Int> GetProvinceBorderTiles()
    {
        return provinceBorderTiles;
    }


    public void SetCenterTileLoc(Vector2Int centerTileLoc)
    {
        this.centerTileLoc = centerTileLoc;
    }

    public Vector2Int GetCenterTileLoc()
    {
        return centerTileLoc;
    }

    public TerrainType GetTerrainType()
    {
        return terrainType;
    }

    public void SetTerrainType(TerrainType terrainType)
    {
        this.terrainType = terrainType;
    }

    public List<Province> GetNeighbours()
    {
        return provinceNeighbours;
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

    public void BuildTerrainTypeDefault()
    {
        product.SetTerrainType(TerrainType.PLAINS);
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

    public Province GenerateDefaultProvince()
    {
        provinceBuilder.BuildTerrainTypeDefault();
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
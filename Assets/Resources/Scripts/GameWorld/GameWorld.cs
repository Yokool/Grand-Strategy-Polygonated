using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
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

    public void RecalculateAllProvinceNeighbours()
    {

        for (int i = 0; i < worldProvinces.Count; ++i)
        {
            Province provinceToRecalculateFor = worldProvinces[i];

            List<Vector2Int> borderTiles = provinceToRecalculateFor.GetProvinceBorderTiles();

            for (int j = 0; j < borderTiles.Count; ++j)
            {
                Vector2Int borderTile = borderTiles[j];

                Vector2Int[] squareAround = TileUtilities.GetTilesInASquare(borderTile);

                for (int k = 0; k < squareAround.Length; ++k)
                {
                    Vector2Int squareAroundTile = squareAround[k];
                    Province tileOwner = GetTileOwner(squareAroundTile);

                    if (tileOwner == null)
                    {
                        continue;
                    }

                    // Check for duplicates and ignore tiles that are owned
                    // by the checking province.
                    // TODO: MAYBE CHANGE THE CHECK TO THE ADDPROVINCENEIGHBOUR METHOD
                    if (tileOwner == provinceToRecalculateFor || provinceToRecalculateFor.GetProvinceNeighbours().Contains(tileOwner))
                    {
                        continue;
                    }

                    provinceToRecalculateFor.AddProvinceNeighbour(tileOwner);
                }



            }


        }

    }

    public Province GetTileOwner(Vector2Int tile)
    {
        List<Province> worldProvinces = GetWorldProvinces();
        for (int i = 0; i < worldProvinces.Count; ++i)
        {
            Province province = worldProvinces[i];

            if (province.GetProvinceTiles().Contains(tile))
            {
                return province;
            }

        }

        return null;

    }

    private List<Province> generateContentRequests = new List<Province>();

    public void AddGenerateContentRequest(Province province)
    {
        generateContentRequests.Add(province);
    }

    private void CreateWorld()
    {
        long time = System.Diagnostics.Stopwatch.GetTimestamp();

        List<VoronoiSeed> seeds = new List<VoronoiSeed>();

        SowVoronoiSeeds(seeds);
        PushSeeds(seeds);
        ValidateVoronoiSeeds(seeds);
        GenerateProvincesFromVoronoiSeeds(seeds);


        BuildProvincesRecursively(seeds);




        time = System.Diagnostics.Stopwatch.GetTimestamp() - time;
        Debug.Log("Time it took to build provinces: " + (time / 10_000_000.0));


        

        /*
        time = System.Diagnostics.Stopwatch.GetTimestamp() - time;
        Debug.Log("VORONOI GEN: " + time / 10000000.0);
        time = System.Diagnostics.Stopwatch.GetTimestamp();

        AssignTilesToProvincesInDistance(seeds);
        GenerateWorldTileContent(seeds);

        time = System.Diagnostics.Stopwatch.GetTimestamp() - time;
        Debug.Log("TILE ASSIGNMENT: " + time / 10000000.0);
        time = System.Diagnostics.Stopwatch.GetTimestamp();

        RecalculateProvinceBorderTiles();
        RecalculateAllProvinceNeighbours();

        time = System.Diagnostics.Stopwatch.GetTimestamp() - time;
        Debug.Log("TIME TAKEN PROVINCE CALC: " + time / 10000000.0);
        time = System.Diagnostics.Stopwatch.GetTimestamp();
        */
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

        /*

        Province p = worldProvinces[2];
        Debug.Log(p.GetTerrainType());
        List<Province> neighbours = p.GetProvinceNeighbours();

        for(int i = 0; i < neighbours.Count; ++i)
        {
            List<Vector2Int> tiles = neighbours[i].GetProvinceTiles();
            for(int j = 0; j < tiles.Count; ++j)
            {
                
                Vector2Int tile = tiles[j];
                terrainTilemap.SetTileFlags(new Vector3Int(tile.x, tile.y, 0), TileFlags.None);
                terrainTilemap.SetColor(new Vector3Int(tile.x, tile.y, 0), new Color32(0, 0, 0, 255));


            }
        }


        terrainTilemap.SetTileFlags(new Vector3Int(p.GetCenterTileLoc().x, p.GetCenterTileLoc().y, 0), TileFlags.None);
        terrainTilemap.SetColor(new Vector3Int(p.GetCenterTileLoc().x, p.GetCenterTileLoc().y, 0), new Color32(255, 255, 0, 255)); terrainTilemap.SetColor(new Vector3Int(p.GetCenterTileLoc().x, p.GetCenterTileLoc().y, 0), new Color32(255, 255, 0, 255));
        terrainTilemap.SetColor(new Vector3Int(p.GetCenterTileLoc().x, p.GetCenterTileLoc().y, 0), new Color32(255, 255, 0, 255));
        terrainTilemap.SetColor(new Vector3Int(p.GetCenterTileLoc().x, p.GetCenterTileLoc().y, 0), new Color32(255, 255, 0, 255));
        */


    }


    private void Update()
    {
        ProcessGenerateContentForProviceRequests();
    }

    private void ProcessGenerateContentForProviceRequests()
    {
        while (generateContentRequests.Count > 0)
        {
            GenerateTileContentForProvince(generateContentRequests[0]);
            generateContentRequests.RemoveAt(0);
        }
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

        for (int i = 0; i < pushCount; ++i)
        {
            for (int j = 0; j < seeds.Count; ++j)
            {
                VoronoiSeed seed = seeds[j];
                seed.PushAway(seeds, seedMinDistanceFromEachOther);
            }
        }

    }

    private void GenerateProvincesFromVoronoiSeeds(List<VoronoiSeed> seeds)
    {
        ProvinceDirector cachedDirector = new ProvinceDirector();

        for (int i = 0; i < seeds.Count; ++i)
        {
            GenerateProvinceFromVoronoiSeed(cachedDirector, seeds[i]);
        }


    }

    private void GenerateProvinceFromVoronoiSeed(ProvinceDirector cachedDirector, VoronoiSeed seed)
    {
        // TODO: CHANGE TO RANDOM OR SOMETHING
        Province generatedProvince = cachedDirector.GenerateRandomProvince();
        AddProvince(generatedProvince);
        generatedProvince.SetCenterTileLoc(new Vector2Int(seed.GetX(), seed.GetY()));
    }


    private void BuildProvincesRecursively(List<VoronoiSeed> seeds)
    {

        List<Province> worldProvinces = GetWorldProvinces();
        Dictionary<Province, ProvinceRecursionInformation> provinceRecursionInformation = new Dictionary<Province, ProvinceRecursionInformation>();

        for (int i = 0; i < worldProvinces.Count; ++i)
        {
            provinceRecursionInformation.Add(worldProvinces[i], new ProvinceRecursionInformation());
        }

        int randomIndex = UnityEngine.Random.Range(0, worldProvinces.Count);

        //Thread startingThread = new Thread(new ParameterizedThreadStart(BuildProvinceRecursively_ThreadWrapperFunction));
        
        BuildProvinceWrapperArgument cachedWrapper = new BuildProvinceWrapperArgument(worldProvinces[randomIndex], provinceRecursionInformation);
        //startingThread.Start(cachedWrapper);

        BackgroundWorker startingThread = new BackgroundWorker();
        startingThread.DoWork += new DoWorkEventHandler(BuildProvinceRecursively_ThreadWrapperFunction);
        startingThread.RunWorkerAsync(cachedWrapper);
        
        //BuildProvinceRecursively(worldProvinces[randomIndex], provinceRecursionInformation);

    }


    private void BuildProvinceRecursively_ThreadWrapperFunction(object sender, DoWorkEventArgs e)
    {
        BuildProvinceWrapperArgument wrapper = e.Argument as BuildProvinceWrapperArgument;
        BuildProvinceRecursively(wrapper.Province, wrapper.Information);
    }
    /*
    private void BuildProvinceRecursively_ThreadWrapperFunction(object o)
    {
        BuildProvinceWrapperArgument wrapper = o as BuildProvinceWrapperArgument;
        BuildProvinceRecursively(wrapper.Province, wrapper.Information);
    }
    */

    private void BuildProvinceRecursively(Province province, Dictionary<Province, ProvinceRecursionInformation> provinceRecursionInformation)
    {

        lock (provinceRecursionInformation)
        {
            ProvinceRecursionInformation recursionInfo = provinceRecursionInformation[province];

            if (recursionInfo.GetCreatedThread())
            {
                Debug.LogWarning("A thread tried to build for a province, who already has a thread assigned and working for it.");
                return;
            }

            recursionInfo.SetCreatedThread(true);

        }

        /* SINCE BUILD PROVINCE RECURSIVE THREAD CAN BE BUILT FOR ALREADY BUILDING PROVINCES, MAKE SURE TO HAVE IT COVERED */
        SetProvincesTilesInRange(province, provinceRecursionInformation);

        SetBorderTiles(province, provinceRecursionInformation);
    }
    /*
    private void SetBordertileToNeighbour(Province province, Dictionary<Province, ProvinceRecursionInformation> provinceRecursionInformation)
    {

        List<Province> neighboursList = province.GetNeighbours();
        Province[] neighbourArray = new Province[neighboursList.Count];
        neighboursList.CopyTo(neighbourArray);

        List<Province> neighboursQueue = new List<Province>(neighbourArray);

        while (neighboursQueue.Count > 0)
        {
            Province neighbourProvince = neighboursQueue[0];
            while (provinceRecursionInformation[neighbourProvince].GetTileInRangeAssignmentFinished())
            {

            }
        }
    }
    */

    private void SetBorderTiles(Province province, Dictionary<Province, ProvinceRecursionInformation> provinceRecursionInformation)
    {
        lock (provinceRecursionInformation)
        {
            // If we fallthrough on another thread
            if (!provinceRecursionInformation[province].GetTileInRangeAssignmentFinished())
            {
                return;
            }


            if (provinceRecursionInformation[province].GetTileBorderAssignmentOngoing() || provinceRecursionInformation[province].GetTileBorderAssignmentFinished())
            {
                return;
            }

            provinceRecursionInformation[province].SetTileBorderAssignmentOngoing(true);
        }


        lock (province)
        {

            List<Vector2Int> provinceTiles = province.GetProvinceTiles();
            lock (provinceTiles)
            {
                for (int i = 0; i < provinceTiles.Count; ++i)
                {
                    Vector2Int provinceTile = provinceTiles[i];

                    Vector2Int[] squaresAround = TileUtilities.GetTilesInACross(provinceTile);


                    for (int j = 0; j < squaresAround.Length; ++j)
                    {
                        Vector2Int squareAround = squaresAround[j];

                        if (!provinceTiles.Contains(squareAround))
                        {
                            lock (province.GetProvinceBorderTiles())
                            {
                                province.AddProvinceBorderTile(provinceTile);
                                break;
                            }

                        }


                    }

                }
            }

            lock (provinceRecursionInformation)
            {
                provinceRecursionInformation[province].SetTileBorderAssignmentFinished(true);
            }
            




        }
        

    }

    private void SetProvincesTilesInRange(Province subjectedProvince, Dictionary<Province, ProvinceRecursionInformation> provinceRecursionInformation)
    {
        lock (provinceRecursionInformation)
        {

            if (provinceRecursionInformation[subjectedProvince].GetTileInRangeAssignmentFinished() || provinceRecursionInformation[subjectedProvince].GetTileInRangeAssignmentOngoing())
            {
                return;
            }

            provinceRecursionInformation[subjectedProvince].SetTileInRangeAssignmentOngoing(true);
        }

        

        lock (subjectedProvince)
        {
            Vector2Int subjectProvinceCenterLocation;

            subjectProvinceCenterLocation = subjectedProvince.GetCenterTileLoc();

            // The "real" province tile is not actually acknowledged until this line
            subjectedProvince.AddProvinceTile(subjectProvinceCenterLocation);
        }

        
        List<Province> foreignProvinces = new List<Province>();

        for (int a = 1; a < int.MaxValue; ++a)
        {

            Vector2Int[] squareTiles = TileUtilities.GetTilesInSquareWithLength(subjectedProvince.GetCenterTileLoc(), a);

            int successfulTiles = 0;

            for (int j = 0; j < squareTiles.Length; ++j)
            {
                Vector2Int subjectedTile = squareTiles[j];

                if(subjectedTile.x > worldWidth || subjectedTile.x < 0)
                {
                    continue;
                }

                if (subjectedTile.y > worldHeight || subjectedTile.y < 0)
                {
                    continue;
                }

                Province closestProvinceToTileInSquare = GetClosestProvinceToTile(subjectedTile);

                if (closestProvinceToTileInSquare == subjectedProvince)
                {
                    subjectedProvince.AddProvinceTile(subjectedTile);
                    ++successfulTiles;
                }
                else
                {
                    
                    

                    if (foreignProvinces.Contains(closestProvinceToTileInSquare))
                    {
                        continue;
                    }

                    foreignProvinces.Add(closestProvinceToTileInSquare);
                    subjectedProvince.AddProvinceNeighbour(closestProvinceToTileInSquare);
                }

            }

            if (successfulTiles == 0)
            {
                

                for (int i = 0; i < foreignProvinces.Count; ++i)
                {

                    lock (provinceRecursionInformation)
                    {
                        if (provinceRecursionInformation[foreignProvinces[i]].GetCreatedThread())
                        {
                            continue;
                        }
                    }

                    //Thread thread = new Thread(BuildProvinceRecursively_ThreadWrapperFunction);
                    //thread.Start(new BuildProvinceWrapperArgument(foreignProvinces[i], provinceRecursionInformation));

                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += new DoWorkEventHandler(BuildProvinceRecursively_ThreadWrapperFunction);
                    backgroundWorker.RunWorkerAsync(new BuildProvinceWrapperArgument(foreignProvinces[i], provinceRecursionInformation));
                    
                }

                lock (provinceRecursionInformation)
                {
                    provinceRecursionInformation[subjectedProvince].SetTileInRangeAssignmentFinished(true);
                }
                AddGenerateContentRequest(subjectedProvince);

                return;
            }


        }

    }

    private void GenerateTileContentForProvince(Province province)
    {
        lock (province)
        {
            List<Vector2Int> tiles = province.GetProvinceTiles();

            for (int i = 0; i < tiles.Count; ++i)
            {
                Vector2Int tile = tiles[i];
                terrainTilemap.SetTile(new Vector3Int(tile.x, tile.y, 0), GameTileSprites.GetSpriteFromTileID(province.GetTerrainType()));
            }
        }
        


    }

    private Province GetClosestProvinceToTile(Vector2Int tile)
    {

        int minDistance = int.MaxValue;
        int foundIndex = -1;

        lock (GetWorldProvinces())
        
        {
            List<Province> worldProvinces = GetWorldProvinces();
            for (int i = 0; i < worldProvinces.Count; ++i)
            {
                Province worldProvince = worldProvinces[i];

                lock (worldProvince)
                {
                    Vector2Int provinceCenter = worldProvince.GetCenterTileLoc();

                    int distance = ChebyshevDistanceSystem.GetDistance(tile, provinceCenter);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        foundIndex = i;
                    }
                }

                

            }
        }

        return worldProvinces[foundIndex];

    }





}

public static class OneDimensionalCustomNoise
{

    private static readonly int[] permutation = { 151,160,137,91,90,15,
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


    public static double Noise(double x)
    {

        int integerX = (int)x;

        int generatedValueX0 = GenerateValueForSpacedPoint(integerX);
        int generatedValueX1 = GenerateValueForSpacedPoint(integerX + 1);

        double difference = x - integerX;

        return Interpolation.CosineInterpolation(generatedValueX0, generatedValueX1, difference);


    }



    private static int GenerateValueForSpacedPoint(int integerX)
    {
        return GenerateSign(integerX) * permutation[((integerX % permutation.Length) * integerX) % permutation.Length] / permutation[permutation[((integerX % permutation.Length) * integerX) % permutation.Length]];
    }

    private static int GenerateSign(int integerX)
    {
        return permutation[integerX % permutation.Length] > 127 ? 1 : -1;
    }



}

public static class Interpolation
{
    public static double CosineInterpolation(double x1, double x2, double percentage)
    {
        double copy = (1 - Math.Cos(percentage * Math.PI)) / 2.0;
        return (x1 * (1 - copy) + x2 * copy);
    }
}

public class BuildProvinceWrapperArgument
{

    public Province Province
    {
        get;
        set;
    }

    public Dictionary<Province, ProvinceRecursionInformation> Information
    {
        get;
        set;
    }

    public BuildProvinceWrapperArgument(Province Province, Dictionary<Province, ProvinceRecursionInformation> Information)
    {
        this.Province = Province;
        this.Information = Information;
    }

}

public static class ChebyshevDistanceSystem
{
    public static int GetDistance(Vector2Int a, Vector2Int b)
    {
        int dX = a.x - b.x;
        int dY = a.y - b.y;

        return Math.Max(Math.Abs(dX), Math.Abs(dY));

    }
}




public class ProvinceRecursionInformation
{

    private bool createdThread = false;
    public void SetCreatedThread(bool createdThread)
    {
        this.createdThread = createdThread;
    }

    public bool GetCreatedThread()
    {
        return this.createdThread;
    }

    private bool tileInRangeAssignmentOngoing = false;
    private bool tileBorderAssignmentOngoing = false;


    private bool tileInRangeAssignmentFinished = false;
    private bool tileBorderAssignmentFinished = false;



    public void SetTileInRangeAssignmentFinished(bool tileInRangeAssignmentFinished)
    {
        this.tileInRangeAssignmentFinished = tileInRangeAssignmentFinished;
    }

    public bool GetTileInRangeAssignmentFinished()
    {
        return this.tileInRangeAssignmentFinished;
    }

    public void SetTileBorderAssignmentFinished(bool tileBorderAssignmentFinished)
    {
        this.tileBorderAssignmentFinished = tileBorderAssignmentFinished;
    }

    public bool GetTileBorderAssignmentFinished()
    {
        return this.tileBorderAssignmentFinished;
    }



    public bool GetTileBorderAssignmentOngoing()
    {
        return this.tileBorderAssignmentOngoing;
    }

    public void SetTileBorderAssignmentOngoing(bool tileContentGenerationOngoing)
    {
        this.tileBorderAssignmentOngoing = tileContentGenerationOngoing;
    }

    public bool GetTileInRangeAssignmentOngoing()
    {
        return this.tileInRangeAssignmentOngoing;
    }

    public void SetTileInRangeAssignmentOngoing(bool tileInRangeAssignmentOngoing)
    {
        this.tileInRangeAssignmentOngoing = tileInRangeAssignmentOngoing;
    }

}

public static class TileUtilities
{

    public static Vector2Int[] GetTilesInASquare(Vector2Int loc)
    {

        return new Vector2Int[8]
        {
            new Vector2Int(loc.x, loc.y + 1), // N
            new Vector2Int(loc.x + 1, loc.y), // E
            new Vector2Int(loc.x, loc.y - 1), // S
            new Vector2Int(loc.x - 1, loc.y), // W
            new Vector2Int(loc.x + 1, loc.y + 1), // NE
            new Vector2Int(loc.x + 1, loc.y - 1), // SE
            new Vector2Int(loc.x - 1, loc.y + 1), // NW
            new Vector2Int(loc.x - 1, loc.y - 1) // SW
        };

    }

    public static Vector2Int[] GetTilesInSquareWithLength(Vector2Int loc, int a)
    {

        if(a == 0)
        {
            Debug.LogWarning("GetTilesInSquareWithLength called with the a argument set to 0.");
            return null;
        }

        int _a = (a * 2) + 1;

        Vector2Int[] square = new Vector2Int[(2 * _a) + (2 * (_a - 2))];
        int i = 0;
        int topY = loc.y + a;
        int bottomY = loc.y - a;
        int leftX = loc.x - a;
        int rightX = loc.x + a;

        for (int y = topY; y >= bottomY; --y)
        {
            for(int x = leftX; x <= rightX; ++x)
            {
                square[i] = new Vector2Int(x, y);
                ++i;

                if(!(y == topY || y == bottomY))
                {
                    square[i] = new Vector2Int(rightX, y);
                    ++i;
                    break;
                }

            }
        }

        return square;

    }

    public static Vector2Int[] GetTilesInACross(Vector2Int loc)
    {
        return new Vector2Int[4]
        {
            new Vector2Int(loc.x + 1, loc.y),
            new Vector2Int(loc.x - 1, loc.y),
            new Vector2Int(loc.x, loc.y + 1),
            new Vector2Int(loc.x, loc.y - 1)

        };
    }

}

public class Province
{

    private TerrainType terrainType;

    private Vector2Int centerTileLoc;

    private List<Vector2Int> provinceTiles = new List<Vector2Int>();

    private List<Province> provinceNeighbours = new List<Province>();
    private List<Vector2Int> provinceBorderTiles = new List<Vector2Int>();

    private Dictionary<Province, List<Vector2Int>> sharedBorders = new Dictionary<Province, List<Vector2Int>>();

    public void AddBorderTileSharedWithNeighbour(Vector2Int tile, Province neighbour)
    {

        if (!sharedBorders.ContainsKey(neighbour))
        {
            sharedBorders.Add(neighbour, new List<Vector2Int>());
        }

        sharedBorders[neighbour].Add(tile);

    }

    public void RemoveBorderTileSharedWithNeighbour(Vector2Int tile, Province neighbour)
    {
        sharedBorders[neighbour].Remove(tile);
    }

    public List<Vector2Int> GetBordersTilesSharedWithNeighbour(Province neighbour)
    {
        return sharedBorders[neighbour];
    }

    public void AddAllProvinceTiles(List<Vector2Int> provinceTiles)
    {
        for(int i = 0; i < provinceTiles.Count; ++i)
        {
            AddProvinceTile(provinceTiles[i]);
        }
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
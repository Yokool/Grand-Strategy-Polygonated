using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

            List<Vector2Int> borderTiles = provinceToRecalculateFor.GetBorderTiles();

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
                    if (tileOwner == provinceToRecalculateFor || provinceToRecalculateFor.GetNeighbours().Contains(tileOwner))
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

        BuildProvinceWrapperArgument cachedWrapper = new BuildProvinceWrapperArgument(worldProvinces[randomIndex], provinceRecursionInformation);
        //startingThread.Start(cachedWrapper);
        Thread thread = new Thread(BuildProvinceRecursively_ThreadWrapperFunction);
        thread.Start(cachedWrapper);


    }


    private void BuildProvinceRecursively_ThreadWrapperFunction(object e)
    {
        BuildProvinceWrapperArgument wrapper = e as BuildProvinceWrapperArgument;
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

        SetProvinceCorners(province, provinceRecursionInformation);

        SetProvinceNeighbour(province, provinceRecursionInformation);

        WaitForAllNeighboursToFinishTileInRangeAssignment(province, provinceRecursionInformation);
        
        lock (province)
        {
            List<Vector2Int> reMOVE = new List<Vector2Int>();
            ArraySort.IterateProvinceClockwise(province, (borderTile) => {
                reMOVE.Add(borderTile);
            });

            lock (province.GetNeighbours())
            {
                for (int i = 0; i < reMOVE.Count; ++i)
                {
                    province.RemoveTileAllConnectionsSafely(reMOVE[i]);
                }
            }
            

            AddGenerateContentRequest(province);
        }
        
        //AddGenerateContentRequest(province);

    }

    private void SetProvinceCorners(Province province, Dictionary<Province, ProvinceRecursionInformation> recursionInformation)
    {
        // TODO: ADD RECURSION INFO SHIT
        RecalculateProvinceCorners(province);
    }

    public void RecalculateProvinceCorners(Province province)
    {

        List<Vector2Int> borderTiles = province.GetBorderTiles();

        for (int i = 0; i < borderTiles.Count; ++i)
        {
            Vector2Int borderTile = borderTiles[i];
            Vector2Int[] cross = TileUtilities.GetTilesInACross(borderTile);

            int count = 0;
            for (int j = 0; j < cross.Length; ++j)
            {
                Vector2Int crossTile = cross[j];

                if (!borderTiles.Contains(crossTile))
                {
                    ++count;
                }

                if (count >= 2)
                {
                    province.AddCorner(borderTile);
                    break;
                }

            }

        }

    }
    

    public void SetProvinceNeighbour(Province province, Dictionary<Province, ProvinceRecursionInformation> provinceRecursionInformation)
    {
        // TODO: ADD RECURSION INFO SHIT
        RecalculateProvinceNeighbours(province);
    }

    public void RecalculateProvinceNeighbours(Province province)
    {
        List<Vector2Int> borderTiles = province.GetBorderTiles();

        for (int i = 0; i < borderTiles.Count; ++i)
        {
            Vector2Int borderTile = borderTiles[i];
            RecalculateProvinceNeighboursForTile(province, borderTile);
        }
    }

    public void RecalculateProvinceNeighboursForTile(Province province, Vector2Int tile)
    {
        if (!province.IsBorderTile(tile))
        {
            Debug.LogError("ASSERTION FAILED");
            return;
        }

        Vector2Int[] crossTiles = TileUtilities.GetTilesInACross(tile);

        for(int i = 0; i < crossTiles.Length; ++i)
        {
            Vector2Int crossTile = crossTiles[i];
            Province neighbour = GameWorld.INSTANCE.GetClosestProvinceToTile(crossTile);
            province.AddProvinceNeighbour(neighbour);
        }

    }

    

    /// <summary>
    /// Currently a ungeneralized method for waiting the province members to do something.
    /// </summary>
    /// <param name="province"></param>
    /// <param name="provinceRecursionInformation"></param>
    private void WaitForAllNeighboursToFinishTileInRangeAssignment(Province province, Dictionary<Province, ProvinceRecursionInformation> provinceRecursionInformation)
    {
        // TODO: ADD LOCK STATEMENTS
        List<Province> neighboursList = null;
        
        neighboursList = province.GetNeighbours();
        
        
        Province[] neighbourArray = new Province[neighboursList.Count];
        neighboursList.CopyTo(neighbourArray);

        List<Province> neighboursQueue = new List<Province>(neighbourArray);

        while (neighboursQueue.Count > 0)
        {
            Province neighbourProvince = neighboursQueue[0];

            bool finished = false;

            while (!finished)
            {
                finished = provinceRecursionInformation[neighbourProvince].GetTileInRangeAssignmentFinished() && provinceRecursionInformation[neighbourProvince].GetTileBorderAssignmentFinished();
                // TODO: CHANGE WITH A WAIT MECHANISM DO NOT FORGET TO ADD LOCK STATEMENTS
                
            }

            neighboursQueue.RemoveAt(0);

        }

        AssignBordersWithNeighbours(province, provinceRecursionInformation);

    }

    private void AssignBordersWithNeighbours(Province province, Dictionary<Province, ProvinceRecursionInformation> provinceRecursionInformation)
    {
        lock (provinceRecursionInformation)
        {
            provinceRecursionInformation[province].SetTileBorderToNeighbourAssignmentOngoing(true);
        }

        lock (province)
        {

            List<Vector2Int> borderTiles = province.GetBorderTiles();

        
            for (int i = 0; i < borderTiles.Count; ++i)
            {
                Vector2Int borderTile = borderTiles[i];
                RecalculateNeighbourSharedBordersForTile(province, borderTile);
            }

        }
        
        lock (provinceRecursionInformation)
        {
            provinceRecursionInformation[province].SetTileBorderToNeighbourAssignmentFinished(true);
            provinceRecursionInformation[province].SetTileBorderToNeighbourAssignmentOngoing(false);
        }
        
        


    }

    public void RecalculateNeighbourSharedBordersForTile(Province province, Vector2Int borderTile)
    {

        Vector2Int[] cross = TileUtilities.GetTilesInACross(borderTile);

        lock (province)
        {

            List<Province> neighbours = province.GetNeighbours();

        
            for (int j = 0; j < neighbours.Count; ++j)
            {
                Province neighbour = neighbours[j];

                for (int k = 0; k < cross.Length; ++k)
                {
                    Vector2Int crossTile = cross[k];

                    List<Vector2Int> neighbourBorderTiles = null;
                    lock (neighbourBorderTiles = neighbour.GetBorderTiles())
                    {
                        if (neighbourBorderTiles.Contains(crossTile))
                        {
                            province.AddBorderTileSharedWithNeighbour(borderTile, neighbour);
                            break;
                        }
                    }

                }



            }
            
        }

    }
    

    private void SetBorderTiles(Province province, Dictionary<Province, ProvinceRecursionInformation> provinceRecursionInformation)
    {
        lock (provinceRecursionInformation)
        {
            // If we fallthrough on another thread
            if (!provinceRecursionInformation[province].GetTileInRangeAssignmentFinished())
            {
                return;
            }


            if (provinceRecursionInformation[province].GetTileBorderAssignmentOngoing())
            {
                return;
            }

            provinceRecursionInformation[province].SetTileBorderAssignmentOngoing(true);
        }


        lock (province)
        {

            RecalculateBorderTiles(province);

            lock (provinceRecursionInformation)
            {
                provinceRecursionInformation[province].SetTileBorderAssignmentFinished(true);
            }
            
        }
        

    }

    public void RecalculateBorderTiles(Province province)
    {
        List<Vector2Int> provinceTiles = province.GetProvinceTiles();

        for (int i = 0; i < provinceTiles.Count; ++i)
        {
            Vector2Int provinceTile = provinceTiles[i];
            RecalculateBorderTile(province, provinceTile);
            
        }
        
    }

    public void RecalculateBorderTile(Province province, Vector2Int provinceTile)
    {

        List<Vector2Int> provinceTiles = province.GetProvinceTiles();

        Vector2Int[] squaresAround = TileUtilities.GetTilesInACross(provinceTile);


        for (int j = 0; j < squaresAround.Length; ++j)
        {
            Vector2Int squareAround = squaresAround[j];

            if (!provinceTiles.Contains(squareAround))
            {
                lock (province.GetBorderTiles())
                {
                    province.AddProvinceBorderTile(provinceTile);
                    break;
                }

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
                }

            }

            if (successfulTiles == 0)
            {

                lock (provinceRecursionInformation)
                {
                    provinceRecursionInformation[subjectedProvince].SetTileInRangeAssignmentOngoing(false);
                    provinceRecursionInformation[subjectedProvince].SetTileInRangeAssignmentFinished(true);
                }

                for (int i = 0; i < foreignProvinces.Count; ++i)
                {

                    lock (provinceRecursionInformation)
                    {
                        if (provinceRecursionInformation[foreignProvinces[i]].GetCreatedThread())
                        {
                            continue;
                        }
                    }

                    Thread thread = new Thread(BuildProvinceRecursively_ThreadWrapperFunction);

                    Province nextProvince = foreignProvinces[i];
                    lock (nextProvince)
                    {
                        lock (provinceRecursionInformation)
                        {
                            provinceRecursionInformation[nextProvince].SetParent(subjectedProvince);
                        }
                    }

                    thread.Start(new BuildProvinceWrapperArgument(nextProvince, provinceRecursionInformation));

                }

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

public static class ArraySort
{

    public static void IterateProvinceClockwise(Province province, Action<Vector2Int> forEachIterator)
    {
        List<Vector2Int> borderTiles = province.GetBorderTiles();
        int lowestXY = FindSmallestXYIndex(borderTiles);


        List<Vector2Int> sortedTiles = new List<Vector2Int>();

        // First Tile is done manually
        Vector2Int firstTileLowestXY = borderTiles[lowestXY];
        sortedTiles.Add(firstTileLowestXY);
        forEachIterator(firstTileLowestXY);

        // Second tile is also done manually as we need to kick start the algorhithm
        Vector2Int? nextTile = null;

        List<Vector2Int> tilesAround = TileUtilities.GetTilesInASquare(firstTileLowestXY).ToList();
        tilesAround = ReturnFriendlyNonVisitedArray(tilesAround, borderTiles, sortedTiles);

        Direction[] startingPreferredDirections = new Direction[3]
        {
            Direction.NORTH, Direction.NORTH_EAST, Direction.EAST
        };


        nextTile = TryGetNextPreferredTile(firstTileLowestXY, tilesAround, startingPreferredDirections);
        

        if (!nextTile.HasValue)
        {
            Debug.LogWarning("This testing warning is a possible error. We can expect it in the scenario where there is only a single tile in a province.");
            return;
        }

        sortedTiles.Add(nextTile.Value);
        forEachIterator(nextTile.Value);


        IterateOver(province, sortedTiles, forEachIterator, startingPreferredDirections);
    }

    private static Vector2Int? TryGetNextPreferredTile(Vector2Int tileFrom, List<Vector2Int> filteredTilesAround, params Direction[] preferredDirections)
    {
        Vector2Int? nextTile = null;

        for (int i = 0; i < preferredDirections.Length; ++i)
        {
            Direction preferredDir = preferredDirections[i];

            Vector2Int tryNextTile = DirectionUtilities.DirectionToRelativePointFromPosition(tileFrom, preferredDir);

            if (filteredTilesAround.Contains(tryNextTile))
            {
                nextTile = tryNextTile;
                break;
            }

        }

        return nextTile;

    }

    public static void IterateOver(Province province, List<Vector2Int> sortedTiles, Action<Vector2Int> forEachIterator, params Direction[] preferredDirections)
    {
        // Take the last element pushed to sortedTiles
        Vector2Int currentTile = sortedTiles[sortedTiles.Count - 1];

        // Consume the current tile
        forEachIterator(currentTile);

        List<Vector2Int> borderTiles = province.GetBorderTiles();

        List<Vector2Int> tilesAroundFull = TileUtilities.GetTilesInASquare(currentTile).ToList();
        //List<Vector2Int> tilesAroundNonDiagonal = TileUtilities.GetTilesInACross(currentTile).ToList();

        // Filter it by friendly and non consumed tiles
        //tilesAroundNonDiagonal = ReturnFriendlyNonVisitedArray(tilesAroundNonDiagonal, borderTiles, sortedTiles);
        tilesAroundFull = ReturnFriendlyNonVisitedArray(tilesAroundFull, borderTiles, sortedTiles);
        
        // If there are no non visited tiles to us all around
        if(tilesAroundFull.Count == 0)
        {
            return;
        }

        // If there is only one tile in a cross, there is only one choice, that is to go to that tile
        if(tilesAroundFull.Count == 1)
        {
            SingleTileScenario(province, currentTile, sortedTiles, tilesAroundFull, forEachIterator);
            return;
        }
        
        
        if(tilesAroundFull.Count == 2)
        {
            TwoTilesScenario(province, sortedTiles, forEachIterator, currentTile, tilesAroundFull, borderTiles, preferredDirections);
            return;
        }

        //ThreeOrMoreScenario(province, sortedTiles, forEachIterator, currentTile, tilesAroundFull, borderTiles);
        Debug.Log("Lol");

    }

    private static void ThreeOrMoreScenario(Province province, List<Vector2Int> sortedTiles, Action<Vector2Int> forEachIterator, Vector2Int currentTile, List<Vector2Int> tilesAroundFull, List<Vector2Int> borderTiles)
    {
        AxisDictionary xAxisToTiles = AxisDictionaryFactory.GenerateStandardRelativeAxisDictionary();
        AxisDictionary yAxisToTiles = AxisDictionaryFactory.GenerateStandardRelativeAxisDictionary();

        PopulateAxisDictionaries(xAxisToTiles, yAxisToTiles, currentTile, tilesAroundFull);


        // A larger axis dictionary is the one that more well built, yes except what are you going to do if:
        // x00
        // xx0
        // xxx

        // TODO: FOR THE TWO TILE SCENARIO, PICK A CROSS TILE IF WE CAN'T FIND THE NEXT PREFERRED TILE

        // Both x and y have largest axis count as 2!!!!!!!!!
        // also cover this
        // BUT WHEN YOU START
        // XX
        // 0X
        //

        AxisDictionary largerAxis = PickLargerAxis(xAxisToTiles, yAxisToTiles);


    }

    private static AxisDictionary PickLargerAxis(AxisDictionary xAxisToTiles, AxisDictionary yAxisToTiles)
    {

        if (xAxisToTiles.IsLarger(yAxisToTiles))
        {
            return xAxisToTiles;
        }
        else
        {
            return yAxisToTiles;
        }


    }

    private static void PopulateAxisDictionaries(AxisDictionary xAxisToTiles, AxisDictionary yAxisToTiles, Vector2Int currentTile, List<Vector2Int> tilesAroundFull)
    {
        for (int i = 0; i < tilesAroundFull.Count; ++i)
        {
            Vector2Int tile = tilesAroundFull[i];

            int dX = tile.x - currentTile.x;
            int dY = tile.y - currentTile.y;

            xAxisToTiles[dX].Add(tile);
            yAxisToTiles[dY].Add(tile);

        }
    }




    private static void TwoTilesScenario(Province province, List<Vector2Int> sortedTiles, Action<Vector2Int> forEachIterator, Vector2Int currentTile, List<Vector2Int> tilesAroundFull,
        List<Vector2Int> borderTiles, params Direction[] preferredDirections)
    {

        int amountOfLoneNeighbours = 0;
        Vector2Int? lastFoundObject = null;

        for(int i = 0; i < tilesAroundFull.Count; ++i)
        {
            Vector2Int tileAround = tilesAroundFull[i];

            List<Vector2Int> tilesAroundTheTileAround = TileUtilities.GetTilesInASquare(tileAround).ToList();
            tilesAroundTheTileAround = ReturnFriendlyNonVisitedArray(tilesAroundTheTileAround, borderTiles, sortedTiles).Where( (tile) => {
                return !tilesAroundFull.Contains(tile);
            } ).ToList();

            if (tilesAroundTheTileAround.Count == 0)
            {
                ++amountOfLoneNeighbours;
                lastFoundObject = tileAround;
            }

        }

        if (amountOfLoneNeighbours == 1)
        {
            IterateOverRecursionCall(province, sortedTiles, forEachIterator, currentTile, lastFoundObject.Value);
            return;
        }

        Vector2Int? nextPreferredPosition = null;

        List<Vector2Int> tilesAroundCurrentTile = TileUtilities.GetTilesInASquare(currentTile).ToList();
        tilesAroundCurrentTile = ReturnFriendlyNonVisitedArray(tilesAroundCurrentTile, borderTiles, sortedTiles);

        nextPreferredPosition = TryGetNextPreferredTile(currentTile, tilesAroundCurrentTile, preferredDirections);

        if (!nextPreferredPosition.HasValue)
        {
            Debug.LogError("This really shouldn't happen. If it does, try to check scenarios where there are two tiles next to each other, which are both not surrounded" +
                " and we can't go to the next preferred tile.");
            return;
        }

        IterateOverRecursionCall(province, sortedTiles, forEachIterator, currentTile, nextPreferredPosition.Value);

    }

    public static List<Vector2Int> ReturnFriendlyNonVisitedArray(List<Vector2Int> arrayToFilter, List<Vector2Int> borderTiles, List<Vector2Int> sortedTiles)
    {
        return arrayToFilter.Where((tile) => { return borderTiles.Contains(tile) && !sortedTiles.Contains(tile); }).ToList();
    }

    private static void SingleTileScenario(Province province, Vector2Int currentTile, List<Vector2Int> sortedTiles, List<Vector2Int> tilesAround, Action<Vector2Int> forEachIterator)
    {
        Vector2Int onlyTile = tilesAround[0];
        IterateOverRecursionCall(province, sortedTiles, forEachIterator, currentTile, onlyTile);
    }

    private static void IterateOverRecursionCall(Province province, List<Vector2Int> sortedTiles, Action<Vector2Int> forEachIterator, Vector2Int currentTile, Vector2Int nextTile)
    {
        Direction? directionDifference = DirectionUtilities.TileDifferenceToDirection(currentTile, nextTile);

        if (!directionDifference.HasValue)
        {
            Debug.Log($"{nameof(IterateOverRecursionCall)} assertion error.");
            return;
        }

        sortedTiles.Add(nextTile);

        IterateOver(province, sortedTiles, forEachIterator, directionDifference.Value);
    }


    public static int FindSmallestXYIndex(List<Vector2Int> positions)
    {
        int smallY = int.MaxValue;
        int smallX = int.MaxValue;
        int index = -1;

        for(int i = 0; i < positions.Count; ++i)
        {
            Vector2Int position = positions[i];

            if(position.x < smallX || (position.x == smallX && position.y < smallY))
            {
                smallY = position.y;
                smallX = position.x;

                index = i;

            }

        }

        return index;


    }



}
/// <summary>
/// THIS CLASS IS UNGENERIC, IF IT'D BE REQUIRED, REFACTOR THIS CLASS INTO A GENERIC VERSION
/// </summary>
public class AxisDictionary : Dictionary<int, List<Vector2Int>>
{

    public bool IsLarger(AxisDictionary comparationAgainst)
    {
        int thisElementCount = this.GetLargestElementCount();
        int otherElementCount = comparationAgainst.GetLargestElementCount();

        return thisElementCount > otherElementCount;

    }

    private int GetLargestElementCount()
    {
        int largestElementCountInThis = 0;

        int _LCount = this[-1].Count;
        int _MCount = this[0].Count;
        int _RCount = this[1].Count;

        if (_LCount > largestElementCountInThis)
        {
            largestElementCountInThis = _LCount;
        }

        if (_MCount > largestElementCountInThis)
        {
            largestElementCountInThis = _MCount;
        }

        if (_RCount > largestElementCountInThis)
        {
            largestElementCountInThis = _RCount;
        }

        return largestElementCountInThis;

    }

    public int GetAllElementsCount()
    {
        return this[-1].Count + this[0].Count + this[1].Count;
    }


}

public static class AxisDictionaryFactory
{

    public static AxisDictionary GenerateStandardRelativeAxisDictionary()
    {
        return new AxisDictionary()
        {
            {
                -1,
                new List<Vector2Int>()
            },
            {
                0,
                new List<Vector2Int>()
            },
            {
                1,
                new List<Vector2Int>()
            }

        };
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
    private bool tileBorderToNeighbourAssignmentOngoing = false;

    private bool tileInRangeAssignmentFinished = false;
    private bool tileBorderAssignmentFinished = false;
    private bool tileBorderToNeighbourAssignmentFinished = false;

    private Province parent;
    public Province GetParent()
    {
        return this.parent;
    }

    public void SetParent(Province parent)
    {
        this.parent = parent;
    }

    public bool GetTileBorderToNeighbourAssignmentOngoing()
    {
        return this.tileBorderToNeighbourAssignmentOngoing;
    }

    public void SetTileBorderToNeighbourAssignmentOngoing(bool tileBorderToNeighbourAssignmentOngoing)
    {
        this.tileBorderToNeighbourAssignmentOngoing = tileBorderToNeighbourAssignmentOngoing;
    }

    public bool GetTileBorderToNeighbourAssignmentFinished()
    {
        return this.tileBorderToNeighbourAssignmentFinished;
    }

    public void SetTileBorderToNeighbourAssignmentFinished(bool tileBorderToNeighbourAssignmentFinished)
    {
        this.tileBorderToNeighbourAssignmentFinished = tileBorderToNeighbourAssignmentFinished;
    }



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

    public static Vector2Int[] GetTilesInSquareForCircularWalk(Vector2Int loc)
    {

        return new Vector2Int[8]
        {
            new Vector2Int(loc.x + 1, loc.y),
            new Vector2Int(loc.x, loc.y + 1),
            new Vector2Int(loc.x - 1, loc.y),
            new Vector2Int(loc.x, loc.y - 1),
			// diagonals
			new Vector2Int(loc.x - 1, loc.y + 1),
            new Vector2Int(loc.x + 1, loc.y + 1),
            new Vector2Int(loc.x - 1, loc.y - 1),
            new Vector2Int(loc.x + 1, loc.y - 1),

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
    /// <summary>
    /// Returns an array of length 4 populated with tiles in a cross next to the parameter.
    /// The array is obviously ordered, with:
    /// 0 - E
    /// 1 - W
    /// 2 - N
    /// 3 - S
    /// </summary>
    /// <param name="loc"></param>
    /// <returns></returns>
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

    private List<Vector2Int> allCorners = new List<Vector2Int>();
    
    public List<Vector2Int> GetCorners()
    {
        return this.allCorners;
    }

    public void AddCorner(Vector2Int corner)
    {
        this.allCorners.Add(corner);
    }

    public void RemoveCorner(Vector2Int corner)
    {
        this.allCorners.Remove(corner);
    }

    private Dictionary<Province, List<Vector2Int>> sharedBorders = new Dictionary<Province, List<Vector2Int>>();
    /// <summary>
    /// UNTESTED
    /// </summary>
    /// <param name="tileToRemove"></param>
    public void RemoveTileAllConnectionsSafely(Vector2Int tileToRemove)
    {
        if (IsBorderTile(tileToRemove))
        {
            RemoveBorderTileSafely(tileToRemove);
        }
        RemoveProvinceTile(tileToRemove);
        
    }

    

    public bool IsBorderTile(Vector2Int tileToCheck)
    {
        return GetBorderTiles().Contains(tileToCheck);
    }
    /// <summary>
    /// UNTESTED
    /// </summary>
    /// <param name="tileToRemove"></param>
    public void RemoveBorderTileSafely(Vector2Int tileToRemove)
    {

        // Since this was a border tile, remove the border tile shared entries ON THE NEIGHBOUR
        // TODO: READD

        /*
        List<Province> tileNeighbours = GetBorderTileNeighbours(tileToRemove);
        
        
        for (int i = 0; i < tileNeighbours.Count; ++i)
        {
            Province neighbour = tileNeighbours[i];
            
            neighbour.RemoveBorderTileSharedWithNeighbour(tileToRemove, this);
            
               
        }
        */

        RemoveProvinceBorderTile(tileToRemove);

        
        // Recalculate border tiles for ourselves
        Vector2Int[] crossTwo = TileUtilities.GetTilesInACross(tileToRemove);
        for (int j = 0; j < crossTwo.Length; ++j)
        {

            Vector2Int crossTile = crossTwo[j];

            if (GetProvinceTiles().Contains(crossTile))
            {
                GameWorld.INSTANCE.RecalculateBorderTile(this, crossTile);
                GameWorld.INSTANCE.RecalculateNeighbourSharedBordersForTile(this, crossTile);
            }

        }
        



    }

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

    public List<Province> GetBorderTileNeighbours(Vector2Int borderTile)
    {
        List<Province> neighbours = new List<Province>();

        List<Province> allNeighbours = GetNeighbours();

        for(int i = 0; i < allNeighbours.Count; ++i)
        {
            Province neighbour = allNeighbours[i];
            if (GetBordersTilesSharedWithNeighbour(neighbour).Contains(borderTile))
            {
                neighbours.Add(neighbour);
            }

        }

        return neighbours;

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

    public void RemoveProvinceTile(Vector2Int provinceTile)
    {
        this.provinceTiles.Remove(provinceTile);
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


    public void AddProvinceBorderTile(Vector2Int tile)
    {
        provinceBorderTiles.Add(tile);
    }

    public void RemoveProvinceBorderTile(Vector2Int tile)
    {
        provinceBorderTiles.Remove(tile);
    }

    public List<Vector2Int> GetBorderTiles()
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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class GameTileSprites
{

    private static Dictionary<TerrainType, TileBase> tileIdToSprite = new Dictionary<TerrainType, TileBase>();


    

    private static TileBase MudLandsTile
    {
        get;
        set;
    }

    private static TileBase PlainsTile
    {
        get;
        set;
    }

    private static TileBase SandTile
    {
        get;
        set;
    }

    private static TileBase SnowTile
    {
        get;
        set;
    }

    private static TileBase DeepWaterTile
    {
        get;
        set;
    }

    private static TileBase WaterTile
    {
        get;
        set;
    }

    private static TileBase ShallowWaterTile
    {
        get;
        set;
    }

    private static TileBase HillsTile
    {
        get;
        set;
    }

    private static TileBase MountainsTile
    {
        get;
        set;
    }

    private static TileBase ForestTile
    {
        get;
        set;
    }

    public static void _Load()
    {

        MudLandsTile = Resources.Load<TileBase>("WorldTiles/MudTile");
        tileIdToSprite[TerrainType.MUD_LANDS] = MudLandsTile;

        SnowTile = Resources.Load<TileBase>("WorldTiles/SnowTile");
        tileIdToSprite[TerrainType.SNOW] = SnowTile;

        MountainsTile = Resources.Load<TileBase>("WorldTiles/MountainsTile");
        tileIdToSprite[TerrainType.MOUNTAINS] = MountainsTile;

        SandTile = Resources.Load<TileBase>("WorldTiles/SandTile");
        tileIdToSprite[TerrainType.SAND] = SandTile;

        PlainsTile = Resources.Load<TileBase>("WorldTiles/PlainsTile");
        tileIdToSprite[TerrainType.PLAINS] = PlainsTile;

        HillsTile = Resources.Load<TileBase>("WorldTiles/HillsTile");
        tileIdToSprite[TerrainType.HILLS] = HillsTile;

        ShallowWaterTile = Resources.Load<TileBase>("WorldTiles/ShallowWaterTile");
        tileIdToSprite[TerrainType.SHALLOW_WATER] = ShallowWaterTile;

        WaterTile = Resources.Load<TileBase>("WorldTiles/WaterTile");
        tileIdToSprite[TerrainType.WATER] = WaterTile;

        DeepWaterTile = Resources.Load<TileBase>("WorldTiles/DeepWaterTile");
        tileIdToSprite[TerrainType.DEEP_WATER] = DeepWaterTile;

        ForestTile = Resources.Load<TileBase>("WorldTiles/ForestTile");
        tileIdToSprite[TerrainType.FOREST] = ForestTile;



    }

    public static TileBase GetSpriteFromTileID(TerrainType tileID)
    {
        return tileIdToSprite[tileID];
    }

}

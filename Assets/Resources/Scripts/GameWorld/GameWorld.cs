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

    private GameWorld instance;
    public GameWorld INSTANCE => instance;


    private Tilemap terrainTilemap;
    private TilemapRenderer terrainTilemapRenderer;

    private static string pathToProvinceMap = @"C:\Users\ederm\Desktop\eee.png";

    private void Awake()
    {
        terrainTilemap = GetComponent<Tilemap>();
        terrainTilemapRenderer = GetComponent<TilemapRenderer>();
        CreateProvinces();
    }

    private void CreateProvinces()
    {
        FileInfo provinceMap = new FileInfo(pathToProvinceMap);
        Texture2D provinceMapT2D = GameTexture2DUtility.FileToT2D(provinceMap);

        for(int y = 0; y < provinceMapT2D.height; ++y)
        {
            for (int x = 0; x < provinceMapT2D.width; ++x)
            {

                Color32 pixelColor = provinceMapT2D.GetPixel(x, y);

                // TODO: PERFORMACE
                foreach(Province province in ProvinceManager.INSTANCE.Provinces)
                {
                    Color32 provincePixelColor = province.PixelColor;

                    if(!ColorUtility.ColorEquals(pixelColor, provincePixelColor))
                    {
                        continue;
                    }

                    Vector3Int tilePosition = new Vector3Int(x, y, 0);

                    terrainTilemap.SetTile(tilePosition, GameTileSprites.GetSpriteFromTileID(province.GetTerrainType()));


                }

            }
        }

    }





}

public static class CountryDeserializer
{

    private static string countryXMLPath = Application.dataPath + @"\countries.xml";

    public static void _Init()
    {
        LoadCountries();
        ProvinceManager.INSTANCE.RefreshNReassignAllProvinceOwners();
    }

    private static void LoadCountries()
    {
        DataContractSerializer countrySerializer = new DataContractSerializer(typeof(CountryManager));

        byte[] buffer = File.ReadAllBytes(countryXMLPath);
        
        XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(buffer, XmlDictionaryReaderQuotas.Max);
        CountryManager.INSTANCE = countrySerializer.ReadObject(xmlDictionaryReader) as CountryManager;
        xmlDictionaryReader.Close();
        
    }


}

[Serializable]
[DataContract(Name = "CountryDatabase", Namespace = "", IsReference = true)]
public class CountryManager
{

    private static CountryManager instance;
    public static CountryManager INSTANCE
    {
        get
        {
            return instance;
        }
        set
        {
            instance = value;
        }
    }


    private Dictionary<CountryID, Country> idToCountry;

    [DataMember(Name = "Countries", Order = 0)]
    private List<Country> countries = new List<Country>();

    public List<Country> GetCountries()
    {
        return countries;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext streamingContext)
    {
        InitializeIDToCountryDictionary();
    }

    private void InitializeIDToCountryDictionary()
    {
        idToCountry = new Dictionary<CountryID, Country>();
        for (int i = 0; i < countries.Count; ++i)
        {
            Country country = countries[i];
            idToCountry[country.GetCountryID()] = country;
        }
    }

    public Country GetCountry(CountryID countryID)
    {
        return idToCountry[countryID];
    }





}

[Serializable]
[DataContract(Name = "Country", Namespace = "", IsReference = true)]
public class Country
{
    [DataMember(Name = "CountryName", Order = 0)]
    private string countryName;

    [DataMember(Name = "CountryID", Order = 1)]
    private CountryID countryID;

    public CountryID GetCountryID()
    {
        return countryID;
    }

    public void SetCountryID(CountryID countryID)
    {
        this.countryID = countryID;
    }

    public void SetCountryName(string countryName)
    {
        this.countryName = countryName;
    }

    public string GetCountryName()
    {
        return countryName;
    }

    private List<Province> provinces = new List<Province>();

    public List<Province> GetAllProvinces()
    {
        return provinces;
    }

    public void AddProvince(Province province)
    {
        provinces.Add(province);
    }

}

[Serializable]
[DataContract(Name = "CountryID", Namespace = "")]
public enum CountryID
{
    [EnumMember(Value = "JOHN")]
    JOHN = 0
}

[Serializable]
[DataContract(Name = "ProvinceDatabase", Namespace = "", IsReference = true)]
public class ProvinceManager
{
    [DataMember(Name = "Provinces", Order = 0)]
    public List<Province> Provinces
    {
        get;
        set;
    }
    
    public static ProvinceManager INSTANCE
    {
        get;
        set;
    }

    [OnDeserialized]
    private void OnDeserialization(StreamingContext streamingContext)
    {
        for (int i = 0; i < Provinces.Count; ++i)
        {
            Province province = Provinces[i];
            province.PixelColor = province.SerializableColor32;
        }
    }

    public void RefreshNReassignAllProvinceOwners()
    {
        for(int i = 0; i < Provinces.Count; ++i)
        {
            Provinces[i].RefreshProvinceOwner();
        }
    }

}

public static class ProvinceDeserializer
{

    private static string provinceFilePath = Application.dataPath + "\\provinces.xml";

    // Called when assemblies load
    public static void _Init()
    {
        LoadSerializedProvinces();
    }

    private static void LoadSerializedProvinces()
    {
        DataContractSerializer provinceSerializer = new DataContractSerializer(typeof(ProvinceManager));

        byte[] buffer = File.ReadAllBytes(provinceFilePath);


        XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(buffer, XmlDictionaryReaderQuotas.Max);
        ProvinceManager.INSTANCE = provinceSerializer.ReadObject(xmlDictionaryReader) as ProvinceManager;
        xmlDictionaryReader.Close();
        
    }

    


}

[Serializable]
public enum ProvinceID
{
    TESTING = 0,
    TESTING_1 = 1,
    TESTING_2 = 2,
    TESTING_3 = 3,
    TESTING_4 = 4,
    TESTING_5 = 5,
}

[Serializable]
[DataContract(Name = "Province", Namespace = "", IsReference = true)]
public class Province
{

    private string provinceName = "";
    
    [DataMember(Name = "ProvinceID", Order = 1)]
    private ProvinceID provinceID;

    [DataMember(Name = "TerrainType", Order = 2)]
    private TerrainType terrainType;


    [DataMember(Name = "ProvinceOwner", Order = 3)]
    private CountryID provinceOwnerID;

    [DataMember(Name = "SerializableColor32", Order = 4)]
    private SerializableColor32 serializableColor32;
    public SerializableColor32 SerializableColor32 => serializableColor32;

    public void RefreshProvinceOwner()
    {
        SetProvinceOwner(CountryManager.INSTANCE.GetCountry(provinceOwnerID));
    }

    public Color32 PixelColor
    {
        get;
        set;
    }

    public void SetProvinceID(ProvinceID provinceID)
    {
        this.provinceID = provinceID;
    }

    public ProvinceID GetProvinceID()
    {
        return this.provinceID;
    }

    public void SetTerrainType(TerrainType terrainType)
    {
        this.terrainType = terrainType;
    }

    public TerrainType GetTerrainType()
    {
        return this.terrainType;
    }

    public void SetProvinceName(string provinceName)
    {
        this.provinceName = provinceName;
    }

    public string GetProvinceName()
    {
        return this.provinceName;
    }

    public void SetProvinceOwner(CountryID countryID)
    {
        provinceOwnerID = countryID;
    }

    public void SetProvinceOwner(Country country)
    {
        SetProvinceOwner(country.GetCountryID());
    }
}

[Serializable]
[DataContract(Name = "SerializableColor32", Namespace = "", IsReference = true)]
public class SerializableColor32
{
    [DataMember(Name = "r", Order = 0)]
    public byte r
    {
        get;
        set;
    }

    [DataMember(Name = "g", Order = 1)]
    public byte g
    {
        get;
        set;
    }

    [DataMember(Name = "b", Order = 2)]
    public byte b
    {
        get;
        set;
    }

    [DataMember(Name = "a", Order = 3)]
    public byte a
    {
        get;
        set;
    }

    public static implicit operator Color32(SerializableColor32 color)
    {
        return new Color32(color.r, color.g, color.b, color.a);
    }

}

[Serializable]
[DataContract(Name = "TerrainType", Namespace = "")]
public enum TerrainType
{
    [EnumMember(Value = "MUD_LANDS")]
    MUD_LANDS,
    [EnumMember(Value = "SAND")]
    SAND,
    [EnumMember(Value = "SNOW")]
    SNOW,
    [EnumMember(Value = "MOUNTAINS")]
    MOUNTAINS,
    [EnumMember(Value = "PLAINS")]
    PLAINS,
    [EnumMember(Value = "HILLS")]
    HILLS,
    [EnumMember(Value = "DEEP_WATER")]
    DEEP_WATER,
    [EnumMember(Value = "WATER")]
    WATER,
    [EnumMember(Value = "SHALLOW_WATER")]
    SHALLOW_WATER,
    [EnumMember(Value = "FOREST")]
    FOREST
}
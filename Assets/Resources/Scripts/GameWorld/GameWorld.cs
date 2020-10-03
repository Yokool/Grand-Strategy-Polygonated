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
                foreach(Province province in ProvinceDatabase.INSTANCE.Provinces)
                {
                    Color32 provincePixelColor = province.PixelColor;

                    if(!ColorUtility.ColorEquals(pixelColor, provincePixelColor))
                    {
                        continue;
                    }

                    Vector3Int tilePosition = new Vector3Int(x, y, 0);

                    terrainTilemap.SetTile(tilePosition, GameTileSprites.GetSpriteFromTileID(province.TerrainType));


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
    }

    private static void LoadCountries()
    {
        DataContractSerializer countrySerializer = new DataContractSerializer(typeof(CountryDatabase));

        byte[] buffer = File.ReadAllBytes(countryXMLPath);
        
        XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(buffer, XmlDictionaryReaderQuotas.Max);
        CountryDatabase.INSTANCE = countrySerializer.ReadObject(xmlDictionaryReader) as CountryDatabase;
        xmlDictionaryReader.Close();
        Debug.Log(CountryDatabase.INSTANCE.GetCountries()[0].GetCountryName());
        
    }

}

[Serializable]
[DataContract(Name = "CountryDatabase", Namespace = "", IsReference = true)]
public class CountryDatabase
{

    private static CountryDatabase instance;
    public static CountryDatabase INSTANCE
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
    [DataMember(Name = "Countries", Order = 0)]
    private List<Country> countries = new List<Country>();
    public List<Country> GetCountries()
    {
        return countries;
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
public enum CountryID
{
    JOHN
}

[Serializable]
[DataContract(Name = "ProvinceDatabase", Namespace = "", IsReference = true)]
public class ProvinceDatabase
{
    [DataMember(Name = "Provinces", Order = 0)]
    public List<Province> Provinces
    {
        get;
        set;
    }
    
    public static ProvinceDatabase INSTANCE
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
        DataContractSerializer provinceSerializer = new DataContractSerializer(typeof(ProvinceDatabase));

        byte[] buffer = File.ReadAllBytes(provinceFilePath);


        XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(buffer, XmlDictionaryReaderQuotas.Max);
        ProvinceDatabase.INSTANCE = provinceSerializer.ReadObject(xmlDictionaryReader) as ProvinceDatabase;
        xmlDictionaryReader.Close();
        
    }

    


}

[Serializable]
public enum ProvinceID
{
    TESTING = 0
}

[Serializable]
[DataContract(Name = "Province", Namespace = "", IsReference = true)]
public class Province
{
    [DataMember(Name = "Name", Order = 0)]
    public string Name
    {
        get;
        set;
    }

    [DataMember(Name = "ProvinceID", Order = 1)]
    public ProvinceID ProvinceID
    {
        get;
        set;
    }

    [DataMember(Name = "TerrainType", Order = 2)]
    public TerrainType TerrainType
    {
        get;
        set;
    }

    [DataMember(Name = "SerializableColor32", Order = 3)]
    private SerializableColor32 serializableColor32;
    public SerializableColor32 SerializableColor32 => serializableColor32;

    

    public Color32 PixelColor
    {
        get;
        set;
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

[Serializable]
[XmlRoot(ElementName = "TerrainType")]
public enum TerrainType
{
    [XmlEnum(Name = "MUD_LANDS")]
    MUD_LANDS,
    [XmlEnum(Name = "SAND")]
    SAND,
    [XmlEnum(Name = "SNOW")]
    SNOW,
    [XmlEnum(Name = "MOUNTAINS")]
    MOUNTAINS,
    [XmlEnum(Name = "PLAINS")]
    PLAINS,
    [XmlEnum(Name = "HILLS")]
    HILLS,
    [XmlEnum(Name = "DEEP_WATER")]
    DEEP_WATER,
    [XmlEnum(Name = "WATER")]
    WATER,
    [XmlEnum(Name = "SHALLOW_WATER")]
    SHALLOW_WATER,
    [XmlEnum(Name = "FOREST")]
    FOREST
}
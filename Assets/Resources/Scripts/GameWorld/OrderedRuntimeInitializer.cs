﻿using UnityEngine;

public static class OrderedRuntimeInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void AfterAssembliesLoaded()
    {
        GameTileSprites._Load();
        ProvinceDeserializer._Init();
        CountryDeserializer._Init();
    }

}

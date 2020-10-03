using UnityEngine;

public static class ColorUtility
{

    public static bool ColorEquals(Color32 c1, Color32 c2)
    {
        return (c1.r == c2.r) && (c1.g == c2.g) && (c1.b == c2.b);
    }

}

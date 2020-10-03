using UnityEngine;

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

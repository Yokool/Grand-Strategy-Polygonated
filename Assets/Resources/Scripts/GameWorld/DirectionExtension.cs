using UnityEngine;

public static class DirectionExtension
{

    public static Vector2Int DirectionToVector(this Direction direction)
    {

        switch (direction)
        {
            case Direction.NORTH:
                return new Vector2Int(0, 1);
            case Direction.NORTH_EAST:
                return new Vector2Int(1, 1);
            case Direction.EAST:
                return new Vector2Int(1, 0);
            case Direction.SOUTH_EAST:
                return new Vector2Int(1, -1);
            case Direction.SOUTH:
                return new Vector2Int(0, -1);
            case Direction.SOUTH_WEST:
                return new Vector2Int(-1, -1);
            case Direction.WEST:
                return new Vector2Int(-1, 0);
            case Direction.NORTH_WEST:
                return new Vector2Int(-1, 1);
            default:
                Debug.LogError(nameof(DirectionToVector) + ": assertion failed. Default code block reached.");
                return Vector2Int.zero;
        }

    }

}

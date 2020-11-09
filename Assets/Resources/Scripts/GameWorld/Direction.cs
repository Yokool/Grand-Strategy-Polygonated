using UnityEngine;

public enum Direction
{
    NORTH,
    NORTH_EAST,
    EAST,
    SOUTH_EAST,
    SOUTH,
    NORTH_WEST,
    WEST,
    SOUTH_WEST
}

public static class DirectionUtilities
{
    public static Direction? TileDifferenceToDirection(Vector2Int v1, Vector2Int v2)
    {

        // TODO: NULLABLES, BOTH TILES ON THE SAME LOCATION RETURN NULL DIRECTION
        if ((v1.x == v2.x) && (v1.y == v2.y))
        {
            Debug.LogError("Both tiles are on the same location, implementation not yet implemeneted to cover this edge case scenario.");
            return null;
        }

        int dX = v2.x - v1.x;
        int dY = v2.y - v1.y;

        dX = Mathf.Clamp(dX, -1, 1);
        dY = Mathf.Clamp(dY, -1, 1);



        Vector2Int dVec = new Vector2Int(dX, dY);

        if (dVec.Equals(new Vector2Int(0, 1)))
        {
            return Direction.NORTH;
        }
        else if (dVec.Equals(new Vector2Int(1, 0)))
        {
            return Direction.EAST;
        }
        else if (dVec.Equals(new Vector2Int(0, -1)))
        {
            return Direction.SOUTH;
        }
        else if (dVec.Equals(new Vector2Int(-1, 0)))
        {
            return Direction.WEST;
        }
        else if (dVec.Equals(new Vector2Int(1, 1)))
        {
            return Direction.NORTH_EAST;
        }
        else if (dVec.Equals(new Vector2Int(1, -1)))
        {
            return Direction.SOUTH_EAST;
        }
        else if (dVec.Equals(new Vector2Int(-1, -1)))
        {
            return Direction.SOUTH_WEST;
        }
        else if (dVec.Equals(new Vector2Int(-1, 1)))
        {
            return Direction.NORTH_WEST;
        }

        Debug.LogError("DIRECTION ASSERTION FAILED");
        return null;


    }

    public static Vector2Int DirectionToRelativePointFromPosition(Vector2Int position, Direction direction)
    {
        Vector2Int relativePoint = DirectionToRelativePoint(direction);

        return new Vector2Int(position.x + relativePoint.x, position.y + relativePoint.y);

    }

    public static Vector2Int DirectionToRelativePoint(Direction direction)
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
            case Direction.NORTH_WEST:
                return new Vector2Int(-1, 1);
            case Direction.WEST:
                return new Vector2Int(-1, 0);
            case Direction.SOUTH_WEST:
                return new Vector2Int(-1, -1);
            default:
                Debug.LogError("ASSERTION ERROR");
                return new Vector2Int(0, 0);
        }

    }


}
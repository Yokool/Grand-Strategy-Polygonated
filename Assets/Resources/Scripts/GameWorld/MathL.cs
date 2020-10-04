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

using System;

public class Util {
    public static float nfmod(float a,float b)
    {
        return a - b * (float) Math.Floor(a / b);
    }

    public static Random random = new Random();
}
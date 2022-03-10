using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CModAPI
{
    public static int Remainder(int first, int second)
    {
        return first % second;
    }
    public static float Remainder(float first, float second)
    {
        return first % second;
    }

    public static Color RandomColor(bool randomAlpha)
    {
        Color randomColor = Random.ColorHSV();
        if (!randomAlpha) randomColor.a = 1;
        return randomColor;
    }

}

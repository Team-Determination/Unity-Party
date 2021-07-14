using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NoteCustomization
{
    public Color[] savedColors;

    public static Color[] defaultFnfColors
    {
        get
        {
            string[] colorCodes = {"#00FFFF", "#12FA05", "#F9393F", "#C24B99"};
            List<Color> colors = new List<Color>();
            foreach (string code in colorCodes)
            {
                ColorUtility.TryParseHtmlString(code, out var outColor);
                colors.Add(outColor);
            }

            return colors.ToArray();
        }
    }
}
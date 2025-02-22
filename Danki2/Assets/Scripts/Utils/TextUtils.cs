﻿using UnityEngine;

public static class TextUtils
{
    /// <summary>
    /// Adds colour tags that unity uses to apply colour to the string. Note that RichText will need to be active
    /// on the Text component for this to work.
    /// </summary>
    public static string ColouredText(Color colour, string text)
    {
        string colourHexCode = ColorUtility.ToHtmlStringRGB(colour);

        return $"<color=#{colourHexCode}>{text}</color>";
    }

    /// <summary>
    /// Adds bold tags that unity uses to display the string in bold. Note that RichText will need to be active
    /// on the Text component for this to work.
    /// </summary>
    public static string BoldText(string text)
    {
        return $"<b>{text}</b>";
    }
}

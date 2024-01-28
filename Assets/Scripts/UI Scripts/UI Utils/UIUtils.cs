using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUtils
{
    public const string DARK_ORANGE_COLOR = "#7A2600";
    public const string YELLOW_COLOR = "#FFE62B";
    public const string BLACK_COLOR = "#000000";
    public const string LIGHT_ORANGE_COLOR = "#F6AB33";

    public static void SetImageColorFromHex( Image img, string hexCode )
    {
        Color color;
        if(ColorUtility.TryParseHtmlString( hexCode, out color )){
            img.color = color;
        }
        else{
            Debug.LogError("Failed to set color");
        }   
    }
}

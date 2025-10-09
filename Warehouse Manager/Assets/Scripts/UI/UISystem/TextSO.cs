using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "CustomUI/TextSO", menuName = "TextSO", order = 0)]
public class TextSO : ScriptableObject
{
    public ThemeSO theme;
    public TMP_FontAsset font;
    public float size;
}


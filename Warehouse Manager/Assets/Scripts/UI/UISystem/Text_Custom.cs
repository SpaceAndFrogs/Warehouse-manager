using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Text_Custom : CustomUIComponent
{
    public TextSO textData;
    public Style style;
    private TextMeshProUGUI textMeshProUGUI;

    protected override void Setup()
    {
        textMeshProUGUI = GetComponentInChildren<TextMeshProUGUI>();
    }

    protected override void Configure()
    {
        textMeshProUGUI.color = textData.theme.GetTextColor(style);
        textMeshProUGUI.font = textData.font;
        textMeshProUGUI.fontSize = textData.size;
    }
}

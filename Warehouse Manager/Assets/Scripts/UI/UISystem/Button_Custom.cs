using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Button_Custom : CustomUIComponent
{
    public ThemeSO theme;
    public Style style;
    private Button button;
    private TextMeshProUGUI buttonText;

    protected override void Setup()
    {
        button = GetComponentInChildren<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    protected override void Configure()
    {
        ColorBlock cb = button.colors;
        cb.normalColor = theme.GetBackgroundColor(style);
        button.colors = cb;

        buttonText.color = theme.GetTextColor(style);
    }

    public Button GetButton()
    {
        return button;
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class InputFieldCustom : CustomUIComponent
{
    public ThemeSO theme;
    public Style style;
    private TMP_InputField inputField;
    
    protected override void Setup()
    {
        inputField = GetComponentInChildren<TMP_InputField>();
    }

    protected override void Configure()
    {
        ColorBlock cb = inputField.colors;
        cb.normalColor = theme.GetBackgroundColor(style);
        inputField.colors = cb;

        inputField.textComponent.color = theme.GetTextColor(style);
    }
}


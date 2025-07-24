using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Text_Custom : MonoBehaviour
{
    public TextSO textData;
    public Style style;
    private TextMeshProUGUI textMeshProUGUI;
    void Awake()
    {
        Init();
    }

    void OnValidate()
    {
        Init();
    }

    void Init()
    {
        Setup();
        Configure();
    }

    void Setup()
    {
        textMeshProUGUI = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Configure()
    {
        textMeshProUGUI.color = textData.theme.GetTextColor(style);
        textMeshProUGUI.font = textData.font;
        textMeshProUGUI.fontSize = textData.size;
    }
}

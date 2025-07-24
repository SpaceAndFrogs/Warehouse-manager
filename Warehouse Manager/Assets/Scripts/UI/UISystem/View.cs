using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class View : MonoBehaviour
{
    public ViewSO viewData;

    public GameObject containerTop;
    public GameObject containerBottom;
    public GameObject containerCenter;
    private Image imageTop;
    private Image imageBottom;
    private Image imageCenter;
    private VerticalLayoutGroup verticalLayoutGroup;

    void Awake()
    {
        Init();
    }

    public void Init()
    {
        Setup();
        Configure();
    }

    public void Setup()
    {
        imageTop = containerTop.GetComponent<Image>();
        imageBottom = containerBottom.GetComponent<Image>();
        imageCenter = containerCenter.GetComponent<Image>();
        verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
    }

    public void Configure()
    {
        verticalLayoutGroup.padding = viewData.padding;
        verticalLayoutGroup.spacing = viewData.spacing;

        imageTop.color = viewData.theme.primary_bg;
        imageBottom.color = viewData.theme.tertiary_bg;
        imageCenter.color = viewData.theme.secondary_bg;
    }
    private void OnValidate()
    {
        Init();
    }
}

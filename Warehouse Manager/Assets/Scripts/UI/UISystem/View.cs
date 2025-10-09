using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class View : CustomUIComponent
{
    public ViewSO viewData;

    public GameObject containerTop;
    public GameObject containerBottom;
    public GameObject containerCenter;
    private Image imageTop;
    private Image imageBottom;
    private Image imageCenter;
    private VerticalLayoutGroup verticalLayoutGroup;

    protected override void Setup()
    {
        imageTop = containerTop.GetComponent<Image>();
        imageBottom = containerBottom.GetComponent<Image>();
        imageCenter = containerCenter.GetComponent<Image>();
        verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
    }

    protected override void Configure()
    {
        verticalLayoutGroup.padding = viewData.padding;
        verticalLayoutGroup.spacing = viewData.spacing;

        imageTop.color = viewData.theme.primary_bg;
        imageBottom.color = viewData.theme.tertiary_bg;
        imageCenter.color = viewData.theme.secondary_bg;
    }
}

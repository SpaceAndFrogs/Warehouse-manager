using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomUIComponent : MonoBehaviour
{
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

    protected abstract void Setup();
    protected abstract void Configure();
}

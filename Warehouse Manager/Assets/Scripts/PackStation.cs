using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PackStation : MonoBehaviour
{
    public static event Action<PackStation>? OnPackStationSpawned;
    public bool havePackWorker = false;

    public Tile tileWithStation;

    void Start()
    {       
        OnPackStationSpawned?.Invoke(this);
    }
}

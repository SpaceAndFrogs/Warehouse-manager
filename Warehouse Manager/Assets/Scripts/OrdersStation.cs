using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OrdersStation : MonoBehaviour
{
    public Tile tileWithStation;

    #nullable enable
    public static event Action<OrdersStation>? OnStationSpawned;
    #nullable disable

    void Start()
    {
        OnStationSpawned?.Invoke(this);
    }
}

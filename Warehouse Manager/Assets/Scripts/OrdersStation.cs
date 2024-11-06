using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OrdersStation : MonoBehaviour
{
    public Tile tileWithStation;

    public static event Action<OrdersStation>? OnStationSpawned;

    void Start()
    {
        OnStationSpawned?.Invoke(this);
    }
}

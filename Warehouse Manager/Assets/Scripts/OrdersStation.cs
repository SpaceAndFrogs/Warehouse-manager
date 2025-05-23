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

    void OnEnable()
    {
        SavingManager.OnSave += SaveOrdersStationData;
    }
    void OnDisable()
    {
        SavingManager.OnSave -= SaveOrdersStationData;
    }

    void SaveOrdersStationData()
    {
        SavingManager.instance.saveData.buildings.Add(new SaveData.BuildingData(Buildings.BuildingType.OrdersStation.ToString(),transform.position, transform.rotation));
    }
}

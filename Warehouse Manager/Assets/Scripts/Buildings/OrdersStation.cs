using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OrdersStation : BuildingBase
{
#nullable enable
    public static event Action<OrdersStation>? OnStationSpawned;
#nullable disable
    protected override void StartFinishHook()
    {
        OnStationSpawned?.Invoke(this);
    }

    protected override void OnSave()
    {
        SavingManager.instance.saveData.buildings.Add(new SaveData.BuildingData(Buildings.BuildingType.OrdersStation.ToString(), transform.position, transform.rotation));
    }
    protected override void EnableFinishHook()
    {
    }
    protected override void DisableFinishHook()
    {
    }
}

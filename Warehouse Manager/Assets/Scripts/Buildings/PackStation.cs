using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PackStation : BuildingBase
{
#nullable enable
    public static event Action<PackStation>? OnPackStationSpawned;
#nullable disable
    public bool havePackWorker = false;

    protected override void StartFinishHook()
    {
        OnPackStationSpawned?.Invoke(this);
    }
    protected override void OnSave()
    {
        SavingManager.instance.saveData.packStations.Add(new SaveData.PackStationData(isInRoom, transform.position, transform.rotation));
    }
    protected override void EnableFinishHook()
    {
    }
    protected override void DisableFinishHook()
    {
    }
}

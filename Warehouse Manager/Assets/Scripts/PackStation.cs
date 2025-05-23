using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PackStation : MonoBehaviour
{
    #nullable enable
    public static event Action<PackStation>? OnPackStationSpawned;
    #nullable disable
    public bool havePackWorker = false;
    public bool isInRoom = false;

    public Tile tileWithStation;

    void Start()
    {   
        isInRoom = PathFinder.instance.IsBuildingSurrounded(tileWithStation);
        OnPackStationSpawned?.Invoke(this);
    }

    void OnEnable()
    {
        Worker.OnBuildingEnded += CheckIfIsInRoom;
        SavingManager.OnSave += SavePackStationData;
    }

    void OnDisable()
    {
        Worker.OnBuildingEnded -= CheckIfIsInRoom;
        SavingManager.OnSave -= SavePackStationData;
    }

    void CheckIfIsInRoom()
    {
        isInRoom = PathFinder.instance.IsBuildingSurrounded(tileWithStation);
    }

    void SavePackStationData()
    {
        SavingManager.instance.saveData.packStations.Add(new SaveData.PackStationData(havePackWorker, isInRoom, transform.position, transform.rotation));
    }
}

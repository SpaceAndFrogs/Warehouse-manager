using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PackStation : MonoBehaviour
{
    public static event Action<PackStation>? OnPackStationSpawned;
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
    }

    void OnDisable()
    {
        Worker.OnBuildingEnded -= CheckIfIsInRoom;
    }

    void CheckIfIsInRoom()
    {
        isInRoom = PathFinder.instance.IsBuildingSurrounded(tileWithStation);
    }
}

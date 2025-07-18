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
        if (tileWithStation == null)
        {
            tileWithStation = GetTile(transform.position);
        }
        
        isInRoom = PathFinder.instance.IsBuildingSurrounded(tileWithStation);
        OnPackStationSpawned?.Invoke(this);
        
    }

    Tile GetTile(Vector3 position)
    {
        Ray ray = new Ray(position+new Vector3(0f,100f,0f), Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        foreach (RaycastHit hit in hits)
        {
            Tile tile = hit.collider.gameObject.GetComponent<Tile>();
            if (tile != null)
            {
                return tile;
            }
        }
        return null;
    }

    void OnEnable()
    {
        BuildingWorker.OnBuildingEnded += CheckIfIsInRoom;
        SavingManager.OnSave += SavePackStationData;
    }

    void OnDisable()
    {
        BuildingWorker.OnBuildingEnded -= CheckIfIsInRoom;
        SavingManager.OnSave -= SavePackStationData;
    }

    void CheckIfIsInRoom()
    {
        if (tileWithStation == null)
        {
            tileWithStation = GetTile(transform.position);
        }
        isInRoom = PathFinder.instance.IsBuildingSurrounded(tileWithStation);
    }

    void SavePackStationData()
    {
        SavingManager.instance.saveData.packStations.Add(new SaveData.PackStationData(isInRoom, transform.position, transform.rotation));
    }
}

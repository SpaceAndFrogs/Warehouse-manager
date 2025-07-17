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
        if (tileWithStation== null)
        {
            tileWithStation = GetTile(transform.position);
        }
        OnStationSpawned?.Invoke(this);
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

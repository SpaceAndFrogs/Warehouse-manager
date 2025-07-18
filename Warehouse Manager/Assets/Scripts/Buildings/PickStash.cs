using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PickStash : MonoBehaviour
{

    public Tile tileWithStash;
    #nullable enable
    public static event Action<PickStash>? OnPickStashSpawned;
    #nullable disable
    public bool isInRoom = false;

    public List<OrdersManager.Order> orders = new List<OrdersManager.Order>();
    void Start()
    {
        if (tileWithStash == null)
        {
            tileWithStash = GetTile(transform.position);
        }
        isInRoom = PathFinder.instance.IsBuildingSurrounded(tileWithStash);
        OnPickStashSpawned?.Invoke(this);
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

    void AddNewOrder(Tile tile, OrdersManager.Order order)
    {
        if(tile != tileWithStash)
        {
            return;
        }

        orders.Add(order);
    }

    void OnEnable()
    {
        BuildingWorker.OnBuildingEnded += CheckIfIsInRoom;
        SavingManager.OnSave += SavePickStashData;
    }

    void OnDisable()
    {
        BuildingWorker.OnBuildingEnded -= CheckIfIsInRoom;
        SavingManager.OnSave -= SavePickStashData;
    }

    void CheckIfIsInRoom()
    {
        if (tileWithStash == null)
        {
            tileWithStash = GetTile(transform.position);
        }
        isInRoom = PathFinder.instance.IsBuildingSurrounded(tileWithStash);
    }

    void SavePickStashData()
    {
        SavingManager.instance.saveData.pickStashes.Add(new SaveData.PickStashData(isInRoom, transform.position, transform.rotation));
    }
}

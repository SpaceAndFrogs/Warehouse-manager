using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PickStash : BuildingBase
{
#nullable enable
    public static event Action<PickStash>? OnPickStashSpawned;
#nullable disable

    public List<OrdersManager.Order> orders = new List<OrdersManager.Order>();
    protected override void StartFinishHook()
    {
        OnPickStashSpawned?.Invoke(this);
    }

    void AddNewOrder(Tile tile, OrdersManager.Order order)
    {
        if (tile != tileWithBuilding)
        {
            return;
        }

        orders.Add(order);
    }

    protected override void OnSave()
    {
        SavingManager.instance.saveData.pickStashes.Add(new SaveData.PickStashData(isInRoom, transform.position, transform.rotation));
    }

    protected override void EnableFinishHook()
    {
    }
    
    protected override void DisableFinishHook()
    {
    }
}

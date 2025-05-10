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
        isInRoom = PathFinder.instance.IsBuildingSurrounded(tileWithStash);
        OnPickStashSpawned?.Invoke(this);
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
        Worker.OnBuildingEnded += CheckIfIsInRoom;
    }

    void OnDisable()
    {
        Worker.OnBuildingEnded -= CheckIfIsInRoom;
    }

    void CheckIfIsInRoom()
    {
        isInRoom = PathFinder.instance.IsBuildingSurrounded(tileWithStash);
    }
}

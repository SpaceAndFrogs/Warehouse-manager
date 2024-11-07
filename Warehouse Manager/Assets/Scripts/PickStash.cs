using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PickStash : MonoBehaviour
{

    public Tile tileWithStash;
    public static event Action<PickStash>? OnPickStashSpawned;

    public List<OrdersManager.Order> orders = new List<OrdersManager.Order>();
    void Start()
    {
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
}

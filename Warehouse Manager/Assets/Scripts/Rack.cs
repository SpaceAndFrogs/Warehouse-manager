using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Rack : MonoBehaviour
{
    public Items.Item itemOnRack;

    public int amountOfItems = 0;

    public int reservedAmountOfItems = 0;

    public Tile tileWithRack;
    public int desiredAmountOfItems = 0;
    public int maxAmountOfItems = 0;
    public static event Action<Rack>? OnRackSpawned;

    void Update()
    {
        CheckAmountOfItems();
    }

    void Start()
    {
        OnRackSpawned?.Invoke(this);
    }

    void CheckAmountOfItems()
    {
        if(amountOfItems < desiredAmountOfItems)
        {
            int amountOfItemsToBuy = desiredAmountOfItems - amountOfItems;
            amountOfItems += amountOfItemsToBuy; 
        }
    }

}

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
            PayForItems(amountOfItemsToBuy);
            amountOfItems += amountOfItemsToBuy; 
        }
    }

    void PayForItems(int amountOfItems)
    {
        float priceOfItem = 0;
        for(int i = 0; i < PricesManager.instance.itemPricesScripts.Count; i++)
        {
            if(PricesManager.instance.itemPricesScripts[i].itemType == itemOnRack.itemType)
            {
                string buyPrice = PricesManager.instance.itemPricesScripts[i].buyPrice.text;
                buyPrice = buyPrice.Substring(0, buyPrice.Length - 1);
                priceOfItem = float.Parse(buyPrice);
            }
        }

        CashManager.instance.SpendCash(priceOfItem * amountOfItems);
    }

    public void GiveItems(int amountOfItemsToGive)
    {
        amountOfItems -= amountOfItemsToGive;
        reservedAmountOfItems -= amountOfItemsToGive;
    }
}

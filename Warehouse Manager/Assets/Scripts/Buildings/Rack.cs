using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Rack : BuildingBase
{
    public Items.Item itemOnRack;

    public int amountOfItems = 0;

    public int reservedAmountOfItems = 0;
    public int desiredAmountOfItems = 0;
    public int maxAmountOfItems = 0;
#nullable enable
    public static event Action<Rack>? OnRackSpawned;
#nullable disable

    void Update()
    {
        CheckAmountOfItems();
    }
    protected override void StartFinishHook()
    {
        OnRackSpawned?.Invoke(this);
    }

    void CheckAmountOfItems()
    {
        if (amountOfItems < desiredAmountOfItems)
        {
            int amountOfItemsToBuy = desiredAmountOfItems - amountOfItems;
            PayForItems(amountOfItemsToBuy);
            amountOfItems += amountOfItemsToBuy;
        }
    }

    void PayForItems(int amountOfItems)
    {
        float priceOfItem = 0;
        for (int i = 0; i < PricesManager.instance.itemPricesScripts.Count; i++)
        {
            if (PricesManager.instance.itemPricesScripts[i].itemType == itemOnRack.itemType)
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

    protected override void OnSave()
    {
        SavingManager.instance.saveData.racks.Add(new SaveData.RackData(itemOnRack.itemType.ToString(), amountOfItems, reservedAmountOfItems, desiredAmountOfItems, maxAmountOfItems, transform.position, transform.rotation, isInRoom));
    }
    protected override void EnableFinishHook()
    {
    }
    protected override void DisableFinishHook()
    {
    }
}

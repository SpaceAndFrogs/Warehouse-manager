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
    #nullable enable
    public static event Action<Rack>? OnRackSpawned;
    #nullable disable
    public bool isInRoom = false;

    void Update()
    {
        CheckAmountOfItems();
    }

    void Start()
    {
        if(tileWithRack == null)
        {
            tileWithRack = GetTile(transform.position);
        }
        isInRoom = PathFinder.instance.IsBuildingSurrounded(tileWithRack);
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

    void OnEnable()
    {
        BuildingWorker.OnBuildingEnded += CheckIfIsInRoom;
        SavingManager.OnSave += SaveRack;
    }

    void OnDisable()
    {
        BuildingWorker.OnBuildingEnded -= CheckIfIsInRoom;
        SavingManager.OnSave -= SaveRack;
    }

    Tile GetTile(Vector3 position)
    {
        Ray ray = new Ray(position + new Vector3(0f, 100f, 0f), Vector3.down);
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

    void CheckIfIsInRoom()
    {
        if(tileWithRack == null)
        {
            tileWithRack = GetTile(transform.position);
        }
        isInRoom = PathFinder.instance.IsBuildingSurrounded(tileWithRack);
    }

    void SaveRack()
    {
        SavingManager.instance.saveData.racks.Add(new SaveData.RackData(itemOnRack.itemType.ToString(), amountOfItems, reservedAmountOfItems, desiredAmountOfItems, maxAmountOfItems, transform.position, transform.rotation, isInRoom));       
    }
}

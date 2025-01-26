using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OrdersManager : MonoBehaviour
{
    [SerializeField]
    public List<Order> ordersOnPick = new List<Order>();
    [SerializeField]
    public List<Order> ordersOnPack = new List<Order>();
    public List<Rack> racks = new List<Rack>();

    [SerializeField]
    float timeBetweenOrders;

    [SerializeField]
    int maxAmountOfOrders;
    [SerializeField]
    Items items;

    [System.Serializable]
    public class Order
    {
        public List<Rack> racksWithItems = new List<Rack>();
        public List<int> amountOfItemsFromRacks = new List<int>();
        public float orderPrice;

        public Order(List<Rack> racksWithItems, float orderPrice, List<int> amountOfItemsFromRacks)
        {
            this.racksWithItems = racksWithItems;
            this.orderPrice = orderPrice;
            this.amountOfItemsFromRacks = amountOfItemsFromRacks;
        }
    }

    void OnEnable()
    {
        Rack.OnRackSpawned += AddRack;
    }
    void OnDisable()
    {
        Rack.OnRackSpawned -= AddRack;
    }

    void AddRack(Rack rack)
    {
        racks.Add(rack);
    }

    void Start()
    {
        StartCoroutine(GenerateOrders());
    }

    IEnumerator GenerateOrders()
    {
        while(true)
        {
            if(racks.Count == 0)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            if(!IsThereItemsOnRacks())
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            int amountOfOrders = Random.Range((int)1, maxAmountOfOrders+1);
            
            for(int i = 0; i < amountOfOrders; i++)
            {
                Order order = MakeOrder();

                if(order != null)
                {
                    ordersOnPick.Add(order);
                }               
            }
            yield return new WaitForSeconds(timeBetweenOrders);
        }
        
    }

    bool IsThereItemsOnRacks()
    {
        for(int i = 0; i < racks.Count; i++)
        {
            if(racks[i].amountOfItems -  racks[i].reservedAmountOfItems > 0)
            {
                return true;
            }
        }

        return false;
    }

    Order MakeOrder()
    {
        (List<Items.Item>, List<int>) items = DrawItemsForOrder();
        if(items.Item1 == null || items.Item2 == null)
        {
            return null;
        }
        List<Items.Item> itemsInOrder = items.Item1;
        List<int> amountOfItemsFromRacks = items.Item2;
        List<Rack> sortedRacks = SortRacksByItems(itemsInOrder);
        float priceOfOrder = CheckPriceOfOrder(itemsInOrder);
        return new Order(sortedRacks, priceOfOrder, amountOfItemsFromRacks);
    }

    List<Rack> SortRacksByItems(List<Items.Item> itemsInOrder)
    {
        List<Rack> racksWithItems = new List<Rack>();

        SortRacks();

        for(int i = 0; i < racks.Count; i++)
        {
            for(int j = 0; j < itemsInOrder.Count; j++)
            {
                if(racks[i].itemOnRack.itemType == itemsInOrder[j].itemType && racks[i].amountOfItems -  racks[i].reservedAmountOfItems > 0)
                {
                    racksWithItems.Add(racks[i]);
                    racks[i].reservedAmountOfItems ++;
                }
            }
        }

        return racksWithItems;
    }

    void SortRacks()
    {
        for(int i = 0; i < racks.Count; i ++)
        {
            int currentRackPathLength = PathFinder.instance.FindPath(PickPackStationsManager.instance.ordersStation.tileWithStation,racks[i].tileWithRack).Length;
            for(int j = i+1; j < racks.Count; j++)
            {
                int lastPathLength = PathFinder.instance.FindPath(PickPackStationsManager.instance.ordersStation.tileWithStation,racks[i].tileWithRack).Length;
                if(currentRackPathLength>lastPathLength)
                {
                    Rack currentRack = racks[i];
                    racks[i] = racks[j];
                    racks[j] = currentRack;
                }
            }
        }
    }

    float CheckPriceOfOrder(List<Items.Item> itemsInOrder)
    {
        float price = 0;
        for(int i = 0; i < itemsInOrder.Count; i++)
        {
            string priceText = PricesManager.instance.itemPricesScripts[i].sellPriceInput.text;
            priceText = priceText.Remove(priceText.Length - 1);
            price += float.Parse(priceText);
        }
        return price;
    }

    (List<Items.Item>, List<int>) DrawItemsForOrder()
    {

        List<Items.Item> drawnItems = new List<Items.Item>();
        List<int> amountOfItemsFromRacks = new List<int>();

        int maxAmountOfItems = 1;
        for(int i = 0; i < racks.Count; i ++)
        {
            maxAmountOfItems += racks[i].amountOfItems - racks[i].reservedAmountOfItems;
        }

        int amountOfItems = Random.Range((int)1, maxAmountOfItems);
        List<int> usedRacksIndexs = new List<int>();
        for(int i = 0; i < amountOfItems; i++)
        {
            List<int> racksChecked = new List<int>();
            int rackIndex = -1;

            while(true)
            {
                rackIndex = Random.Range(0,racks.Count);
                if(!racksChecked.Contains(rackIndex))
                {
                    racksChecked.Add(rackIndex);
                }

                int itemIndex = -1;
                for(int j = 0; j < items.items.Count; j++)
                {
                    if(racks[rackIndex].itemOnRack.itemType == PricesManager.instance.itemPricesScripts[j].itemType)
                    {
                        itemIndex = j;
                        break;
                    }
                }

                if(itemIndex == -1)
                {
                    Debug.Log("No item found");
                    
                    return (null,null);
                }

                if(PricesManager.instance.itemPricesScripts[itemIndex].sellingLocked)
                {
                    Debug.Log("Selling locked");
                    if(racksChecked.Count == racks.Count)
                    {
                        return (null,null);
                    }
                    continue;
                }

                if(racks[rackIndex].amountOfItems-racks[rackIndex].reservedAmountOfItems > 0)
                {
                    break;
                }

                if(racksChecked.Count == racks.Count)
                {
                    return (null,null);
                }
            }

            int amountOfItemsFromRack = Random.Range(1,racks[rackIndex].amountOfItems-racks[rackIndex].reservedAmountOfItems+1);
            if(!usedRacksIndexs.Contains(rackIndex))
            {
                rackIndex = Random.Range(0,racks.Count);
                amountOfItemsFromRack = Random.Range(1,racks[rackIndex].amountOfItems-racks[rackIndex].reservedAmountOfItems+1);
            }

            usedRacksIndexs.Add(rackIndex);

            if(amountOfItemsFromRack > amountOfItems - i)
            {
                amountOfItemsFromRack = amountOfItems - i;
            }

            i += amountOfItemsFromRack-1;

            for(int j = 0 ; j < amountOfItemsFromRack; j++)
            {
                amountOfItemsFromRacks.Add(1);
            }
            

            for(int j = 0; j < amountOfItemsFromRack; j++)
            {
                drawnItems.Add(racks[rackIndex].itemOnRack);
            }
            
        }

        return (drawnItems, amountOfItemsFromRacks);
    }
}

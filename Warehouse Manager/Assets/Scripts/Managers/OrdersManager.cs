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
        public Queue<Rack> racksWithItems = new Queue<Rack>();
        public Queue<int> amountOfItemsFromRacks = new Queue<int>();
        public float orderPrice;

        public Order(Queue<Rack> racksWithItems, float orderPrice, Queue<int> amountOfItemsFromRacks)
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
            yield return new WaitForSeconds(TimeManager.instance.GetOneHour());
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
        (Queue<Items.Item>, Queue<int>) items = DrawItemsForOrder();
        if(items.Item1 == null || items.Item2 == null)
        {
            return null;
        }
        Queue<Items.Item> itemsInOrder = items.Item1;
        Queue<int> amountOfItemsFromRacks = items.Item2;
        Queue<Rack> sortedRacks = SortRacksByItems(itemsInOrder.ToArray());
        float priceOfOrder = CheckPriceOfOrder(itemsInOrder);
        return new Order(sortedRacks, priceOfOrder, amountOfItemsFromRacks);
    }

    Queue<Rack> SortRacksByItems(Items.Item[] itemsInOrder)
    {
        Queue<Rack> racksWithItems = new Queue<Rack>();

        SortRacks();

        for(int i = 0; i < racks.Count; i++)
        {
            for(int j = 0; j < itemsInOrder.Length; j++)
            {
                if(racks[i].itemOnRack.itemType == itemsInOrder[j].itemType && racks[i].amountOfItems -  racks[i].reservedAmountOfItems > 0)
                {
                    racksWithItems.Enqueue(racks[i]);
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
            int currentRackPathLength = PathFinder.instance.FindPath(PickPackStationsManager.instance.ordersStation.tileWithBuilding,racks[i].tileWithBuilding).Count;
            for(int j = i+1; j < racks.Count; j++)
            {
                int lastPathLength = PathFinder.instance.FindPath(PickPackStationsManager.instance.ordersStation.tileWithBuilding,racks[i].tileWithBuilding).Count;
                if(currentRackPathLength>lastPathLength)
                {
                    Rack currentRack = racks[i];
                    racks[i] = racks[j];
                    racks[j] = currentRack;
                }
            }
        }
    }

    float CheckPriceOfOrder(Queue<Items.Item> itemsInOrder)
    {
        float price = 0;
        while(itemsInOrder.Count > 0)
        {
            price += FindPriceOfItem(itemsInOrder.Dequeue().itemType);
        }
        return price;
    }

    float FindPriceOfItem(Items.ItemType itemType)
    {
        float price = 0;

        for(int i = 0; i < PricesManager.instance.itemPricesScripts.Count; i++)
        {
            if(itemType == PricesManager.instance.itemPricesScripts[i].itemType)
            {
                string priceText = PricesManager.instance.itemPricesScripts[i].sellPriceInput.text;
                priceText = priceText.Remove(priceText.Length - 1);
                price = float.Parse(priceText);

                break;
            }
        }

        return price;
    }

    (Queue<Items.Item>, Queue<int>) DrawItemsForOrder()
    {

        Queue<Items.Item> drawnItems = new Queue<Items.Item>();
        Queue<int> amountOfItemsFromRacks = new Queue<int>();

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

                    if(racksChecked.Count == racks.Count && usedRacksIndexs.Contains(rackIndex))
                    {
                        if(racksChecked.Count == racks.Count)
                        {
                            return (null,null);
                        }

                        continue; 
                    }
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

                float sellPrice = float.Parse(PricesManager.instance.itemPricesScripts[itemIndex].sellPriceInput.text.Remove(PricesManager.instance.itemPricesScripts[itemIndex].sellPriceInput.text.Length - 1));
                float demandPrice = float.Parse(PricesManager.instance.itemPricesScripts[itemIndex].demandPrice.text.Remove(PricesManager.instance.itemPricesScripts[itemIndex].demandPrice.text.Length - 1));
                float probabilityModifier = Mathf.Clamp01(1.0f - ((sellPrice - demandPrice) / demandPrice)); 

                if (Random.value > probabilityModifier)
                {
                    Debug.Log("Item skipped due to overpricing");
                    if (racksChecked.Count == racks.Count)
                    {
                        return (null, null);
                    }
                    continue;
                }

                if(racks[rackIndex].amountOfItems-racks[rackIndex].reservedAmountOfItems > 0)
                {
                    usedRacksIndexs.Add(rackIndex);
                    break;
                }

                if(racksChecked.Count == racks.Count)
                {
                    return (null,null);
                }
            }

            int amountOfItemsFromRack = Random.Range(1,racks[rackIndex].amountOfItems-racks[rackIndex].reservedAmountOfItems);

            usedRacksIndexs.Add(rackIndex);

            if(amountOfItemsFromRack > amountOfItems - i)
            {
                amountOfItemsFromRack = amountOfItems - i;
            }

            i += amountOfItemsFromRack-1;

            for(int j = 0 ; j < amountOfItemsFromRack; j++)
            {
                amountOfItemsFromRacks.Enqueue(1);
            }
            

            for(int j = 0; j < amountOfItemsFromRack; j++)
            {
                drawnItems.Enqueue(racks[rackIndex].itemOnRack);
            }
            
        }

        return (drawnItems, amountOfItemsFromRacks);
    }
}

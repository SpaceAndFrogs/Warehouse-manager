using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrdersManager : MonoBehaviour
{
    public List<Order> ordersOnPick = new List<Order>();
    public List<Order> ordersOnPack = new List<Order>();
    public List<Rack> racks = new List<Rack>();

    [SerializeField]
    float timeBetweenOrders;

    [SerializeField]
    int maxAmountOfOrders;
    [SerializeField]
    Items items;
    public OrdersStation ordersStation;

    public class Order
    {
        public List<Rack> racksWithItems = new List<Rack>();
        public float orderPrice;

        public Order(List<Rack> racksWithItems, float orderPrice)
        {
            this.racksWithItems = racksWithItems;
            this.orderPrice = orderPrice;
        }
    }

    void OnEnable()
    {
        Rack.OnRackSpawned += AddRack;
        OrdersStation.OnStationSpawned += AddOrdersStation;
    }
    void OnDisable()
    {
        Rack.OnRackSpawned -= AddRack;
        OrdersStation.OnStationSpawned -= AddOrdersStation;
    }

    void AddRack(Rack rack)
    {
        racks.Add(rack);
    }

    void AddOrdersStation(OrdersStation ordersStation)
    {
        this.ordersStation = ordersStation;
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

                ordersOnPick.Add(order);
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
        List<Items.Item> itemsInOrder = DrawItemsForOrder();
        List<Rack> sortedRacks = SortRacksByItems(itemsInOrder);
        float priceOfOrder = CheckPriceOfOrder(itemsInOrder);
        return new Order(sortedRacks, priceOfOrder);
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
            int currentRackPathLength = PathFinder.instance.FindPath(ordersStation.tileWithStation,racks[i].tileWithRack).Length;
            for(int j = i+1; j < racks.Count; j++)
            {
                int lastPathLength = PathFinder.instance.FindPath(ordersStation.tileWithStation,racks[i].tileWithRack).Length;
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
            price += itemsInOrder[i].price;
        }
        return price;
    }

    List<Items.Item> DrawItemsForOrder()
    {

        List<Items.Item> drawnItems = new List<Items.Item>();

        int maxAmountOfItems = 1;
        for(int i = 0; i < racks.Count; i ++)
        {
            maxAmountOfItems += racks[i].amountOfItems - racks[i].reservedAmountOfItems;
        }

        int amountOfItems = Random.Range((int)1, maxAmountOfItems);
        List<int> usedRacksIndexs = new List<int>();
        for(int i = 0; i < amountOfItems; i++)
        {
            
            int rackIndex = Random.Range(0,racks.Count);
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

            for(int j = 0; j < amountOfItemsFromRack; j++)
            {
                drawnItems.Add(racks[rackIndex].itemOnRack);
            }
            
        }

        return drawnItems;
    }
}

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

    public class Order
    {
        public List<Items.Item> itemsInOrder = new List<Items.Item>();
        public List<Rack> racksWithItems = new List<Rack>();
        public float orderPrice;

        public Order(List<Items.Item> itemsInOrder, List<Rack> racksWithItems, float orderPrice)
        {
            this.itemsInOrder = itemsInOrder;
            this.racksWithItems = racksWithItems;
            this.orderPrice = orderPrice;
        }
    }

    IEnumerator GenerateOrders()
    {
        while(true)
        {
            int amountOfOrders = Random.Range((int)1, maxAmountOfOrders+1);
            
            for(int i = 0; i < amountOfOrders; i++)
            {
                Order order = MakeOrder();

                ordersOnPick.Add(order);
            }
            yield return new WaitForSeconds(timeBetweenOrders);
        }
        
    }

    Order MakeOrder()
    {
        Order order = null;

        List<Items.Item> itemsInOrder = DrawItemsForOrder();
        //Sort items for from closest to farthest
        return order;
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
            while(usedRacksIndexs.Contains(rackIndex))
            {
                rackIndex = Random.Range(0,racks.Count);
                amountOfItemsFromRack = Random.Range(1,racks[rackIndex].amountOfItems-racks[rackIndex].reservedAmountOfItems+1);
            }

            usedRacksIndexs.Add(rackIndex);

            if(amountOfItemsFromRack > amountOfItems - i-1)
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

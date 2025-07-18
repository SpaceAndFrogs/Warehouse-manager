using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickWorker : WorkerBase
{
    public Tile pickStashTile;
    public override void GoToStation()
    {
        endNode = PickPackStationsManager.instance.ordersStation.tileWithStation;
        if (endNode != null)
        {
            GetPathToTarget();
        }
    }

    protected override void FindPickStash()
    {
        PickStash pickStashWithLeastOrders = PickPackStationsManager.instance.pickStashes[0];
        for (int i = 1; i < PickPackStationsManager.instance.pickStashes.Count; i++)
        {
            if (pickStashWithLeastOrders.orders.Count == 0)
            {
                pickStashWithLeastOrders = PickPackStationsManager.instance.pickStashes[i];
            }
            else if (pickStashWithLeastOrders.orders.Count > PickPackStationsManager.instance.pickStashes[i].orders.Count)
            {
                pickStashWithLeastOrders = PickPackStationsManager.instance.pickStashes[i];
            }
        }

        pickStashTile = pickStashWithLeastOrders.tileWithStash;
    }

    protected override void StartTask()
    {
        endNode = currentTask.order.racksWithItems.Peek().tileWithRack;
        GetPathToTarget();
    }

    protected override void OnPathCompleted()
    {
        StopCoroutine(PathCompleted());
        StartCoroutine(PathCompleted());      
    }

    IEnumerator PathCompleted()
    {
        if (currentTask.order.racksWithItems.Count > 0)
        {
            if (currentTask.order.racksWithItems.Peek().isInRoom)
            {
                yield return new WaitForSeconds(stats.workSpeed);
            }
            else
            {
                yield return new WaitForSeconds(stats.workSpeed + stats.workSpeed * 0.8f);
            }

            TakeItemsFromRack();

            currentTask.order.racksWithItems.Dequeue();
            currentTask.order.amountOfItemsFromRacks.Dequeue();

        }

        if (currentTask.order.racksWithItems.Count > 0)
        {

            endNode = currentTask.order.racksWithItems.Peek().tileWithRack;

        }
        else if (endNode == pickStashTile)
        {
            PickStash pickStash = pickStashTile.building.buildingObject.GetComponent<PickStash>();

            if (pickStash.isInRoom)
            {
                yield return new WaitForSeconds(stats.workSpeed);
            }
            else
            {
                yield return new WaitForSeconds(stats.workSpeed + stats.workSpeed * 0.8f);
            }

            currentTask.task.taskClass = TasksTypes.TaskClass.Pack;
            currentTask.tileOfPickStashWithOrder = pickStashTile;
            TaskManager.instance.packTasks.Enqueue(currentTask);
            ReturnWorker();
            StopCoroutine(FollowPath());
        }
        else
        {
            FindPickStash();
            endNode = pickStashTile;
        }

        GetPathToTarget(); 
    }
    
    void TakeItemsFromRack()
    {
        currentTask.order.racksWithItems.Peek().GiveItems(currentTask.order.amountOfItemsFromRacks.Peek());
    }
}

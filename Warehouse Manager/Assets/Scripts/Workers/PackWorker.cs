using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackWorker : WorkerBase
{
    public Tile packStationTile;
    public Tile pickStashTile;
    public bool goingToPickStash = true;

    public override void GoToStation()
    {
        endNode = packStationTile;

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
            if (pickStashWithLeastOrders.orders.Count < PickPackStationsManager.instance.pickStashes[i].orders.Count)
            {
                pickStashWithLeastOrders = PickPackStationsManager.instance.pickStashes[i];
            }
        }

        pickStashTile = pickStashWithLeastOrders.tileWithStash;
    }

    protected override void StartTask()
    {
        endNode = currentTask.tileOfPickStashWithOrder;
        GetPathToTarget();
    }

    protected override void OnPathCompleted()
    {
        if (goingToPickStash)
        {
            goingToPickStash = false;
            endNode = packStationTile;
            GetPathToTarget();
        }
        else
        {
            StopCoroutine(StartPackOrder());
            StartCoroutine(StartPackOrder());
            goingToPickStash = true;
        }
    }
    
    IEnumerator StartPackOrder()
    {
        PackStation packStation = packStationTile.building.buildingObject.GetComponent<PackStation>();
        if(packStation.isInRoom)
        {
            yield return new WaitForSeconds(stats.workSpeed);
        }
        else
        {
            yield return new WaitForSeconds(stats.workSpeed + stats.workSpeed * 0.8f);
        }

        CashManager.instance.GetCash(currentTask.order.orderPrice);
        currentTask = null;
        ReturnWorker();
        yield break;
    }
}

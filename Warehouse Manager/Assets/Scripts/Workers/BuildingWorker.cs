using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BuildingWorker : WorkerBase
{
    #nullable enable
    public static event Action? OnBuildingEnded;
    #nullable disable
    protected override void StartTask()
    {
        endNode = currentTask.tileWithTask;
        GetPathToTarget();
    }

    protected override void OnPathCompleted()
    {
        StopCoroutine(StartBuildTask());
        StartCoroutine(StartBuildTask());
    }

    IEnumerator StartBuildTask()
    {

        if (currentTask.task.taskType == TasksTypes.TaskType.None)
            yield break;

        if (currentTask.task.taskType == TasksTypes.TaskType.Build)
        {
            yield return new WaitForSeconds(stats.workSpeed * currentTask.building.buildingTime);

            IndicatorsPool.instance.ReturnBuildingIndicator(currentTask.buildingIndicator);
            currentTask.tileWithTask.RemoveBuilding(false);
            currentTask.tileWithTask.ChangeTileType(currentTask.task.tileTypeAfterTask);
            BuildingsPool.Building newBuilding = BuildingsPool.instance.GetBuilding(currentTask.building.buildingType);
            newBuilding.buildingObject.transform.position = currentTask.tileWithTask.transform.position;
            currentTask.tileWithTask.building = newBuilding;
            currentTask.tileWithTask.haveTask = false;

            CheckIfBuildingIsRack(newBuilding.buildingObject);

            CheckIfBuildingIsOrdersStation(newBuilding.buildingObject);

            CheckIfBuildingIsPackStation(newBuilding.buildingObject);

            CheckIfBuildingIsPickStash(newBuilding.buildingObject);

            OnBuildingEnded?.Invoke();
        }
        else
        {
            yield return new WaitForSeconds(stats.workSpeed * currentTask.task.taskTime);
            currentTask.tileWithTask.haveTask = false;
            currentTask.tileWithTask.ChangeTileType(currentTask.task.tileTypeAfterTask);
            IndicatorsPool.instance.ReturnTaskIndicator(currentTask.taskIndicator);
        }


        currentTask = null;
        ReturnWorker();
        yield break;
    }

    void CheckIfBuildingIsRack(GameObject building)
    {
        Rack rack = building.GetComponent<Rack>();
        if (rack != null)
        {
            rack.tileWithBuilding = currentTask.tileWithTask;
        }
    }

    void CheckIfBuildingIsOrdersStation(GameObject building)
    {
        OrdersStation ordersStation = building.GetComponent<OrdersStation>();
        if (ordersStation != null)
        {
            ordersStation.tileWithBuilding = currentTask.tileWithTask;
        }
    }

    void CheckIfBuildingIsPackStation(GameObject building)
    {
        PackStation packStation = building.GetComponent<PackStation>();
        if (packStation != null)
        {
            packStation.tileWithBuilding = currentTask.tileWithTask;
        }
    }
    void CheckIfBuildingIsPickStash(GameObject building)
    {
        PickStash pickStash = building.GetComponent<PickStash>();
        if (pickStash != null)
        {
            pickStash.tileWithBuilding = currentTask.tileWithTask;
        }
    }
    
    public static void NotifyBuildingEnded()
    {
        OnBuildingEnded?.Invoke();
    }
}

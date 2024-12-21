using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Worker : MonoBehaviour
{
    public Tile endNode; 
    public Tile startNode;
    public Tile[] path = new Tile[0];
    
    public WorkerData workerData;

    public TaskManager.Task currentTask = null;

    public Tile packStationTile;
    public Tile pickStashTile;
    public bool goingToPickStash =true;

    public static event Action<Worker>? OnWorkerSpawned;
    public static event Action<Tile, OrdersManager.Order>? OnOrderAddedToStash;

    private void Start()
    {
        ReturnWorker();
        OnWorkerSpawned?.Invoke(this);
    }

    public void GoToStation()
    {
        switch(workerData.workerType)
        {
            case WorkerData.WorkerType.Pack:
            {
                endNode = packStationTile;
                break;
            }
            case WorkerData.WorkerType.Pick:
            {
                endNode = PickPackStationsManager.instance.ordersStation.tileWithStation;
                break;
            }
        }

        if(endNode != null)
        {
            GetPathToTarget();
        }
    }

    void FindPickStash()
    {
        
        PickStash pickStashWithLeastOrders = PickPackStationsManager.instance.pickStashes[0];
        for(int i = 1; i < PickPackStationsManager.instance.pickStashes.Count; i++)
        {
            if(workerData.workerType == WorkerData.WorkerType.Pick)
            {
                if(pickStashWithLeastOrders.orders.Count == 0)
                {
                    pickStashWithLeastOrders = PickPackStationsManager.instance.pickStashes[i];
                }
                else if(pickStashWithLeastOrders.orders.Count > PickPackStationsManager.instance.pickStashes[i].orders.Count)
                {
                    pickStashWithLeastOrders = PickPackStationsManager.instance.pickStashes[i];
                }
            }else
            {
                if(pickStashWithLeastOrders.orders.Count < PickPackStationsManager.instance.pickStashes[i].orders.Count)
                {
                    pickStashWithLeastOrders = PickPackStationsManager.instance.pickStashes[i];
                }
            }
        }
        pickStashTile = pickStashWithLeastOrders.tileWithStash;
        
    }

    public void GetTask(TaskManager.Task task)
    {
        currentTask = task;

        switch(task.task.taskClass)
        {
            case TasksTypes.TaskClass.Building:
            {
                StartBuildingTask();
                break;
            }
            case TasksTypes.TaskClass.Pick:
            {
                StartPickTask();
                break;
            }
            case TasksTypes.TaskClass.Pack:
            {
                StartPackTask();
                break;
            }
        }
        
    }

    void StartPackTask()
    {
        endNode = currentTask.tileOfPickStashWithOrder;
        GetPathToTarget();
    }

    void StartPickTask()
    {
        endNode = currentTask.order.racksWithItems[0].tileWithRack;
        GetPathToTarget();
    }
    void StartBuildingTask()
    {
        endNode = currentTask.tileWithTask;        
        GetPathToTarget();
    }
    public void GetPathToTarget()
    {
        path = PathFinder.instance.FindPath(startNode, endNode);

        if(path == null)
        {
            currentTask = null;
            endNode = null;
            return;
        }

        StopCoroutine(FollowPath());
        StartCoroutine(FollowPath());
    }

    IEnumerator FollowPath()
    {
        int i = 0;
        Vector3 direction = path[i].transform.position - transform.position;
        direction.Normalize();
        while (i < path.Length)
        {

            direction = path[i].transform.position - transform.position;
            direction.Normalize();
            transform.position += direction * workerData.moveSpeed * Time.deltaTime;

            if(Vector3.Distance(transform.position, path[i].transform.position)<= workerData.proxyMargin && i < path.Length - 1)
            { 
                i++;
            }

            if(Vector3.Distance(transform.position, path[i].transform.position)<= workerData.proxyMarginOfFinalTile && i == path.Length - 1)
            { 
                i++;
            }
            yield return new WaitForEndOfFrame();
        }

        if(currentTask == null)
        {
            yield break;
        }

        switch(currentTask.task.taskClass)
        {
            case TasksTypes.TaskClass.Building:
            {
                StopCoroutine(StartBuildTask());
                StartCoroutine(StartBuildTask());
                break;
            }
            case TasksTypes.TaskClass.Pack:
            {
                if(goingToPickStash)
                {
                    goingToPickStash = false;
                    endNode = packStationTile;
                    GetPathToTarget();
                }else
                {
                    StopCoroutine(StartPackOrder());
                    StartCoroutine(StartPackOrder());
                    goingToPickStash = true;
                }
                break;
            }
            case TasksTypes.TaskClass.Pick:
            {
                if(currentTask.order.racksWithItems.Count > 0)
                {
                    TakeItemsFromRack();
                    currentTask.order.racksWithItems.RemoveAt(0);
                    currentTask.order.amountOfItemsFromRacks.RemoveAt(0);
                }
                
                if(currentTask.order.racksWithItems.Count > 0)
                {        
                               
                    endNode = currentTask.order.racksWithItems[0].tileWithRack;
                    
                }else if(endNode == pickStashTile)
                {
                    OnOrderAddedToStash?.Invoke(pickStashTile,currentTask.order);
                    currentTask.task.taskClass = TasksTypes.TaskClass.Pack;
                    currentTask.tileOfPickStashWithOrder = pickStashTile;
                    TaskManager.instance.packTasks.Add(currentTask);
                    ReturnWorker();
                    StopCoroutine(FollowPath());
                    break;
                }else
                {
                    FindPickStash();
                    endNode = pickStashTile;
                }

                GetPathToTarget();
                break;
            }
        }     
    }

    void TakeItemsFromRack()
    {
        currentTask.order.racksWithItems[0].GiveItems(currentTask.order.amountOfItemsFromRacks[0]);
    }

    IEnumerator StartPackOrder()
    {
        float timer = 0;

        while(timer <= currentTask.task.taskTime)
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }


        currentTask = null;
        ReturnWorker();
        yield break;
    }

    IEnumerator StartBuildTask()
    {
        float timer = 0;

        if(currentTask.task.taskType != TasksTypes.TaskType.Build)
        yield break;
        
        while(timer <= currentTask.building.buildingTime)
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        currentTask.tileWithTask.ChangeTileType(currentTask.task.tileTypeAfterTask);

        GameObject newBuilding = Instantiate(currentTask.building.buildingObject, currentTask.tileWithTask.transform.position, currentTask.tileWithTask.transform.rotation);
        currentTask.tileWithTask.building = newBuilding;

        CheckIfBuildingIsRack(newBuilding);

        CheckIfBuildingIsOrdersStation(newBuilding);

        CheckIfBuildingIsPackStation(newBuilding);

        CheckIfBuildingIsPickStash(newBuilding);
        
        currentTask = null;
        ReturnWorker();
        yield break;
    }

    void CheckIfBuildingIsRack(GameObject building)
    {
        Rack rack = building.GetComponent<Rack>();
            if(rack != null)
            {
                rack.tileWithRack = currentTask.tileWithTask;
            }
    }

    void CheckIfBuildingIsOrdersStation(GameObject building)
    {
        OrdersStation ordersStation= building.GetComponent<OrdersStation>();
            if(ordersStation != null)
            {
                ordersStation.tileWithStation = currentTask.tileWithTask;
            }  
    }

    void CheckIfBuildingIsPackStation(GameObject building)
    {
        PackStation packStation= building.GetComponent<PackStation>();
            if(packStation != null)
            {
                packStation.tileWithStation = currentTask.tileWithTask;
            }  
    }
    void CheckIfBuildingIsPickStash(GameObject building)
    {
        PickStash pickStash= building.GetComponent<PickStash>();
            if(pickStash != null)
            {
                pickStash.tileWithStash = currentTask.tileWithTask;
            }  
    }

    void ReturnWorker()
    {
        TaskManager.instance.ReturnWorker(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Tile tile = collision.gameObject.GetComponent<Tile>();

        if (tile == null)
            return;

        startNode = tile; 
    }

    
}

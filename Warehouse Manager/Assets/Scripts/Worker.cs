using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Worker : MonoBehaviour
{
    public Tile endNode; 
    public Tile startNode;
    public Tile[] path = new Tile[0];
    
    [SerializeField]
    public Stats stats;

    public TaskManager.Task currentTask = null;

    public Tile packStationTile;
    public Tile pickStashTile;
    public bool goingToPickStash =true;

    public static event Action<Worker>? OnWorkerSpawned;
    public static event Action<Tile, OrdersManager.Order>? OnOrderAddedToStash;
    public static event Action? OnBuildingEnded;

    private void Start()
    {
        ReturnWorker();
        OnWorkerSpawned?.Invoke(this);
    }

    public void GoToStation()
    {
        switch(stats.workerType)
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
            if(stats.workerType == WorkerData.WorkerType.Pick)
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
            case TasksTypes.TaskClass.Build:
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
            Debug.Log("Drop task");
            TaskManager.instance.DropTask(currentTask);
            currentTask = null;
            endNode = null;
            return;
        }

        StopCoroutine(FollowPath());
        StartCoroutine(FollowPath());
    }

    IEnumerator FollowPath()
    {
        if(path.Length == 0)
        {
            Debug.Log("null path");
        }
        int i = 0;
        Vector3 fixedPositionOfTile = new Vector3(path[i].transform.position.x,transform.position.y,path[i].transform.position.z);
        Vector3 direction = fixedPositionOfTile - transform.position;
        direction.Normalize();
        while (i < path.Length)
        {
            fixedPositionOfTile = new Vector3(path[i].transform.position.x,transform.position.y,path[i].transform.position.z);
            direction = fixedPositionOfTile - transform.position;
            direction.Normalize();
            transform.position += direction * stats.moveSpeed * Time.deltaTime;

            if(Vector3.Distance(transform.position, fixedPositionOfTile)<= stats.proxyMargin && i < path.Length-1)
            { 
                i++;
            }

            if(Vector3.Distance(transform.position, fixedPositionOfTile)<= stats.proxyMarginOfFinalTile && i == path.Length - 1)
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
            case TasksTypes.TaskClass.Build:
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
                    if(currentTask.order.racksWithItems[0].isInRoom)
                    {
                        yield return new WaitForSeconds(stats.workSpeed);
                    }
                    else
                    {
                        yield return new WaitForSeconds(stats.workSpeed + stats.workSpeed * 0.8f);
                    }
                    
                    TakeItemsFromRack();

                    currentTask.order.racksWithItems.RemoveAt(0);
                    currentTask.order.amountOfItemsFromRacks.RemoveAt(0);

                    
                }
                
                if(currentTask.order.racksWithItems.Count > 0)
                {        
                               
                    endNode = currentTask.order.racksWithItems[0].tileWithRack;
                    
                }else if(endNode == pickStashTile)
                {
                    PickStash pickStash = pickStashTile.building.GetComponent<PickStash>();

                    if(pickStash.isInRoom)
                    {
                        yield return new WaitForSeconds(stats.workSpeed);
                    }
                    else
                    {
                        yield return new WaitForSeconds(stats.workSpeed + stats.workSpeed * 0.8f);
                    }

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
        PackStation packStation = packStationTile.building.GetComponent<PackStation>();
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

    IEnumerator StartBuildTask()
    {

        if(currentTask.task.taskType == TasksTypes.TaskType.None)
        yield break;
        
        if(currentTask.task.taskType == TasksTypes.TaskType.Build)
        {
            yield return new WaitForSeconds(stats.workSpeed * currentTask.building.buildingTime);

            IndicatorsPool.instance.ReturnIndicator(currentTask.indicator);
            currentTask.tileWithTask.RemoveBuilding(false);
            currentTask.tileWithTask.ChangeTileType(currentTask.task.tileTypeAfterTask);
            GameObject newBuilding = Instantiate(currentTask.building.buildingObject, currentTask.tileWithTask.transform.position, currentTask.tileWithTask.transform.rotation);
            currentTask.tileWithTask.building = newBuilding;

            CheckIfBuildingIsRack(newBuilding);

            CheckIfBuildingIsOrdersStation(newBuilding);

            CheckIfBuildingIsPackStation(newBuilding);

            CheckIfBuildingIsPickStash(newBuilding);

            OnBuildingEnded?.Invoke();
        }
        else
        {
            yield return new WaitForSeconds(stats.workSpeed * currentTask.task.taskTime);
            currentTask.tileWithTask.ChangeTileType(currentTask.task.tileTypeAfterTask);   
        }
        
        
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

    [System.Serializable]
    public class Stats
    {
        public WorkerData.WorkerType workerType;
        public float moveSpeed;
        public float workSpeed;
        public float salary;
        public float proxyMargin;
        public float proxyMarginOfFinalTile;
        public string name;
        public Stats(float moveSpeed, float workSpeed, float salary, WorkerData.WorkerType workerType, float proxyMargin, float proxyMarginOfFinalTile, string name)
        {
            this.moveSpeed = moveSpeed;
            this.workSpeed = workSpeed;
            this.salary = salary;
            this.workerType = workerType;
            this.proxyMargin = proxyMargin;
            this.proxyMarginOfFinalTile = proxyMarginOfFinalTile;
            this.name = name;
        }
    }
}

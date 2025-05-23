using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Worker : MonoBehaviour
{
    public Tile endNode; 
    public Tile startNode;
    public Queue<Tile> path = new Queue<Tile>();
    
    [SerializeField]
    public Stats stats;

    public TaskManager.Task currentTask = null;

    public Tile packStationTile;
    public Tile pickStashTile;
    public bool goingToPickStash =true;

    #nullable enable
    public static event Action<Worker>? OnWorkerSpawned;
    public static event Action<Worker>? OnWorkerFired;
    public static event Action<Tile, OrdersManager.Order>? OnOrderAddedToStash;
    public static event Action? OnBuildingEnded;
    #nullable disable

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
        endNode = currentTask.order.racksWithItems.Peek().tileWithRack;
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
        if(path.Count == 0)
        {
            Debug.Log("null path");
        }
        Tile nextTile = path.Dequeue();
        Vector3 fixedPositionOfTile = new Vector3(nextTile.transform.position.x,transform.position.y,nextTile.transform.position.z);
        Vector3 direction = fixedPositionOfTile - transform.position;
        direction.Normalize();
        while (path.Count > 0)
        {
            fixedPositionOfTile = new Vector3(nextTile.transform.position.x,transform.position.y,nextTile.transform.position.z);
            direction = fixedPositionOfTile - transform.position;
            direction.Normalize();
            transform.position += direction * stats.moveSpeed * Time.deltaTime;

            if(Vector3.Distance(transform.position, fixedPositionOfTile)<= stats.proxyMargin && 1 < path.Count)
            { 
                nextTile = path.Dequeue();
            }

            if(Vector3.Distance(transform.position, fixedPositionOfTile)<= stats.proxyMarginOfFinalTile && 1 == path.Count)
            { 
                nextTile = path.Dequeue();
            }

            if(!nextTile.walkable && nextTile != endNode)
            {
                Debug.Log("Drop task");
                TaskManager.instance.DropTask(currentTask);
                currentTask = null;
                endNode = null;
                yield break;
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
                    if(currentTask.order.racksWithItems.Peek().isInRoom)
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
                
                if(currentTask.order.racksWithItems.Count > 0)
                {        
                               
                    endNode = currentTask.order.racksWithItems.Peek().tileWithRack;
                    
                }else if(endNode == pickStashTile)
                {
                    PickStash pickStash = pickStashTile.building.buildingObject.GetComponent<PickStash>();

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
                    TaskManager.instance.packTasks.Enqueue(currentTask);
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
        currentTask.order.racksWithItems.Peek().GiveItems(currentTask.order.amountOfItemsFromRacks.Peek());
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

    IEnumerator StartBuildTask()
    {

        if(currentTask.task.taskType == TasksTypes.TaskType.None)
        yield break;
        
        if(currentTask.task.taskType == TasksTypes.TaskType.Build)
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

    public void HireWorker()
    {
        ReturnWorker();
        OnWorkerSpawned?.Invoke(this);
    }

    public void FireWorker()
    {
        if(currentTask != null)
            TaskManager.instance.DropTask(currentTask);

        OnWorkerFired?.Invoke(this);
        TaskManager.instance.FireWorker(this);

        WorkersPool.instance.ReturnWorker(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Tile tile = collision.gameObject.GetComponent<Tile>();

        if (tile == null)
            return;

        startNode = tile; 
    }
    
    private void OnEnable() 
    {
        SavingManager.OnSave += OnSave;
    }

    private void OnDisable() 
    {
        SavingManager.OnSave -= OnSave;
    }

    void OnSave()
    {
        SaveWorkerData();
        SaveTask();
    }

    void SaveTask()
    {
        if(currentTask == null)
            return;

        if (currentTask.task.taskClass == TasksTypes.TaskClass.Build)
        {
            SaveData.TaskData taskData = new SaveData.TaskData(currentTask.task.taskType.ToString(), currentTask.task.taskClass.ToString(), currentTask.tileWithTask.transform.position, new Vector3(), currentTask.task.tileTypeAfterTask.ToString(), new List<Vector3>(), 0, new List<int>());
            SavingManager.instance.saveData.tasks.Add(taskData);
        }
        else if (currentTask.task.taskClass == TasksTypes.TaskClass.Pick)
        {
            List<Vector3> racksWithItems = ConvertRacksToVector3(new List<Rack>(currentTask.order.racksWithItems));
            SaveData.TaskData taskData = new SaveData.TaskData(currentTask.task.taskType.ToString(), currentTask.task.taskClass.ToString(), new Vector3(), new Vector3(), null, racksWithItems, currentTask.order.orderPrice, new List<int>(currentTask.order.amountOfItemsFromRacks));
            SavingManager.instance.saveData.tasks.Add(taskData);
        }
        else
        {
            SaveData.TaskData taskData = new SaveData.TaskData(currentTask.task.taskType.ToString(), currentTask.task.taskClass.ToString(), new Vector3(), currentTask.tileOfPickStashWithOrder.transform.position, null, null, currentTask.order.orderPrice, null);
            SavingManager.instance.saveData.tasks.Add(taskData);
        }
    }

    List<Vector3> ConvertRacksToVector3(List<Rack> racks)
    {
        List<Vector3> positions = new List<Vector3>();

        for(int i = 0; i < racks.Count; i++)
        {
            positions.Add(racks[i].transform.position);
        }

        return positions;
    }

    void SaveWorkerData()
    {
        SaveData.WorkerData workerData = new SaveData.WorkerData(stats.name,stats.workerType.ToString(),stats.moveSpeed,stats.workSpeed,stats.salary,transform.position,transform.rotation);
        SavingManager.instance.saveData.workers.Add(workerData);
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

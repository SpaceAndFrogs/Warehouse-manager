using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : MonoBehaviour
{
    public Tile endNode; 
    public Tile startNode;
    public Tile[] path = new Tile[0];
    
    public WorkerData workerData;

    TaskManager.Task currentTask = null;

    public Tile packStationTile;
    public Tile pickStashTile;
    public bool goingToPickStash =false;

    private void Start()
    {
        ReturnWorker();
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
                break;
            }
        }
        
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
                    StopCoroutine(StartPackTask());
                    StartCoroutine(StartPackTask());
                }
                break;
            }
            case TasksTypes.TaskClass.Pick:
            {
                currentTask.order.racksWithItems.RemoveAt(0);
                if(currentTask.order.racksWithItems.Count > 0)
                {
                    endNode = currentTask.order.racksWithItems[0].tileWithRack;
                }else if(endNode == pickStashTile)
                {
                    StopCoroutine(FollowPath());
                }else
                {
                    endNode = pickStashTile;
                }

                GetPathToTarget();
                break;
            }
        }     
    }

    IEnumerator StartPackTask()
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
        {
            while(timer <= currentTask.task.taskTime)
            {
                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            currentTask.tileWithTask.ChangeTileType(currentTask.task.tileTypeAfterTask);
        }else
        {
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

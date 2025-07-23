using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class WorkerBase : MonoBehaviour
{
    public Tile endNode;
    public Tile startNode;
    public Queue<Tile> path = new Queue<Tile>();
    [SerializeField]
    public Stats stats;
    public TaskManager.Task currentTask = null;

    #nullable enable
    public static event Action<WorkerBase>? OnWorkerSpawned;
    public static event Action<WorkerBase>? OnWorkerFired;
    #nullable disable

    public virtual void GoToStation()
    {
        // This method should be overridden in derived classes to implement specific behavior
        Debug.LogWarning("GoToStation method not implemented in " + this.GetType().Name);
    }

    protected virtual void FindPickStash()
    {
        // This method can be overridden in derived classes if needed
        Debug.LogWarning("FindPickStash method not implemented in " + this.GetType().Name);
    }

    public void GetTask(TaskManager.Task task)
    {
        currentTask = task;
        
        StartTask();
    }

    protected abstract void StartTask();

    public void GetPathToTarget()
    {
        path = PathFinder.instance.FindPath(startNode, endNode);

        if(path == null)
        {
            Debug.LogWarning("Path not found from");
            TaskManager.instance.DropTask(currentTask);
            currentTask = null;
            endNode = null;
            return;
        }

        StopCoroutine(FollowPath());
        StartCoroutine(FollowPath());
    }

    protected IEnumerator FollowPath()
    {
        Tile nextTile = path.Dequeue();
        Vector3 fixedPositionOfTile = new Vector3(nextTile.transform.position.x, transform.position.y, nextTile.transform.position.z);
        Vector3 direction = fixedPositionOfTile - transform.position;
        direction.Normalize();
        while (path.Count > 0)
        {
            fixedPositionOfTile = new Vector3(nextTile.transform.position.x, transform.position.y, nextTile.transform.position.z);
            direction = fixedPositionOfTile - transform.position;
            direction.Normalize();
            transform.position += direction * stats.moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, fixedPositionOfTile) <= stats.proxyMargin && 1 < path.Count)
            {
                nextTile = path.Dequeue();
            }

            if (Vector3.Distance(transform.position, fixedPositionOfTile) <= stats.proxyMarginOfFinalTile && 1 == path.Count)
            {
                nextTile = path.Dequeue();
            }

            if (!nextTile.walkable && nextTile != endNode)
            {
                TaskManager.instance.DropTask(currentTask);
                currentTask = null;
                endNode = null;
                ReturnWorker();
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }

        if (currentTask == null)
        {
            yield break;
        }

        OnPathCompleted();
    }

    protected abstract void OnPathCompleted();

    protected void ReturnWorker()
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
        if (currentTask.task.taskType == TasksTypes.TaskType.None)
            return;

        if (currentTask.task.taskClass == TasksTypes.TaskClass.Build)
        {
            SaveData.TaskData taskData = new SaveData.TaskData(currentTask.task.taskType.ToString(), currentTask.task.taskClass.ToString(), currentTask.tileWithTask.transform.position, new Vector3(), currentTask.task.tileTypeAfterTask.ToString(), new List<Vector3>(), 0, new List<int>(),currentTask.building.buildingType.ToString(), currentTask.rotationTransform.rotation.eulerAngles);
            SavingManager.instance.saveData.tasks.Add(taskData);
        }
        else if (currentTask.task.taskClass == TasksTypes.TaskClass.Pick)
        {
            List<Vector3> racksWithItems = ConvertRacksToVector3(new List<Rack>(currentTask.order.racksWithItems));
            SaveData.TaskData taskData = new SaveData.TaskData(currentTask.task.taskType.ToString(), currentTask.task.taskClass.ToString(), new Vector3(), new Vector3(), null, racksWithItems, currentTask.order.orderPrice, new List<int>(currentTask.order.amountOfItemsFromRacks),null,Vector3.zero);
            SavingManager.instance.saveData.tasks.Add(taskData);
        }
        else if(currentTask.task.taskClass == TasksTypes.TaskClass.Pack)
        {
            SaveData.TaskData taskData = new SaveData.TaskData(currentTask.task.taskType.ToString(), currentTask.task.taskClass.ToString(), new Vector3(), currentTask.tileOfPickStashWithOrder.transform.position, null, null, currentTask.order.orderPrice, null,null,Vector3.zero);
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

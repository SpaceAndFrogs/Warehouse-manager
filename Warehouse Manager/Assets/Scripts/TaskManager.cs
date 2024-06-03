using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskManager : MonoBehaviour
{
    public static TaskManager instance { get; private set; }

    public List<Task> tasks = new List<Task>();

    public TasksTypes.Task currentTask;

    public List<Worker> freeWorkers = new List<Worker>();

    [SerializeField]
    Transform uiCanvasTransform;
    [SerializeField]
    Button buttonPrefab;

    [SerializeField]
    TasksTypes tasksTypes;
    public class Task
    {
        public TasksTypes.Task task;
        public Tile tileWithTask;
        public Task(TasksTypes.Task task, Tile tileWithTask)
        {
            this.task = task;
            this.tileWithTask = tileWithTask;
        }
    }

    void CreateButtons()
    {
        for(int i = 0; i < tasksTypes.tasks.Count; i ++)
        {
            int index = i;
            Button newButton = Instantiate(buttonPrefab,uiCanvasTransform);
            newButton.name = "Button: " + tasksTypes.tasks[i].nameOfButton;
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = tasksTypes.tasks[i].nameOfButton;
            newButton.onClick.AddListener(() => SetCurrentTask(index));
        }
    }

    public void SetCurrentTask(int indexOfTask)
    {
        currentTask = tasksTypes.tasks[indexOfTask];
    }

    private void Awake()
    {
        if(instance != null) 
        {
            Destroy(instance);
        }else
        {
            instance = this;
        }
    }

    void Start()
    {
        CreateButtons();
    }

    private void Update()
    {        
        CheckForInput();
        GiveTasksToWorkers();
    }

    void CheckForInput()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                
                Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

                if (tile == null)
                    return;

                if(!IsTileCompatibleWithTask(tile))
                    return;

                tasks.Add(new Task(currentTask,tile));
            }

        }
    }

    bool IsTileCompatibleWithTask(Tile tile)
    {
        switch(tile.tileType)
        {
            case TileTypes.TileType.Ground:
            {
                if(currentTask.taskType == TasksTypes.TaskType.Build)
                    return true;

                return false;    
            }
            case TileTypes.TileType.Water:
            {
                if(currentTask.taskType == TasksTypes.TaskType.Dry)
                    return true;

                return false;    
            }
            case TileTypes.TileType.Rocks:
            {
                if(currentTask.taskType == TasksTypes.TaskType.Mine)
                    return true;

                return false;    
            }
        }

        return false;
    }

    public void ReturnWorker(Worker worker)
    {
        freeWorkers.Add(worker);
    }

    void GiveTasksToWorkers()
    {
        if (tasks.Count == 0)
            return;

        if (freeWorkers.Count == 0)
            return;

        List<Task> givenTasks = new List<Task>();

        for(int i = 0; i < tasks.Count; i++)
        {
            Worker workerForTask = FindClosestWorker(tasks[i].tileWithTask);

            if (workerForTask == null)
                continue;

            workerForTask.GetTask(tasks[i]);

            givenTasks.Add(tasks[i]);

            freeWorkers.Remove(workerForTask);
        }

        for(int i = 0;i < givenTasks.Count;i++)
        {
            tasks.Remove(givenTasks[i]);
        }
    }

    Worker FindClosestWorker(Tile endTile)
    {
        int currentClosestPath = MapGenerator.instance.GetAmountOfAllTiles();
        Worker currentClosestWorker = null;

        for (int i = 0; i < freeWorkers.Count; i++)
        {
            Tile[] path = PathFinder.instance.FindPath(freeWorkers[i].startNode, endTile);

            if(path == null)
                continue;

            if(path.Length < currentClosestPath)
            {
                currentClosestWorker = freeWorkers[i];
                currentClosestPath = path.Length;
            }

        }

        if (currentClosestWorker! != null)
            return currentClosestWorker;

        return null;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class TaskManager : MonoBehaviour
{
    public static TaskManager instance { get; private set; }

    public List<Task> buildingTasks = new List<Task>();

    public List<Task> pickTasks = new List<Task>();
    public List<Task> packTasks = new List<Task>();

    [SerializeField]
    OrdersManager ordersManager;

    public TasksTypes.Task currentTask;

    public Buildings.Building currentBuilding;

    public List<Worker> freeBuilders = new List<Worker>();
    public List<Worker> freePickWorkers = new List<Worker>();
    public List<Worker> freePackWorkers = new List<Worker>();

    [SerializeField]
    Transform tasksCanvasTransform;

    [SerializeField]
    Transform buildingsCanvasTransform;

    [SerializeField]
    Button buttonPrefab;

    [SerializeField]
    TasksTypes tasksTypes;

    [SerializeField]
    Buildings buildings;
    [SerializeField]
    Button backToTasksButton;

    public class Task
    {
        public TasksTypes.Task task;
        public Tile tileWithTask;
        public Buildings.Building building;
        public Task(TasksTypes.Task task, Tile tileWithTask, Buildings.Building building)
        {
            this.task = task;
            this.tileWithTask = tileWithTask;
            this.building = building;
        }
    }

    void CreateButtons()
    {
        for(int i = 0; i < tasksTypes.tasks.Count; i ++)
        {
            int index = i;
            Button newButton = Instantiate(buttonPrefab,tasksCanvasTransform);
            newButton.name = "Button: " + tasksTypes.tasks[i].nameOfButton;
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = tasksTypes.tasks[i].nameOfButton;
            newButton.onClick.AddListener(() => SetCurrentTask(index));

            if(tasksTypes.tasks[i].taskType == TasksTypes.TaskType.Build)
            {
                CreateBuildingButtons();
            }
        }
    }

    void CreateBuildingButtons()
    {
        Button backButton = Instantiate(buttonPrefab, buildingsCanvasTransform);
        backButton.name = "Button: Back To Tasks";
        backButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back To Tasks";
        backButton.onClick.AddListener(() => BackToTaskButtons());

        for(int i = 0; i < buildings.buildings.Count; i ++)
        {
            int index = i;
            Button newButton = Instantiate(buttonPrefab, buildingsCanvasTransform);
            newButton.name = "Button: " + buildings.buildings[i].nameOfButton;
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = buildings.buildings[i].nameOfButton;
            newButton.onClick.AddListener(() => SetCurrentBuilding(index));
        }
    }

    public void BackToTaskButtons()
    {
        tasksCanvasTransform.gameObject.SetActive(true);
        buildingsCanvasTransform.gameObject.SetActive(false);
    }

    public void SetCurrentTask(int indexOfTask)
    {
        currentTask = tasksTypes.tasks[indexOfTask];

        if(tasksTypes.tasks[indexOfTask].taskType == TasksTypes.TaskType.Build)
        {
            tasksCanvasTransform.gameObject.SetActive(false);
            buildingsCanvasTransform.gameObject.SetActive(true);
        }
    }

    public void SetCurrentBuilding(int indexOfBuilding)
    {
        currentBuilding = buildings.buildings[indexOfBuilding];
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
        GiveTasksToBuilders();
        GiveTasksToPick();
        GiveTasksToPack();
    }

    bool IsMouseOverUi()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    void CheckForInput()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) && !IsMouseOverUi()) 
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

                buildingTasks.Add(new Task(currentTask,tile, currentBuilding));
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
        switch(worker.workerData.workerType)
        {
            case WorkerData.WorkerType.Builder:
            {
                freeBuilders.Add(worker);
                break;
            }
        }
        
    }

    void GiveTasksToBuilders()
    {
        if (buildingTasks.Count == 0)
            return;

        if (freeBuilders.Count == 0)
            return;

        List<Task> givenBuildingTasks = new List<Task>();

        for(int i = 0; i < buildingTasks.Count; i++)
        {
            Worker workerForTask = FindClosestWorker(buildingTasks[i].tileWithTask);

            if (workerForTask == null)
                continue;

            workerForTask.GetTask(buildingTasks[i]);

            givenBuildingTasks.Add(buildingTasks[i]);

            freeBuilders.Remove(workerForTask);
        }

        for(int i = givenBuildingTasks.Count - 1; i >= 0; i--)
        {
            buildingTasks.Remove(givenBuildingTasks[i]);
        }
    }

    void GiveTasksToPick()
    {
        if (ordersManager.ordersOnPick.Count == 0)
            return;

        if (freePickWorkers.Count == 0)
            return;

        List<Task> givenPickTasks = new List<Task>();

        for(int i = 0; i < ordersManager.ordersOnPick.Count; i++)
        {
            Worker workerForTask = freePickWorkers[0];

            if (workerForTask == null)
                continue;

            workerForTask.GetTask(pickTasks[i]);

            givenPickTasks.Add(pickTasks[i]);

            freePickWorkers.Remove(workerForTask);
        }

        for(int i = givenPickTasks.Count - 1; i >= 0; i--)
        {
            pickTasks.Remove(givenPickTasks[i]);
        }
    }

    void GiveTasksToPack()
    {
        if (ordersManager.ordersOnPack.Count == 0)
            return;

        if (freePackWorkers.Count == 0)
            return;

        List<Task> givenPackTasks = new List<Task>();

        for(int i = 0; i < ordersManager.ordersOnPack.Count; i++)
        {
            Worker workerForTask = freePackWorkers[0];

            if (workerForTask == null)
                continue;

            workerForTask.GetTask(packTasks[i]);

            givenPackTasks.Add(packTasks[i]);

            freePackWorkers.Remove(workerForTask);
        }

        for(int i = givenPackTasks.Count - 1; i >= 0; i--)
        {
            packTasks.Remove(givenPackTasks[i]);
        }
    }

    Worker FindClosestWorker(Tile endTile)
    {
        int currentClosestPath = MapGenerator.instance.GetAmountOfAllTiles();
        Worker currentClosestWorker = null;

        for (int i = 0; i < freeBuilders.Count; i++)
        {
            Tile[] path = PathFinder.instance.FindPath(freeBuilders[i].startNode, endTile);

            if(path == null)
                continue;

            if(path.Length < currentClosestPath)
            {
                currentClosestWorker = freeBuilders[i];
                currentClosestPath = path.Length;
            }

        }

        if (currentClosestWorker! != null)
            return currentClosestWorker;

        return null;
    }
}

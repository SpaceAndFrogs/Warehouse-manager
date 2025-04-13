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

    #region Tasks Variables
    public Queue<Task> buildingTasks = new Queue<Task>();
    public Queue<Task> pickTasks = new Queue<Task>();
    public Queue<Task> packTasks = new Queue<Task>();
    public Queue<Task> dropedBuildingTasks = new Queue<Task>();
    public Queue<Task> dropedPickTasks = new Queue<Task>();
    public Queue<Task> dropedPackTasks = new Queue<Task>();
    public TasksTypes.Task currentTask;
    #endregion
    
    #region Orders Variables
    public OrdersManager ordersManager;
    #endregion

    #region Buildings Variables
    Tile currentTile = null;
    Tile currentTileBuild = null;
    public Buildings.Building currentBuilding = null;
    #endregion

    #region Indicators Variables
    Tile lastIndicatorEndTile = null;
    List<IndicatorsPool.Indicator> indicators = new List<IndicatorsPool.Indicator>();
    #endregion
    
    #region Workers Variables
    public HashSet<Worker> freeBuilders = new HashSet<Worker>();
    public Queue<Worker> freePickWorkers = new Queue<Worker>();
    public Queue<Worker> freePackWorkers = new Queue<Worker>();
    #endregion

    #region Ui Variables
    [SerializeField] Transform tasksCanvasTransform;  
    [SerializeField] Transform buildingsCanvasTransform;   
    [SerializeField] Button buttonPrefab;   
    [SerializeField] TasksTypes tasksTypes;  
    [SerializeField] Buildings buildings;
    [SerializeField] Button backToTasksButton;
    #endregion

    #region Classes
    [System.Serializable]
    public class Task
    {
        public TasksTypes.Task task;
        public Tile tileWithTask;
        public Buildings.Building building;
        public OrdersManager.Order order;
        public Tile tileOfPickStashWithOrder;
        public IndicatorsPool.Indicator indicator;
        public Task(TasksTypes.Task task, Tile tileWithTask, Buildings.Building building,IndicatorsPool.Indicator indicator, OrdersManager.Order order,Tile tileOfPickStashWithOrder)
        {
            this.task = task;
            this.tileWithTask = tileWithTask;
            this.building = building;
            this.order = order;
            this.tileOfPickStashWithOrder = tileOfPickStashWithOrder;
            this.indicator = indicator;
        }
    }
    #endregion
    
    #region Start Methods
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
        SetIndicators();
        GiveTasksToBuilders();
        ConvertOrdersToTasks();
        GiveTasksToPick();
        GiveTasksToPack();
    }

    void OnEnable()
    {
        Worker.OnBuildingEnded += FreeDropedTasks;
    }

    void OnDisable()
    {
        Worker.OnBuildingEnded -= FreeDropedTasks;
    }
    #endregion

    #region Ui Methods
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
            tasksCanvasTransform.gameObject.SetActive(true);
            buildingsCanvasTransform.gameObject.SetActive(false);
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

    bool IsMouseOverUi()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
    #endregion
    
    #region Indicators Methods
    void SetIndicators()
    {
        if(currentTile == null || IsMouseOverUi() || currentBuilding.buildingType == Buildings.BuildingType.None)
            return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide))
        {
                
            Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

            if (tile == null || lastIndicatorEndTile == tile)
                return;

            lastIndicatorEndTile = tile;

            ReturnIndicators();

            if(currentBuilding.buildingType == Buildings.BuildingType.Wall)
            {
                MakeContourIndicators(tile); 
            }
            else
            {
                MakeFillIndicators(tile);         
            }  
                                
        }           
    }

    void ReturnIndicators()
    {
        for(int i = indicators.Count - 1; i >= 0; i--)
        {
            IndicatorsPool.instance.ReturnIndicator(indicators[i]);
            indicators.RemoveAt(i);
        }
    }

    void MakeContourIndicators(Tile endTile)
    {
        List<Tile> tiles = Figuers.instance.MakeFiguer(currentTile, endTile, Figuers.FiguersType.Contour);

        for(int i = 0; i < tiles.Count; i++)
        {
            indicators.Add(MakeIndicator(tiles[i]));
        }
    }

    void MakeFillIndicators(Tile endTile)
    {
        List<Tile> tiles = Figuers.instance.MakeFiguer(currentTile, endTile, Figuers.FiguersType.Fill);

        for(int i = 0; i < tiles.Count; i++)
        {
            indicators.Add(MakeIndicator(tiles[i]));
        }
    }

    IndicatorsPool.Indicator MakeIndicator(Tile tile)
    {
        IndicatorsPool.Indicator indicator = IndicatorsPool.instance.GetIndicator(tile.tileType, currentBuilding.buildingType);
        indicator.indicatorObject.transform.position = tile.transform.position;
    
        return indicator;
    }
    #endregion
    
    #region Building Methods
    void Contour(Tile endTile)
    {
        List<Tile> tiles = Figuers.instance.MakeFiguer(currentTile, endTile, Figuers.FiguersType.Contour);

        for(int i = 0; i < tiles.Count; i++)
        {
            MakeBuildingTask(tiles[i]);
        }
    }

    void Fill(Tile endTile)
    {
        List<Tile> tiles = Figuers.instance.MakeFiguer(currentTile, endTile, Figuers.FiguersType.Fill);

        for(int i = 0; i < tiles.Count; i++)
        {
            MakeBuildingTask(tiles[i]);
        }
    }

    void FillForBuild(Tile endTile)
    {
        List<Tile> tiles = Figuers.instance.MakeFiguer(currentTileBuild, endTile, Figuers.FiguersType.Fill);

        for(int i = 0; i < tiles.Count; i++)
        {
            MakeBuildTask(tiles[i]);
        }
    }

    void CheckForAmountAndTilesForBuildings(Tile endTile, bool fill)
    {
        if(endTile == currentTile)
        {
            MakeBuildingTask(endTile);
            currentTile = null;
            return;
        }

        if(fill)
        {
            Fill(endTile);
        }
        else
        {
            Contour(endTile);
        }

        currentTile = null;
            
    }

    void CheckForAmountAndTilesForBuild(Tile endTile)
    {
        if(endTile == currentTileBuild)
        {
            MakeBuildTask(endTile);
            currentTileBuild = null;
            return;
        }

        FillForBuild(endTile);

        currentTileBuild = null;
            
    }
    #endregion
    
    #region Tasks Methods

    void FreeDropedTasks()
    {
        while(dropedBuildingTasks.Count > 0)
        {
            buildingTasks.Enqueue(dropedBuildingTasks.Dequeue());
        }
        while(dropedPackTasks.Count > 0)
        {
            packTasks.Enqueue(dropedPackTasks.Dequeue());
        }
        while(dropedPickTasks.Count > 0)
        {
            pickTasks.Enqueue(dropedPickTasks.Dequeue());
        }
    }

    public void DropTask(Task task)
    {
        switch(task.task.taskClass)
        {
            case TasksTypes.TaskClass.Build:
            {
                dropedBuildingTasks.Enqueue(task);
                return;
            }

            case TasksTypes.TaskClass.Pick:
            {
                dropedPickTasks.Enqueue(task);
                return;
            }

            case TasksTypes.TaskClass.Pack:
            {
                dropedPackTasks.Enqueue(task);
                return;
            }
        }
    }
    void ConvertOrdersToTasks()
    {
        for(int i = 0; i < ordersManager.ordersOnPick.Count; i++)
        {
            Task newPickTask = new Task(new TasksTypes.Task(TasksTypes.TaskClass.Pick),null,null,null, ordersManager.ordersOnPick[i],null);
            pickTasks.Enqueue(newPickTask);
        }

        ordersManager.ordersOnPick.Clear();
    }

    void MakeBuildingTask(Tile tile)
    {
        if(!IsTileCompatibleWithTask(tile))
            return;

        if(currentBuilding.cost > CashManager.instance.AmountOfCash())
        return;

        IndicatorsPool.Indicator indicator = MakeIndicator(tile);
        
        CashManager.instance.SpendCash(currentBuilding.cost);
        currentTask.tileTypeAfterTask = TileTypeAfterTask();        

        buildingTasks.Enqueue(new Task(currentTask, tile, currentBuilding, indicator, null, null));
    }

    void MakeBuildTask(Tile tile)
    {
        if(!IsTileCompatibleWithTask(tile))
            return;

        if(currentTask.cost > CashManager.instance.AmountOfCash())
        return;
        
        CashManager.instance.SpendCash(currentTask.cost);

        buildingTasks.Enqueue(new Task(currentTask, tile, null, null, null, null));
    }

    void GiveTasksToBuilders()
    {
        if (buildingTasks.Count == 0)
            return;

        if (freeBuilders.Count == 0)
            return;

        Queue<Task> unusedTasks = new Queue<Task>();
        while(buildingTasks.Count > 0)
        {
            Task givenTask = buildingTasks.Dequeue();

            Worker workerForTask = FindClosestWorker(givenTask.tileWithTask);

            if (workerForTask == null)
            {
                unusedTasks.Enqueue(givenTask);
                continue;
            }
                
            workerForTask.GetTask(givenTask);

            freeBuilders.Remove(workerForTask);

        }

        while(unusedTasks.Count > 0)
        {
            buildingTasks.Enqueue(unusedTasks.Dequeue());
        }
    }

    void GiveTasksToPick()
    {
        if (pickTasks.Count == 0)
            return;

        if (freePickWorkers.Count == 0)
            return;

        while(pickTasks.Count > 0)
        {
            if(freePickWorkers.Count == 0)
                break;

            Worker workerForTask = freePickWorkers.Dequeue();
                
            workerForTask.GetTask(pickTasks.Dequeue());
        }   
    }

    void GiveTasksToPack()
    {
        if (packTasks.Count == 0)
            return;

        if (freePackWorkers.Count == 0)
            return;

        while(packTasks.Count > 0)
        {
            if(freePackWorkers.Count == 0)
                break;

            Worker workerForTask = freePackWorkers.Dequeue();
                
            workerForTask.GetTask(packTasks.Dequeue());
        }

    }
    #endregion
    
    #region Check Methods

    TileTypes.TileType TileTypeAfterTask()
    {
        if(currentBuilding.buildingType == Buildings.BuildingType.Floor)
        {
            return TileTypes.TileType.Floor;
        }
        if(currentBuilding.buildingType == Buildings.BuildingType.Wall)
        {
            return TileTypes.TileType.Wall;
        }
        if(currentBuilding.buildingType == Buildings.BuildingType.Door)
        {
            return TileTypes.TileType.Door;
        }

        return TileTypes.TileType.Other;
    }

    void CheckBuildingsInput()
    {
        if(Input.GetKeyDown(KeyCode.Mouse1) || currentBuilding.buildingType == Buildings.BuildingType.None)
        {
            currentTile = null;
            lastIndicatorEndTile = null;
            ReturnIndicators();
            return;
        }

        if(!Input.GetKeyDown(KeyCode.Mouse0) || IsMouseOverUi() || currentBuilding.buildingType == Buildings.BuildingType.None)
            return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide))
        {
                
            Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

            if (tile == null)
                return;

            if(currentTile == null)
            {
                currentTile = tile;
            }
            else
            {
                if(currentBuilding.buildingType == Buildings.BuildingType.Wall)
                {
                   CheckForAmountAndTilesForBuildings(tile, false);
                } 
                else
                {
                    CheckForAmountAndTilesForBuildings(tile, true);
                }

                lastIndicatorEndTile = null;
                ReturnIndicators();
            }
                                
        }
    }

    void CheckBuildInput()
    {     
        if(Input.GetKeyDown(KeyCode.Mouse1) || currentTask.taskType == TasksTypes.TaskType.None || currentTask.taskType == TasksTypes.TaskType.Build)
        {
            currentTileBuild = null;
            return;
        }

        if(!Input.GetKeyDown(KeyCode.Mouse0) || IsMouseOverUi() || currentTask.taskType == TasksTypes.TaskType.None || currentTask.taskType == TasksTypes.TaskType.Build)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide))
        {
            Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

            if (tile == null)
                return;
            
            if(currentTileBuild == null)
            {
                currentTileBuild = tile;
            }
            else
            {
                CheckForAmountAndTilesForBuild(tile);
            }
        }
    }
    void CheckForInput()
    {
        CheckBuildingsInput();
        CheckBuildInput();
    }

    bool IsTileCompatibleWithTask(Tile tile)
    {
        switch(tile.tileType)
        {
            case TileTypes.TileType.Ground:
            {
                if(currentTask.taskType == TasksTypes.TaskType.Build && currentBuilding.buildingType == Buildings.BuildingType.Floor)
                    return true;

                return false;    
            }
            case TileTypes.TileType.Wall:
            {
                if(currentTask.taskType == TasksTypes.TaskType.Build && currentBuilding.buildingType == Buildings.BuildingType.Door)
                    return true;

                return false;
            }
            case TileTypes.TileType.Floor:
            {
                if(currentTask.taskType == TasksTypes.TaskType.Build && currentBuilding.buildingType != Buildings.BuildingType.Floor)
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
    #endregion
    
    #region Worker Methods
    public void ReturnWorker(Worker worker)
    {
        switch(worker.stats.workerType)
        {
            case WorkerData.WorkerType.Builder:
            {
                freeBuilders.Add(worker);
                break;
            }
            case WorkerData.WorkerType.Pick:
            {
                freePickWorkers.Enqueue(worker);
                break;
            }
            case WorkerData.WorkerType.Pack:
            {
                freePackWorkers.Enqueue(worker);
                break;
            }
        }
        
    }

    

    Worker FindClosestWorker(Tile endTile)
    {
        int currentClosestPath = MapGenerator.instance.GetAmountOfAllTiles();
        Worker currentClosestWorker = null;

        foreach(Worker worker in freeBuilders)
        {
            Tile[] path = PathFinder.instance.FindPath(worker.startNode, endTile);

            if(path == null)
                continue;

            if(path.Length < currentClosestPath)
            {
                currentClosestWorker = worker;
                currentClosestPath = path.Length;
            }
        }

        if (currentClosestWorker! != null)
            return currentClosestWorker;

        return null;
    }
    #endregion   
}

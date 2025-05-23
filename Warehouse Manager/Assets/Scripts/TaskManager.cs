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
    [SerializeField]
    Transform rotationTransform;
    #endregion

    #region Indicators Variables
    Tile lastIndicatorEndTile = null;
    Queue<IndicatorsPool.BuildingIndicator> buildingIndicators = new Queue<IndicatorsPool.BuildingIndicator>();
    Queue<IndicatorsPool.TaskIndicator> taskIndicators = new Queue<IndicatorsPool.TaskIndicator>();
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
        public Transform rotationTransform;
        public OrdersManager.Order order;
        public Tile tileOfPickStashWithOrder;
        public IndicatorsPool.BuildingIndicator buildingIndicator;
        public IndicatorsPool.TaskIndicator taskIndicator;
        public Task(TasksTypes.Task task, Tile tileWithTask, Buildings.Building building, Transform rotationTrasform,IndicatorsPool.BuildingIndicator buildingIndicator, IndicatorsPool.TaskIndicator taskIndicator, OrdersManager.Order order,Tile tileOfPickStashWithOrder)
        {
            this.task = task; 
            this.tileWithTask = tileWithTask;
            this.building = building;
            this.order = order;
            this.tileOfPickStashWithOrder = tileOfPickStashWithOrder;
            this.buildingIndicator = buildingIndicator;
            this.taskIndicator = taskIndicator;
            this.rotationTransform = rotationTrasform;
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
        SetBuildingIndicators();
        SetTaskIndicators();
        GiveTasksToBuilders();
        ConvertOrdersToTasks();
        GiveTasksToPick();
        GiveTasksToPack();
    }

    void FixedUpdate() 
    {
        CheckForIndicator();
    }

    void OnEnable()
    {
        Worker.OnBuildingEnded += FreeDropedTasks;
        SavingManager.OnSave += OnSave;
    }

    void OnDisable()
    {
        Worker.OnBuildingEnded -= FreeDropedTasks;
        SavingManager.OnSave -= OnSave;
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
    void CheckForIndicator()
    {
        if(IsMouseOverUi())
            return;

        CheckForBuildingIndicator();
        CheckForTaskIndicator();
    }

    void CheckForTaskIndicator()
    {
        if(currentTask.taskType == TasksTypes.TaskType.None || currentTask.taskType == TasksTypes.TaskType.Build || currentTileBuild != null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
        {
                
            Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

            if (tile == null || lastIndicatorEndTile == tile)
                return;

            lastIndicatorEndTile = tile;

            ReturnTaskIndicators();

            taskIndicators.Enqueue(MakeTaskIndicator(tile)); 
                                
        }
    }
    void CheckForBuildingIndicator()
    {
        if(currentBuilding.buildingType == Buildings.BuildingType.None || currentTile != null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
        {
                
            Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

            if (tile == null || lastIndicatorEndTile == tile)
                return;

            lastIndicatorEndTile = tile;

            ReturnBuildingIndicators();

            buildingIndicators.Enqueue(MakeBuildingIndicator(tile)); 
                                
        }
    }

    
    void SetBuildingIndicators()
    {
        if(currentTile == null || IsMouseOverUi() || currentBuilding.buildingType == Buildings.BuildingType.None)
            return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
        {
                
            Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

            if (tile == null || lastIndicatorEndTile == tile)
                return;

            lastIndicatorEndTile = tile;

            ReturnBuildingIndicators();

            if(currentBuilding.buildingType == Buildings.BuildingType.Wall)
            {
                MakeContourBuildingIndicators(tile); 
            }
            else
            {
                MakeFillBuildingIndicators(tile);         
            }  
                                
        }           
    }

    void SetTaskIndicators()
    {
        if(currentTileBuild == null || IsMouseOverUi() || currentTask.taskType == TasksTypes.TaskType.None || currentTask.taskType == TasksTypes.TaskType.Build)
            return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
        {
                
            Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

            if (tile == null || lastIndicatorEndTile == tile)
                return;

            lastIndicatorEndTile = tile;

            ReturnTaskIndicators();

            MakeFillTaskIndicators(tile);                     
        }           
    }

    void RotateIndicators()
    {
        for(int i = 0; i < buildingIndicators.Count; i++)
        {
            IndicatorsPool.BuildingIndicator indicator = buildingIndicators.Dequeue();
            indicator.indicatorObject.transform.rotation = rotationTransform.rotation;
            buildingIndicators.Enqueue(indicator);
        }

        for(int i = 0; i < taskIndicators.Count; i++)
        {
            IndicatorsPool.TaskIndicator indicator = taskIndicators.Dequeue();
            indicator.indicatorObject.transform.rotation = rotationTransform.rotation;
            taskIndicators.Enqueue(indicator);
        }
    }
    void ReturnBuildingIndicators()
    {
        while(buildingIndicators.Count > 0)
        {
            IndicatorsPool.instance.ReturnBuildingIndicator(buildingIndicators.Dequeue());
        }
    }

    void ReturnTaskIndicators()
    {
        while(taskIndicators.Count > 0)
        {
            IndicatorsPool.instance.ReturnTaskIndicator(taskIndicators.Dequeue());
        }
    }

    void MakeContourBuildingIndicators(Tile endTile)
    {
        List<Tile> tiles = Figuers.instance.MakeFiguer(currentTile, endTile, Figuers.FiguersType.Contour);

        for(int i = 0; i < tiles.Count; i++)
        {
            buildingIndicators.Enqueue(MakeBuildingIndicator(tiles[i]));
        }
    }

    void MakeFillBuildingIndicators(Tile endTile)
    {
        List<Tile> tiles = Figuers.instance.MakeFiguer(currentTile, endTile, Figuers.FiguersType.Fill);

        for(int i = 0; i < tiles.Count; i++)
        {
            buildingIndicators.Enqueue(MakeBuildingIndicator(tiles[i]));
        }
    }
    void MakeFillTaskIndicators(Tile endTile)
    {
        List<Tile> tiles = Figuers.instance.MakeFiguer(currentTileBuild, endTile, Figuers.FiguersType.Fill);

        for(int i = 0; i < tiles.Count; i++)
        {
            taskIndicators.Enqueue(MakeTaskIndicator(tiles[i]));
        }
    }

    IndicatorsPool.BuildingIndicator MakeBuildingIndicator(Tile tile)
    {
        IndicatorsPool.BuildingIndicator indicator = IndicatorsPool.instance.GetBuildingIndicator(tile.tileType, currentBuilding.buildingType);
        indicator.indicatorObject.transform.position = tile.transform.position;
        indicator.indicatorObject.transform.rotation = rotationTransform.rotation;
    
        return indicator;
    }

    IndicatorsPool.TaskIndicator MakeTaskIndicator(Tile tile)
    {
        IndicatorsPool.TaskIndicator indicator = IndicatorsPool.instance.GetTaskIndicator(tile.tileType, currentTask.taskType);
        indicator.indicatorObject.transform.position = tile.transform.position;
        indicator.indicatorObject.transform.rotation = rotationTransform.rotation;
    
        return indicator;
    }
    #endregion
    
    #region Building Methods

    void RotateBuilding(bool left)
    {
        if(left)
        {
            rotationTransform.Rotate(0, 90f, 0);
        }
        else
        {
            rotationTransform.Rotate(0, 90f, 0);
        }

        RotateIndicators();
    }
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

    void OnSave()
    {
        for (int i = 0; i < buildingTasks.Count; i++)
        {
            Task task = buildingTasks.Dequeue();

            SavingManager.instance.saveData.tasks.Add(new SaveData.TaskData(task.task.taskType.ToString(), task.task.taskClass.ToString(), task.tileWithTask.transform.position, new Vector3(), task.task.tileTypeAfterTask.ToString(), new List<Vector3>(), 0, new List<int>()));
        }

        for (int i = 0; i < pickTasks.Count; i++)
        {
            Task task = pickTasks.Dequeue();

            List<Vector3> racksWithItems = ConvertRacksToVector3(new List<Rack>(task.order.racksWithItems));

            SavingManager.instance.saveData.tasks.Add(new SaveData.TaskData(task.task.taskType.ToString(), task.task.taskClass.ToString(), new Vector3(), new Vector3(), null, racksWithItems, task.order.orderPrice, new List<int>(task.order.amountOfItemsFromRacks)));
        }

        for (int i = 0; i < packTasks.Count; i++)
        {
            Task task = packTasks.Dequeue();

            SavingManager.instance.saveData.tasks.Add(new SaveData.TaskData(task.task.taskType.ToString(), task.task.taskClass.ToString(), new Vector3(), task.tileOfPickStashWithOrder.transform.position, null, null, task.order.orderPrice, null));
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
            Task newPickTask = new Task(new TasksTypes.Task(TasksTypes.TaskClass.Pick),null,null,null,null,null, ordersManager.ordersOnPick[i],null);
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

        IndicatorsPool.BuildingIndicator indicator = MakeBuildingIndicator(tile);
        
        CashManager.instance.SpendCash(currentBuilding.cost);
        currentTask.tileTypeAfterTask = TileTypeAfterTask();   
        tile.haveTask = true;     

        buildingTasks.Enqueue(new Task(currentTask, tile, currentBuilding, rotationTransform, indicator,null, null, null));
    }

    void MakeBuildTask(Tile tile)
    {
        if(!IsTileCompatibleWithTask(tile))
            return;

        if(currentTask.cost > CashManager.instance.AmountOfCash())
        return;

        if(currentTask.taskType == TasksTypes.TaskType.Destroy)
        {
            if(tile.tileType == TileTypes.TileType.Floor)
            {
                currentTask.tileTypeAfterTask = TileTypes.TileType.Ground;
            }
            else
            {
                currentTask.tileTypeAfterTask = TileTypes.TileType.Floor;
            }
            
        }

        IndicatorsPool.TaskIndicator indicator = MakeTaskIndicator(tile);
        
        CashManager.instance.SpendCash(currentTask.cost);

        tile.haveTask = true;  

        buildingTasks.Enqueue(new Task(currentTask, tile, null, null, null, indicator, null, null));
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
        if(Input.GetKeyDown(KeyCode.Mouse1) || IsMouseOverUi() || currentBuilding.buildingType == Buildings.BuildingType.None)
        {
            currentTile = null;
            lastIndicatorEndTile = null;
            ReturnBuildingIndicators();
            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
            {

                Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

                if (tile == null)
                    return;

                currentTile = tile;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
            {

                Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

                if (tile == null || currentTile == null)
                    return;

                if (currentBuilding.buildingType == Buildings.BuildingType.Wall)
                {
                    CheckForAmountAndTilesForBuildings(tile, false);
                }
                else
                {
                    CheckForAmountAndTilesForBuildings(tile, true);
                }

                lastIndicatorEndTile = null;
                ReturnBuildingIndicators();
            }
        }
    }

    void CheckBuildInput()
    {     
        if(Input.GetKeyDown(KeyCode.Mouse1) || IsMouseOverUi() || currentTask.taskType == TasksTypes.TaskType.None || currentTask.taskType == TasksTypes.TaskType.Build)
        {
            currentTileBuild = null;
            lastIndicatorEndTile = null;
            ReturnTaskIndicators();
            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
            {
                Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

                if (tile == null)
                    return;

                currentTileBuild = tile;
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
            {
                Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

                if (tile == null || currentTileBuild == null)
                    return;

                CheckForAmountAndTilesForBuild(tile);

                lastIndicatorEndTile = null;
                ReturnTaskIndicators();               
            }
        }

        
    }

    void CheckForRotationInput()
    {
        if(currentBuilding.buildingType == Buildings.BuildingType.None)
            return;

        if(Input.GetKeyDown(KeyCode.Q))
        {
            RotateBuilding(false);
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            RotateBuilding(true);
        }
    }
    void CheckForInput()
    {
        CheckForRotationInput();
        CheckBuildingsInput();
        CheckBuildInput();
    }

    bool IsTileCompatibleWithTask(Tile tile)
    {
        if(tile == null)
            return false;
        
        if(currentTask.taskType == TasksTypes.TaskType.None)
            return false;

        if(tile.haveTask)
            return false;
        
        if(tile.building != null && currentTask.taskType == TasksTypes.TaskType.Destroy)
            return true;
            
        switch(tile.tileType)
        {
            case TileTypes.TileType.Ground:
            {
                if(currentTask.taskType == TasksTypes.TaskType.Build && currentBuilding.buildingType == Buildings.BuildingType.Floor)
                    return true;

                break;  
            }
            case TileTypes.TileType.Wall:
            {
                if(currentTask.taskType == TasksTypes.TaskType.Build && currentBuilding.buildingType == Buildings.BuildingType.Door)
                    return true;

                break; 
            }
            case TileTypes.TileType.Floor:
            {
                if(currentTask.taskType == TasksTypes.TaskType.Build && currentBuilding.buildingType != Buildings.BuildingType.Floor)
                    return true;

                break; 
            }
            case TileTypes.TileType.Water:
            {
                if(currentTask.taskType == TasksTypes.TaskType.Dry)
                    return true; 

                break;  
            }
            case TileTypes.TileType.Rocks:
            {
                if(currentTask.taskType == TasksTypes.TaskType.Mine)
                    return true;  

                break; 
            }
            case TileTypes.TileType.Tree:
            {
                if(currentTask.taskType == TasksTypes.TaskType.Chop)
                    return true;  

                break; 
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

    public void FireWorker(Worker worker)
    {
        switch(worker.stats.workerType)
        {
            case WorkerData.WorkerType.Builder:
                {
                    freeBuilders.Remove(worker);
                    return;
                }
            case WorkerData.WorkerType.Pick:
                {
                    Queue<Worker> checkedWorkers = new Queue<Worker>();
                    while(freePickWorkers.Count > 0)
                    {
                        Worker workerInCheck = freePickWorkers.Dequeue();
                        if(workerInCheck != worker)
                        {
                            checkedWorkers.Enqueue(workerInCheck);
                        }
                    }

                    freePickWorkers = checkedWorkers;
                    return;
                }
            case WorkerData.WorkerType.Pack:
                {
                    Queue<Worker> checkedWorkers = new Queue<Worker>();
                    while (freePackWorkers.Count > 0)
                    {
                        Worker workerInCheck = freePackWorkers.Dequeue();
                        if (workerInCheck != worker)
                        {
                            checkedWorkers.Enqueue(workerInCheck);
                        }
                    }

                    freePackWorkers = checkedWorkers;
                    return;
                }
        }
    }

    Worker FindClosestWorker(Tile endTile)
    {
        int currentClosestPath = MapGenerator.instance.GetAmountOfAllTiles();
        Worker currentClosestWorker = null;

        foreach(Worker worker in freeBuilders)
        {
            Queue<Tile> path = PathFinder.instance.FindPath(worker.startNode, endTile);

            if(path == null)
                continue;

            if(path.Count < currentClosestPath)
            {
                currentClosestWorker = worker;
                currentClosestPath = path.Count;
            }
        }

        if (currentClosestWorker! != null)
            return currentClosestWorker;

        return null;
    }
    #endregion   
}

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
    public HashSet<BuildingWorker> freeBuilders = new HashSet<BuildingWorker>();
    public Queue<PickWorker> freePickWorkers = new Queue<PickWorker>();
    public Queue<PackWorker> freePackWorkers = new Queue<PackWorker>();
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
        BuildingWorker.OnBuildingEnded += FreeDropedTasks;
        SavingManager.OnSave += OnSave;
        SavingManager.OnTasksLoad += LoadTasks;
    }

    void OnDisable()
    {
        BuildingWorker.OnBuildingEnded -= FreeDropedTasks;
        SavingManager.OnSave -= OnSave;
        SavingManager.OnTasksLoad -= LoadTasks;
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
    void LoadTasks()
    {
        for (int i = 0; i < SavingManager.instance.saveData.tasks.Count; i++)
        {
            SaveData.TaskData taskData = SavingManager.instance.saveData.tasks[i];
            TasksTypes.TaskClass taskClass = (TasksTypes.TaskClass)Enum.Parse(typeof(TasksTypes.TaskClass), taskData.taskClass);

            switch (taskClass)
            {
                case TasksTypes.TaskClass.Build:
                    {
                        LoadBuildingTask(taskData);
                        continue;
                    }

                case TasksTypes.TaskClass.Pick:
                    {
                        LoadPickTask(taskData);
                        continue;
                    }

                case TasksTypes.TaskClass.Pack:
                    {
                        LoadPackTask(taskData);
                        continue;
                    }
                
            }
        }
    }

    void LoadBuildingTask(SaveData.TaskData taskData)
    {
        Tile tileWithTask = FindTileByPosition(taskData.positionOfTileWithTask);
        TasksTypes.Task task = new TasksTypes.Task((TasksTypes.TaskClass)Enum.Parse(typeof(TasksTypes.TaskClass), taskData.taskClass));
        task.taskType = (TasksTypes.TaskType)Enum.Parse(typeof(TasksTypes.TaskType), taskData.type);
        task.tileTypeAfterTask = (TileTypes.TileType)Enum.Parse(typeof(TileTypes.TileType), taskData.tileTypeAfterTask);
        for (int i = 0; i < tasksTypes.tasks.Count; i++)
        {
            if (tasksTypes.tasks[i].taskType == task.taskType && tasksTypes.tasks[i].taskClass == task.taskClass)
            {
                task.cost = tasksTypes.tasks[i].cost;
                task.taskTime = tasksTypes.tasks[i].taskTime;
                break;
            }
        }

        Buildings.Building building = null;
        if (task.taskType == TasksTypes.TaskType.Build)
        {
            for (int i = 0; i < buildings.buildings.Count; i++)
            {
                if (buildings.buildings[i].buildingType.ToString() == taskData.buildingType)
                {
                    building = buildings.buildings[i];
                    break;
                }
            }
        }

        IndicatorScript indicatorScript = GetIndicatorScriptByPosition(tileWithTask.transform.position);
        IndicatorsPool.BuildingIndicator buildingIndicator = new IndicatorsPool.BuildingIndicator();
        IndicatorsPool.TaskIndicator taskIndicator = new IndicatorsPool.TaskIndicator();

        if (indicatorScript.buildingType != Buildings.BuildingType.None)
        {
            for (int i = 0; i < IndicatorsPool.instance.buildingIndicators.Count; i++)
            {
                if (IndicatorsPool.instance.buildingIndicators[i].buildingType == indicatorScript.buildingType)
                {
                    buildingIndicator.buildingType = indicatorScript.buildingType;
                    buildingIndicator.indicatorObject = indicatorScript.gameObject;
                    buildingIndicator.isAffirmative = indicatorScript.isAffirmative;
                    buildingIndicator.indicatorsInPool = new Queue<IndicatorsPool.BuildingIndicator>();
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < IndicatorsPool.instance.taskIndicators.Count; i++)
            {
                if (IndicatorsPool.instance.taskIndicators[i].taskType == task.taskType)
                {
                    taskIndicator.taskType = task.taskType;
                    taskIndicator.indicatorObject = indicatorScript.gameObject;
                    taskIndicator.isAffirmative = indicatorScript.isAffirmative;
                    taskIndicator.indicatorsInPool = new Queue<IndicatorsPool.TaskIndicator>();
                    break;
                }
            }
        }




        Task newBuildingTask = new Task(task, tileWithTask, building, indicatorScript.transform, buildingIndicator, taskIndicator, null, null);
        
        buildingTasks.Enqueue(newBuildingTask);
    }

    IndicatorScript GetIndicatorScriptByPosition(Vector3 position)
    {
        Ray ray = new Ray(position + Vector3.up * 100f, Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        foreach (RaycastHit hit in hits)
        {
            IndicatorScript indicatorScript = hit.collider.gameObject.GetComponent<IndicatorScript>();
            if (indicatorScript != null)
            {
                return indicatorScript;
            }
        }

        Debug.LogWarning("No indicator found at position: " + position);
        return null;
    }

    void LoadPickTask(SaveData.TaskData taskData)
    {
        Queue<Rack> racksWithItems = ConvertPositionsToRacks(taskData.positionOfRacksWithItems);
        OrdersManager.Order order = new OrdersManager.Order(racksWithItems, taskData.orderPrice, new Queue<int>(taskData.amountOfItemsFromRacks));
        Task newPickTask = new Task(new TasksTypes.Task(TasksTypes.TaskClass.Pick),null,null,null,null,null, order,null);
        pickTasks.Enqueue(newPickTask);
    }

    Queue<Rack> ConvertPositionsToRacks(List<Vector3> positions)
    {
        Queue<Rack> racks = new Queue<Rack>();

        for (int i = 0; i < positions.Count; i++)
        {
            Rack rack = FindRackByPosition(positions[i]);
            if (rack != null)
            {
                racks.Enqueue(rack);
            }
            else
            {
                Debug.LogWarning("Rack not found at position: " + positions[i]);
            }
        }

        return racks;
    }

    Rack FindRackByPosition(Vector3 position)
    {
        Ray ray = new Ray(position + Vector3.up * 100f, Vector3.down);
        float sphereRadius = 0.1f;
        float distance = 200f;

        RaycastHit[] hits = Physics.SphereCastAll(ray, sphereRadius, distance);

        foreach (RaycastHit hit in hits)
        {
            Debug.Log("HIT: " + hit.collider.name);
            Rack rack = hit.collider.gameObject.GetComponent<Rack>();
            if (rack != null)
            {
                return rack;
            }
        }
        return null;

    }

    void LoadPackTask(SaveData.TaskData taskData)
    { 
        OrdersManager.Order order = new OrdersManager.Order(new Queue<Rack>(), taskData.orderPrice, new Queue<int>(taskData.amountOfItemsFromRacks));
        Task newPackTask = new Task(new TasksTypes.Task(TasksTypes.TaskClass.Pack),null,null,null,null,null, order,FindTileByPosition(taskData.positionOfTileWithPickStah));
        packTasks.Enqueue(newPackTask);
    }
    
    Tile FindTileByPosition(Vector3 position)
    {
        Ray ray = new Ray(position + new Vector3(0f, 100f, 0f), Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        foreach (RaycastHit hit in hits)
        {
            Tile tile = hit.collider.gameObject.GetComponent<Tile>();
            if (tile != null)
            {
                return tile;
            }
        }
        return null;
    }

    void OnSave()
    {
        FreeDropedTasks();
        for (int i = 0; i < buildingTasks.Count; i++)
        {
            Task task = buildingTasks.Dequeue();

            SavingManager.instance.saveData.tasks.Add(new SaveData.TaskData(task.task.taskType.ToString(), task.task.taskClass.ToString(), task.tileWithTask.transform.position, new Vector3(), task.task.tileTypeAfterTask.ToString(), new List<Vector3>(), 0, new List<int>(),task.building.buildingType.ToString(),task.rotationTransform.rotation.eulerAngles));
        }

        for (int i = 0; i < pickTasks.Count; i++)
        {
            Task task = pickTasks.Dequeue();

            List<Vector3> racksWithItems = ConvertRacksToVector3(new List<Rack>(task.order.racksWithItems));

            SavingManager.instance.saveData.tasks.Add(new SaveData.TaskData(task.task.taskType.ToString(), task.task.taskClass.ToString(), new Vector3(), new Vector3(), null, racksWithItems, task.order.orderPrice, new List<int>(task.order.amountOfItemsFromRacks),null, Vector3.zero));
        }

        for (int i = 0; i < packTasks.Count; i++)
        {
            Task task = packTasks.Dequeue();

            SavingManager.instance.saveData.tasks.Add(new SaveData.TaskData(task.task.taskType.ToString(), task.task.taskClass.ToString(), new Vector3(), task.tileOfPickStashWithOrder.transform.position, null, null, task.order.orderPrice, null,null,Vector3.zero));
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
        Debug.Log($"DropTask: task={task}, task.task={task?.task}, tileWithTask={task?.tileWithTask}, building={task?.building}");
        if (task == null || task.task == null || task.tileWithTask == null)
        {
            Debug.LogError("DropTask received a null field!");
            return;
        }

        

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

        if (currentBuilding.cost > CashManager.instance.AmountOfCash())
        {
            NotificationsManager.instance.ShowNotification(NotificationsData.NotificationType.NotEnoughCash);
            return;
        }
        

        IndicatorsPool.BuildingIndicator indicator = MakeBuildingIndicator(tile);
        
        CashManager.instance.SpendCash(currentBuilding.cost);
        currentTask.tileTypeAfterTask = TileTypeAfterTask();   
        tile.haveTask = true;

        // Clone the currentTask to avoid shared reference bugs
        TasksTypes.Task taskCopy = new TasksTypes.Task(currentTask.taskClass)
        {
            taskTime = currentTask.taskTime,
            cost = currentTask.cost,
            taskType = currentTask.taskType,
            taskClass = currentTask.taskClass,
            buttonSprite = currentTask.buttonSprite,
            nameOfButton = currentTask.nameOfButton,
            tileTypeAfterTask = TileTypeAfterTask()
        };    

    // Store a copy of the building type, not the reference
        Buildings.Building buildingCopy = new Buildings.Building()
        {
            buildingType = currentBuilding.buildingType,
            buildingSprite = currentBuilding.buildingSprite,
            nameOfButton = currentBuilding.nameOfButton,
            buildingTime = currentBuilding.buildingTime,
            cost = currentBuilding.cost
        };     

        buildingTasks.Enqueue(new Task(taskCopy, tile, buildingCopy, rotationTransform, indicator,null, null, null));
    }

    void MakeBuildTask(Tile tile)
    {
        if(!IsTileCompatibleWithTask(tile))
            return;

        if(currentTask.cost > CashManager.instance.AmountOfCash())
        {
            NotificationsManager.instance.ShowNotification(NotificationsData.NotificationType.NotEnoughCash);
            return;
        }
        

        if (currentTask.taskType == TasksTypes.TaskType.Destroy)
            {
                if (tile.tileType == TileTypes.TileType.Floor)
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

        // Clone the currentTask to avoid shared reference bugs
        TasksTypes.Task taskCopy = new TasksTypes.Task(currentTask.taskClass)
        {
            taskTime = currentTask.taskTime,
            cost = currentTask.cost,
            taskType = currentTask.taskType,
            taskClass = currentTask.taskClass,
            buttonSprite = currentTask.buttonSprite,
            nameOfButton = currentTask.nameOfButton,
            tileTypeAfterTask = TileTypeAfterTask()
        };     

        buildingTasks.Enqueue(new Task(taskCopy, tile, null, null, null, indicator, null, null));
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

            BuildingWorker workerForTask = FindClosestWorker(givenTask.tileWithTask);

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

            WorkerBase workerForTask = freePickWorkers.Dequeue();

            Task task = pickTasks.Dequeue();

            Task copyOfTask = new Task(task.task, task.tileWithTask, task.building, task.rotationTransform, task.buildingIndicator, task.taskIndicator, task.order, task.tileOfPickStashWithOrder);
                
            workerForTask.GetTask(copyOfTask);
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

            WorkerBase workerForTask = freePackWorkers.Dequeue();

            Task task = packTasks.Dequeue();

            Task copyOfTask = new Task(task.task, task.tileWithTask, task.building, task.rotationTransform, task.buildingIndicator, task.taskIndicator, task.order, task.tileOfPickStashWithOrder);
                
            workerForTask.GetTask(copyOfTask);
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

        Debug.LogWarning("Unexpected building type: " + currentBuilding.buildingType);
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
    public void ReturnWorker(WorkerBase worker)
    {
        switch (worker.stats.workerType)
        {
            case WorkerData.WorkerType.Builder:
                {
                    BuildingWorker buildingWorker = worker.GetComponent<BuildingWorker>();
                    freeBuilders.Add(buildingWorker);
                    break;
                }
            case WorkerData.WorkerType.Pick:
                {
                    PickWorker pickWorker = worker.GetComponent<PickWorker>();
                    freePickWorkers.Enqueue(pickWorker);
                    break;
                }
            case WorkerData.WorkerType.Pack:
                {
                    PackWorker packWorker = worker.GetComponent<PackWorker>();
                    freePackWorkers.Enqueue(packWorker);
                    break;
                }
        }
        
    }

    public void FireWorker(WorkerBase worker)
    {
        switch(worker.stats.workerType)
        {
            case WorkerData.WorkerType.Builder:
                {
                    BuildingWorker buildingWorker = worker.GetComponent<BuildingWorker>();
                    freeBuilders.Remove(buildingWorker);
                    return;
                }
            case WorkerData.WorkerType.Pick:
                {

                    Queue<PickWorker> checkedWorkers = new Queue<PickWorker>();
                    while(freePickWorkers.Count > 0)
                    {
                        PickWorker workerInCheck = freePickWorkers.Dequeue();
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
                    Queue<PackWorker> checkedWorkers = new Queue<PackWorker>();
                    while (freePackWorkers.Count > 0)
                    {
                        PackWorker workerInCheck = freePackWorkers.Dequeue();
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

    BuildingWorker FindClosestWorker(Tile endTile)
    {
        int currentClosestPath = MapGenerator.instance.GetAmountOfAllTiles();
        WorkerBase currentClosestWorker = null;

        foreach(WorkerBase worker in freeBuilders)
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
        {
            BuildingWorker worker = currentClosestWorker.GetComponent<BuildingWorker>();
            return worker;
        }
            

        return null;
    }
    #endregion   
}

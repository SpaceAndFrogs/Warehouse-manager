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

    [SerializeField]
    public List<Task> pickTasks = new List<Task>();
    public List<Task> packTasks = new List<Task>();
    public OrdersManager ordersManager;

    public TasksTypes.Task currentTask;
    [SerializeField]
    Tile currentTile = null;
    Tile lastIndicatorEndTile = null;

    List<IndicatorsPool.Indicator> indicators = new List<IndicatorsPool.Indicator>();

    public Buildings.Building currentBuilding = null;

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

    void SetIndicators()
    {
        if(currentTile == null || IsMouseOverUi() || currentBuilding.buildingType == Buildings.BuildingType.None)
            return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
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

    void MakeContourIndicators(Tile endTile)
    {
        Tile currentTileInCheck = currentTile;
        indicators.Add(MakeIndicator(currentTileInCheck));
        while(endTile.transform.position.x != currentTileInCheck.transform.position.x)
        {
            if(endTile.transform.position.x > currentTileInCheck.transform.position.x)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, true, true);
                indicators.Add(MakeIndicator(currentTileInCheck));
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, true, false);
                indicators.Add(MakeIndicator(currentTileInCheck));
            }
        }

        while(endTile.transform.position.z != currentTileInCheck.transform.position.z)
        {
            if(endTile.transform.position.z > currentTileInCheck.transform.position.z)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, true);
                indicators.Add(MakeIndicator(currentTileInCheck));
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, false);
                indicators.Add(MakeIndicator(currentTileInCheck));
            }
        }

        currentTileInCheck = currentTile;
        while(endTile.transform.position.z != currentTileInCheck.transform.position.z)
        {
            if(endTile.transform.position.z > currentTileInCheck.transform.position.z)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, true);
                indicators.Add(MakeIndicator(currentTileInCheck));
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, false);
                indicators.Add(MakeIndicator(currentTileInCheck));
            }
        }

        while(endTile.transform.position.x != currentTileInCheck.transform.position.x)
        {
            if(endTile.transform.position.x > currentTileInCheck.transform.position.x)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, true, true);
                indicators.Add(MakeIndicator(currentTileInCheck));
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, true, false);
                indicators.Add(MakeIndicator(currentTileInCheck));
            }
        }
    }

    void MakeFillIndicators(Tile endTile)
    {
        Tile currentTileInCheck = currentTile;
        List<Tile> tilesInZAxis = new List<Tile>();
        tilesInZAxis.Add(currentTileInCheck);
        while(endTile.transform.position.z != currentTileInCheck.transform.position.z)
        {
            if(endTile.transform.position.z > currentTileInCheck.transform.position.z)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, true);
                tilesInZAxis.Add(currentTileInCheck);
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, false);
                tilesInZAxis.Add(currentTileInCheck);
            }
        }

        

        for(int i = 0; i < tilesInZAxis.Count; i++)
        {
            currentTileInCheck = tilesInZAxis[i];
            indicators.Add(MakeIndicator(currentTileInCheck));

            while(endTile.transform.position.x != currentTileInCheck.transform.position.x)
            {
                if(endTile.transform.position.x > currentTileInCheck.transform.position.x)
                {
                    currentTileInCheck = CheckForNextTile(currentTileInCheck, true, true);
                    indicators.Add(MakeIndicator(currentTileInCheck));
                }
                else
                {
                    currentTileInCheck = CheckForNextTile(currentTileInCheck, true, false);
                    indicators.Add(MakeIndicator(currentTileInCheck));
                }
            }
        }
    }

    void ConvertOrdersToTasks()
    {
        for(int i = 0; i < ordersManager.ordersOnPick.Count; i++)
        {
            Task newPickTask = new Task(new TasksTypes.Task(TasksTypes.TaskClass.Pick),null,null,null, ordersManager.ordersOnPick[i],null);
            pickTasks.Add(newPickTask);
        }

        ordersManager.ordersOnPick.Clear();
    }

    bool IsMouseOverUi()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    void CheckForInput()
    {
        if(!Input.GetKeyDown(KeyCode.Mouse0) || IsMouseOverUi() || currentBuilding.buildingType == Buildings.BuildingType.None)
            return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
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

    void ReturnIndicators()
    {
        for(int i = 0; i < IndicatorsPool.instance.indicators.Count; i++)
        {
            if(IndicatorsPool.instance.indicators[i].buildingType == currentBuilding.buildingType)
            {
                for(int j = indicators.Count - 1; j >= 0; j--)
                {
                IndicatorsPool.instance.indicators[i].ReturnIndicator(indicators[j]);
                indicators.RemoveAt(j);
                }

                return;
            }
        }
    }

    Tile CheckForNextTile(Tile startTile, bool checkX, bool checkMore)
    {
        Tile tile = null;

        for(int i = 0; i < startTile.neighborTiles.Count; i++)
        {
            if(checkX)
            {
                if(checkMore)
                {
                    if(startTile.transform.position.x < startTile.neighborTiles[i].transform.position.x && startTile.transform.position.z == startTile.neighborTiles[i].transform.position.z)
                    {
                        tile = startTile.neighborTiles[i];
                        break;
                    }
                }
                else
                {
                    if(startTile.transform.position.x > startTile.neighborTiles[i].transform.position.x && startTile.transform.position.z == startTile.neighborTiles[i].transform.position.z)
                    {
                        tile = startTile.neighborTiles[i];
                        break;
                    }
                }
            }
            else
            {
                if(checkMore)
                {
                    if(startTile.transform.position.z < startTile.neighborTiles[i].transform.position.z && startTile.transform.position.x == startTile.neighborTiles[i].transform.position.x)
                    {
                        tile = startTile.neighborTiles[i];
                        break;
                    }
                }
                else
                {
                    if(startTile.transform.position.z > startTile.neighborTiles[i].transform.position.z && startTile.transform.position.x == startTile.neighborTiles[i].transform.position.x)
                    {
                        tile = startTile.neighborTiles[i];
                        break;
                    }
                }
            }
        }

        return tile;
    }

    void Contour(Tile endTile)
    {
        MakeBuildingTask(currentTile);
        Tile currentTileInCheck = currentTile;
        while(endTile.transform.position.x != currentTileInCheck.transform.position.x)
        {
            if(endTile.transform.position.x > currentTileInCheck.transform.position.x)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, true, true);
                MakeBuildingTask(currentTileInCheck);
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, true, false);
                MakeBuildingTask(currentTileInCheck);
            }
        }

        while(endTile.transform.position.z != currentTileInCheck.transform.position.z)
        {
            if(endTile.transform.position.z > currentTileInCheck.transform.position.z)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, true);
                MakeBuildingTask(currentTileInCheck);
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, false);
                MakeBuildingTask(currentTileInCheck);
            }
        }

        currentTileInCheck = currentTile;
        while(endTile.transform.position.z != currentTileInCheck.transform.position.z)
        {
            if(endTile.transform.position.z > currentTileInCheck.transform.position.z)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, true);
                MakeBuildingTask(currentTileInCheck);
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, false);
                MakeBuildingTask(currentTileInCheck);
            }
        }

        while(endTile.transform.position.x != currentTileInCheck.transform.position.x)
        {
            if(endTile.transform.position.x > currentTileInCheck.transform.position.x)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, true, true);
                MakeBuildingTask(currentTileInCheck);
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, true, false);
                MakeBuildingTask(currentTileInCheck);
            }
        }
    }

    void Fill(Tile endTile)
    {
        Tile currentTileInCheck = currentTile;
        List<Tile> tilesInZAxis = new List<Tile>();
        tilesInZAxis.Add(currentTileInCheck);
        while(endTile.transform.position.z != currentTileInCheck.transform.position.z)
        {
            if(endTile.transform.position.z > currentTileInCheck.transform.position.z)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, true);
                tilesInZAxis.Add(currentTileInCheck);
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, false);
                tilesInZAxis.Add(currentTileInCheck);
            }
        }

        

        for(int i = 0; i < tilesInZAxis.Count; i++)
        {
            currentTileInCheck = tilesInZAxis[i];
            MakeBuildingTask(currentTileInCheck);

            do{
                if(endTile.transform.position.x > currentTileInCheck.transform.position.x)
                {
                    currentTileInCheck = CheckForNextTile(currentTileInCheck, true, true);
                    MakeBuildingTask(currentTileInCheck);
                }
                else
                {
                    currentTileInCheck = CheckForNextTile(currentTileInCheck, true, false);
                    MakeBuildingTask(currentTileInCheck);
                }
            }while(endTile.transform.position.x != currentTileInCheck.transform.position.x);
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

    IndicatorsPool.Indicator MakeIndicator(Tile tile)
    {
        IndicatorsPool.Indicator indicator = null;
        for(int i = 0; i < IndicatorsPool.instance.indicators.Count; i++)
        {
            if(IndicatorsPool.instance.indicators[i].buildingType == currentBuilding.buildingType)
            {
                indicator = IndicatorsPool.instance.indicators[i].GetIndicator();
                indicator.indicatorObject.transform.position = tile.transform.position;
                break;
            }
        }

        return indicator;
    }

    void MakeBuildingTask(Tile tile)
    {
        if(!IsTileCompatibleWithTask(tile))
            return;

        if(currentBuilding.cost > CashManager.instance.AmountOfCash())
            return;

        IndicatorsPool.Indicator indicator = MakeIndicator(tile);
        
        CashManager.instance.SpendCash(currentBuilding.cost);

        buildingTasks.Add(new Task(currentTask, tile, currentBuilding, indicator, null, null));
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
        switch(worker.stats.workerType)
        {
            case WorkerData.WorkerType.Builder:
            {
                freeBuilders.Add(worker);
                break;
            }
            case WorkerData.WorkerType.Pick:
            {
                freePickWorkers.Add(worker);
                break;
            }
            case WorkerData.WorkerType.Pack:
            {
                freePackWorkers.Add(worker);
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
        if (pickTasks.Count == 0)
            return;

        if (freePickWorkers.Count == 0)
            return;

        List<Task> givenPickTasks = new List<Task>();

        for(int i = 0; i < pickTasks.Count; i++)
        {
            if(freePickWorkers.Count == 0)
                break;
                
            Worker workerForTask = freePickWorkers[0];

            workerForTask.GetTask(pickTasks[i]);

            givenPickTasks.Add(pickTasks[i]);

            freePickWorkers.RemoveAt(0);
        }

        for(int i = 0; i < givenPickTasks.Count; i++)
        {
            pickTasks.Remove(givenPickTasks[i]);
        }
    }

    void GiveTasksToPack()
    {
        if (packTasks.Count == 0)
            return;

        if (freePackWorkers.Count == 0)
            return;

        List<Task> givenPackTasks = new List<Task>();

        for(int i = 0; i < packTasks.Count; i++)
        {
            if(freePackWorkers.Count == 0)
            {
                break;
            }
            
            
            

            Worker workerForTask = freePackWorkers[0];

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

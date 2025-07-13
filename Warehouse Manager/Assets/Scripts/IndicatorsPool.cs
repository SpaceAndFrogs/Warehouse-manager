using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IndicatorsPool : MonoBehaviour
{
    public static IndicatorsPool instance;
    #nullable enable
    public static event Action<WorkerSpawnerIndicator, Tile>? OnWorkerSpawnerIndicatorsLoad; 
    #nullable disable


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }
    #region TasksIndicators
    public List<TaskIndicator> taskIndicators = new List<TaskIndicator>();
    public void ReturnTaskIndicator(TaskIndicator indicator)
    {
        for (int i = 0; i < instance.taskIndicators.Count; i++)
        {
            if (instance.taskIndicators[i].taskType == indicator.taskType)
            {
                if (instance.taskIndicators[i].isAffirmative == indicator.isAffirmative)
                {
                    instance.taskIndicators[i].ReturnIndicator(indicator);
                    return;
                }
            }
        }
    }

    public TaskIndicator GetTaskIndicator(TileTypes.TileType tileType, TasksTypes.TaskType taskType)
    {

        for (int i = 0; i < instance.taskIndicators.Count; i++)
        {
            if (instance.taskIndicators[i].taskType == taskType)
            {
                if (taskType == TasksTypes.TaskType.Chop)
                {
                    if (tileType == TileTypes.TileType.Tree)
                    {
                        if (instance.taskIndicators[i].isAffirmative)
                            return instance.taskIndicators[i].GetIndicator();
                    }
                    else
                    {
                        if (!instance.taskIndicators[i].isAffirmative)
                            return instance.taskIndicators[i].GetIndicator();
                    }
                }

                if (taskType == TasksTypes.TaskType.Mine)
                {
                    if (tileType == TileTypes.TileType.Rocks)
                    {
                        if (instance.taskIndicators[i].isAffirmative)
                            return instance.taskIndicators[i].GetIndicator();
                    }
                    else
                    {
                        if (!instance.taskIndicators[i].isAffirmative)
                            return instance.taskIndicators[i].GetIndicator();
                    }
                }

                if (taskType == TasksTypes.TaskType.Dry)
                {
                    if (tileType == TileTypes.TileType.Water)
                    {
                        if (instance.taskIndicators[i].isAffirmative)
                            return instance.taskIndicators[i].GetIndicator();
                    }
                    else
                    {
                        if (!instance.taskIndicators[i].isAffirmative)
                            return instance.taskIndicators[i].GetIndicator();
                    }
                }

                if (taskType == TasksTypes.TaskType.Destroy)
                {
                    if (tileType == TileTypes.TileType.Floor || tileType == TileTypes.TileType.Wall || tileType == TileTypes.TileType.Door || tileType == TileTypes.TileType.Other)
                    {
                        if (instance.taskIndicators[i].isAffirmative)
                            return instance.taskIndicators[i].GetIndicator();
                    }
                    else
                    {
                        if (!instance.taskIndicators[i].isAffirmative)
                            return instance.taskIndicators[i].GetIndicator();
                    }
                }


            }
        }

        return null;
    }

    [System.Serializable]
    public class TaskIndicator

    {
        public TasksTypes.TaskType taskType;
        public GameObject indicatorObject;
        public bool isAffirmative;
        public Queue<TaskIndicator> indicatorsInPool = new Queue<TaskIndicator>();

        public TaskIndicator(TasksTypes.TaskType taskType, GameObject indicatorObject, bool isAffirmative)
        {
            this.taskType = taskType;
            this.indicatorObject = indicatorObject;
            this.indicatorsInPool = new Queue<TaskIndicator>();
            this.isAffirmative = isAffirmative;
        }

        public TaskIndicator()
        {
            
        }

        public TaskIndicator GetIndicator()
        {
            TaskIndicator indicator = null;

            if (indicatorsInPool == null)
            {
                indicatorsInPool = new Queue<TaskIndicator>();
            }

            if (indicatorsInPool.Count == 0)
            {
                SpawnIndicator();
            }

            indicator = indicatorsInPool.Dequeue();

            return indicator;
        }

        void SpawnIndicator()
        {
            GameObject spawnedIndicatorObject = Instantiate(indicatorObject, instance.transform.position, instance.transform.rotation);
            TaskIndicator indicator = new TaskIndicator(this.taskType, spawnedIndicatorObject, this.isAffirmative);
            indicatorsInPool.Enqueue(indicator);
        }

        public void ReturnIndicator(TaskIndicator indicator)
        {
            indicator.indicatorObject.transform.position = instance.transform.position;
            indicatorsInPool.Enqueue(indicator);
        }
    }
    #endregion

    #region Building Indicators
    public List<BuildingIndicator> buildingIndicators = new List<BuildingIndicator>();
    public void ReturnBuildingIndicator(BuildingIndicator indicator)
    {
        for (int i = 0; i < instance.buildingIndicators.Count; i++)
        {
            if (instance.buildingIndicators[i].buildingType == indicator.buildingType)
            {
                if (instance.buildingIndicators[i].isAffirmative == indicator.isAffirmative)
                {
                    instance.buildingIndicators[i].ReturnIndicator(indicator);
                    return;
                }
            }
        }
    }

    public BuildingIndicator GetBuildingIndicator(TileTypes.TileType tileType, Buildings.BuildingType buildingType)
    {

        for (int i = 0; i < instance.buildingIndicators.Count; i++)
        {
            if (instance.buildingIndicators[i].buildingType == buildingType)
            {
                if (buildingType == Buildings.BuildingType.Floor)
                {
                    if (tileType == TileTypes.TileType.Ground)
                    {
                        if (instance.buildingIndicators[i].isAffirmative)
                            return instance.buildingIndicators[i].GetIndicator();
                    }
                    else
                    {
                        if (!instance.buildingIndicators[i].isAffirmative)
                            return instance.buildingIndicators[i].GetIndicator();
                    }
                }

                if (buildingType == Buildings.BuildingType.Door)
                {
                    if (tileType == TileTypes.TileType.Wall)
                    {
                        if (instance.buildingIndicators[i].isAffirmative)
                            return instance.buildingIndicators[i].GetIndicator();
                    }
                    else
                    {
                        if (!instance.buildingIndicators[i].isAffirmative)
                            return instance.buildingIndicators[i].GetIndicator();
                    }
                }

                if (buildingType != Buildings.BuildingType.Floor && buildingType != Buildings.BuildingType.Door)
                {
                    if (tileType == TileTypes.TileType.Floor)
                    {
                        if (instance.buildingIndicators[i].isAffirmative)
                            return instance.buildingIndicators[i].GetIndicator();
                    }
                    else
                    {
                        if (!instance.buildingIndicators[i].isAffirmative)
                            return instance.buildingIndicators[i].GetIndicator();
                    }
                }


            }
        }

        return null;
    }

    [System.Serializable]
    public class BuildingIndicator

    {
        public Buildings.BuildingType buildingType;
        public GameObject indicatorObject;
        public bool isAffirmative;
        public Queue<BuildingIndicator> indicatorsInPool = new Queue<BuildingIndicator>();

        public BuildingIndicator(Buildings.BuildingType buildingType, GameObject indicatorObject, bool isAffirmative)
        {
            this.buildingType = buildingType;
            this.indicatorObject = indicatorObject;
            this.indicatorsInPool = new Queue<BuildingIndicator>();
            this.isAffirmative = isAffirmative;
        }

        public BuildingIndicator()
        {

        }


        public BuildingIndicator GetIndicator()
        {
            BuildingIndicator indicator = null;

            if (indicatorsInPool == null)
            {
                indicatorsInPool = new Queue<BuildingIndicator>();
            }

            if (indicatorsInPool.Count == 0)
            {
                SpawnIndicator();
            }

            indicator = indicatorsInPool.Dequeue();

            return indicator;
        }

        void SpawnIndicator()
        {
            GameObject spawnedIndicatorObject = Instantiate(indicatorObject, instance.transform.position, instance.transform.rotation);
            BuildingIndicator indicator = new BuildingIndicator(this.buildingType, spawnedIndicatorObject, this.isAffirmative);
            indicatorsInPool.Enqueue(indicator);
        }

        public void ReturnIndicator(BuildingIndicator indicator)
        {
            indicator.indicatorObject.transform.position = instance.transform.position;
            indicatorsInPool.Enqueue(indicator);
        }
    }
    #endregion

    #region WorkerSpawnerIndicators
    public WorkerSpawnerIndicator workerSpawnerIndicators;
    [System.Serializable]
    public class WorkerSpawnerIndicator
    {
        public bool isAffirmative;
        public GameObject affirmativeIndicatorObject;
        public GameObject negativeIndicatorObject;
        public Queue<WorkerSpawnerIndicator> affirmativeIndicatorsInPool = new Queue<WorkerSpawnerIndicator>();
        public Queue<WorkerSpawnerIndicator> negativeIndicatorsInPool = new Queue<WorkerSpawnerIndicator>();

        public WorkerSpawnerIndicator(GameObject affirmativeIndicatorObject, GameObject negativeIndicatorObject, bool isAffirmative)
        {
            this.affirmativeIndicatorObject = affirmativeIndicatorObject;
            this.negativeIndicatorObject = negativeIndicatorObject;
            this.affirmativeIndicatorsInPool = new Queue<WorkerSpawnerIndicator>();
            this.negativeIndicatorsInPool = new Queue<WorkerSpawnerIndicator>();
            this.isAffirmative = isAffirmative;
        }


        public WorkerSpawnerIndicator GetIndicator(TileTypes.TileType tileType)
        {
            WorkerSpawnerIndicator indicator = null;

            if (tileType == TileTypes.TileType.Ground || tileType == TileTypes.TileType.Floor)
            {
                if (affirmativeIndicatorsInPool == null)
                {
                    affirmativeIndicatorsInPool = new Queue<WorkerSpawnerIndicator>();
                }

                if (affirmativeIndicatorsInPool.Count == 0)
                {
                    SpawnIndicator(true);
                }

                indicator = affirmativeIndicatorsInPool.Dequeue();
            }
            else
            {
                if (negativeIndicatorsInPool == null)
                {
                    negativeIndicatorsInPool = new Queue<WorkerSpawnerIndicator>();
                }

                if (negativeIndicatorsInPool.Count == 0)
                {
                    SpawnIndicator(false);
                }

                indicator = negativeIndicatorsInPool.Dequeue();
            }


            return indicator;
        }

        void SpawnIndicator(bool affirmative)
        {
            GameObject spawnedIndicatorObject = null;
            if (affirmative)
            {
                spawnedIndicatorObject = Instantiate(affirmativeIndicatorObject, instance.transform.position, instance.transform.rotation);
                WorkerSpawnerIndicator indicator = new WorkerSpawnerIndicator(spawnedIndicatorObject, null, true);
                affirmativeIndicatorsInPool.Enqueue(indicator);
            }
            else
            {
                spawnedIndicatorObject = Instantiate(negativeIndicatorObject, instance.transform.position, instance.transform.rotation);
                WorkerSpawnerIndicator indicator = new WorkerSpawnerIndicator(null, spawnedIndicatorObject, false);
                negativeIndicatorsInPool.Enqueue(indicator);
            }

        }

        public void ReturnIndicator(WorkerSpawnerIndicator indicator)
        {
            if (indicator.isAffirmative)
            {
                indicator.affirmativeIndicatorObject.transform.position = instance.transform.position;
                affirmativeIndicatorsInPool.Enqueue(indicator);
            }
            else
            {
                indicator.negativeIndicatorObject.transform.position = instance.transform.position;
                negativeIndicatorsInPool.Enqueue(indicator);
            }

        }
    }
    #endregion

    void OnEnable()
    {
        SavingManager.OnIndicatorsLoad += LoadIndicators;
    }

    void OnDisable()
    {
        SavingManager.OnIndicatorsLoad -= LoadIndicators;
    }

    void LoadIndicators()
    {
        LoadWorkerSpawnerIndicators();
        LoadTaskIndicators();
        LoadBuildingIndicators();
    }

    void LoadWorkerSpawnerIndicators()
    {
        if (SavingManager.instance.saveData.workerSpawnerIndicators.Count == 0)
        {
            return;
        }

        foreach (var indicatorData in SavingManager.instance.saveData.workerSpawnerIndicators)
        {
            Tile tile = GetTileFromPosition(indicatorData.position);
            WorkerSpawnerIndicator indicator = workerSpawnerIndicators.GetIndicator(tile.tileType);
            indicator.isAffirmative = indicatorData.isAffirmative;
            indicator.affirmativeIndicatorObject.transform.position = indicatorData.position;
            indicator.affirmativeIndicatorObject.transform.rotation = indicatorData.rotation;

            OnWorkerSpawnerIndicatorsLoad?.Invoke(indicator, tile);
        }
        

    }

    void LoadTaskIndicators()
    {
        if(SavingManager.instance.saveData.taskIndicators.Count == 0)
        {
            return;
        }

        foreach (var indicatorData in SavingManager.instance.saveData.taskIndicators)
        {
            Tile tile = GetTileFromPosition(indicatorData.position);
            TaskIndicator indicator = GetTaskIndicator(tile.tileType, (TasksTypes.TaskType)Enum.Parse(typeof(TasksTypes.TaskType), indicatorData.type));
            indicator.isAffirmative = indicatorData.isAffirmative;
            indicator.indicatorObject.transform.position = indicatorData.position;
            indicator.indicatorObject.transform.rotation = indicatorData.rotation;
        }
    }

    void LoadBuildingIndicators()
    {
        if (SavingManager.instance.saveData.buildingIndicators.Count == 0)
        {
            return;
        }
        
        foreach (var indicatorData in SavingManager.instance.saveData.buildingIndicators)
        {
            Tile tile = GetTileFromPosition(indicatorData.position);
            if( tile == null)
            {
                Debug.LogWarning("Tile not found for position: " + indicatorData.position);
                continue;
            }
            BuildingIndicator indicator = GetBuildingIndicator(tile.tileType, (Buildings.BuildingType)Enum.Parse(typeof(Buildings.BuildingType), indicatorData.type));
            if (indicator == null)
            {
                Debug.LogWarning("Building indicator not found for type: " + indicatorData.type);
                continue;
            }
            indicator.isAffirmative = indicatorData.isAffirmative;
            indicator.indicatorObject.transform.position = indicatorData.position;
            indicator.indicatorObject.transform.rotation = indicatorData.rotation;
        }
    }
    
    Tile GetTileFromPosition(Vector3 position)
    {
        Ray ray = new Ray(position+new Vector3(0f,100f,0f), Vector3.down);
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
}

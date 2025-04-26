using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorsPool : MonoBehaviour
{
    public static IndicatorsPool instance;
    

    void Awake() 
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    #region Building Indicators
    public List<BuildingIndicator> buildingIndicators = new List<BuildingIndicator>();
    public void ReturnBuildingIndicator(BuildingIndicator indicator)
    {
        for(int i = 0; i < instance.buildingIndicators.Count; i++)
        {
            if(instance.buildingIndicators[i].buildingType == indicator.buildingType)
            {
                if(instance.buildingIndicators[i].isAffirmative == indicator.isAffirmative)
                {
                    instance.buildingIndicators[i].ReturnIndicator(indicator);
                    return;
                }                
            }            
        }
    }

    public BuildingIndicator GetBuildingIndicator(TileTypes.TileType tileType, Buildings.BuildingType buildingType)
    {

        for(int i = 0; i < instance.buildingIndicators.Count; i++)
        {
            if(instance.buildingIndicators[i].buildingType == buildingType)
            {
                if(buildingType == Buildings.BuildingType.Floor)
                {
                    if(tileType == TileTypes.TileType.Ground)
                    {
                        if(instance.buildingIndicators[i].isAffirmative)
                            return instance.buildingIndicators[i].GetIndicator();
                    }
                    else
                    {
                        if(!instance.buildingIndicators[i].isAffirmative)
                            return instance.buildingIndicators[i].GetIndicator();
                    }
                }

                if(buildingType == Buildings.BuildingType.Door)
                {
                    if(tileType == TileTypes.TileType.Wall)
                    {
                        if(instance.buildingIndicators[i].isAffirmative)
                            return instance.buildingIndicators[i].GetIndicator();
                    }
                    else
                    {
                        if(!instance.buildingIndicators[i].isAffirmative)
                            return instance.buildingIndicators[i].GetIndicator();
                    }
                }

                if(buildingType != Buildings.BuildingType.Floor && buildingType != Buildings.BuildingType.Door)
                {
                    if(tileType == TileTypes.TileType.Floor)
                    {
                        if(instance.buildingIndicators[i].isAffirmative)
                            return instance.buildingIndicators[i].GetIndicator();
                    }
                    else
                    {
                        if(!instance.buildingIndicators[i].isAffirmative)
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


        public BuildingIndicator GetIndicator()
        {
            BuildingIndicator indicator = null;

            if(indicatorsInPool.Count == 0)
            {
                SpawnIndicator();
            }

            indicator = indicatorsInPool.Dequeue();           
            
            return indicator;
        }

        void SpawnIndicator()
        {
            GameObject spawnedIndicatorObject = Instantiate(indicatorObject,instance.transform.position, instance.transform.rotation);
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

            if(tileType == TileTypes.TileType.Ground || tileType == TileTypes.TileType.Floor)
            {
                if(affirmativeIndicatorsInPool == null)
                {
                    affirmativeIndicatorsInPool = new Queue<WorkerSpawnerIndicator>();
                }
                
                if(affirmativeIndicatorsInPool.Count == 0)
                {
                    SpawnIndicator(true);
                }

                indicator = affirmativeIndicatorsInPool.Dequeue(); 
            }else
            {
                if(negativeIndicatorsInPool == null)
                {
                    negativeIndicatorsInPool = new Queue<WorkerSpawnerIndicator>();
                }

                if(negativeIndicatorsInPool.Count == 0)
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
            if(affirmative)
            {
                spawnedIndicatorObject = Instantiate(affirmativeIndicatorObject,instance.transform.position, instance.transform.rotation);
                WorkerSpawnerIndicator indicator = new WorkerSpawnerIndicator(spawnedIndicatorObject,null,true);
                affirmativeIndicatorsInPool.Enqueue(indicator);
            }else
            {
                spawnedIndicatorObject = Instantiate(negativeIndicatorObject,instance.transform.position, instance.transform.rotation);
                WorkerSpawnerIndicator indicator = new WorkerSpawnerIndicator(null,spawnedIndicatorObject,false);
                negativeIndicatorsInPool.Enqueue(indicator);
            }
            
        }

        public void ReturnIndicator(WorkerSpawnerIndicator indicator)
        {
            if(indicator.isAffirmative)
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
}

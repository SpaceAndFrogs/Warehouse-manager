using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorsPool : MonoBehaviour
{
    public static IndicatorsPool instance;
    public List<Indicator> indicators = new List<Indicator>();

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

    public void ReturnIndicator(Indicator indicator)
    {
        for(int i = 0; i < instance.indicators.Count; i++)
        {
            if(instance.indicators[i].buildingType == indicator.buildingType)
            {
                if(instance.indicators[i].isAffirmative == indicator.isAffirmative)
                {
                    instance.indicators[i].ReturnIndicator(indicator);
                    return;
                }                
            }            
        }
    }

    public Indicator GetIndicator(TileTypes.TileType tileType, Buildings.BuildingType buildingType)
    {
        for(int i = 0; i < instance.indicators.Count; i++)
        {
            if(instance.indicators[i].buildingType == buildingType)
            {
                if(buildingType == Buildings.BuildingType.Floor)
                {
                    if(tileType == TileTypes.TileType.Ground)
                    {
                        if(instance.indicators[i].isAffirmative)
                            return instance.indicators[i].GetIndicator();
                    }
                    else
                    {
                        if(!instance.indicators[i].isAffirmative)
                            return instance.indicators[i].GetIndicator();
                    }
                }

                if(buildingType == Buildings.BuildingType.Door)
                {
                    if(tileType == TileTypes.TileType.Wall)
                    {
                        if(instance.indicators[i].isAffirmative)
                            return instance.indicators[i].GetIndicator();
                    }
                    else
                    {
                        if(!instance.indicators[i].isAffirmative)
                            return instance.indicators[i].GetIndicator();
                    }
                }

                if(buildingType != Buildings.BuildingType.Floor && buildingType != Buildings.BuildingType.Door)
                {
                    if(tileType == TileTypes.TileType.Floor)
                    {
                        if(instance.indicators[i].isAffirmative)
                            return instance.indicators[i].GetIndicator();
                    }
                    else
                    {
                        if(!instance.indicators[i].isAffirmative)
                            return instance.indicators[i].GetIndicator();
                    }
                }

                
            }
        }

        return null;
    }

    [System.Serializable]
    public class Indicator
    {
        public Buildings.BuildingType buildingType;
        public GameObject indicatorObject;
        public bool isAffirmative;
        public List<Indicator> indicatorsInPool = new List<Indicator>();

        public Indicator(Buildings.BuildingType buildingType, GameObject indicatorObject, bool isAffirmative)
        {
            this.buildingType = buildingType;
            this.indicatorObject = indicatorObject;
            this.indicatorsInPool = new List<Indicator>();
            this.isAffirmative = isAffirmative;
        }


        public Indicator GetIndicator()
        {
            Indicator indicator = null;

            if(indicatorsInPool.Count == 0)
            {
                SpawnIndicator();
            }

            indicator = indicatorsInPool[0];
            indicatorsInPool.RemoveAt(0);
            
            
            return indicator;
        }

        void SpawnIndicator()
        {
            GameObject spawnedIndicatorObject = Instantiate(indicatorObject,instance.transform.position, instance.transform.rotation);
            Indicator indicator = new Indicator(this.buildingType, spawnedIndicatorObject, this.isAffirmative);
            indicatorsInPool.Add(indicator);
        }

        public void ReturnIndicator(Indicator indicator)
        {
            indicator.indicatorObject.transform.position = instance.transform.position;
            indicatorsInPool.Add(indicator);
        }
    }
}

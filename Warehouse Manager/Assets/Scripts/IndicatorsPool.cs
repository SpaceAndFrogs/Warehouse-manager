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

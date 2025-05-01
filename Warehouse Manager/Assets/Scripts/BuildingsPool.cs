using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingsPool : MonoBehaviour
{
    public static BuildingsPool instance;
    [SerializeField]
    List<Building> buildings = new List<Building>();

    public Building GetBuilding(Buildings.BuildingType buildingType)
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            if (buildings[i].buildingType == buildingType)
            {
                return buildings[i].GetBuilding();
            }
        }
        return null;
    }

    public void ReturnBuilding(Building building)
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            if (buildings[i].buildingType == building.buildingType)
            {
                buildings[i].ReturnBuilding(building);
                return;
            }
        }
    }

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

    [System.Serializable]
    public class Building
    {
        public Buildings.BuildingType buildingType;
        public GameObject buildingObject;
        public Queue<Building> buildingsInPool = new Queue<Building>();

        public Building(Buildings.BuildingType type, GameObject buildingObject)
        {
            this.buildingType = type;
            this.buildingObject = buildingObject;
            this.buildingsInPool = new Queue<Building>();
        }

        public Building GetBuilding()
        {
            if(buildingsInPool == null)
            {
                buildingsInPool = new Queue<Building>();
            }

            if (buildingsInPool.Count == 0)
            {
                MakeBuilding();
            }

            return buildingsInPool.Dequeue();
            
        }

        void MakeBuilding()
        {
            GameObject buildingObject = Instantiate(this.buildingObject, instance.transform.position, Quaternion.identity);
            Building building = new Building(this.buildingType, buildingObject);
            if (this.buildingType == Buildings.BuildingType.Floor)
            {
                buildingObject.GetComponent<BuildingScript>().building = building;
            }
            
            buildingsInPool.Enqueue(building);
        }

        public void ReturnBuilding(Building building)
        {
            building.buildingObject.transform.position = instance.transform.position;
            buildingsInPool.Enqueue(building);
        } 
    }
}

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

    void LoadBuildings()
    {
        List<SaveData.BuildingData> buildingData = SavingManager.instance.saveData.buildings;
        foreach (var data in buildingData)
        {
            Building building = GetBuilding((Buildings.BuildingType)System.Enum.Parse(typeof(Buildings.BuildingType), data.type));
            if (building != null)
            {
                BuildingScript buildingScript = building.buildingObject.GetComponent<BuildingScript>();
                buildingScript.building = building;
                building.buildingObject.transform.position = data.position;
                building.buildingObject.transform.rotation = data.rotation;
                building.buildingObject.SetActive(true);
                Tile tile = GetTile(data.position);

                if (tile.building == null || tile.building.buildingType == Buildings.BuildingType.Floor)
                {
                    tile.building = building;
                }
            }
        }

        Worker.NotifyBuildingEnded();
    }

    Tile GetTile(Vector3 position)
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

    void OnEnable()
    {
        SavingManager.OnBuildingsLoad += LoadBuildings;
    }

    void OnDisable()
    {
        SavingManager.OnBuildingsLoad -= LoadBuildings;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingsPool : MonoBehaviour
{
    public static BuildingsPool instance;
    [SerializeField]
    List<Building> buildings = new List<Building>();
    [SerializeField]
    Items itemsData;

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
            Buildings.BuildingType buildingType = (Buildings.BuildingType)System.Enum.Parse(typeof(Buildings.BuildingType), data.type);
            if (buildingType == Buildings.BuildingType.Rack || buildingType == Buildings.BuildingType.PackStation || buildingType == Buildings.BuildingType.PickStash)
                continue;

                Building building = GetBuilding(buildingType);
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

        List<SaveData.PickStashData> pickStashData = SavingManager.instance.saveData.pickStashes;
        foreach (var data in pickStashData)
        {
            Building building = GetBuilding(Buildings.BuildingType.PickStash);
            if (building != null)
            {
                building.buildingObject.transform.position = data.position;
                building.buildingObject.transform.rotation = data.rotation;
                building.buildingObject.SetActive(true);
                Tile tile = GetTile(data.position);

                if (tile.building == null || tile.building.buildingType == Buildings.BuildingType.Floor)
                {
                    tile.building = building;
                }

                PickStash pickStash= building.buildingObject.GetComponent<PickStash>();
                if(pickStash != null)
                {
                    pickStash.tileWithBuilding = tile;
                } 
            }
        }

        List<SaveData.PackStationData> packStationData = SavingManager.instance.saveData.packStations;
        foreach (var data in packStationData)
        {
            Building building = GetBuilding(Buildings.BuildingType.PackStation);
            if (building != null)
            {
                building.buildingObject.transform.position = data.position;
                building.buildingObject.transform.rotation = data.rotation;
                building.buildingObject.SetActive(true);
                Tile tile = GetTile(data.position);
                if(tile == null)
                {
                    Debug.LogError("Tile not found at position: " + data.position);
                    continue;
                }

                if (tile.building == null || tile.building.buildingType == Buildings.BuildingType.Floor)
                {
                    tile.building = building;
                }

                PackStation packStation = building.buildingObject.GetComponent<PackStation>();
                if (packStation != null)
                {
                    packStation.tileWithBuilding = tile;
                    packStation.isInRoom = data.isInRoom;
                }
                else
                {
                    Debug.LogError("PackStation component not found on PackStation building object.");
                }
            }
        }

        List<SaveData.RackData> rackData = SavingManager.instance.saveData.racks;
        foreach (var data in rackData)
        {
            Building building = GetBuilding(Buildings.BuildingType.Rack);
            if (building != null)
            {
                building.buildingObject.transform.position = data.position;
                building.buildingObject.transform.rotation = data.rotation;
                building.buildingObject.SetActive(true);
                Tile tile = GetTile(data.position);

                if (tile.building == null || tile.building.buildingType == Buildings.BuildingType.Floor)
                {
                    tile.building = building;
                }

                Rack rack = building.buildingObject.GetComponent<Rack>();
                if (rack != null)
                {
                    rack.tileWithBuilding = tile;
                    rack.isInRoom = data.isInRoom;
                    rack.amountOfItems = data.amountOfItems;
                    rack.reservedAmountOfItems = data.reservedAmountOfItems;
                    rack.desiredAmountOfItems = data.desiredAmountOfItems;
                    rack.maxAmountOfItems = data.maxAmountOfItems;
                    Items.ItemType itemType = (Items.ItemType)System.Enum.Parse(typeof(Items.ItemType), data.itemType);
                    Items.Item itemOnRack = null;
                    for (int i = 0; i < itemsData.items.Count; i++)
                    {
                        if (itemsData.items[i].itemType == itemType)
                        {
                            itemOnRack = itemsData.items[i];
                            break;
                        }
                    }
                }
            }
        }

        BuildingWorker.NotifyBuildingEnded();
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

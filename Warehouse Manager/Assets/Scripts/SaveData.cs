using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<RackData> racks = new List<RackData>();
    public List<PackStationData> packStations = new List<PackStationData>();
    public List<PickStashData> pickStashes = new List<PickStashData>();
    public List<WorkerData> workers = new List<WorkerData>();
    public List<BuildingData> buildings = new List<BuildingData>();
    public List<TileData> tiles = new List<TileData>();
    public List<MapFragmentData> mapFragments = new List<MapFragmentData>();
    public List<TaskData> tasks = new List<TaskData>();
    public List<BuildingIndicatorData> buildingIndicators = new List<BuildingIndicatorData>();
    public List<TaskIndicatorData> taskIndicators = new List<TaskIndicatorData>();
    public List<WorkerSpawnerIndicatorData> workerSpawnerIndicators = new List<WorkerSpawnerIndicatorData>();
    public List<LoansData> loans = new List<LoansData>();
    public List<ItemPricesData> itemPrices = new List<ItemPricesData>();
    public float playerCash;
    public float gameTime;

    [System.Serializable]
    public class TaskData
    {
        public string type;
        public string taskClass;
        public Vector3 positionOfTileWithTask;
        public Vector3 positionOfTileWithPickStah;
        public string tileTypeAfterTask;
        public List<Vector3> positionOfRacksWithItems = new List<Vector3>();
        public List<int> amountOfItemsFromRacks = new List<int>();
        public float orderPrice;

        public TaskData(string type, string taskClass, Vector3 positionOfTileWithTask, Vector3 positionOfTileWithPickStah, string tileTypeAfterTask, List<Vector3> positionOfRacksWithItems, float orderPrice, List<int> amountOfItemsFromRacks)
        {
            this.type = type;
            this.taskClass = taskClass;
            this.positionOfTileWithTask = positionOfTileWithTask;
            this.positionOfTileWithPickStah = positionOfTileWithPickStah;
            this.tileTypeAfterTask = tileTypeAfterTask;
            this.positionOfRacksWithItems = positionOfRacksWithItems;
            this.orderPrice = orderPrice;
            this.amountOfItemsFromRacks = amountOfItemsFromRacks;
        }
    }    

    [System.Serializable]
    public class LoansData
    {
        public string cashToPayOff;
        public string installment;

        public LoansData(string cashToPayOff, string installment)
        {
            this.cashToPayOff = cashToPayOff;
            this.installment = installment;
        }
    }

    [System.Serializable]
    public class ItemPricesData
    {
        public string itemName;
        public string buyPrice;
        public string sellPrice;
        public string demandPrice;
        public string growthRate;
        public bool sellingLocked;
        public string itemType;

        public ItemPricesData(string itemName, string buyPrice, string sellPrice, string demandPrice, string growthRate, bool sellingLocked, string itemType)
        {
            this.itemName = itemName;
            this.buyPrice = buyPrice;
            this.sellPrice = sellPrice;
            this.demandPrice = demandPrice;
            this.growthRate = growthRate;
            this.sellingLocked = sellingLocked;
            this.itemType = itemType;
        }
        
    }

    [System.Serializable]
    public class WorkerSpawnerIndicatorData
    {
        public bool isAffirmative;
        public Vector3 position;
        public Quaternion rotation;

        public WorkerSpawnerIndicatorData(bool isAffirmative, Vector3 position, Quaternion rotation)
        {
            this.isAffirmative = isAffirmative;
            this.position = position;
            this.rotation = rotation;
        }
    }

    [System.Serializable]
    public class TaskIndicatorData
    {
        public string type;
        public bool isAffirmative;
        public Vector3 position;
        public Quaternion rotation;
        public TaskIndicatorData(string type, bool isAffirmative, Vector3 position, Quaternion rotation)
        {
            this.type = type;
            this.isAffirmative = isAffirmative;
            this.position = position;
            this.rotation = rotation;
        }
    }

    [System.Serializable]
    public class BuildingIndicatorData
    {
        public string type;
        public bool isAffirmative;
        public Vector3 position;
        public Quaternion rotation;
        public BuildingIndicatorData(string type, bool isAffirmative, Vector3 position, Quaternion rotation)
        {
            this.type = type;
            this.isAffirmative = isAffirmative;
            this.position = position;
            this.rotation = rotation;
        }
    }

    [System.Serializable]
    public class RackData
    {
        public string itemType;
        public int amountOfItems;
        public int reservedAmountOfItems;
        public int desiredAmountOfItems;
        public int maxAmountOfItems;
        public Vector3 position;
        public Quaternion rotation;
        public RackData(string itemType, int amountOfItems, int reservedAmountOfItems, int desiredAmountOfItems, int maxAmountOfItems, Vector3 position, Quaternion rotation)
        {
            this.itemType = itemType;
            this.amountOfItems = amountOfItems;
            this.reservedAmountOfItems = reservedAmountOfItems;
            this.desiredAmountOfItems = desiredAmountOfItems;
            this.maxAmountOfItems = maxAmountOfItems;
            this.position = position;
            this.rotation = rotation;
        }
    }

    [System.Serializable]
    public class PackStationData
    {
        public bool havePackWorker;
        public bool isInRoom;
        public Vector3 position;
        public Quaternion rotation;
        public PackStationData(bool havePackWorker, bool isInRoom, Vector3 position, Quaternion rotation)
        {
            this.havePackWorker = havePackWorker;
            this.isInRoom = isInRoom;
            this.position = position;
            this.rotation = rotation;
        }
    }
    [System.Serializable]
    public class PickStashData
    {
        public bool isInRoom;
        public Vector3 position;
        public Quaternion rotation;
        public PickStashData(bool isInRoom, Vector3 position, Quaternion rotation)
        {
            this.isInRoom = isInRoom;
            this.position = position;
            this.rotation = rotation;
        }
    }
    [System.Serializable]
    public class WorkerData
    {
        public string name;
        public string type;
        public float moveSpeed;
        public float workSpeed;
        public float salary;
        public Vector3 position;
        public Quaternion rotation;
        public WorkerData(string name, string type, float moveSpeed, float workSpeed, float salary, Vector3 position, Quaternion rotation)
        {
            this.name = name;
            this.type = type;
            this.moveSpeed = moveSpeed;
            this.workSpeed = workSpeed;
            this.salary = salary;
            this.position = position;
            this.rotation = rotation;
        }
    }
    [System.Serializable]
    public class BuildingData
    {
        public string type;
        public Vector3 position;
        public Quaternion rotation;
        public BuildingData(string type, Vector3 position, Quaternion rotation)
        {
            this.type = type;
            this.position = position;
            this.rotation = rotation;
        }
    }
    [System.Serializable]
    public class TileData
    {
        public Vector3 position;
        public Quaternion rotation;
        public string type;
        public TileData(Vector3 position, Quaternion rotation, string type)
        {
            this.position = position;
            this.rotation = rotation;
            this.type = type;
        }
    }
    [System.Serializable]
    public class MapFragmentData
    {
        public Vector3 position;
        public Quaternion rotation;
        public List<SerializableColor> colorSamples;
        public int width, height;
        public MapFragmentData(Vector3 position, Quaternion rotation, List<List<Color>> colorSamples)
        {
            this.position = position;
            this.rotation = rotation;
            width = colorSamples[0].Count;
            height = colorSamples.Count;
            this.colorSamples = Flatten(SetColorSamples(colorSamples), width);
        }

        public List<SerializableColor> Flatten(List<List<SerializableColor>> twoDList, int width)
        {
            List<SerializableColor> flatList = new List<SerializableColor>();

            for(int i = 0; i < width; i++)
            {                
                flatList.AddRange(twoDList[i]);                
            }

            return flatList;
        }

        public List<List<SerializableColor>> Unflatten(List<SerializableColor> flatList, int width, int height)
        {
            List<List<SerializableColor>> twoDList = new List<List<SerializableColor>>();

            for (int i = 0; i < height; i++)
            {
                List<SerializableColor> row = new List<SerializableColor>();
                for (int j = 0; j < width; j++)
                {
                    row.Add(flatList[i * width + j]);
                }
                twoDList.Add(row);
            }

            return twoDList;
        }

        public List<List<SerializableColor>> SetColorSamples(List<List<Color>> colorSamples)
        {
            List<List<SerializableColor>> colors = new List<List<SerializableColor>>();
            for (int i = 0; i < colorSamples.Count; i++)
            {
                List<SerializableColor> row = new List<SerializableColor>();
                for (int j = 0; j < colorSamples[i].Count; j++)
                {
                    row.Add(new SerializableColor(colorSamples[i][j]));
                }

                colors.Add(row);
            }

            return colors;
        }

        [System.Serializable]
        public class SerializableColor
        {
            public float r, g, b, a;

            public SerializableColor() { }

            public SerializableColor(Color color)
            {
                r = color.r;
                g = color.g;
                b = color.b;
                a = color.a;
            }

            public Color ToUnityColor()
            {
                return new Color(r, g, b, a);
            }
        }
    }
    
}

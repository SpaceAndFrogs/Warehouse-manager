using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickPackStationsManager : MonoBehaviour
{
    public List<PackStation> packStations = new List<PackStation>();
    public List<PickStash> pickStashes = new List<PickStash>();

    public List<PackWorker> packWorkers = new List<PackWorker>();
    public List<PickWorker> pickWorkers = new List<PickWorker>();

    public OrdersStation ordersStation = null;

    public static PickPackStationsManager instance;

    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        PackStation.OnPackStationSpawned += AddNewPackStation;
        WorkerBase.OnWorkerSpawned += AddNewWorker;
        WorkerBase.OnWorkerFired += RemoveWorker;
        PickStash.OnPickStashSpawned += AddNewPickStash;
        OrdersStation.OnStationSpawned += AddOrderStation;
    }

    void OnDisable()
    {
        PackStation.OnPackStationSpawned -= AddNewPackStation;
        WorkerBase.OnWorkerSpawned -= AddNewWorker;
        WorkerBase.OnWorkerFired -= RemoveWorker;
        PickStash.OnPickStashSpawned -= AddNewPickStash;
        OrdersStation.OnStationSpawned -= AddOrderStation;
    }

    void AddNewPickStash(PickStash pickStash)
    {
        pickStashes.Add(pickStash);
    }

    void AddNewPackStation(PackStation packStation)
    {
        for(int i = 0; i < packWorkers.Count; i++)
        {
            if(packWorkers[i].packStationTile == null)
            {
                packWorkers[i].packStationTile = packStation.tileWithBuilding;
                packStation.havePackWorker = true;
                packWorkers[i].GoToStation();
                break;
            }
        }
        packStations.Add(packStation);
    }

    void RemoveWorker(WorkerBase worker)
    {
        
        if (worker.stats.workerType == WorkerData.WorkerType.Pack)
        {
            PackWorker packWorker = worker.GetComponent<PackWorker>();
            for (int i = 0; i < packStations.Count; i++)
            {
                if (packStations[i].havePackWorker && packWorker.packStationTile == packStations[i].tileWithBuilding)
                {
                    packStations[i].havePackWorker = false;
                    packWorker.packStationTile = null;
                    packWorkers.Remove(packWorker);
                    break;
                }
            }

            return;
        }
        else if (worker.stats.workerType == WorkerData.WorkerType.Pick)
        {
            PickWorker pickWorker = worker.GetComponent<PickWorker>();
            pickWorkers.Remove(pickWorker);
            return;
        }
        else
        {
            return;
        }
    }

    void AddNewWorker(WorkerBase worker)
    {
        if(worker.stats.workerType == WorkerData.WorkerType.Pack)
        {
            PackWorker packWorker = worker.GetComponent<PackWorker>();
            packWorkers.Add(packWorker);
            
            for(int i = 0; i < packStations.Count; i++)
            {
                if(!packStations[i].havePackWorker)
                {
                    packStations[i].havePackWorker = true;
                    packWorker.packStationTile = packStations[i].tileWithBuilding;
                    packWorker.GoToStation();
                    break;
                }
            }

            return;
        }else if(worker.stats.workerType == WorkerData.WorkerType.Pick)
        {
            PickWorker pickWorker = worker.GetComponent<PickWorker>();
            pickWorkers.Add(pickWorker);
            if(ordersStation != null)
            {
                pickWorker.GoToStation();
            }            
            return;
        }else
        {
            return;
        }


    }

    void AddOrderStation(OrdersStation ordersStation)
    {
        this.ordersStation = ordersStation;
        for(int i = 0; i < pickWorkers.Count; i++)
        {
            pickWorkers[i].GoToStation();
        }
    }
}

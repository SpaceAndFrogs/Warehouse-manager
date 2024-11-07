using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickPackStationsManager : MonoBehaviour
{
    public List<PackStation> packStations = new List<PackStation>();
    public List<PickStash> pickStashes = new List<PickStash>();

    public List<Worker> packWorkers = new List<Worker>();
    public List<Worker> pickWorkers = new List<Worker>();

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
        Worker.OnWorkerSpawned += AddNewWorker;
        PickStash.OnPickStashSpawned += AddNewPickStash;
    }

    void OnDisable()
    {
        PackStation.OnPackStationSpawned -= AddNewPackStation;
        Worker.OnWorkerSpawned -= AddNewWorker;
        PickStash.OnPickStashSpawned -= AddNewPickStash;
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
                packWorkers[i].packStationTile = packStation.tileWithStation;
                packStation.havePackWorker = true;
                break;
            }
        }
        packStations.Add(packStation);
    }

    void AddNewWorker(Worker worker)
    {
        if(worker.workerData.workerType == WorkerData.WorkerType.Pack)
        {
            packWorkers.Add(worker);
            
            for(int i = 0; i < packStations.Count; i++)
            {
                if(!packStations[i].havePackWorker)
                {
                    packStations[i].havePackWorker = true;
                    worker.packStationTile = packStations[i].tileWithStation;
                    break;
                }
            }

            return;
        }else if(worker.workerData.workerType == WorkerData.WorkerType.Pick)
        {
            pickWorkers.Add(worker);
            return;
        }else
        {
            return;
        }


    }
}

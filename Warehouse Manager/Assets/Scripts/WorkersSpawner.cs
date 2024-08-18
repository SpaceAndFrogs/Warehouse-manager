using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkersSpawner : MonoBehaviour
{
    [SerializeField]
    Worker builderPrefab;
    [SerializeField]
    Worker pickWorkerPrefab;
    [SerializeField]
    Worker packWorkerPrefab;

    [SerializeField]
    int amountOfBuildersToSpawnAtStart;
    [SerializeField]
    int amountOfPickWorkersToSpawnAtStart;
    [SerializeField]
    int amountOfPackWorkersToSpawnAtStart;
    void Start()
    {
        SpawnWorkers();
    }

    void SpawnWorkers()
    {
        SpawnBuilders(amountOfBuildersToSpawnAtStart);
        SpawnPickWorkers(amountOfPickWorkersToSpawnAtStart);
        SpawnPackWorkers(amountOfPackWorkersToSpawnAtStart);
    }

    void SpawnBuilders(int amountOfBuilders)
    {
        for(int i = 0; i < amountOfBuilders; i++)
        {
            Instantiate(builderPrefab,transform.position,transform.rotation);
        }
    }
    void SpawnPickWorkers(int amountOfPickWorkers)
    {
        for(int i = 0; i < amountOfPickWorkers; i++)
        {
            Instantiate(pickWorkerPrefab,transform.position,transform.rotation);
        }
    }
    void SpawnPackWorkers(int amountOfPackWorkers)
    {
        for(int i = 0; i < amountOfPackWorkers; i++)
        {
            Instantiate(packWorkerPrefab,transform.position,transform.rotation);
        }
    }
}

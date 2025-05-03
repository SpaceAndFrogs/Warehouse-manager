using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkersPool : MonoBehaviour
{
    public static WorkersPool instance;
    [SerializeField]
    List<WorkerClass> workers = new List<WorkerClass>();
    public Worker GetWorker(WorkerData.WorkerType workerType, Worker.Stats stats)
    {
        foreach(WorkerClass workerClass in workers)
        {
            if(workerClass.workerType == workerType)
            {
                return workerClass.GetWorker(stats);
            }
        }

        return null;
    }

    public void ReturnWorker(Worker worker)
    {
        foreach(WorkerClass workerClass in workers)
        {
            if(workerClass.workerType == worker.stats.workerType)
            {
                workerClass.ReturnWorker(worker);
            }
        }
    }

    [System.Serializable]
    public class WorkerClass
    {
        public WorkerData.WorkerType workerType;
        public Worker workerObject;
        public Queue<Worker> workersInPool = new Queue<Worker>();

        public Worker GetWorker(Worker.Stats stats)
        {
            if(workersInPool == null)
            {
                workersInPool = new Queue<Worker>();
            }

            if(workersInPool.Count == 0)
            {
                MakeWorker();
            }

            Worker worker = workersInPool.Dequeue();
            worker.stats = stats;
            return worker;
        }

        void MakeWorker()
        {
            Worker worker = Instantiate(workerObject, instance.transform.position, instance.transform.rotation);
            workersInPool.Enqueue(worker);
        }

        public void ReturnWorker(Worker worker)
        {
            worker.transform.position = instance.transform.position;
            worker.stats = null;
            workersInPool.Enqueue(worker);
        }
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }else
        {
            Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkersPool : MonoBehaviour
{
    public static WorkersPool instance;
    [SerializeField]
    List<WorkerClass> workers = new List<WorkerClass>();
    public WorkerBase GetWorker(WorkerData.WorkerType workerType, WorkerBase.Stats stats)
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

    public void ReturnWorker(WorkerBase worker)
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
        public WorkerBase workerObject;
        public Queue<WorkerBase> workersInPool = new Queue<WorkerBase>();

        public WorkerBase GetWorker(WorkerBase.Stats stats)
        {
            if(workersInPool == null)
            {
                workersInPool = new Queue<WorkerBase>();
            }

            if(workersInPool.Count == 0)
            {
                MakeWorker();
            }

            WorkerBase worker = workersInPool.Dequeue();
            worker.stats = stats;
            return worker;
        }

        void MakeWorker()
        {
            WorkerBase worker = Instantiate(workerObject, instance.transform.position, instance.transform.rotation);
            workersInPool.Enqueue(worker);
        }

        public void ReturnWorker(WorkerBase worker)
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

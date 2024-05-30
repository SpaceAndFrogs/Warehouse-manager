using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager instance { get; private set; }

    public List<Task> tasks = new List<Task>();

    public Tasks currentTask;

    public List<Worker> freeWorkers = new List<Worker>();

    public enum Tasks { Go, None};
    public class Task
    {
        public Tasks task;
        public Tile tileWithTask;
        public Task(Tasks task, Tile tileWithTask)
        {
            this.task = task;
            this.tileWithTask = tileWithTask;
        }
    }

    private void Awake()
    {
        if(instance != null) 
        {
            Destroy(instance);
        }else
        {
            instance = this;
        }
    }

    private void Update()
    {        
        CheckForInput();
        GiveTasksToWorkers();
    }

    void CheckForInput()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                
                Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

                if (tile == null)
                    return;

                if (!tile.walkable)
                    return;

                tasks.Add(new Task(currentTask,tile));
            }

        }
    }

    public void ReturnWorker(Worker worker)
    {
        freeWorkers.Add(worker);
    }

    void GiveTasksToWorkers()
    {
        if (tasks.Count == 0)
            return;

        if (freeWorkers.Count == 0)
            return;

        List<Task> givenTasks = new List<Task>();

        for(int i = 0; i < tasks.Count; i++)
        {
            Worker workerForTask = FindClosestWorker(tasks[i].tileWithTask);

            if (workerForTask == null)
                continue;

            workerForTask.GetTask(tasks[i]);

            givenTasks.Add(tasks[i]);

            freeWorkers.Remove(workerForTask);
        }

        for(int i = 0;i < givenTasks.Count;i++)
        {
            tasks.Remove(givenTasks[i]);
        }
    }

    Worker FindClosestWorker(Tile endTile)
    {
        int currentClosestPath = MapGenerator.instance.GetAmountOfAllTiles();
        Worker currentClosestWorker = null;

        for (int i = 0; i < freeWorkers.Count; i++)
        {
            Tile[] path = PathFinder.instance.FindPath(freeWorkers[i].startNode, endTile);

            if(path == null)
                continue;

            if(path.Length < currentClosestPath)
            {
                currentClosestWorker = freeWorkers[i];
                currentClosestPath = path.Length;
            }

        }

        if (currentClosestWorker! != null)
            return currentClosestWorker;

        return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : MonoBehaviour
{
    public Tile endNode; 
    public Tile startNode;
    public Tile[] path = new Tile[0];
    [SerializeField]
    WorkerData workerData;

    TaskManager.Task currentTask = null;


    private void Start()
    {
        ReturnWorker();
    }

    public void GetTask(TaskManager.Task task)
    {

        endNode = task.tileWithTask;
        currentTask = task;
        GetPathToTarget();
    }
    public void GetPathToTarget()
    {
        path = PathFinder.instance.FindPath(startNode, endNode);
        StopCoroutine(FollowPath());
        StartCoroutine(FollowPath());
    }

    IEnumerator FollowPath()
    {
        int i = 1;
        Vector3 direction = path[i].transform.position - transform.position;
        direction.Normalize();
        while (i < path.Length)
        {

            direction = path[i].transform.position - transform.position;
            direction.Normalize();
            transform.position += direction * workerData.moveSpeed * Time.deltaTime;

            if(Vector3.Distance(transform.position, path[i].transform.position)<= workerData.proxyMargin && i < path.Length - 1)
            { 
                i++;
            }

            if(Vector3.Distance(transform.position, path[i].transform.position)<= workerData.proxyMarginOfFinalTile && i == path.Length - 1)
            { 
                i++;
            }
            yield return new WaitForEndOfFrame();
        }

        StopCoroutine(StartTask());
        StartCoroutine(StartTask());
    }

    IEnumerator StartTask()
    {
        float timer = 0;

        if(currentTask.task.taskType != TasksTypes.TaskType.Build)
        {
            while(timer <= currentTask.task.taskTime)
            {
                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            currentTask.tileWithTask.ChangeTileType(currentTask.task.tileTypeAfterTask);
        }else
        {
            while(timer <= currentTask.building.buildingTime)
            {
                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            currentTask.tileWithTask.ChangeTileType(currentTask.task.tileTypeAfterTask);

            GameObject newBuilding = Instantiate(currentTask.building.buildingObject, currentTask.tileWithTask.transform.position, currentTask.tileWithTask.transform.rotation);
            newBuilding.transform.parent = currentTask.tileWithTask.transform;        
        }

        currentTask = null;
        ReturnWorker();
        yield break;
    }

    void ReturnWorker()
    {
        TaskManager.instance.ReturnWorker(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Tile tile = collision.gameObject.GetComponent<Tile>();

        if (tile == null)
            return;

        startNode = tile; 
    }

    
}

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

    public void GetPathToTarget()
    {
        path = PathFinder.instance.FindPath(startNode, endNode);
        StartCoroutine(FollowPath());

        Debug.Log("Path length: " + path.Length);
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

            if(Vector3.Distance(transform.position, path[i].transform.position)<= workerData.proxyMargin)
            { 
                i++;
                Debug.Log("Index: " + i);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Tile tile = collision.gameObject.GetComponent<Tile>();

        if (tile == null)
            return;

        startNode = tile; 
    }

    
}

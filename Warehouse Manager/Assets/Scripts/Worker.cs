using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : MonoBehaviour
{
    public Tile endNode; 
    public Tile startNode;
    Tile[] path = new Tile[0];
    [SerializeField]
    WorkerData workerData;

    public void GetPathToTarget()
    {
        path = PathFinder.instance.FindPath(startNode, endNode);
        StartCoroutine(FollowPath());
    }

    IEnumerator FollowPath()
    {
        int i = 1;
        Vector3 direction = path[i].transform.position - transform.position;
        direction.Normalize();
        while (i < path.Length)
        {
            transform.position += direction * workerData.moveSpeed * Time.deltaTime;

            if(Vector3.Distance(transform.position, path[i].transform.position)<= workerData.proxyMargin)
            {
                direction = path[i].transform.position - transform.position;
                direction.Normalize();
                i++;
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

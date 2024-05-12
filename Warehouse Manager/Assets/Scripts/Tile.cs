using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField]
    TileTypes tileTypes;

    public bool walkable
    {
        get; private set;
    }

    public float gCost;
    public float hCost;

    public float fCost
    {
        get { return gCost + hCost; }
    }

    public List<Tile> neighborTiles = new List<Tile>();

    public void ChangeTileType(float noiseSample)
    {
        for(int i = 0;i<tileTypes.tileTypesRanges.Count;i++)
        {
            if(noiseSample >= tileTypes.tileTypesRanges[i].tileRange.x && noiseSample < tileTypes.tileTypesRanges[i].tileRange.y)
            {
                walkable = tileTypes.tileTypesRanges[i].walkable;
            }
        }
    }

    public void AddTileToNeighborTiles(Tile tile)
    {
        if(neighborTiles.Contains(tile)) 
        { return; }
        neighborTiles.Add(tile);
    }

    private void Start()
    {
        CheckForNeighbours();
    }

    void CheckForNeighbours()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.back, out hit, Mathf.Infinity))
        {
            Tile tile = hit.collider.gameObject.GetComponent<Tile>();
            if (tile != null)
            { AddTileToNeighborTiles(tile); }
        }

        if (Physics.Raycast(transform.position, Vector3.forward, out hit, Mathf.Infinity))
        {
            Tile tile = hit.collider.gameObject.GetComponent<Tile>();
            if (tile != null)
            { AddTileToNeighborTiles(tile); }
        }

        if (Physics.Raycast(transform.position, Vector3.left, out hit, Mathf.Infinity))
        {
            Tile tile = hit.collider.gameObject.GetComponent<Tile>();
            if (tile != null)
            { AddTileToNeighborTiles(tile); }
        }

        if (Physics.Raycast(transform.position, Vector3.right, out hit, Mathf.Infinity))
        {
            Tile tile = hit.collider.gameObject.GetComponent<Tile>();
            if (tile != null)
            { AddTileToNeighborTiles(tile); }
        }

        if (Physics.Raycast(transform.position, new Vector3(1, 0, 1), out hit, Mathf.Infinity))
        {
            Tile tile = hit.collider.gameObject.GetComponent<Tile>();
            if (tile != null)
            { AddTileToNeighborTiles(tile); }
        }

        if (Physics.Raycast(transform.position, new Vector3(1, 0, -1), out hit, Mathf.Infinity))
        {
            Tile tile = hit.collider.gameObject.GetComponent<Tile>();
            if (tile != null)
            { AddTileToNeighborTiles(tile); }
        }

        if (Physics.Raycast(transform.position, new Vector3(-1, 0, 1), out hit, Mathf.Infinity))
        {
            Tile tile = hit.collider.gameObject.GetComponent<Tile>();
            if (tile != null)
            { AddTileToNeighborTiles(tile); }
        }

        if (Physics.Raycast(transform.position, new Vector3(-1, 0, -1), out hit, Mathf.Infinity))
        {
            Tile tile = hit.collider.gameObject.GetComponent<Tile>();
            if (tile != null)
            { AddTileToNeighborTiles(tile); }
        }
    }
}

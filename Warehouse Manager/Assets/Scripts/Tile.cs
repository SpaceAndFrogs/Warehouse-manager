using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField]
    TileTypes tileTypes;

    public TileTypes.TileType tileType;

    public Tile parentTile;

    public bool walkable;

    public float gCost;
    public float hCost;

    public GameObject building = null;

    public MapFragmentScript mapFragmentScript
    {
        private get; set;
    }
    public Vector2 tileCords
    {
        private get; set;
    }

    public float fCost
    {
        get { return gCost + hCost; }
    }

    public List<Tile> neighborTiles = new List<Tile>();

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

    public void SetTileType(float noiseSample)
    {
        for (int r = 0; r < tileTypes.tileTypesRanges.Count; r++)
        {
            if (noiseSample >= tileTypes.tileTypesRanges[r].tileRange.x && noiseSample < tileTypes.tileTypesRanges[r].tileRange.y)
            {
                walkable = tileTypes.tileTypesRanges[r].walkable;
                tileType = tileTypes.tileTypesRanges[r].tileType;
            }
        }
    }

    public void ChangeTileType(TileTypes.TileType tileTypeToChange)
    {
        int tileTypeRangesIndex = 0; 

        for (int r = 0; r < tileTypes.tileTypesRanges.Count; r++)
        {
            if(tileTypeToChange == tileTypes.tileTypesRanges[r].tileType)
            {
                tileTypeRangesIndex = r;
                break;
            }
        }

        mapFragmentScript.ChangeTileOnMap(tileCords,tileTypes.tileTypesRanges[tileTypeRangesIndex].color);

        walkable = tileTypes.tileTypesRanges[tileTypeRangesIndex].walkable;
        tileType = tileTypes.tileTypesRanges[tileTypeRangesIndex].tileType;
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

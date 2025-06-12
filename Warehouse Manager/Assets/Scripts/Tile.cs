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

    [NonSerialized]
    public BuildingsPool.Building building = null;
    public bool haveTask = false;

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

    [SerializeField]
    GameObject outlineObject;

    public List<Tile> neighborTiles = new List<Tile>();

    public void AddTileToNeighborTiles(Tile tile)
    {
        if(neighborTiles.Contains(tile)) 
        { return; }
        neighborTiles.Add(tile);
    }

    private void Start()
    {
        outlineObject.SetActive(false);
        CheckForNeighbours();
    }

    private void OnMouseEnter() 
    {
        outlineObject.SetActive(true);
    }
    private void OnMouseExit() 
    {
        outlineObject.SetActive(false);
    }

    public void SetTileType(float noiseSample)
    {
        for (int r = 0; r < tileTypes.tileTypesRanges.Count; r++)
        {
            if (noiseSample >= tileTypes.tileTypesRanges[r].tileRange.x && noiseSample <= tileTypes.tileTypesRanges[r].tileRange.y)
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

        if(tileTypeToChange == TileTypes.TileType.Ground && tileType == TileTypes.TileType.Floor)
        {
            RemoveBuilding(true);
        }
        else if(tileTypeToChange == TileTypes.TileType.Floor && (tileType == TileTypes.TileType.Wall || tileType == TileTypes.TileType.Other || tileType == TileTypes.TileType.Door))
        {
            RemoveBuilding(true);
            CheckForBuilding();
        }

        

        mapFragmentScript.ChangeTileOnMap(tileCords,tileTypes.tileTypesRanges[tileTypeRangesIndex].color);

        walkable = tileTypes.tileTypesRanges[tileTypeRangesIndex].walkable;
        tileType = tileTypes.tileTypesRanges[tileTypeRangesIndex].tileType;
    }

    void CheckForBuilding()
    {

        Ray ray = new Ray(transform.position+Vector3.up, Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, 10f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        for(int i = 1; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.TryGetComponent<BuildingScript>(out BuildingScript buildingScript))
            {
                Debug.Log("Found: " + hits[i].collider.gameObject.name);
                building = buildingScript.building;
                break;
            }
        }
    }

    public void RemoveBuilding(bool removeAll)
    {
        if(building == null)
            return;

        if(removeAll)
        {
            BuildingsPool.instance.ReturnBuilding(building);
            building = null;
            return;
        }

        switch(tileType)
        {
            case TileTypes.TileType.Wall:
            {
                    BuildingsPool.instance.ReturnBuilding(building);
                    building = null;
                    return;
            }
        }
        
    }

    void CheckForNeighbours()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.left, out hit, Mathf.Infinity))
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

        if (Physics.Raycast(transform.position, Vector3.forward, out hit, Mathf.Infinity))
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

        if (Physics.Raycast(transform.position, Vector3.right, out hit, Mathf.Infinity))
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

        if (Physics.Raycast(transform.position, Vector3.back, out hit, Mathf.Infinity))
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
  
    }
}

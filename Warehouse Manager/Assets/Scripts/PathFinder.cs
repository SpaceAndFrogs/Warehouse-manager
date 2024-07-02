using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public static PathFinder instance;

    void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }
    
    bool CheckIfEndTileIsCLosed(Tile[] endTileNeighbours)
    {
        for(int i = 0; i < endTileNeighbours.Length; i ++)
        {
            if(endTileNeighbours[i].walkable)
            {
                return false;
            }
        }

        return true;
    }

    public Tile[] FindPath(Tile startTile, Tile endTile)
    {
        if(CheckIfEndTileIsCLosed(endTile.neighborTiles.ToArray()))
        {
            return null;
        }


        List<Tile> openList = new List<Tile>();
        List<Tile> closeList = new List<Tile>();

        openList.Add(startTile);

        while(openList.Count > 0)
        {
            Tile currentTile = openList[0];

            for(int i = 1; i < openList.Count; i++)
            {
                if(openList[i].fCost < currentTile.fCost || (openList[i].fCost == currentTile.fCost && openList[i].hCost < currentTile.hCost))
                {
                    currentTile = openList[i];
                }
            }

            openList.Remove(currentTile);
            closeList.Add(currentTile);

            if(currentTile == endTile)
            {               
                return RetracePath(startTile, endTile).ToArray();
            }

            foreach(Tile tile in currentTile.neighborTiles)
            {
                if (!tile.walkable && tile != endTile)
                {
                    continue;
                }

                if(closeList.Contains(tile))
                {
                    continue;
                }

                float newMovementCostToNeighbour = currentTile.gCost + Vector3.Distance(currentTile.transform.position, tile.transform.position);

                if(newMovementCostToNeighbour < tile.gCost || !openList.Contains(tile))
                {
                    tile.gCost = newMovementCostToNeighbour;
                    tile.hCost = Vector3.Distance(endTile.transform.position, tile.transform.position);
                    tile.parentTile = currentTile;

                    openList.Add(tile);
                }
            }
        }

        return null;
    }

    List<Tile> RetracePath(Tile startTile, Tile endTile)
    {
        List<Tile> path = new List<Tile>();

        Tile currentTile = endTile;

        while(currentTile != startTile) 
        {
            path.Add(currentTile);
            currentTile = currentTile.parentTile;
        }

        path.Reverse();

        return path;
    }
}

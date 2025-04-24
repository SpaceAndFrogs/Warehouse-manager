using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    
    public bool IsBuildingSurrounded(Tile tileToCheck)
    {
        HashSet<Tile> visited = new HashSet<Tile>();
        Queue<Tile> queue = new Queue<Tile>();

        visited.Add(tileToCheck);
        queue.Enqueue(tileToCheck);

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            foreach (Tile neighbor in current.neighborTiles)
            {
                if (!visited.Contains(neighbor))
                {
                    if (neighbor.tileType == TileTypes.TileType.Floor || neighbor.tileType == TileTypes.TileType.Other)
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        // Sprawdzamy, czy obszar jest domknięty
        foreach (Tile tile in visited)
        {
            foreach (Tile neighbor in tile.neighborTiles)
            {
                if (!visited.Contains(neighbor) && neighbor.tileType != TileTypes.TileType.Wall && neighbor.tileType != TileTypes.TileType.Door)
                {
                    return false;
                }
            }
        }

        return true;
    }


    bool IsTileSurrounded(Tile tileToCheck, Tile startTile)
    {
        HashSet<Tile> visited = new HashSet<Tile>();
        Queue<Tile> queue = new Queue<Tile>();

        visited.Add(tileToCheck);
        queue.Enqueue(tileToCheck);

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            foreach (Tile neighbor in current.neighborTiles)
            {
                if(neighbor == startTile)
                    return false;

                if(visited.Contains(neighbor))
                    continue;

                if(!neighbor.walkable)
                {               
                    continue;
                }

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }
        return true;
    }

    public Queue<Tile> FindPath(Tile startTile, Tile endTile)
    {
        if(IsTileSurrounded(endTile, startTile))
        {
            return null;
        }

        if(endTile == startTile)
        {
            Queue<Tile> path = new Queue<Tile>();
            path.Enqueue(endTile);
            return path;
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
                return RetracePath(startTile, endTile);
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

    Queue<Tile> RetracePath(Tile startTile, Tile endTile)
    {
        Queue<Tile> path = new Queue<Tile>();

        Tile currentTile = endTile;

        while(currentTile != startTile) 
        {
            path.Enqueue(currentTile);
            currentTile = currentTile.parentTile;
        }

        path = new Queue<Tile>(path.Reverse());

        return path;
    }
}

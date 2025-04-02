using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Figuers : MonoBehaviour
{
    public static Figuers instance;
    public enum FiguersType {Contour,Fill}; 
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<Tile> MakeFiguer(Tile startTile, Tile endTile, FiguersType figuerType)
    {
        List<Tile> tiles = new List<Tile>();

        switch(figuerType)
        {
            case FiguersType.Contour:
            {
                tiles = MakeContour(startTile,endTile);
                break;
            }
            case FiguersType.Fill:
            {
                tiles = MakeFill(startTile,endTile);
                break;
            }
        }

        Debug.Log("Amount of tiles: " + tiles.Count);
        return tiles;
    }

    List<Tile> MakeFill(Tile startTile, Tile endTile)
    {
        List<Tile> tiles = new List<Tile>();
        Tile currentTileInCheck = startTile;
        List<Tile> tilesInZAxis = new List<Tile>();
        tilesInZAxis.Add(currentTileInCheck);
        while(endTile.transform.position.z != currentTileInCheck.transform.position.z)
        {
            if(endTile.transform.position.z > currentTileInCheck.transform.position.z)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, true);
                tilesInZAxis.Add(currentTileInCheck);
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, false);
                tilesInZAxis.Add(currentTileInCheck);
            }
        }

        

        for(int i = 0; i < tilesInZAxis.Count; i++)
        {
            currentTileInCheck = tilesInZAxis[i];
            tiles.Add(currentTileInCheck);

            while(endTile.transform.position.x != currentTileInCheck.transform.position.x)
            {
                if(endTile.transform.position.x > currentTileInCheck.transform.position.x)
                {
                    currentTileInCheck = CheckForNextTile(currentTileInCheck, true, true);
                    tiles.Add(currentTileInCheck);
                }
                else
                {
                    currentTileInCheck = CheckForNextTile(currentTileInCheck, true, false);
                    tiles.Add(currentTileInCheck);
                }
            }
        }

        return tiles;
    }

    List<Tile> MakeContour(Tile startTile, Tile endTile)
    {
        List<Tile> tiles = new List<Tile>();
        tiles.Add(startTile);
        Tile currentTileInCheck = startTile;
        while(endTile.transform.position.x != currentTileInCheck.transform.position.x)
        {
            if(endTile.transform.position.x > currentTileInCheck.transform.position.x)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, true, true);
                tiles.Add(currentTileInCheck);
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, true, false);
                tiles.Add(currentTileInCheck);
            }
        }

        while(endTile.transform.position.z != currentTileInCheck.transform.position.z)
        {
            if(endTile.transform.position.z > currentTileInCheck.transform.position.z)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, true);
                tiles.Add(currentTileInCheck);
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, false);
                tiles.Add(currentTileInCheck);
            }
        }

        currentTileInCheck = startTile;
        while(endTile.transform.position.z != currentTileInCheck.transform.position.z)
        {
            if(endTile.transform.position.z > currentTileInCheck.transform.position.z)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, true);
                tiles.Add(currentTileInCheck);
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, false, false);
                tiles.Add(currentTileInCheck);
            }
        }

        while(endTile.transform.position.x != currentTileInCheck.transform.position.x)
        {
            if(endTile.transform.position.x > currentTileInCheck.transform.position.x)
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, true, true);
                tiles.Add(currentTileInCheck);
            }
            else
            {
                currentTileInCheck = CheckForNextTile(currentTileInCheck, true, false);
                tiles.Add(currentTileInCheck);                 
            }
        }

        return tiles;
    }

    Tile CheckForNextTile(Tile startTile, bool checkX, bool checkMore)
    {
        Tile tile = null;

        for(int i = 0; i < startTile.neighborTiles.Count; i++)
        {
            if(checkX)
            {
                if(checkMore)
                {
                    if(startTile.transform.position.x < startTile.neighborTiles[i].transform.position.x && startTile.transform.position.z == startTile.neighborTiles[i].transform.position.z)
                    {
                        tile = startTile.neighborTiles[i];
                        break;
                    }
                }
                else
                {
                    if(startTile.transform.position.x > startTile.neighborTiles[i].transform.position.x && startTile.transform.position.z == startTile.neighborTiles[i].transform.position.z)
                    {
                        tile = startTile.neighborTiles[i];
                        break;
                    }
                }
            }
            else
            {
                if(checkMore)
                {
                    if(startTile.transform.position.z < startTile.neighborTiles[i].transform.position.z && startTile.transform.position.x == startTile.neighborTiles[i].transform.position.x)
                    {
                        tile = startTile.neighborTiles[i];
                        break;
                    }
                }
                else
                {
                    if(startTile.transform.position.z > startTile.neighborTiles[i].transform.position.z && startTile.transform.position.x == startTile.neighborTiles[i].transform.position.x)
                    {
                        tile = startTile.neighborTiles[i];
                        break;
                    }
                }
            }
        }

        return tile;
    }
}

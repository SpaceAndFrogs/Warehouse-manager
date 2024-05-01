using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField]
    TileTypes tileTypes;
    [SerializeField]
    MeshRenderer meshRenderer;
    [SerializeField]
    Rigidbody rB;

    public List<Tile> neighborTiles = new List<Tile>();
    public void ChangeTileType(float noiseSample)
    {
        for(int i = 0;i<tileTypes.tileTypesRanges.Count;i++)
        {
            if(noiseSample >= tileTypes.tileTypesRanges[i].tileRange.x && noiseSample < tileTypes.tileTypesRanges[i].tileRange.y)
            {
                Material material = meshRenderer.material;

                
                material.color = tileTypes.tileTypesRanges[i].color;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Button button = other.GetComponent<Button>();

        if (button != null)
        {
            other.gameObject.SetActive(false);
        }
    }


    public void AddTileToNeighborTiles(Tile tile)
    {
        if(neighborTiles.Contains(tile)) 
        { return; }
        neighborTiles.Add(tile);
    }

    void OnCollisionStay(Collision collision)
    {
        if (neighborTiles.Count == 8)
        { return; }

        Tile hittedTile = collision.gameObject.GetComponent<Tile>();

        if(hittedTile == null)
        { return; }

        AddTileToNeighborTiles(hittedTile);
    }
}

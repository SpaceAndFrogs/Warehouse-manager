using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    MapFragment mapFragmentPrefab;
    [SerializeField]
    List<MapFragment> mapFragments;
    [SerializeField]
    Noise noise;

    [Serializable]
    public class MapFragment
    {
        public float mapFragmentSize;
        public GameObject mapFragmentObject;
        public int amountOfTilesOnFragment;
        public Tile tile;
        public List<Tile> tiles;
        public const float tileHeigth = 0.01f;

        public MapFragment(float mapFragmentSize, GameObject mapFragmentObject, int amountOfTilesOnFragment, Tile tile, List<Tile> tiles)
        {
            this.mapFragmentSize = mapFragmentSize;
            this.mapFragmentObject = mapFragmentObject;
            this.tile = tile;
            this.amountOfTilesOnFragment = amountOfTilesOnFragment;
            this.tiles = tiles;


            SetSizeOfMapFragment();
            SetSizeOfTile();
        }

        [Serializable]
        public class Tile
        {
            public GameObject tileObject;

            public Tile(GameObject tileObject)
            {
                this.tileObject = tileObject;
            }
        }

        public void SetSizeOfMapFragment()
        {
            mapFragmentObject.transform.localScale = new Vector3(mapFragmentSize, 1, mapFragmentSize);
        }

        public void SetSizeOfTile()
        {
            float tileSize = mapFragmentSize / amountOfTilesOnFragment;

            tile.tileObject.transform.localScale = new Vector3(tileSize, 1, tileSize);
        }


    }

    [Serializable]
    public class Noise
    {
        public int size;
        public float scale;
        public List<List<float>> noiseSamples = new List<List<float>>();

        public void GenerateNoiseSamples()
        {
            noiseSamples.Clear();

            for (int y = 0; y < size; y++)
            {
                List<float> xRow = new List<float>();
                for (int x = 0; x < size; x++)
                {
                    float xCoord = (float)x / size * scale + Random.Range(0f, 100f);
                    float yCoord = (float)y / size * scale + Random.Range(0f, 100f);
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);

                    xRow.Add(sample);
                }
                noiseSamples.Add(xRow);
            }

        }

    }



    public void GenerateMapFragment(Vector3 positionToSpawn)
    {
        MapFragment mapFragment = new MapFragment(mapFragmentPrefab.mapFragmentSize, mapFragmentPrefab.mapFragmentObject, mapFragmentPrefab.amountOfTilesOnFragment, mapFragmentPrefab.tile, new List<MapFragment.Tile>());
        
        GameObject mapFragmentObject = Instantiate(mapFragment.mapFragmentObject,positionToSpawn,transform.rotation);
        mapFragment.mapFragmentObject = mapFragmentObject;

        GenerateTilesOnMapFragment(mapFragment);

        mapFragments.Add(mapFragment);
    }

    public void GenerateTilesOnMapFragment(MapFragment mapFragment)
    {
        float halfOfTileSize = mapFragment.tile.tileObject.GetComponent<MeshRenderer>().bounds.size.x/2;
        float halfOfFragmentSize = mapFragment.mapFragmentObject.GetComponent<MeshRenderer>().bounds.size.x / 2;
        Vector3 positionOfFragment = mapFragment.mapFragmentObject.transform.position;

        for (float x = positionOfFragment.x - halfOfFragmentSize + halfOfTileSize; x <= positionOfFragment.x + halfOfFragmentSize - halfOfTileSize; x += halfOfTileSize * 2)
        {
            for(float z = positionOfFragment.z - halfOfFragmentSize + halfOfTileSize; z <= positionOfFragment.z + halfOfFragmentSize - halfOfTileSize; z += halfOfTileSize * 2)
            {
                GameObject tileObject = Instantiate(mapFragment.tile.tileObject, new Vector3(x, mapFragment.mapFragmentObject.transform.position.y + MapFragment.tileHeigth,z), mapFragment.mapFragmentObject.transform.rotation);
                tileObject.transform.parent = mapFragment.mapFragmentObject.transform;

                MapFragment.Tile tile = new MapFragment.Tile(tileObject);
                mapFragment.tiles.Add(tile);
            }
        }
    }

    private void Start()
    {
        noise.GenerateNoiseSamples();
        GenerateMapFragment(Vector3.zero);
    }
}

using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static MapGenerator.MapFragment;
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
        public TileClass tile;
        public List<List<TileClass>> tiles;
        public const float tileHeigth = 0.01f;

        public MapFragment(float mapFragmentSize, GameObject mapFragmentObject, int amountOfTilesOnFragment, TileClass tile, List<List<TileClass>> tiles)
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
        public class TileClass
        {
            public Tile tileScript;

            public TileClass(Tile tileScript)
            {
                this.tileScript = tileScript;
            }
        }

        public void SetSizeOfMapFragment()
        {
            mapFragmentObject.transform.localScale = new Vector3(mapFragmentSize, 1, mapFragmentSize);
        }

        public void SetSizeOfTile()
        {
            float tileSize = mapFragmentSize / amountOfTilesOnFragment;

            tile.tileScript.gameObject.transform.localScale = new Vector3(tileSize, 1, tileSize);
        }

        public MapFragment GenerateMapFragment(Vector3 positionToSpawn, MapFragment mapFragmentPrefab, Transform transform)
        {
            MapFragment mapFragment = new MapFragment(mapFragmentPrefab.mapFragmentSize, mapFragmentPrefab.mapFragmentObject, mapFragmentPrefab.amountOfTilesOnFragment, mapFragmentPrefab.tile, new List<List<TileClass>>());

            GameObject mapFragmentObject = Instantiate(mapFragment.mapFragmentObject, positionToSpawn, transform.rotation);
            mapFragment.mapFragmentObject = mapFragmentObject;

            GenerateTilesOnMapFragment(mapFragment);

            return mapFragment;
        }

        public void GenerateTilesOnMapFragment(MapFragment mapFragment)
        {
            float halfOfTileSize = mapFragment.tile.tileScript.gameObject.GetComponent<MeshRenderer>().bounds.size.x / 2;
            float halfOfFragmentSize = mapFragment.mapFragmentObject.GetComponent<MeshRenderer>().bounds.size.x / 2;
            Vector3 positionOfFragment = mapFragment.mapFragmentObject.transform.position;

            for (float x = positionOfFragment.x - halfOfFragmentSize + halfOfTileSize; x <= positionOfFragment.x + halfOfFragmentSize - halfOfTileSize; x += halfOfTileSize * 2)
            {
                List<TileClass> tileRow = new List<TileClass>();

                for (float z = positionOfFragment.z - halfOfFragmentSize + halfOfTileSize; z <= positionOfFragment.z + halfOfFragmentSize - halfOfTileSize; z += halfOfTileSize * 2)
                {
                    Tile tileScript = Instantiate(mapFragment.tile.tileScript, new Vector3(x, mapFragment.mapFragmentObject.transform.position.y + tileHeigth, z), mapFragment.mapFragmentObject.transform.rotation);
                    tileScript.transform.parent = mapFragment.mapFragmentObject.transform;

                    TileClass tile = new TileClass(tileScript);
                    tileRow.Add(tile);
                }

                mapFragment.tiles.Add(tileRow);
            }
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
                    float xCoord = (float)x / size * scale;
                    float yCoord = (float)y / size * scale;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);

                    xRow.Add(sample);
                }
                noiseSamples.Add(xRow);
            }

        }
        public void SetTileTypes(List<List<TileClass>> tiles)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int y = 0; y < tiles.Count; y++)
                {
                    tiles[i][y].tileScript.ChangeTileType(noiseSamples[i][y]);
                }
            }
        }

    }

    private void Start()
    {
        noise.GenerateNoiseSamples();

        MapFragment fragment = mapFragmentPrefab.GenerateMapFragment(Vector3.zero, mapFragmentPrefab, transform);
        mapFragments.Add(fragment);
        noise.SetTileTypes(fragment.tiles);
    }
}

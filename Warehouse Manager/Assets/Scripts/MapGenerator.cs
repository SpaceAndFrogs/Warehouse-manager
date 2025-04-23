using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    MapFragment mapFragmentPrefab;
    [SerializeField]
    List<MapFragment> mapFragments;
    [SerializeField]
    Noise noise;

    [SerializeField]
    float cashForMapFragment = 1000f;
    [SerializeField]
    float mapFragmentCashMultiplier = 1.5f;
    bool firstFragment = true;

    public static MapGenerator instance { get; private set; }

    [Serializable]
    public class MapFragment
    {
        public float mapFragmentSize;
        public GameObject mapFragmentObject;
        public int amountOfTilesOnFragment;
        public TileClass tile;
        public List<List<TileClass>> tiles;
        public float tileHeigth = 0.01f;
        public List<NewMapFragmentButton> newMapFragmentButtons = new List<NewMapFragmentButton>();
        public Canvas canvasForButtons;
        public Button buttonPrefab;

        public MapFragment(float mapFragmentSize, GameObject mapFragmentObject, int amountOfTilesOnFragment, TileClass tile, List<List<TileClass>> tiles, Canvas canvasForButtons)
        {
            this.mapFragmentSize = mapFragmentSize;
            this.mapFragmentObject = mapFragmentObject;
            this.tile = tile;
            this.amountOfTilesOnFragment = amountOfTilesOnFragment;
            this.tiles = tiles;
            SetSizeOfMapFragment();
            SetSizeOfTile();
            this.canvasForButtons = canvasForButtons;
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

        [Serializable]
        public class NewMapFragmentButton
        {
            public enum DirectionsOfButtons { Left, Right, Up, Down };
            public DirectionsOfButtons directionOfButton;
            public Button button;

            public NewMapFragmentButton(DirectionsOfButtons direction, Button button)
            {
                directionOfButton = direction;
                this.button = button;
                TextMeshProUGUI textMesh = button.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                textMesh.text = direction.ToString();
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
            float offSetX = UnityEngine.Random.Range(0f, 10000f);
            float offSetY = UnityEngine.Random.Range(0f, 10000f);
            for (int y = 0; y < size; y++)
            {
                List<float> xRow = new List<float>();
                for (int x = 0; x < size; x++)
                {
                    float xCoord = (float)x / size * scale + offSetX;
                    float yCoord = (float)y / size * scale + offSetY;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    sample = Mathf.Clamp01(sample);

                    if (sample > 1f || sample < 0)
                        Debug.Log("Sample: " + sample);

                    xRow.Add(sample);
                }
                noiseSamples.Add(xRow);

            }

        }
        public void SetTileTypes(List<List<MapFragment.TileClass>> tiles)
        {

            for (int i = 0; i < noiseSamples.Count; i++)
            {
                for (int j = 0; j < noiseSamples.Count; j++)
                {

                    tiles[i][j].tileScript.SetTileType(noiseSamples[i][j]);

                }
            }            
        }

    }

    public int GetAmountOfAllTiles()
    {
        int amountOfTiles = 0;

        for(int i = 0; i < mapFragments.Count; i ++)
        {
            amountOfTiles += mapFragments[i].tiles.Count;
        }

        return amountOfTiles;
    }

    public void AddListenerToButton(MapFragment mapFragment, MapFragment mapPrefab, MapFragment.NewMapFragmentButton newMapFragmentButton)
    {
        float lengthOfFragmentSide = mapFragment.mapFragmentObject.GetComponent<MeshRenderer>().bounds.size.x;
        Vector3 positionToSpawnNewFragment = mapFragment.mapFragmentObject.transform.position;

        switch (newMapFragmentButton.directionOfButton)
        {
            case MapFragment.NewMapFragmentButton.DirectionsOfButtons.Left:
                {
                    positionToSpawnNewFragment -= new Vector3(lengthOfFragmentSide, 0, 0);
                    break;
                }
            case MapFragment.NewMapFragmentButton.DirectionsOfButtons.Right:
                {
                    positionToSpawnNewFragment += new Vector3(lengthOfFragmentSide, 0, 0);
                    break;
                }
            case MapFragment.NewMapFragmentButton.DirectionsOfButtons.Up:
                {
                    positionToSpawnNewFragment += new Vector3(0, 0, lengthOfFragmentSide);
                    break;
                }
            case MapFragment.NewMapFragmentButton.DirectionsOfButtons.Down:
                {
                    positionToSpawnNewFragment -= new Vector3(0, 0, lengthOfFragmentSide);
                    break;
                }
        }

        newMapFragmentButton.button.onClick.AddListener(() => GenerateMapFragment(positionToSpawnNewFragment, mapPrefab, mapFragment.mapFragmentObject.transform));
    }

    public void GenerateNewFragmentsButtons(MapFragment mapFragmentPrefab)
    {
        MapFragment mapFragment = mapFragments[mapFragments.Count - 1];
        Canvas buttonsCanvas = Instantiate(mapFragmentPrefab.canvasForButtons, mapFragment.mapFragmentObject.transform.position, Quaternion.Euler(90f, 0f, 0f));

        for (int i = 0; i < 4; i++)
        {
            Button button = Instantiate(mapFragmentPrefab.buttonPrefab, buttonsCanvas.transform);
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            MapFragment.NewMapFragmentButton.DirectionsOfButtons direction = MapFragment.NewMapFragmentButton.DirectionsOfButtons.Up;
            switch (i)
            {
                case 0:
                    {
                        rectTransform.anchoredPosition3D = new Vector3(0f, 30f, 0f);
                        rectTransform.anchorMin = new Vector2(0.5f, 1f);
                        rectTransform.anchorMax = new Vector2(0.5f, 1f);
                        direction = MapFragment.NewMapFragmentButton.DirectionsOfButtons.Up;
                        break;
                    }
                case 1:
                    {
                        rectTransform.anchoredPosition3D = new Vector3(0f, 0f, 0f);
                        rectTransform.anchorMin = new Vector2(0.5f, 0f);
                        rectTransform.anchorMax = new Vector2(0.5f, 0f);
                        direction = MapFragment.NewMapFragmentButton.DirectionsOfButtons.Down;
                        break;
                    }
                case 2:
                    {
                        rectTransform.anchoredPosition3D = new Vector3(30f, 0f, 0f);
                        rectTransform.anchorMin = new Vector2(1f, 0.5f);
                        rectTransform.anchorMax = new Vector2(1f, 0.5f);
                        rectTransform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                        direction = MapFragment.NewMapFragmentButton.DirectionsOfButtons.Right;
                        break;
                    }
                case 3:
                    {
                        rectTransform.anchoredPosition3D = new Vector3(-30f, 0f, 0f);
                        rectTransform.anchorMin = new Vector2(0f, 0.5f);
                        rectTransform.anchorMax = new Vector2(0f, 0.5f);
                        rectTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                        direction = MapFragment.NewMapFragmentButton.DirectionsOfButtons.Left;
                        break;
                    }
            }

            MapFragment.NewMapFragmentButton newMapFragmentButton = new MapFragment.NewMapFragmentButton(direction, button);
            mapFragment.newMapFragmentButtons.Add(newMapFragmentButton);

            AddListenerToButton(mapFragment, mapFragmentPrefab, newMapFragmentButton);
        }
    }

    public void GenerateMapFragment(Vector3 positionToSpawn, MapFragment mapFragmentPrefab, Transform transform)
    {
        if(CashManager.instance.AmountOfCash() < cashForMapFragment)
        {
            return;
        }

        if (!firstFragment)
        {
            CashManager.instance.SpendCash(cashForMapFragment);
            cashForMapFragment *= mapFragmentCashMultiplier;
        }
        
        firstFragment = false;
        MapFragment mapFragment = new MapFragment(mapFragmentPrefab.mapFragmentSize, mapFragmentPrefab.mapFragmentObject, mapFragmentPrefab.amountOfTilesOnFragment, mapFragmentPrefab.tile, new List<List<MapFragment.TileClass>>(), mapFragmentPrefab.canvasForButtons);

        GameObject mapFragmentObject = Instantiate(mapFragment.mapFragmentObject, positionToSpawn, transform.rotation);
        mapFragment.mapFragmentObject = mapFragmentObject;

        mapFragments.Add(mapFragment);

        MapFragmentScript mapFragmentScript = mapFragmentObject.GetComponent<MapFragmentScript>();

        noise.GenerateNoiseSamples();

        mapFragmentScript.ChangeFragmentTexture(noise.noiseSamples);

        GenerateTilesOnMapFragment();

        noise.SetTileTypes(mapFragments[mapFragments.Count - 1].tiles);

        GenerateNewFragmentsButtons(mapFragmentPrefab);

        SetCashIndicatorsForButtons();

    }

    void SetCashIndicatorsForButtons()
    {
        for (int i = 0; i < mapFragments.Count; i++)
        {
            for (int j = 0; j < mapFragments[i].newMapFragmentButtons.Count; j++)
            {
                MapFragment.NewMapFragmentButton button = mapFragments[i].newMapFragmentButtons[j];
                TextMeshProUGUI textMesh = button.button.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                textMesh.text = cashForMapFragment.ToString() + "$";
            }
        }
    }

    public void GenerateTilesOnMapFragment()
    {
        MapFragment mapFragment = mapFragments[mapFragments.Count - 1];
        float halfOfFragmentSize = mapFragmentPrefab.mapFragmentObject.GetComponent<MeshRenderer>().bounds.size.x / 2;
        float halfOfTileSize = halfOfFragmentSize * mapFragmentPrefab.tile.tileScript.transform.localScale.x;
        Vector3 positionOfFragment = mapFragment.mapFragmentObject.transform.position;
        float x = positionOfFragment.x - halfOfFragmentSize + halfOfTileSize;
        
        

        for (int i = 0; i < mapFragment.amountOfTilesOnFragment; i++ )
        {
            float z = positionOfFragment.z - halfOfFragmentSize + halfOfTileSize;
            List<MapFragment.TileClass> tileRow = new List<MapFragment.TileClass>();

            for (int y = 0; y < mapFragment.amountOfTilesOnFragment; y++)
            {
                Tile tileScript = Instantiate(mapFragment.tile.tileScript, new Vector3(x, mapFragment.mapFragmentObject.transform.position.y + mapFragment.tileHeigth, z), transform.rotation);

                MapFragment.TileClass tile = new MapFragment.TileClass(tileScript);
                tile.tileScript.mapFragmentScript = mapFragment.mapFragmentObject.GetComponent<MapFragmentScript>();
                tile.tileScript.tileCords = new Vector2(i,y);
                tileRow.Add(tile);

                z += halfOfTileSize * 2;
            }

            mapFragment.tiles.Add(tileRow);
            x += halfOfTileSize * 2;
        }
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        GenerateMapFragment(Vector3.zero, mapFragmentPrefab, transform);        
    }
}

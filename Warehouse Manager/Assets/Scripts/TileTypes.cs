using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileType", menuName = "Tile Types", order = 1)]
public class TileTypes : ScriptableObject
{
    public enum TileType {Ground, Water, Rocks, Floor, Wall, Door, Other};

    public List<TileTypesRanges> tileTypesRanges = new List<TileTypesRanges>();

    [Serializable]
    public class TileTypesRanges
    {
        public TileType tileType;
        public Vector2 tileRange;
        public Color color;
        public bool walkable;
    }

}



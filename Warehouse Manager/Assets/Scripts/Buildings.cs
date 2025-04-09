using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Buildings", order = 1)]
public class Buildings : ScriptableObject
{
    public enum BuildingType {None, Floor, Wall, PackStation, Rack, PickStash, OrdersStation,Door}

    public List<Building> buildings = new List<Building>();

    [Serializable]
    public class Building
    {
        public GameObject buildingObject;
        public Sprite buildingSprite;
        public BuildingType buildingType;
        public string nameOfButton;
        public float buildingTime;
        public float cost;

    }
}

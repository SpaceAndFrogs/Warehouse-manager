using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ItemsData", menuName = "Items", order = 1)]
public class Items : ScriptableObject
{
    public enum ItemType {Test1,Test2}

    public List<Item> items = new List<Item>();

    [Serializable]
    public class Item
    {
        public ItemType itemType;
        public Sprite sprite;
        public Vector2 minMaxBuyPrice;
        public Vector2 minMaxPercentageOfDemandPrice;
        public string name;
    }
}

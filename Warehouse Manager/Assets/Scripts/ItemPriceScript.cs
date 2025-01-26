using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemPriceScript : MonoBehaviour
{
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI buyPrice;
    public TMP_InputField sellPriceInput;
    public TextMeshProUGUI demandPrice;
    public TextMeshProUGUI growthRate;
    public Button lockSellingButton;
    public bool sellingLocked = false;
    public Items.ItemType itemType;
}

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

    private void OnEnable()
    {
        SavingManager.OnSave += OnSave;
    }

    void OnSave()
    {
        SavingManager.instance.saveData.itemPrices.Add(new SaveData.ItemPricesData(itemName.text, buyPrice.text, sellPriceInput.text, demandPrice.text, growthRate.text, sellingLocked, itemType.ToString()));
    }
}

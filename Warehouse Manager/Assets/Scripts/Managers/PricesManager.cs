using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PricesManager : MonoBehaviour
{
    [SerializeField]
    Items itemsData;
    [SerializeField]
    ItemPriceScript itemPricePrefab;
    [SerializeField]
    PricesPanel pricesPanel;
    public List<ItemPriceScript> itemPricesScripts = new List<ItemPriceScript>();
    public static PricesManager instance;
    List<int> amountOfItems = new List<int>(); 

    void Awake()
    {
        MakeSingelton();
    }

    void MakeSingelton()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }else
        {
            instance = this;
            return;
        }
    }

    void Start()
    {
        pricesPanel.pricesPanelObject.SetActive(true);
        MakeItemPrices();
        AddListeners();
        pricesPanel.pricesPanelObject.SetActive(false);

        MakeListOfItems();

        StartCoroutine(UpdateDemandPrices());
    }

    void MakeListOfItems()
    {
        for(int i = 0; i < itemsData.items.Count; i++)
        {
            amountOfItems.Add(0);
        }
    }

    void UpdateGrowthRate(ItemPriceScript itemPriceScript, bool growing)
    {
        if(growing)
        {
            itemPriceScript.growthRate.text = "Yes";
        }
        else
        {
            itemPriceScript.growthRate.text = "No";
        }
    }

    public void UpdateAmountOfItemsInSale(int currentAmountOfItem, int indexOfItem)
    {
        amountOfItems[indexOfItem] = currentAmountOfItem;
    }

    IEnumerator UpdateDemandPrices()
    {
        while(true)
        {
            

            int amountOfAllItems = 0;
            for(int i = 0; i < amountOfItems.Count; i++)
            {
                if(itemPricesScripts[i].sellingLocked)
                {
                    continue;
                }

                amountOfAllItems += amountOfItems[i];
            }

            if(amountOfAllItems == 0)
            {
                yield return new WaitForSeconds(TimeManager.instance.GetOneWeek());
                continue;
            }

            pricesPanel.pricesPanelObject.SetActive(true);

            List<ItemPriceScript> checkerPrices = itemPricesScripts;

            for(int i = 0; i < amountOfItems.Count; i++)
            {
                float percentage = (amountOfItems[i] * 100) / amountOfAllItems;
                if(percentage > 30)
                {
                    for(int j = 0; j < itemPricesScripts.Count; j++)
                    {
                        if(itemPricesScripts[j].sellingLocked)
                        {
                            continue;
                        }

                        float itemPrice = float.Parse(itemPricesScripts[j].demandPrice.text.Remove(itemPricesScripts[j].demandPrice.text.Length -1));
                        if(i == j)
                        {            
                            itemPricesScripts[j].demandPrice.text = (itemPrice - 0.1).ToString("F2") + "$";
                            continue;
                        }

                        itemPricesScripts[j].demandPrice.text = (itemPrice + 0.05).ToString("F2") + "$";
                    }
                }
            }

            float newItemPrice = 0;
            float oldItemPrice = 0;

            for(int i = 0; i < itemPricesScripts.Count; i++)
            {
                newItemPrice = float.Parse(itemPricesScripts[i].demandPrice.text.Remove(itemPricesScripts[i].demandPrice.text.Length -1));
                oldItemPrice = float.Parse(checkerPrices[i].demandPrice.text.Remove(checkerPrices[i].demandPrice.text.Length -1));
                if(newItemPrice > oldItemPrice)
                {
                    UpdateGrowthRate(itemPricesScripts[i], true);
                }
                else if(newItemPrice < oldItemPrice)
                {
                    UpdateGrowthRate(itemPricesScripts[i], false);
                }
            }

            if(!pricesPanel.isGoingToPanel)
            {
                pricesPanel.pricesPanelObject.SetActive(false);
            }

            yield return new WaitForSeconds(TimeManager.instance.GetOneWeek());
        }
    }

    void MakeItemPrices()
    {
        for(int i = 0; i < itemsData.items.Count; i++)
        {
            ItemPriceScript newItemPriceScript = Instantiate(itemPricePrefab.gameObject,pricesPanel.itemPricesListContent.transform).GetComponent<ItemPriceScript>();
            newItemPriceScript.itemName.text = itemsData.items[i].name;
            float priceOfItem = UnityEngine.Random.Range(itemsData.items[i].minMaxBuyPrice.x, itemsData.items[i].minMaxBuyPrice.y);
            newItemPriceScript.buyPrice.text = priceOfItem.ToString("F2") + "$";
            float percentageOfDemand = UnityEngine.Random.Range(itemsData.items[i].minMaxPercentageOfDemandPrice.x, itemsData.items[i].minMaxPercentageOfDemandPrice.y)/100;           
            newItemPriceScript.demandPrice.text = (priceOfItem + (priceOfItem*percentageOfDemand)).ToString("F2") + "$";
            newItemPriceScript.sellPriceInput.text = (priceOfItem + (priceOfItem*percentageOfDemand)).ToString("F2") + "$";
            newItemPriceScript.sellPriceInput.onValueChanged.AddListener(newItemPriceScript.OnValueChanged);
            newItemPriceScript.lockSellingButton.onClick.AddListener(() => LockSelling(newItemPriceScript));
            newItemPriceScript.itemType = itemsData.items[i].itemType;
            itemPricesScripts.Add(newItemPriceScript);
        }
        
    }

    void AddListeners()
    {
        pricesPanel.closePanelButton.onClick.AddListener(() => GoToPanel(false));
        pricesPanel.goToPanelButton.onClick.AddListener(() => GoToPanel(true));
    }

    void GoToPanel(bool toPanel)
    {
        pricesPanel.isGoingToPanel = toPanel;

        if(toPanel)
        {
            pricesPanel.goToPanelButton.gameObject.SetActive(false);
            pricesPanel.pricesPanelObject.gameObject.SetActive(true);
            
            return;
        }else
        {
            pricesPanel.goToPanelButton.gameObject.SetActive(true);
            pricesPanel.pricesPanelObject.gameObject.SetActive(false);

            return;
        }
    }

    void LockSelling(ItemPriceScript itemPriceScript)
    {
        if(itemPriceScript.sellingLocked)
        {
            itemPriceScript.lockSellingButton.GetComponentInChildren<TextMeshProUGUI>().text = "No";
            itemPriceScript.sellingLocked = false;
            return;
        }else
        {
            itemPriceScript.lockSellingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Yes";
            itemPriceScript.sellingLocked = true;
            return;
        }
    }

    void OnEnable()
    {
        SavingManager.OnPricesLoad += LoadPrices;   
    }

    void OnDisable()
    {
        SavingManager.OnPricesLoad -= LoadPrices;
    }

    void LoadPrices()
    {
        for(int i = 0; i < itemPricesScripts.Count; i++)
        {
            itemPricesScripts[i].buyPrice.text = SavingManager.instance.saveData.itemPrices[i].buyPrice;
            itemPricesScripts[i].demandPrice.text = SavingManager.instance.saveData.itemPrices[i].demandPrice;
            itemPricesScripts[i].sellPriceInput.text = SavingManager.instance.saveData.itemPrices[i].sellPrice;
            itemPricesScripts[i].itemName.text = SavingManager.instance.saveData.itemPrices[i].itemName;
            itemPricesScripts[i].itemType = (Items.ItemType)Enum.Parse(typeof(Items.ItemType), SavingManager.instance.saveData.itemPrices[i].itemType);
            itemPricesScripts[i].sellingLocked = SavingManager.instance.saveData.itemPrices[i].sellingLocked;

            if(itemPricesScripts[i].sellingLocked)
            {
                itemPricesScripts[i].lockSellingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Yes";
            }else
            {
                itemPricesScripts[i].lockSellingButton.GetComponentInChildren<TextMeshProUGUI>().text = "No";
            }
        }
    }

    [System.Serializable]
    public class PricesPanel
    {
        public GameObject pricesPanelObject;
        public Button goToPanelButton;
        public Button closePanelButton;
        public GameObject itemPricesListContent;
        public bool isGoingToPanel = false;
    }
}

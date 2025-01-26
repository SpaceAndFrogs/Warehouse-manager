using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class RackPanel : MonoBehaviour
{
    Rack currentRack = null;
    [SerializeField]
    Panel panel;
    [SerializeField]
    Button buttonPrefab;
    [SerializeField]
    Items items;

    [Serializable]
    public class Panel
    {
        public Transform itemsCanvasTransform;
        public GameObject mainPanelObject;
        public GameObject choosePanelObject;
        public TextMeshProUGUI itemOnRack;
        public TextMeshProUGUI currentAmountOfItemsOnRack;

        public TMP_InputField desiredAmountOfItemsOnRack;
        public TextMeshProUGUI maxAmountOfItemsOnRack;
    }

    void SetItemButtonsOnPanel()
    {
        panel.choosePanelObject.SetActive(true);
        for(int i = 0; i < items.items.Count; i ++)
        {
            int index = i;
            Button newButton = Instantiate(buttonPrefab,panel.itemsCanvasTransform);
            newButton.name = "Button: " + items.items[i].name;
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = items.items[i].name;
            newButton.onClick.AddListener(() => SetCurrentItem(index));
        }
        panel.choosePanelObject.SetActive(false);
    }

    public void SetCurrentItem(int index)
    {
        currentRack.itemOnRack = items.items[index];
        UpdateRackPanel();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                Rack rack = hitInfo.collider.gameObject.GetComponent<Rack>();

                if (rack == null)
                    return;

                if(currentRack == rack)
                    return;


                currentRack = rack;

                OpenPanelForRack();
            }

        }
        UpdateRackPanel();
    }

    void Start()
    {
        panel.desiredAmountOfItemsOnRack.onValueChanged.AddListener(ChangeDesiredAmount);
        SetItemButtonsOnPanel();
    }

    void OpenPanelForRack()
    {
        UpdateRackPanel();
        panel.mainPanelObject.SetActive(true);
    }

    void UpdateRackPanel()
    {
        if(currentRack == null)
        return;
        panel.currentAmountOfItemsOnRack.text = currentRack.amountOfItems.ToString();
        panel.desiredAmountOfItemsOnRack.text = currentRack.desiredAmountOfItems.ToString();
        panel.maxAmountOfItemsOnRack.text = currentRack.maxAmountOfItems.ToString();
        panel.itemOnRack.text = currentRack.itemOnRack.name;
    }

    public void ClosePanel()
    {
        currentRack = null;
        panel.mainPanelObject.SetActive(false);
    }

    public void OpenChoosePanel(bool openPanel)
    {
        if(openPanel)
        {
            panel.mainPanelObject.SetActive(false);
            panel.choosePanelObject.SetActive(true);
        }else
        {
            panel.mainPanelObject.SetActive(true);
            panel.choosePanelObject.SetActive(false);
        }
        
    }

    void ChangeDesiredAmount(string newText)
    {
        int desiredAmount = int.Parse(newText);
        int itemIndex = -1;

        for(int i = 0; i < items.items.Count; i++)
        {
            if(items.items[i].itemType == currentRack.itemOnRack.itemType)
            {
                itemIndex = i;
                break;
            }
        }
        currentRack.desiredAmountOfItems = desiredAmount;
        PricesManager.instance.UpdateAmountOfItemsInSale(desiredAmount, itemIndex);
    }
}

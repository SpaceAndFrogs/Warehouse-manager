using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class RackPanel : MonoBehaviour
{
    Rack currentRack = null;
    [SerializeField]
    Panel panel;

    [Serializable]
    public class Panel
    {
        public GameObject panelObject;
        public TextMeshProUGUI itemOnRack;
        public TextMeshProUGUI currentAmountOfItemsOnRack;

        public TMP_InputField desiredAmountOfItemsOnRack;
        public TextMeshProUGUI maxAmountOfItemsOnRack;
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
    }

    void Start()
    {
        panel.desiredAmountOfItemsOnRack.onValueChanged.AddListener(ChangeDesiredAmount);
    }

    void OpenPanelForRack()
    {
        panel.currentAmountOfItemsOnRack.text = currentRack.amountOfItems.ToString();
        panel.desiredAmountOfItemsOnRack.text = currentRack.desiredAmountOfItems.ToString();
        panel.maxAmountOfItemsOnRack.text = currentRack.maxAmountOfItems.ToString();
        panel.itemOnRack.text = currentRack.itemOnRack.name;
        panel.panelObject.SetActive(true);
    }

    public void ClosePanel()
    {
        currentRack = null;
        panel.panelObject.SetActive(false);
    }

    void ChangeDesiredAmount(string newText)
    {
        currentRack.desiredAmountOfItems = int.Parse(newText);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CashManager : MonoBehaviour
{
    float currentCash;
    [SerializeField]
    float startCash;

    [SerializeField]
    TextMeshProUGUI cashCounter;

    public static CashManager instance = null;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentCash = startCash;
        UpdateCounter();
    }

    public void SpendCash(float cashToSpend)
    {
        currentCash -= cashToSpend;
        UpdateCounter();

        if(currentCash < 0)
        {
            //Check if player can get loan if not player loses
        }
    }

    public void GetCash(float cashToGet)
    {
        currentCash += cashToGet;
        UpdateCounter();
    }

    public float AmountOfCash()
    {
        return currentCash;
    }

    void UpdateCounter()
    {
        cashCounter.text = currentCash.ToString() + "$";
    }
}

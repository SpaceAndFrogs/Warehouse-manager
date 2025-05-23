using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Loan : MonoBehaviour
{
    public Button payOffButton;
    public TextMeshProUGUI cashToPayOff;
    public TextMeshProUGUI installment;
    public int index = -1;

    private void OnEnable()
    {
        SavingManager.OnSave += OnSave;
    }

    void OnSave()
    {
        SavingManager.instance.saveData.loans.Add(new SaveData.LoansData(cashToPayOff.text,installment.text));
    }
}

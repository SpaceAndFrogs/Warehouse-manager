using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LoanData", menuName = "Loans", order = 1)]
public class LoanData : ScriptableObject
{
    public int timeToPayOff;
    public float amountOfCashAtStartOfGame;
    public float percentage;
}

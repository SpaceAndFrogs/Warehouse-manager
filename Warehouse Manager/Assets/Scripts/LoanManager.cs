using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoanManager : MonoBehaviour
{
    [SerializeField]
    int amountOfPossibleLoans;
    [SerializeField]
    LoanData loanData;
    List<Loan> takenLoans = new List<Loan>();


    private void Start()
    {
        StartCoroutine(PayOffInstallments());
    }
    public void TakeLoan()
    {
        if(amountOfPossibleLoans == takenLoans.Count)
        {
            //Player loses
            return;
        }
        float amountOfCashFromLoan = loanData.amountOfCashAtStartOfGame;
        Loan newLoan = new Loan(loanData.timeToPayOff, amountOfCashFromLoan, loanData.percentage);
        CashManager.instance.GetCash(amountOfCashFromLoan);
        takenLoans.Add(newLoan);
    }

    IEnumerator PayOffInstallments()
    {
        while(true)
        {
            for(int i = takenLoans.Count - 1; i >= 0;i--) 
            {
                CashManager.instance.SpendCash(takenLoans[i].installment);
                takenLoans[i].amountOfCashToPayOff -= takenLoans[i].installment;
                if(takenLoans[i].amountOfCashToPayOff <= 0)
                {
                    RemoveLoanFromList(i);
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    public void PayOffLoan(int index)
    {
        CashManager.instance.SpendCash(takenLoans[index].amountOfCashToPayOff);
        RemoveLoanFromList(index);
    }

    public void PayOffMultipleLoans()
    {
        int indexOfLastLoanToPayOff = 0;
        for(int i = 0; i < takenLoans.Count; i++)
        {
            if(takenLoans[i].amountOfCashToPayOff >= CashManager.instance.AmountOfCash())
            {
                indexOfLastLoanToPayOff = i;
                CashManager.instance.SpendCash(takenLoans[i].amountOfCashToPayOff);
            }else
            {
                break;
            }
        }

        for(int i = indexOfLastLoanToPayOff; i >= 0; i--)
        {
            RemoveLoanFromList(i);
        }
    }

    void RemoveLoanFromList(int index)
    {
        takenLoans.RemoveAt(index);
    }

    public class Loan
    {
        public int timeToPayOffLoan;
        public float amountOfCashFromLoan;
        public float percentageOfLoan;
        public float amountOfCashToPayOff;
        public float installment;

        public Loan(int timeToPayOffLoan, float amountOfCashFromLoan, float percentageOfLoan)
        {
            this.timeToPayOffLoan = timeToPayOffLoan;
            this.amountOfCashFromLoan = amountOfCashFromLoan;
            this.percentageOfLoan = percentageOfLoan;
            amountOfCashToPayOff = amountOfCashFromLoan + amountOfCashFromLoan * (percentageOfLoan / 100);
            installment = amountOfCashToPayOff / timeToPayOffLoan;
        }
    }
}

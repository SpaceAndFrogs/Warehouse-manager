using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoanManager : MonoBehaviour
{
    [SerializeField]
    int amountOfPossibleLoans;
    [SerializeField]
    LoanData loanData;
    [SerializeField]
    LoanPanel loanPanel;
    List<LoanClass> takenLoans = new List<LoanClass>();
    [SerializeField]
    string cashToPayOffAllText;
    [SerializeField]
    string allInstalmentsText;
    [SerializeField]
    Loan loanScript = null;
    
    private void Start()
    {
        StartCoroutine(PayOffInstallments());
        AddListeners();
        UpdateCounters();
    }
    void AddListeners()
    {
        loanPanel.panel.SetActive(true);

        loanPanel.goToPanelButton.onClick.AddListener(() => GoToPanel(true));
        loanPanel.closePanelButton.onClick.AddListener(() => GoToPanel(false));
        loanPanel.takeLoan.onClick.AddListener(() => TakeLoan());
        loanPanel.payOffMultipleLoansButton.onClick.AddListener(() => PayOffMultipleLoans());

        loanPanel.panel.SetActive(false);
    }
    void AddListenerToLoan(Loan newLoanScript)
    {
        newLoanScript.payOffButton.onClick.AddListener(() => PayOffLoan(newLoanScript.index));
    }
    public void GoToPanel(bool toPanel)
    {
        if(toPanel)
        {
            loanPanel.panel.SetActive(true);
            loanPanel.goToPanelButton.gameObject.SetActive(false);
            UpdateCounters();
        }else
        {
            loanPanel.panel.SetActive(false);
            loanPanel.goToPanelButton.gameObject.SetActive(true);
        }
        
    }
    public void TakeLoan()
    {
        if(amountOfPossibleLoans == takenLoans.Count)
        {
            //Player loses
            return;
        }
        float amountOfCashFromLoan = loanData.amountOfCashAtStartOfGame;
        Loan newLoanScript = Instantiate(loanScript.gameObject,loanPanel.contentList.transform).GetComponent<Loan>();
        newLoanScript.index = takenLoans.Count;
        AddListenerToLoan(newLoanScript);
        LoanClass newLoan = new LoanClass(loanData.timeToPayOff, amountOfCashFromLoan, loanData.percentage,newLoanScript);
        CashManager.instance.GetCash(amountOfCashFromLoan);
        takenLoans.Add(newLoan);
        UpdateCounters();
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

            UpdateCounters();
            yield return new WaitForSeconds(TimeManager.instance.GetOneWeek());
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
        Loan loan = takenLoans[index].loanScript;
        takenLoans.RemoveAt(index);
        Destroy(loan.gameObject);
        UpdateCounters();
    }

    void UpdateCounters()
    {
        float allCashToPayOff = 0;
        float allInstallments = 0;
        for(int i = 0; i < takenLoans.Count; i++)
        {
            takenLoans[i].loanScript.cashToPayOff.text = takenLoans[i].amountOfCashToPayOff.ToString();
            allCashToPayOff += takenLoans[i].amountOfCashToPayOff;
            allInstallments += takenLoans[i].installment;
        }

        loanPanel.cashToPayOffAllLoans.text = cashToPayOffAllText + " " + allCashToPayOff.ToString() + "$";
        loanPanel.allInstalments.text = allInstallments + " " + allInstallments.ToString() + "$"; 

    }

    public class LoanClass
    {
        public int timeToPayOffLoan;
        public float amountOfCashFromLoan;
        public float percentageOfLoan;
        public float amountOfCashToPayOff;
        public float installment;
        public Loan loanScript;

        public LoanClass(int timeToPayOffLoan, float amountOfCashFromLoan, float percentageOfLoan, Loan loanScript)
        {
            this.timeToPayOffLoan = timeToPayOffLoan;
            this.amountOfCashFromLoan = amountOfCashFromLoan;
            this.percentageOfLoan = percentageOfLoan;
            amountOfCashToPayOff = amountOfCashFromLoan + amountOfCashFromLoan * (percentageOfLoan / 100);
            installment = amountOfCashToPayOff / timeToPayOffLoan;
            this.loanScript = loanScript;
        }

    }

    [System.Serializable]
    public class LoanPanel
    {
        public GameObject panelObject;
        public Button goToPanelButton;
        public Button closePanelButton;
        public GameObject contentList;
        public Button payOffMultipleLoansButton;
        public Button takeLoan;
        public TextMeshProUGUI cashToPayOffAllLoans;
        public TextMeshProUGUI allInstalments;
        public GameObject panel;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkerRecordScript : MonoBehaviour
{
    public TextMeshProUGUI nameTMP;
    public TextMeshProUGUI moveSpeedTMP;
    public TextMeshProUGUI workSpeedTMP;
    public TextMeshProUGUI salaryTMP;
    public TextMeshProUGUI workerTypeTMP;
    public Button actionButton;
    public WorkerBase worker = null;
}

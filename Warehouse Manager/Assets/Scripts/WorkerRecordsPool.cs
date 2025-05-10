using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;

public class WorkerRecordsPool : MonoBehaviour
{
    public static WorkerRecordsPool instance;
    [SerializeField]
    List<string> lastNames = new List<string>();
    [SerializeField]
    List<string> firstNames = new List<string>();
    [SerializeField]
    TextAsset firstNamesFile;
    [SerializeField]
    TextAsset lastNamesFile;
    [SerializeField]
    List<Record> recordsInPool = new List<Record>();

    public (WorkerRecordScript, Worker.Stats) GetRecord(WorkerData.WorkerType workerType, bool candidate)
    {
        foreach(Record record in recordsInPool)
        {
            if(record.workerType == workerType && candidate == record.candidate)
            {
                return record.GetRecord(firstNames, lastNames);
            }
        }

        return (null,null);
    }

    public void ReturnRecord(WorkerRecordScript workerRecordScript,bool candidate)
    {
        foreach (Record record in recordsInPool)
        {
            WorkerData.WorkerType workerType = (WorkerData.WorkerType)Enum.Parse(typeof(WorkerData.WorkerType), workerRecordScript.workerTypeTMP.text);
            if (record.workerType == workerType && candidate == record.candidate)
            {
                record.ReturnRecord(workerRecordScript);
            }
        }
    }

    void LoadNames()
    {
        JObject jsonObj = JObject.Parse(firstNamesFile.text);
        JArray namesArray = (JArray)jsonObj["male_names"];
        firstNames = namesArray.ToObject<List<string>>();

        jsonObj = JObject.Parse(lastNamesFile.text);
        namesArray = (JArray)jsonObj["male_surnames"];
        lastNames = namesArray.ToObject<List<string>>();
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }else
        {
            Destroy(gameObject);
        }

        LoadNames();
    }  
    
    [System.Serializable]
    public class Record
    {
        public WorkerData.WorkerType workerType;
        public bool candidate;
        [SerializeField]
        WorkerData workerData;
        [SerializeField]
        WorkerRecordScript recordObject;
        [SerializeField]
        GameObject recordsPanel;
        Queue<WorkerRecordScript> records = new Queue<WorkerRecordScript>();

        public (WorkerRecordScript,Worker.Stats) GetRecord(List<string> firstNames, List<string> lastNames)
        {
            if(records == null)
            {
                records = new Queue<WorkerRecordScript>();
            }

            if(records.Count == 0)
            {
                MakeRecord();
            }

            WorkerRecordScript workerRecordScript = records.Dequeue();
            Worker.Stats stats = MakeDataForRecord(firstNames, lastNames);

            SetValuesToRecord(workerRecordScript,stats);

            return (workerRecordScript,stats);
        }

        Worker.Stats MakeDataForRecord(List<string> firstNames, List<string> lastNames)
        {

            int indexOfFirstName = UnityEngine.Random.Range(0, firstNames.Count);
            int indexOfLastName = UnityEngine.Random.Range(0, lastNames.Count);
            string name = firstNames[indexOfFirstName] + " " + lastNames[indexOfLastName];

            float moveSpeed = UnityEngine.Random.Range(workerData.minMaxMoveSpeed.x, workerData.minMaxMoveSpeed.y);
            float workSpeed = UnityEngine.Random.Range(workerData.minMaxWorkSpeed.x, workerData.minMaxWorkSpeed.y);
            float proxyMargin = workerData.proxyMargin;
            float proxyMarginOfFinalTile = workerData.proxyMarginOfFinalTile;

            float salary = moveSpeed - workSpeed;

            return new Worker.Stats(moveSpeed, workSpeed, salary, workerType, proxyMargin, proxyMarginOfFinalTile, name);
        }

        void SetValuesToRecord(WorkerRecordScript workerRecordScript, Worker.Stats stats)
        {
            workerRecordScript.nameTMP.text = stats.name;
            workerRecordScript.moveSpeedTMP.text = (stats.moveSpeed.ToString()).Substring(0, 3);
            workerRecordScript.workSpeedTMP.text = (stats.moveSpeed.ToString()).Substring(0, 3);
            workerRecordScript.salaryTMP.text = (stats.salary.ToString()).Substring(0, 3);
            workerRecordScript.workerTypeTMP.text = stats.workerType.ToString();
        }

        void MakeRecord()
        {
            WorkerRecordScript workerRecordScript = Instantiate(recordObject, recordsPanel.transform).GetComponent<WorkerRecordScript>();
            records.Enqueue(workerRecordScript);
        }

        public void ReturnRecord(WorkerRecordScript workerRecordScript)
        {
            workerRecordScript.transform.SetParent(recordsPanel.transform);
            records.Enqueue(workerRecordScript);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;


public class WorkersManager : MonoBehaviour
{
    public Tile tileToSpawnWorker = null;
    Vector3 posToSpawnWorkers;
    IndicatorsPool.WorkerSpawnerIndicator setWorkersSpawnerIndicator = null;
    IndicatorsPool.WorkerSpawnerIndicator currentWorkersSpawnerIndicator = null;

    Tile lastTile = null;
    [SerializeField]
    WorkersPanel workersPanel;
    bool isSettingWorkersSpawn = false;
    [SerializeField]
    List<WorkerBase> workers = new List<WorkerBase>();
    [SerializeField]
    WorkerData workerData;

    
    void Start()
    {
        AddListeners();
        StartCoroutine(PaySalaries());
    }

    IEnumerator PaySalaries()
    {
        while(true)
        {
            yield return new WaitForSeconds(TimeManager.instance.GetOneMounth());

            for (int i = 0; i < workers.Count; i++)
            {
                float salary = workers[i].stats.salary;
                CashManager.instance.SpendCash(salary);
            }        
        }
    }

    public void SpawnWorker(WorkerBase.Stats stats, WorkerRecordScript workerRecordScript)
    {
        if(tileToSpawnWorker == null)
        {
            NotificationsManager.instance.ShowNotification(NotificationsData.NotificationType.NewWorkersPointNotSet);
            return;    
        }
        WorkerBase worker = WorkersPool.instance.GetWorker(stats.workerType,stats);
        worker.transform.position = posToSpawnWorkers;
        worker.HireWorker();
        workers.Add(worker);

        WorkerRecordsPool.instance.ReturnRecord(workerRecordScript, true);
        workersPanel.candidates.records.Remove(workerRecordScript);

        MakeEmployedRecord(stats, worker);
    }

    public void FireWorker(WorkerRecordScript workerRecordScript)
    {
        workerRecordScript.worker.FireWorker();
        workers.Remove(workerRecordScript.worker);
        workersPanel.employed.records.Remove(workerRecordScript);

        WorkerRecordsPool.instance.ReturnRecord(workerRecordScript, false);
    }

    void SetTileForWorkersSpawn(Vector3 tilePosition)
    {
        if(setWorkersSpawnerIndicator != null)
        {
            IndicatorsPool.instance.workerSpawnerIndicators.ReturnIndicator(setWorkersSpawnerIndicator);
        }

        IndicatorsPool.instance.workerSpawnerIndicators.ReturnIndicator(currentWorkersSpawnerIndicator);       

        setWorkersSpawnerIndicator = IndicatorsPool.instance.workerSpawnerIndicators.GetIndicator(tileToSpawnWorker.tileType);
        setWorkersSpawnerIndicator.affirmativeIndicatorObject.transform.position = tilePosition;
        
        posToSpawnWorkers = tilePosition + new Vector3(0,0.1f,0);
        isSettingWorkersSpawn = false;
        workersPanel.workersPanel.SetActive(true);
    }

    void WorkerSpawnerLoad(IndicatorsPool.WorkerSpawnerIndicator indicator, Tile tile)
    {
        setWorkersSpawnerIndicator = indicator;
        tileToSpawnWorker = tile;
        posToSpawnWorkers = tile.transform.position + new Vector3(0, 0.1f, 0);
    }

    IEnumerator MakeCandidates()
    {
        while(true)
        {
            for(int i = workersPanel.candidates.records.Count-1; i >= 0; i--)
            {
                WorkerRecordScript record = workersPanel.candidates.records[i];
                workersPanel.candidates.records.Remove(record);
                WorkerRecordsPool.instance.ReturnRecord(record, true);

            }
            MakeCandidateRecord(workersPanel.candidates.records.Count,WorkerData.WorkerType.Builder);
            MakeCandidateRecord(workersPanel.candidates.records.Count,WorkerData.WorkerType.Pick);
            MakeCandidateRecord(workersPanel.candidates.records.Count,WorkerData.WorkerType.Pack);
            yield return new WaitForSeconds(TimeManager.instance.GetOneDay());
        }
    }

    void MakeEmployedRecord(WorkerBase.Stats stats, WorkerBase worker)
    {
        WorkerRecordScript workerRecordScript = WorkerRecordsPool.instance.GetRecord(worker.stats.workerType,false).Item1;
        workerRecordScript.worker = worker;
        workerRecordScript.transform.SetParent(workersPanel.employed.content.transform);

        SetValuesToRecord(workerRecordScript, stats);

        workersPanel.employed.records.Add(workerRecordScript);

        AddListenerToRecord(true, stats, workerRecordScript, workersPanel.employed.records.Count - 1);
    }

    void MakeCandidateRecord(int index, WorkerData.WorkerType workerType)
    {

        (WorkerRecordScript, WorkerBase.Stats) workerRecordScript = WorkerRecordsPool.instance.GetRecord(workerType,true);
 
        workersPanel.candidates.records.Add(workerRecordScript.Item1);
        workerRecordScript.Item1.transform.SetParent(workersPanel.candidates.content.transform);

        AddListenerToRecord(false, workerRecordScript.Item2, workerRecordScript.Item1, index);
    }

    void SetValuesToRecord(WorkerRecordScript workerRecordScript, WorkerBase.Stats stats)
    {
        workerRecordScript.nameTMP.text = stats.name;
        workerRecordScript.moveSpeedTMP.text = (stats.moveSpeed.ToString()).Substring(0, 3);
        workerRecordScript.workSpeedTMP.text = (stats.workSpeed.ToString()).Substring(0, 3);
        workerRecordScript.salaryTMP.text = (stats.salary.ToString()).Substring(0, 3);
        workerRecordScript.workerTypeTMP.text = stats.workerType.ToString();
    }

    void AddListenerToRecord(bool isEmployed, WorkerBase.Stats stats, WorkerRecordScript workerRecordScript,int index)
    {
        if(isEmployed)
        {
            workersPanel.employed.records[index].actionButton.onClick.RemoveAllListeners();
            workersPanel.employed.records[index].actionButton.onClick.AddListener(() => FireWorker(workerRecordScript));
        }
        else
        {
            workersPanel.candidates.records[index].actionButton.onClick.RemoveAllListeners();
            workersPanel.candidates.records[index].actionButton.onClick.AddListener(() => SpawnWorker(stats, workerRecordScript));
        }
    }
    void CheckForTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide))
            {
                Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

                if (tile == null)
                    return;

                if(tile.tileType == TileTypes.TileType.Ground || tile.tileType == TileTypes.TileType.Floor)
                {
                    tile.haveTask = true;

                    if(tileToSpawnWorker != null)
                        tileToSpawnWorker.haveTask = false;

                    tileToSpawnWorker = tile;
                    SetTileForWorkersSpawn(tile.transform.position);
                }
            }
    }

    void CheckForClick()
    {
        if(!isSettingWorkersSpawn || IsMouseOverUi())
            return;

        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            CheckForTile();
        }
    }

    void Update()
    {
        CheckForClick();
        CheckForIndicator();
    }

    void CheckForIndicator()
    {
        if(!isSettingWorkersSpawn || IsMouseOverUi())
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide))
            {
                
                Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

                if (tile == null)
                    return;

                if(tile != lastTile)
                {
                    lastTile = tile;

                    if(currentWorkersSpawnerIndicator != null)
                    {
                        IndicatorsPool.instance.workerSpawnerIndicators.ReturnIndicator(currentWorkersSpawnerIndicator);
                    }

                    currentWorkersSpawnerIndicator = IndicatorsPool.instance.workerSpawnerIndicators.GetIndicator(tile.tileType);

                    if(currentWorkersSpawnerIndicator.isAffirmative)
                    {
                        currentWorkersSpawnerIndicator.affirmativeIndicatorObject.transform.position = tile.transform.position;
                    }
                    else
                    {
                        currentWorkersSpawnerIndicator.negativeIndicatorObject.transform.position = tile.transform.position;
                    }
                }
            }
    }

    bool IsMouseOverUi()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void StartSettingSpawnPoint()
    {
        isSettingWorkersSpawn = true;
        workersPanel.workersPanel.SetActive(false);
    }

    public void OpenPanel(bool open)
    {
        if(open)
        {
            workersPanel.workersPanel.SetActive(true);
            workersPanel.employed.panel.SetActive(true);
            workersPanel.candidates.panel.SetActive(false);
        }
        else
        {
            workersPanel.workersPanel.SetActive(false);
            workersPanel.employed.panel.SetActive(false);
            workersPanel.candidates.panel.SetActive(false);
        }
    }

    public void GoToCandidates(bool goTo)
    {
        if(goTo)
        {
            workersPanel.candidates.panel.SetActive(true);
            workersPanel.employed.panel.SetActive(false);
        }
        else
        {
            workersPanel.candidates.panel.SetActive(false);
            workersPanel.employed.panel.SetActive(true);
        }
    }

    void AddListeners()
    {
        workersPanel.workersPanel.SetActive(true);

        workersPanel.openPanel.onClick.AddListener(() => OpenPanel(true));
        workersPanel.close.onClick.AddListener(() => OpenPanel(false));
        workersPanel.setSpawnPoint.onClick.AddListener(() => StartSettingSpawnPoint());

        workersPanel.employed.panel.SetActive(true);
        workersPanel.candidates.panel.SetActive(true);

        workersPanel.employed.goToNext.onClick.AddListener(() => GoToCandidates(true));
        workersPanel.candidates.goToNext.onClick.AddListener(() => GoToCandidates(false));

        workersPanel.employed.panel.SetActive(false);
        workersPanel.candidates.panel.SetActive(false);

        workersPanel.workersPanel.SetActive(false);

        StartCoroutine(MakeCandidates());
    }

    void LoadWorkers()
    {
        List<SaveData.WorkerData> workersData = SavingManager.instance.saveData.workers;

        foreach (SaveData.WorkerData data in workersData)
        {
            WorkerBase.Stats stats = new WorkerBase.Stats(data.moveSpeed, data.workSpeed, data.salary, (WorkerData.WorkerType)Enum.Parse(typeof(WorkerData.WorkerType), data.type), 
                workerData.proxyMargin, workerData.proxyMarginOfFinalTile, data.name);

            WorkerBase worker = WorkersPool.instance.GetWorker(stats.workerType, stats);
            worker.transform.position = data.position;
            worker.HireWorker();
            workers.Add(worker);

            WorkerRecordScript workerRecordScript = WorkerRecordsPool.instance.GetRecord(worker.stats.workerType, false).Item1;
            workerRecordScript.worker = worker;
            workerRecordScript.transform.SetParent(workersPanel.employed.content.transform);

            SetValuesToRecord(workerRecordScript, stats);
            workersPanel.employed.records.Add(workerRecordScript);

            AddListenerToRecord(true, stats, workerRecordScript, workersPanel.employed.records.Count - 1);
        }
    }

    void OnEnable()
    {
        SavingManager.OnWorkersLoad += LoadWorkers;
        IndicatorsPool.OnWorkerSpawnerIndicatorsLoad += WorkerSpawnerLoad;        
    }

    void OnDisable()
    {
        SavingManager.OnWorkersLoad -= LoadWorkers;
        IndicatorsPool.OnWorkerSpawnerIndicatorsLoad -= WorkerSpawnerLoad;   
    }

    void SaveIndicator()
    {
        if (setWorkersSpawnerIndicator != null)
        {
            SavingManager.instance.saveData.workerSpawnerIndicators.Add(new SaveData.WorkerSpawnerIndicatorData(
                setWorkersSpawnerIndicator.isAffirmative, setWorkersSpawnerIndicator.affirmativeIndicatorObject.transform.position, 
                setWorkersSpawnerIndicator.affirmativeIndicatorObject.transform.rotation));
        }
    }

    [System.Serializable]
    public class WorkersPanel
    {
        public GameObject workersPanel;
        public Panels employed;
        public Panels candidates;
        public Button setSpawnPoint;
        public Button close;
        public Button openPanel;
        
        [System.Serializable]
        public class Panels
        {
            public GameObject panel;
            public Button goToNext;
            public GameObject content;
            public WorkerRecordScript recordObjectPrefab;
            public List<WorkerRecordScript> records = new List<WorkerRecordScript>();
        }
    }
}

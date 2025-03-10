using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class WorkersManager : MonoBehaviour
{
    Vector3 posToSpawnWorkers;
    [SerializeField]
    WorkersPanel workersPanel;
    bool isSettingWorkersSpawn = false;

    [SerializeField]
    WorkerData builderData;
    [SerializeField]
    WorkerData packData;
    [SerializeField]
    WorkerData pickData;
    [SerializeField]
    Worker workerPrefab;
    [SerializeField]
    List<string> lastNames = new List<string>();
    [SerializeField]
    List<string> firstNames = new List<string>();
    [SerializeField]
    TextAsset firstNamesFile;
    [SerializeField]
    TextAsset lastNamesFile;
    void Awake()
    {
        LoadNames();
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
    void Start()
    {
        AddListeners();
    }

    public void SpawnWorker(Worker.Stats stats, WorkerRecordScript workerRecordScript)
    {
        Worker worker = Instantiate(workerPrefab,posToSpawnWorkers, transform.rotation);
        worker.stats = stats;

        workersPanel.candidates.records.Remove(workerRecordScript);

        Destroy(workerRecordScript.gameObject);

        MakeEmployedRecord(stats, worker);
    }

    public void FireWorker(WorkerRecordScript workerRecordScript)
    {
        workersPanel.employed.records.Remove(workerRecordScript);
        
        Destroy(workerRecordScript.worker.gameObject);

        Destroy(workerRecordScript.gameObject);
    }

    void SetTileForWorkersSpawn(Vector3 tilePosition)
    {
        posToSpawnWorkers = tilePosition + new Vector3(0,1,0);
        isSettingWorkersSpawn = false;
        workersPanel.workersPanel.SetActive(true);
    }

    IEnumerator MakeCandidates()
    {
        while(true)
        {
            MakeCandidateRecord(workersPanel.candidates.records.Count,WorkerData.WorkerType.Builder);
            MakeCandidateRecord(workersPanel.candidates.records.Count,WorkerData.WorkerType.Pick);
            MakeCandidateRecord(workersPanel.candidates.records.Count,WorkerData.WorkerType.Pack);
            yield return new WaitForSeconds(TimeManager.instance.GetOneDay());
        }
    }

    void MakeEmployedRecord(Worker.Stats stats, Worker worker)
    {

        WorkerRecordScript workerRecordScript = Instantiate(workersPanel.employed.recordObjectPrefab, workersPanel.employed.content.transform).GetComponent<WorkerRecordScript>();
        workerRecordScript.worker = worker;


        SetValuesToRecord(workerRecordScript, stats);

        workersPanel.employed.records.Add(workerRecordScript);

        AddListenerToRecord(true, stats, workerRecordScript, workersPanel.employed.records.Count - 1);
    }

    void MakeCandidateRecord(int index, WorkerData.WorkerType workerType)
    {
        float moveSpeed = 0;
        float workSpeed = 0;
        float proxyMargin = 0;
        float proxyMarginOfFinalTile = 0;
        string name = "";

        int indexOfFirstName = Random.Range(0, firstNames.Count);
        int indexOfLastName = Random.Range(0, lastNames.Count);
        name = firstNames[indexOfFirstName] + " " + lastNames[indexOfLastName];


        switch(workerType)
        {
            case WorkerData.WorkerType.Builder:
            {
                moveSpeed = Random.Range(builderData.minMaxMoveSpeed.x, builderData.minMaxMoveSpeed.y);
                workSpeed = Random.Range(builderData.minMaxWorkSpeed.x, builderData.minMaxWorkSpeed.y);
                proxyMargin = builderData.proxyMargin;
                proxyMarginOfFinalTile = builderData.proxyMarginOfFinalTile;
                break;
            }
            case WorkerData.WorkerType.Pick:
            {
                moveSpeed = Random.Range(pickData.minMaxMoveSpeed.x, pickData.minMaxMoveSpeed.y);
                workSpeed = Random.Range(pickData.minMaxWorkSpeed.x, pickData.minMaxWorkSpeed.y);
                proxyMargin = pickData.proxyMargin;
                proxyMarginOfFinalTile = pickData.proxyMarginOfFinalTile;
                break;
            }
            case WorkerData.WorkerType.Pack:
            {
                moveSpeed = Random.Range(packData.minMaxMoveSpeed.x, packData.minMaxMoveSpeed.y);
                workSpeed = Random.Range(packData.minMaxWorkSpeed.x, packData.minMaxWorkSpeed.y);
                proxyMargin = packData.proxyMargin;
                proxyMarginOfFinalTile = packData.proxyMarginOfFinalTile;
                break;
            }
        }

        float salary = moveSpeed + workSpeed;

        Worker.Stats stats = new Worker.Stats(moveSpeed, workSpeed, salary, workerType, proxyMargin, proxyMarginOfFinalTile, name); 
    
        WorkerRecordScript workerRecordScript = Instantiate(workersPanel.candidates.recordObjectPrefab, workersPanel.candidates.content.transform).GetComponent<WorkerRecordScript>();

        SetValuesToRecord(workerRecordScript, stats);

        workersPanel.candidates.records.Add(workerRecordScript);

        AddListenerToRecord(false, stats, workerRecordScript, index);
    }

    void SetValuesToRecord(WorkerRecordScript workerRecordScript, Worker.Stats stats)
    {
        workerRecordScript.nameTMP.text = stats.name;
        workerRecordScript.moveSpeedTMP.text = (stats.moveSpeed.ToString()).Substring(0,3);
        workerRecordScript.workSpeedTMP.text = (stats.moveSpeed.ToString()).Substring(0,3);
        workerRecordScript.salaryTMP.text = (stats.salary.ToString()).Substring(0,3);
        workerRecordScript.workerTypeTMP.text = stats.workerType.ToString();
    }

    void AddListenerToRecord(bool isEmployed, Worker.Stats stats, WorkerRecordScript workerRecordScript,int index)
    {
        if(isEmployed)
        {
            workersPanel.employed.records[index].actionButton.onClick.AddListener(() => FireWorker(workerRecordScript));
        }
        else
        {
            workersPanel.candidates.records[index].actionButton.onClick.AddListener(() => SpawnWorker(stats, workerRecordScript));
        }
    }
    void CheckForTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                
                Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();

                if (tile == null)
                    return;

                if(tile.tileType == TileTypes.TileType.Ground && !tile.building)
                {
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

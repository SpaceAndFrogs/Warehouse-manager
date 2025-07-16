using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using UnityEngine.UI;

public class SavingManager : MonoBehaviour
{
    public static SavingManager instance;
    public string currentSaveFileName = "saveData.json";
    public string saveDirectoryName = "Saves";
    public SaveData saveData;
    public bool newGame = true;
    public GameObject newGameNamePanel;
    public GameObject loadGamePanel;
    public GameObject loadGamePanelContent;
    public GameObject newGameButton;
    public GameObject loadGameButton;
    public TextMeshProUGUI saveNameText;
    public LoadRowScript loadRowPrefab;
    public List<LoadRowScript> loadRowScripts = new List<LoadRowScript>();
    public Button saveButton;
    public Button mainMenuButton;

    private void Awake()
    {
        // Ensure the save directory exists
        if (!System.IO.Directory.Exists(Path.Combine(Application.persistentDataPath, saveDirectoryName)))
        {
            string saveDir = Path.Combine(Application.persistentDataPath, saveDirectoryName);
            Directory.CreateDirectory(saveDir);
            Debug.Log("Created Saves directory at: " + saveDir);
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("SavingManager started.");
        MakeLoadRows();
    }

    public void EnableNewGamePanel()
    {
        newGameNamePanel.SetActive(true);
        loadGamePanel.SetActive(false);
        newGameButton.SetActive(false);
        loadGameButton.SetActive(false);
        saveNameText.text = null;
    }

    public void EnableLoadGamePanel()
    {
        newGameNamePanel.SetActive(false);
        loadGamePanel.SetActive(true);
        newGameButton.SetActive(false);
        loadGameButton.SetActive(false);

    }

    public void DisablePanels()
    {
        newGameNamePanel.SetActive(false);
        loadGamePanel.SetActive(false);
        newGameButton.SetActive(true);
        loadGameButton.SetActive(true);
    }

    public void StartNewGame()
    {
        newGame = true;
        saveData = new SaveData();
        currentSaveFileName = saveNameText.text + ".json";
        string json = JsonUtility.ToJson(saveData, true);
        System.IO.File.WriteAllText(System.IO.Path.Combine(Path.Combine(Application.persistentDataPath, saveDirectoryName), currentSaveFileName), json);

        StartCoroutine(NewGame());
    }


    public void SaveGame()
    {
        saveData = new SaveData();
        OnSave?.Invoke();
        instance.saveData.playerCash = CashManager.instance.AmountOfCash();
        instance.saveData.gameTime = TimeManager.instance.currentTimeScale;

        string json = JsonUtility.ToJson(saveData, true);
        System.IO.File.WriteAllText(System.IO.Path.Combine(Path.Combine(Application.persistentDataPath, saveDirectoryName), currentSaveFileName), json);
    }

    void MakeLoadRows()
    {

        string[] files = GetFilesInSaveDirectory();
        foreach (string file in files)
        {
            Debug.Log("Creating load row for file: " + file);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
            AddNewLoadRow(fileName);
        }
    }

    public void AddNewLoadRow(string saveName)
    {
        GameObject rowObject = Instantiate(loadRowPrefab.gameObject, loadGamePanelContent.transform);
        LoadRowScript rowScript = rowObject.GetComponent<LoadRowScript>();
        rowScript.saveNameText.text = saveName;
        loadRowScripts.Add(rowScript);
        rowScript.loadButton.onClick.AddListener(() => Load(saveName));
        rowScript.deleteSaveButton.onClick.AddListener(() => rowScript.DeleteSave());
    }

    string[] GetFilesInSaveDirectory()
    {
        string saveDir = Path.Combine(Application.persistentDataPath, saveDirectoryName);

        if (Directory.Exists(saveDir))
        {
            string[] files = Directory.GetFiles(Path.Combine(Application.persistentDataPath, saveDirectoryName), "*.json");
            string[] fileNamesWithoutExtension = new string[files.Length];

            if (files.Length == 0)
            {
                Debug.Log("Brak plików w katalogu save.");
            }
            else
            {
                for (int i = 0; i < files.Length; i++)
                {
                    string cleanName = Path.GetFileNameWithoutExtension(files[i]);
                    fileNamesWithoutExtension[i] = cleanName;
                    Debug.Log("Znaleziono plik: " + cleanName);
                }
            }

            return fileNamesWithoutExtension;
        }
        else
        {
            Debug.Log("Katalog Saves nie istnieje.");
        }

        return null;
    }

    public void Load(string saveFileName)
    {
        currentSaveFileName = saveFileName + ".json";
        string filePath = System.IO.Path.Combine(Path.Combine(Application.persistentDataPath, saveDirectoryName), currentSaveFileName);
        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            saveData = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            Debug.LogWarning("Save file not found.");
            return;
        }

        StartCoroutine(LoadGame());

    }

    void FindObjectsInGame()
    {
        saveButton = FindObjectOfType<SaveButton>().GetComponent<Button>();
        saveButton.onClick.AddListener(SaveGame);
        mainMenuButton = FindObjectOfType<MainMenuButton>().GetComponent<Button>();
        mainMenuButton.onClick.AddListener(LoadMenu);
        Debug.Log("Found SaveButton and MainMenuButton in the scene.");
    }

    void FindObjectsInMenu()
    {
        MainMenuUI mainMenuUI = FindObjectOfType<MainMenuUI>();
        if (mainMenuUI != null)
        {
            newGameNamePanel = mainMenuUI.newGameNamePanel;
            loadGamePanel = mainMenuUI.loadGamePanel;
            loadGamePanelContent = mainMenuUI.loadGamePanelContent;
            saveNameText = mainMenuUI.saveNameText;
            Button newGameButtonElement = mainMenuUI.newGameButton.GetComponent<Button>();
            newGameButtonElement.onClick.AddListener(EnableNewGamePanel);
            newGameButton = mainMenuUI.newGameButton.gameObject;
            Button loadGameButtonElement = mainMenuUI.loadGameButton.GetComponent<Button>();
            loadGameButtonElement.onClick.AddListener(EnableLoadGamePanel);
            loadGameButton = mainMenuUI.loadGameButton.gameObject;
            foreach (Button backButton in mainMenuUI.backButtons)
            {
                backButton.onClick.AddListener(DisablePanels);
            }
            Button startNewGameButtonElement = mainMenuUI.startNewGameButton.GetComponent<Button>();
            startNewGameButtonElement.onClick.AddListener(StartNewGame);
        }
        else
        {
            Debug.LogWarning("MainMenuUI not found in the scene.");
        }
    }

    public void LoadMenu()
    {
        StartCoroutine(LoadMainMenu());
    }
    IEnumerator LoadMainMenu()
    {
        SaveGame();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);

        while (!asyncLoad.isDone)
            yield return null;
        
        yield return null;

        FindObjectsInMenu();
        loadRowScripts.Clear();
        MakeLoadRows();
    }

    IEnumerator NewGame()
    {
        newGame = true;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);

        while (!asyncLoad.isDone)
            yield return null;
        
        yield return null;
        FindObjectsInGame();
    }
    IEnumerator LoadGame()
    {
        newGame = false;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);

        while (!asyncLoad.isDone)
            yield return null;

        OnMapLoad?.Invoke();
        yield return null;

        OnWorkersLoad?.Invoke();
        yield return null;

        OnBuildingsLoad?.Invoke();
        yield return null;

        OnIndicatorsLoad?.Invoke();
        yield return null;

        OnPricesLoad?.Invoke();
        yield return null;

        OnLoansLoad?.Invoke();
        yield return null;

        yield return new WaitForSeconds(0.1f);
        OnTasksLoad?.Invoke();

        yield return null;
    
        FindObjectsInGame();
    }

#nullable enable
    #region Events
    public static event Action? OnSave;
    public static event Action? OnMapLoad;
    public static event Action? OnBuildingsLoad;
    public static event Action? OnWorkersLoad;
    public static event Action? OnIndicatorsLoad;
    public static event Action? OnPricesLoad;
    public static event Action? OnLoansLoad;
    public static event Action? OnTasksLoad;
        
    #endregion
#nullable disable
}

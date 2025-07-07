using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class SavingManager : MonoBehaviour
{
    public static SavingManager instance;
    public string saveFileName = "saveData.json";
    public string saveDirectory = "Saves";
    public SaveData saveData;

    private void Awake()
    {
        // Ensure the save directory exists
        if (!System.IO.Directory.Exists(saveDirectory))
        {
            System.IO.Directory.CreateDirectory(saveDirectory);
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

    public void SaveGame()
    {
        saveData = new SaveData();
        OnSave?.Invoke();
        instance.saveData.playerCash = CashManager.instance.AmountOfCash();
        instance.saveData.gameTime = TimeManager.instance.currentTimeScale;

        string json = JsonUtility.ToJson(saveData, true);
        System.IO.File.WriteAllText(System.IO.Path.Combine(saveDirectory, saveFileName), json);
    }

    public void Load()
    {
        string filePath = System.IO.Path.Combine(saveDirectory, saveFileName);
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

    IEnumerator LoadGame()
    {
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

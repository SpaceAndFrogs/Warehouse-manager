using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load the game data when the game starts
        LoadGame();
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

    public void LoadGame()
    {
        string filePath = System.IO.Path.Combine(saveDirectory, saveFileName);
        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            saveData = JsonUtility.FromJson<SaveData>(json);
            OnMapLoad?.Invoke();
            OnTileLoad?.Invoke();
            OnBuildingsLoad?.Invoke();
            OnWorkersLoad?.Invoke();
            OnIndicatorsLoad?.Invoke();
        }
        else
        {
            Debug.LogWarning("Save file not found.");
        }
    }

    #nullable enable
    #region Events
        public static event Action? OnSave;       
        public static event Action? OnMapLoad;
        public static event Action? OnTileLoad;
        public static event Action? OnBuildingsLoad;
        public static event Action? OnWorkersLoad;
        public static event Action? OnIndicatorsLoad;
        
    #endregion
    #nullable disable
}

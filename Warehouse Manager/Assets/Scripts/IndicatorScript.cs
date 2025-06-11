using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorScript : MonoBehaviour
{
    public TasksTypes.TaskType taskType;
    public Buildings.BuildingType buildingType;
    public bool isAffirmative;
    public bool isWorkerSpawner;

    private void OnEnable()
    {
        SavingManager.OnSave += OnSave;
    }
    private void OnDisable()
    {
        SavingManager.OnSave -= OnSave;
    }

    void OnSave()
    {
        if (taskType != TasksTypes.TaskType.None)
        {
            SavingManager.instance.saveData.taskIndicators.Add(new SaveData.TaskIndicatorData(taskType.ToString(), isAffirmative, transform.position, transform.rotation));
        }
        else
        {
            SavingManager.instance.saveData.buildingIndicators.Add(new SaveData.BuildingIndicatorData(buildingType.ToString(), isAffirmative, transform.position, transform.rotation));
        }
    }
}

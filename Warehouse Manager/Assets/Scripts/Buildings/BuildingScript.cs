using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingScript : MonoBehaviour
{
    public BuildingsPool.Building building = null;

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
        SavingManager.instance.saveData.buildings.Add(new SaveData.BuildingData(building.buildingType.ToString(), transform.position, transform.rotation));
    }
}

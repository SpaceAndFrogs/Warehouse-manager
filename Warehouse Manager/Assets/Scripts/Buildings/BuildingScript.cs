using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingScript : BuildingBase
{
    protected override void EnableFinishHook()
    {
    }
    protected override void DisableFinishHook()
    {
    }
    protected override void StartFinishHook()
    {
    }
    protected override void OnSave()
    {
        SavingManager.instance.saveData.buildings.Add(new SaveData.BuildingData(building.buildingType.ToString(), transform.position, transform.rotation));
    }
}

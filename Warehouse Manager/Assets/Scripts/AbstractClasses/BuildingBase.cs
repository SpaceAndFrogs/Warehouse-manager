using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingBase : MonoBehaviour
{
    public Tile tileWithBuilding = null;
    public BuildingsPool.Building building = null;
    public bool isInRoom = false;
    void Start()
    {
        CheckIfIsInRoom();
        StartFinishHook();
    }

    void CheckIfIsInRoom()
    {
        if (tileWithBuilding == null)
        {
            tileWithBuilding = GetTile(transform.position);
        }
        isInRoom = PathFinder.instance.IsBuildingSurrounded(tileWithBuilding);
    }
    protected abstract void StartFinishHook();
    protected Tile GetTile(Vector3 position)
    {
        Ray ray = new Ray(position + new Vector3(0f, 100f, 0f), Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        foreach (RaycastHit hit in hits)
        {
            Tile tile = hit.collider.gameObject.GetComponent<Tile>();
            if (tile != null)
            {
                return tile;
            }
        }
        return null;
    }

    protected void OnEnable()
    {
        SavingManager.OnSave += OnSave;
        BuildingWorker.OnBuildingEnded += CheckIfIsInRoom;
        EnableFinishHook();
    }

    protected void OnDisable()
    {
        SavingManager.OnSave -= OnSave;
        BuildingWorker.OnBuildingEnded -= CheckIfIsInRoom;
        DisableFinishHook();
    }

    protected abstract void EnableFinishHook();
    protected abstract void DisableFinishHook();

    protected abstract void OnSave();
}

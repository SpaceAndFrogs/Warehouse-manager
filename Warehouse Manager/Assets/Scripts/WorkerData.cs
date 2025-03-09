using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorkerData", menuName = "Workers", order = 1)]
public class WorkerData : ScriptableObject
{
    public enum WorkerType {Builder, Pick, Pack}

    public WorkerType workerType;
    public Vector2 minMaxMoveSpeed;
    public Vector2 minMaxWorkSpeed;
    public string name;
    public float proxyMargin;
    public float proxyMarginOfFinalTile;
}

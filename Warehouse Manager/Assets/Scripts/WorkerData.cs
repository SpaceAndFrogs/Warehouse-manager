using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorkerData", menuName = "Workers", order = 1)]
public class WorkerData : ScriptableObject
{
    public float moveSpeed;
    public float proxyMargin;
    public float proxyMarginOfFinalTile;
}

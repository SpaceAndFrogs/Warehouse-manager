using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PickStash : MonoBehaviour
{

    public Tile tileWithStash;
    public static event Action<PickStash>? OnPickStashSpawned;
    void Start()
    {
        OnPickStashSpawned?.Invoke(this);
    }
}

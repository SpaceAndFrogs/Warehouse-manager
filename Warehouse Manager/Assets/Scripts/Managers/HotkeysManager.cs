using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class HotkeysManager : MonoBehaviour
{
    
    public static HotkeysManager instance { get; private set; } = null!;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            return;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode))
                {
                    OnKeyPressed?.Invoke(kcode);
                    UnityEngine.Debug.Log("KeyCode down: " + kcode);
                    break;
                }
            }
        }
    }

    #nullable enable
    public static event Action<KeyCode>? OnKeyPressed;
    #nullable disable
}

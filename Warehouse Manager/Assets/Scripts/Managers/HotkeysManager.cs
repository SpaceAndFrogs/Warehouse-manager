using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotkeysManager : MonoBehaviour
{
    public static HotkeysManager instance { get; private set; } = null!;
    public float keyRepeatDelay = 0.3f; // Delay in seconds

    private Dictionary<KeyCode, float> lastInvokeTime = new Dictionary<KeyCode, float>();

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
        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(kcode))
            {
                float currentTime = Time.unscaledTime;
                if (!lastInvokeTime.ContainsKey(kcode) || currentTime - lastInvokeTime[kcode] >= keyRepeatDelay)
                {
                    OnKeyPressed?.Invoke(kcode);
                    UnityEngine.Debug.Log("KeyCode held: " + kcode);
                    lastInvokeTime[kcode] = currentTime;
                }
            }
            else
            {
                // Reset timer when key is released
                if (lastInvokeTime.ContainsKey(kcode))
                    lastInvokeTime.Remove(kcode);
            }
        }
    }

    #nullable enable
    public static event Action<KeyCode>? OnKeyPressed;
    #nullable disable
}

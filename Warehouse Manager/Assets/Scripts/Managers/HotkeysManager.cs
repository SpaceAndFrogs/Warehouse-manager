using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotkeysManager : MonoBehaviour
{
    public static HotkeysManager instance { get; private set; } = null!;
    public float keyRepeatDelay = 0.3f; // Delay in seconds

    private Dictionary<KeyCode, float> lastInvokeTime = new Dictionary<KeyCode, float>();

    // new: track which keys were down in the previous frame
    private HashSet<KeyCode> keysDown = new HashSet<KeyCode>();

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
        // iterate all possible KeyCodes
        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
        {
            bool isDown = Input.GetKey(kcode);

            if (isDown)
            {
                float currentTime = Time.unscaledTime;
                if (!lastInvokeTime.ContainsKey(kcode) || currentTime - lastInvokeTime[kcode] >= keyRepeatDelay)
                {
                    OnKeyPressed?.Invoke(kcode);
                    UnityEngine.Debug.Log("KeyCode held: " + kcode);
                    lastInvokeTime[kcode] = currentTime;
                }

                // mark key as currently down for release detection next frame
                keysDown.Add(kcode);
            }
            else
            {
                // if key was down previous frame and now is up -> release event
                if (keysDown.Contains(kcode))
                {
                    OnKeyReleased?.Invoke(kcode);
                    UnityEngine.Debug.Log("KeyCode released: " + kcode);
                    keysDown.Remove(kcode);
                }

                // Reset timer when key is released
                if (lastInvokeTime.ContainsKey(kcode))
                    lastInvokeTime.Remove(kcode);
            }
        }
    }

#nullable enable
    public static event Action<KeyCode>? OnKeyPressed;
    public static event Action<KeyCode>? OnKeyReleased;
    #nullable disable
}

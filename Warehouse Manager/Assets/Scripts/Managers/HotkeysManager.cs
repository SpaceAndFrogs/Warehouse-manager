using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotkeysManager : MonoBehaviour
{
    public static HotkeysManager instance { get; private set; } = null!;
    public float keyRepeatDelay = 0.3f; // Delay in seconds

    private Dictionary<KeyCode, float> lastInvokeTime = new Dictionary<KeyCode, float>();

    private Dictionary<KeyCode, KeyCode> hotkeys = new Dictionary<KeyCode, KeyCode>();
    private HashSet<KeyCode> keysDown = new HashSet<KeyCode>();
    HashSet<HotkeyScript> hotkeyScripts = new HashSet<HotkeyScript>();
    [SerializeField]
    HotkeysData hotkeysData;
    [SerializeField]
    HotkeyScript hotkeyRecordPrefab;
    [SerializeField]
    Transform hotkeysPanelTransform;

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

    void Start()
    {
        AddHotkeysToDictionary();
        MakeHotkeysRecords();
    }

    void MakeHotkeysRecords()
    {
        foreach(HotkeysData.Hotkey hotkeyData in hotkeysData.hotkeys)
        {
            HotkeyScript hotkeyScript = Instantiate(hotkeyRecordPrefab, hotkeysPanelTransform);
            hotkeyScript.labelTMP.text = hotkeyData.label;
            hotkeyScript.inputField.text = hotkeyData.defaultKeyCode.ToString();
            hotkeyScripts.Add(hotkeyScript);
        }
    }

    void AddListenersToInputFields()
    {
        foreach(HotkeyScript hotkeyScript in hotkeyScripts)
        {
            hotkeyScript.inputField.onValueChanged.AddListener(ChangeHotkey);
        }
    }

    void ChangeHotkey(string newHotkeyString) 
    {


    }

    void AddHotkeysToDictionary()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            hotkeys[key] = key;
        }

        Debug.Log($"Zainicjalizowano {hotkeys.Count} skrótów klawiszowych.");
    }

    void Update()
    {
        foreach (KeyCode inputKey in Enum.GetValues(typeof(KeyCode)))
        {
            KeyCode mappedKey = hotkeys.TryGetValue(inputKey, out var mk) ? mk : inputKey;

            if (Input.GetKey(inputKey))
            {
                float currentTime = Time.unscaledTime;

                if (!lastInvokeTime.ContainsKey(mappedKey) || currentTime - lastInvokeTime[mappedKey] >= keyRepeatDelay)
                {
                    OnKeyPressed?.Invoke(mappedKey);
                    Debug.Log($"Key held: {inputKey} → mapped to {mappedKey}");
                    lastInvokeTime[mappedKey] = currentTime;
                }

                keysDown.Add(mappedKey);
            }
            else
            {
                if (keysDown.Contains(mappedKey))
                {
                    OnKeyReleased?.Invoke(mappedKey);
                    Debug.Log($"Key released: {inputKey} → mapped to {mappedKey}");
                    keysDown.Remove(mappedKey);
                }

                if (lastInvokeTime.ContainsKey(mappedKey))
                    lastInvokeTime.Remove(mappedKey);
            }
        }
    }

#nullable enable
    public static event Action<KeyCode>? OnKeyPressed;
    public static event Action<KeyCode>? OnKeyReleased;
    #nullable disable
}

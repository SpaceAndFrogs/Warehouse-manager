using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "HotkeysData", menuName = "Hotkeys", order = 1)]
public class HotkeysData : ScriptableObject
{
    public Hotkey[] hotkeys = new Hotkey[1];
    [Serializable]
    public class Hotkey
    {
        public string label;
        public KeyCode defaultKeyCode;
    }
}

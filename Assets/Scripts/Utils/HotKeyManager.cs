using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class HotKeyManager : StaticMonoBehaviour<HotKeyManager>
{
    [SerializeField] private Hotkeys _hotKeys;
    private Dictionary<HotKeyName, Hotkey> _hotKeyMapping;
    public event Action<KeyCode, KeyCode> onHotKeySet;   

    private void Start()
    {
        //LoadHotkeys();
        PopulateMapping();
    }

    private void OnApplicationQuit()
    {
        //SaveHotkeys();
    }

    void LoadHotkeys()
    {
        string filePath = FilePathUtil.GetHotkeyFilePath();

        if (File.Exists(filePath))
        {
            Debug.Log("Loading saved hotkeys");
            string fileData = File.ReadAllText(filePath);
            _hotKeys = JsonUtility.FromJson<Hotkeys>(fileData);
        }
        else
        {
            Debug.Log("No saved hotkeys, using defaults");
        }
    }

    void SaveHotkeys()
    {
        string filePath = FilePathUtil.GetHotkeyFilePath();
        string fileData = JsonUtility.ToJson(_hotKeys);
        File.WriteAllText(filePath, fileData);
    }

    void PopulateMapping()
    {
        _hotKeyMapping = new Dictionary<HotKeyName, Hotkey>();
        
        foreach (var hotKey in _hotKeys.hotkeys)
        {
            _hotKeyMapping.Add(hotKey.hotkeyName, hotKey);            
        }
    }
    
    
    
    public Hotkey[] GetHotKeys()
    {
        return _hotKeys.hotkeys;
    }

    public void SetButtonForKey(HotKeyName keyName, KeyCode keyCode)
    {
        _hotKeyMapping[keyName].savedKeyCode = keyCode;
    }

    public KeyCode GetKeyFor(HotKeyName keyName)
    {
        if(_hotKeyMapping.TryGetValue(keyName, out var hotkey))
        {
            return hotkey.savedKeyCode;
        }
        
        Debug.LogError($"No hotkey found with name {keyName}");
        return KeyCode.Alpha0;
    }

    public void SetDefaults()
    {
        foreach(Hotkey hotkey in _hotKeys.hotkeys)
        {
            hotkey.savedKeyCode = hotkey.defaultKeyCode;
        }
    }

    public static bool GetKeyDown(HotKeyName keyName)
    {
        return Input.GetKeyDown(Instance.GetKeyFor(keyName));
    }

    public static bool GetKeyUp(HotKeyName keyName)
    {
        return Input.GetKeyUp(Instance.GetKeyFor(keyName));
    }

    public static bool GetKey(HotKeyName keyName)
    {
        return Input.GetKey(Instance.GetKeyFor(keyName));
    }

    /// <summary>
    /// Used when checking if "control + key" is being used
    /// </summary>
    /// <param name="keyName"></param>
    /// <returns></returns>
    public static bool GetModifiedKeyDown(HotKeyName keyName)
    {
        return Input.GetKey(Instance.GetKeyFor(HotKeyName.ControlModifier))
            && Input.GetKeyDown(Instance.GetKeyFor(keyName));
    }
}
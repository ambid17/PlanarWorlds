using System;
using System.Collections.Generic;
using UnityEngine;

public class HotKeyManager : StaticMonoBehaviour<HotKeyManager> {
    [SerializeField]
    private Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();
    private Dictionary<string, KeyCode> defaults = new Dictionary<string, KeyCode>();
    private Dictionary<string, string> tooltips = new Dictionary<string, string>();

    public static HotKeyManager Instance;

    public event Action<KeyCode, KeyCode> onHotKeySet;

    private void Start()
    {
        InitDefaults();
        LoadSavedHotkeys();
        LoadTooltips();
    }

    public Dictionary<String, KeyCode> GetHotKeys()
    {
        return keys;
    }
	
	public Dictionary<String, KeyCode> GetDefaultHotKeys() 
	{
		return defaults;
	}

    public Dictionary<string, string> GetTooltips()
    {
        return tooltips;
    }

    public void SetButtonForKey(string key, KeyCode keyCode)
    {

        KeyCode oldKeyCode = keys[key];
        keys[key] = keyCode;
        onHotKeySet?.Invoke(oldKeyCode, keys[key]);
        PlayerPrefs.SetString(key, keyCode.ToString());
    }

    public KeyCode GetKeyFor(string action)
    {
        KeyCode keyCode = KeyCode.Alpha0; 
        try
        {
            keyCode = keys[action];
        }
        catch
        {
            Debug.LogError($"Tried to get key {action} but it does not exist");
        }
        return keyCode;
    }

    public void LoadSavedHotkeys()
    {
        LoadSavedKey(HotkeyConstants.SelectPosition, HotkeyConstants.SelectPositionDefault);
        LoadSavedKey(HotkeyConstants.SelectRotation, HotkeyConstants.SelectRotationDefault);
        LoadSavedKey(HotkeyConstants.SelectScale, HotkeyConstants.SelectScaleDefault);
        LoadSavedKey(HotkeyConstants.DeletePrefab, HotkeyConstants.DeletePrefabDefault);
        LoadSavedKey(HotkeyConstants.Focus, HotkeyConstants.FocusDefault);
        LoadSavedKey(HotkeyConstants.Duplicate, HotkeyConstants.DuplicateDefault);
        LoadSavedKey(HotkeyConstants.ModifierKey, HotkeyConstants.ModifierKeyDefault);
    }

    public void LoadSavedKey(string keyName, string defaultValue)
    {
        string key = PlayerPrefs.GetString(keyName, defaultValue);

        KeyCode keyCode;
        if (Enum.TryParse(key, out keyCode))
        {
            keys.Add(keyName, keyCode);
        }
        else
        {
            Debug.Log("Could not parse key code: " + keyName);
        }
    }

    public void SetDefaults()
    {
        keys = new Dictionary<string, KeyCode>(defaults);

        foreach(KeyValuePair<String, KeyCode> entry in keys)
        {
            Debug.Log("SetDefaults KVP: " + entry.Key + ": " + entry.Value); 
            PlayerPrefs.SetString(entry.Key, entry.Value.ToString());
        }
    }

    public void InitDefaults()
    {
        AddDefaultKey(HotkeyConstants.SelectPosition, HotkeyConstants.SelectPositionDefault);
        AddDefaultKey(HotkeyConstants.SelectRotation, HotkeyConstants.SelectRotationDefault);
        AddDefaultKey(HotkeyConstants.SelectScale, HotkeyConstants.SelectScaleDefault);
        AddDefaultKey(HotkeyConstants.DeletePrefab, HotkeyConstants.DeletePrefabDefault);
        AddDefaultKey(HotkeyConstants.Focus, HotkeyConstants.FocusDefault);
        AddDefaultKey(HotkeyConstants.Duplicate, HotkeyConstants.DuplicateDefault); 
    }

    public void AddDefaultKey(string keyName, string defaultValue)
    {
        KeyCode keyCode;
        if (Enum.TryParse(defaultValue, out keyCode))
        {
            defaults.Add(keyName, keyCode);
        }
        else
        {
            Debug.Log("Could not parse default key code: " + keyName);
        }
    }

    public void LoadTooltips()
    {
        tooltips.Add(HotkeyConstants.SelectPosition, HotkeyConstants.SelectPositionTooltip);
        tooltips.Add(HotkeyConstants.SelectRotation, HotkeyConstants.SelectRotationTooltip);
        tooltips.Add(HotkeyConstants.SelectScale, HotkeyConstants.SelectScaleTooltip);
        tooltips.Add(HotkeyConstants.DeletePrefab, HotkeyConstants.DeletePrefabTooltip);
        tooltips.Add(HotkeyConstants.Focus, HotkeyConstants.FocusTooltip);
        tooltips.Add(HotkeyConstants.Duplicate, HotkeyConstants.DuplicateTooltip);
    }



    public static bool GetKeyDown(string keyName)
    {
        return Input.GetKeyDown(HotKeyManager.GetInstance().GetKeyFor(keyName));
    }

    public static bool GetKeyUp(string keyName)
    {
        return Input.GetKeyUp(HotKeyManager.GetInstance().GetKeyFor(keyName));
    }

    public static bool GetKey(string keyName)
    {
        return Input.GetKey(HotKeyManager.GetInstance().GetKeyFor(keyName));
    }

    public static bool GetModifiedKeyDown(string keyName)
    {
        return Input.GetKey(HotKeyManager.GetInstance().GetKeyFor(HotkeyConstants.ModifierKey))
            && Input.GetKeyDown(HotKeyManager.GetInstance().GetKeyFor(keyName));
    }
}
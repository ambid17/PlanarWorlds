using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;

public class TerrainMenu : MonoBehaviour
{
    public Button closeButton;

    private void Awake()
    {
        UIManager.OnEditModeChanged += EditModeChanged;
    }

    void Start()
    {
        closeButton.onClick.AddListener(Close);
    }
    private void Close()
    {
        gameObject.SetActive(false);
    }

    private void EditModeChanged(EditMode newEditMode)
    {
        if(newEditMode != EditMode.Terrain)
        {
            Close();
        }
    }
}

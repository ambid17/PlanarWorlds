using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorModeUI : MonoBehaviour
{
    public Button mapButton;
    public Button propButton;
    public Button encounterButton;

    private UIManager _uiManager;

    private void Awake()
    {
        _uiManager = UIManager.GetInstance();
    }

    void Start()
    {
        mapButton.onClick.AddListener(() => _uiManager.SetEditMode(EditMode.Map));
        propButton.onClick.AddListener(() => _uiManager.SetEditMode(EditMode.Prop));
        encounterButton.onClick.AddListener(() => _uiManager.SetEditMode(EditMode.Encounter));
    }
}
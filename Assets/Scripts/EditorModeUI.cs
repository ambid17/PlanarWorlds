using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorModeUI : MonoBehaviour
{
    public ToggleButton mapButton;
    public ToggleButton propButton;
    public ToggleButton encounterButton;

    private UIManager _uiManager;


    private void Awake()
    {
        _uiManager = UIManager.GetInstance();
    }

    void Start()
    {
        mapButton.SetupAction(() => SelectButton(EditMode.Terrain));
        propButton.SetupAction(() => SelectButton(EditMode.Prefab));
        encounterButton.SetupAction(() => SelectButton(EditMode.Encounter));

        mapButton.Select();
    }

    void SelectButton(EditMode editMode)
    {
        _uiManager.SetEditMode(editMode);

        mapButton.Unselect();
        propButton.Unselect();
        encounterButton.Unselect();
    }
}

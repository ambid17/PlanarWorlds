using Michsky.UI.ModernUIPack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverwriteSaveModal : MonoBehaviour
{
    public ModalWindowManager modalWindowManager;

    public event Action<string, OverwriteType> WillOverwrite;
    public event Action<string> CancelOverwrite;

    private string path;
    private OverwriteType type;

    void Start()
    {
        modalWindowManager.CloseWindow();
        modalWindowManager.confirmButton.onClick.AddListener(Ok);
        modalWindowManager.cancelButton.onClick.AddListener(Cancel);
    }

    public void Show(string path, OverwriteType type)
    {
        this.path = path;
        this.type = type;
        modalWindowManager.OpenWindow();
        modalWindowManager.titleText = "File Overwrite Detected";
        modalWindowManager.descriptionText = $"Are you sure you want to overwrite the following file?\n\t{path}";
        modalWindowManager.UpdateUI();
    }

    private void Ok()
    {
        WillOverwrite?.Invoke(path, type);
        modalWindowManager.CloseWindow();
    }

    private void Cancel()
    {
        CancelOverwrite?.Invoke(path);
        modalWindowManager.CloseWindow();
    }
}

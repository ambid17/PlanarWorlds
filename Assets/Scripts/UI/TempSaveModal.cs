using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.ModernUIPack;

public class TempSaveModal : MonoBehaviour
{
    public ModalWindowManager modalWindowManager;

    void Start()
    {
        modalWindowManager.CloseWindow();
        modalWindowManager.confirmButton.onClick.AddListener(Ok);
    }

    public void Show(string text)
    {
        modalWindowManager.OpenWindow();
        modalWindowManager.titleText = "Notification";
        modalWindowManager.descriptionText = $"We detected you have tried to open a new file without saving.\nWe have saved your current changes to:\n\t{text}";
        modalWindowManager.UpdateUI();
    }

    private void Ok()
    {
        modalWindowManager.CloseWindow();
    }
}

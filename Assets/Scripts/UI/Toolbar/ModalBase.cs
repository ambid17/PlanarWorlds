using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public enum ModalResult
{
    Yes, No, Nothing
}
public class ModalBase : MonoBehaviour
{
    public ModalWindowManager modalWindowManager;
    protected Action<bool> callback;
    protected virtual void Start()
    {
        modalWindowManager.CloseWindow();
        modalWindowManager.confirmButton.onClick.AddListener(OnYes);
        modalWindowManager.cancelButton.onClick.AddListener(OnNo);
    }
    
    private void OnYes()
    {
        callback(true);
        modalWindowManager.CloseWindow();
    }

    private void OnNo()
    {
        callback(false);
        modalWindowManager.CloseWindow();
    }
}

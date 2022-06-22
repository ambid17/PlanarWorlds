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
    
    protected virtual void Start()
    {
        modalWindowManager.CloseWindow();
        modalWindowManager.confirmButton.onClick.AddListener(OnYes);
        modalWindowManager.cancelButton.onClick.AddListener(OnNo);
    }
    
    private void OnYes()
    {
        modalWindowManager.CloseWindow();
    }

    private void OnNo()
    {
        modalWindowManager.CloseWindow();
    }
}

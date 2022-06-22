using Michsky.UI.ModernUIPack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OverwriteType
{
    Save, New
}

public class OverwriteSaveModal : ModalBase
{
    public void Show(string path)
    {
        modalWindowManager.OpenWindow();
        modalWindowManager.titleText = "File Overwrite Detected";
        modalWindowManager.descriptionText = $"Are you sure you want to overwrite the following file?\n\t{path}";
        modalWindowManager.UpdateUI();
    }
}

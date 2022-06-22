using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.ModernUIPack;

public class TempSaveModal : ModalBase
{
    public void Show()
    {
        modalWindowManager.OpenWindow();
        modalWindowManager.titleText = "Notification";
        string filePath = CampaignManager.Instance.CurrentCampaign.filePath;
        modalWindowManager.descriptionText = $"We detected you have tried to open a new file without saving.\nWe have saved your current changes to:\n\t{filePath}";
        modalWindowManager.UpdateUI();
        
    }
}

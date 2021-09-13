using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarUI : MonoBehaviour
{
    public Button fileButton;
    public GameObject fileMenu;

    void Start()
    {
        fileButton.onClick.AddListener(FileButtonClicked);
        fileMenu.SetActive(false);
    }

    void Update()
    {
        
    }

    private void FileButtonClicked()
    {
        fileMenu.SetActive(true);
    }
}

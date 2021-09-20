using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public Button continueButton;
    public Button optionsButton;
    public Button quitButton;

    public GameObject container;
    public GameObject pausePanel;
    public GameObject optionsPanel;

    private UIManager _uiManager;

    private void Awake()
    {
        _uiManager = UIManager.GetInstance();
    }

    void Start()
    {
        continueButton.onClick.AddListener(Continue);
        optionsButton.onClick.AddListener(Options);
        quitButton.onClick.AddListener(Quit);

        container.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !_uiManager.isFileBrowserOpen)
        {
            if (_uiManager.isPaused)
            {
                container.SetActive(false);
            }
            else
            {
                container.SetActive(true);
                pausePanel.SetActive(true);
                optionsPanel.SetActive(false);
            }

            _uiManager.isPaused = !_uiManager.isPaused;
        }    
    }

    void Continue()
    {
        container.SetActive(false);
    }

    void Options()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    void Quit()
    {
        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #else
                            Application.Quit();
        #endif
    }
}

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

    private bool _isPaused;
    public bool IsPaused { get => _isPaused; }

    void Start()
    {
        continueButton.onClick.AddListener(Continue);
        optionsButton.onClick.AddListener(Options);
        quitButton.onClick.AddListener(Quit);

        container.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused)
            {
                container.SetActive(false);
            }
            else
            {
                container.SetActive(true);
                pausePanel.SetActive(true);
                optionsPanel.SetActive(false);
            }

            _isPaused = !_isPaused;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotkeyDisplay : MonoBehaviour
{
    [SerializeField]
    GameObject uIContainer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            uIContainer.SetActive(!uIContainer.activeSelf);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonColorUI : MonoBehaviour
{
    public Image normalImage;
    public Image highlightedImage;

    public Sprite outlineSprite;
    public Sprite filledSprite;

    public Button button;

    private void Start()
    {
        button = GetComponent<Button>();
    }
}

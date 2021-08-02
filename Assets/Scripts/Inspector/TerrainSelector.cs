using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class TerrainSelector : MonoBehaviour
{
    public Material[] materialsForTerrain;
    public GameObject buttonPrefab;

    private PrefabGizmoManager _prefabGizmoManager;

    void Start()
    {
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();
        CreateButtons();
    }

    private void CreateButtons()
    {
        foreach(Material material in materialsForTerrain)
        {
            GameObject newButton = Instantiate(buttonPrefab, transform);
            ButtonManagerBasicIcon buttonManager = newButton.GetComponent<ButtonManagerBasicIcon>();
            Texture2D myTex = (Texture2D) material.mainTexture;
            Sprite buttonSprite = Sprite.Create(myTex, new Rect(0, 0, myTex.width, myTex.height), new Vector2(0.5f, 0.5f));
            buttonManager.buttonIcon = buttonSprite;
            buttonManager.UpdateUI();

            Button myButton = newButton.GetComponent<Button>();
            myButton.onClick.AddListener(() => SetTerrainMaterial(material));
        }
    }

    private void SetTerrainMaterial(Material material)
    {
        _prefabGizmoManager.SetMaterialForCurrentObject(material);
    }
}

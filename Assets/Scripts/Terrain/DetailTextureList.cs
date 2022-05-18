using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TextureList", menuName = "ScriptableObjects/TextureList")]
public class DetailTextureList : ScriptableObject 
{
    [SerializeField]
    public Texture2D[] textures;
}

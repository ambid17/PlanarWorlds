using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterInstanceData : PrefabInstanceData, IComparable, IEquatable<CharacterInstanceData>
{
    // We need some way to tell that the character instances are unique
    // This is due to the Dictionary in InitiativeUI
    public int instanceId;
    public string characterName;
    public int characterHp;
    public int characterSpeed;
    public int characterInitiative;

    private TMP_Text nameText;

    private void Start()
    {
        nameText = GetComponentInChildren<TMP_Text>();
    }

    public CharacterModel GetCharacterModel()
    {
        CharacterModel myModel = new CharacterModel()
        {
            position = new MyVector3(transform.position),
            rotation = new MyVector3(transform.rotation.eulerAngles),
            scale = new MyVector3(transform.localScale),
            prefabId = prefabId,
            name = characterName,
            prefabType = prefabType,
            characterHp = characterHp,
            characterSpeed = characterSpeed,
            characterInitiative = characterInitiative
        };

        return myModel;
    }

    public void UpdateName(string newName)
    {
        characterName = newName;
        nameText.text = newName;
    }

    // Sorts the players in descending initiative order
    public int CompareTo(object obj)
    {
        CharacterInstanceData characterToCompare = obj as CharacterInstanceData;
        
        if (characterToCompare == null)
        {
            return 1;
        }

        if(characterToCompare.characterInitiative < characterInitiative)
        {
            return -1;
        }

        if(characterToCompare.characterInitiative > characterInitiative)
        {
            return 1;
        }

        return 0;
    }

    public bool Equals(CharacterInstanceData other)
    {
        return other != null && other.instanceId == this.instanceId;
    }

    public int GetHashCode()
    {
        return instanceId;
    }
}

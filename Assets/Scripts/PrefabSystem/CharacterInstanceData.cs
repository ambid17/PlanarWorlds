using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInstanceData : PrefabInstanceData, IComparable
{
    public string characterName;
    public int characterHp;
    public int characterSpeed;
    public int characterInitiative;

    public CharacterModel GetCharacterModel()
    {
        CharacterModel myModel = new CharacterModel()
        {
            position = transform.position,
            rotation = transform.rotation.eulerAngles,
            scale = transform.localScale,
            prefabId = prefabId,
            name = characterName,
            prefabType = prefabType,
            characterHp = characterHp,
            characterSpeed = characterSpeed,
            characterInitiative = characterInitiative
        };

        return myModel;
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
}

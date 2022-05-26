using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class Campaign
{
    [SerializeField]
    public List<PrefabModel> props;
    [SerializeField]
    public List<CharacterModel> characters;
    [SerializeField]
    public string filePath;
    [SerializeField]
    public TerrainModel terrainData;

    public Campaign()
    {
        props = new List<PrefabModel>();
        characters = new List<CharacterModel>();
    }

    #region Serialization
    public void Save()
    {
        if(filePath == null || filePath == string.Empty)
        {
            SetTempFilePath();
        }
        
        FileStream fs = new FileStream(filePath, FileMode.Create);

        // Construct a BinaryFormatter and use it to serialize the data to the stream.
        BinaryFormatter formatter = new BinaryFormatter();
        try
        {
            formatter.Serialize(fs, this);
        }
        catch (SerializationException e)
        {
            Console.WriteLine("Failed to serialize. Reason: " + e.Message);
            throw;
        }
        finally
        {
            fs.Close();
        }
    }

    public static Campaign LoadFromPath(string filePath)
    {
        Campaign campaign = new Campaign();
        
        FileStream fs = new FileStream(filePath, FileMode.Open);
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            campaign = (Campaign) formatter.Deserialize(fs);
        }
        catch (SerializationException e)
        {
            Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
            throw;
        }
        finally
        {
            fs.Close();
        }

        return campaign;
    }

    // Find the next unused campaign name
    // i.e. : tempCampaign0, tempCampaign1, etc
    public string SetTempFilePath()
    {
        string baseFilePath = FilePathUtil.GetSaveFolder();

        int fileCounter = 1;
        string fullFilePath = Path.Combine(baseFilePath, $"tempCampaign{fileCounter}.plane");

        while (File.Exists(fullFilePath))
        {
            fileCounter++;
            fullFilePath = Path.Combine(baseFilePath, $"tempCampaign{fileCounter}.plane");
        }

        filePath = fullFilePath;
        return fullFilePath;
    }
    #endregion
}

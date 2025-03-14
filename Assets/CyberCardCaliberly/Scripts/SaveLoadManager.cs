using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveLoadManager
{
    public static PlayerLevelArray playerLevelArray;
    
    // Define your data class

    public static void SaveGame()
    {
        string jsonString = JsonUtility.ToJson(playerLevelArray);
        string filePath = Application.persistentDataPath + "/save.json";

        using (StreamWriter file = new StreamWriter(filePath))
        {
            file.WriteLine(jsonString);
        }
        Debug.Log("Game Saved!");
    }

    public static void LoadGame()
    {
        string filePath = Application.persistentDataPath + "/save.json";

        if (File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);
            playerLevelArray = JsonUtility.FromJson<PlayerLevelArray>(jsonString);
            Debug.Log("Game Loaded!");
        }
        else
        {
            Debug.Log("No save file found!");
        }
    }
}

[System.Serializable]
public class PlayerLevelData {
    public int levelNumber;
    public int turnCount;
    public int matchScore;
}

[System.Serializable]
public class PlayerLevelArray
{
    public List<PlayerLevelData> playerLevelDataArray = new();
}
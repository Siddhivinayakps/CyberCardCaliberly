using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Level Design/Level Data")]
public class LevelDataArray : ScriptableObject
{
    public LevelData[] levelDatas;
}

[System.Serializable]
public class LevelData {
    public int levelNumber;
    public int rows;
    public int columns;
    public int matchScore;
    public int streakScoreMultiplier;
    public int cardCountToMatch;
}
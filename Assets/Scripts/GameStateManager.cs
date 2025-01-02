using System.IO;
using UnityEngine;

[System.Serializable]
public class GameState
{
    public int score;
    public float time;
    public int[] cardIDs;
    public int[] cardSpriteIDs;
    public bool[] cardFlippedStates;
    public Vector3[] cardPositions;
    public Color[] cardColors;
    public Vector3[] cardScales;

}

public static class GameStateManager
{


    private const string saveFileName = "SavedGame.json";

    public static GameState LoadGame()
    {
        string filePath = Application.persistentDataPath + "/" + saveFileName;
        if (!File.Exists(filePath))
        {
            return null;
        }
        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<GameState>(json);
    }

    public static void SaveGame(GameState state)
    { 
       string json =   JsonUtility.ToJson(state);
       string filePath = Application.persistentDataPath + "/" + saveFileName;
        File.WriteAllText(filePath, json);
        Debug.Log(Application.persistentDataPath);
    
    }

   
}

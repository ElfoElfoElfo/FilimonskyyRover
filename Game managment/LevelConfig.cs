using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Scriptable Objects/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [Header("Level info")]
    public int level;
    public int nextLevel;
    public string title;
    [TextArea(3, 10)] public string hintText;

    [Header("Initial player state")]
    public Vector2Int startPosition;
    public Vector2Int startForward;

    [Header("Grading")]
    public int bestLineCount;
    public int worstLineCount;
}

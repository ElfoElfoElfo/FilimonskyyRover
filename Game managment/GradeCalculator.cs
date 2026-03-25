using Unity.Mathematics;
using TMPro;
using UnityEngine;

public class GradeCalculator : MonoBehaviour
{
    LevelManager manager;

    void Start()
    {
        manager = GetComponent<LevelManager>();
    }

    public int GetNonEmptyLineCount(TMP_Text tmpText)
    {
        // Ensure mesh data is up to date before checking line info
        tmpText.ForceMeshUpdate();

        // Access the textInfo which stores metadata about the text layout
        TMP_TextInfo textInfo = tmpText.textInfo;

        int nonEmptyCount = 0;
        for (int i = 0; i < textInfo.lineCount; i++)
        {
            // A line is "empty" if it has 0 characters or is a comment
            if (textInfo.lineInfo[i].visibleCharacterCount > 0 && textInfo.characterInfo[textInfo.lineInfo[i].firstVisibleCharacterIndex].character != '#')
            {
                nonEmptyCount++;
            }
        }
        return nonEmptyCount;
    }

    public int CalculateGrade(int lineCount)
    {
        int clampedLineCount = Mathf.Clamp(lineCount, manager.levelConfig.bestLineCount, manager.levelConfig.worstLineCount);
        float mappedLineCount = math.remap(manager.levelConfig.bestLineCount, manager.levelConfig.worstLineCount, 5, 2, clampedLineCount);

        return (int)Mathf.Round(mappedLineCount);
    }
}

using TMPro;
using UnityEngine;

public class LevelComplete : MonoBehaviour
{
    public GradeCalculator calculator;
    public GameObject levelCompleteTab;

    [Header("Grading")]
    public TextMeshProUGUI gradeText;


    void Start()
    {
        levelCompleteTab.SetActive(false);
    }

    public void LevelCompleteProcedure(int lines)
    {
        gradeText.text = $"Оценка : {calculator.CalculateGrade(lines)}";
        levelCompleteTab.SetActive(true);
    }
}

using System.IO;
using UnityEngine;
using TMPro;

public class CodeUploader : MonoBehaviour
{
    public int lineCount;

    [Header("Code input field")]
    public TMP_InputField field;

    [Header("Config")]
    LevelConfig config;
    public GradeCalculator grader;

    string path;
    
    // Components
    RoverInterpreter interpreter;
    RoverController controller;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interpreter = GetComponent<RoverInterpreter>();
        controller = GetComponent<RoverController>();
        config = controller.levelConfig;
    }

    void Update()
    {
        lineCount = grader.GetNonEmptyLineCount(field.textComponent);
    }

    public void CompileCode()
    {
        interpreter.ExecuteCode(field.text);
        
        SaveCode();
    }

    public void RunCode()
    {
        controller.MasterReset();

        if (!controller.animatingCode)
        {
            CompileCode();
        }
        else
        {
            Debug.Log("Code execution interrupted");

            controller.StopAllCoroutines();
            controller.animatingCode = false;
        }
    }

    // Мы пользуемся функционалом save/load из CodeStorageService.
    public void SaveCode()
    {
        CodeStorageService.SaveCode($"level{config.level}", field.text);
    }

    public void LoadCode()
    {
        field.text = CodeStorageService.LoadCode($"level{config.level}");
    }
}

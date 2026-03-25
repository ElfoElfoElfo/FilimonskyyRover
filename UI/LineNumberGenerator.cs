using TMPro;
using UnityEngine;

public class LineNumberGenerator : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI lineNumbersText;

    void Start()
    {
        // Initial update
        UpdateLineNumbers();
        // Subscribe to changes in the input field
        inputField.onValueChanged.AddListener(delegate { UpdateLineNumbers(); });
    }

    public void UpdateLineNumbers()
    {
        string lineNumbers = "";
        // Split the text by newlines to count them
        string[] lines = inputField.text.Split('\n');

        for (int i = 1; i <= lines.Length; i++)
        {
            lineNumbers += i + "\n";
        }

        lineNumbersText.text = lineNumbers;
    }
}
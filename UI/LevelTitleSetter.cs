using TMPro;
using UnityEngine;

public class LevelTitleSetter : MonoBehaviour
{
    public LevelManager manager;
    public TextMeshProUGUI levelTitle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelTitle.text = $"Уровень {manager.levelConfig.level} - <i>{manager.levelConfig.title}</i>";
    }
}

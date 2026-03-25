using TMPro;
using UnityEngine;

public class ToggleHint : MonoBehaviour
{
    public LevelManager manager;
    public GameObject hintTab;
    public TextMeshProUGUI hintText;

    private void Start()
    {
        hintTab.SetActive(true);
        hintText.text = manager.levelConfig.hintText;
    }
    public void ToggleHintTab()
    {
        hintTab.SetActive(!hintTab.activeInHierarchy);
    }
}

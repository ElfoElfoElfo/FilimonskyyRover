using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public LevelConfig levelConfig;
    
    public void LoadLevel(int level)
    {
        SceneManager.LoadScene($"Level{level}");
    }
    public void LoadNextLevel()
    {
        LoadLevel(levelConfig.nextLevel);
    }
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

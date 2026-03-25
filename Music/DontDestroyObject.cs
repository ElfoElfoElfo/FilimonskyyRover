using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroyObject : MonoBehaviour
{
    // Static reference to the active instance
    public static GameObject instance { get; private set; }

    private void Awake()
    {
        // 1. Check if an instance already exists in the DDOL scene
        if (instance != null && instance != this.gameObject)
        {
            // 2. If a duplicate is found, destroy this new one immediately
            Destroy(gameObject);
            return;
        }

        // 3. Set this as the active instance and protect it from scene unloads
        instance = this.gameObject;
        DontDestroyOnLoad(gameObject);
    }
}

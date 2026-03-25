using UnityEngine;

public class CodeStorageService
{
    // A collection of methods to save and load code.
    // For desktop/mobile builds we use System.IO. But for WebGL we'll use PlayerPrefs, because System.IO isn't supported by WebGL
    
    // Save code to path (key)
    public static void SaveCode(string key, string code)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PlayerPrefs.SetString(key, code);
        PlayerPrefs.Save();
#else
        string path = System.IO.Path.Combine(Application.persistentDataPath, $"{key}.py");
        System.IO.File.WriteAllText(path, code);
#endif
    }

    // Load code at path (key)
    public static string LoadCode(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return PlayerPrefs.GetString(key, "");
#else
        string path = System.IO.Path.Combine(Application.persistentDataPath, $"{key}.py");
        if (System.IO.File.Exists(path))
        {
            return System.IO.File.ReadAllText(path);
        }
        return "";
#endif
    }

    // Check if code exists at path (key)
    public static bool HasSavedCode(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return PlayerPrefs.HasKey(key);
#else
        string path = System.IO.Path.Combine(Application.persistentDataPath, $"{key}.py");
        return System.IO.File.Exists(path);
#endif
    }
}

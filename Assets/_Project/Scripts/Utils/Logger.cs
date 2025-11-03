using UnityEngine;
using System;

public class Logger : MonoBehaviour
{
    public static Logger Instance;

    [Header("Logging Toggles")]
    public bool showGameActions = true;
    public bool showDebug = true;
    

    [Header("Log Colors")]
    public Color gameActionColor = Color.green;
    public Color debugColor = Color.blue;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject); // Optional: persist across scenes
    }

    public enum LogTypeCategory
    {
        GameAction,
        Debug,
    }

    public static void Log(string message, LogTypeCategory type = LogTypeCategory.Debug)
    {
        #if UNITY_EDITOR
        if (Instance == null)
        {
            Debug.LogWarning("Logger not initialized. Add Logger to a scene GameObject.");
            Debug.Log(message);
            return;
        }

        if (!Instance.IsEnabled(type)) return;

        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Color color = Instance.GetColor(type);
        string hexColor = ColorUtility.ToHtmlStringRGB(color);

        Debug.Log($"<color=#{hexColor}>[{timestamp}] [{type}]</color> {message}");
        #endif
    }

    private bool IsEnabled(LogTypeCategory type)
    {
        return type switch
        {
            LogTypeCategory.GameAction => showGameActions,
            LogTypeCategory.Debug => showDebug,
            _ => true
        };
    }

    private Color GetColor(LogTypeCategory type)
    {
        return type switch
        {
            LogTypeCategory.GameAction => gameActionColor,
            LogTypeCategory.Debug => debugColor,
            _ => Color.white
        };
    }
}

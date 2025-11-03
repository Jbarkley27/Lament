using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public string VeilSceneName;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found a GlobalDataStore Manager object, destroying new one");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        #if UNITY_EDITOR
            Debug.Log("Running inside the Unity Editor");
        #else
            LoadVeilUniverse();
        #endif
    }


    public void LoadVeilUniverse()
    {
        if (!string.IsNullOrEmpty(VeilSceneName))
        {
            SceneManager.LoadScene(VeilSceneName, LoadSceneMode.Additive);
            Logger.Log($"Creating Veil Universe");
        }
        else
        {
            Debug.LogWarning("Scene name to load is not assigned in the Inspector.");
        }
    }

}
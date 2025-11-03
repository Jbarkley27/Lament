using Unity.Cinemachine;
using UnityEngine;

public class GlobalDataStore : MonoBehaviour
{
    public static GlobalDataStore Instance;
    public InputManager InputManager;
    public PlayerMovement PlayerMovement;
    public GameObject PlayerVisual;
    public MapManager MapManager;
    public GameObject OverviewMapRoot;

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
    }
}
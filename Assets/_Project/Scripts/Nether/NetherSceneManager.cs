using UnityEngine;
using UnityEngine.SceneManagement;

public class NetherSceneManager : MonoBehaviour
{
    // Scene management code here
    void Start()
    {
        #if !UNITY_EDITOR
                SceneManager.LoadScene("Scene_01", LoadSceneMode.Additive);
        #endif
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    private void OnEnable()
    {
        YaSDK.SDKInitialized += () => SceneManager.LoadScene(1);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        YaSDK.WhenReady(() => SceneManager.LoadScene(1));
        YaSDK.WhenReady(OnSDKReady);
    }

    private void OnSDKReady()
    {
        Debug.Log("SDK Ready");
        SceneManager.LoadScene(1);
    }
}

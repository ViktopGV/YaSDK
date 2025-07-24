using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(WaitForSDK());
    }

    IEnumerator WaitForSDK()
    {
        yield return new WaitUntil(() => YaSDK.IsSDKReady() == true);
        SceneManager.LoadScene(1);
    }
}

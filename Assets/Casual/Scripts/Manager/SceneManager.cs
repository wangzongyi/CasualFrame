using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Casual
{
    public class SceneManager
    {
        static public void LoadSceneAsync(string sceneName, Action callback = null, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            CoroutineAgent.StartCoroutine(DoLoadSceneAsync(sceneName, callback, loadSceneMode));
        }

        static private IEnumerator DoLoadSceneAsync(string sceneName, Action callback, LoadSceneMode loadSceneMode)
        {
            AsyncOperation operation = USceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            while (!operation.isDone)
            {
                yield return null;
            }

            yield return null;

            callback?.Invoke();
        }

        static public void UnloadSceneAsync(string sceneName, Action action = null)
        {
            CoroutineAgent.StartCoroutine(DoUnloadSceneAsync(sceneName, action));
        }

        static private IEnumerator DoUnloadSceneAsync(string sceneName, Action action)
        {
            yield return USceneManager.UnloadSceneAsync(sceneName);
            action?.Invoke();
        }
    }
}



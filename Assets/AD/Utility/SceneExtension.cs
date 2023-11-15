using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AD.Utility
{
    public struct SceneExtenedInfomation
    {
        public AsyncOperation asyncOperation;
    }

    static public class SceneExtension
    {
        #region SceneConversion

        public static void SceneConversion(this string targetname, LoadSceneMode mode = LoadSceneMode.Single)
        {
            SceneManager.LoadScene(targetname, mode);
        }

        public static void SceneConversion(this int targetScene, LoadSceneMode mode = LoadSceneMode.Single)
        {
            SceneManager.LoadScene(targetScene, mode);
        }

        public static IEnumerator SceneConversion(this string targetname, SceneExtenedInfomation results_asyncInfo)
        {
            results_asyncInfo.asyncOperation = SceneManager.LoadSceneAsync(targetname);
            results_asyncInfo.asyncOperation.allowSceneActivation = false;
            yield return results_asyncInfo.asyncOperation;
        }

        #endregion

        public static UnityEngine.SceneManagement.Scene Get()
        {
            return SceneManager.GetActiveScene();
        }

        /*
        Scene scene = SceneManager.GetActiveScene();
        Debug.Log(scene.name);

        Debug.Log(scene.isLoaded);

        Debug.Log(scene.path);

        Debug.Log(scene.buildIndex);
 

        GameObject[] gos = scene.GetRootGameObjects();
        Debug.Log(gos.Length);
 

       Scene newS= SceneManager.CreateScene("123");
 

        SceneManager.UnloadSceneAsync(newS);
 

        Debug.Log(SceneManager.sceneCount); 
        */

    }

    static public class AsyncOperationExtension
    {
        public static bool IsDone(this AsyncOperation operation,float targetProgress)
        {
            Debug.Log("now progress : " + operation.progress);
            if (operation.progress >= targetProgress)
            {
                Debug.Log("The operation is complete");
                return true;
            }
            else return false;
        }
    }
}
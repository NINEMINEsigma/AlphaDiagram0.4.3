using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static AsyncOperation LoadSceneAsync(this string targetname,LoadSceneMode mode = LoadSceneMode.Single)
        {
            return SceneManager.LoadSceneAsync(targetname, mode);
        }

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

        public static Scene CreateNewScene(this string newSceneName)
        {
            return SceneManager.CreateScene(newSceneName);
        }

        #endregion

        public static Scene Get()
        {
            return SceneManager.GetActiveScene();
        }

        public static Scene GetCurrent()
        {
            return Get();
        }

        public static GameObject[] GetCurrentRootGameObject()
        {
            try
            {
                return GetCurrent().GetRootGameObjects();
            }
            catch
            {
                return new GameObject[] { };
            }
        }

        public static GameObject[] Find(this Scene self,Predicate<GameObject> predicate)
        {
            List<GameObject> allGameObject = self.GetRootGameObjects().ToList();
            List<GameObject> found = new();
            found.AddRange(allGameObject);
            while(found.Count!=0)
            {
                List<GameObject> temp = new();
                foreach (var single in found)
                {
                    foreach (Transform child in single.transform)
                    {
                        if (predicate(child.gameObject))
                        {
                            temp.Add(child.gameObject);
                            allGameObject.Add(child.gameObject);
                        }
                    }
                }
                found = temp;
            }
            return allGameObject.ToArray();
        }

        public static AsyncOperation UnloadSceneAsync(this string self)
        {
            return SceneManager.UnloadSceneAsync(self);
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
}

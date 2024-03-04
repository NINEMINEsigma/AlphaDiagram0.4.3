using System;
using System.Collections.Generic;
using System.Linq;
using AD.BASE;
using AD.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AD.Experimental.Performance
{
    public interface ISceneLoadAssets
    {
        void Load(string sceneName);
        AsyncOperation Unload(string sceneName);
    }

    [Serializable]
    public class MainSceneLoadAssets : ISceneLoadAssets
    {
        public ADSerializableDictionary<string, SubSceneLoader> SubBlocks = new();
        public Scene? MainCurrent;
        public bool IsAllSceneByPreAssets = true;
        public bool IsAllSceneByNewCreate = false;

        public SubSceneLoader SubSceneLoaderPrefab;

        public void Load(string sceneName, Vector3 position)
        {
            Load(sceneName);
            SubBlocks[sceneName].transform.position = position;
        }

        public virtual void Load(string sceneName)
        {
            Scene scene;
            SubSceneLoader subLoader = null;
            try
            {
                if (IsAllSceneByNewCreate) throw new ADException();
                sceneName.SceneConversion(LoadSceneMode.Additive);
                scene = SceneManager.GetSceneByName(sceneName);

                subLoader = scene.GetRootGameObjects().GetSubList(T =>
                {
                    var temp = T.GetComponents<SubSceneLoader>();
                    return temp != null && temp.Length > 0;
                }).First().GetComponents<SubSceneLoader>()[0];
            }
            catch
            {
                if (IsAllSceneByPreAssets) throw;
                if (subLoader != null)
                {
                    GameObject.Destroy(subLoader.gameObject);
                }
                scene = sceneName.CreateNewScene();
                subLoader = GameObject.Instantiate(SubSceneLoaderPrefab);
                subLoader.gameObject.SetActive(true);
                subLoader.name = sceneName + "(Root)";
                subLoader.transform.position = Vector3.zero;
            }
            if (SceneManager.SetActiveScene(scene))
            {
                subLoader.Scene = scene;
                subLoader.SceneIndex = SceneManager.sceneCount;
                subLoader.SceneName = sceneName;
                subLoader.MainLoadAssets = this;
                subLoader.Load();
                SubBlocks[sceneName] = subLoader;
            }
            else throw new ADException("Load Failed");
            SceneManager.SetActiveScene(MainCurrent.GetValueOrDefault());
        }

        public virtual AsyncOperation Unload(string sceneName)
        {
            SubBlocks[sceneName].Unload();
            SubBlocks.Remove(sceneName);
            return sceneName.UnloadSceneAsync();
        }

        public virtual void UnloadAll()
        {
            List<string> keyList = SubBlocks.GetSubListAboutKey();
            foreach (var key in keyList)
            {
                Unload(key);
            }
        }
    }

    public class MainSceneLoader : ADController
    {
        public MainSceneLoadAssets sceneLoadAssets = new();

        private void Start()
        {
            sceneLoadAssets.MainCurrent ??= SceneExtension.GetCurrent();
        }

        public void Load(string sceneName, Vector3 position)
        {
            sceneLoadAssets.Load(sceneName, position);
        }

        public virtual void Load(string sceneName)
        {
            sceneLoadAssets.Load(sceneName);
        }

        public virtual void Unload(string sceneName)
        {
            sceneLoadAssets.Unload(sceneName);
        }

        public virtual void UnloadAll()
        {
            sceneLoadAssets.UnloadAll();
        }

        private void OnDestroy()
        {
            UnloadAll();
        }

        public override void Init()
        {
            UnloadAll();
        }
    }
}

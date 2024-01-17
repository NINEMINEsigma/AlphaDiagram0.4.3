using System;
using AD.BASE;
using AD.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AD.Experimental.Performance
{
    [Serializable]
    public class MainSceneLoadAssets
    {
        public ADSerializableDictionary<string, SubSceneLoader> SubBlocks = new();
        public Scene? MainCurrent;

        public SubSceneLoader SubSceneLoaderPrefab;

        public void Load(string sceneName, Vector3 position)
        {
            Load(sceneName);
            SubBlocks[sceneName].transform.position = position;
        }

        public virtual void Load(string sceneName)
        {
            Scene scene = sceneName.CreateNewScene();
            if (SceneManager.SetActiveScene(scene))
            {
                var game_object = GameObject.Instantiate(SubSceneLoaderPrefab);
                game_object.gameObject.SetActive(true);
                game_object.name = sceneName + "(Root)";
                game_object.transform.position = Vector3.zero;
                SubSceneLoader cat = game_object.GetComponent<SubSceneLoader>();
                cat.Scene = scene;
                cat.SceneIndex = SceneManager.sceneCount;
                cat.SceneName = sceneName;
                cat.MainLoadAssets = this;
                cat.Load();
                SubBlocks[sceneName] = cat;
            }
            else throw new ADException("Create Failed");
            SceneManager.SetActiveScene(MainCurrent.GetValueOrDefault());
        }

        public virtual void Unload(string sceneName)
        {
            SubBlocks[sceneName].Unload();
            SubBlocks.Remove(sceneName);
            SceneManager.UnloadSceneAsync(sceneName);
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

        public override void Init()
        {

        }
    }
}
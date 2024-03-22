using AD.BASE;
using AD.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AD.Experimental.Performance
{
    public class MulSceneLoader : ADController, ISceneLoadAssets
    {
        public string KeySceneName;
        public Scene MainCurrent { get; private set; }

        private void OnEnable()
        {
            MainCurrent = SceneExtension.GetCurrent();
        }

        public virtual void Load(string SceneName)
        {
            KeySceneName.LoadSceneAsync(LoadSceneMode.Additive);
            SceneManager.SetActiveScene(MainCurrent);
        }

        public virtual AsyncOperation Unload(string SceneName)
        {
            return KeySceneName.UnloadSceneAsync();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(!string.IsNullOrEmpty(KeySceneName)) Unload(KeySceneName);
        }

        public override void Init()
        {
            if (!string.IsNullOrEmpty(KeySceneName)) Unload(KeySceneName);
        }
    }
}

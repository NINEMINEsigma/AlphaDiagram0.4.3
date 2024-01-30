using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AD.Experimental.Performance
{
    public class SubSceneLoader : MonoBehaviour
    {
        public int SceneIndex;
        public Scene Scene;
        public string SceneName;

        public ISceneLoadAssets MainLoadAssets;

        public List<GameObject> Childs = new();

        protected virtual void OnDestroy()
        {
            foreach (var child in Childs)
            {
                GameObject.Destroy(child);
            }
        }

        protected virtual IEnumerator HowLoading()
        {
            yield return null;
            Debug.Log(SceneName);
        }

        public virtual void Load()
        {
            StartCoroutine(HowLoading());
        }

        public virtual void Reload()
        {
            foreach (var child in Childs)
            {
                GameObject.Destroy(child);
            }
            Load();
        }

        public virtual void Unload()
        {
            foreach (var child in Childs)
            {
                GameObject.Destroy(child);
            }
        }

    }
}

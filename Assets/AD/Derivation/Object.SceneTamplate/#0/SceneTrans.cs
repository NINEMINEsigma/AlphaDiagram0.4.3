using AD.BASE;
using AD.Utility;
using UnityEngine;

namespace AD.Experimental.SceneTrans
{
    public class SceneTrans : MonoBehaviour
    {
        public ADSerializableDictionary<string, ADEvent> SceneOpenEvent = new();
        public ADSerializableDictionary<string, ADEvent> SceneCloseEvent = new();

        public ADSerializableDictionary<string, string> SceneOpenAnimation = new();
        public ADSerializableDictionary<string, string> SceneCloseAnimation = new();

        [SerializeField] private Animator animator;
        public Animator Animator
        {
            get
            {
                if (animator == null) animator = GetComponent<Animator>();
                return animator;
            }
            set
            {
                if (value != null) animator = value;
            }
        }

        protected virtual void Start()
        {
            SceneBaseController sceneBase = null;
            var temp = GameObject.FindObjectsOfType<SceneBaseController>();
            if (temp.Length == 0) throw new ADException("Scene Controller is not found");
            else if (temp.Length == 1) sceneBase = temp[0];
            else if (temp.Length == 2)
            {
                if (temp[0] is ADGlobalSystem) sceneBase = temp[1];
                else if (temp[1] is ADGlobalSystem) sceneBase = temp[0];
                else throw new ADException("There have too much Scene Controller");
            }
            else
            {
                throw new ADException("There have too much Scene Controller");
            }
            if (GameObject.FindObjectsOfType<SceneTrans>().Length > 1)
            {
                instance.SetupCurrentSceneBaseEvent(sceneBase);
                instance.SetupCurrentAnimation(sceneBase);
                DestroyImmediate(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
                SetupCurrentSceneBaseEvent(sceneBase);
                SetupCurrentAnimation(sceneBase);
            }
        }

        private void SetupCurrentSceneBaseEvent(SceneBaseController sceneBase)
        {
            if (SceneOpenEvent.TryGetValue(SceneExtension.GetCurrent().name, out var openAction))
            {
                openAction.Invoke();
            }
            sceneBase.OnSceneEnd.AddListener(() =>
            {
                if (SceneCloseEvent.TryGetValue(sceneBase.TargetSceneName, out var targetAction))
                {
                    targetAction.Invoke();
                }
            });
        }

        private void SetupCurrentAnimation(SceneBaseController sceneBase)
        {
            if (SceneOpenAnimation.TryGetValue(SceneExtension.GetCurrent().name, out var openAnimtion))
            {
                Animator.Play(openAnimtion);
            }
            sceneBase.OnSceneEnd.AddListener(() =>
            {
                if (SceneCloseAnimation.TryGetValue(SceneExtension.GetCurrent().name, out var targetAnimation))
                {
                    Animator.Play(targetAnimation);
                }
            });
        }

        public static SceneTrans instance;

        public void PlayAnimation(string key)
        {
            if (SceneOpenAnimation.TryGetValue(key, out var openAnimtion))
            {
                Animator.Play(openAnimtion);
            }
            else if (SceneCloseAnimation.TryGetValue(key, out var closeAnimtion))
            {
                Animator.Play(closeAnimtion);
            }
        }
        public void PlayAction(string key)
        {
            if (SceneOpenEvent.TryGetValue(key, out var openAction))
            {
                openAction.Invoke();
            }
            else if (SceneCloseEvent.TryGetValue(key, out var closeAction))
            {
                closeAction.Invoke();
            }
        }

        public void Replace(string key, string value)
        {
            if (SceneOpenAnimation.ContainsKey(key)) SceneOpenAnimation[key] = value;
            if (SceneCloseAnimation.ContainsKey(key)) SceneCloseAnimation[key] = value;
        }

        public void Replace(string key, ADEvent value)
        {
            if (SceneOpenEvent.ContainsKey(key)) SceneOpenEvent[key] = value;
            if (SceneCloseEvent.ContainsKey(key)) SceneCloseEvent[key] = value;
        }
    }
}

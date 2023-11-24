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

        protected virtual void Awake()
        {
            var temp = GameObject.FindObjectsOfType<SceneBaseController>();
            if (temp.Length == 0)
            {
                throw new ADException("Scene Controller is not found");
            }
            if (temp.Length > 1)
            {
                throw new ADException("There have too much Scene Controller");
            }
            SceneBaseController sceneBase = temp[0];
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
                if (SceneCloseAnimation.TryGetValue(sceneBase.TargetSceneName, out var targetAnimation))
                {
                    Animator.Play(openAnimtion);
                }
            });
        }

        public static SceneTrans instance;

        public void PlayAnimation(string key)
        {
            if (SceneOpenAnimation.TryGetValue(key, out var openAnimtion))
            {
                animator.Play(openAnimtion);
            }
            else if (SceneCloseAnimation.TryGetValue(key, out var closeAnimtion))
            {
                animator.Play(closeAnimtion);
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
    }
}

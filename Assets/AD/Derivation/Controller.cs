using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using UnityEngine;

namespace AD
{
    [Serializable]
    public abstract class BaseController : AD.BASE.ADController
    {
        public override void Init()
        {
            mCanvasInitializer.Initialize();
        }

        [Header("BaseController")]
        [SerializeField] private AD.UI.CanvasInitializer mCanvasInitializer = new AD.UI.CanvasInitializer();
    }
    [Serializable]
    public abstract class BaseController<T> : AD.BASE.ADController
    {
        public override void Init()
        {
            mCanvasInitializer.Initialize();
        }

        [Header("BaseController")]
        [SerializeField] private AD.UI.CanvasInitializer mCanvasInitializer = new AD.UI.CanvasInitializer();
        public ADEvent<T> OnEvent = new ADEvent<T>();
    }
    [Serializable]
    public abstract class BaseController<T1, T2> : AD.BASE.ADController
    {
        public override void Init()
        {
            mCanvasInitializer.Initialize();
        }

        [Header("BaseController")]
        [SerializeField] private AD.UI.CanvasInitializer mCanvasInitializer = new AD.UI.CanvasInitializer();
        public ADEvent<T1, T2> OnEvent = new ADEvent<T1, T2>();
    }
    [Serializable]
    public abstract class BaseController<T1, T2, T3> : AD.BASE.ADController
    {
        public override void Init()
        {
            mCanvasInitializer.Initialize();
        }

        [Header("BaseController")]
        [SerializeField] private AD.UI.CanvasInitializer mCanvasInitializer = new AD.UI.CanvasInitializer();
        public ADEvent<T1, T2, T3> OnEvent = new ADEvent<T1, T2, T3>();
    }
    [Serializable]
    public abstract class BaseController<T1, T2, T3, T4> : AD.BASE.ADController
    {
        public override void Init()
        {
            mCanvasInitializer.Initialize();
        }

        [Header("BaseController")]
        [SerializeField] private AD.UI.CanvasInitializer mCanvasInitializer = new AD.UI.CanvasInitializer();
        public ADEvent<T1, T2, T3, T4> OnEvent = new ADEvent<T1, T2, T3, T4>();
    }

    public interface ISceneSingleController
    {
        MonoBehaviour SceneMono {  get; }
    }
    public interface ICanPrepareToOtherScene
    {
        void PrepareToOtherScene();
    }

    [Serializable]
    public abstract class SceneBaseController : BaseController, ISceneSingleController
    {
        [Header("SceneBaseController")]
        public string TargetSceneName = "";

        protected virtual void Awake()
        {
            SceneSingleAssets.Init();
            base.Init();

            InitFormLastSceneInfo();

            if (TargetSceneName == "") TargetSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            SceneSingleAssets.instence = this;
            OnSceneStart.Invoke();
        }

        protected virtual void InitFormLastSceneInfo(object info)
        {

        }

        public void InitFormLastSceneInfo()
        {
            InitFormLastSceneInfo(SceneSingleAssets.infomation);
        }

        public virtual void OnEnd()
        {
            foreach (var source in SceneSingleAssets.audioSourceControllers) source.PrepareToOtherScene();

            OnSceneEnd.Invoke();

            StartCoroutine(HowToLoadScene());
        }

        protected virtual IEnumerator HowToLoadScene()
        {
            yield return null;
            UnityEngine.SceneManagement.SceneManager.LoadScene(TargetSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        public ADEvent OnSceneStart = new ADEvent(), OnSceneEnd = new ADEvent();

        public MonoBehaviour SceneMono { get => this; }
    }

    public class CoroutineWorkerMono:MonoBehaviour
    {
        
    }

    public static class SceneSingleAssets
    {
        public static ISceneSingleController instence = null;
        public static List<ICanPrepareToOtherScene> translist = new();
        public static List<AudioSourceController> audioSourceControllers = new();
        public static object infomation = null;
        private static MonoBehaviour _m_CoroutineWorker = null;
        public static MonoBehaviour CoroutineWorker
        {
            get
            {
                _m_CoroutineWorker ??= ((instence == null) ? new GameObject("CoroutineWorker(SingleAssets)").AddComponent<CoroutineWorkerMono>() : instence.SceneMono);
                return _m_CoroutineWorker;
            }
            set
            {
                CoroutineWorker = value;
            }
        }

        public static void Init()
        {
            instence = null;
            audioSourceControllers.Clear();
        }
    }



}

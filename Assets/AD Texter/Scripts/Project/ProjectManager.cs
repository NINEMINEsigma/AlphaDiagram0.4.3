using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AD.BASE;
using AD.Experimental.GameEditor;
using AD.Experimental.Performance;
using AD.Experimental.SceneTrans;
using AD.Sample.Texter.Project;
using AD.Utility;
using AD.Utility.Object;
using UnityEngine;

namespace AD.Sample.Texter
{
    [Serializable]
    public class PrefabModel : ADModel
    {
        [HideInInspector] public Transform Root;

        public ADSerializableDictionary<string, EditGroup> Prefabs = new();

        public ADSerializableDictionary<string, List<GameObject>> SubProjectItemPrefabs = new();

        public EditGroup ObtainInstance(string name)
        {
            return Prefabs[name].PrefabInstantiate();
        }

        public T ObtainInstance<T>(string name) where T : Component
        {
            return Prefabs[name].PrefabInstantiate<T, EditGroup>();
        }

        public override void Init()
        {
            //Not Init
        }

        public override IADModel Load(string path)
        {
            throw new System.NotImplementedException();
        }

        public override void Save(string path)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ProjectManager : ADController
    {

        public class ProjectLoadEntry : ADModel
        {
            public DataAssets Current;
            public ProjectLoadEntry PastInfo;

            public ProjectLoadEntry() : this(null, null) { }
            public ProjectLoadEntry(DataAssets current, ProjectLoadEntry pastInfo)
            {
                Current = current;
                PastInfo = pastInfo;
            }

            public ProjectLoadEntry GetNext(DataAssets next)
            {
                return new ProjectLoadEntry(next, this);
            }

            public static ProjectLoadEntry Temp(DataAssets next)
            {
                return new ProjectLoadEntry(next, null);
            }

            public override void Init()
            {
                //Not Init
            }

            public override IADModel Load(string path)
            {
                throw new NotImplementedException();
            }

            public override void Save(string path)
            {
                throw new NotImplementedException();
            }
        }

        public GameEditorApp UIApp => GameEditorApp.instance;

        public ProjectData CurrentProjectData;

        [Header("Assets")]
        public Transform ProjectTransformRoot;
        public ProjectRoot ProjectRootMono;
        public CameraCore MainCameraCore;
        public TimeClocker ADGTimeC;
        public MainSceneLoader MainSceneLoaderManager;
        [Header("Prefab")]
        public PrefabModel ProjectPrefabModel;
#if UNITY_EDITOR
        [Header("Debug")]
        public List<ProjectItemData> s_Datas = new();
#endif
         

        private void Start()
        {
            App.instance.RegisterController(this);
            App.instance.OnGenerate.AddListener(T => this.OnGenerate.Invoke(T));
            ADGlobalSystem.instance.IsAutoSaveArchitecturesDebugLog = true;
            ADGTimeC = ADGlobalSystem.instance.AutoSaveArchitecturesDebugLogTimeLimitCounter;
#if UNITY_EDITOR
            s_Datas = ProjectItemData.s_Datas;
#endif
        }

        private void OnApplicationQuit()
        {
            App.instance.SaveRecord();
        }

        private TaskInfo loadingTask, savingTask;

        public override void Init()
        {
            Architecture.RegisterController(MainSceneLoaderManager);
            StartCoroutine(LoadEveryOne());
        }

        private IEnumerator LoadEveryOne()
        {
            while (!GameEditorApp.instance.Contains<Information>())
            {
                yield return null;  
            }

            loadingTask = new TaskInfo("Project Loading", 0, 0, new Vector2(0, 2.3f), false);
            loadingTask.Register();

            DebugExtenion.LogMessage("Project Manager Init");

            Architecture.AddMessage("Start Loading Model");

            CurrentProjectData = new()
            {
                DataAssetsForm = Architecture.GetModel<DataAssets>()
            };
            if (Architecture.Contains<ProjectLoadEntry>())
            {
                //更新
                Architecture.RegisterModel<ProjectLoadEntry>(Architecture.GetModel<ProjectLoadEntry>().GetNext(CurrentProjectData.DataAssetsForm));
            }
            else
            {
                Architecture.RegisterModel(ProjectLoadEntry.Temp(CurrentProjectData.DataAssetsForm));
            }
            ProjectPrefabModel.Root = ProjectTransformRoot;
            Architecture.RegisterModel(ProjectPrefabModel);

            loadingTask.TaskValue = 0.1f;
            yield return new WaitForSeconds(0.5f);
            loadingTask.TaskValue = 0.3f;


            ProjectRootMono.Init();
            yield return CurrentProjectData.Load(loadingTask);

        }

        public void SaveProjectData()
        {
            if (loadingTask != null)
            {
                loadingTask.UnRegister();
                loadingTask = null;
            }
            if (savingTask != null)
            {
                savingTask.UnRegister();
                savingTask = null;
            }
            savingTask = new TaskInfo("Project Saving", 0, 0, new Vector2(0, 1f), false);
            savingTask.Register();
            SceneTrans.instance.StartCoroutine(CurrentProjectData.Save(savingTask));
        }

        public void BackToEntry()
        {
            ADGlobalSystem.instance.OnEnd();
        }

        public void LoadSubProject(DataAssets next)
        {
            Architecture.RegisterModel(next);
            ADGlobalSystem.instance.TargetSceneName = SceneExtension.GetCurrent().name;
            ADGlobalSystem.instance.OnEnd();
        }

        public void BackToPreviousScene()
        {
            ADGlobalSystem.instance.TargetSceneName = SceneExtension.GetCurrent().name;
            Architecture.RegisterModel(Architecture.GetModel<ProjectLoadEntry>());
            ADGlobalSystem.instance.OnEnd();
        }

        public ADEvent<ProjectItemData> OnGenerate = new();

        public GameObject LastFocusTarget;
        public void CatchItemByCameraCore(GameObject target, RayExtension.RayInfo info)
        {
            if (LastFocusTarget == target)
            {
                MainCameraCore.TryStartCoroutineMove();
                LastFocusTarget = null;
                if (target.transform.parent != null && target.transform.parent.TryGetComponent<ColliderLayer>(out var colliderLayer))
                {
                    if (colliderLayer.ParentGroup.gameObject.ObtainComponent(out IProjectItem item))
                    {
                        UIApp.GetController<Properties>().MatchTarget = item;
                        UIApp.GetController<Properties>().ClearAndRefresh();
                    }
                }
            }
            else LastFocusTarget = target;
        }

        //XXX

        public void LoadFromOfflineFile(string path)
        {
            ADFile file = new(path, false, true, false, true);
            if (file)
            {
                StartCoroutine(EndWaitLoadFromOfflineFile(file));
            }
            else Debug.LogException(file.ErrorException);
        }

        private IEnumerator EndWaitLoadFromOfflineFile(ADFile file)
        {
            CurrentProjectData.BuildFromOffline(file.FileData);
            if (loadingTask != null)
            {
                loadingTask.UnRegister();
                loadingTask = null;
            }
            if (savingTask != null)
            {
                savingTask.UnRegister();
                savingTask = null;
            }
            savingTask = new TaskInfo("Project Saving", 0, 0, new Vector2(0, 1f), false);
            savingTask.Register();
            yield return CurrentProjectData.Save(savingTask);
            App.instance.GetController<MainSceneLoader>().UnloadAll();
            ADGlobalSystem.instance.TargetSceneName = SceneExtension.GetCurrent().name;
            ADGlobalSystem.instance.OnEnd();
        }

        public void CreateOfflineFile(string path)
        {
            CurrentProjectData.BuildOffline(path);
        }

        private IEnumerator EndWaitCreateOfflineFile()
        {
            if (loadingTask != null)
            {
                loadingTask.UnRegister();
                loadingTask = null;
            }
            if (savingTask != null)
            {
                savingTask.UnRegister();
                savingTask = null;
            }
            savingTask = new TaskInfo("Project Saving", 0, 0, new Vector2(0, 1f), false);
            savingTask.Register();
            yield return CurrentProjectData.Save(savingTask);
            CreateOfflineFile(Path.Combine(LoadingManager.FilePath, "Temp", CurrentProjectData.DataAssetsForm.AssetsName + "." + ProjectData.OfflineExtension));
            App.instance.GetController<MainSceneLoader>().UnloadAll();
            ADGlobalSystem.instance.TargetSceneName = SceneExtension.GetCurrent().name;
            ADGlobalSystem.instance.OnEnd();
        }

        public void CreateOfflineFile()
        {
            StartCoroutine(EndWaitCreateOfflineFile());
        }
    }
}

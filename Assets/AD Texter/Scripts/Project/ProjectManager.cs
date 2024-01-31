using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.Experimental.GameEditor;
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
        [Header("Prefab")]
        public PrefabModel ProjectPrefabModel;

        private void Start()
        {
            App.instance.RegisterController(this);
        }

        private void OnDestroy()
        {
            Architecture.UnRegister<ProjectManager>();
        }

        public override void Init()
        {
            CurrentProjectData = new()
            {
                DataAssetsForm = Architecture.GetModel<DataAssets>()
            };
            CurrentProjectData.Load();
            if(Architecture.Contains<ProjectLoadEntry>())
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
        }

        public void SaveProjectData()
        {
            CurrentProjectData.Save();
        }

        public void BackToEntry()
        {
            SaveProjectData();
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
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AD.BASE;
using AD.Experimental.GameEditor;
using AD.Sample.Texter.Data;
using AD.Utility;
using UnityEngine;

namespace AD.Sample.Texter
{
    [Serializable]
    public class ProjectData : AD.Experimental.Localization.Cache.CacheAssets<AD.Experimental.Localization.Cache.CacheAssetsKey, ProjectItemDataCache, ProjectItemData, ProjectData_BaseMap>
    {
        public static string ProjectItemDataExtension => "projitem";
        public static string ProjectItemDataPointExtension = "." + ProjectItemDataExtension;

        public DataAssets DataAssetsForm;

        private static ProjectData_BaseMap Load(FileInfo file)
        {
            return ES3.Load<ProjectData_BaseMap>(ProjectItemDataExtension, file.FullName);
        }

        private static void Save(AD.Experimental.Localization.Cache.ICanCacheData<ProjectItemData, ProjectData_BaseMap> cacheDataForm, string path)
        {
            ES3.Save<ProjectData_BaseMap>(ProjectItemDataExtension, cacheDataForm.MatchElementBM.Get(), path);
        }

        public IEnumerator Load(TaskInfo loadingTask)
        {
            DebugExtenion.Log();
            App.instance.AddMessage("Start Loading Data To Items 1/2");

            TimeClocker timer = TimeExtension.GetTimer();

            string fileListPath = Path.Combine(LoadingManager.FilePath, DataAssetsForm.AssetsName);
            FileC.TryCreateDirectroryOfFile(Path.Combine(fileListPath, "Empty.empty"));
            var fileList = FileC.FindAll(fileListPath, ProjectItemDataExtension);
            yield return new WaitForEndOfFrame();
            App.instance.AddMessage("Start Loading Data To Items 2/2");
            if (fileList != null)
            {
                List<ProjectItemData> projectItemDatas = new();
                for (int i = 0, e = fileList.Count; i < e; i++)
                {
                    FileInfo file = fileList[i];
                    if (timer.LastDalteSceond > 0.1f)
                    {
                        loadingTask.TaskValue = (float)i / (float)e + 0.3f;
                        yield return new WaitForEndOfFrame();
                        timer.Update();
                    }
                    ProjectData_BaseMap bmap = Load(file);
                    string key = bmap.ProjectItemID;
                    bmap.ToObject(out ProjectItemData projcetdata);
                    this.Add(
                        new AD.Experimental.Localization.Cache.CacheAssetsKey(key)
                        , new ProjectItemDataCache(key, projcetdata, bmap));
                    projectItemDatas.Add(projcetdata);
                }
                for (int i = 0, e = projectItemDatas.Count; i < e; i++)
                {
                    ProjectItemData projcetdata = projectItemDatas[i];
                    if (timer.LastDalteSceond > 0.1f)
                    {
                        loadingTask.TaskValue = (float)i / (float)e + 1.2f;
                        yield return new WaitForEndOfFrame();
                        timer.Update();
                    }
                    App.instance.OnGenerate.Invoke(projcetdata);
                }
            }

            loadingTask.TaskPercent = 1.00f;
        }

        public IEnumerator Save(TaskInfo savingTask)
        {
            DebugExtenion.Log();
            App.instance.AddMessage("Start Saving Data");

            TimeClocker timer = TimeExtension.GetTimer();

            string fileListPath = Path.Combine(LoadingManager.FilePath, DataAssetsForm.AssetsName);
            FileC.ReCreateDirectroryOfFile(Path.Combine(fileListPath, "Empty.empty"));

            this.datas.RemoveNullValues();

            int i = 0, e = this.Count;
            foreach (var item in this)
            {
                if (timer.LastDalteSceond > 0.1f)
                {
                    savingTask.TaskValue = (float)i / (float)e + 0.3f;
                    yield return new WaitForEndOfFrame();
                    timer.Update();
                }
                Save(item, Path.Combine(LoadingManager.FilePath, DataAssetsForm.AssetsName, item.MatchElementBM.Get().ProjectItemID + ProjectItemDataPointExtension));
            }

            savingTask.TaskPercent = 1.00f;
        }
    }

    [Serializable]
    public class ProjectItemDataCache : AD.Experimental.Localization.Cache.AbstractCache<ProjectItemData, ProjectData_BaseMap>
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
#if UNITY_EDITOR

        [SerializeField] private ProjectItemData m_ProjectItemData;
        [SerializeField] private ProjectData_BaseMap m_ProjectData_BaseMap;

        public void OnAfterDeserialize()
        {
            this.MatchElement.Set(m_ProjectItemData);
            this.MatchElementBM.Set(m_ProjectData_BaseMap);
        }

        public void OnBeforeSerialize()
        {
            m_ProjectItemData = this.MatchElement.Get();
            m_ProjectData_BaseMap = this.MatchElementBM.Get();
        }

#endif

        public ProjectItemDataCache(string key, ProjectItemData baseData, ProjectData_BaseMap mapData) : base(key)
        {
            this.MatchElement = new();
            if (baseData != null) this.MatchElement.Set(baseData);
            this.MatchElementBM = new();
            if (mapData != null) this.MatchElementBM.Set(mapData);
        }
        public ProjectItemDataCache(string key) : this(key, null, null) { }
        public ProjectItemDataCache() : this("Empty New Key") { }

        public override bool FromObject(ProjectItemData from)
        {
            this.MatchElement.Set(from);
            UpdataMatchBMByBase();
            return true;
        }

        public override bool FromObject(IBase from)
        {
            bool result = false;
            if (from is ProjectItemData projectItemData)
            {
                result = FromObject(projectItemData);
                UpdataMatchBMByBase();
            }
            return result;
        }

        public override void ToObject(out ProjectItemData obj)
        {
            obj = this.MatchElement.Get();
        }
    }

    [Serializable]
    public class ProjectItemData : IBase<ProjectData_BaseMap>
    {
        public const string ProjectRootID = "ProjectRoot";

        public static Dictionary<string, IProjectItemWhereNeedInitData> IDSet = new();

        public static IProjectItem GetParent(string key)
        {
            if (key == null)
            {
                App.instance.AddMessage("null key when GetParent");
                return App.instance.GetController<ProjectManager>().ProjectRootMono;
            }
            if (key.Equals(ProjectRootID)) return App.instance.GetController<ProjectManager>().ProjectRootMono;
            return IDSet.TryGetValue(key, out var parent) ? parent : null;
        }

        public ProjectItemData() : this(null) { }
        public ProjectItemData(IProjectItemWhereNeedInitData matchProjectItem) : this(matchProjectItem, "New Object", ProjectRootID, Vector2.zero) { }
        protected ProjectItemData(IProjectItemWhereNeedInitData matchProjectItem, string projectItemID, string parentItemID, Vector2 projectItemPosition)
        {
            _MatchProjectItem = matchProjectItem;
            ProjectItemID = projectItemID;
            ParentItemID = parentItemID ?? ProjectItemData.ProjectRootID;
            ProjectItemPosition = projectItemPosition;
        }
        ~ProjectItemData()
        {
            IDSet.Remove(ProjectItemID);
        }

        private IProjectItemWhereNeedInitData _MatchProjectItem;
        public IProjectItemWhereNeedInitData MatchProjectItem
        {
            get => _MatchProjectItem;
            set
            {
                _MatchProjectItem = value;
                Internal_Set_ProjectItemID(ProjectItemID);
            }
        }
        private string projectItemID;
        public string ProjectItemID
        {
            get => projectItemID;
            set
            {
                Internal_Set_ProjectItemID(value);
            }
        }

        private void Internal_Set_ProjectItemID(string value)
        {
            if (value == null)
            {
                ADGlobalSystem.Error("ProjectItemID : value arg is null");
                return;
            }
            if (projectItemID == value)
            {
                if(IDSet.ContainsKey(projectItemID))
                {
                    IDSet[projectItemID] = MatchProjectItem;
                }
                else
                {
                    IDSet.Add(projectItemID, MatchProjectItem);
                }
            }
            else
            {
                int counter = 1;
                string newID = value;
                while (IDSet.ContainsKey(newID))
                {
                    newID = value + $"({counter++})";
                }
                if (projectItemID != null)
                {
                    IDSet.Remove(projectItemID);
                    App.instance.GetController<ProjectManager>().CurrentProjectData.Remove(new(projectItemID));
                }
                IDSet.Add(newID, this.MatchProjectItem);
                projectItemID = newID;
            }
        }

        public string ParentItemID;
        public Vector2 ProjectItemPosition;

        #region IBase

        public bool FromMap(IBaseMap from)
        {
            if (from is ProjectData_BaseMap projectData_BaseMap)
            {
                return FromMap(projectData_BaseMap);
            }
            return false;
        }

        public void ToMap(out IBaseMap BM)
        {
            ToMap(out ProjectData_BaseMap projectData_BaseMap);
            BM = projectData_BaseMap;
        }

        #endregion

        /// <summary>
        /// Base is useful
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public virtual bool FromMap(ProjectData_BaseMap from)
        {
            this.ProjectItemID = from.ProjectItemID;
            this.ParentItemID = from.ParentItemID ?? ProjectItemData.ProjectRootID;
            this.ProjectItemPosition = from.ProjectItemPosition;
            return true;
        }

        /// <summary>
        /// Base is NotImplementedException
        /// </summary>
        /// <param name="BM"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void ToMap(out ProjectData_BaseMap BM)
        {
            throw new NotImplementedException();
        }
    }

    namespace Data
    {
        [EaseSave3]
        [Serializable]
        public class ProjectData_BaseMap : IBaseMap<ProjectItemData>
        {
            public string ProjectItemID;
            public string ParentItemID;
            public Vector2 ProjectItemPosition;

            #region NotSupport

            public bool Deserialize(string source)
            {
                throw new ADException("Not Support");
            }

            public string Serialize()
            {
                throw new ADException("Not Support");
            }

            #endregion

            #region IBaseMap

            public bool FromObject(IBase from)
            {
                if (from is ProjectItemData projectItemData)
                {
                    return FromObject(projectItemData);
                }
                return false;
            }

            public void ToObject(out IBase obj)
            {
                ToObject(out ProjectItemData projectItemData);
                obj = projectItemData;
            }

            #endregion

            #region IBaseMap<ProjectItemData>

            /// <summary>
            /// Base is NotImplementedException
            /// </summary>
            /// <param name="obj"></param>
            /// <exception cref="NotImplementedException"></exception>
            public virtual void ToObject(out ProjectItemData obj)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Base is useful
            /// </summary>
            /// <param name="from"></param>
            /// <returns></returns>
            public virtual bool FromObject(ProjectItemData from)
            {
                this.ProjectItemID = from.ProjectItemID;
                this.ParentItemID = from.ParentItemID ?? ProjectItemData.ProjectRootID;
                this.ProjectItemPosition = from.ProjectItemPosition;
                return true;
            }

            #endregion

        }
    }
}

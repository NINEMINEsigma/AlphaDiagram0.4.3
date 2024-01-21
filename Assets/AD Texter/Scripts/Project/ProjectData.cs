using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AD.BASE;
using AD.Simple.Texter.Data;
using TMPro;
using UnityEngine;

namespace AD.Simple.Texter
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

        private static void Save(AD.Experimental.Localization.Cache.ICanCacheData<ProjectItemData, ProjectData_BaseMap> cacheDataForm)
        {
            ES3.Save<ProjectData_BaseMap>(ProjectItemDataPointExtension, cacheDataForm.MatchElementBM.Get());
        }

        public void Load()
        {
            string fileListPath = Path.Combine(LoadingManager.FilePath, DataAssetsForm.AssetsName);
            FileC.TryCreateDirectroryOfFile(Path.Combine(fileListPath, "Empty.empty"));
            var fileList = FileC.FindAll(fileListPath, ProjectItemDataPointExtension);
            if (fileListPath != null)
            {
                foreach (var file in fileList)
                {
                    ProjectData_BaseMap bmap = Load(file);
                    string key = Path.GetFileNameWithoutExtension(file.Name);
                    bmap.ToObject(out ProjectItemData projcetdata);
                    this.Add(
                        new AD.Experimental.Localization.Cache.CacheAssetsKey(key)
                        , new ProjectItemDataCache(key, projcetdata, bmap));
                }
            }
        }

        public void Save()
        {
            string fileListPath = Path.Combine(LoadingManager.FilePath, DataAssetsForm.AssetsName);
            FileC.TryCreateDirectroryOfFile(Path.Combine(fileListPath, "Empty.empty"));
            foreach (AD.Experimental.Localization.Cache.ICanCacheData<ProjectItemData, ProjectData_BaseMap> item in this)
            {
                Save(item);
            }
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
        public long ProjectItemID;

        #region IBase

        public bool FromMap(IBaseMap from)
        {
            if(from is ProjectData_BaseMap projectData_BaseMap)
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

        public virtual bool FromMap(ProjectData_BaseMap from)
        {
            this.ProjectItemID = from.ProjectItemID;
            return true;
        }

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
            public long ProjectItemID;

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
                if(from is ProjectItemData projectItemData)
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

            public virtual void ToObject(out ProjectItemData obj)
            {
                throw new NotImplementedException();
            }

            public virtual bool FromObject(ProjectItemData from)
            {
                this.ProjectItemID = from.ProjectItemID;
                return true;
            }

            #endregion

        }
    }
}

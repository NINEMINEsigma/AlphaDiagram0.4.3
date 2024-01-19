using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AD.BASE;
using AD.Simple.Texter.Data;
using UnityEngine;

namespace AD.Simple.Texter
{
    [EaseSave3]
    [Serializable]
    public class ProjectData : AD.Experimental.Localization.Cache.CacheAssets<AD.Experimental.Localization.Cache.CacheAssetsKey, ProjectItemDataCache, ProjectItemData, ProjectData_BaseMap>
    {
        public static string ProjectItemDataPointExtension = ".projitem";

        public DataAssets DataAssetsForm;

        public void Load()
        {
            string fileListPath = Path.Combine(LoadingManager.FilePath, DataAssetsForm.AssetsName);
            FileC.TryCreateDirectroryOfFile(Path.Combine(fileListPath, "Empty.empty"));
            var fileList = FileC.FindAll(fileListPath, ProjectItemDataPointExtension);
            if (fileListPath != null)
            {

            }
        }

        public void Save()
        {

        }
    }

    [Serializable]
    public class ProjectItemDataCache : AD.Experimental.Localization.Cache.AbstractCache<ProjectItemData, ProjectData_BaseMap>, ISerializationCallbackReceiver
    {
        [SerializeField] private ProjectItemData m_ProjectItemData;
        [SerializeField] private ProjectData_BaseMap m_ProjectData_BaseMap;

        public ProjectItemDataCache()
        {
            this.MatchElement = new();
            this.MatchElementBM = new();
        }

        public override bool FromObject(ProjectItemData from)
        {
            throw new NotImplementedException();
        }

        public override bool FromObject(IBase from)
        {
            throw new NotImplementedException();
        }

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

        public override void ToObject(out ProjectItemData obj)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class ProjectItemData : IBase<ProjectData_BaseMap>
    {
        public long ProjectItemID;

        public bool FromMap(ProjectData_BaseMap from)
        {
            throw new NotImplementedException();
        }

        public bool FromMap(IBaseMap from)
        {
            throw new NotImplementedException();
        }

        public void ToMap(out ProjectData_BaseMap BM)
        {
            throw new NotImplementedException();
        }

        public void ToMap(out IBaseMap BM)
        {
            throw new NotImplementedException();
        }
    }

    namespace Data
    {
        [Serializable]
        public class ProjectData_BaseMap : IBaseMap<ProjectItemData>
        {
            public long ProjectItemID;

            public virtual bool Deserialize(string source)
            {
                throw new NotImplementedException();
            }

            public virtual bool FromObject(ProjectItemData from)
            {
                throw new NotImplementedException();
            }

            public virtual bool FromObject(IBase from)
            {
                throw new NotImplementedException();
            }

            public virtual string Serialize()
            {
                throw new NotImplementedException();
            }

            public virtual void ToObject(out ProjectItemData obj)
            {
                throw new NotImplementedException();
            }

            public virtual void ToObject(out IBase obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}

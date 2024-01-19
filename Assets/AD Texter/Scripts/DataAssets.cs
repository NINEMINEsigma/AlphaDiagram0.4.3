using System;
using System.IO;
using AD.BASE;
using AD.Simple.Texter.Data;
using UnityEngine;

namespace AD.Simple.Texter
{
    [Serializable]
    public class DataAssets : ADModel
    {
        public FileInfo LoadFile;
        public bool IsAble { get; private set; }

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(AssetsName)) AssetsName = LoadFile == null ? "[ Test ]" : Path.GetFileNameWithoutExtension(LoadFile.Name); ;
                return AssetsName;
            }
        }

        public static implicit operator bool(DataAssets dataAssets) => dataAssets.IsAble;

        public DataAssets() { IsAble = true; }
        public DataAssets(FileInfo file) { LoadFile = file; IsAble = false; }

        public DataAssets(string createrName, string description, string dateTime, string assetsName)
        {
            CreaterName = createrName;
            Description = description;
            DateTime = dateTime;
            AssetsName = assetsName;

            IsAble = true;
        }

        public void CreateInstance(GameObject prefab, Canvas canvas)
        {
            EntryItem entryItem = GameObject.Instantiate(prefab, canvas.transform).GetComponent<EntryItem>();
            entryItem.MyData = this;
            entryItem.MyIndex = EntryItem.MaxIndex++;
        }

        public override void Init()
        {

        }

        public DataAssets Load()
        {
            if (LoadFile == null && IsAble == false) ADGlobalSystem.ThrowLogicError("Cannt Load");
            return IsAble ? this : SetupSelf(LoadingManager.LoadDataAssets(LoadFile.FullName));
        }
        public override IADModel Load(string path)
        {
            if (!File.Exists(path)) return null;
            return SetupSelf(LoadingManager.LoadDataAssets(path));
        }

        public DataAssets Save()
        {
            Save(AssetsName + LoadingManager.ProntExtension);
            return this;
        }
        public override void Save(string path)
        {
            LoadingManager.SaveDataAssets(path, this);
            IsAble = true;
        }

        private DataAssets SetupSelf(DataFile data)
        {
            this.CreaterName = data.CreaterName;
            this.Description = data.Description;
            this.DateTime = data.DateTime;
            this.AssetsName = data.AssetsName;
            this.IsAble = true;
            return this;
        }

        public string CreaterName;
        public string Description;
        public string DateTime;
        public string AssetsName;
    }

    namespace Data
    {
        [Serializable]
        [EaseSave3]
        public class DataFile
        {
            public string CreaterName;
            public string Description;
            public string DateTime;
            public string AssetsName;

            public DataFile(DataAssets data) : this(data.CreaterName, data.Description, data.DateTime, data.AssetsName)
            {
            }

            public DataFile(string createrName, string description, string dateTime, string assetsName)
            {
                CreaterName = createrName;
                Description = description;
                DateTime = dateTime;
                AssetsName = assetsName;
            }
        }
    }

}

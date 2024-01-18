using System;
using System.IO;
using AD.BASE;
using UnityEngine;

namespace AD.Simple.Texter
{
    [Serializable]
    //[EaseSave3]
    public class DataAssets : ADModel
    {
        [NonSerialized]public FileInfo LoadFile;
        [NonSerialized]public readonly bool IsAble;

        public string Name => LoadFile == null ? "[ Test ]" : Path.GetFileNameWithoutExtension(LoadFile.Name);

        public static implicit operator bool(DataAssets dataAssets) => dataAssets.IsAble;

        public DataAssets() { IsAble = true; }
        public DataAssets(FileInfo file) { LoadFile = file; IsAble = false; }

        public DataAssets(string createrName, string description, string dateTime)
        {
            CreaterName = createrName;
            Description = description;
            DateTime = dateTime;
            IsAble = true;
        }

        public void CreateInstance(GameObject prefab, Canvas canvas)
        {
            EntryItem entryItem = GameObject.Instantiate(prefab, canvas.transform).GetComponent<EntryItem>();
            entryItem.MyData = this;
        }

        public override void Init()
        {

        }

        public DataAssets Load()
        {
            if (LoadFile == null) ADGlobalSystem.ThrowLogicError("Cannt Load");
            return LoadingManager.LoadDataAssets(LoadFile);
        }
        public override IADModel Load(string path)
        {
            if (!File.Exists(path)) return null;
            return LoadingManager.LoadDataAssets(new StreamReader(File.OpenRead(path)).ReadToEnd());
        }

        public override void Save(string path)
        {
            LoadingManager.SaveDataAssets(path, this);
        }

        public string CreaterName;
        public string Description;
        public string DateTime;
    }
}

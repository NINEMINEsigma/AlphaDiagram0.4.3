using System.Collections.Generic;
using System.IO;
using AD.BASE;
using AD.Utility;
using Unity.VisualScripting;
using UnityEngine;

namespace AD.Simple.Texter
{
    public class LoadingManager : ADController
    {
        public static string ES3Key => "AD_Assests";
        public static string Extension => "texter";
        public static string ProntExtension => "." + Extension;
        public static string FilePath => Path.Combine(Application.streamingAssetsPath, "Texter");

        //Scene Name
        public static string ChoosingPage = "ChoosingPage";

        public static string CurrentCreateNameKey = "CurrentCreaterName";

        #region Load

        public static List<FileInfo> AllFile => FileC.FindAll(FilePath, ProntExtension);

        private static DataAssets CurrentTransform;
        public static List<DataAssets> GetDatas(List<FileInfo> files)
        {
            return files.GetSubList(T => (CurrentTransform = LoadDataAssets(T)) != null, T => CurrentTransform);
        }
        public static DataAssets LoadDataAssets(FileInfo source)
        {
            return new DataAssets(source);
        }
        public static DataAssets LoadDataAssets(string source)
        {
            return ADGlobalSystem.Deserialize<DataAssets>(source, out object result) ? result as DataAssets : null;
        }

        #endregion

        #region Save

        public static void SaveDataAssets(string fileName, DataAssets data)
        {
            ADGlobalSystem.Output<DataAssets>(Path.Combine(FilePath, fileName), data);
        }

        #endregion

        public GameObject Prefab;
        public Canvas ParentCanvas;

        private void Start()
        {
            App.instance.RegisterController(this);
        }

        public override void Init()
        {
            FileC.TryCreateDirectroryOfFile(Path.Combine(FilePath, "File"));
            var af = AllFile;
            if (af == null) return;
            foreach (var single in GetDatas(af))
            {
                single.CreateInstance(Prefab, ParentCanvas);
            }
        }

        private void OnDestroy()
        {
            Architecture.UnRegister<LoadingManager>();
        }

    }
}

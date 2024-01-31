using System.Collections.Generic;
using System.IO;
using AD.BASE;
using AD.Utility;
using UnityEngine;

namespace AD.Sample.Texter
{
    public class LoadingManager : ADController
    {
        public static string ES3Key => "AD_Assests";
        public static string Extension => "texter";
        public static string PointExtension => "." + Extension;
        public static string FilePath => Path.Combine(Application.streamingAssetsPath, "Texter");

        //Scene Name
        public static string ChoosingPage = "ChoosingPage";

        public static string CurrentCreateNameKey = "CurrentCreaterName";

        #region Load

        public static List<FileInfo> AllFile => FileC.FindAll(FilePath, PointExtension);

        private static DataAssets CurrentTransform;
        public static List<DataAssets> GetDatas(List<FileInfo> files)
        {
            return files.GetSubList(T => (CurrentTransform = LoadDataAssets(T)) != null, T => CurrentTransform);
        }
        public static DataAssets LoadDataAssets(FileInfo source)
        {
            return new DataAssets(source);
        }
        public static Data.DataFile LoadDataAssets(string path)
        {
            var file = new ADFile(false, path, false, false, true);
            try
            {
                if (file.Deserialize<Data.DataFile>(true, System.Text.Encoding.UTF8, out object result))
                    return ADGlobalSystem.FinalCheck(result as Data.DataFile, path + " is load failed");
                else throw new ADException(path + " is load failed");
            }
            catch
            {
                file.Delete();
                throw;
            }
        }

        #endregion

        #region Save

        public static void SaveDataAssets(string fileName, DataAssets data)
        {
            new ADFile(Path.Combine(FilePath, fileName), true, false, false, true).Serialize<Data.DataFile>(new(data), System.Text.Encoding.UTF8, false);
            //ADGlobalSystem.Output<Data.DataFile>(Path.Combine(FilePath, fileName), new(data));
        }

        #endregion

        public GameObject Prefab;
        public Transform ParentTransform;

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
                single.CreateInstance(Prefab, ParentTransform);
            }
        }

        private void OnDestroy()
        {
            Architecture.UnRegister<LoadingManager>();
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Experimental.AssetScript
{
    public static class AS
    {
        public static readonly string DefaultKeyName = "AS/Default";

        #region Save

        public static ASResult Save(object source, string path)
        {
            return Save(source, path, AS.DefaultKeyName, new ASSeting());
        }

        public static ASResult Save(object source, string path, string key)
        {
            return Save(source, path, key, new ASSeting());
        }

        public static ASResult Save(object source, string path, ASSeting setting)
        {
            return Save(source,path, AS.DefaultKeyName, setting);
        }

        public static ASResult Save(object source, string path, string key, ASSeting setting)
        {
            return setting.AssetSaveMethod.Invoke(source, path, key);
        }

        #endregion

        #region Load

        public static ASResult Load(string sourcePath)
        {
            return new ASSeting().AssetLoadMethod.Invoke(sourcePath, AS.DefaultKeyName);
        }

        public static ASResult Load(string sourcePath, string key)
        {
            return new ASSeting().AssetLoadMethod.Invoke(sourcePath, key);
        }

        public static ASResult Load(string sourcePath, ASSeting setting)
        {
            return setting.AssetLoadMethod.Invoke(sourcePath, AS.DefaultKeyName);
        }

        public static ASResult Load(string sourcePath,string key, ASSeting setting)
        {
            return setting.AssetLoadMethod.Invoke(sourcePath, key);
        }

        #endregion
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Experimental.AssetScript
{
    public class ASSeting
    {
        public delegate ASResult AssetSaveMethodDelegate(object source, string path, string key);
        public delegate ASResult AssetLoadMethodDelegate(string path, string key);

        AssetSaveMethodDelegate assetSaveMethod = null;
        AssetLoadMethodDelegate assetLoadMethod = null;

        public ASSeting()
        {
        }

        public ASSeting(AssetSaveMethodDelegate assetMethod)
        {
            AssetSaveMethod = assetMethod;
        }

        public ASSeting(AssetLoadMethodDelegate assetLoadMethod)
        {
            AssetLoadMethod = assetLoadMethod;
        }

        public ASSeting(AssetSaveMethodDelegate assetSaveMethod, AssetLoadMethodDelegate assetLoadMethod) : this(assetSaveMethod)
        {
            AssetLoadMethod = assetLoadMethod;
        }

        public AssetSaveMethodDelegate AssetSaveMethod
        {
            get
            {
                return assetSaveMethod ?? DefaultAssetSaveMethodDelegate;
            }
            set
            {
                assetSaveMethod = value;
            }
        }
        public AssetLoadMethodDelegate AssetLoadMethod
        {
            get
            {
                return assetLoadMethod ?? DefaultAssetLoadMethodDelegate;
            }
            set
            {
                assetLoadMethod = value;
            }
        }

        private ASResult DefaultAssetSaveMethodDelegate(object source, string path, string key)
        {
            ASResult result = new();
            return result;
        }
        private ASResult DefaultAssetLoadMethodDelegate(string path, string key)
        {
            ASResult result = new();
            return result;
        }
    }
}

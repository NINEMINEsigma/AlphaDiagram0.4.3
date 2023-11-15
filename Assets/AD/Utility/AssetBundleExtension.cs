using UnityEngine;

namespace AD.Utility
{
    public static class AssetBundleExtension
    {
		public static AssetBundle LoadAssetBundle(this string self)
        {
            return AD.BASE.FileC.LoadAssetBundle(self);
        }

        public static AssetBundle LoadAssetBundle(this string self,params string[] targetName)
        {
            return AD.BASE.FileC.LoadAssetBundle(self, targetName);
        }



    }  
}


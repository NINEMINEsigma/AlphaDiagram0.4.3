using System.Collections.Generic;
using System.Linq;
using AD.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace AD.Experimental.GameEditor
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New GUISkin", menuName = "AD/GUISkin", order = 10)]
    public class GUISkin : AD.Experimental.EditorAsset.Cache.CacheAssets<AD.Experimental.EditorAsset.Cache.CacheAssetsKey, GUIStyle>
    {
        public GUIStyle FindStyle(string name)
        {
            var result = this.GetData().FirstOrDefault(T => T.key.Equals(name));
            return result == null ? null : result.data;
        }

        [SerializeField] bool isAutoUpdate = true;

        private void OnValidate()
        {
            foreach (var item in GetData())
            {
                item.data.OnValidate();
                var strs = item.data.CacheKey.Split(" ");
                if (strs.Length == 3 && strs[1] == "-")
                    item.key = new(strs[2] + "(" + strs[0] + ")");
                else if (strs.Length > 3)
                    item.key = new(item.data.CacheKey);
                else item.key = new(strs[^1]);
            }
            if (isAutoUpdate) GetData().Sort();
        }
    }
}

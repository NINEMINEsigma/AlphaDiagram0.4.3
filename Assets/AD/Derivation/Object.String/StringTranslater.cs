using System;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Utility.Object
{
    [Serializable]
    [CreateAssetMenu(menuName = "AD/StringTranslater")]
    public class StringTranslater:ScriptableObject
    { 
        [Serializable]
        public class Package
        {
            [Serializable]
            public class StringDictionaryPair
            {
                public string key;
                public string value;
            }

            public string Name;
            public string Description;
            public int index;
            public ulong ID;

            public Package()
            {
                Name = "Empty Package";
                Description = "";
                ID = (ulong)this.GetHashCode();
            }

            public Package(string name, string description, int index, ulong iD)
            {
                Name = name;
                Description = description;
                this.index = index;
                ID = iD;
            }

            public List<StringDictionaryPair> values = new();

        }

        public List<Package> Packages = new();

        public void Set()
        {
            foreach (var PackageItem in Packages)
            {
                AD.Utility.StringExtension.Package package = new AD.Utility.StringExtension.Package();
                foreach (var pair in PackageItem.values)
                {
                    package.values.TryAdd(pair.key, pair.value);
                }
                AD.Utility.StringExtension.Packages.Add(package);
            }
        }
    }

}


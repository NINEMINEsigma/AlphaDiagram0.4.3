using System;
using System.Collections.Generic;
using System.Linq;

namespace AD.Utility
{
    public static class StringExtension
    {
        public static bool Input<T>(this string self, out object result)
        {
            return ADGlobalSystem.Input<T>(self, out result);
        }

        public static bool Input(this string self, out string result)
        {
            return ADGlobalSystem.Input(self, out result);
        }

        public static void Output<T>(this string self, T source)
        {
            ADGlobalSystem.Output(self, source);
        }

        public static void Output(this string self, string str)
        {
            ADGlobalSystem.Output(self, str);
        }

        public static bool Install(string path, bool isSetCurrent = true)
        {
            if (ADGlobalSystem.Input<Package>(path, out object obj))
            {
                if (isSetCurrent) CurrentIndex = Packages.Count;
                Packages.Add(obj as Package);
                return true;
            }
            else return false;
        }

        public static void Uninstall(string Name, ulong ID)
        {
            var cat = Packages.FirstOrDefault(T => T.Name == Name && T.ID == ID);
            Packages.Remove(cat);
        }

        public static string Get(this string self)
        {
            return CurrentPackage[self];
        }

        public static string Translate(this string self)
        {
            return CurrentPackage[self];
        }

        [Serializable]
        public class Package
        {
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

            public virtual Dictionary<string,string> values { get; set; } = new();

            public virtual string this[string key] => values.TryGetValue(key, out var value) ? value : key;

        }

        [Serializable]
        public class DefualtPackage : Package
        {
            public override string this[string key] => key;
        }

        internal static List<Package> Packages = new List<Package>();
        private static readonly DefualtPackage defualt = new DefualtPackage();
        public static int CurrentIndex = -1;
        public static int Count => Packages.Count;
        public static Package CurrentPackage
        {
            get
            {
                if (CurrentIndex < 0 || CurrentIndex >= Packages.Count)
                    return defualt;
                else return Packages[CurrentIndex];
            }
        } 
    }
}


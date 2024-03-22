using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AD.BASE;
using Newtonsoft.Json;
using UnityEngine;

namespace AD.Utility
{
    public static class ContainerExtension
    {
        public static List<P> GetSubList<T, P>(this List<T> self) where P : class
        {
            List<P> result = new();
            result.AddRange(from T item in self
                            where item.Convertible<P>()
                            select item as P);
            return result;
        }

        public static P SelectCast<T, P>(this List<T> self) where P : class
        {
            foreach (var item in self)
                if (item.As<P>(out var result)) return result;
            return null;
        }

        public static List<T> GetSubList<T>(this IEnumerable<T> self, Predicate<T> predicate)
        {
            List<T> result = new();
            result.AddRange(from T item in self
                            where predicate(item)
                            select item);
            return result;
        }

        public static List<Value> GetSubListAboutValue<Key, Value>(this Dictionary<Key, Value> self)
        {
            List<Value> result = new();
            foreach (var item in self)
            {
                result.Add(item.Value);
            }
            return result;
        }

        public static List<Key> GetSubListAboutKey<Key, Value>(this Dictionary<Key, Value> self)
        {
            List<Key> result = new();
            foreach (var item in self)
            {
                result.Add(item.Key);
            }
            return result;
        }

        public static List<T> GetSubListAboutValue<T, Key, Value>(this Dictionary<Key, Value> self) where Value : T
        {
            List<T> result = new();
            foreach (var item in self)
            {
                result.Add(item.Value);
            }
            return result;
        }

        public static List<T> GetSubListAboutKey<T, Key, Value>(this Dictionary<Key, Value> self) where Key : T
        {
            List<T> result = new();
            foreach (var item in self)
            {
                result.Add(item.Key);
            }
            return result;
        }

        public static List<Result> GetSubListAfterLinking<T, Result>(this List<T> self) where T : IProperty_Value_get<List<Result>>
        {
            List<Result> result = new();
            foreach (var single in self)
            {
                foreach (var item in single.Value)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public static List<Result> GetSubListAfterLinkingWithoutSameValue<T, Result>(this List<T> self) where T : IProperty_Value_get<List<Result>>
        {
            List<Result> result = new();
            foreach (var single in self)
            {
                foreach (var item in single.Value)
                {
                    if (!result.Contains(item))
                        result.Add(item);
                }
            }
            return result;
        }

        public static List<Result> GetSubList<Result, T>(this IEnumerable<T> self, Func<T, bool> predicate, Func<T, Result> transformFunc)
        {
            List<Result> result = new();
            foreach (var item in self)
            {
                if (predicate(item)) result.Add(transformFunc(item));
            }
            return result;
        }

        public static List<T> UnPackage<T>(this List<List<T>> self)
        {
            List<T> result = new();
            foreach (var item in self)
            {
                result.AddRange(item);
            }
            return result;
        }

        public static T[] SubArray<T>(this T[] self, int start, int end)
        {
            T[] result = new T[end - start];
            for (int i = start; i < end; i++)
            {
                result[i] = self[i];
            }
            return result;
        }

        public static T[] SubArray<T>(this T[] self, T[] buffer, int start, int end)
        {
            if (buffer == null || buffer.Length < end - start) throw new ADException("Buffer is null or too small");
            for (int i = start; i < end; i++)
            {
                buffer[i] = self[i];
            }
            return buffer;
        }

        public static T[] SafeSubArray<T>(this T[] self, T[] buffer, int start, int end)
        {
            if (buffer == null) return SubArray(self, start, end);
            for (int i = start, e = Mathf.Min(start + buffer.Length, end); i < e; i++)
            {
                buffer[i] = self[i];
            }
            return buffer;
        }

        public static List<Value> GetSubListAboutSortValue<Key, Value>(this Dictionary<Key, Value> self) where Key : IComparable<Key>
        {
            List<(Key, Value)> temp = new();
            foreach (var item in self)
            {
                temp.Add((item.Key, item.Value));
            }
            temp.Sort((T, P) => T.Item1.CompareTo(P.Item1));
            List<Value> result = new();
            for (int i = 0, e = temp.Count; i < e; i++)
            {
                result.Add(temp[i].Item2);
            }
            return result;
        }

        public static T[] Expand<T>(this T[] self, params T[] args)
        {
            T[] result = new T[args.Length + self.Length];
            for (int i = 0, e = self.Length; i < e; i++)
            {
                result[i] = self[i];
            }
            for (int i = 0, e = args.Length, p = self.Length; i < e; i++)
            {
                result[i + p] = args[i];
            }
            return result;
        }

        public static List<Result> GetSubList<Result, KeyArgs, T>(this IEnumerable<T> self, Func<T, (bool, KeyArgs)> predicate, Func<T, KeyArgs, Result> transformFunc)
        {
            List<Result> result = new();
            foreach (var item in self)
            {
                if (predicate(item).Share(out var keyArgs).Item1) result.Add(transformFunc(item, keyArgs.Item2));
            }
            return result;
        }

        public static List<Result> Contravariance<Origin, Result>(this IEnumerable<Origin> self)
            where Result : class, Origin
        {
            List<Result> result = new();
            foreach (var item in self)
            {
                if (item.As<Result>(out Result cat))
                {
                    result.Add(cat);
                }
            }
            return result;
        }


        public static List<Result> Contravariance<Origin, Result>(this IEnumerable<Origin> self, Func<Origin, Result> transformer)
        {
            List<Result> result = new();
            foreach (var item in self)
            {
                result.Add(transformer(item));
            }
            return result;
        }

        public static List<Result> Covariance<Origin, Result>(this IEnumerable<Origin> self)
            where Origin : class, Result
        {
            List<Result> result = new();
            foreach (var item in self)
            {
                result.Add(item);
            }
            return result;
        }

        public static List<T> RemoveNull<T>(this List<T> self)
        {
            self.RemoveAll(T => T == null);
            return self;
        }


        public static List<T> RemoveNullAsNew<T>(this List<T> self)
        {
            List<T> result = new();
            foreach (var item in self)
            {
                if (item != null)
                    result.Add(item);
            }
            return result;
        }

    }

    [System.Serializable]
    public class ADSerializableDictionary<TKey, TVal> : Dictionary<TKey, TVal>, ISerializationCallbackReceiver //where TKey : class
    {
        [Serializable]
        class Entry
        {
            public Entry() { }

            public Entry(TKey key, TVal value)
            {
                Key = key;
                Value = value;
            }

            public TKey Key;
            public TVal Value;
        }

        [SerializeField, JsonIgnore]
        private List<Entry> Data;

        public ADEvent<TKey> OnAdd = new(), OnTryAdd = new(), OnRemove = new();
        public ADEvent<TKey,bool> OnReplace = new();

        public virtual void OnBeforeSerialize()
        {
            Data = new();
            foreach (KeyValuePair<TKey, TVal> pair in this)
            {
                try
                {
                    Data.Add(new Entry(pair.Key, pair.Value));
                }
                catch { }
            }
        }

        // load dictionary from lists
        public virtual void OnAfterDeserialize()
        {
            if (Data == null) return;
            base.Clear();
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i] != null)
                {
                    if (!base.TryAdd(Data[i].Key, Data[i].Value))
                    {
                        Type typeTkey = typeof(TKey);
                        if (typeTkey == typeof(string)) (this as Dictionary<string, TVal>).Add("New Key", default);
                        else if (typeTkey.IsSubclassOf(typeof(Experimental.Localization.Cache.CacheAssetsKey)) || typeTkey == typeof(Experimental.Localization.Cache.CacheAssetsKey))
                            (this as Dictionary<Experimental.Localization.Cache.CacheAssetsKey, TVal>).Add(new("New Key"), default);
                        else if (typeTkey.IsSubclassOf(typeof(object))) base.Add(default, default);
                        else if (ReflectionExtension.IsPrimitive(typeTkey)) base.Add(default, default);
                    }
                }
            }

            Data = null;
        }

        public int RemoveNullValues()
        {
            var nullKeys = this.Where(pair => pair.Value == null)
                .Select(pair => pair.Key)
                .ToList();
            foreach (var nullKey in nullKeys)
                base.Remove(nullKey);
            return nullKeys.Count;
        }

        public new void Add(TKey key, TVal value)
        {
            base.Add(key, value);
            OnAdd.Invoke(key);
        }

        public new bool TryAdd(TKey key, TVal value)
        {
            bool result = base.TryAdd(key, value);
            OnTryAdd.Invoke(key);
            return result;
        }

        public new bool Remove(TKey key)
        {
            bool result = base.Remove(key);
            OnRemove.Invoke(key);
            return result;
        }

        public new TVal this[TKey key]
        {
            get
            {
                return base[key];
            }
            set
            {
                var result = base.ContainsKey(key);
                base[key] = value;
                this.OnReplace.Invoke(key, result);
            }
        }
    }
}


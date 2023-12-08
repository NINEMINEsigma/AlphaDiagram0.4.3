using System;
using System.Collections.Generic;
using System.Linq;
using AD.BASE;
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

        public static List<Result> GetSubList<Result, T>(this IEnumerable<T> self, Func<T, bool> predicate, Func<T, Result> transformFunc)
        {
            List<Result> result = new();
            foreach (var item in self)
            {
                if (predicate(item)) result.Add(transformFunc(item));
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

        [SerializeField]
        private List<Entry> Data;

        public void OnBeforeSerialize()
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
        public void OnAfterDeserialize()
        {
            if (Data == null) return;
            this.Clear();
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i] != null)
                {
                    try
                    {
                        if (!this.TryAdd(Data[i].Key, Data[i].Value))
                        {
                            if (typeof(TKey) == typeof(string)) this.Add(default, Data[i].Value);
                            else if (ReflectionExtension.IsPrimitive(typeof(TKey))) this.Add(default, Data[i].Value);
                        }
                    }
                    catch { }
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
                Remove(nullKey);
            return nullKeys.Count;
        }
    }
}


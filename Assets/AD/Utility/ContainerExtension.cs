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

    }

    [System.Serializable]
    public class ADSerializableDictionary<TKey, TVal> : Dictionary<TKey, TVal>, ISerializationCallbackReceiver where TKey : class
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
                            if (typeof(TKey) == typeof(string)) this.Add("New Key".As<TKey>(), Data[i].Value);
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

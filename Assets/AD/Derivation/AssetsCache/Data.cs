using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.Utility;
using Unity.VisualScripting;
using UnityEngine;

namespace AD.Experimental.Localization.Cache
{
    /// <summary>
    /// 能够对缓存对象进行管理以及历遍
    /// </summary>
    /// <typeparam name="Key">管理用的Key</typeparam>
    /// <typeparam name="T">IBase对象类型</typeparam>
    /// <typeparam name="P">IBase对象的目标IBaseMap类型</typeparam>
    public interface ICanOrganizeData<Key, T, P> :
        IEnumerable,
        IEnumerable<ICanCacheData<T, P>>
        where Key : CacheAssetsKey
        where T : class, IBase<P>, new()
        where P : class, IBaseMap<T>, new()
    {

    }
    /// <summary>
    /// 能够缓存对象并进行序列化的缓存接口
    /// </summary>
    /// <typeparam name="T">目标对象类型</typeparam>
    /// <typeparam name="P">目标对象的IBaseMap类型</typeparam>
    public interface ICanCacheData<T, P> : IBaseMap<T>
        where T : class, IBase<P>, new()
        where P : class, IBaseMap, new()
    {
        BindProperty<T> MatchElement { get; set; }
        BindProperty<P> MatchElementBM { get; set; }
    }
    [Serializable]
    /// <summary>
    /// 以一个字符串作为识别ID的Key类
    /// <para>继承并重写Equals与GetHashCode以实现更加扩展性的功能</para>
    /// </summary>
    public class CacheAssetsKey : IComparable<CacheAssetsKey>
    {
        public CacheAssetsKey() : this("New Key") { }
        public CacheAssetsKey(string key)
        {
            IdentifyID = key;
        }

        public string IdentifyID;

        public override bool Equals(object obj)
        {
            if (obj is CacheAssetsKey key)
                return IdentifyID.Equals(key.IdentifyID);
            else if (obj is string skey)
                return IdentifyID.Equals(skey);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return IdentifyID.GetHashCode();
        }
        public override string ToString()
        {
            return IdentifyID;
        }

        public int CompareTo(CacheAssetsKey other)
        {
            return this.IdentifyID.CompareTo(other.IdentifyID);
        }
    }
    /// <summary>
    /// 实现对其他缓存对象的依赖或获取
    /// </summary>
    public interface ICanAcquireOtherCache<Key, T, P>
        where Key : CacheAssetsKey
        where T : class, IBase<P>, new()
        where P : class, IBaseMap<T>, new()
    {
        T Acquire(ICanOrganizeData<Key, T, P> assets, string key);
    }
    /// <summary>
    /// 能够保存自身的Key值，以稳定自身在Assets中的环境和依赖于此的其他对象
    /// </summary>
    public interface ICanBeCatched<P> where P : PropertyAsset<string>, new()
    {
        IPropertyHasGet<string, P> CacheKey { get; }
    }

    [Serializable]
    /// <summary>
    /// 这个类的缓存方式使用IBase&IBaseMap的虚拟的序列化和反序列化方式来实现本地化和内存化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractCache<T, P> : ICanCacheData<T, P>, ICanBeCatched<PropertyAsset<string>>
        where T : class, IBase<P>, new()
        where P : class, IBaseMap<T>, new()
    {
        public AbstractCache()
        {
            BindKey = new();
            BindKey.SetOriginal("");
        }

        public AbstractCache(string key)
        {
            BindKey = new();
            BindKey.SetOriginal(key);
        }

        public IPropertyHasGet<string, PropertyAsset<string>> CacheKey => BindKey;

        protected BindProperty<string> BindKey;

        public abstract void ToObject(out T obj);

        public abstract bool FromObject(T from);

        public virtual void ToObject(out IBase obj)
        {
            ToObject(out obj);
        }

        public abstract bool FromObject(IBase from);

        public BindProperty<T> MatchElement { get; set; }
        public BindProperty<P> MatchElementBM { get; set; }

        public virtual bool Deserialize(string source)
        {
            if (MatchElementBM.Get().Deserialize(source))
            {
                MatchElementBM.Get().ToObject(out var target);
                MatchElement.Set(target);
                return true;
            }
            else return false;
        }

        public virtual string Serialize()
        {
            MatchElement.Get().ToMap(out var value);
            MatchElementBM.Set(value);
            return MatchElementBM.Get().Serialize();
        }

        public virtual bool UpdataMatchBaseByBM()
        {
            if (MatchElementBM.GetOriginal() == null) return false;
            MatchElementBM.Get().ToObject(out T result);
            MatchElement.Set(result);
            return true;
        }

        public virtual bool UpdataMatchBMByBase()
        {
            if (MatchElement.GetOriginal() == null) return false;
            MatchElement.Get().ToMap(out P result);
            MatchElementBM.Set(result);
            return true;
        }
    }

    [Serializable]
    public class CacheAssets<Key, T, P> : ICanOrganizeData<Key, T, P>
        where Key : CacheAssetsKey
        where T : class, IBase<P>, new()
        where P : class, IBaseMap<T>, new()
    {
        public class Enumerator : IEnumerator<ICanCacheData<T, P>>, IEnumerator, IDisposable
        {
            public Enumerator(
                Context.Enumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            Context.Enumerator enumerator;

            public ICanCacheData<T, P> Current => enumerator.Current.Value;

            object IEnumerator.Current => enumerator.Current.Value;

            public void Dispose()
            {
                enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Dispose();
            }
        }

        [Serializable]
        public class Context : ADSerializableDictionary<CacheAssetsKey, ICanCacheData<T, P>>, ISerializationCallbackReceiver
        {
            public override void OnAfterDeserialize()
            {
                base.OnAfterDeserialize();
            }

            public override void OnBeforeSerialize()
            {
                base.OnBeforeSerialize();
            }
        }

        [SerializeField] private Context datas = new();

        public IEnumerator<ICanCacheData<T, P>> GetEnumerator()
        {
            return new Enumerator(datas.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public ICollection<KeyValuePair<CacheAssetsKey, ICanCacheData<T, P>>> GetReadOnlyData()
        {
            return datas.AsReadOnlyCollection();
        }

        public Context GetData()
        {
            return datas;
        }

        public void Add(CacheAssetsKey key, ICanCacheData<T, P> cache) => TryAdd(key, cache);

        public bool TryAdd(CacheAssetsKey key, ICanCacheData<T, P> cache)
        {
            return datas.TryAdd(key, cache);
        }

        //返回值为尝试Add之前尝试获取的结果
        public bool AddOrGet(CacheAssetsKey key, ICanCacheData<T, P> cache, out ICanCacheData<T, P> result_slot)
        {
            bool result = datas.TryGetValue(key, out result_slot);
            if (!result)
            {
                datas.Add(key, result_slot);
                result_slot = cache;
            }
            return result;
        }

        public void Clear()
        {
            datas.Clear();
        }

        public bool Contains(CacheAssetsKey key)
        {
            return datas.ContainsKey(key);
        }

        public bool TryGetValue(CacheAssetsKey key, out ICanCacheData<T, P> result)
        {
            return datas.TryGetValue(key, out result);
        }

        public bool Contains(ICanCacheData<T, P> value)
        {
            return datas.ContainsValue(value);
        }

        public bool TryGetKey(ICanCacheData<T, P> value, out List<CacheAssetsKey> result)
        {
            result = null;
            bool isFind = false;
            foreach (var data in datas)
            {
                if (data.Value.Equals(value))
                {
                    result ??= new();
                    result.Add(data.Key);
                    isFind = true;
                }
            }
            return isFind;
        }

        public void Remove(CacheAssetsKey key)
        {
            datas.Remove(key);
        }



    }

    [Serializable]
    public class CacheAssets<Key, Value, T, P> : ICanOrganizeData<Key, T, P>
        where Key : CacheAssetsKey
        where Value : AbstractCache<T, P>
        where T : class, IBase<P>, new()
        where P : class, IBaseMap<T>, new()
    {
        public class Enumerator : IEnumerator<Value>, IEnumerator, IDisposable
        {
            public Enumerator(
                Context.Enumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            Context.Enumerator enumerator;

            public Value Current => enumerator.Current.Value;

            object IEnumerator.Current => enumerator.Current.Value;

            public void Dispose()
            {
                enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Dispose();
            }
        }

        [Serializable]
        public class Context : ADSerializableDictionary<Key, Value>, ISerializationCallbackReceiver
        {
            public override void OnAfterDeserialize()
            {
                base.OnAfterDeserialize();
            }

            public override void OnBeforeSerialize()
            {
                base.OnBeforeSerialize();
            }
        }

        [SerializeField] protected Context datas = new();

        public IEnumerator<ICanCacheData<T, P>> GetEnumerator()
        {
            return new Enumerator(datas.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public ICollection<KeyValuePair<Key, Value>> GetReadOnlyData()
        {
            return datas.AsReadOnlyCollection();
        }

        public Context GetData()
        {
            return datas;
        }

        public void Add(Key key, Value cache) => TryAdd(key, cache);

        public bool TryAdd(Key key, Value cache)
        {
            return datas.TryAdd(key, cache);
        }

        //返回值为尝试Add之前尝试获取的结果
        public bool AddOrGet(Key key, Value cache, out Value result_slot)
        {
            bool result = datas.TryGetValue(key, out result_slot);
            if (!result)
            {
                datas.Add(key, result_slot);
                result_slot = cache;
            }
            return result;
        }

        public void Clear()
        {
            datas.Clear();
        }

        public bool Contains(Key key)
        {
            return datas.ContainsKey(key);
        }

        public bool TryGetValue(Key key, out Value result)
        {
            return datas.TryGetValue(key, out result);
        }

        public bool Contains(Value value)
        {
            return datas.ContainsValue(value);
        }

        public bool TryGetKey(Value value, out List<Key> result)
        {
            result = null;
            bool isFind = false;
            foreach (var data in datas)
            {
                if (data.Value.Equals(value))
                {
                    result ??= new();
                    result.Add(data.Key);
                    isFind = true;
                }
            }
            return isFind;
        }

        public void Remove(Key key)
        {
            datas.Remove(key);
        }

        public int Count => datas.Count;

    }

}

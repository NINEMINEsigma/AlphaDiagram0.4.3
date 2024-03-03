using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AD.Experimental.EditorAsset.Cache
{
    /// <summary>
    /// 能够对缓存对象进行管理以及历遍
    /// </summary>
    /// <typeparam name="Key">管理用的Key</typeparam>
    /// <typeparam name="T">IBase对象类型</typeparam>
    /// <typeparam name="P">IBase对象的目标IBaseMap类型</typeparam>
    public interface ICanOrganizeData<Key> :
        IEnumerable,
        IEnumerable<ICanCacheData>
        where Key : CacheAssetsKey
    {

    }
    /// <summary>
    /// 能够缓存对象并进行序列化的缓存接口
    /// </summary>
    /// <typeparam name="T">目标对象类型</typeparam>
    /// <typeparam name="P">目标对象的IBaseMap类型</typeparam>
    public interface ICanCacheData
    {
    }
    /// <summary>
    /// 以一个字符串作为识别ID的Key类
    /// <para>继承并重写Equals与GetHashCode以实现更加扩展性的功能</para>
    /// </summary>
    [Serializable]
    public class CacheAssetsKey : IComparable<CacheAssetsKey>
    {
        public CacheAssetsKey() :this("New Key"){ }

        public CacheAssetsKey(string key)
        {
            IdentifyID = key;
        }

        [SerializeField] string IdentifyID;

        public override bool Equals(object obj)
        {
            if (obj is CacheAssetsKey ckey) return Equals(ckey);
            else if (obj is string skey) return Equals(skey);
            else return base.Equals(obj);
        }

        public bool Equals(CacheAssetsKey other)
        {
            return IdentifyID.Equals(other.IdentifyID);
        }

        public bool Equals(string key)
        {
            return IdentifyID.Equals(key);
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
    {
        T Acquire(ICanOrganizeData<Key> assets, string key);
    }
    /// <summary>
    /// 能够保存自身的Key值，以稳定自身在Assets中的环境和依赖于此的其他对象
    /// </summary>
    public interface ICanBeCatched
    {
        string CacheKey { get; }
    }

    /// <summary>
    /// 这个类的缓存方式使用IBase&IBaseMap的虚拟的序列化和反序列化方式来实现本地化和内存化
    /// [CreateAssetMenu(fileName = "New _", menuName = "AD/_", order = 10)]
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class AbstractScriptableObject : ScriptableObject, ICanCacheData, ICanBeCatched
    {
        public AbstractScriptableObject()
        {
        }

        public AbstractScriptableObject(string key)
        {
            BindKey = key;
        }

        public string CacheKey => BindKey;

        [SerializeField] protected string BindKey;
    }

    [Serializable]
    public abstract class CacheAssets<Key, _AbstractScriptableObject> : ScriptableObject, ICanOrganizeData<Key>
        where Key : CacheAssetsKey
        where _AbstractScriptableObject : AbstractScriptableObject, new()
    {
        public class Enumerator : IEnumerator<ICanCacheData>, IEnumerator, IDisposable
        {
            public Enumerator(
                List<SourcePair>.Enumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            List<SourcePair>.Enumerator enumerator;

            public ICanCacheData Current => enumerator.Current.data as ICanCacheData;

            object IEnumerator.Current => enumerator.Current.data;

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
        public class SourcePair : IComparable<SourcePair>
        {
            public Key key;
            public _AbstractScriptableObject data;

            public int CompareTo(SourcePair other)
            {
                return key.CompareTo(other.key);
            }
        }

        [SerializeField] List<SourcePair> AssetsData = new();

        public IEnumerator<ICanCacheData> GetEnumerator()
        {
            return new Enumerator(AssetsData.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public ICollection<SourcePair> GetReadOnlyData()
        {
            return AssetsData.AsReadOnlyCollection();
        }

        public List<SourcePair> GetData()
        {
            return AssetsData;
        }

        public void Add(Key key, _AbstractScriptableObject cache) => TryAdd(key, cache);

        public bool TryAdd(Key key, _AbstractScriptableObject cache)
        {
            if (AssetsData.Find(T => T.key.Equals(key)) == null)
            {
                AssetsData.Add(new SourcePair() { key = key, data = cache });
                return true;
            }
            return false;
        }

        //返回值为尝试Add之前尝试获取的结果
        public bool AddOrGet(Key key, _AbstractScriptableObject cache, out _AbstractScriptableObject result_slot)
        {
            result_slot = null;
            bool result = AssetsData.Find(T => T.key.Equals(key)) == null;
            if (result)
            {
                Add(key, cache);
                result_slot = cache;
            }
            return result;
        }

        public void Clear()
        {
            AssetsData.Clear();
        }

        public bool Contains(Key key)
        {
            return AssetsData.Find(T => T.key.Equals(key)) == null;
        }

        public bool TryGetValue(Key key, out _AbstractScriptableObject result)
        {
            result = null;
            var temp = AssetsData.Find(T => T.key.Equals(key));
            if (temp == null) return false;
            result = temp.data as _AbstractScriptableObject;
            return true;
        }

        public bool Contains(_AbstractScriptableObject value)
        {
            foreach (var data in AssetsData)
            {
                if (data.data.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryGetKey(_AbstractScriptableObject value, out List<Key> result)
        {
            result = null;
            bool isFind = false;
            foreach (var data in AssetsData)
            {
                if (data.data.Equals(value))
                {
                    result ??= new();
                    result.Add(data.key as Key);
                    isFind = true;
                }
            }
            return isFind;
        }

        public void Remove(Key key)
        {
            AssetsData.Remove(AssetsData.Find(_K => _K.key.Equals(key)));
        }
    }

}

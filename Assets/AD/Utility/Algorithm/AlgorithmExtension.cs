using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Utility
{
    public static class AlgorithmExtension
    {
        static AlgorithmExtension()
        {
            AlgorithmBases = new();
            RegisterAlgorithm(nameof(EaseCurve), null, new EaseCurve());
        }

        public class AlgorithmEntry
        {
            public string KeyName;
            public object KeyObject;
            public AlgorithmBase Algorithm;

            public AlgorithmEntry()
            {

            }

            public AlgorithmEntry(string keyName, object keyObject, AlgorithmBase algorithm)
            {
                KeyName = keyName;
                KeyObject = keyObject;
                Algorithm = algorithm;
            }
        }

        private static List<AlgorithmEntry> AlgorithmBases;

        public static AlgorithmBase GetAlgorithm(string algorithmName, object algorithmKeyObject, bool MustSafeBind = false)
        {
            foreach (var item in AlgorithmBases)
            {
                if (MustSafeBind && (item.KeyName == algorithmName || item.KeyObject == algorithmKeyObject))
                {
                    return item.Algorithm.Clone() ?? item.Algorithm;
                }
                if (!MustSafeBind && item.KeyName == algorithmName && item.KeyObject == algorithmKeyObject)
                {
                    return item.Algorithm.Clone() ?? item.Algorithm;
                }
            }
            return null;
        }

        public static AlgorithmBase GetAlgorithm(string algorithmName)
        {
            if (string.IsNullOrEmpty(algorithmName)) return null;
            foreach (var item in AlgorithmBases)
            {
                if (item.KeyName == algorithmName)
                {
                    return item.Algorithm.Clone() ?? item.Algorithm;
                }
            }
            return null;
        }

        public static AlgorithmBase GetAlgorithm(object algorithmKeyObject)
        {
            if (algorithmKeyObject == null) return null;
            foreach (var item in AlgorithmBases)
            {
                if (item.KeyObject == algorithmKeyObject)
                {
                    return item.Algorithm.Clone() ?? item.Algorithm;
                }
            }
            return null;
        }

        public static AlgorithmBase[] GetAlgorithms(string algorithmName, object algorithmKeyObject, bool MustSafeBind = false)
        {
            List<AlgorithmBase> result = new();
            foreach (var item in AlgorithmBases)
            {
                if (MustSafeBind && (item.KeyName == algorithmName || item.KeyObject == algorithmKeyObject))
                {
                    result.Add(item.Algorithm.Clone() ?? item.Algorithm);
                }
                if (!MustSafeBind && item.KeyName == algorithmName && item.KeyObject == algorithmKeyObject)
                {
                    result.Add(item.Algorithm.Clone() ?? item.Algorithm);
                }
            }
            return result.ToArray();
        }

        public static AlgorithmBase[] GetAlgorithms(string algorithmName)
        {
            List<AlgorithmBase> result = new();
            if (string.IsNullOrEmpty(algorithmName)) return result.ToArray();
            foreach (var item in AlgorithmBases)
            {
                if (item.KeyName == algorithmName)
                {
                    result.Add(item.Algorithm.Clone() ?? item.Algorithm);
                }
            }
            return result.ToArray();
        }

        public static AlgorithmBase[] GetAlgorithms(object algorithmKeyObject)
        {
            if (algorithmKeyObject == null) return null;
            List<AlgorithmBase> result = new();
            foreach (var item in AlgorithmBases)
            {
                if (item.KeyObject == algorithmKeyObject)
                {
                    result.Add(item.Algorithm.Clone() ?? item.Algorithm);
                }
            }
            return result.ToArray();
        }

        public static void RegisterAlgorithm(string KeyName, object KeyObject, AlgorithmBase algorithm)
        {
            AlgorithmBases.Add(new(KeyName, KeyObject, algorithm));
        }

        /// <summary>
        /// 你可以使用一个委托，或者一个具有Invoke名称的方法作为_Delegate的类型来作为算法的载体
        /// </summary>
        /// <typeparam name="_Delegate">一个委托，或者一个具有Invoke名称的方法</typeparam>
        /// <param name="KeyName"></param>
        /// <param name="KeyObject"></param>
        /// <param name="algorithm">_Delegate实体</param>
        public static void RegisterAlgorithm<_Delegate>(string KeyName, object KeyObject, _Delegate algorithm)
        {
            AlgorithmBases.Add(new(KeyName, KeyObject, new AlgorithmDelegate<_Delegate>(algorithm)));
        }

        public static void UnRegisterAlgorithm(AlgorithmBase algorithm)
        {
            AlgorithmBases.RemoveAll(T =>
            {
                return T.Algorithm == algorithm;
            });
        }

        public static void UnRegisterAlgorithm<_T>()
        {
            AlgorithmBases.RemoveAll(T =>
            {
                return T.Algorithm.GetType() == typeof(_T);
            });
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AD.Experimental.ADTypes;
using AD.Utility;

namespace AD.Experimental.ADInternal
{
    [UnityEngine.Scripting.Preserve]
    public static class ADTypeMgr
    {
        private static object _lock = new object();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static Dictionary<Type, ADType> types = null;

        // We cache the last accessed type as we quite often use the same type multiple times,
        // so this improves performance as another lookup is not required.
        private static ADType lastAccessedType = null;

        public static ADType GetOrCreateES3Type(Type type, bool throwException = true)
        {
            if (types == null)
                Init();

            if (type != typeof(object) && lastAccessedType != null && lastAccessedType.type == type)
                return lastAccessedType;

            // If type doesn't exist, create one.
            if (types.TryGetValue(type, out lastAccessedType))
                return lastAccessedType;
            return (lastAccessedType = CreateADType(type, throwException));
        }

        public static ADType GetES3Type(Type type)
        {
            if (types == null)
                Init();

            if (types.TryGetValue(type, out lastAccessedType))
                return lastAccessedType;
            return null;
        }

        internal static void Add(Type type, ADType es3Type)
        {
            if (types == null)
                Init();

            var existingType = GetES3Type(type);
            if (existingType != null && existingType.priority > es3Type.priority)
                return;

            lock (_lock)
            {
                types[type] = es3Type;
            }
        }

        //根据类型创建实际的ADType
        internal static ADType CreateADType(Type type, bool throwException = true)
        {
            ADType adType;

            if (ReflectionExtension.IsEnum(type))
                return new ES3Type_enum(type);
            else if (ReflectionExtension.TypeIsArray(type))
            {
                int rank = ReflectionExtension.GetArrayRank(type);
                if (rank == 1)
                    adType = new ES3ArrayType(type);
                else if (rank == 2)
                    adType = new ES32DArrayType(type);
                else if (rank == 3)
                    adType = new ES33DArrayType(type);
                else if (throwException)
                    throw new NotSupportedException("Only arrays with up to three dimensions are supported by Easy Save.");
                else
                    return null;
            }
            //是否是已预见的泛型容器
            else if (ReflectionExtension.IsGenericType(type) && ReflectionExtension.ImplementsInterface(type, typeof(IEnumerable)))
            {
                Type genericType = ReflectionExtension.GetGenericTypeDefinition(type);
                if (typeof(List<>).IsAssignableFrom(genericType))
                    adType = new ES3ListType(type);
                else if (typeof(IDictionary).IsAssignableFrom(genericType))
                    adType = new ES3DictionaryType(type);
                else if (genericType == typeof(Queue<>))
                    adType = new ES3QueueType(type);
                else if (genericType == typeof(Stack<>))
                    adType = new ES3StackType(type);
                else if (genericType == typeof(HashSet<>))
                    adType = new ES3HashSetType(type);
                else if (genericType == typeof(Unity.Collections.NativeArray<>))
                    adType = new ES3NativeArrayType(type);
                else if (throwException)
                    throw new NotSupportedException("Generic type \"" + type.ToString() + "\" is not supported by Easy Save.");
                else
                    return null;
            }
            //基础值类型
            else if (ReflectionExtension.IsPrimitive(type)) 
            {
                //TODO :  We should not have to create an ADType for a primitive.
                //为基础类型实现ADType
            }
            //Unity类型
            else
            {
                if (ReflectionExtension.IsAssignableFrom(typeof(Component), type))
                    adType = new ES3ReflectedComponentType(type);
                else if (ReflectionExtension.IsValueType(type))
                    adType = new ES3ReflectedValueType(type);
                else if (ReflectionExtension.IsAssignableFrom(typeof(ScriptableObject), type))
                    adType = new ES3ReflectedScriptableObjectType(type);
                else if (ReflectionExtension.IsAssignableFrom(typeof(UnityEngine.Object), type))
                    adType = new ES3ReflectedUnityObjectType(type);
                else if (type.Name.StartsWith("Tuple`"))
                    adType = new ES3TupleType(type);
                else
                    adType = new ES3ReflectedObjectType(type);
            }

            //可预见的判定结束后仍然没有得到相应的ADType，判定为失败
            if (adType.type == null || adType.isUnsupported)
            {
                if (throwException)
                    throw new NotSupportedException(string.Format("ADType.type is null when trying to create an ADType for {0}, possibly because the element type is not supported.", type));
                return null;
            }

            Add(type, adType);
            return adType;
        }

        //意义不明的初始化
        internal static void Init()
        {
            lock (_lock)
            {
                types = new Dictionary<Type, ADType>();
                // ADTypes add themselves to the types Dictionary.
                var _ = ReflectionExtension.GetInstances<ADType>();

                // Check that the type list was initialised correctly.
                if (types == null || types.Count == 0)
                    throw new TypeLoadException("Type list could not be initialised");
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.Utility;
using UnityEngine;

namespace AD.Types
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [UnityEngine.Scripting.Preserve]
    public abstract class ADType
    {
        #region ADType

        public const string typeFieldName = "__type";

        public ADMember[] members;
        public Type type;
        public bool isPrimitive { get; protected set; } = false;
        public bool isValueType { get; protected set; } = false;
        public bool isCollection { get; protected set; } = false;
        public bool isDictionary { get; protected set; } = false;
        public bool isTuple { get; protected set; } = false;
        public bool isEnum { get; protected set; } = false;
        public bool isADTypeUnityObject { get; protected set; } = false;
        public bool isReflectedType { get; protected set; } = false;
        public bool isUnsupported { get; protected set; } = false;
        public int priority { get; protected set; } = 0;

        protected ADType(Type type)
        {
            ADType.AddType(type, this);
            this.type = type;
            this.isValueType = ReflectionExtension.IsValueType(type);
        }

        public abstract void Write(object obj, ADStream stream);
        public abstract object Read<T>(ADStream stream);

        #endregion

        #region ADTypeMgr

        private static object _lock = new object();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static Dictionary<Type, ADType> types = null;

        // We cache the last accessed type as we quite often use the same type multiple times,
        // so this improves performance as another lookup is not required.
        private static ADType lastAccessedType = null;

        public static ADType GetOrCreateADType(Type type, bool throwException = true)
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

        public static ADType GetADType(Type type)
        {
            if (types == null)
                Init();

            if (types.TryGetValue(type, out lastAccessedType))
                return lastAccessedType;
            return null;
        }

        internal static void AddType(Type type, ADType es3Type)
        {
            if (types == null)
                Init();

            var existingType = GetADType(type);
            if (existingType != null && existingType.priority > es3Type.priority)
                return;

            lock (_lock)
            {
                types[type] = es3Type;
            }
        }

        internal static ADType CreateADType(Type type, bool throwException = true)
        {
            ADType adType = null;

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
                    throw new NotSupportedException("Only arrays with up to three dimensions are supported by ADType.");
                else
                    return null;
            }
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
            else if (ReflectionExtension.IsPrimitive(type)) // ERROR: We should not have to create an ES3Type for a primitive.
            {
                if (types == null || types.Count == 0)  // If the type list is not initialised, it is most likely an initialisation error.
                    throw new TypeLoadException("ES3Type for primitive could not be found, and the type list is empty. Please contact Easy Save developers at http://www.moodkie.com/contact");
                else // Else it's a different error, possibly an error in the specific ES3Type for that type.
                    throw new TypeLoadException("ES3Type for primitive could not be found, but the type list has been initialised and is not empty. Please contact Easy Save developers on mail@moodkie.com");
            }
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

            if (adType.type == null || adType.isUnsupported)
            {
                if (throwException)
                    throw new NotSupportedException(string.Format("ES3Type.type is null when trying to create an ES3Type for {0}, possibly because the element type is not supported.", type));
                return null;
            }

            ADType.AddType(type, adType);

            return adType;
        }

        internal static void Init()
        {
            lock (_lock)
            {
                types = new Dictionary<Type, ADType>();
                // ES3Types add themselves to the types Dictionary.
                ReflectionExtension.GetInstances<ADType>();

                // Check that the type list was initialised correctly.
                if (types == null || types.Count == 0)
                    throw new TypeLoadException("Type list could not be initialised");
            }
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ADPropertiesAttribute : System.Attribute
    {
        public readonly string[] members;

        public ADPropertiesAttribute(params string[] members)
        {
            this.members = members;
        }
    }

    public class ADMember
    {
        public string name;
        public Type type;
        public bool isProperty;
        public ReflectionExtension.ADReflectedMember reflectedMember;
        public bool useReflection = false;

        public ADMember(string name, Type type, bool isProperty)
        {
            this.name = name;
            this.type = type;
            this.isProperty = isProperty;
        }

        public ADMember(ReflectionExtension.ADReflectedMember reflectedMember)
        {
            this.reflectedMember = reflectedMember;
            this.name = reflectedMember.Name;
            this.type = reflectedMember.MemberType;
            this.isProperty = reflectedMember.isProperty;
            this.useReflection = true;
        }
    }

}

namespace AD.Types
{
    [UnityEngine.Scripting.Preserve]
    public abstract class ADCollectionType : ADType
    {
        public ADType elementType;

        public ADCollectionType(Type type) : base(type)
        {
            elementType = ADType.GetOrCreateADType(ReflectionExtension.GetElementTypes(type)[0], false);
            isCollection = true;

            if (elementType == null)
                isUnsupported = true;
        }

        public ADCollectionType(Type type, ADType elementType) : base(type)
        {
            this.elementType = elementType;
            isCollection = true;
        }

        public override object Read<T>(ADStream stream)
        {
            var current = stream.CurrentStr;
            for
        }

        public override void Write(object obj, ADStream stream)
        {
            throw new NotImplementedException();
        }

        protected abstract object ReadObject(string strs);
        protected abstract void WriteObject(object obj, ADStream stream);

        protected abstract object AddRange()
        {

        }
    }
}

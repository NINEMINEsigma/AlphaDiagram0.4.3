using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.Utility;

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

        public abstract void Write(object obj, ADWriter stream);
        public abstract object Read<T>(ADReader stream);

        public virtual void ReadInto<T>(ADReader reader, object obj)
        {
            throw new NotImplementedException("Self-assigning Read is not implemented or supported on this type.");
        }

        protected bool WriteUsingDerivedType(object obj, ADWriter writer)
        {
            var objType = obj.GetType();

            if (objType != this.type)
            {
                writer.WriteType(objType);
                ADType.GetOrCreateADType(objType).Write(obj, writer);
                return true;
            }
            return false;
        }

        protected void ReadUsingDerivedType<T>(ADReader reader, object obj)
        {
            ADType.GetOrCreateADType(reader.ReadType()).ReadInto<T>(reader, obj);
        }

        internal string ReadPropertyName(ADReader reader)
        {
            if (reader.overridePropertiesName != null)
            {
                string propertyName = reader.overridePropertiesName;
                reader.overridePropertiesName = null;
                return propertyName;
            }
            return reader.ReadPropertyName();
        }

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

            if (ReflectionExtension.IsEnum(type)) adType = CreateEnumType(type);
            else if (ReflectionExtension.TypeIsArray(type)) adType = CreateArrayType(type, throwException);
            else if (ReflectionExtension.IsGenericType(type)
                && ReflectionExtension.ImplementsInterface(type, typeof(IEnumerable))) adType = CreateGenericImplementsInterface(type, throwException);
            else if (ReflectionExtension.IsPrimitive(type)) adType = CreatePrimitiveType(type);
            else adType = CreateElseType(type);

            if (adType.type == null || adType.isUnsupported)
            {
                if (throwException)
                    throw new NotSupportedException(string.Format("ADType.type is null when trying to create an ADType for {0}, possibly because the element type is not supported.", type));
                return null;
            }

            ADType.AddType(type, adType);

            return adType;
        }

        private static ADType CreateEnumType(Type type)
        {
            //return new ES3Type_enum(type);
            throw new NotImplementedException();
        }
        private static ADType CreateArrayType(Type type,bool throwException)
        {
            throw new NotImplementedException();
            /*int rank = ReflectionExtension.GetArrayRank(type);
            if (rank == 1)
                return new ES3ArrayType(type);
            else if (rank == 2)
                return new AD2DArrayType(type);
            else if (rank == 3)
                return new ES33DArrayType(type);
            else if (throwException)
                throw new NotSupportedException("Only arrays with up to three dimensions are supported by ADType.");
            else
                return null;*/
        }
        private static ADType CreateGenericImplementsInterface(Type type, bool throwException)
        {
            throw new NotImplementedException();/*
            Type genericType = ReflectionExtension.GetGenericTypeDefinition(type);
            if (typeof(List<>).IsAssignableFrom(genericType))
                return new ES3ListType(type);
            else if (typeof(IDictionary).IsAssignableFrom(genericType))
                return new ES3DictionaryType(type);
            else if (genericType == typeof(Queue<>))
                return new ES3QueueType(type);
            else if (genericType == typeof(Stack<>))
                return new ES3StackType(type);
            else if (genericType == typeof(HashSet<>))
                return new ES3HashSetType(type);
            else if (genericType == typeof(Unity.Collections.NativeArray<>))
                return new ES3NativeArrayType(type);
            else if (throwException)
                throw new NotSupportedException("Generic type \"" + type.ToString() + "\" is not supported by Easy Save.");
            else
                return null;*/
        }
        private static ADType CreatePrimitiveType(Type type)
        {
            return null;
        }
        private static ADType CreateElseType(Type type)
        {
            throw new NotImplementedException();
            /*if (ReflectionExtension.IsAssignableFrom(typeof(Component), type))
                return new ES3ReflectedComponentType(type);
            else if (ReflectionExtension.IsValueType(type))
                return new ES3ReflectedValueType(type);
            else if (ReflectionExtension.IsAssignableFrom(typeof(ScriptableObject), type))
                return new ES3ReflectedScriptableObjectType(type);
            else if (ReflectionExtension.IsAssignableFrom(typeof(UnityEngine.Object), type))
                return new ES3ReflectedUnityObjectType(type);
            else if (type.Name.StartsWith("Tuple`"))
                return new ES3TupleType(type);
            else
                return new ES3ReflectedObjectType(type);*/
        }

        internal static void Init()
        {
            lock (_lock)
            {
                types = new Dictionary<Type, ADType>();
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

    public struct ADData
    {
        public ADType type;
        public byte[] bytes;

        public ADData(Type type, byte[] bytes)
        {
            this.type = type == null ? null : ADType.GetOrCreateADType(type);
            this.bytes = bytes;
        }

        public ADData(ADType type, byte[] bytes)
        {
            this.type = type;
            this.bytes = bytes;
        }
    }

}

namespace AD.Types
{
    [UnityEngine.Scripting.Preserve]
    public abstract class ADCollectionType : ADType
    {
        public ADType elementType;

        public abstract void ReadInto(ADReader reader, object obj);
        public abstract object Read(ADReader stream);

        public ADCollectionType(Type type) : base(type)
        {
            elementType = ADType.GetOrCreateADType(ReflectionExtension.GetElementTypes(type)[0], false);
            isCollection = true;

            // If the element type is null (i.e. unsupported), make this ES3Type null.
            if (elementType == null)
                isUnsupported = true;
        }

        public ADCollectionType(Type type, ADType elementType) : base(type)
        {
            this.elementType = elementType;
            isCollection = true;
        }

        protected virtual bool ReadICollection<T>(ADReader reader, ICollection<T> collection, ADType elementType)
        {
            if (reader.StartReadCollection())
                return false;

            // Iterate through each character until we reach the end of the array.
            while (true)
            {
                if (!reader.StartReadCollectionItem())
                    break;
                collection.Add(reader.Read<T>(elementType));

                if (reader.EndReadCollectionItem())
                    break;
            }

            reader.EndReadCollection();

            return true;
        }

        protected virtual void ReadICollectionInto<T>(ADReader reader, ICollection<T> collection, ADType elementType)
        {
            ReadICollectionInto(reader, collection, elementType);
        }

        [UnityEngine.Scripting.Preserve]
        protected virtual void ReadICollectionInto(ADReader reader, ICollection collection, ADType elementType)
        {
            if (reader.StartReadCollection())
                throw new NullReferenceException("The Collection we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

            int itemsLoaded = 0;

            // Iterate through each item in the collection and try to load it.
            foreach (var item in collection)
            {
                itemsLoaded++;

                if (!reader.StartReadCollectionItem())
                    break;

                reader.ReadInto<object>(item, elementType);

                // If we find a ']', we reached the end of the array.
                if (reader.EndReadCollectionItem())
                    break;

                // If there's still items to load, but we've reached the end of the collection we're loading into, throw an error.
                if (itemsLoaded == collection.Count)
                    throw new IndexOutOfRangeException("The collection we are loading is longer than the collection provided as a parameter.");
            }

            // If we loaded fewer items than the parameter collection, throw index out of range exception.
            if (itemsLoaded != collection.Count)
                throw new IndexOutOfRangeException("The collection we are loading is shorter than the collection provided as a parameter.");

            reader.EndReadCollection();
        }

    }

    #region Collection Type
    public class AD2DArrayType : ADCollectionType
    {
        public AD2DArrayType(Type type) : base(type) { }

        public override void Write(object obj, ADWriter writer)
        {
            var array = (System.Array)obj;

            if (elementType == null)
                throw new ArgumentNullException("ADType argument cannot be null.");

            //writer.StartWriteCollection();

            for (int i = 0; i < array.GetLength(0); i++)
            {
                writer.StartWriteCollectionItem(i);
                writer.StartWriteCollection();
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    writer.StartWriteCollectionItem(j);
                    writer.Write(array.GetValue(i, j), elementType);
                    writer.EndWriteCollectionItem(j);
                }
                writer.EndWriteCollection();
                writer.EndWriteCollectionItem(i);
            }

            //writer.EndWriteCollection();
        }

        public override object Read<T>(ADReader reader)
        {
            return Read(reader);
            /*if(reader.StartReadCollection())
				return null;

			// Create a List to store the items as a 1D array, which we can work out the positions of by calculating the lengths of the two dimensions.
			var items = new List<T>();
			int length1 = 0;

			// Iterate through each character until we reach the end of the array.
			while(true)
			{
				if(!reader.StartReadCollectionItem())
					break;

				ReadICollection<T>(reader, items, elementType);
				length1++;

				if(reader.EndReadCollectionItem())
					break;
			}

			int length2 = items.Count / length1;

			var array = new T[length1,length2];

			for(int i=0; i<length1; i++)
				for(int j=0; j<length2; j++)
					array[i,j] = items[ (i * length2) + j ];

			return array;*/
        }

        public override object Read(ADReader reader)
        {
            if (reader.StartReadCollection())
                return null;

            // Create a List to store the items as a 1D array, which we can work out the positions of by calculating the lengths of the two dimensions.
            var items = new List<object>();
            int length1 = 0;

            // Iterate through each character until we reach the end of the array.
            while (true)
            {
                if (!reader.StartReadCollectionItem())
                    break;

                ReadICollection<object>(reader, items, elementType);
                length1++;

                if (reader.EndReadCollectionItem())
                    break;
            }

            int length2 = items.Count / length1;

            var array = ES3Reflection.ArrayCreateInstance(elementType.type, new int[] { length1, length2 });

            for (int i = 0; i < length1; i++)
                for (int j = 0; j < length2; j++)
                    array.SetValue(items[(i * length2) + j], i, j);

            return array;
        }

        public override void ReadInto<T>(ADReader reader, object obj)
        {
            ReadInto(reader, obj);
        }

        public override void ReadInto(ADReader reader, object obj)
        {
            var array = (Array)obj;

            if (reader.StartReadCollection())
                throw new NullReferenceException("The Collection we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

            bool iHasBeenRead = false;

            for (int i = 0; i < array.GetLength(0); i++)
            {
                bool jHasBeenRead = false;

                if (!reader.StartReadCollectionItem())
                    throw new IndexOutOfRangeException("The collection we are loading is smaller than the collection provided as a parameter.");

                reader.StartReadCollection();
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (!reader.StartReadCollectionItem())
                        throw new IndexOutOfRangeException("The collection we are loading is smaller than the collection provided as a parameter.");
                    reader.ReadInto<object>(array.GetValue(i, j), elementType);
                    jHasBeenRead = reader.EndReadCollectionItem();
                }

                if (!jHasBeenRead)
                    throw new IndexOutOfRangeException("The collection we are loading is larger than the collection provided as a parameter.");

                reader.EndReadCollection();

                iHasBeenRead = reader.EndReadCollectionItem();
            }

            if (!iHasBeenRead)
                throw new IndexOutOfRangeException("The collection we are loading is larger than the collection provided as a parameter.");

            reader.EndReadCollection();
        }
    }
    #endregion
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AD.Utility;
using Unity.Collections;
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
                return new ADArrayType(type);
            else if (rank == 2)
                return new AD2DArrayType(type);
            else if (rank == 3)
                return new AD3DArrayType(type);
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
                return new ADListType(type);
            else if (typeof(IDictionary).IsAssignableFrom(genericType))
                return new ADDictionaryType(type);
            else if (genericType == typeof(Queue<>))
                return new ADQueueType(type);
            else if (genericType == typeof(Stack<>))
                return new ADStackType(type);
            else if (genericType == typeof(HashSet<>))
                return new ADHashSetType(type);
            else if (genericType == typeof(Unity.Collections.NativeArray<>))
                return new ADNativeArrayType(type);
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
                return new ADTupleType(type);
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

    #region Array

    [UnityEngine.Scripting.Preserve]
    public class ADArrayType : ADCollectionType
    {
        public ADArrayType(Type type) : base(type) { }
        public ADArrayType(Type type, ADType elementType) : base(type, elementType) { }

        public override void Write(object obj, ADWriter writer)
        {
            var array = (System.Array)obj;

            if (elementType == null)
                throw new ArgumentNullException("ADType argument cannot be null.");

            //writer.StartWriteCollection();

            for (int i = 0; i < array.Length; i++)
            {
                writer.StartWriteCollectionItem(i);
                writer.Write(array.GetValue(i), elementType);
                writer.EndWriteCollectionItem(i);
            }

            //writer.EndWriteCollection();
        }

        public override object Read(ADReader reader)
        {
            var list = new List<object>();
            if (!ReadICollection(reader, list, elementType))
                return null;

            var array = ReflectionExtension.ArrayCreateInstance(elementType.type, list.Count);
            int i = 0;
            foreach (var item in list)
            {
                array.SetValue(item, i);
                i++;
            }

            return array;
        }

        public override object Read<T>(ADReader reader)
        {
            return Read(reader);
        }

        public override void ReadInto<T>(ADReader reader, object obj)
        {
            ReadICollectionInto(reader, (ICollection)obj, elementType);
        }

        public override void ReadInto(ADReader reader, object obj)
        {
            var collection = (IList)obj;

            if (collection.Count == 0)
                Debug.LogWarning("LoadInto/ReadInto expects a collection containing instances to load data in to, but the collection is empty.");

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

            var array = ReflectionExtension.ArrayCreateInstance(elementType.type, new int[] { length1, length2 });

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

    public class AD3DArrayType : ADCollectionType
    {
        public AD3DArrayType(Type type) : base(type) { }

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
                    writer.StartWriteCollection();

                    for (int k = 0; k < array.GetLength(2); k++)
                    {
                        writer.StartWriteCollectionItem(k);
                        writer.Write(array.GetValue(i, j, k), elementType);
                        writer.EndWriteCollectionItem(k);
                    }
                    writer.EndWriteCollection();
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
        }

        public override object Read(ADReader reader)
        {
            if (reader.StartReadCollection())
                return null;

            // Create a List to store the items as a 1D array, which we can work out the positions of by calculating the lengths of the two dimensions.
            var items = new List<object>();
            int length1 = 0;
            int length2 = 0;

            // Iterate through each sub-array
            while (true)
            {
                if (!reader.StartReadCollectionItem())
                    break;
                reader.StartReadCollection();

                length1++;

                while (true)
                {
                    if (!reader.StartReadCollectionItem())
                        break;

                    ReadICollection<object>(reader, items, elementType);
                    length2++;

                    if (reader.EndReadCollectionItem())
                        break;
                }

                reader.EndReadCollection();
                if (reader.EndReadCollectionItem())
                    break;
            }

            reader.EndReadCollection();

            length2 = length2 / length1;
            int length3 = items.Count / length2 / length1;

            var array = ReflectionExtension.ArrayCreateInstance(elementType.type, new int[] { length1, length2, length3 });

            for (int i = 0; i < length1; i++)
                for (int j = 0; j < length2; j++)
                    for (int k = 0; k < length3; k++)
                        array.SetValue(items[i * (length2 * length3) + (j * length3) + k], i, j, k);

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
                    bool kHasBeenRead = false;

                    if (!reader.StartReadCollectionItem())
                        throw new IndexOutOfRangeException("The collection we are loading is smaller than the collection provided as a parameter.");

                    reader.StartReadCollection();

                    for (int k = 0; k < array.GetLength(2); k++)
                    {
                        if (!reader.StartReadCollectionItem())
                            throw new IndexOutOfRangeException("The collection we are loading is smaller than the collection provided as a parameter.");
                        reader.ReadInto<object>(array.GetValue(i, j, k), elementType);
                        kHasBeenRead = reader.EndReadCollectionItem();
                    }

                    if (!kHasBeenRead)
                        throw new IndexOutOfRangeException("The collection we are loading is larger than the collection provided as a parameter.");

                    reader.EndReadCollection();

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

    [UnityEngine.Scripting.Preserve]
    public class ADConcurrentDictionaryType : ADType
    {
        public ADType keyType;
        public ADType valueType;

        protected ReflectionExtension.ADReflectedMethod readMethod = null;
        protected ReflectionExtension.ADReflectedMethod readIntoMethod = null;

        public ADConcurrentDictionaryType(Type type) : base(type)
        {
            var types = ReflectionExtension.GetElementTypes(type);
            keyType = ADType.GetOrCreateADType(types[0], false);
            valueType = ADType.GetOrCreateADType(types[1], false);

            // If either the key or value type is unsupported, make this type NULL.
            if (keyType == null || valueType == null)
                isUnsupported = true; ;

            isDictionary = true;
        }

        public ADConcurrentDictionaryType(Type type, ADType keyType, ADType valueType) : base(type)
        {
            this.keyType = keyType;
            this.valueType = valueType;

            // If either the key or value type is unsupported, make this type NULL.
            if (keyType == null || valueType == null)
                isUnsupported = true; ;

            isDictionary = true;
        }

        public override void Write(object obj, ADWriter writer)
        {
            var dict = (IDictionary)obj;

            //writer.StartWriteDictionary(dict.Count);

            int i = 0;
            foreach (System.Collections.DictionaryEntry kvp in dict)
            {
                writer.StartWriteDictionaryKey(i);
                writer.Write(kvp.Key, keyType);
                writer.EndWriteDictionaryKey(i);
                writer.StartWriteDictionaryValue(i);
                writer.Write(kvp.Value, valueType);
                writer.EndWriteDictionaryValue(i);
                i++;
            }

            //writer.EndWriteDictionary();
        }

        public override object Read<T>(ADReader reader)
        {
            return Read(reader);
        }

        public override void ReadInto<T>(ADReader reader, object obj)
        {
            ReadInto(reader, obj);
        }

        /*
		 * 	Allows us to call the generic Read method using Reflection so we can define the generic parameter at runtime.
		 * 	It also caches the method to improve performance in later calls.
		 */
        public object Read(ADReader reader)
        {
            if (reader.StartReadDictionary())
                return null;

            var dict = (IDictionary)ReflectionExtension.CreateInstance(type);

            // Iterate through each character until we reach the end of the array.
            while (true)
            {
                if (!reader.StartReadDictionaryKey())
                    return dict;
                var key = reader.Read<object>(keyType);
                reader.EndReadDictionaryKey();

                reader.StartReadDictionaryValue();
                var value = reader.Read<object>(valueType);

                dict.Add(key, value);

                if (reader.EndReadDictionaryValue())
                    break;
            }

            reader.EndReadDictionary();

            return dict;
        }

        public void ReadInto(ADReader reader, object obj)
        {
            if (reader.StartReadDictionary())
                throw new NullReferenceException("The Dictionary we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

            var dict = (IDictionary)obj;

            // Iterate through each character until we reach the end of the array.
            while (true)
            {
                if (!reader.StartReadDictionaryKey())
                    return;
                var key = reader.Read<object>(keyType);

                if (!dict.Contains(key))
                    throw new KeyNotFoundException("The key \"" + key + "\" in the Dictionary we are loading does not exist in the Dictionary we are loading into");
                var value = dict[key];
                reader.EndReadDictionaryKey();

                reader.StartReadDictionaryValue();

                reader.ReadInto<object>(value, valueType);

                if (reader.EndReadDictionaryValue())
                    break;
            }

            reader.EndReadDictionary();
        }
    }

    [UnityEngine.Scripting.Preserve]
    public class ADDictionaryType : ADType
    {
        public ADType keyType;
        public ADType valueType;

        protected ReflectionExtension.ADReflectedMethod readMethod = null;
        protected ReflectionExtension.ADReflectedMethod readIntoMethod = null;

        public ADDictionaryType(Type type) : base(type)
        {
            var types = ReflectionExtension.GetElementTypes(type);
            keyType = ADType.GetOrCreateADType(types[0], false);
            valueType = ADType.GetOrCreateADType(types[1], false);

            // If either the key or value type is unsupported, make this type NULL.
            if (keyType == null || valueType == null)
                isUnsupported = true; ;

            isDictionary = true;
        }

        public ADDictionaryType(Type type, ADType keyType, ADType valueType) : base(type)
        {
            this.keyType = keyType;
            this.valueType = valueType;

            // If either the key or value type is unsupported, make this type NULL.
            if (keyType == null || valueType == null)
                isUnsupported = true; ;

            isDictionary = true;
        }

        public override void Write(object obj, ADWriter writer)
        {
            var dict = (IDictionary)obj;

            //writer.StartWriteDictionary(dict.Count);

            int i = 0;
            foreach (System.Collections.DictionaryEntry kvp in dict)
            {
                writer.StartWriteDictionaryKey(i);
                writer.Write(kvp.Key, keyType);
                writer.EndWriteDictionaryKey(i);
                writer.StartWriteDictionaryValue(i);
                writer.Write(kvp.Value, valueType);
                writer.EndWriteDictionaryValue(i);
                i++;
            }

            //writer.EndWriteDictionary();
        }

        public override object Read<T>(ADReader reader)
        {
            return Read(reader);
        }

        public override void ReadInto<T>(ADReader reader, object obj)
        {
            ReadInto(reader, obj);
        }

        /*
		 * 	Allows us to call the generic Read method using Reflection so we can define the generic parameter at runtime.
		 * 	It also caches the method to improve performance in later calls.
		 */
        public object Read(ADReader reader)
        {
            if (reader.StartReadDictionary())
                return null;

            var dict = (IDictionary)ReflectionExtension.CreateInstance(type);

            // Iterate through each character until we reach the end of the array.
            while (true)
            {
                if (!reader.StartReadDictionaryKey())
                    return dict;
                var key = reader.Read<object>(keyType);
                reader.EndReadDictionaryKey();

                reader.StartReadDictionaryValue();
                var value = reader.Read<object>(valueType);

                dict.Add(key, value);

                if (reader.EndReadDictionaryValue())
                    break;
            }

            reader.EndReadDictionary();

            return dict;
        }

        public void ReadInto(ADReader reader, object obj)
        {
            if (reader.StartReadDictionary())
                throw new NullReferenceException("The Dictionary we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

            var dict = (IDictionary)obj;

            // Iterate through each character until we reach the end of the array.
            while (true)
            {
                if (!reader.StartReadDictionaryKey())
                    return;
                var key = reader.Read<object>(keyType);

                if (!dict.Contains(key))
                    throw new KeyNotFoundException("The key \"" + key + "\" in the Dictionary we are loading does not exist in the Dictionary we are loading into");
                var value = dict[key];
                reader.EndReadDictionaryKey();

                reader.StartReadDictionaryValue();

                reader.ReadInto<object>(value, valueType);

                if (reader.EndReadDictionaryValue())
                    break;
            }

            reader.EndReadDictionary();
        }
    }

    [UnityEngine.Scripting.Preserve]
    public class ADHashSetType : ADCollectionType
    {
        public ADHashSetType(Type type) : base(type) { }

        public override void Write(object obj, ADWriter writer)
        {
            if (obj == null) { writer.WriteNull(); return; };

            var list = (IEnumerable)obj;

            if (elementType == null)
                throw new ArgumentNullException("ADType argument cannot be null.");

            int count = 0;
            foreach (var item in list)
                count++;

            //writer.StartWriteCollection(count);

            int i = 0;
            foreach (object item in list)
            {
                writer.StartWriteCollectionItem(i);
                writer.Write(item, elementType);
                writer.EndWriteCollectionItem(i);
                i++;
            }

            //writer.EndWriteCollection();
        }

        public override object Read<T>(ADReader reader)
        {
            var val = Read(reader);
            if (val == null)
                return default(T);
            return (T)val;
        }


        public override object Read(ADReader reader)
        {
            /*var method = typeof(ES3CollectionType).GetMethod("ReadICollection", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(elementType.type);
            if(!(bool)method.Invoke(this, new object[] { reader, list, elementType }))
                return null;*/

            var genericParam = ReflectionExtension.GetGenericArguments(type)[0];
            var listType = ReflectionExtension.MakeGenericType(typeof(List<>), genericParam);
            var list = (IList)ReflectionExtension.CreateInstance(listType);

            if (!reader.StartReadCollection())
            {
                // Iterate through each character until we reach the end of the array.
                while (true)
                {
                    if (!reader.StartReadCollectionItem())
                        break;
                    list.Add(reader.Read<object>(elementType));

                    if (reader.EndReadCollectionItem())
                        break;
                }

                reader.EndReadCollection();
            }

            return ReflectionExtension.CreateInstance(type, list);
        }

        public override void ReadInto<T>(ADReader reader, object obj)
        {
            ReadInto(reader, obj);
        }

        public override void ReadInto(ADReader reader, object obj)
        {
            throw new NotImplementedException("Cannot use LoadInto/ReadInto with HashSet because HashSets do not maintain the order of elements");
        }
    }

    [UnityEngine.Scripting.Preserve]
    public class ADListType : ADCollectionType
    {
        public ADListType(Type type) : base(type) { }
        public ADListType(Type type, ADType elementType) : base(type, elementType) { }

        public override void Write(object obj, ADWriter writer)
        {
            if (obj == null) { writer.WriteNull(); return; };

            var list = (IList)obj;

            if (elementType == null)
                throw new ArgumentNullException("ADType argument cannot be null.");

            //writer.StartWriteCollection();

            int i = 0;
            foreach (object item in list)
            {
                writer.StartWriteCollectionItem(i);
                writer.Write(item, elementType);
                writer.EndWriteCollectionItem(i);
                i++;
            }

            //writer.EndWriteCollection();
        }

        public override object Read<T>(ADReader reader)
        {
            return Read(reader);

            /*var list = new List<T>();
			if(!ReadICollection<T>(reader, list, elementType))
				return null;
			return list;*/
        }

        public override void ReadInto<T>(ADReader reader, object obj)
        {
            ReadICollectionInto(reader, (ICollection)obj, elementType);
        }

        public override object Read(ADReader reader)
        {
            var instance = (IList)ReflectionExtension.CreateInstance(type);

            if (reader.StartReadCollection())
                return null;

            // Iterate through each character until we reach the end of the array.
            while (true)
            {
                if (!reader.StartReadCollectionItem())
                    break;
                instance.Add(reader.Read<object>(elementType));

                if (reader.EndReadCollectionItem())
                    break;
            }

            reader.EndReadCollection();

            return instance;
        }

        public override void ReadInto(ADReader reader, object obj)
        {
            var collection = (IList)obj;

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

    [UnityEngine.Scripting.Preserve]
    public class ADNativeArrayType : ADCollectionType
    {
        public ADNativeArrayType(Type type) : base(type) { }
        public ADNativeArrayType(Type type, ADType elementType) : base(type, elementType) { }

        public override void Write(object obj, ADWriter writer)
        {
            if (elementType == null)
                throw new ArgumentNullException("ADType argument cannot be null.");

            var enumerable = (IEnumerable)obj;

            int i = 0;
            foreach (var item in enumerable)
            {
                writer.StartWriteCollectionItem(i);
                writer.Write(item, elementType);
                writer.EndWriteCollectionItem(i);
                i++;
            }
        }

        public override object Read(ADReader reader)
        {
            var array = ReadAsArray(reader);

            return ReflectionExtension.CreateInstance(type, new object[] { array, Allocator.Persistent });
        }

        public override object Read<T>(ADReader reader)
        {
            return Read(reader);
        }

        public override void ReadInto<T>(ADReader reader, object obj)
        {
            ReadInto(reader, obj);
        }

        public override void ReadInto(ADReader reader, object obj)
        {
            var array = ReadAsArray(reader);
            var copyFromMethod = ReflectionExtension.GetMethods(type, "CopyFrom").First(m => ReflectionExtension.TypeIsArray(m.GetParameters()[0].GetType()));
            copyFromMethod.Invoke(obj, new object[] { array });
        }

        private System.Array ReadAsArray(ADReader reader)
        {
            var list = new List<object>();
            if (!ReadICollection(reader, list, elementType))
                return null;

            var array = ReflectionExtension.ArrayCreateInstance(elementType.type, list.Count);
            int i = 0;
            foreach (var item in list)
            {
                array.SetValue(item, i);
                i++;
            }

            return array;
        }
    }

    [UnityEngine.Scripting.Preserve]
    public class ADQueueType : ADCollectionType
    {
        public ADQueueType(Type type) : base(type) { }

        public override void Write(object obj, ADWriter writer)
        {
            var list = (ICollection)obj;

            if (elementType == null)
                throw new ArgumentNullException("ADType argument cannot be null.");

            //writer.StartWriteCollection();

            int i = 0;
            foreach (object item in list)
            {
                writer.StartWriteCollectionItem(i);
                writer.Write(item, elementType);
                writer.EndWriteCollectionItem(i);
                i++;
            }

            //writer.EndWriteCollection();
        }

        public override void ReadInto<T>(ADReader reader, object obj)
        {
            if (reader.StartReadCollection())
                throw new NullReferenceException("The Collection we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

            int itemsLoaded = 0;

            var queue = (Queue<T>)obj;

            // Iterate through each item in the collection and try to load it.
            foreach (var item in queue)
            {
                itemsLoaded++;

                if (!reader.StartReadCollectionItem())
                    break;

                reader.ReadInto<T>(item, elementType);

                // If we find a ']', we reached the end of the array.
                if (reader.EndReadCollectionItem())
                    break;
                // If there's still items to load, but we've reached the end of the collection we're loading into, throw an error.
                if (itemsLoaded == queue.Count)
                    throw new IndexOutOfRangeException("The collection we are loading is longer than the collection provided as a parameter.");
            }

            // If we loaded fewer items than the parameter collection, throw index out of range exception.
            if (itemsLoaded != queue.Count)
                throw new IndexOutOfRangeException("The collection we are loading is shorter than the collection provided as a parameter.");

            reader.EndReadCollection();
        }

        public override object Read<T>(ADReader reader)
        {
            return Read(reader);
        }

        public override object Read(ADReader reader)
        {
            var instance = (IList)ReflectionExtension.CreateInstance(ReflectionExtension.MakeGenericType(typeof(List<>), elementType.type));

            if (reader.StartReadCollection())
                return null;

            // Iterate through each character until we reach the end of the array.
            while (true)
            {
                if (!reader.StartReadCollectionItem())
                    break;
                instance.Add(reader.Read<object>(elementType));

                if (reader.EndReadCollectionItem())
                    break;
            }

            reader.EndReadCollection();

            return ReflectionExtension.CreateInstance(type, instance);
        }

        public override void ReadInto(ADReader reader, object obj)
        {
            if (reader.StartReadCollection())
                throw new NullReferenceException("The Collection we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

            int itemsLoaded = 0;

            var collection = (ICollection)obj;

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

    [UnityEngine.Scripting.Preserve]
    public class ADStackType : ADCollectionType
    {
        public ADStackType(Type type) : base(type) { }

        public override void Write(object obj, ADWriter writer)
        {
            var list = (ICollection)obj;

            if (elementType == null)
                throw new ArgumentNullException("ADType argument cannot be null.");

            //writer.StartWriteCollection();

            int i = 0;
            foreach (object item in list)
            {
                writer.StartWriteCollectionItem(i);
                writer.Write(item, elementType);
                writer.EndWriteCollectionItem(i);
                i++;
            }

            //writer.EndWriteCollection();
        }

        public override object Read<T>(ADReader reader)
        {
            return Read(reader);
        }

        public override void ReadInto<T>(ADReader reader, object obj)
        {
            if (reader.StartReadCollection())
                throw new NullReferenceException("The Collection we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

            int itemsLoaded = 0;

            var stack = (Stack<T>)obj;

            // Iterate through each item in the collection and try to load it.
            foreach (var item in stack)
            {
                itemsLoaded++;

                if (!reader.StartReadCollectionItem())
                    break;

                reader.ReadInto<T>(item, elementType);

                // If we find a ']', we reached the end of the array.
                if (reader.EndReadCollectionItem())
                    break;
                // If there's still items to load, but we've reached the end of the collection we're loading into, throw an error.
                if (itemsLoaded == stack.Count)
                    throw new IndexOutOfRangeException("The collection we are loading is longer than the collection provided as a parameter.");
            }

            // If we loaded fewer items than the parameter collection, throw index out of range exception.
            if (itemsLoaded != stack.Count)
                throw new IndexOutOfRangeException("The collection we are loading is shorter than the collection provided as a parameter.");

            reader.EndReadCollection();
        }

        public override object Read(ADReader reader)
        {
            var instance = (IList)ReflectionExtension.CreateInstance(ReflectionExtension.MakeGenericType(typeof(List<>), elementType.type));

            if (reader.StartReadCollection())
                return null;

            // Iterate through each character until we reach the end of the array.
            while (true)
            {
                if (!reader.StartReadCollectionItem())
                    break;
                instance.Add(reader.Read<object>(elementType));

                if (reader.EndReadCollectionItem())
                    break;
            }

            reader.EndReadCollection();

            ReflectionExtension.GetMethods(instance.GetType(), "Reverse").FirstOrDefault(t => !t.IsStatic).Invoke(instance, new object[] { });
            return ReflectionExtension.CreateInstance(type, instance);

        }

        public override void ReadInto(ADReader reader, object obj)
        {
            if (reader.StartReadCollection())
                throw new NullReferenceException("The Collection we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

            int itemsLoaded = 0;

            var collection = (ICollection)obj;

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

    [UnityEngine.Scripting.Preserve]
    public class ADTupleType : ADType
    {
        public ADType[] adTypes;
        public Type[] subTypes;

        protected ReflectionExtension.ADReflectedMethod readMethod = null;
        protected ReflectionExtension.ADReflectedMethod readIntoMethod = null;

        public ADTupleType(Type type) : base(type)
        {
            subTypes = ReflectionExtension.GetElementTypes(type);
            adTypes = new ADType[subTypes.Length];

            for (int i = 0; i < subTypes.Length; i++)
            {
                adTypes[i] = ADType.GetOrCreateADType(subTypes[i], false);
                if (adTypes[i] == null)
                    isUnsupported = true;
            }

            isTuple = true;
        }

        public override void Write(object obj, ADWriter writer)
        {
            if (obj == null) { writer.WriteNull(); return; };

            writer.StartWriteCollection();

            for (int i = 0; i < adTypes.Length; i++)
            {
                var itemProperty = ReflectionExtension.GetProperty(type, "Item" + (i + 1));
                var item = itemProperty.GetValue(obj);
                writer.StartWriteCollectionItem(i);
                writer.Write(item, adTypes[i]);
                writer.EndWriteCollectionItem(i);
            }

            writer.EndWriteCollection();
        }

        public override object Read<T>(ADReader reader)
        {
            var objects = new object[subTypes.Length];

            if (reader.StartReadCollection())
                return null;

            for (int i = 0; i < subTypes.Length; i++)
            {
                reader.StartReadCollectionItem();
                objects[i] = reader.Read<object>(adTypes[i]);
                reader.EndReadCollectionItem();
            }

            reader.EndReadCollection();

            var constructor = type.GetConstructor(subTypes);
            var instance = constructor.Invoke(objects);

            return instance;
        }
    }

    #endregion
}

namespace AD.Types
{
    [UnityEngine.Scripting.Preserve]
    [ADProperties()]
    public class ADType_Type : ADType
    {
        public static ADType Instance = null;

        public ADType_Type() : base(typeof(System.Type))
        {
            Instance = this;
        }

        public override void Write(object obj, ADWriter writer)
        {
            Type type = (Type)obj;
            writer.WriteProperty("assemblyQualifiedName", type.AssemblyQualifiedName);
        }

        public override object Read<T>(ADReader reader)
        {
            return Type.GetType(reader.ReadProperty<string>());
        }
    }
}

namespace AD.Types
{
    [UnityEngine.Scripting.Preserve]
    public abstract class ADObjectType : ADType
    {
        public ADObjectType(Type type) : base(type) { }

        protected abstract void WriteObject(object obj, ADWriter writer);
        protected abstract object ReadObject<T>(ADReader reader);

        protected virtual void ReadObject<T>(ADReader reader, object obj)
        {
            throw new NotSupportedException("ReadInto is not supported for type " + type);
        }

        public override void Write(object obj, ADWriter writer)
        {
            if (!WriteUsingDerivedType(obj, writer))
            {
                var baseType = ReflectionExtension.BaseType(obj.GetType());
                if (baseType != typeof(object))
                {
                    var adType = ADType.GetOrCreateADType(baseType, false);
                    // If it's a Dictionary or Collection, we need to write it as a field with a property name.
                    if (adType != null && (adType.isDictionary || adType.isCollection))
                        writer.WriteProperty("_Values", obj, adType);
                }

                WriteObject(obj, writer);
            }
        }

        public override object Read<T>(ADReader reader)
        {
            string propertyName;
            while (true)
            {
                propertyName = ReadPropertyName(reader);

                if (propertyName == ADType.typeFieldName)
                    return ADType.GetOrCreateADType(reader.ReadType()).Read<T>(reader);
                else
                {
                    reader.overridePropertiesName = propertyName;

                    return ReadObject<T>(reader);
                }
            }
        }

        public override void ReadInto<T>(ADReader reader, object obj)
        {
            string propertyName;
            while (true)
            {
                propertyName = ReadPropertyName(reader);

                if (propertyName == ADType.typeFieldName)
                {
                    ADType.GetOrCreateADType(reader.ReadType()).ReadInto<T>(reader, obj);
                    return;
                }
                // This is important we return if the enumerator returns null, otherwise we will encounter an endless cycle.
                else if (propertyName == null)
                    return;
                else
                {
                    reader.overridePropertiesName = propertyName;
                    ReadObject<T>(reader, obj);
                }
            }
        }
    }
}

namespace AD.Types
{
    #region String

    [UnityEngine.Scripting.Preserve]
    public class ADType_string : ADType
    {
        public static ADType Instance = null;

        public ADType_string() : base(typeof(string))
        {
            isPrimitive = true;
            Instance = this;
        }

        public override void Write(object obj, ADWriter writer)
        {
            writer.WritePrimitive((string)obj);
        }

        public override object Read<T>(ADReader reader)
        {
            return reader.Read_string();
        }
    }

    public class ADType_StringArray : ADArrayType
    {
        public static ADType Instance;

        public ADType_StringArray() : base(typeof(string[]), ADType_string.Instance)
        {
            Instance = this;
        }
    }

    #endregion
}

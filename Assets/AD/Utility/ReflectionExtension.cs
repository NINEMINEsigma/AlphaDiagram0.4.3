using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using AD.BASE;
using AD.Utility.Pipe;
using Unity.VisualScripting;
using UnityEngine;

namespace AD.Utility
{
    [Serializable]
    public class ReflectionException : ADException
    {
        public ReflectionException() { }
        public ReflectionException(string message) : base(message) { }
        public ReflectionException(string message, Exception inner) : base(message, inner) { }
        protected ReflectionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class FieldException : ReflectionException
    {
        public FieldException() : base("Error : Field") { }
    }

    [Serializable]
    public class MethodException : ReflectionException
    {
        public MethodException() : base("Error : Method") { }
    }

    public static class ReflectionExtension
    {
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static string[] assemblyNames = new string[] { "Assembly-CSharp-firstpass", "Assembly-CSharp" };

        public static readonly BindingFlags PublicFlags = BindingFlags.Public | BindingFlags.Instance;
        public static readonly BindingFlags DefaultBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        public static readonly BindingFlags AllBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        public static bool CreateInstance(this Assembly assembly, string fullName, out object obj)
        {
            obj = assembly.CreateInstance(fullName);
            return obj != null;
        }

        public static bool CreatePipeLineStep<T, P>(this object self, string methodName, out PipeFunc pipeFunc)
        {
            pipeFunc = null;
            MethodInfo method_info = self.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            if (method_info == null) return false;
            pipeFunc = new((obj) => method_info.Invoke(self, new object[] { obj }), typeof(T), typeof(P));
            return true;
        }

        public static bool CreatePipeLineStep<T, P>(this Assembly assembly, string fullName, out PipeFunc pipeFunc)
        {
            pipeFunc = null;
            string objName = fullName[..fullName.LastIndexOf('.')], methodName = fullName[fullName.LastIndexOf('.')..];
            var a = assembly.CreateInstance(objName);
            if (a == null) return false;
            return CreatePipeLineStep<T, P>(a, methodName, out pipeFunc);
        }

        public static bool Run(this Assembly assembly, string typeName, string detecter, string targetFuncName)
        {
            var objs = UnityEngine.Object.FindObjectsOfType(assembly.GetType(typeName));
            string objName = detecter[..detecter.LastIndexOf('.')], methodName = detecter[detecter.LastIndexOf('.')..];
            var a = assembly.CreateInstance(objName);
            if (a == null) return false;
            a.GetType().GetMethod("DetecterInit")?.Invoke(a, new object[] { });
            var detecterFunc = a.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            if (detecterFunc == null) return false;
            foreach (var obj in objs)
            {
                if (obj == null) continue;
                if ((bool)detecterFunc.Invoke(a, new object[] { obj }))
                {
                    var targetFunc = obj.GetType().GetMethod(targetFuncName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
                    if (targetFunc == null) return false;
                    targetFunc.Invoke(obj, new object[] { });
                    return true;
                }
            }
            return false;
        }

        public static bool Run(this Assembly assembly, string typeName, object detecter, string detecterFuncName, string targetFuncName)
        {
            var objs = UnityEngine.Object.FindObjectsOfType(assembly.GetType(typeName));
            if (detecter == null) return false;
            detecter.GetType().GetMethod("DetecterInit")?.Invoke(detecter, new object[] { });
            var detecterFunc = detecter.GetType().GetMethod(detecterFuncName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            if (detecterFunc == null) return false;
            foreach (var obj in objs)
            {
                if (obj == null) continue;
                if ((bool)detecterFunc.Invoke(detecter, new object[] { obj }))
                {
                    var targetFunc = obj.GetType().GetMethod(targetFuncName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
                    if (targetFunc == null) return false;
                    targetFunc.Invoke(obj, new object[] { });
                    return true;
                }
            }
            return false;
        }

        public class TypeResult
        {
            public Type type;
            public object target;
            public string CallingName;

            public void Init(Type type, object target, string CallingName = "@")
            {
                this.type = type;
                this.target = target;
                this.CallingName = CallingName;
            }
        }

        public class FullAutoRunResultInfo
        {
            public bool result = true;
            public Exception ex = null;
            public TypeResult[] typeResults = null;
        }

        public static FullAutoRunResultInfo FullAutoRun<T>(this T self, string callingStr)
        {
            string[] callingName = callingStr.Split("->");
            TypeResult[] currentStack = new TypeResult[callingName.Length + 1];
            for (int i = 0, e = callingName.Length + 1; i < e; i++)
            {
                currentStack[i] = new();
            }
            try
            {
                currentStack[0].Init(self.GetType(), self);
                for (int i = 0, e = callingName.Length; i < e; i++)
                {
                    TypeResult current = currentStack[i], next = currentStack[i + 1];
                    object currentTarget = callingName[i].Contains('(')
                        ? GetCurrentTargetWhenCallFunc(current.target, callingName[i], current)
                        : GetCurrentTargetWhenGetField(callingName[i], current);
                    next.Init(currentTarget?.GetType(), currentTarget, callingName[i]);
                }
                //TypeResult resultCammand = currentStack[^1];
            }
            catch (Exception ex)
            {
                return new() { result = false, ex = ex, typeResults = currentStack };
            }
            return new() { typeResults = currentStack };
        }

        private static object[] GetCurrentArgsWhenNeedArgs<T>(T self, string currentCallingName)
        {
            object[] currentArgs;
            string[] currentArgsStrs = currentCallingName.Split(',');
            currentArgs = new object[currentArgsStrs.Length];
            for (int j = 0, e2 = currentArgsStrs.Length; j < e2; j++)
            {
                if (currentArgsStrs[j][0] == '\"' && currentArgsStrs[j][^1] == '\"')
                    currentArgs[j] = currentArgsStrs[j][1..^1];
                else if (currentArgsStrs[j][0] == '$')
                    currentArgs[j] = float.Parse(currentArgsStrs[j][1..]);
                else if (!self.FullAutoRun(out currentArgs[j], currentArgsStrs[j]).result)
                    throw new ReflectionException("Parse Error : ResultValue");
            }
            return currentArgs;
        }

        private static object GetCurrentTargetWhenCallFunc<T>(T self, string currentCallingName, TypeResult current)
        {
            object currentTarget;
            object[] currentArgs = new object[0];
            int a_s = currentCallingName.IndexOf('(') + 1, b_s = currentCallingName.LastIndexOf(")");
            if (b_s - a_s > 1)
            {
                currentArgs = GetCurrentArgsWhenNeedArgs(self, currentCallingName[a_s..b_s]);
            }
            MethodBase method =
                current.target.GetType().GetMethod(currentCallingName[..(a_s - 1)], DefaultBindingFlags)
                ?? throw new MethodException();
            currentTarget = method.Invoke(current.target, currentArgs);
            return currentTarget;
        }

        private static object GetCurrentTargetWhenGetField(string currentCallingName, TypeResult current)
        {
            object currentTarget;
            FieldInfo data =
                current.target.GetType().GetField(currentCallingName, DefaultBindingFlags)
                ?? throw new FieldException();
            currentTarget = data.GetValue(current.target);
            return currentTarget;
        }

        public static FullAutoRunResultInfo FullAutoRun<T>(this T self, out object result, string callingStr)
        {
            string[] callingName = callingStr.Split("->");
            TypeResult[] currentStack = new TypeResult[callingName.Length + 1];
            for (int i = 0, e = callingName.Length + 1; i < e; i++)
            {
                currentStack[i] = new();
            }
            try
            {
                currentStack[0].Init(self.GetType(), self);
                for (int i = 0, e = callingName.Length; i < e; i++)
                {
                    TypeResult current = currentStack[i], next = currentStack[i + 1];
                    object currentTarget = callingName[i].Contains('(')
                        ? GetCurrentTargetWhenCallFunc(current.target, callingName[i], current)
                        : GetCurrentTargetWhenGetField(callingName[i], current);
                    next.Init(currentTarget.GetType(), currentTarget, callingName[i]);
                }
                TypeResult resultCammand = currentStack[^1];
                result = resultCammand.target;
            }
            catch (Exception ex)
            {
                result = null;
                return new() { result = false, ex = ex, typeResults = currentStack };
            }
            return new() { typeResults = currentStack };
        }

        public static Type ToType(this string self)
        {
            return Assembly.GetExecutingAssembly().GetType(self);
        }

        public static Type ToType(this string self, Assembly assembly)
        {
            return assembly.GetType(self);
        }

        public static Type Typen(string typeName, string singleTypeName = null)
        {
            Type type = null;
            Assembly[] assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
            int assemblyArrayLength = assemblyArray.Length;
            for (int i = 0; i < assemblyArrayLength; ++i)
            {
                type = assemblyArray[i].GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            for (int i = 0; (i < assemblyArrayLength); ++i)
            {
                Type[] typeArray = assemblyArray[i].GetTypes();
                int typeArrayLength = typeArray.Length;
                for (int j = 0; j < typeArrayLength; ++j)
                {
                    if (typeArray[j].Name.Equals(singleTypeName ?? typeName))
                    {
                        return typeArray[j];
                    }
                }
            }
            return type;
        }

        public static Type Typen(this Assembly self, string typeName, string singleTypeName = null)
        {
            Type type = self.GetType(typeName);
            if (type != null)
            {
                return type;
            }
            Type[] typeArray = self.GetTypes();
            int typeArrayLength = typeArray.Length;
            for (int j = 0; j < typeArrayLength; ++j)
            {
                if (typeArray[j].Name.Equals(singleTypeName ?? typeName))
                {
                    return typeArray[j];
                }
            }
            return type;
        }

        public static object CreateInstance(this Type type)
        {
            return Activator.CreateInstance(type);
        }

        public static T CreateInstance<T>(this Type type)
        {
            return (T)Activator.CreateInstance(type);
        }

        public static object GetFieldByName(this object self, string fieldName)
        {
            return self.GetType().GetField(fieldName, DefaultBindingFlags).GetValue(self);
        }

        public static T GetFieldByName<T>(this object self, string fieldName)
        {
            return (T)self.GetType().GetField(fieldName, DefaultBindingFlags).GetValue(self);
        }

        public static object GetFieldByName(this object self, string fieldName, BindingFlags flags)
        {
            return self.GetType().GetField(fieldName, flags).GetValue(self);
        }

        public static T GetFieldByName<T>(this object self, string fieldName, BindingFlags flags)
        {
            return (T)self.GetType().GetField(fieldName, flags).GetValue(self);
        }

        public static object RunMethodByName(this object self, string methodName, BindingFlags flags, params object[] args)
        {
            return self.GetType().GetMethod(methodName, flags).Invoke(self, args);
        }

        public static FieldInfo[] GetAllFields(this object self)
        {
            return self.GetType().GetFields(AllBindingFlags);
        }

        public static PropertyInfo[] GetAllProperties(this object self)
        {
            return self.GetType().GetProperties(AllBindingFlags);
        }

        public static Type[] GetAllInterfaces(this object self)
        {
            return self.GetType().GetInterfaces();
        }

        public static bool GetInterface<T>(this object self, out Type _interface, out T result)
        {
            _interface = self.GetType().GetInterface(typeof(T).Name);
            result = (T)self;
            return _interface != null;
        }

        public static Type GetInterface<T>(this object self)
        {
            return self.GetType().GetInterface(typeof(T).Name);
        }

        public const string memberFieldPrefix = "m_";
        public const string componentTagFieldName = "tag";
        public const string componentNameFieldName = "name";
        public static readonly string[] excludedPropertyNames = new string[] { "runInEditMode", "useGUILayout", "hideFlags" };

        public static readonly Type serializableAttributeType = typeof(System.SerializableAttribute);
        public static readonly Type serializeFieldAttributeType = typeof(SerializeField);
        public static readonly Type obsoleteAttributeType = typeof(System.ObsoleteAttribute);
        public static readonly Type nonSerializedAttributeType = typeof(System.NonSerializedAttribute);

        public static Type[] EmptyTypes = new Type[0];

        private static Assembly[] _assemblies = null;
        private static Assembly[] Assemblies
        {
            get
            {
                if (_assemblies == null)
                {
                    _assemblies = AppDomain.CurrentDomain.GetAssemblies();
                }
                return _assemblies;
            }
        }

        public static void AssembliesUpdate()
        {
            _assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        public static Assembly[] GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        /*	
		 * 	Gets the element type of a collection or array.
		 * 	Returns null if type is not a collection type.
		 */
        public static Type[] GetElementTypes(Type type)
        {
            if (IsGenericType(type))
                return GetGenericArguments(type);
            else if (type.IsArray)
                return new Type[] { GetElementType(type) };
            else
                return null;
        }

        public static List<FieldInfo> GetSerializableFields(Type type,
                                                            List<FieldInfo> serializableFields = null,
                                                            bool safe = true,
                                                            string[] memberNames = null,
                                                            BindingFlags bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
        {
            if (type == null)
                return new List<FieldInfo>();

            var fields = type.GetFields(bindings);

            serializableFields ??= new List<FieldInfo>();

            foreach (var field in fields)
            {
                var fieldName = field.Name;

                // If a members array was provided as a parameter, only include the field if it's in the array.
                if (memberNames != null)
                    if (!memberNames.Contains(fieldName))
                        continue;

                var fieldType = field.FieldType;

                if (safe)
                {
                    // If the field is private, only serialize it if it's explicitly marked as serializable.
                    if (!field.IsPublic && !AttributeIsDefined(field, serializeFieldAttributeType))
                        continue;
                }

                // Exclude const or readonly fields.
                if (field.IsLiteral || field.IsInitOnly)
                    continue;

                // Don't store fields whose type is the same as the class the field is housed in unless it's stored by reference (to prevent cyclic references)
                if (fieldType == type && !IsAssignableFrom(typeof(UnityEngine.Object), fieldType))
                    continue;

                // If property is marked as obsolete or non-serialized, don't serialize it.
                if (AttributeIsDefined(field, nonSerializedAttributeType) || AttributeIsDefined(field, obsoleteAttributeType))
                    continue;

                if (!TypeIsSerializable(field.FieldType))
                    continue;

                // Don't serialize member fields.
                if (safe && fieldName.StartsWith(memberFieldPrefix) && field.DeclaringType.Namespace != null && field.DeclaringType.Namespace.Contains("UnityEngine"))
                    continue;

                serializableFields.Add(field);
            }

            var baseType = BaseType(type);
            if (baseType != null && baseType != typeof(System.Object) && baseType != typeof(UnityEngine.Object))
                GetSerializableFields(BaseType(type), serializableFields, safe, memberNames);

            return serializableFields;
        }

        public static List<PropertyInfo> GetSerializableProperties(Type type,
                                                                   List<PropertyInfo> serializableProperties = null,
                                                                   bool safe = true,
                                                                   string[] memberNames = null,
                                                                   BindingFlags bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
        {
            bool isComponent = IsAssignableFrom(typeof(UnityEngine.Component), type);

            // Only get private properties if we're not getting properties safely.
            if (!safe)
                bindings = bindings | BindingFlags.NonPublic;

            var properties = type.GetProperties(bindings);

            if (serializableProperties == null)
                serializableProperties = new List<PropertyInfo>();

            foreach (var p in properties)
            {
                var propertyName = p.Name;

                if (excludedPropertyNames.Contains(propertyName))
                    continue;

                // If a members array was provided as a parameter, only include the property if it's in the array.
                if (memberNames != null)
                    if (!memberNames.Contains(propertyName))
                        continue;

                if (safe)
                {
                    // If safe serialization is enabled, only get properties which are explicitly marked as serializable.
                    if (!AttributeIsDefined(p, serializeFieldAttributeType))
                        continue;
                }

                var propertyType = p.PropertyType;

                // Don't store properties whose type is the same as the class the property is housed in unless it's stored by reference (to prevent cyclic references)
                if (propertyType == type && !IsAssignableFrom(typeof(UnityEngine.Object), propertyType))
                    continue;

                if (!p.CanRead || !p.CanWrite)
                    continue;

                // Only support properties with indexing if they're an array.
                if (p.GetIndexParameters().Length != 0 && !propertyType.IsArray)
                    continue;

                // Check that the type of the property is one which we can serialize.
                // Also check whether an ES3Type exists for it.
                if (!TypeIsSerializable(propertyType))
                    continue;

                // Ignore certain properties on components.
                if (isComponent)
                {
                    // Ignore properties which are accessors for GameObject fields.
                    if (propertyName == componentTagFieldName || propertyName == componentNameFieldName)
                        continue;
                }

                // If property is marked as obsolete or non-serialized, don't serialize it.
                if (AttributeIsDefined(p, obsoleteAttributeType) || AttributeIsDefined(p, nonSerializedAttributeType))
                    continue;

                serializableProperties.Add(p);
            }

            var baseType = BaseType(type);
            if (baseType != null && baseType != typeof(System.Object))
                GetSerializableProperties(baseType, serializableProperties, safe, memberNames);

            return serializableProperties;
        }

        public static bool TypeIsSerializable(Type type)
        {
            if (type == null)
                return false;

            if (IsPrimitive(type) || IsValueType(type) || IsAssignableFrom(typeof(UnityEngine.Component), type) || IsAssignableFrom(typeof(UnityEngine.ScriptableObject), type))
                return true;

            if (TypeIsArray(type))
            {
                if (TypeIsSerializable(type.GetElementType()))
                    return true;
                return false;
            }

            var genericArgs = type.GetGenericArguments();
            for (int i = 0; i < genericArgs.Length; i++)
                if (!TypeIsSerializable(genericArgs[i]))
                    return false;

            /*if (HasParameterlessConstructor(type))
                return true;*/
            return false;
        }

        public static System.Object CreateInstance(Type type, params object[] args)
        {
            if (IsAssignableFrom(typeof(ScriptableObject), type))
                return ScriptableObject.CreateInstance(type);
            return Activator.CreateInstance(type, args);
        }

        public static Array ArrayCreateInstance(Type type, int length)
        {
            return Array.CreateInstance(type, new int[] { length });
        }

        public static Array ArrayCreateInstance(Type type, int[] dimensions)
        {
            return Array.CreateInstance(type, dimensions);
        }

        public static Type MakeGenericType(Type type, Type genericParam)
        {
            return type.MakeGenericType(genericParam);
        }

        public static ADReflectedMember[] GetSerializableMembers(Type type, bool safe = true, string[] memberNames = null)
        {
            if (type == null)
                return new ADReflectedMember[0];

            var fieldInfos = GetSerializableFields(type, new List<FieldInfo>(), safe, memberNames);
            var propertyInfos = GetSerializableProperties(type, new List<PropertyInfo>(), safe, memberNames);
            var reflectedFields = new ADReflectedMember[fieldInfos.Count + propertyInfos.Count];

            for (int i = 0; i < fieldInfos.Count; i++)
                reflectedFields[i] = new ADReflectedMember(fieldInfos[i]);
            for (int i = 0; i < propertyInfos.Count; i++)
                reflectedFields[i + fieldInfos.Count] = new ADReflectedMember(propertyInfos[i]);

            return reflectedFields;
        }

        public static ADReflectedMember GetADReflectedProperty(Type type, string propertyName)
        {
            var propertyInfo = GetProperty(type, propertyName);
            return new ADReflectedMember(propertyInfo);
        }

        public static ADReflectedMember GetADReflectedMember(Type type, string fieldName)
        {
            var fieldInfo = GetField(type, fieldName);
            return new ADReflectedMember(fieldInfo);
        }

        /*
		 * 	Finds all classes of a specific type, and then returns an instance of each.
		 * 	Ignores classes which can't be instantiated (i.e. abstract classes, those without parameterless constructors).
		 */
        public static IList<T> GetInstances<T>()
        {
            var instances = new List<T>();
            foreach (var assembly in Assemblies)
                foreach (var type in assembly.GetTypes())
                    if (IsAssignableFrom(typeof(T), type) && HasParameterlessConstructor(type) && !IsAbstract(type))
                        instances.Add((T)Activator.CreateInstance(type));
            return instances;
        }

        public static IList<Type> GetDerivedTypes(Type derivedType)
        {
            return
                (
                    from assembly in Assemblies
                    from type in assembly.GetTypes()
                    where IsAssignableFrom(derivedType, type)
                    select type
                ).ToList();
        }

        public static IList<Type> GetTagedTypes(Type attributeType)
        {
            return
                (
                    from assembly in Assemblies
                    from type in assembly.GetTypes()
                    where type.GetCustomAttributes(attributeType, false).Length == 1
                    select type
                ).ToList();
        }

        public static IList<Attribute> GetTagedTypesAttributes(Type attributeType)
        {
            return
                (
                    from assembly in Assemblies
                    from type in assembly.GetTypes()
                    where type.GetCustomAttribute(attributeType, false) != null
                    select type.GetCustomAttribute(attributeType, false)
                ).ToList();
        }

        public static IList<_AttributeType> GetTagedTypesAttributes<_AttributeType>() where _AttributeType : Attribute
        {
            var attributeType = typeof(_AttributeType);
            return
                (
                    from assembly in Assemblies
                    from type in assembly.GetTypes()
                    where type.GetCustomAttribute(attributeType, false) != null
                    select type.GetCustomAttribute(attributeType, false) as _AttributeType
                ).ToList();
        }

        public static IList<CustomAttributeData> GetTagedTypesAttributeDatas<_AttributeType>()
        {
            var attributeType = typeof(_AttributeType);
            return
                (
                    from assembly in Assemblies
                    from type in assembly.GetTypes()
                    from data in type.GetCustomAttributesData()
                    where data.AttributeType == attributeType
                    select data
                ).ToList();
        }

        public static bool IsAssignableFrom(Type a, Type b)
        {
            return a.IsAssignableFrom(b);
        }

        public static Type GetGenericTypeDefinition(Type type)
        {
            return type.GetGenericTypeDefinition();
        }

        public static Type[] GetGenericArguments(Type type)
        {
            return type.GetGenericArguments();
        }

        public static int GetArrayRank(Type type)
        {
            return type.GetArrayRank();
        }

        public static string GetAssemblyQualifiedName(Type type)
        {
            return type.AssemblyQualifiedName;
        }

        public static ADReflectedMethod GetMethod(Type type, string methodName, Type[] genericParameters, Type[] parameterTypes)
        {
            return new ADReflectedMethod(type, methodName, genericParameters, parameterTypes);
        }

        public static bool TypeIsArray(Type type)
        {
            return type.IsArray;
        }

        public static Type GetElementType(Type type)
        {
            return type.GetElementType();
        }
        public static bool IsAbstract(Type type)
        {
            return type.IsAbstract;
        }

        public static bool IsInterface(Type type)
        {
            return type.IsInterface;
        }

        public static bool IsGenericType(Type type)
        {
            return type.IsGenericType;
        }

        public static bool IsValueType(Type type)
        {
            return type.IsValueType;
        }

        public static bool IsEnum(Type type)
        {
            return type.IsEnum;
        }

        public static bool HasParameterlessConstructor(Type type)
        {
            if (IsValueType(type) || GetParameterlessConstructor(type) != null)
                return true;
            return false;
        }

        public static ConstructorInfo GetParameterlessConstructor(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var constructor in constructors)
                if (constructor.GetParameters().Length == 0)
                    return constructor;
            return null;
        }

        public static string GetShortAssemblyQualifiedName(Type type)
        {
            if (IsPrimitive(type))
                return type.ToString();
            return type.FullName + "," + type.Assembly.GetName().Name;
        }

        public static PropertyInfo GetProperty(Type type, string propertyName)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null && BaseType(type) != typeof(object))
                return GetProperty(BaseType(type), propertyName);
            return property;
        }

        public static FieldInfo GetField(Type type, string fieldName)
        {
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null && BaseType(type) != typeof(object))
                return GetField(BaseType(type), fieldName);
            return field;
        }

        public static MethodInfo[] GetMethods(Type type, string methodName)
        {
            return type.GetMethods().Where(t => t.Name == methodName).ToArray();
        }

        public static bool IsPrimitive(Type type)
        {
            return (type.IsPrimitive || type == typeof(string) || type == typeof(decimal));
        }

        public static bool AttributeIsDefined(MemberInfo info, Type attributeType)
        {
            return Attribute.IsDefined(info, attributeType, true);
        }

        public static bool AttributeIsDefined(Type type, Type attributeType)
        {
            return type.IsDefined(attributeType, true);
        }

        public static bool ImplementsInterface(Type type, Type interfaceType)
        {
            return (type.GetInterface(interfaceType.Name) != null);
        }

        public static Type BaseType(Type type)
        {
            return type.BaseType;
        }

        public static Type GetType(string typeString)
        {
            switch (typeString)
            {
                case "bool":
                    return typeof(bool);
                case "byte":
                    return typeof(byte);
                case "sbyte":
                    return typeof(sbyte);
                case "char":
                    return typeof(char);
                case "decimal":
                    return typeof(decimal);
                case "double":
                    return typeof(double);
                case "float":
                    return typeof(float);
                case "int":
                    return typeof(int);
                case "uint":
                    return typeof(uint);
                case "long":
                    return typeof(long);
                case "ulong":
                    return typeof(ulong);
                case "short":
                    return typeof(short);
                case "ushort":
                    return typeof(ushort);
                case "string":
                    return typeof(string);
                case "Vector2":
                    return typeof(Vector2);
                case "Vector3":
                    return typeof(Vector3);
                case "Vector4":
                    return typeof(Vector4);
                case "Color":
                    return typeof(Color);
                case "Transform":
                    return typeof(Transform);
                case "Component":
                    return typeof(UnityEngine.Component);
                case "GameObject":
                    return typeof(GameObject);
                case "MeshFilter":
                    return typeof(MeshFilter);
                case "Material":
                    return typeof(Material);
                case "Texture2D":
                    return typeof(Texture2D);
                case "UnityEngine.Object":
                    return typeof(UnityEngine.Object);
                case "System.Object":
                    return typeof(object);
                default:
                    return Type.GetType(typeString);
            }
        }

        public static string GetTypeString(Type type)
        {
            if (type == typeof(bool))
                return "bool";
            else if (type == typeof(byte))
                return "byte";
            else if (type == typeof(sbyte))
                return "sbyte";
            else if (type == typeof(char))
                return "char";
            else if (type == typeof(decimal))
                return "decimal";
            else if (type == typeof(double))
                return "double";
            else if (type == typeof(float))
                return "float";
            else if (type == typeof(int))
                return "int";
            else if (type == typeof(uint))
                return "uint";
            else if (type == typeof(long))
                return "long";
            else if (type == typeof(ulong))
                return "ulong";
            else if (type == typeof(short))
                return "short";
            else if (type == typeof(ushort))
                return "ushort";
            else if (type == typeof(string))
                return "string";
            else if (type == typeof(Vector2))
                return "Vector2";
            else if (type == typeof(Vector3))
                return "Vector3";
            else if (type == typeof(Vector4))
                return "Vector4";
            else if (type == typeof(Color))
                return "Color";
            else if (type == typeof(Transform))
                return "Transform";
            else if (type == typeof(UnityEngine.Component))
                return "Component";
            else if (type == typeof(GameObject))
                return "GameObject";
            else if (type == typeof(MeshFilter))
                return "MeshFilter";
            else if (type == typeof(Material))
                return "Material";
            else if (type == typeof(Texture2D))
                return "Texture2D";
            else if (type == typeof(UnityEngine.Object))
                return "UnityEngine.Object";
            else if (type == typeof(object))
                return "System.Object";
            else
                return GetShortAssemblyQualifiedName(type);
        }

        /*
        * 	Allows us to use FieldInfo and PropertyInfo interchangably.
        */
        public struct ADReflectedMember
        {
            // The FieldInfo or PropertyInfo for this field.
            private FieldInfo fieldInfo;
            private PropertyInfo propertyInfo;
            public bool isProperty;

            public bool IsNull { get { return fieldInfo == null && propertyInfo == null; } }
            public string Name { get { return (isProperty ? propertyInfo.Name : fieldInfo.Name); } }
            public Type MemberType { get { return (isProperty ? propertyInfo.PropertyType : fieldInfo.FieldType); } }
            public bool IsPublic { get { return (isProperty ? (propertyInfo.GetGetMethod(true).IsPublic && propertyInfo.GetSetMethod(true).IsPublic) : fieldInfo.IsPublic); } }
            public bool IsProtected { get { return (isProperty ? (propertyInfo.GetGetMethod(true).IsFamily) : fieldInfo.IsFamily); } }
            public bool IsStatic { get { return (isProperty ? (propertyInfo.GetGetMethod(true).IsStatic) : fieldInfo.IsStatic); } }

            public ADReflectedMember(System.Object fieldPropertyInfo)
            {
                if (fieldPropertyInfo == null)
                {
                    this.propertyInfo = null;
                    this.fieldInfo = null;
                    isProperty = false;
                    return;
                }

                isProperty = IsAssignableFrom(typeof(PropertyInfo), fieldPropertyInfo.GetType());
                if (isProperty)
                {
                    this.propertyInfo = (PropertyInfo)fieldPropertyInfo;
                    this.fieldInfo = null;
                }
                else
                {
                    this.fieldInfo = (FieldInfo)fieldPropertyInfo;
                    this.propertyInfo = null;
                }
            }

            public void SetValue(System.Object obj, System.Object value)
            {
                if (isProperty)
                    propertyInfo.SetValue(obj, value, null);
                else
                    fieldInfo.SetValue(obj, value);
            }

            public System.Object GetValue(System.Object obj)
            {
                if (isProperty)
                    return propertyInfo.GetValue(obj, null);
                else
                    return fieldInfo.GetValue(obj);
            }
        }

        public class ADReflectedMethod
        {
            private MethodInfo method;
            public int ArgsTotal { get; private set; }

            public ADReflectedMethod(Type type, string methodName, Type[] genericParameters, Type[] parameterTypes)
            {
                MethodInfo nonGenericMethod = type.GetMethod(methodName, parameterTypes);
                this.method = nonGenericMethod.MakeGenericMethod(genericParameters);
                ArgsTotal = parameterTypes == null ? 0 : parameterTypes.Length;
            }

            public ADReflectedMethod(Type type, string methodName, Type[] genericParameters, Type[] parameterTypes, BindingFlags bindingAttr)
            {
                MethodInfo nonGenericMethod = type.GetMethod(methodName, bindingAttr, null, parameterTypes, null);
                this.method = nonGenericMethod.MakeGenericMethod(genericParameters);
                ArgsTotal = parameterTypes == null ? 0 : parameterTypes.Length;
            }

            public ADReflectedMethod(Type type, string methodName, Type[] parameterTypes)
            {
                MethodInfo nonGenericMethod = type.GetMethod(methodName, parameterTypes);
                ArgsTotal = parameterTypes == null ? 0 : parameterTypes.Length;
            }

            public ADReflectedMethod(Type type, string methodName, Type[] parameterTypes, BindingFlags bindingAttr)
            {
                MethodInfo nonGenericMethod = type.GetMethod(methodName, bindingAttr, null, parameterTypes, null);
                ArgsTotal = parameterTypes == null ? 0 : parameterTypes.Length;
            }

            public object Invoke(object obj, object[] parameters = null)
            {
                if (parameters.Length != ArgsTotal) throw new ADException("The number of parameters is incorrect");
                return method.Invoke(obj, parameters);
            }
        }

        public static void _ShowAttributeData(IList<CustomAttributeData> attributes)
        {
            foreach (CustomAttributeData cad in attributes)
            {
                Debug.Log("   " + cad);
                Debug.Log("      Constructor: '" + cad.Constructor + "'");

                Debug.Log("      Constructor arguments:");
                foreach (CustomAttributeTypedArgument cata
                    in cad.ConstructorArguments)
                {
                    _ShowValueOrArray(cata);
                }

                Debug.Log("      Named arguments:");
                foreach (CustomAttributeNamedArgument cana
                    in cad.NamedArguments)
                {
                    Debug.Log("         MemberInfo: '" + cana.MemberInfo + "'");
                    _ShowValueOrArray(cana.TypedValue);
                }
            }
        }

        public static void _ShowValueOrArray(CustomAttributeTypedArgument cata)
        {
            if (cata.Value.GetType() == typeof(ReadOnlyCollection<CustomAttributeTypedArgument>))
            {
                Debug.Log("         Array of '" + cata.ArgumentType + "':");

                foreach (CustomAttributeTypedArgument cataElement in
                    (ReadOnlyCollection<CustomAttributeTypedArgument>)cata.Value)
                {
                    Debug.Log("             Type: '" + cataElement.ArgumentType + "'  Value: '" + cataElement.Value + "'");
                }
            }
            else
            {
                Debug.Log("         Type: '" + cata.ArgumentType + "'  Value: '" + cata.Value + "'");
            }
        }

        public static CustomAttributeNamedArgument Find(this CustomAttributeData self, string name)
        {
            return self.NamedArguments.First(T => T.MemberName == name);
        }

        public static object GetValue(this CustomAttributeData self, string name)
        {
            return self.NamedArguments.First(T => T.MemberName == name).TypedValue.Value;
        }

    }
}

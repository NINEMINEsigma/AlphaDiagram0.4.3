using System;
using System.Collections.Generic;
using UnityEngine;
using AD.Utility;
using System.Reflection;
using System.Linq;
using static AD.Utility.ReflectionExtension;
using Unity.VisualScripting;
using AD.BASE;

namespace AD.Experimental.GameEditor
{
    public class HierarchyBlock<T> : ISerializeHierarchyEditor where T : ICanSerializeOnCustomEditor
    {
        public HierarchyBlock(T that, string name)
        {
            this.that = that;
            this._Title = name;
        }

        public HierarchyBlock(T that, BindProperty<string> name)
        {
            this.that = that;
            this._p_Title = name;
            this._p_Title.AddListenerOnSet(TitleLinking);
        }

        public HierarchyBlock(T that, Func<string> name, Action<string> name_seter)
        {
            this.that = that;
            this._f_Title = name;
            this._f_Title_seter = name_seter;
        }

        public HierarchyBlock(T that, Func<string> name)
        {
            this.that = that;
            this._f_Title = name;
        }

        private void TitleLinking(string T)
        {
            foreach (var item in MatchItems)
            {
                item.SetTitle(T);
            }
        }

        ~HierarchyBlock()
        {
            this._p_Title?.RemoveListenerOnSet(TitleLinking);
        }

        protected readonly T that;

        public HierarchyItem MatchItem
        {
            get => MatchItems.Count == 0 ? null : MatchItems[0];
            set
            {
                if (MatchItems.Count == 0) MatchItems.Add(value);
                else MatchItems[0] = value;
                value.SetTitle(this.Title);
            }
        }
        public List<HierarchyItem> MatchItems { get; private set; } = new();

        public ICanSerializeOnCustomEditor MatchTarget { get => that; }

        public bool IsOpenListView { get; set; }
        public int SerializeIndex { get => MatchTarget.SerializeIndex; set => throw new NotImplementedException(); }

        public virtual void OnSerialize(HierarchyItem MatchItem)
        {
            MatchItem.SetTitle(Title);
        }

        private BindProperty<string> _p_Title = null;
        private string _Title = null;
        private Func<string> _f_Title = null;
        private Action<string> _f_Title_seter = null;
        public virtual string Title
        {
            get
            {
                if (_p_Title != null) return _p_Title.Get();
                else if (_f_Title != null) return _f_Title();
                else return _Title;
            }
            set
            {
                if (_p_Title != null) _p_Title.Set(value);
                else if (_f_Title != null) _f_Title_seter?.Invoke(value);
                else _Title = value;
                foreach (var item in MatchItems)
                {
                    item.Refresh();
                }
            }
        }
    }

    public class PropertiesBlock<T> : ISerializePropertiesEditor where T : class, ICanSerializeOnCustomEditor
    {
        public static readonly Type CanSerialize = typeof(ADSerializeAttribute);
        public static readonly Type CanSetupAction = typeof(ADActionButtonAttribute);
        public static readonly BindingFlags bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        protected PropertiesBlock(string layer, int index = 0)
        {
            SetUpSelf(this as T, layer, index);
        }
        public PropertiesBlock(T that, string layer, int index = 0)
        {
            SetUpSelf(that, layer, index);
        }

        private void SetUpSelf(T that, string layer, int index = 0)
        {
            this.that = that;
            this.SerializeIndex = index;
            this.LayerTitle = layer;

            GetSerializableFields(that.GetType()).GetSubList(T =>
            {
                foreach (CustomAttributeData cad in T.GetCustomAttributesData())
                {
                    if (cad.AttributeType == CanSerialize)
                    {
                        var cat = new ADSerializeEntry();
                        if (SetupEntry(cat, cad.NamedArguments))
                        {
                            fields[cat] = T;
                            maxLine.Add(cat);
                        }
                        break;
                    }
                }
                return true;
            });
            GetSerializableProperties(that.GetType()).GetSubList(T =>
            {
                foreach (CustomAttributeData cad in T.GetCustomAttributesData())
                {
                    if (cad.AttributeType == CanSerialize)
                    {
                        var cat = new ADSerializeEntry();
                        if (SetupEntry(cat, cad.NamedArguments))
                        {
                            properties[cat] = T;
                            maxLine.Add(cat);
                        }
                        break;
                    }
                }
                return true;
            });
            GetSerializableMethods(that.GetType()).GetSubList(T =>
            {
                foreach (CustomAttributeData cad in T.GetCustomAttributesData())
                {
                    if (cad.AttributeType == CanSetupAction)
                    {
                        var cat = new ADSerializeEntry();
                        if (SetupEntry(cat, cad.NamedArguments))
                        {
                            methods[cat] = T;
                            maxLine.Add(cat);
                        }
                        break;
                    }
                }
                return true;
            });

            maxLine.Sort((T, P) => T.index.CompareTo(P.index));

            bool SetupEntry(ADSerializeEntry cat, IList<CustomAttributeNamedArgument> temp_catas)
            {
                bool isTure = false;
                foreach (var cata in temp_catas)
                {
                    switch (cata.MemberName)
                    {
                        case "layer":
                            {
                                cat.layer = (string)cata.TypedValue.Value;
                                if (cat.layer != layer) return false;
                                else isTure = true;
                            }
                            break;
                        case "index":
                            {
                                cat.index = (int)cata.TypedValue.Value;
                            }
                            break;
                        case "message":
                            {
                                cat.message = (string)cata.TypedValue.Value;
                            }
                            break;
                        case "methodName":
                            {
                                cat.methodName = (string)cata.TypedValue.Value;
                            }
                            break;
                    }
                }
                return isTure;
            }
        }

        public class ADSerializeEntry
        {
            public string layer;
            public int index;
            public string message;
            private string _methodName;
            public string methodName
            {
                get => (string.IsNullOrEmpty(_methodName)) ? ADSerializeAttribute.DefaultKey : _methodName; set => _methodName = value;
            }
        }

        private readonly Dictionary<ADSerializeEntry, FieldInfo> fields = new();
        private readonly Dictionary<ADSerializeEntry, PropertyInfo> properties = new();
        private readonly Dictionary<ADSerializeEntry, MethodInfo> methods = new();
        private readonly List<ADSerializeEntry> maxLine = new();

        protected T that { get; private set; }

        public string LayerTitle { get; private set; }

        public PropertiesItem MatchItem { get; set; }

        public ICanSerializeOnCustomEditor MatchTarget { get => that; }

        public int SerializeIndex { get; set; }
        public bool IsDirty { get; set; }

        public void OnSerialize()
        {
            PropertiesLayout.SetUpPropertiesLayout(this);

            MatchItem.SetTitle(LayerTitle);
            HowSerialize();

            PropertiesLayout.ApplyPropertiesLayout();
        }

        protected virtual void HowSerialize()
        {
            foreach (var item in maxLine)
            {
                if (fields.TryGetValue(item, out var field)) DoGUI_Field(item, field);
                else if (properties.TryGetValue(item, out var _proerty)) DoGUI_Property(item, _proerty);
                else if (methods.TryGetValue(item, out var method)) PropertiesLayout.ModernUIButton(item.methodName, item.message, () => method.Invoke(that, null));
            }
        }

        public static List<FieldInfo> GetSerializableFields(Type type,
                                                            List<FieldInfo> serializableFields = null,
                                                            string[] memberNames = null)
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

                // If the field is private, only serialize it if it's explicitly marked as ADSerialize.
                if (!AttributeIsDefined(field, CanSerialize))
                    continue;

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
                if (fieldName.StartsWith(memberFieldPrefix) && field.DeclaringType.Namespace != null && field.DeclaringType.Namespace.Contains("UnityEngine"))
                    continue;

                serializableFields.Add(field);
            }

            var baseType = BaseType(type);
            if (baseType != null && baseType != typeof(System.Object) && baseType != typeof(UnityEngine.Object))
                GetSerializableFields(BaseType(type), serializableFields, memberNames);

            return serializableFields;
        }

        public static List<PropertyInfo> GetSerializableProperties(Type type,
                                                                   List<PropertyInfo> serializableProperties = null,
                                                                   string[] memberNames = null)
        {
            bool isComponent = IsAssignableFrom(typeof(UnityEngine.Component), type);

            var properties = type.GetProperties(bindings);

            serializableProperties ??= new List<PropertyInfo>();

            foreach (var p in properties)
            {
                var propertyName = p.Name;

                if (excludedPropertyNames.Contains(propertyName))
                    continue;

                // If a members array was provided as a parameter, only include the property if it's in the array.
                if (memberNames != null)
                    if (!memberNames.Contains(propertyName))
                        continue;

                // If safe serialization is enabled, only get methods which are explicitly marked as CanSerialize.
                if (!AttributeIsDefined(p, CanSerialize))
                    continue;

                var propertyType = p.PropertyType;

                // Don't store methods whose type is the same as the class the property is housed in unless it's stored by reference (to prevent cyclic references)
                if (propertyType == type && !IsAssignableFrom(typeof(UnityEngine.Object), propertyType))
                    continue;

                if (!p.CanRead || !p.CanWrite)
                    continue;

                // Only support methods with indexing if they're an array.
                if (p.GetIndexParameters().Length != 0 && !propertyType.IsArray)
                    continue;

                // Check that the type of the property is one which we can serialize.
                if (!TypeIsSerializable(propertyType))
                    continue;

                // Ignore certain methods on components.
                if (isComponent)
                {
                    // Ignore methods which are accessors for GameObject fields.
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
                GetSerializableProperties(baseType, serializableProperties, memberNames);

            return serializableProperties;
        }

        public static List<MethodInfo> GetSerializableMethods(Type type,
                                                                   List<MethodInfo> serializableMethods = null,
                                                                   string[] memberNames = null)
        {
            var methods = type.GetMethods(bindings);

            serializableMethods ??= new List<MethodInfo>();

            foreach (var m in methods)
            {
                var methodName = m.Name;

                // If a members array was provided as a parameter, only include the method if it's in the array.
                if (memberNames != null)
                    if (!memberNames.Contains(methodName))
                        continue;

                // If safe serialization is enabled, only get methods which are explicitly marked as CanSetupAction.
                if (!AttributeIsDefined(m, CanSetupAction))
                    continue;

                var argTypes = m.GetGenericArguments();

                if (argTypes.Length > 0)
                    continue;

                serializableMethods.Add(m);
            }

            var baseType = BaseType(type);
            if (baseType != null && baseType != typeof(System.Object))
                GetSerializableMethods(baseType, serializableMethods, memberNames);

            return serializableMethods;
        }

        private void DoGUI_Field(ADSerializeEntry entry, FieldInfo field)
        {
            Type type = field.FieldType;
            bool isDefaultMethodName = entry.methodName == ADSerializeAttribute.DefaultKey;
            if (type == typeof(bool))
            {
                if (isDefaultMethodName)
                    PropertiesLayout.ModernUISwitch(field.Name, (bool)field.GetValue(that), entry.message, T => field.SetValue(that, T));
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(char))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(field.Name, entry.message);
                    var cat = PropertiesLayout.InputField(((char)field.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        field.SetValue(that, T[0]);
                        cat.SetTextWithoutNotify(T[..1]);
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(double))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(field.Name, entry.message);
                    var cat = PropertiesLayout.InputField(((double)field.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (double.TryParse(T, out double value))
                        {
                            field.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((double)field.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(float))
            {
                if (entry.message[0] == '$')
                {
                    PropertiesLayout.BeginHorizontal();
                    var str = entry.message[1..].Split(',');
                    float min = float.Parse(str[0]), max = float.Parse(str[1]);
                    PropertiesLayout.Label(field.Name, str[2]);
                    var cat = PropertiesLayout.Slider(min, max, (float)field.GetValue(that), true, str[2], T =>
                    {
                        field.SetValue(that, T);
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(field.Name, entry.message);
                    var cat = PropertiesLayout.InputField(((float)field.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (float.TryParse(T, out float value))
                        {
                            field.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((float)field.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(int))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(field.Name + "(integer)", entry.message);
                    var cat = PropertiesLayout.InputField(((int)field.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (int.TryParse(T, out int value))
                        {
                            field.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((int)field.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(uint))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(field.Name + "(unsigned integer)", entry.message);
                    var cat = PropertiesLayout.InputField(((uint)field.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (uint.TryParse(T, out uint value))
                        {
                            field.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((uint)field.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(long))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(field.Name + "(integer)", entry.message);
                    var cat = PropertiesLayout.InputField(((long)field.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (long.TryParse(T, out long value))
                        {
                            field.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((long)field.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(ulong))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(field.Name + "(unsigned integer)", entry.message);
                    var cat = PropertiesLayout.InputField(((ulong)field.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (ulong.TryParse(T, out ulong value))
                        {
                            field.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((ulong)field.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(short))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(field.Name + "(integer)", entry.message);
                    var cat = PropertiesLayout.InputField(((short)field.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (short.TryParse(T, out short value))
                        {
                            field.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((short)field.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(ushort))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(field.Name + "(unsigh integer)", entry.message);
                    var cat = PropertiesLayout.InputField(((ushort)field.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (ushort.TryParse(T, out ushort value))
                        {
                            field.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((ushort)field.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(string))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(field.Name, entry.message);
                    var cat = PropertiesLayout.InputField((string)field.GetValue(that), entry.message);
                    cat.AddListener(T =>
                    {
                        field.SetValue(that, T);
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(Vector2))
            {
                if (isDefaultMethodName)
                {
                    /*
                    PropertiesLayout.Label(field.Name, entry.message);
                    var value = (Vector2)field.GetValue(that);

                    PropertiesLayout.BeginHorizontal();
                    var catX = PropertiesLayout.InputField(value.x.ToString(), entry.message);
                    var catY = PropertiesLayout.InputField(value.y.ToString(), entry.message);
                    PropertiesLayout.EndHorizontal();

                    catX.AddListener(T =>
                    {
                        var value = (Vector2)field.GetValue(that);
                        if (float.TryParse(T, out float nx))
                        {
                            value.x = nx;
                            field.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.x.ToString());
                    });
                    catY.AddListener(T =>
                    {
                        var value = (Vector2)field.GetValue(that);
                        if (float.TryParse(T, out float ny))
                        {
                            value.y = ny;
                            field.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.y.ToString());
                    });
                    */
                    var value = (Vector2)field.GetValue(that);
                    PropertiesLayout.Vector2(field.Name, value, entry.message, T => field.SetValue(that, T));
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }

            }
            else if (type == typeof(Vector3))
            {
                if (isDefaultMethodName)
                {
                    /*
                    PropertiesLayout.Label(field.Name, entry.message);
                    var value = (Vector3)field.GetValue(that);

                    PropertiesLayout.BeginHorizontal();
                    var catX = PropertiesLayout.InputField(value.x.ToString(), entry.message);
                    var catY = PropertiesLayout.InputField(value.y.ToString(), entry.message);
                    var catZ = PropertiesLayout.InputField(value.z.ToString(), entry.message);
                    PropertiesLayout.EndHorizontal();

                    catX.AddListener(T =>
                    {
                        var value = (Vector3)field.GetValue(that);
                        if (float.TryParse(T, out float nx))
                        {
                            value.x = nx;
                            field.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.x.ToString());
                    });
                    catY.AddListener(T =>
                    {
                        var value = (Vector3)field.GetValue(that);
                        if (float.TryParse(T, out float ny))
                        {
                            value.y = ny;
                            field.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.y.ToString());
                    });
                    catZ.AddListener(T =>
                    {
                        var value = (Vector3)field.GetValue(that);
                        if (float.TryParse(T, out float nz))
                        {
                            value.z = nz;
                            field.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.z.ToString());
                    });
                    */
                    var value = (Vector3)field.GetValue(that);
                    PropertiesLayout.Vector3(field.Name, value, entry.message, T => field.SetValue(that, T));
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(Vector4))
            {
                if (isDefaultMethodName)
                {
                    /*
                    PropertiesLayout.Label(field.Name, entry.message);
                    var value = (Vector4)field.GetValue(that);

                    PropertiesLayout.BeginHorizontal();
                    var catX = PropertiesLayout.InputField(value.x.ToString(), entry.message);
                    var catY = PropertiesLayout.InputField(value.y.ToString(), entry.message);
                    PropertiesLayout.EndHorizontal();
                    PropertiesLayout.BeginHorizontal();
                    var catZ = PropertiesLayout.InputField(value.z.ToString(), entry.message);
                    var catW = PropertiesLayout.InputField(value.w.ToString(), entry.message);
                    PropertiesLayout.EndHorizontal();

                    catX.AddListener(T =>
                    {
                        var value = (Vector4)field.GetValue(that);
                        if (float.TryParse(T, out float nx))
                        {
                            value.x = nx;
                            field.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.x.ToString());
                    });
                    catY.AddListener(T =>
                    {
                        var value = (Vector4)field.GetValue(that);
                        if (float.TryParse(T, out float ny))
                        {
                            value.y = ny;
                            field.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.y.ToString());
                    });
                    catZ.AddListener(T =>
                    {
                        var value = (Vector4)field.GetValue(that);
                        if (float.TryParse(T, out float nz))
                        {
                            value.z = nz;
                            field.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.z.ToString());
                    });
                    catW.AddListener(T =>
                    {
                        var value = (Vector4)field.GetValue(that);
                        if (float.TryParse(T, out float nw))
                        {
                            value.w = nw;
                            field.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.w.ToString());
                    });
                    */
                    var value = (Vector4)field.GetValue(that);
                    PropertiesLayout.Vector4(field.Name, value, entry.message, T => field.SetValue(that, T));
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(Color))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.ColorPanel(field.Name, (Color)field.GetValue(that), entry.message, T =>
                {
                    field.SetValue(that, T);
                });
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(Texture2D))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.Label(field.Name, entry.message);
                    PropertiesLayout.RawImage((Texture2D)field.GetValue(that), entry.message);
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(Sprite))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.Image(field.Name, entry.message).CurrentImagePair = new() { SpriteSource = (Sprite)field.GetValue(that) };
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(Transform))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.EndHorizontal();
                    PropertiesLayout.Label(field.Name, field.Name);
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Transform((Transform)field.GetValue(that));
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (isDefaultMethodName) DoGUI_Field_Extension(entry, field);
            else that.RunMethodByName(entry.methodName, bindings, null);
        }

        private void DoGUI_Property(ADSerializeEntry entry, PropertyInfo property)
        {
            Type type = property.PropertyType;
            bool isDefaultMethodName = entry.methodName == ADSerializeAttribute.DefaultKey;
            if (type == typeof(bool))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.ModernUISwitch(property.Name, (bool)property.GetValue(that), entry.message, T => property.SetValue(that, T));
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(char))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(property.Name, entry.message);
                    var cat = PropertiesLayout.InputField(((char)property.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        property.SetValue(that, T[0]);
                        cat.SetTextWithoutNotify(T[..1]);
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(double))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(property.Name, entry.message);
                    var cat = PropertiesLayout.InputField(((double)property.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (double.TryParse(T, out double value))
                        {
                            property.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((double)property.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(float))
            {
                if (entry.message[0] == '$')
                {
                    PropertiesLayout.BeginHorizontal();
                    var str = entry.message[1..].Split(',');
                    float min = float.Parse(str[0]), max = float.Parse(str[1]);
                    PropertiesLayout.Label(property.Name, str[2]);
                    var cat = PropertiesLayout.Slider(min, max, (float)property.GetValue(that), true, str[2], T =>
                    {
                        property.SetValue(that, T);
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(property.Name, entry.message);
                    var cat = PropertiesLayout.InputField(((float)property.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (float.TryParse(T, out float value))
                        {
                            property.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((float)property.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(int))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(property.Name + "(integer)", entry.message);
                    var cat = PropertiesLayout.InputField(((int)property.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (int.TryParse(T, out int value))
                        {
                            property.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((int)property.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(uint))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(property.Name + "(unsigned integer)", entry.message);
                    var cat = PropertiesLayout.InputField(((uint)property.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (uint.TryParse(T, out uint value))
                        {
                            property.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((uint)property.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(long))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(property.Name + "(integer)", entry.message);
                    var cat = PropertiesLayout.InputField(((long)property.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (long.TryParse(T, out long value))
                        {
                            property.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((long)property.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(ulong))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(property.Name + "(unsigned integer)", entry.message);
                    var cat = PropertiesLayout.InputField(((ulong)property.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (ulong.TryParse(T, out ulong value))
                        {
                            property.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((ulong)property.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(short))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(property.Name + "(integer)", entry.message);
                    var cat = PropertiesLayout.InputField(((short)property.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (short.TryParse(T, out short value))
                        {
                            property.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((short)property.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(ushort))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(property.Name + "(unsigh integer)", entry.message);
                    var cat = PropertiesLayout.InputField(((ushort)property.GetValue(that)).ToString(), entry.message);
                    cat.AddListener(T =>
                    {
                        if (ushort.TryParse(T, out ushort value))
                        {
                            property.SetValue(that, value);
                        }
                        else
                        {
                            cat.SetTextWithoutNotify(((ushort)property.GetValue(that)).ToString());
                        }
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(string))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Label(property.Name, entry.message);
                    var cat = PropertiesLayout.InputField((string)property.GetValue(that), entry.message);
                    cat.AddListener(T =>
                    {
                        property.SetValue(that, T);
                    });
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(Vector2))
            {
                if (isDefaultMethodName)
                {
                    /*
                    PropertiesLayout.Label(property.Name, entry.message);
                    var value = (Vector2)property.GetValue(that);

                    PropertiesLayout.BeginHorizontal();
                    var catX = PropertiesLayout.InputField(value.x.ToString(), entry.message);
                    var catY = PropertiesLayout.InputField(value.y.ToString(), entry.message);
                    PropertiesLayout.EndHorizontal();

                    catX.AddListener(T =>
                    {
                        var value = (Vector2)property.GetValue(that);
                        if (float.TryParse(T, out float nx))
                        {
                            value.x = nx;
                            property.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.x.ToString());
                    });
                    catY.AddListener(T =>
                    {
                        var value = (Vector2)property.GetValue(that);
                        if (float.TryParse(T, out float ny))
                        {
                            value.y = ny;
                            property.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.y.ToString());
                    });
                    */
                    var value = (Vector2)property.GetValue(that);
                    PropertiesLayout.Vector2(property.Name, value, entry.message, T => property.SetValue(that, T));
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(Vector3))
            {
                if (isDefaultMethodName)
                {
                    /*
                    PropertiesLayout.Label(property.Name, entry.message);
                    var value = (Vector3)property.GetValue(that);

                    PropertiesLayout.BeginHorizontal();
                    var catX = PropertiesLayout.InputField(value.x.ToString(), entry.message);
                    var catY = PropertiesLayout.InputField(value.y.ToString(), entry.message);
                    var catZ = PropertiesLayout.InputField(value.z.ToString(), entry.message);
                    PropertiesLayout.EndHorizontal();

                    catX.AddListener(T =>
                    {
                        var value = (Vector3)property.GetValue(that);
                        if (float.TryParse(T, out float nx))
                        {
                            value.x = nx;
                            property.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.x.ToString());
                    });
                    catY.AddListener(T =>
                    {
                        var value = (Vector3)property.GetValue(that);
                        if (float.TryParse(T, out float ny))
                        {
                            value.y = ny;
                            property.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.y.ToString());
                    });
                    catZ.AddListener(T =>
                    {
                        var value = (Vector3)property.GetValue(that);
                        if (float.TryParse(T, out float nz))
                        {
                            value.z = nz;
                            property.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.z.ToString());
                    });
                    */
                    var value = (Vector3)property.GetValue(that);
                    PropertiesLayout.Vector3(property.Name, value, entry.message, T => property.SetValue(that, T));
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(Vector4))
            {
                if (isDefaultMethodName)
                {
                    /*
                    PropertiesLayout.Label(property.Name, entry.message);
                    var value = (Vector4)property.GetValue(that);

                    PropertiesLayout.BeginHorizontal();
                    var catX = PropertiesLayout.InputField(value.x.ToString(), entry.message);
                    var catY = PropertiesLayout.InputField(value.y.ToString(), entry.message);
                    PropertiesLayout.EndHorizontal();
                    PropertiesLayout.BeginHorizontal();
                    var catZ = PropertiesLayout.InputField(value.z.ToString(), entry.message);
                    var catW = PropertiesLayout.InputField(value.w.ToString(), entry.message);
                    PropertiesLayout.EndHorizontal();

                    catX.AddListener(T =>
                    {
                        var value = (Vector4)property.GetValue(that);
                        if (float.TryParse(T, out float nx))
                        {
                            value.x = nx;
                            property.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.x.ToString());
                    });
                    catY.AddListener(T =>
                    {
                        var value = (Vector4)property.GetValue(that);
                        if (float.TryParse(T, out float ny))
                        {
                            value.y = ny;
                            property.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.y.ToString());
                    });
                    catZ.AddListener(T =>
                    {
                        var value = (Vector4)property.GetValue(that);
                        if (float.TryParse(T, out float nz))
                        {
                            value.z = nz;
                            property.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.z.ToString());
                    });
                    catW.AddListener(T =>
                    {
                        var value = (Vector4)property.GetValue(that);
                        if (float.TryParse(T, out float nw))
                        {
                            value.w = nw;
                            property.SetValue(that, value);
                        }
                        else catX.SetTextWithoutNotify(value.w.ToString());
                    });
                    */
                    var value = (Vector4)property.GetValue(that);
                    PropertiesLayout.Vector4(property.Name, value, entry.message, T => property.SetValue(that, T));
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(Color))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.ColorPanel(property.Name, (Color)property.GetValue(that), entry.message, T =>
                {
                    property.SetValue(that, T);
                });
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(Texture2D))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.Label(property.Name, entry.message);
                    PropertiesLayout.RawImage((Texture2D)property.GetValue(that), entry.message);
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(Sprite))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Image(property.Name, entry.message).CurrentImagePair = new() { SpriteSource = (Sprite)property.GetValue(that) };
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (type == typeof(Transform))
            {
                if (isDefaultMethodName)
                {
                    PropertiesLayout.EndHorizontal();
                    PropertiesLayout.Label(property.Name, property.Name);
                    PropertiesLayout.BeginHorizontal();
                    PropertiesLayout.Transform((Transform)property.GetValue(that));
                    PropertiesLayout.EndHorizontal();
                }
                else
                {
                    that.RunMethodByName(entry.methodName, bindings, null);
                }
            }
            else if (isDefaultMethodName) DoGUI_Property_Extension(entry, property);
            else that.RunMethodByName(entry.methodName, bindings, null);
        }

        protected virtual void DoGUI_Field_Extension(ADSerializeEntry entry, FieldInfo field)
        {
            Debug.LogWarning("Cannt Generate This Type's UIComponent By Auto");
        }
        protected virtual void DoGUI_Property_Extension(ADSerializeEntry entry, PropertyInfo property)
        {
            Debug.LogWarning("Cannt Generate This Type's UIComponent By Auto");
        }

    }

    public class UnSafePropertiesBlock<T> : ISerializePropertiesEditor where T : class, ICanSerializeOnCustomEditor
    {
        public static readonly Type CanSetupAction = typeof(ADUnSafeActionButtonAttribute);
        public static readonly BindingFlags bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        protected UnSafePropertiesBlock(string layer, int index = 0)
        {
            SetUpSelf(this as T, layer, index);
        }
        public UnSafePropertiesBlock(T that, string layer, int index = 0)
        {
            SetUpSelf(that, layer, index);
        }

        private void SetUpSelf(T that, string layer, int index = 0)
        {
            this.that = that;
            this.SerializeIndex = index;
            this.LayerTitle = layer;

            fields = GetSerializableFields(that.GetType());
            propertys = GetSerializableProperties(that.GetType());
            methods = GetSerializableMethods(that.GetType());
        }

        private List<FieldInfo> fields = new();
        private List<PropertyInfo> propertys = new();
        private List<MethodInfo> methods = new();

        protected T that { get; private set; }

        public string LayerTitle { get; private set; }

        public PropertiesItem MatchItem { get; set; }

        public ICanSerializeOnCustomEditor MatchTarget { get => that; }

        public int SerializeIndex { get; set; }
        public bool IsDirty { get; set; }

        public void OnSerialize()
        {
            PropertiesLayout.SetUpPropertiesLayout(this);

            MatchItem.SetTitle(LayerTitle);
            HowSerialize();

            PropertiesLayout.ApplyPropertiesLayout();
        }

        protected virtual void HowSerialize()
        {
            if (that is MonoBehaviour mono) PropertiesLayout.Transform(mono.transform);
            PropertiesLayout.Space(1);
            PropertiesLayout.Title("Field", "Field");
            foreach (var item in fields)
            {
                DoGUI_Field(item);
            }
            PropertiesLayout.Title("Properties", "Properties");
            foreach (var item in propertys)
            {
                DoGUI_Property(item);
            }
            PropertiesLayout.Title("Properties", "Properties");
            foreach (var item in methods)
            {
                PropertiesLayout.ModernUIButton(item.Name, item.Name, () => item.Invoke(that, null));
            }
        }

        public static List<FieldInfo> GetSerializableFields(Type type,
                                                            List<FieldInfo> serializableFields = null,
                                                            string[] memberNames = null)
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

                // If the field is private, only serialize it if it's explicitly marked as ADSerialize.
                //if (!AttributeIsDefined(field, CanSerialize))
                //    continue;

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
                if (fieldName.StartsWith(memberFieldPrefix) && field.DeclaringType.Namespace != null && field.DeclaringType.Namespace.Contains("UnityEngine"))
                    continue;

                serializableFields.Add(field);
            }

            var baseType = BaseType(type);
            if (baseType != null && baseType != typeof(System.Object) && baseType != typeof(UnityEngine.Object))
                GetSerializableFields(BaseType(type), serializableFields, memberNames);

            return serializableFields;
        }

        public static List<PropertyInfo> GetSerializableProperties(Type type,
                                                                   List<PropertyInfo> serializableProperties = null,
                                                                   string[] memberNames = null)
        {
            bool isComponent = IsAssignableFrom(typeof(UnityEngine.Component), type);

            var properties = type.GetProperties(bindings);

            serializableProperties ??= new List<PropertyInfo>();

            foreach (var p in properties)
            {
                var propertyName = p.Name;

                if (excludedPropertyNames.Contains(propertyName))
                    continue;

                // If a members array was provided as a parameter, only include the property if it's in the array.
                if (memberNames != null)
                    if (!memberNames.Contains(propertyName))
                        continue;

                // If safe serialization is enabled, only get methods which are explicitly marked as CanSerialize.
                //if (!AttributeIsDefined(p, CanSerialize))
                //    continue;

                var propertyType = p.PropertyType;

                // Don't store methods whose type is the same as the class the property is housed in unless it's stored by reference (to prevent cyclic references)
                if (propertyType == type && !IsAssignableFrom(typeof(UnityEngine.Object), propertyType))
                    continue;

                if (!p.CanRead || !p.CanWrite)
                    continue;

                // Only support methods with indexing if they're an array.
                if (p.GetIndexParameters().Length != 0 && !propertyType.IsArray)
                    continue;

                // Check that the type of the property is one which we can serialize.
                if (!TypeIsSerializable(propertyType))
                    continue;

                // Ignore certain methods on components.
                if (isComponent)
                {
                    // Ignore methods which are accessors for GameObject fields.
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
                GetSerializableProperties(baseType, serializableProperties, memberNames);

            return serializableProperties;
        }

        public static List<MethodInfo> GetSerializableMethods(Type type,
                                                                   List<MethodInfo> serializableMethods = null,
                                                                   string[] memberNames = null)
        {
            var methods = type.GetMethods(bindings);

            serializableMethods ??= new List<MethodInfo>();

            foreach (var m in methods)
            {
                var methodName = m.Name;

                // If a members array was provided as a parameter, only include the method if it's in the array.
                if (memberNames != null)
                    if (!memberNames.Contains(methodName))
                        continue;

                // If safe serialization is enabled, only get methods which are explicitly marked as CanSetupAction.
                if (!AttributeIsDefined(m, CanSetupAction))
                    continue;

                var argTypes = m.GetGenericArguments();

                if (argTypes.Length > 0)
                    continue;

                serializableMethods.Add(m);
            }

            var baseType = BaseType(type);
            if (baseType != null && baseType != typeof(System.Object))
                GetSerializableMethods(baseType, serializableMethods, memberNames);

            return serializableMethods;
        }

        private void DoGUI_Field(FieldInfo field)
        {
            Type type = field.FieldType;
            if (type == typeof(bool))
            {
                PropertiesLayout.ModernUISwitch(field.Name, (bool)field.GetValue(that), field.Name, T => field.SetValue(that, T));
            }
            else if (type == typeof(char))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name, field.Name);
                var cat = PropertiesLayout.InputField(((char)field.GetValue(that)).ToString(), field.Name);
                cat.AddListener(T =>
                {
                    field.SetValue(that, T[0]);
                    cat.SetTextWithoutNotify(T[..1]);
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(double))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name, field.Name);
                var cat = PropertiesLayout.InputField(((double)field.GetValue(that)).ToString(), field.Name);
                cat.AddListener(T =>
                {
                    if (double.TryParse(T, out double value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((double)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(float))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name, field.Name);
                var cat = PropertiesLayout.InputField(((float)field.GetValue(that)).ToString(), field.Name);
                cat.AddListener(T =>
                {
                    if (float.TryParse(T, out float value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((float)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(int))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name + "(integer)", field.Name + "(integer)");
                var cat = PropertiesLayout.InputField(((int)field.GetValue(that)).ToString(), field.Name + "(integer)");
                cat.AddListener(T =>
                {
                    if (int.TryParse(T, out int value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((int)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(uint))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name + "(unsigned integer)", field.Name + "(unsigned integer)");
                var cat = PropertiesLayout.InputField(((uint)field.GetValue(that)).ToString(), field.Name + "(unsigned integer)");
                cat.AddListener(T =>
                {
                    if (uint.TryParse(T, out uint value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((uint)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(long))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name + "(long)", field.Name + "(long)");
                var cat = PropertiesLayout.InputField(((long)field.GetValue(that)).ToString(), field.Name + "(long)");
                cat.AddListener(T =>
                {
                    if (long.TryParse(T, out long value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((long)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(ulong))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name + "(unsigned long)", field.Name + "(unsigned long)");
                var cat = PropertiesLayout.InputField(((ulong)field.GetValue(that)).ToString(), field.Name + "(unsigned long)");
                cat.AddListener(T =>
                {
                    if (ulong.TryParse(T, out ulong value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((ulong)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(short))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name + "(short)", field.Name + "(short)");
                var cat = PropertiesLayout.InputField(((short)field.GetValue(that)).ToString(), field.Name + "(short)");
                cat.AddListener(T =>
                {
                    if (short.TryParse(T, out short value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((short)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(ushort))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name + "(unsigh short)", field.Name + "(unsigh short)");
                var cat = PropertiesLayout.InputField(((ushort)field.GetValue(that)).ToString(), field.Name + "(unsigh short)");
                cat.AddListener(T =>
                {
                    if (ushort.TryParse(T, out ushort value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((ushort)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(string))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name, field.Name);
                var cat = PropertiesLayout.InputField((string)field.GetValue(that), field.Name);
                cat.AddListener(T =>
                {
                    field.SetValue(that, T);
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(Vector2))
            {
                var value = (Vector2)field.GetValue(that);
                PropertiesLayout.Vector2(field.Name, value, field.Name, T => field.SetValue(that, T));
            }
            else if (type == typeof(Vector3))
            {
                var value = (Vector3)field.GetValue(that);
                PropertiesLayout.Vector3(field.Name, value, field.Name, T => field.SetValue(that, T));
            }
            else if (type == typeof(Vector4))
            {
                var value = (Vector4)field.GetValue(that);
                PropertiesLayout.Vector4(field.Name, value, field.Name, T => field.SetValue(that, T));
            }
            else if (type == typeof(Color))
            {
                PropertiesLayout.ColorPanel(field.Name, (Color)field.GetValue(that), field.Name, T =>
                    {
                        field.SetValue(that, T);
                    });
            }
            else if (type == typeof(Texture2D))
            {
                PropertiesLayout.Label(field.Name, field.Name);
                PropertiesLayout.RawImage((Texture2D)field.GetValue(that), field.Name);
            }
            else if (type == typeof(Sprite))
            {
                PropertiesLayout.Image(field.Name, field.Name).CurrentImagePair = new() { SpriteSource = (Sprite)field.GetValue(that) };
            }
            else if (type == typeof(Transform))
            {
                PropertiesLayout.Label(field.Name, field.Name);
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Transform((Transform)field.GetValue(that));
                PropertiesLayout.EndHorizontal();
            }
            else DoGUI_Field_Extension(field);
        }

        private void DoGUI_Property(PropertyInfo property)
        {
            Type type = property.PropertyType;
            if (type == typeof(bool))
            {
                PropertiesLayout.ModernUISwitch(property.Name, (bool)property.GetValue(that), property.Name, T => property.SetValue(that, T));
            }
            else if (type == typeof(char))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name, property.Name);
                var cat = PropertiesLayout.InputField(((char)property.GetValue(that)).ToString(), property.Name);
                cat.AddListener(T =>
                {
                    property.SetValue(that, T[0]);
                    cat.SetTextWithoutNotify(T[..1]);
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(double))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name, property.Name);
                var cat = PropertiesLayout.InputField(((double)property.GetValue(that)).ToString(), property.Name);
                cat.AddListener(T =>
                {
                    if (double.TryParse(T, out double value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((double)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(float))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name, property.Name);
                var cat = PropertiesLayout.InputField(((float)property.GetValue(that)).ToString(), property.Name);
                cat.AddListener(T =>
                {
                    if (float.TryParse(T, out float value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((float)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(int))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name + "(integer)", property.Name + "(integer)");
                var cat = PropertiesLayout.InputField(((int)property.GetValue(that)).ToString(), property.Name + "(integer)");
                cat.AddListener(T =>
                {
                    if (int.TryParse(T, out int value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((int)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(uint))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name + "(unsigned integer)", property.Name + "(unsigned integer)");
                var cat = PropertiesLayout.InputField(((uint)property.GetValue(that)).ToString(), property.Name + "(unsigned integer)");
                cat.AddListener(T =>
                {
                    if (uint.TryParse(T, out uint value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((uint)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(long))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name + "(long)", property.Name + "(long)");
                var cat = PropertiesLayout.InputField(((long)property.GetValue(that)).ToString(), property.Name + "(long)");
                cat.AddListener(T =>
                {
                    if (long.TryParse(T, out long value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((long)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(ulong))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name + "(unsigned long)", property.Name + "(unsigned long)");
                var cat = PropertiesLayout.InputField(((ulong)property.GetValue(that)).ToString(), property.Name + "(unsigned long)");
                cat.AddListener(T =>
                {
                    if (ulong.TryParse(T, out ulong value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((ulong)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(short))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name + "(short)", property.Name + "(short)");
                var cat = PropertiesLayout.InputField(((short)property.GetValue(that)).ToString(), property.Name + "(short)");
                cat.AddListener(T =>
                {
                    if (short.TryParse(T, out short value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((short)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(ushort))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name + "(unsigh short)", property.Name + "(unsigh short)");
                var cat = PropertiesLayout.InputField(((ushort)property.GetValue(that)).ToString(), property.Name + "(unsigh short)");
                cat.AddListener(T =>
                {
                    if (ushort.TryParse(T, out ushort value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((ushort)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(string))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name, property.Name);
                var cat = PropertiesLayout.InputField((string)property.GetValue(that), property.Name);
                cat.AddListener(T =>
                {
                    property.SetValue(that, T);
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(Vector2))
            {
                var value = (Vector2)property.GetValue(that);
                PropertiesLayout.Vector2(property.Name, value, property.Name, T => property.SetValue(that, T));
            }
            else if (type == typeof(Vector3))
            {
                var value = (Vector3)property.GetValue(that);
                PropertiesLayout.Vector3(property.Name, value, property.Name, T => property.SetValue(that, T));
            }
            else if (type == typeof(Vector4))
            {
                var value = (Vector4)property.GetValue(that);
                PropertiesLayout.Vector4(property.Name, value, property.Name, T => property.SetValue(that, T));
            }
            else if (type == typeof(Color))
            {
                PropertiesLayout.ColorPanel(property.Name, (Color)property.GetValue(that), property.Name, T =>
                {
                    property.SetValue(that, T);
                });
            }
            else if (type == typeof(Texture2D))
            {
                PropertiesLayout.Label(property.Name, property.Name);
                PropertiesLayout.RawImage((Texture2D)property.GetValue(that), property.Name);
            }
            else if (type == typeof(Sprite))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Image(property.Name, property.Name).CurrentImagePair = new() { SpriteSource = (Sprite)property.GetValue(that) };
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(Transform))
            {
                PropertiesLayout.EndHorizontal();
                PropertiesLayout.Label(property.Name, property.Name);
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Transform((Transform)property.GetValue(that));
                PropertiesLayout.EndHorizontal();
            }
            else DoGUI_Property_Extension(property);
        }

        protected virtual void DoGUI_Field_Extension(FieldInfo field)
        {
            Debug.LogWarning("Cannt Generate This Type's UIComponent By Auto");
        }
        protected virtual void DoGUI_Property_Extension(PropertyInfo property)
        {
            Debug.LogWarning("Cannt Generate This Type's UIComponent By Auto");
        }
    }

    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public sealed class ADSerializeAttribute : Attribute
    {
        public string layer;
        public int index;
        public string message = "---";
        public string methodName = DefaultKey;
        public const string DefaultKey = "Default";

        /*
        public ADSerializeAttribute(string layer, int index, string message)
        {
            this.layer = layer;
            this.index = index;
            this.message = message;
        }
        */
    }

    [System.AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class ADActionButtonAttribute : Attribute
    {
        public string layer;
        public int index;
        public string message;
        public string methodName;

        /*
        public ADActionButtonAttribute(string layer, int index, string message, string methodName)
        {
            this.layer = layer;
            this.index = index;
            this.message = message;
            this.methodName = methodName;
        }
        */
    }

    [System.AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class ADUnSafeActionButtonAttribute : Attribute { }

}

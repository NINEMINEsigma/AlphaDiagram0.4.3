using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using AD.Utility;
using System;
using System.Reflection;
using ES3Internal;
using static UnityEngine.Rendering.DebugUI;

namespace AD.Experimental.AssetScript
{
    public class ASWriter : ASStream
    {
        public object target;

        public ASWriter(object target, string path, string key = null, ASSeting setting = null) : base(path, key ?? AS.DefaultKeyName, setting ?? new())
        {
            this.target = target;
        }
        public ASWriter(string path, string key = null, ASSeting setting = null) : base(path, key ?? AS.DefaultKeyName, setting ?? new())
        {
        }

        public void SetPath(string path)
        {
            this.path = path;
        }

        public bool Write()
        {
            this.RefreshResult();
            return this.Result.ResultValue;
        }

        protected override void RefreshResult()
        {
            /*if (ADGlobalSystem.Input(path, out string outStrResult))
            {
                DataStr = outStrResult;
                if (SourceStrStreamLine.Length == 0 || SourceStrStreamLine[0] != key)
                {
                    SetError(null, ASResult.ErrorType.KeyNotMatch, "The key is not match", outStrResult);
                }
                else
                {
                    GetSerializableInfos();
                }
            }
            else
            {
                SetError(null, ASResult.ErrorType.FileNotFind, "Can not find this file", null);
            }*/
            List<string> dataStr = new();
            Dictionary<object, int> objectDic = new();
            try
            {
                WriteStart(dataStr);
                WriteFields(dataStr, objectDic);
                WriteEnd(dataStr);
            }
            catch (Exception ex)
            {
                SetError(ex, ASResult.ErrorType.ThrowError, ex.Message, null);
            }
        }

        private void WriteEnd(List<string> dataStr)
        {
            dataStr.Add("End");
        }

        private void WriteStart(List<string> dataStr)
        {
            dataStr.Add(this.key);
            dataStr.Add(this.type.FullName + "|" + this.type.Assembly.FullName);
            dataStr.Add("Start");
        }

        private void WriteFields(List<string> dataStr, Dictionary<object, int> objectDic)
        {
            foreach (var field in fields)
            {
                object current = field.GetValue(target);
                if(objectDic.TryAdd(current, dataStr.Count))
                {
                    //新对象，加入池中并生成它的数据
                    dataStr.Add(dataStr.Count.ToString() + "=" + FixFieldLine(field));
                    //将新生成的数据进行引用
                    dataStr.Add(dataStr.Count.ToString() + ":" + objectDic[current].ToString());
                }
                else
                {
                    //对象重复，对其引用
                    dataStr.Add(dataStr.Count.ToString() + ":" + objectDic[current].ToString());
                }
            }
        }

        private string FixFieldLine(FieldInfo fieldInfo)
        {
            object value = fieldInfo.GetValue(target);

            // Note that we have to check UnityEngine.Object types for null by casting it first, otherwise
            // it will always return false.
            if (value == null || (ReflectionExtension.IsAssignableFrom(typeof(UnityEngine.Object), value.GetType()) && value as UnityEngine.Object == null))
            {
                return "null";
            }

            // Deal with System.Objects
            if (type == typeof(object))
            {
                if (!type.isCollection && !type.isDictionary)
                {
                    StartWriteObject(null);
                    WriteType(valueType);

                    type.Write(value, this);

                    EndWriteObject(null);
                    return;
                }
            }
            throw new NotSupportedException("Types of " + type + " are not supported.");
        }
        private Type type;
        private List<FieldInfo> fields;
        private List<PropertyInfo> properties;

        protected void GetSerializableInfos()
        {
            type = target.GetType();
            fields = ReflectionExtension.GetSerializableFields(type);
            properties =ReflectionExtension.GetSerializableProperties(type);
        }
    }
}

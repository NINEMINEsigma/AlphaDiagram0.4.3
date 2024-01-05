using System;
using System.Collections.Generic;
using AD.BASE;

namespace AD.Utility
{
    [Serializable]
    public class ConventionErrorTagException : ADException
    {
        public ConventionErrorTagException() : base("Convention Tag is error") { }
    }
    [Serializable]
    public class ConventionTagNotFoundException : ADException
    {
        public ConventionTagNotFoundException() : base("Convention Tag cannt been found") { }
    }

    public static class Convention
    {
        public readonly static Type ConventionTag = typeof(IConvention_Tag);
        public readonly static Type ConventionPropertyTag = typeof(IConvention_Property_Tag);
        public readonly static Type ConventionFunctionTag = typeof(IConvention_Function_Tag);

        public static ConventionInfo GetAllConvention(this object self)
        {
            return new ConventionInfo(self);
        }

        #region Get

        public static bool TryGetValue<T>(object self, out object value, out Exception exception, bool safe = true) where T : IConvention_Property_Tag
        {
            return self.TryGetValueByPropertyTag<T>(out value, out exception, safe);
        }
        public static object GetValue<T>(object self, bool safe = true) where T : IConvention_Property_Tag
        {
            return self.GetValueByPropertyTag<T>(safe);
        }

        public static bool TryGetValue<T, _Value>(object self, out _Value value, out Exception exception, bool safe = true) where T : IConvention_Property_Tag
        {
            return self.TryGetValueByPropertyTag<T, _Value>(out value, out exception, safe);
        }
        public static _Value GetValue<T, _Value>(object self, bool safe = true) where T : IConvention_Property_Tag
        {
            return self.GetValueByPropertyTag<T, _Value>(safe);
        }

        public static bool TryGetValueByPropertyTag<T>(this object self, out object value, out Exception exception, bool safe = true) where T : IConvention_Property_Tag
        {
            exception = null;
            try
            {
                T that = (T)self;
                if (that is IProperty_Value_get property_c)
                {
                    value = property_c.Value;
                    return true;
                }
                else
                {
                    if (safe) throw new ConventionTagNotFoundException();
                    value = self.GetType().GetProperty("Value").GetValue(self);
                    return true;
                }
            }
            catch (Exception ex)
            {
                value = default;
                exception = ex;
                return false;
            }
        }
        public static object GetValueByPropertyTag<T>(this object self, bool safe = true) where T : IConvention_Property_Tag
        {
            T that = (T)self;
            if (that is IProperty_Value_get property_c)
            {
                return property_c.Value;
            }
            else
            {
                if (safe) throw new ConventionTagNotFoundException();
                return self.GetType().GetProperty("Value").GetValue(self);
            }
        }

        public static bool TryGetValueByPropertyTag<T,_Value>(this object self, out _Value value, out Exception exception, bool safe = true) where T : IConvention_Property_Tag
        {
            exception = null;
            try
            {
                T that = (T)self;
                if (that is IProperty_Value_get<_Value> property_c)
                {
                    value = property_c.Value;
                    return true;
                }
                else
                {
                    if (safe) throw new ConventionTagNotFoundException();
                    value = (_Value)self.GetType().GetProperty("Value").GetValue(self);
                    return true;
                }
            }
            catch (Exception ex)
            {
                value = default;
                exception = ex;
                return false;
            }
        }
        public static _Value GetValueByPropertyTag<T, _Value>(this object self, bool safe = true) where T : IConvention_Property_Tag
        {
            T that = (T)self;
            if (that is IProperty_Value_get<_Value> property_c)
            {
                return property_c.Value;
            }
            else
            {
                if (safe) throw new ConventionTagNotFoundException();
                return (_Value)self.GetType().GetProperty("Value").GetValue(self);
            }
        }

        #endregion

        #region Set

        public static bool TrySetValue<T, _Value>(object self, _Value value, out Exception exception, bool safe = true) where T : IConvention_Property_Tag
        {
            return self.TrySetValueByPropertyTag<T, _Value>(value, out exception, safe);
        }

        public static bool TrySetValueByPropertyTag<T>(this object self, object value, out Exception exception, bool safe = true) where T : IConvention_Property_Tag
        {
            exception = null;
            try
            {
                T that = (T)self;
                if (that is IProperty_Value_set property_c)
                {
                    property_c.Value = value;
                    return true;
                }
                else
                {
                    if (safe) throw new ConventionTagNotFoundException();
                    self.GetType().GetProperty("Value").SetValue(self, value);
                    return true;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool TrySetValueByPropertyTag<T, _Value>(this object self, _Value value, out Exception exception, bool safe = true) where T : IConvention_Property_Tag
        {
            exception = null;
            try
            {
                T that = (T)self;
                if (that is IProperty_Value_set<_Value> property_c)
                {
                    property_c.Value = value;
                    return true;
                }
                else
                {
                    if (safe) throw new ConventionTagNotFoundException();
                    ((_Value)self).GetType().GetProperty("Value").SetValue(self, value);
                    return true;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        #endregion

        #region Invoke

        public static bool ConventionInvoke_Return_Void<T>(this object self, out Exception exception, bool safe = true)
        {
            exception = null;
            try
            {
                T that = (T)self;
                if (that is IFunction_Return_Void func_c)
                {
                    func_c.Invoke();
                    return true;
                }
                else
                {
                    if (safe) throw new ConventionTagNotFoundException();
                    self.GetType().GetMethod("Invoke").Invoke(self, null);
                    return true;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }
        public static bool ConventionInvoke_Return<T, _Return>(this object self, out _Return result, out Exception exception, bool safe = true)
        {
            exception = null;
            try
            {
                T that = (T)self;
                if (that is IFunction_Return<_Return> func_c)
                {
                    result = func_c.Invoke();
                    return true;
                }
                else
                {
                    if (safe) throw new ConventionTagNotFoundException();
                    result = (_Return)self.GetType().GetMethod("Invoke").Invoke(self, null);
                    return true;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                result = default;
                return false;
            }
        }
        public static bool ConventionInvoke_Return_Args<T, _Return, _Args>(this object self, out _Return result, _Args args, out Exception exception, bool safe = true)
        {
            exception = null;
            try
            {
                T that = (T)self;
                if (that is IFunction_Return_Args<_Return, _Args> func_c)
                {
                    result = func_c.Invoke(args);
                    return true;
                }
                else
                {
                    if (safe) throw new ConventionTagNotFoundException();
                    result = (_Return)self.GetType().GetMethod("Invoke").Invoke(self, new object[] { args });
                    return true;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                result = default;
                return false;
            }
        }

        #endregion

        public static _Sub As<_Sub>(this IConvention_As_interface<_Sub> self)
        {
            return (_Sub)self;
        }

        public static bool IsConventionType(this Type self) => self.IsSubclassOf(ConventionTag);
        public static bool IsConventionPropertyType(this Type self) => self.IsSubclassOf(ConventionPropertyTag);
        public static bool IsConventionFunctionType(this Type self) => self.IsSubclassOf(ConventionFunctionTag);
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class ConventionInfo
    {
        Dictionary<Type, ConventionPropertyInfo> ConventionInterface_Property_Data = new();
        Dictionary<Type, ConventionFunctionInfo> ConventionInterface_Function_Data = new();
        public readonly object that;

        internal ConventionInfo(object that)
        {
            foreach (var interfaceItem in
                that.GetAllInterfaces().GetSubList(S => S.IsConventionType()))
            {
                if (interfaceItem.IsConventionPropertyType())
                {
                    ConventionInterface_Property_Data.Add(interfaceItem, new ConventionPropertyInfo(interfaceItem, interfaceItem.GetProperty("Value").PropertyType));
                }
                else if (interfaceItem.IsConventionFunctionType())
                {
                    var info = interfaceItem.GetMethod("Invoke");
                    var ParameterInfoList = info.GetParameters();
                    Type[] ArgsTypes = new Type[ParameterInfoList.Length];
                    foreach (var item in ParameterInfoList)
                    {
                        ArgsTypes[item.Position] = item.ParameterType;
                    }
                    ConventionInterface_Function_Data.Add(interfaceItem, new ConventionFunctionInfo(interfaceItem, info.ReturnType, ArgsTypes));
                }
                else throw new ConventionErrorTagException();
            }
            this.that = that;
        }
    }

    public class ConventionPropertyInfo
    {
        public Type ConventionInterface;
        public Type PropertyType;

        public ConventionPropertyInfo(Type conventionInterface, Type propertyType)
        {
            ConventionInterface = conventionInterface;
            PropertyType = propertyType;
        }
    }

    public class ConventionFunctionInfo
    {
        public Type ConventionInterface;
        public Type ReturnType;
        public Type[] ArgsTypes;

        public ConventionFunctionInfo(Type conventionInterface, Type returnType, Type[] argsTypes)
        {
            ConventionInterface = conventionInterface;
            ReturnType = returnType;
            ArgsTypes = argsTypes;
        }
    }

    #region Convention Interface

    public interface IConvention_Tag { }
    public interface IConvention_Property_Tag : IConvention_Tag { }
    public interface IConvention_Function_Tag : IConvention_Tag { }

    public interface IConvention_As_interface<_Sub> : IConvention_Tag { }

    public interface IProperty_Value_get : IConvention_Property_Tag { object Value { get; } }
    public interface IProperty_Value_set : IConvention_Property_Tag { object Value { set; } }
    public interface IProperty_Value : IProperty_Value_get, IProperty_Value_set { }

    public interface IProperty_Value_get<T> : IConvention_Property_Tag { T Value { get; } }
    public interface IProperty_Value_set<T> : IConvention_Property_Tag { T Value { set; } }
    public interface IProperty_Value<T> : IProperty_Value_get<T>, IProperty_Value_set<T> { }

    public interface IFunction_Return_Void : IConvention_Function_Tag { void Invoke(); }
    /// <summary>
    /// _Args应为一个参数包
    /// </summary>
    /// <typeparam name="_Args">参数包装类</typeparam>
    public interface IFunction_Return_Void_Args<_Args> : IConvention_Function_Tag 
    { void Invoke(_Args args); }
    public interface IFunction_Return_Void_Params_Args<_Args> : IConvention_Function_Tag 
    { void Invoke(params object[] args); }
    public interface IFunction_Return_Void_Args<_First, _Second> : IConvention_Function_Tag 
    { void Invoke(_First first, _Second second); }
    public interface IFunction_Return_Void_Args<_First, _Second, _Third> : IConvention_Function_Tag 
    { void Invoke(_First first, _Second second, _Third third); }
    public interface IFunction_Return_Void_Args<_First, _Second, _Third, _Fourth> : IConvention_Function_Tag 
    { void Invoke(_First first, _Second second, _Third third, _Fourth fourth); }

    public interface IFunction_Return<_Return> : IConvention_Function_Tag { _Return Invoke(); }
    /// <summary>
    /// _Args应为一个参数包
    /// </summary>
    /// <typeparam name="_Args">参数包装类</typeparam>
    public interface IFunction_Return_Args<_Return, _Args> : IConvention_Function_Tag 
    { _Return Invoke(_Args args); }
    public interface IFunction_Return_Params_Args<_Return, _Args> : IConvention_Function_Tag 
    { _Return Invoke(params object[] args); }
    public interface IFunction_Return_Args<_Return, _First, _Second> : IConvention_Function_Tag 
    { _Return Invoke(_First first, _Second second); }
    public interface IFunction_Return_Args<_Return, _First, _Second, _Third> : IConvention_Function_Tag 
    { _Return Invoke(_First first, _Second second, _Third third); }
    public interface IFunction_Return_Args<_Return, _First, _Second, _Third, _Fourth> : IConvention_Function_Tag 
    { _Return Invoke(_First first, _Second second, _Third third, _Fourth fourth); }

    #endregion

}

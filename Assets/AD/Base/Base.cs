using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using AD.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AD.BASE
{
    #region AD_I

    /*
     * ADInstance
     * Base
     * Low-level public implementation
     */

    /// <summary>
    /// Implementations for runtime entities or data , The relevant interfaces are : 
    /// <para><see cref="IBaseMap"/> is the type used to convert to local data</para>
    /// <para><see cref="IBase{T}"/> This type is used for stronger constraints and clear goals</para>
    /// </summary>
    public interface IBase
    {
        void ToMap(out IBaseMap BM);
        bool FromMap(IBaseMap from);
    }

    /// <summary>
    /// A strongly constrained version of <see cref="IBase"/>
    /// </summary>
    /// <typeparam name="T">The target type of <see cref="IBaseMap{T}"/> you want to match</typeparam>
    public interface IBase<T> : IBase where T : class, IBaseMap, new()
    {
        void ToMap(out T BM);
        bool FromMap(T from);
    }


    /// <summary>
    /// Implementations for cache data , The relevant interfaces are : 
    /// <para><see cref="IBase"/> is the type used to convert to runtime entities or data</para>
    /// <para><see cref="IBaseMap{T}"/> This type is used for stronger constraints and clear goals</para>
    /// </summary>
    public interface IBaseMap
    {
        void ToObject(out IBase obj);
        bool FromObject(IBase from);
        string Serialize();
        bool Deserialize(string source);
    }

    /// <summary>
    /// A strongly constrained version of <see cref="IBaseMap"/>
    /// </summary>
    /// <typeparam name="T">The target type of <see cref="IBase{T}"/> you want to match</typeparam>
    public interface IBaseMap<T> : IBaseMap where T : class, IBase, new()
    {
        void ToObject(out T obj);
        bool FromObject(T from);
    }

    #endregion

    #region AD_S

    /// <summary>
    /// Commonly used exception
    /// </summary>
    [Serializable]
    public class ADException : Exception
    {
        public ADException() { AD__GeneratedTime = DateTime.Now; }
        public ADException(string message) : base(message) { AD__GeneratedTime = DateTime.Now; }
        public ADException(string message, Exception inner) : base(message, inner) { AD__GeneratedTime = DateTime.Now; }
        public ADException(Exception inner) : base("inner error", inner) { AD__GeneratedTime = DateTime.Now; }
        protected ADException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { AD__GeneratedTime = DateTime.Now; }

        public string Serialize()
        {
            return "[" + AD__GeneratedTime.ToString() + "]:" + Message;
        }
        public string SerializeStackTrace()
        {
            return "[" + AD__GeneratedTime.ToString() + "]:" + StackTrace;
        }
        public string SerializeSource()
        {
            return "[" + AD__GeneratedTime.ToString() + "]:" + Source;
        }
        public string SerializeMessage()
        {
            return "[" + AD__GeneratedTime.ToString() + "]:" + Message;
        }
        public string SerializeHelpLink()
        {
            return "[" + AD__GeneratedTime.ToString() + "]:" + HelpLink;
        }

        private DateTime AD__GeneratedTime;
    }

    public class NullArchitecture : ADException
    {
        public NullArchitecture() : base("Architecture is null but you also try to use it") { }
    }

    public static class ArchitectureExtension
    {
        public static _System GetSystem<_System>(this ICanGetSystem self) where _System : class, IADSystem
        {
            if(self.Architecture==null)
            {
                throw new NullArchitecture();
            }
            return self.Architecture.GetSystem<_System>();
        }

        public static _Model GetModel<_Model>(this ICanGetSystem self) where _Model : class, IADModel
        {
            if (self.Architecture == null)
            {
                throw new NullArchitecture();
            }
            return self.Architecture.GetModel<_Model>();
        }

        public static _Controller GetController<_Controller>(this ICanGetController self) where _Controller : class, IADController
        {
            if (self.Architecture == null)
            {
                throw new NullArchitecture();
            }
            return self.Architecture.GetController<_Controller>();
        }

        public static IADArchitecture SendCommand<_Command>(this ICanSendCommand self) where _Command : class, IADCommand,new()
        {
            if (self.Architecture == null)
            {
                throw new NullArchitecture();
            }
            return self.Architecture.SendCommand<_Command>();
        }

        public static void RegisterCommand<_Command>(this IADSystem self)where _Command : class, IADCommand,new()
        {
            if(self.Architecture == null)
            {
                throw new NullArchitecture();
            }
            self.Architecture.RegisterCommand<_Command>();
        }
        public static void RegisterCommand<_Command>(this IADSystem self, _Command command) where _Command : class, IADCommand
        {
            if (self.Architecture == null)
            {
                throw new NullArchitecture();
            }
            self.Architecture.RegisterCommand<_Command>(command);
        }
        public static  void UnRegisterCommand<_Command>(this IADSystem self) where _Command : class, IADCommand
        {
            if(self.Architecture == null)
            {
                throw new NullArchitecture();
            }
            self.Architecture.UnRegister<_Command>();
        }
        public static void SendCommand<_Command>(this IADSystem self) where _Command : class, IADCommand
        {
            if (self.Architecture == null)
            {
                throw new NullArchitecture();
            }
            self.Architecture.SendCommand<_Command>();
        }
        public static void DiffusingCommand<_Command>(this IADSystem self) where _Command : class, IADCommand
        {
            if (self.Architecture == null)
            {
                throw new NullArchitecture();
            }
            self.Architecture.Diffusing<_Command>();
        }
    }

    /// <summary>
    /// The lowest-level interface of all AD Architecture interfaces
    /// </summary>
    public interface IAnyArchitecture
    {

    }
    /// <summary>
    /// <see cref="Init"/> : used to initialize or reset the class
    /// </summary>
    public interface ICanInitialize : IAnyArchitecture
    {
        void Init();
    }
    /// <summary>
    /// Implement this interface to get or set the Architecture
    /// <para><see cref="IADArchitecture"/> : Target Architecture Interface</para>
    /// </summary>
    public interface ICanGetArchitecture : IAnyArchitecture
    {
        [Obsolete]
        IADArchitecture ADInstance();
        [Obsolete]
        void SetArchitecture(IADArchitecture target);

        IADArchitecture Architecture { get; set; }
    }
    public interface ICanGetSystem : ICanGetArchitecture
    {

    }
    public interface ICanGetModel : ICanGetArchitecture
    {

    }
    public interface ICanGetController : ICanGetArchitecture
    {

    }
    public interface ICanSendCommand : ICanGetArchitecture
    {

    }
    public interface ICanMonitorCommand<_Command> : IAnyArchitecture where _Command : IADCommand
    {
        void OnCommandCall(_Command c);
    }

    //TODO
    public interface IADArchitecture:IAnyArchitecture,ICanInitialize
    {
        /// <summary>
        /// Save the messages generated during the Architecture running
        /// </summary>
        /// <param name="message">Target message</param>
        /// <returns>Architecture itself</returns>
        IADArchitecture AddMessage(string message);

        #region Get
        _Model GetModel<_Model>() where _Model : class, IADModel;
        _System GetSystem<_System>() where _System : class, IADSystem;
        _Controller GetController<_Controller>() where _Controller : class, IADController;
        #endregion

        #region Register By Instance
        IADArchitecture RegisterModel<_Model>(_Model model) where _Model : IADModel;
        IADArchitecture RegisterSystem<_System>(_System system) where _System : IADSystem;
        IADArchitecture RegisterController<_Controller>(_Controller controller) where _Controller : IADController;
        IADArchitecture RegisterCommand<_Command>(_Command command) where _Command : IADCommand;
        #endregion

        #region Register By Default new()
        IADArchitecture RegisterModel<_Model>() where _Model : IADModel, new();
        IADArchitecture RegisterSystem<_System>() where _System : IADSystem, new();
        IADArchitecture RegisterController<_Controller>() where _Controller : IADController, new();
        IADArchitecture RegisterCommand<_Command>() where _Command : IADCommand, new();
        #endregion

        #region Send Command Execute By OnExecute()
        public IADArchitecture SendImmediatelyCommand<_Command>() where _Command : class, IADCommand, new();
        public IADArchitecture SendImmediatelyCommand<_Command>(_Command command) where _Command : class, IADCommand;
        IADArchitecture SendCommand<_Command>() where _Command : class, IADCommand;
        #endregion

        #region Send Command Or Diffusing Command To Target
        void Diffusing<_Command>() where _Command : IADCommand;
        void Diffusing<_Command>(_Command command) where _Command : IADCommand;
        void Send<_Command, _CanMonitorCommand>() where _Command : IADCommand where _CanMonitorCommand : class, ICanMonitorCommand<_Command>;
        void Send<_Command, _CanMonitorCommand>(_Command command) where _Command : IADCommand where _CanMonitorCommand : class, ICanMonitorCommand<_Command>;
        #endregion

        #region Register Or UnRegister Or Contains
        IADArchitecture UnRegister<_T>() where _T : IAnyArchitecture;
        IADArchitecture UnRegister(Type type);
        IADArchitecture Register<_T>() where _T : IAnyArchitecture, new();
        IADArchitecture Register<_T>(_T instance) where _T : IAnyArchitecture;
        bool Contains<_Type>();
        #endregion
    }

    public interface IADModel : ICanInitialize, ICanGetArchitecture
    {

    }
    /// <summary>
    /// Standard implementation of <see cref="IADModel"/>
    /// </summary>
    public abstract class ADModel : IADModel
    {
        public IADArchitecture Architecture { get; set; }

        public IADArchitecture ADInstance()
        {
            return Architecture;
        }
        public void SetArchitecture(IADArchitecture target)
        {
            Architecture = target;
        }

        public abstract void Init();
    }

    public interface IADSystem : ICanInitialize, ICanSendCommand, ICanGetArchitecture, ICanGetController
    {

    }
    /// <summary>
    /// Standard implementation of <see cref="IADSystem"/>
    /// </summary>
    public abstract class ADSystem : IADSystem
    {
        public IADArchitecture Architecture { get; set; }
        public IADArchitecture ADInstance()
        {
            return Architecture;
        }
        public void SetArchitecture(IADArchitecture target)
        {
            Architecture = target;
        }
        public abstract void Init();
    }
    /// <summary>
    /// Standard implementation of <see cref="IADSystem"/> , and Base on <see cref="MonoBehaviour"/>
    /// </summary>
    public abstract class MonoSystem : MonoBehaviour, IADSystem
    {
        public IADArchitecture Architecture { get; set; }
        public IADArchitecture ADInstance()
        {
            return Architecture;
        }
        public void SetArchitecture(IADArchitecture target)
        {
            Architecture = target;
        }
        public abstract void Init();
    }

    public interface IADController : ICanInitialize, ICanGetArchitecture, ICanSendCommand, ICanGetSystem, ICanGetModel
    {

    }
    /// <summary>
    /// Standard implementation of <see cref="IADController"/> , and Base on <see cref="MonoBehaviour"/>
    /// <para>Default <see cref="OnDestroy"/> will Unregister itself</para>
    /// </summary>
    public abstract class ADController : MonoBehaviour, IADController
    {
        public IADArchitecture Architecture { get; set; } = null;
        public IADArchitecture ADInstance()
        {
            return Architecture;
        }
        public abstract void Init();
        public void SetArchitecture(IADArchitecture target)
        {
            Architecture = target;
        }
        protected virtual void OnDestroy()
        {
            Architecture?.UnRegister(this.GetType());
        }
    }

    public interface IADCommand : ICanGetArchitecture, ICanGetModel, ICanGetController
    {
        void Execute();
        string LogMessage();
    }
    /// <summary>
    /// Standard implementation of <see cref="IADCommand"/>
    /// </summary>
    public abstract class ADCommand : IADCommand
    {
        public IADArchitecture Architecture { get; set; } = null;

        public IADArchitecture ADInstance()
        {
            return Architecture;
        }

        public void Execute()
        {
            if (Architecture == null) throw new ADException("Can not execute a command without setting architecture");
            Architecture.AddMessage(LogMessage());
            OnExecute();
        }

        public virtual string LogMessage()
        {
            return this.GetType().FullName + "(Command) is been send";
        }

        public abstract void OnExecute();

        public void SetArchitecture(IADArchitecture target)
        {
            Architecture = target;
        }
    }
    /// <summary>
    /// The purpose is to provide <see cref="AD.BASE.IADArchitecture.Diffusing{_Command}()"/>> or
    ///  <see cref="IADArchitecture.Diffusing{_Command}(_Command)"/>> within the Architecture
    /// <para>The relevant interfaces are : </para>
    /// <para><see cref="ICanMonitorCommand{_Command}"/> is the type used for execute when 
    /// <see cref="Vibration"/>(also <see cref="IADCommand"/>) start a Command Diffusing(<see cref="IADArchitecture"/>)</para>
    /// </summary>
    public class Vibration : ADCommand
    {
        protected Vibration() { }

        public override void OnExecute()
        {

        }
    }

    /// <summary>
    /// It is used to collect messages within the schema and has <see cref="What"/> that returns a string
    /// </summary>
    public interface IADMessage
    {
        string What();
    }
    /// <summary>
    /// Standard implementation of <see cref="IADMessage"/>
    /// <para>This message type is able to record <see cref="DateTime.Now"/> it was generated</para>
    /// </summary>
    public class ADMessage : IADMessage
    {
        public string AD__Message = "null";

        public ADMessage(string message) { AD__Message = "[" + DateTime.Now.ToString() + "] " + message; }

        public string What()
        {
            return AD__Message;
        }
    }
    /// <summary>
    /// A default message collector
    /// <para>The relevant interfaces are : </para>
    /// <para><see cref="IADMessage"/></para>
    /// </summary>
    public class ADMessageRecord : IADModel
    {
        public IADArchitecture Architecture { get; set; }

        private List<IADMessage> AD__messages = new();

        public void Init()
        {
            AD__messages.Add(new ADMessage("Already generated"));
        }

        public string What()
        {
            string cat = "";
            foreach (var message in AD__messages) cat += message.What() + "\n";
            return cat;
        }

        public void Add(IADMessage message)
        {
            if (Count > MaxCount) AD__messages.RemoveAt(0);
            AD__messages.Add(message);
        }

        public void Remove(IADMessage message)
        {
            AD__messages.Remove(message);
        }

        public void SetArchitecture(IADArchitecture target)
        {
            Architecture = target;
        }

        public virtual void Save(string path)
        {
            ADFile file = new(path, true, false, false, true);
            file.Serialize(What());
            file.Dispose();
        }

        public IADArchitecture ADInstance()
        {
            return Architecture;
        }

        public int Count { get { return AD__messages.Count; } }

        public int MaxCount = 1000;
    }

    //TODO
    public abstract class ADArchitecture<T> : IADArchitecture where T : ADArchitecture<T>, new()
    {
        #region attribute

        private Dictionary<Type, object> AD__Objects = new();
        private static IADArchitecture __ADinstance = null;
        public static T instance
        {
            get
            {
                if (__ADinstance == null)
                {
                    __ADinstance = new T();
                    ObjectExtension.AllArchitecture.Add(typeof(T), __ADinstance);
                    __ADinstance.Init();
                }
                return __ADinstance as T;
            }
        }

        protected ADMessageRecord MessageRecord
        {
            get
            {
                Type key = typeof(ADMessageRecord);
                if (!this.AD__Objects.ContainsKey(key)) AD__Objects.Add(key, new ADMessageRecord());
                return this.AD__Objects[key] as ADMessageRecord;
            }
        }

        protected ADArchitecture() { }

        #endregion

        #region mFunction

        public static void Destory()
        {
            __ADinstance = null;
            System.GC.Collect();
            ObjectExtension.AllArchitecture.Remove(typeof(T));
        }

        public virtual void SaveRecord()
        {
            string fileName = "";
            fileName += DateTime.Now.DayOfWeek.ToString() + " ";
            fileName += DateTime.Now.Hour.ToString() + "h";
            fileName += DateTime.Now.Minute.ToString() + "m";
            fileName += DateTime.Now.Second.ToString() + "s";
            string fullPath = Path.Combine(Application.persistentDataPath, this.GetType().FullName.Replace('.', '\\'), fileName) + ".AD.log";
            var dic = FileC.CreateDirectroryOfFile(fullPath);
            if (dic.GetFiles().Length > 100) dic.Delete();
            MessageRecord.Save(fullPath);
        }

        public virtual void Init()
        {

        }

        public IADArchitecture Register<_T>(_T _object) where _T :IAnyArchitecture
        {
            var key = typeof(_T);
            if (_p_null_type.FirstOrDefault(T => T.Equals(key)) != null)
            {
                Debug.LogWarning($"{key.FullName} is register now , but you try get it at last");
                _p_null_type.RemoveAll(T => T.Equals(key));
            }
            bool boolen = AD__Objects.ContainsKey(key);
            AD__Objects[key] = _object;
            if (boolen)
                AddMessage(_object.GetType().FullName + " is register on slot[" + key.FullName + "]");
            else
                AddMessage(_object.GetType().FullName + " is register on slot[" + key.FullName + "] , this slot is been create now");
            return instance;
        }

        public IADArchitecture Register<_T>() where _T : IAnyArchitecture,new()
        {
            return Register<_T>(new _T());
        }

        private object _p_last_object;
        private Type _p_last_type;
        private List<Type> _p_null_type = new();
        /// <summary>
        /// When there are extremely common objects, you can try to override the method
        /// </summary>
        /// <typeparam name="_T"></typeparam>
        /// <returns></returns>
        protected virtual object Get<_T>()
        {
            if (typeof(_T) == _p_last_type) return _p_last_object;
            if (AD__Objects.TryGetValue(typeof(_T), out var result))
            {
                _p_last_type = typeof(_T);
                return _p_last_object = result;
            }
            else
            {
                Debug.LogWarning("You get a unregister target , it may be distroy or missing");
                _p_null_type.Add(typeof(_T));
                return null;
            }
        }

        public IADArchitecture UnRegister(Type type)
        {
            if(type.GetInterface(nameof(IAnyArchitecture))==null)
            {
                AddMessage(type.FullName + " is not base on " + nameof(IAnyArchitecture));
            }
            if( AD__Objects.Remove(type))
            {
                AddMessage(type.FullName + " is unregister now");
            }
            else
            {
                AddMessage(type.FullName + " is not register , but you try to unregister it");
            }
            return this;
        }

        public IADArchitecture UnRegister<_T>() where _T : IAnyArchitecture
        {
            return UnRegister(typeof(_T));
        }

        public bool Contains<_Type>()
        {
            return AD__Objects.ContainsKey(typeof(_Type));
        }

        public _Model GetModel<_Model>() where _Model : class, IADModel
        {
            return Get<_Model>() as _Model;
        }

        public _System GetSystem<_System>() where _System : class, IADSystem
        {
            return Get<_System>() as _System;
        }

        public _Controller GetController<_Controller>() where _Controller : class, IADController
        {
            return Get<_Controller>() as _Controller;
        }

        public IADArchitecture RegisterModel<_Model>(_Model model) where _Model : IADModel
        {
            Register<_Model>(model);
            model.Architecture = this;
            model.Init();
            return instance;
        }

        public IADArchitecture RegisterSystem<_System>(_System system) where _System : IADSystem
        {
            Register<_System>(system);
            system.Architecture = this;
            system.Init();
            return instance;
        }

        public IADArchitecture RegisterController<_Controller>(_Controller controller) where _Controller : IADController
        {
            Register<_Controller>(controller);
            controller.Architecture = this;
            controller.Init();
            return instance;
        }

        public IADArchitecture RegisterModel<_Model>() where _Model : IADModel, new()
        {
            RegisterModel(new _Model());
            return instance;
        }

        public IADArchitecture RegisterSystem<_System>() where _System : IADSystem, new()
        {
            RegisterSystem(new _System());
            return instance;
        }

        public IADArchitecture RegisterController<_Controller>() where _Controller : IADController, new()
        {
            RegisterController(new _Controller());
            return instance;
        }

        public IADArchitecture RegisterCommand<_Command>(_Command command) where _Command : IADCommand
        {
            Register(command);
            command.Architecture = this;
            return instance;
        }

        public IADArchitecture RegisterCommand<_Command>() where _Command : IADCommand, new()
        {
            RegisterCommand(new _Command());
            return instance;
        }

        public virtual IADArchitecture AddMessage(string message)
        {
            MessageRecord.Add(new ADMessage(message));
            Debug.Log(message);
            return instance;
        }

        public IADArchitecture SendCommand<_Command>() where _Command : class, IADCommand
        {
            (Get<_Command>() as _Command).Execute();
            return instance;
        }
        public IADArchitecture SendImmediatelyCommand<_Command>() where _Command : class, IADCommand, new()
        {
            if (Contains<_Command>()) SendCommand<_Command>();
            else SendImmediatelyCommand(new _Command());
            return instance;
        }
        public IADArchitecture SendImmediatelyCommand<_Command>(_Command command) where _Command : class, IADCommand
        {
            RegisterCommand(command);
            command.Execute();
            return instance;
        }
        #endregion

        #region sFunction

        /// <summary>
        /// Pass directives (types) to the entire architecture so that registered objects 
        /// that can accept this type can trigger callbacks
        /// </summary>
        /// <typeparam name="_Command"></typeparam>
        public void Diffusing<_Command>() where _Command : IADCommand
        {
            foreach (var item in AD__Objects)
            {
                if (item.Is(out ICanMonitorCommand<_Command> monitor))
                {
                    monitor.OnCommandCall(default);
                }
            }
        }

        /// <summary>
        /// Pass directives (types) to the entire architecture so that registered objects 
        /// that can accept this type can trigger callbacks
        /// </summary>
        /// <typeparam name="_Command"></typeparam>
        public void Diffusing<_Command>(_Command command) where _Command : IADCommand
        {
            foreach (var item in AD__Objects)
            {
                if (item.Is(out ICanMonitorCommand<_Command> monitor))
                {
                    monitor.OnCommandCall(command);
                }
            }
        }

        /// <summary>
        /// Provides an easy way to propagate directives in a targeted manner, using a common flag class to enable the target class to call
        /// <para> a predetermined function (the predecessor of the convention pattern) </para>
        /// </summary>
        /// <typeparam name="_Command"></typeparam>
        /// <typeparam name="_CanMonitorCommand"></typeparam>
        public void Send<_Command, _CanMonitorCommand>(_Command command) where _Command : IADCommand where _CanMonitorCommand : class, ICanMonitorCommand<_Command>
        {
            if (AD__Objects.TryGetValue(typeof(_CanMonitorCommand), out object target))
            {
                target.As<_CanMonitorCommand>().OnCommandCall(command);
            }
            else
            {
                AddMessage(typeof(_CanMonitorCommand).Name + " is not register");
            }
        }

        /// <summary>
        /// Provides an easy way to propagate directives in a targeted manner, using a common flag class to enable the target class to call
        /// <para> a predetermined function (the predecessor of the convention pattern) </para>
        /// </summary>
        /// <typeparam name="_Command"></typeparam>
        /// <typeparam name="_CanMonitorCommand"></typeparam>
        public void Send<_Command, _CanMonitorCommand>() where _Command : IADCommand where _CanMonitorCommand : class, ICanMonitorCommand<_Command>
        {
            if (AD__Objects.TryGetValue(typeof(_CanMonitorCommand), out object target))
            {
                target.As<_CanMonitorCommand>().OnCommandCall(default);
            }
            else
            {
                AddMessage(typeof(_CanMonitorCommand).Name + " is not register");
            }
        }

        #endregion
    }

    public interface ISubPagesArchitecture : IEnumerable<IADArchitecture>
    {
        Dictionary<Type, IADArchitecture> SubArchitectures { get; }
        IADArchitecture this[Type type] { get; }
    }

    public abstract class TopArchitecture<T, _Entry, _MainPage, _EndPage, _SubPages>
        : ADArchitecture<T>
        where T : TopArchitecture<T, _Entry, _MainPage, _EndPage, _SubPages>, new()
        where _Entry : IADArchitecture
        where _MainPage : IADArchitecture
        where _EndPage : IADArchitecture
        where _SubPages : ISubPagesArchitecture
    {
        public abstract _Entry EntryArchitecture { get; }
        public abstract _MainPage MainArchitecture { get; }
        public abstract _EndPage EndArchitecture { get; }
        public abstract _SubPages SubArchitectures { get; }

        public IADArchitecture this[Type type]
        {
            get
            {
                if (type == typeof(_Entry)) return EntryArchitecture;
                else if (type == typeof(_MainPage)) return MainArchitecture;
                else if (type == typeof(_EndPage)) return EndArchitecture;
                else return SubArchitectures[type];
            }
        }
    }

    #endregion

    #region Event from Unity & ExtAD

    [Serializable]
    public abstract class ADBaseInvokableCall
    {
        public static bool IsLogParametersInstance
#if UNITY_EDITOR
            = true;
#else
            = false;
#endif

        public string ReturnType;
        public string Name;
        public string[] ParametersType;

        public static void DebugLogTarget(MethodInfo info)
        {
            string ReturnType = info.ReturnType.FullName;
            string FunName = info.Name;

            string ParaNames =
                info.GetParameters().Length > 0
                ? StringExtension.LinkAndInsert(info.GetParameters().GetSubList<string, ParameterInfo>(T => true, T => T.ParameterType.FullName + " " + T.Name).ToArray(), ",")
                : "";
            DebugExtenion.LogMessage($"{ReturnType} {FunName}({ParaNames})");
        }

        public static void DebugLogMethod(MethodInfo info, params object[] args)
        {
            if (IsLogParametersInstance)
            {
                InternalDebugLogMethod(info, null, args);
            }
            else
            {
                DebugLogTarget(info);
            }
        }

        protected static void InternalDebugLogMethod(MethodInfo info, Exception exception, params object[] args)
        {
            string ReturnType = info.ReturnType.FullName;
            string FunName = info.Name;

            int index = 0;

            if (exception == null)
            {
                if (args != null && args.Length != 0)
                {
                    string[] argsStrs = args.GetSubList<string, object>(T => true, T =>
                    {
                        string str = T.ToString();
                        return (str.Length < 40 && !str.Contains("\n") && !str.Contains("<b>")) ? str : "[Too Long]";
                    }).ToArray();
                    string ParaNames =
                        info.GetParameters().Length > 0
                        ? StringExtension
                        .LinkAndInsert(info.GetParameters().GetSubList<string, ParameterInfo>(T => true, T => T.ParameterType.FullName + " " + T.Name + $"[{argsStrs[index++]}]").ToArray(), ",")
                        : "";
                    DebugExtenion.LogMessage($"{ReturnType} {FunName}({ParaNames})");
                }
                else
                    DebugExtenion.LogMessage($"{ReturnType} {FunName}()");
            }
            else
            {
                string[] argsStrs = args.GetSubList<string, object>(T => true, T => T.ToString()).ToArray();
                string ParaNames =
                info.GetParameters().Length > 0
                    ? StringExtension
                    .LinkAndInsert(info.GetParameters().GetSubList<string, ParameterInfo>(T => true, T => "\t" + T.ParameterType.FullName + " " + T.Name + $"[{argsStrs[index++]}]").ToArray(), ",\n")
                    : "";
                DebugExtenion.LogMessage($"{ReturnType} {FunName}({ParaNames})");
            }
        }

        protected ADBaseInvokableCall(object target, MethodInfo function) : this(function)
        {
            if (function is null)
            {
                throw new ArgumentNullException("function");
            }

            if (function.IsStatic)
            {
                if (target != null)
                {
                    throw new ArgumentException("_Target must be null");
                }
            }
            else if (target == null)
            {
                throw new ArgumentNullException("_Target");
            }
        }

        protected ADBaseInvokableCall(Delegate function) : this(function.Method) { }

        private ADBaseInvokableCall(MethodInfo function)
            : this(function.ReflectedType.FullName,
                   function.Name,
                   function.GetParameters().GetSubList(T => true, T => T.ParameterType.FullName).ToArray())
        {

        }

        private ADBaseInvokableCall(string returnType, string name, string[] parameters)
        {
            ReturnType = returnType;
            Name = name;
            ParametersType = parameters;
        }

        public abstract void Invoke(object[] args);

        protected static void ThrowOnInvalidArg<T>(object arg)
        {
            if (arg != null && arg is not T)
            {
                throw new ArgumentException(string.Format("Passed argument is the wrong type. Type:{0} Expected:{1}", arg.GetType(), typeof(T)));
            }
        }

        protected static bool AllowInvoke(Delegate @delegate)
        {
            object target = @delegate.Target;
            if (target == null)
            {
                return true;
            }

            if (target is object @object)
            {
                return @object != null;
            }

            return true;
        }

        public abstract bool Find(object targetObj, MethodInfo method);
    }

    [Serializable]
    public class ADInvokableCall : ADBaseInvokableCall
    {
        protected event UnityAction Delegate;

        public ADInvokableCall(object target, MethodInfo theFunction)
            : base(target, theFunction)
        {
            this.Delegate = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), target, theFunction);
        }

        public ADInvokableCall(UnityAction action) : base(action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (args.Length != 0)
            {
                throw new ArgumentException("Passed argument 'args' is invalid size. Expected size is 1");
            }
            Invoke();
        }

        public void Invoke()
        {
            if (ADBaseInvokableCall.AllowInvoke(this.Delegate))
            {
                Exception exception = null;
                try
                {
                    this.Delegate();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                InternalDebugLogMethod(this.Delegate.Method, exception, null);
                if (exception != null) throw exception;
            }
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return this.Delegate.Target == targetObj && this.Delegate.Method.Equals(method);
        }

        public static ADInvokableCall Temp(object target, MethodInfo theFunction) => new(target, theFunction);
        public static ADInvokableCall Temp(UnityAction action) => new(action);
        public static ADInvokableCall Temp(Action action) => new(new UnityAction(action));

        public static implicit operator ADInvokableCall(UnityAction action) => Temp(action);
        public static implicit operator ADInvokableCall(Action action) => Temp(action);
    }

    [Serializable]
    public class ADInvokableCall<T1> : ADBaseInvokableCall
    {
        protected event UnityAction<T1> Delegate;

        public ADInvokableCall(object target, MethodInfo theFunction)
            : base(target, theFunction)
        {
            this.Delegate = (UnityAction<T1>)System.Delegate.CreateDelegate(typeof(UnityAction<T1>), target, theFunction);
        }

        public ADInvokableCall(UnityAction<T1> action) : base(action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Passed argument 'args' is invalid size. Expected size is 1");
            }

            ADBaseInvokableCall.ThrowOnInvalidArg<T1>(args[0]);
            Invoke((T1)args[0]);
        }

        public void Invoke(T1 args)
        {
            if (ADBaseInvokableCall.AllowInvoke(this.Delegate))
            {
                Exception exception = null;
                try
                {
                    this.Delegate(args);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                InternalDebugLogMethod(this.Delegate.Method, exception, args);
                if (exception != null) throw exception;
            }
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return this.Delegate.Target == targetObj && this.Delegate.Method.Equals(method);
        }

        public static ADInvokableCall<T1> Temp(object target, MethodInfo theFunction) => new(target, theFunction);
        public static ADInvokableCall<T1> Temp(UnityAction<T1> action) => new(action);
        public static ADInvokableCall<T1> Temp(Action<T1> action) => new(new UnityAction<T1>(action));

        public static implicit operator ADInvokableCall<T1>(UnityAction<T1> action) => Temp(action);
        public static implicit operator ADInvokableCall<T1>(Action<T1> action) => Temp(action);
    }

    [Serializable]
    public class ADInvokableCall<T1, T2> : ADBaseInvokableCall
    {
        protected event UnityAction<T1, T2> Delegate;

        public ADInvokableCall(object target, MethodInfo theFunction)
            : base(target, theFunction)
        {
            this.Delegate = (UnityAction<T1, T2>)System.Delegate.CreateDelegate(typeof(UnityAction<T1, T2>), target, theFunction);
        }

        public ADInvokableCall(UnityAction<T1, T2> action) : base(action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("Passed argument 'args' is invalid size. Expected size is 1");
            }

            ADBaseInvokableCall.ThrowOnInvalidArg<T1>(args[0]);
            ADBaseInvokableCall.ThrowOnInvalidArg<T2>(args[1]);
            Invoke((T1)args[0], (T2)args[1]);
        }

        public void Invoke(T1 args0, T2 args1)
        {
            if (ADBaseInvokableCall.AllowInvoke(this.Delegate))
            {
                Exception exception = null;
                try
                {
                    this.Delegate(args0, args1);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                InternalDebugLogMethod(this.Delegate.Method, exception, args0, args1);
                if (exception != null) throw exception;
            }
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return this.Delegate.Target == targetObj && this.Delegate.Method.Equals(method);
        }

        public static ADInvokableCall<T1, T2> Temp(object target, MethodInfo theFunction) => new(target, theFunction);
        public static ADInvokableCall<T1, T2> Temp(UnityAction<T1, T2> action) => new(action);
        public static ADInvokableCall<T1, T2> Temp(Action<T1, T2> action) => new(new UnityAction<T1, T2>(action));

        public static implicit operator ADInvokableCall<T1, T2>(UnityAction<T1, T2> action) => Temp(action);
        public static implicit operator ADInvokableCall<T1, T2>(Action<T1, T2> action) => Temp(action);
    }

    [Serializable]
    public class ADInvokableCall<T1, T2, T3> : ADBaseInvokableCall
    {
        protected event UnityAction<T1, T2, T3> Delegate;

        public ADInvokableCall(object target, MethodInfo theFunction)
            : base(target, theFunction)
        {
            this.Delegate = (UnityAction<T1, T2, T3>)System.Delegate.CreateDelegate(typeof(UnityAction<T1, T2, T3>), target, theFunction);
        }

        public ADInvokableCall(UnityAction<T1, T2, T3> action) : base(action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("Passed argument 'args' is invalid size. Expected size is 1");
            }

            ADBaseInvokableCall.ThrowOnInvalidArg<T1>(args[0]);
            ADBaseInvokableCall.ThrowOnInvalidArg<T2>(args[1]);
            ADBaseInvokableCall.ThrowOnInvalidArg<T3>(args[2]);
            Invoke((T1)args[0], (T2)args[1], (T3)args[2]);
        }

        public void Invoke(T1 args0, T2 args1, T3 args2)
        {
            if (ADBaseInvokableCall.AllowInvoke(this.Delegate))
            {
                Exception exception = null;
                try
                {
                    this.Delegate(args0, args1, args2);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                InternalDebugLogMethod(this.Delegate.Method, exception, args0, args1, args2);
                if (exception != null) throw exception;
            }
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return this.Delegate.Target == targetObj && this.Delegate.Method.Equals(method);
        }

        public static ADInvokableCall<T1, T2, T3> Temp(object target, MethodInfo theFunction) => new(target, theFunction);
        public static ADInvokableCall<T1, T2, T3> Temp(UnityAction<T1, T2, T3> action) => new(action);
        public static ADInvokableCall<T1, T2, T3> Temp(Action<T1, T2, T3> action) => new(new UnityAction<T1, T2, T3>(action));

        public static implicit operator ADInvokableCall<T1, T2, T3>(UnityAction<T1, T2, T3> action) => Temp(action);
        public static implicit operator ADInvokableCall<T1, T2, T3>(Action<T1, T2, T3> action) => Temp(action);
    }

    [Serializable]
    public class ADInvokableCall<T1, T2, T3, T4> : ADBaseInvokableCall
    {
        protected event UnityAction<T1, T2, T3, T4> Delegate;

        public ADInvokableCall(object target, MethodInfo theFunction)
            : base(target, theFunction)
        {
            this.Delegate = (UnityAction<T1, T2, T3, T4>)System.Delegate.CreateDelegate(typeof(UnityAction<T1, T2, T3, T4>), target, theFunction);
        }

        public ADInvokableCall(UnityAction<T1, T2, T3, T4> action) : base(action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (args.Length != 4)
            {
                throw new ArgumentException("Passed argument 'args' is invalid size. Expected size is 1");
            }

            ADBaseInvokableCall.ThrowOnInvalidArg<T1>(args[0]);
            ADBaseInvokableCall.ThrowOnInvalidArg<T2>(args[1]);
            ADBaseInvokableCall.ThrowOnInvalidArg<T3>(args[2]);
            ADBaseInvokableCall.ThrowOnInvalidArg<T4>(args[3]);
            Invoke((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]);
        }

        public void Invoke(T1 args0, T2 args1, T3 args2, T4 args3)
        {
            if (ADBaseInvokableCall.AllowInvoke(this.Delegate))
            {
                Exception exception = null;
                try
                {
                    this.Delegate(args0, args1, args2, args3);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                InternalDebugLogMethod(this.Delegate.Method, exception, args0, args1, args2, args3);
                if (exception != null) throw exception;
            }
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return this.Delegate.Target == targetObj && this.Delegate.Method.Equals(method);
        }

        public static ADInvokableCall<T1, T2, T3, T4> Temp(object target, MethodInfo theFunction) => new(target, theFunction);
        public static ADInvokableCall<T1, T2, T3, T4> Temp(UnityAction<T1, T2, T3, T4> action) => new(action);
        public static ADInvokableCall<T1, T2, T3, T4> Temp(Action<T1, T2, T3, T4> action) => new(new UnityAction<T1, T2, T3, T4>(action));

        public static implicit operator ADInvokableCall<T1, T2, T3, T4>(UnityAction<T1, T2, T3, T4> action) => Temp(action);
        public static implicit operator ADInvokableCall<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action) => Temp(action);
    }

    [Serializable]
    public abstract class ADBaseOrderlyEvent
    {
        public abstract ADBaseInvokableCall[] GetAllListener();
        public abstract void RemoveAllListeners();
        protected abstract MethodInfo FindMethod_Impl(string name, Type targetObjType);
        public abstract void Invoke(params object[] args);
    }

    [Serializable]
    public class ADOrderlyEvent : ADBaseOrderlyEvent
    {
        public List<int> InvokeArray = null;
        public int Count => (_m_Delegates == null) ? 0 : _m_Delegates.Count;
        [SerializeField] private List<ADInvokableCall> _m_Delegates = null;

        public void AddListener(UnityAction call)
        {
            InvokeArray ??= new List<int>();
            _m_Delegates ??= new List<ADInvokableCall>();
            InvokeArray.Add(_m_Delegates.Count);
            _m_Delegates.Add(GetDelegate(call));
        }

        public void RemoveListener(UnityAction call)
        {
            if (_m_Delegates == null) return;
            int index = _m_Delegates.FindIndex(T => T.Find(call.Target, call.Method));
            if (index != -1)
            {
                var cat = _m_Delegates[index];

                InvokeArray.RemoveAll(T => T == index);
                _m_Delegates.Remove(cat);

                for (int i = 0, e = InvokeArray.Count; i < e; i++)
                {
                    if (InvokeArray[i] > index)
                    {
                        InvokeArray[i]--;
                    }
                }
            }
        }

        public override void RemoveAllListeners()
        {
            InvokeArray = null;
            _m_Delegates = null;
        }

        protected override MethodInfo FindMethod_Impl(string name, Type targetObjType)
        {
            return UnityEventBase.GetValidMethodInfo(targetObjType, name, new Type[0]
            {
            });
        }

        public ADInvokableCall GetDelegate(object target, MethodInfo theFunction)
        {
            return new ADInvokableCall(target, theFunction);
        }

        private static ADInvokableCall GetDelegate(UnityAction action)
        {
            return new ADInvokableCall(action);
        }

        public void Invoke()
        {
            if (InvokeArray == null) return;

            List<int> bugIndex = new();
            foreach (var index in InvokeArray)
            {
                if (index >= 0 && index < _m_Delegates.Count)
                {
                    if (_m_Delegates[index] == null)
                    {
                        Debug.LogWarning("you like to try invoke a error action");
                        continue;
                    }
                    _m_Delegates[index].Invoke();
                }
                else
                {
                    Debug.LogWarning("you try to use a error index");
                    bugIndex.Add(index);
                }
            }
            foreach (var index in bugIndex)
            {
                InvokeArray.RemoveAll(T => T == index);
            }
        }

        public override ADBaseInvokableCall[] GetAllListener()
        {
            return _m_Delegates.ToArray();
        }

        public override void Invoke(params object[] args)
        {
            if (args.Length > 0) Debug.LogWarning("you try to input some error args");
            Invoke();
        }
    }

    [Serializable]
    public class ADOrderlyEvent<T0> : ADBaseOrderlyEvent
    {
        public List<int> InvokeArray = null;
        public int Count => (_m_Delegates == null) ? 0 : _m_Delegates.Count;
        [SerializeField] private List<ADInvokableCall<T0>> _m_Delegates = null;

        public void AddListener(UnityAction<T0> call)
        {
            InvokeArray ??= new List<int>();
            _m_Delegates ??= new List<ADInvokableCall<T0>>();
            InvokeArray.Add(_m_Delegates.Count);
            _m_Delegates.Add(GetDelegate(call));
        }

        public void RemoveListener(UnityAction<T0> call)
        {
            if (_m_Delegates == null) return;
            int index = _m_Delegates.FindIndex(T => T.Find(call.Target, call.Method));
            if (index != -1)
            {
                var cat = _m_Delegates[index];

                InvokeArray.RemoveAll(T => T == index);
                _m_Delegates.Remove(cat);

                for (int i = 0, e = InvokeArray.Count; i < e; i++)
                {
                    if (InvokeArray[i] > index)
                    {
                        InvokeArray[i]--;
                    }
                }
            }
        }

        public override void RemoveAllListeners()
        {
            InvokeArray = null;
            _m_Delegates = null;
        }

        protected override MethodInfo FindMethod_Impl(string name, Type targetObjType)
        {
            return UnityEventBase.GetValidMethodInfo(targetObjType, name, new Type[1]
            {
                typeof(T0)
            });
        }

        public ADInvokableCall<T0> GetDelegate(object target, MethodInfo theFunction)
        {
            return new ADInvokableCall<T0>(target, theFunction);
        }

        private static ADInvokableCall<T0> GetDelegate(UnityAction<T0> action)
        {
            return new ADInvokableCall<T0>(action);
        }

        public void Invoke(T0 arg0)
        {
            if (InvokeArray == null) return;

            List<int> bugIndex = new();
            foreach (var index in InvokeArray)
            {
                if (index >= 0 && index < _m_Delegates.Count)
                {
                    if (_m_Delegates[index] == null)
                    {
                        Debug.LogWarning("you like to try invoke a error action");
                        continue;
                    }
                    _m_Delegates[index].Invoke(arg0);
                }
                else
                {
                    Debug.LogWarning("you try to use a error index");
                    bugIndex.Add(index);
                }
            }
            foreach (var index in bugIndex)
            {
                InvokeArray.RemoveAll(T => T == index);
            }
        }

        public override ADBaseInvokableCall[] GetAllListener()
        {
            return _m_Delegates.ToArray();
        }

        public override void Invoke(params object[] args)
        {
            if (args.Length != 1) Debug.LogWarning("you try to input some error args");
            T0 a0 = (args[0] is T0 t0) ? t0 : default;
            Invoke(a0);
        }
    }

    [Serializable]
    public class ADOrderlyEvent<T0, T1> : ADBaseOrderlyEvent
    {
        public List<int> InvokeArray = null;
        public int Count => (_m_Delegates == null) ? 0 : _m_Delegates.Count;
        [SerializeField] private List<ADInvokableCall<T0, T1>> _m_Delegates = null;

        public void AddListener(UnityAction<T0, T1> call)
        {
            InvokeArray ??= new List<int>();
            _m_Delegates ??= new List<ADInvokableCall<T0, T1>>();
            InvokeArray.Add(_m_Delegates.Count);
            _m_Delegates.Add(GetDelegate(call));
        }

        public void RemoveListener(UnityAction<T0, T1> call)
        {
            if (_m_Delegates == null) return;
            int index = _m_Delegates.FindIndex(T => T.Find(call.Target, call.Method));
            if (index != -1)
            {
                var cat = _m_Delegates[index];

                InvokeArray.RemoveAll(T => T == index);
                _m_Delegates.Remove(cat);

                for (int i = 0, e = InvokeArray.Count; i < e; i++)
                {
                    if (InvokeArray[i] > index)
                    {
                        InvokeArray[i]--;
                    }
                }
            }
        }

        public override void RemoveAllListeners()
        {
            InvokeArray = null;
            _m_Delegates = null;
        }

        protected override MethodInfo FindMethod_Impl(string name, Type targetObjType)
        {
            return UnityEventBase.GetValidMethodInfo(targetObjType, name, new Type[2]
            {
                typeof(T0),
                typeof(T1)
            });
        }

        public ADInvokableCall<T0, T1> GetDelegate(object target, MethodInfo theFunction)
        {
            return new ADInvokableCall<T0, T1>(target, theFunction);
        }

        private static ADInvokableCall<T0, T1> GetDelegate(UnityAction<T0, T1> action)
        {
            return new ADInvokableCall<T0, T1>(action);
        }

        public void Invoke(T0 arg0, T1 arg1)
        {
            if (InvokeArray == null) return;

            List<int> bugIndex = new();
            foreach (var index in InvokeArray)
            {
                if (index >= 0 && index < _m_Delegates.Count)
                {
                    if (_m_Delegates[index] == null)
                    {
                        Debug.LogWarning("you like to try invoke a error action");
                        continue;
                    }
                    _m_Delegates[index].Invoke(arg0, arg1);
                }
                else
                {
                    Debug.LogWarning("you try to use a error index");
                    bugIndex.Add(index);
                }
            }
            foreach (var index in bugIndex)
            {
                InvokeArray.RemoveAll(T => T == index);
            }
        }

        public override ADBaseInvokableCall[] GetAllListener()
        {
            return _m_Delegates.ToArray();
        }

        public override void Invoke(params object[] args)
        {
            if (args.Length != 2) Debug.LogWarning("you try to input some error args");
            T0 a0 = (args[0] is T0 t0) ? t0 : default;
            T1 a1 = (args[1] is T1 t1) ? t1 : default;
            Invoke(a0, a1);
        }
    }

    [Serializable]
    public class ADOrderlyEvent<T0, T1, T2> : ADBaseOrderlyEvent
    {
        public List<int> InvokeArray = null;
        public int Count => (_m_Delegates == null) ? 0 : _m_Delegates.Count;
        [SerializeField] private List<ADInvokableCall<T0, T1, T2>> _m_Delegates = null;

        public void AddListener(UnityAction<T0, T1, T2> call)
        {
            InvokeArray ??= new List<int>();
            _m_Delegates ??= new List<ADInvokableCall<T0, T1, T2>>();
            InvokeArray.Add(_m_Delegates.Count);
            _m_Delegates.Add(GetDelegate(call));
        }

        public void RemoveListener(UnityAction<T0, T1, T2> call)
        {
            if (_m_Delegates == null) return;
            int index = _m_Delegates.FindIndex(T => T.Find(call.Target, call.Method));
            if (index != -1)
            {
                var cat = _m_Delegates[index];

                InvokeArray.RemoveAll(T => T == index);
                _m_Delegates.Remove(cat);

                for (int i = 0, e = InvokeArray.Count; i < e; i++)
                {
                    if (InvokeArray[i] > index)
                    {
                        InvokeArray[i]--;
                    }
                }
            }
        }

        public override void RemoveAllListeners()
        {
            InvokeArray = null;
            _m_Delegates = null;
        }

        protected override MethodInfo FindMethod_Impl(string name, Type targetObjType)
        {
            return UnityEventBase.GetValidMethodInfo(targetObjType, name, new Type[3]
            {
                typeof(T0),
                typeof(T1),
                typeof(T2)
            });
        }

        public ADInvokableCall<T0, T1, T2> GetDelegate(object target, MethodInfo theFunction)
        {
            return new ADInvokableCall<T0, T1, T2>(target, theFunction);
        }

        private static ADInvokableCall<T0, T1, T2> GetDelegate(UnityAction<T0, T1, T2> action)
        {
            return new ADInvokableCall<T0, T1, T2>(action);
        }

        public void Invoke(T0 arg0, T1 arg1, T2 arg2)
        {
            if (InvokeArray == null) return;

            List<int> bugIndex = new();
            foreach (var index in InvokeArray)
            {
                if (index >= 0 && index < _m_Delegates.Count)
                {
                    if (_m_Delegates[index] == null)
                    {
                        Debug.LogWarning("you like to try invoke a error action");
                        continue;
                    }
                    _m_Delegates[index].Invoke(arg0, arg1, arg2);
                }
                else
                {
                    Debug.LogWarning("you try to use a error index");
                    bugIndex.Add(index);
                }
            }
            foreach (var index in bugIndex)
            {
                InvokeArray.RemoveAll(T => T == index);
            }
        }

        public override ADBaseInvokableCall[] GetAllListener()
        {
            return _m_Delegates.ToArray();
        }

        public override void Invoke(params object[] args)
        {
            if (args.Length != 3) Debug.LogWarning("you try to input some error args");
            T0 a0 = (args[0] is T0 t0) ? t0 : default;
            T1 a1 = (args[1] is T1 t1) ? t1 : default;
            T2 a2 = (args[2] is T2 t2) ? t2 : default;
            Invoke(a0, a1, a2);
        }
    }

    [Serializable]
    public class ADOrderlyEvent<T0, T1, T2, T3> : ADBaseOrderlyEvent
    {
        public List<int> InvokeArray = null;
        public int Count => (_m_Delegates == null) ? 0 : _m_Delegates.Count;
        [SerializeField] private List<ADInvokableCall<T0, T1, T2, T3>> _m_Delegates = null;

        public void AddListener(UnityAction<T0, T1, T2, T3> call)
        {
            InvokeArray ??= new List<int>();
            _m_Delegates ??= new List<ADInvokableCall<T0, T1, T2, T3>>();
            InvokeArray.Add(_m_Delegates.Count);
            _m_Delegates.Add(GetDelegate(call));
        }

        public void RemoveListener(UnityAction<T0, T1, T2, T3> call)
        {
            if (_m_Delegates == null) return;
            int index = _m_Delegates.FindIndex(T => T.Find(call.Target, call.Method));
            if (index != -1)
            {
                var cat = _m_Delegates[index];

                InvokeArray.RemoveAll(T => T == index);
                _m_Delegates.Remove(cat);

                for (int i = 0, e = InvokeArray.Count; i < e; i++)
                {
                    if (InvokeArray[i] > index)
                    {
                        InvokeArray[i]--;
                    }
                }
            }
        }

        public override void RemoveAllListeners()
        {
            InvokeArray = null;
            _m_Delegates = null;
        }

        protected override MethodInfo FindMethod_Impl(string name, Type targetObjType)
        {
            return UnityEventBase.GetValidMethodInfo(targetObjType, name, new Type[4]
            {
                typeof(T0),
                typeof(T1),
                typeof(T2),
                typeof(T3)
            });
        }

        public ADInvokableCall<T0, T1, T2, T3> GetDelegate(object target, MethodInfo theFunction)
        {
            return new ADInvokableCall<T0, T1, T2, T3>(target, theFunction);
        }

        private static ADInvokableCall<T0, T1, T2, T3> GetDelegate(UnityAction<T0, T1, T2, T3> action)
        {
            return new ADInvokableCall<T0, T1, T2, T3>(action);
        }

        public void Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            if (InvokeArray == null) return;

            List<int> bugIndex = new();
            foreach (var index in InvokeArray)
            {
                if (index >= 0 && index < _m_Delegates.Count)
                {
                    if (_m_Delegates[index] == null)
                    {
                        Debug.LogWarning("you like to try invoke a error action");
                        continue;
                    }
                    _m_Delegates[index].Invoke(arg0, arg1, arg2, arg3);
                }
                else
                {
                    Debug.LogWarning("you try to use a error index");
                    bugIndex.Add(index);
                }
            }
            foreach (var index in bugIndex)
            {
                InvokeArray.RemoveAll(T => T == index);
            }
        }

        public override ADBaseInvokableCall[] GetAllListener()
        {
            return _m_Delegates.ToArray();
        }

        public override void Invoke(params object[] args)
        {
            if (args.Length != 4) Debug.LogWarning("you try to input some error args");
            T0 a0 = (args[0] is T0 t0) ? t0 : default;
            T1 a1 = (args[1] is T1 t1) ? t1 : default;
            T2 a2 = (args[2] is T2 t2) ? t2 : default;
            T3 a3 = (args[3] is T3 t3) ? t3 : default;
            Invoke(a0, a1, a2, a3);
        }
    }

    [Serializable]
    public class ADEvent : UnityEvent
    {
        public ADEvent() { }
        public ADEvent(UnityAction call) { this.AddListener(call); }

        public new void Invoke()
        {
            DebugExtenion.Log();
            base.Invoke();
        }
    }
    [Serializable]
    public class ADEvent<T1> : UnityEvent<T1>
    {
        public ADEvent() { }
        public ADEvent(UnityAction<T1> call) { this.AddListener(call); }

        public ADEvent Close(T1 a)
        {
            ADEvent result = new();
            result.AddListener(() => this.As<UnityEvent<T1>>().Invoke(a));
            return result;
        }

        public new void Invoke(T1 a)
        {
            DebugExtenion.Log();
            base.Invoke(a);
        }
    }
    [Serializable]
    public class ADEvent<T1, T2> : UnityEvent<T1, T2>
    {
        public ADEvent() { }
        public ADEvent(UnityAction<T1, T2> call) { this.AddListener(call); }

        public ADEvent Close(T1 a, T2 b)
        {
            ADEvent result = new();
            result.AddListener(() => this.As<UnityEvent<T1, T2>>().Invoke(a, b));
            return result;
        }

        public new void Invoke(T1 a, T2 b)
        {
            DebugExtenion.Log();
            base.Invoke(a, b);
        }
    }
    [Serializable]
    public class ADEvent<T1, T2, T3> : UnityEvent<T1, T2, T3>
    {
        public ADEvent() { }
        public ADEvent(UnityAction<T1, T2, T3> call) { this.AddListener(call); }

        public ADEvent Close(T1 a, T2 b, T3 c)
        {
            ADEvent result = new();
            result.AddListener(() => this.As<UnityEvent<T1, T2, T3>>().Invoke(a, b, c));
            return result;
        }

        public new void Invoke(T1 a, T2 b, T3 c)
        {
            DebugExtenion.Log();
            base.Invoke(a, b, c);
        }
    }
    [Serializable]
    public class ADEvent<T1, T2, T3, T4> : UnityEvent<T1, T2, T3, T4>
    {
        public ADEvent() { }
        public ADEvent(UnityAction<T1, T2, T3, T4> call) { this.AddListener(call); }

        public ADEvent Close(T1 a, T2 b, T3 c, T4 d)
        {
            ADEvent result = new();
            result.AddListener(() => this.As<UnityEvent<T1, T2, T3, T4>>().Invoke(a, b, c, d));
            return result;
        }

        public new void Invoke(T1 a, T2 b, T3 c, T4 d)
        {
            DebugExtenion.Log();
            base.Invoke(a, b, c, d);
        }
    }

    #endregion

    #region Property 

    public interface IPropertyHasGet<T, P> where P : PropertyAsset<T>, new()
    {
        AbstractBindProperty<T, P> Property { get; }
    }
    public interface IPropertyHasSet<T, P> where P : PropertyAsset<T>, new()
    {
        AbstractBindProperty<T, P> Property { get; }
    }

    public interface IPropertyHasGet<T> : IPropertyHasGet<T, PropertyAsset<T>>
    {

    }
    public interface IPropertyHasSet<T> : IPropertyHasGet<T, PropertyAsset<T>>
    {

    }

    public class PropertyAsset<T>
    {
        public PropertyAsset()
        {
        }
        public PropertyAsset(T from)
        {
            value = from;
        }

        public ADOrderlyEvent<T> OnDestory = null;
        public virtual T value { get; set; } = default;
    }

    public class Property<T, P> where P : PropertyAsset<T>, new()
    {
        internal ADOrderlyEvent _m_get = null;
        internal ADOrderlyEvent<T> _m_set = null;
        internal ADOrderlyEvent<T> _m_set_same = null;
        internal P _m_data = null;
        public bool IsHaveValue => _m_data != null;

        public Property(ADOrderlyEvent<T> set, T data)
        {
            _m_set = set ?? throw new ArgumentNullException(nameof(set));
            _m_data = new P();
            _m_data.value = data;
        }
        public Property(T data)
        {
            _m_data = new P();
            _m_data.value = data;
        }
        public Property()
        {
        }

        public T Set(T _Right)
        {
            if (_m_data != null)
            {
                if (_m_data.value.Equals(_Right))
                    _m_set_same?.Invoke(_Right);
                else
                    _m_set?.Invoke(_Right);
            }
            else
            {
                _m_data = new P();
                _m_set?.Invoke(_Right);
            }
            _m_data.value = _Right;
            return _Right;
        }

        public T Get()
        {
            return (IsHaveValue) ? _m_data.value : default;
        }

        public void Init()
        {
            _m_get = null;
            _m_set = null;
            _m_set_same = null;
            _m_data = null;
        }

        public void AddListenerOnSet(UnityAction<T> action)
        {
            _m_set ??= new ADOrderlyEvent<T>();
            _m_set.AddListener(action);
        }
        public void AddListenerOnSetSame(UnityAction<T> action)
        {
            _m_set_same ??= new ADOrderlyEvent<T>();
            _m_set_same.AddListener(action);
        }
        public void AddListenerOnGet(UnityAction action)
        {
            _m_get ??= new ADOrderlyEvent();
            _m_get.AddListener(action);
        }

        public void RemoveListenerOnSet(UnityAction<T> action)
        {
            _m_set?.RemoveListener(action);
        }
        public void RemoveListenerOnSetSame(UnityAction<T> action)
        {
            _m_set_same?.RemoveListener(action);
        }
        public void RemoveListenerOnGet(UnityAction action)
        {
            _m_get?.RemoveListener(action);
        }

        public void RemoveListenerOnSet()
        {
            _m_set = null;
        }
        public void RemoveListenerOnSetSame()
        {
            _m_set_same = null;
        }
        public void RemoveListenerOnGet()
        {
            _m_get = null;
        }

        public void SortOnSet(IComparer<int> comparer)
        {
            _m_set?.InvokeArray.Sort(comparer);
        }
        public void SortOnSetSame(IComparer<int> comparer)
        {
            _m_set_same?.InvokeArray.Sort(comparer);
        }
        public void SortOnGet(IComparer<int> comparer)
        {
            _m_get.InvokeArray.Sort(comparer);
        }

        public List<int> IndexArrayOnSet()
        {
            return _m_set?.InvokeArray;
        }
        public List<int> IndexArrayOnSetSame()
        {
            return _m_set_same?.InvokeArray;
        }
        public List<int> IndexArrayOnGet()
        {
            return _m_get?.InvokeArray;
        }

        public bool Equals(Property<T, P> _Right)
        {
            return Get().Equals(_Right.Get());
        }

        public override int GetHashCode()
        {
            return Get().GetHashCode();
        }

        public override string ToString()
        {
            return Get().ToString();
        }

        public int GetThisHashCode()
        {
            return base.GetHashCode();
        }

        public string ToThisString()
        {
            return base.ToString();
        }
    }

    public abstract class AbstractBindProperty<T, P> where P : PropertyAsset<T>, new()
    {
        internal Property<T, P> _m_value = new Property<T, P>();
        internal T value
        {
            get
            {
                return this._m_value.Get();
            }
            set
            {
                this._m_value.Set(value);
            }
        }

        protected void SetPropertyAsset(P asset)
        {
            _m_value._m_data = asset;
        }
        internal void _SetPropertyAsset(P asset)
        {
            _m_value._m_data = asset;
        }

        #region Func

        public AbstractBindProperty<T, P> Init()
        {
            _m_value.Init();
            return this;
        }

        public void TrackThisShared(AbstractBindProperty<T, P> OtherProperty)
        {
            this._m_value = OtherProperty._m_value;
        }

        protected void Init(T _init)
        {
            _m_value = new Property<T, P>(_init);
        }

        internal AbstractBindProperty<T, P> AddListenerOnSet(UnityAction<T> action)
        {
            _m_value.AddListenerOnSet(action);
            return this;
        }
        internal AbstractBindProperty<T, P> AddListenerOnSetSame(UnityAction<T> action)
        {
            _m_value.AddListenerOnSetSame(action);
            return this;
        }
        internal AbstractBindProperty<T, P> AddListenerOnGet(UnityAction action)
        {
            _m_value.AddListenerOnGet(action);
            return this;
        }

        internal AbstractBindProperty<T, P> RemoveListenerOnSet(UnityAction<T> action)
        {
            _m_value.RemoveListenerOnSet(action);
            return this;
        }
        internal AbstractBindProperty<T, P> RemoveListenerOnSetSame(UnityAction<T> action)
        {
            _m_value.RemoveListenerOnSetSame(action);
            return this;
        }
        internal AbstractBindProperty<T, P> RemoveListenerOnGet(UnityAction action)
        {
            _m_value.RemoveListenerOnGet(action);
            return this;
        }

        internal AbstractBindProperty<T, P> RemoveListenerOnSet()
        {
            _m_value.RemoveListenerOnSet();
            return this;
        }
        internal AbstractBindProperty<T, P> RemoveListenerOnSetSame()
        {
            _m_value.RemoveListenerOnSetSame();
            return this;
        }
        internal AbstractBindProperty<T, P> RemoveListenerOnGet()
        {
            _m_value.RemoveListenerOnGet();
            return this;
        }

        internal AbstractBindProperty<T, P> SortOnSet(IComparer<int> comparer)
        {
            _m_value.SortOnSet(comparer);
            return this;
        }
        internal AbstractBindProperty<T, P> SortOnSetSame(IComparer<int> comparer)
        {
            _m_value.SortOnSetSame(comparer);
            return this;
        }
        internal AbstractBindProperty<T, P> SortOnGet(IComparer<int> comparer)
        {
            _m_value.SortOnGet(comparer);
            return this;
        }

        internal List<int> IndexArrayOnSet()
        {
            return _m_value.IndexArrayOnSet();
        }
        internal List<int> IndexArrayOnSetSame()
        {
            return _m_value.IndexArrayOnSetSame();
        }
        internal List<int> IndexArrayOnGet()
        {
            return _m_value.IndexArrayOnGet();
        }

        #endregion

        public bool Equals(AbstractBindProperty<T, P> _Right)
        {
            if (!(_Right._m_value.IsHaveValue || this._m_value.IsHaveValue)) return true;
            else if (_Right._m_value.IsHaveValue != this._m_value.IsHaveValue) return false;
            else if (_Right._m_value.Get().Equals(this._m_value.Get())) return true;
            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return _m_value.Get().ToString();
        }

        public int GetPropertyHashCode()
        {
            return _m_value.GetHashCode();
        }

        public int GetValueHashCode()
        {
            return (_m_value.IsHaveValue) ? _m_value.Get().GetHashCode() : -1;
        }

        public string ToThisString()
        {
            return base.ToString();
        }

        public string ToPropertyString()
        {
            return _m_value.ToString();
        }
    }

    public class BindProperty<T, P> : AbstractBindProperty<T, P>, IPropertyHasGet<T, P>, IPropertyHasSet<T, P> where P : PropertyAsset<T>, new()
    {
        public AbstractBindProperty<T, P> Property => this;

        public BindPropertyJustGet<T, P> BindJustGet()
        {
            BindPropertyJustGet<T, P> get = new BindPropertyJustGet<T, P>();
            get.TrackThisShared(this);
            return get;
        }

        public BindPropertyJustSet<T, P> BindJustSet()
        {
            BindPropertyJustSet<T, P> get = new BindPropertyJustSet<T, P>();
            get.TrackThisShared(this);
            return get;
        }
    }

    public class BindPropertyJustGet<T, P> : AbstractBindProperty<T, P>, IPropertyHasGet<T, P> where P : PropertyAsset<T>, new()
    {
        public AbstractBindProperty<T, P> Property => this;
    }

    public class BindPropertyJustSet<T, P> : AbstractBindProperty<T, P>, IPropertyHasSet<T, P> where P : PropertyAsset<T>, new()
    {
        public AbstractBindProperty<T, P> Property => this;
    }

    public class BindProperty<T> : BindProperty<T, PropertyAsset<T>>
    {

    }

    public class BindPropertyJustGet<T> : BindPropertyJustGet<T, PropertyAsset<T>>
    {

    }

    public class BindPropertyJustSet<T> : BindPropertyJustSet<T, PropertyAsset<T>>
    {

    }

    public static class PropertyExtension
    {
        public static T GetOriginal<T, P>(this IPropertyHasGet<T, P> self) where P : PropertyAsset<T>, new()
        {
            return self.Property._m_value._m_data == null ? default : self.Property._m_value._m_data.value;
        }

        public static T SetOriginal<T, P>(this IPropertyHasSet<T, P> self, T value) where P : PropertyAsset<T>, new()
        {
            if (self.Property._m_value._m_data == null) self.Property._m_value._m_data = new();
            self.Property._m_value._m_data.value = value;
            return self.Property._m_value._m_data.value;
        }

        public static IPropertyHasSet<T, P> AddListenerOnSet<T, P>(this IPropertyHasSet<T, P> self, UnityAction<T> action) where P : PropertyAsset<T>, new()
        {
            self.Property.AddListenerOnSet(action);
            return self;
        }

        public static IPropertyHasSet<T, P> AddListenerOnSetSame<T, P>(this IPropertyHasSet<T, P> self, UnityAction<T> action) where P : PropertyAsset<T>, new()
        {
            self.Property.AddListenerOnSetSame(action);
            return self;
        }

        public static IPropertyHasGet<T, P> AddListenerOnGet<T, P>(this IPropertyHasGet<T, P> self, UnityAction action) where P : PropertyAsset<T>, new()
        {
            self.Property.AddListenerOnGet(action);
            return self;
        }

        public static IPropertyHasSet<T, P> RemoveListenerOnSet<T, P>(this IPropertyHasSet<T, P> self, UnityAction<T> action) where P : PropertyAsset<T>, new()
        {
            self.Property.RemoveListenerOnSet(action);
            return self;
        }

        public static IPropertyHasSet<T, P> RemoveListenerOnSetSame<T, P>(this IPropertyHasSet<T, P> self, UnityAction<T> action) where P : PropertyAsset<T>, new()
        {
            self.Property.RemoveListenerOnSetSame(action);
            return self;
        }

        public static IPropertyHasGet<T, P> RemoveListenerOnGet<T, P>(this IPropertyHasGet<T, P> self, UnityAction action) where P : PropertyAsset<T>, new()
        {
            self.Property.RemoveListenerOnGet(action);
            return self;
        }

        public static IPropertyHasSet<T, P> RemoveListenerOnSet<T, P>(this IPropertyHasSet<T, P> self) where P : PropertyAsset<T>, new()
        {
            self.RemoveListenerOnSet();
            return self;
        }

        public static IPropertyHasSet<T, P> RemoveListenerOnSetSame<T, P>(this IPropertyHasSet<T, P> self) where P : PropertyAsset<T>, new()
        {
            self.RemoveListenerOnSetSame();
            return self;
        }

        public static IPropertyHasGet<T, P> RemoveListenerOnGet<T, P>(this IPropertyHasGet<T, P> self) where P : PropertyAsset<T>, new()
        {
            self.RemoveListenerOnGet();
            return self;
        }

        public static IPropertyHasSet<T, P> SortOnSet<T, P>(this IPropertyHasSet<T, P> self, IComparer<int> comparer) where P : PropertyAsset<T>, new()
        {
            self.SortOnSet(comparer);
            return self;
        }

        public static IPropertyHasSet<T, P> SortOnSetSame<T, P>(this IPropertyHasSet<T, P> self, IComparer<int> comparer) where P : PropertyAsset<T>, new()
        {
            self.SortOnSetSame(comparer);
            return self;
        }

        public static IPropertyHasGet<T, P> SortOnGet<T, P>(this IPropertyHasGet<T, P> self, IComparer<int> comparer) where P : PropertyAsset<T>, new()
        {
            self.SortOnGet(comparer);
            return self;
        }

        public static List<int> IndexArrayOnSet<T, P>(this IPropertyHasSet<T, P> self) where P : PropertyAsset<T>, new()
        {
            return self.IndexArrayOnSet();
        }

        public static List<int> IndexArrayOnSetSame<T, P>(this IPropertyHasSet<T, P> self) where P : PropertyAsset<T>, new()
        {
            return self.IndexArrayOnSetSame();
        }

        public static List<int> IndexArrayOnGet<T, P>(this IPropertyHasGet<T, P> self) where P : PropertyAsset<T>, new()
        {
            return self.IndexArrayOnGet();
        }

        public static T Get<T, P>(this IPropertyHasGet<T, P> self) where P : PropertyAsset<T>, new()
        {
            return self.Property._m_value.Get();
        }

        public static T Set<T, P>(this IPropertyHasSet<T, P> self, T value) where P : PropertyAsset<T>, new()
        {
            return self.Property._m_value.Set(value);
        }

        public static void BindToValue(BindProperty<string> stringProperty, BindProperty<float> valueProperty)
        {
            stringProperty._SetPropertyAsset(new ValueToStringPropertyAsset(valueProperty._m_value._m_data));
        }

        public static void BindToValue(BindProperty<float> valueProperty, BindProperty<string> stringProperty)
        {
            valueProperty._SetPropertyAsset(new StringToValuePropertyAsset(stringProperty._m_value._m_data));
        }

        internal class ValueToStringPropertyAsset : PropertyAsset<string>
        {
            public ValueToStringPropertyAsset() { throw new ADException("ValueToStringPropertyAsset"); }
            public ValueToStringPropertyAsset(PropertyAsset<float> from)
            {
                this.from = from;
            }

            PropertyAsset<float> from;

            public override string value
            {
                get
                {
                    return from.value.ToString();
                }
                set
                {
                    if (float.TryParse(value, out float val))
                        from.value = val;
                    else from.value = 0;
                }
            }
        }

        internal class StringToValuePropertyAsset : PropertyAsset<float>
        {
            public StringToValuePropertyAsset() { throw new ADException("StringToValuePropertyAsset"); }
            public StringToValuePropertyAsset(PropertyAsset<string> from)
            {
                this.from = from;
            }

            PropertyAsset<string> from;

            public override float value
            {
                get
                {
                    if (float.TryParse(from.value, out float val))
                        return val;
                    else return 0;
                }
                set
                {
                    from.value = value.ToString();
                }
            }
        }
    }

    #endregion

    #region Extension

    public interface IInvariant<T> where T : class
    {

    }

    public static class ObjectExtension
    {
        public static Dictionary<Type, IADArchitecture> AllArchitecture = new();

        public static T As<T>(this object self) where T : class
        {
            if (self == null) throw new ADException("Now As._Left is null");
            if(self is not IInvariant<T>)
            {
                return self as T;
            }
            else
            {
                Debug.LogWarning($"you try to use an Invariant<{typeof(T).FullName}> by As");
                return null;
            }
        }

        public static bool As<T>(this object self, out T result) where T : class
        {
            if (self is not IInvariant<T> && self != null)
            {
                result = self as T;
                return result != null;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public static bool Convertible<T>(this object self) where T : class
        {
            if (self != null)
            {
                if (self is IInvariant<T>) return false;
                return self is T;
            }
            else return false;
        }

        public static bool Is<T>(this object self, out T result) where T : class
        {
            result = null;
            if (self is not IInvariant<T> && self is T r)
            {
                result = r;
                return true;
            }
            else return false;
        }

        public static bool IsAssignableFromOrSubClass(this Type self, Type target)
        {
            return self.IsAssignableFrom(target) || self.IsSubclassOf(target);
        }

        public enum ClassCorrelation
        {
            None, Base, Derived
        }

        public static ClassCorrelation DetectCorrelation(Type _Left, Type _Target)
        {
            if (_Left.IsAssignableFrom(_Target)) return ClassCorrelation.Base;
            else if (_Left.IsSubclassOf(_Target)) return ClassCorrelation.Derived;
            else return ClassCorrelation.None;
        }

        public static ClassCorrelation DetectCorrelation(object _Left, object _Target)
        {
            return DetectCorrelation(_Left.GetType(), _Target.GetType());
        }

        public static GameObject PrefabInstantiate(this GameObject self)
        {
            return GameObject.Instantiate(self);
        }

        public static T PrefabInstantiate<T>(this T self) where T : Component
        {
            return GameObject.Instantiate(self.gameObject).GetComponent<T>();
        }

        public static T PrefabInstantiate<T, _PrefabType>(this _PrefabType self) where T : Component where _PrefabType : Component
        {
            return GameObject.Instantiate(self.gameObject).GetComponent<T>();
        }

        public static T ObtainComponent<T>(this GameObject self, out T[] components) where T : class
        {
            components = self.GetComponents<T>();
            return components.Length > 0 ? components[0] : null;
        }

        public static T ObtainComponent<T>(this GameObject self) where T : class
        {
            var components = self.GetComponents<T>();
            return components.Length > 0 ? components[0] : null;
        }

        public static bool ObtainComponent<T>(this GameObject self, out T component) where T : class
        {
            var components = self.GetComponents<T>();
            component = components.Length > 0 ? components[0] : null;
            return component != null;
        }

        public static T Fetch<T>(this T self, out T me) where T : class
        {
            return me = self;
        }

        public static T Fetch<T, P>(this T self, out P me) where T : class where P : class
        {
            me = self as P;
            return self;
        }

        public static T Share<T>(this T self, out T shared)
        {
            return shared = self;
        }

        public static T SeekComponent<T>(this GameObject self) where T : class
        {
            foreach (var item in self.GetComponents<MonoBehaviour>())
            {
                if (item.As<T>(out var result))
                {
                    return result;
                }
            }
            return null;
        }

        public static T SeekComponent<T>(this Component self) where T : class
        {
            return SeekComponent<T>(self.gameObject);
        }
    }

    #endregion

    #region Event System Handler

    public interface IADEventSystemHandler : IEventSystemHandler
    {

    }

    public static class ADEventSystemExtension
    {
        public static void Execute<_IADArchitecture, _IADEventSystemHandler>
            (this _IADArchitecture architecture, GameObject target, BaseEventData data, ExecuteEvents.EventFunction<_IADEventSystemHandler> functor)
            where _IADArchitecture : IADArchitecture
            where _IADEventSystemHandler : IADEventSystemHandler
        {
            try
            {
                ExecuteEvents.Execute<_IADEventSystemHandler>(target, data, functor);
            }
            catch (ADException ex)
            {
                architecture.AddMessage(" [ AD Error Message ] " + ex.SerializeMessage());
                architecture.AddMessage(" [ AD Error Stack ] " + ex.SerializeStackTrace());
                DebugExtenion.Log();
            }
            catch (Exception ex)
            {
                architecture.AddMessage(" [ Unknow Error Message ] " + ex.Message);
                architecture.AddMessage(" [ Unknow Error Stack ] " + ex.StackTrace);
                DebugExtenion.Log();
            }
        }

        public static void Execute<_IADEventSystemHandler>
            (GameObject target, BaseEventData data, ExecuteEvents.EventFunction<_IADEventSystemHandler> functor)
            where _IADEventSystemHandler : IADEventSystemHandler
        {
            try
            {
                ExecuteEvents.Execute<_IADEventSystemHandler>(target, data, functor);
            }
            catch
            {
                DebugExtenion.Log();
                throw;
            }
        }
    }

    #endregion

    #region Debug

    public static class DebugExtenion
    {
        public static string LogPath = Path.Combine(Application.persistentDataPath, "Debug.dat");

        public static bool LogMethodEnabled = true;

        public static string[] FilterdName = new string[] { "GetStackTraceModelName" };

        static DebugExtenion()
        {
            ADFile file = new(LogPath, true, false, false, false);
            file.Dispose();
            Application.logMessageReceived += LogHandler;
        }

        private static void LogHandler(string logString, string stackTrace, LogType type)
        {
            try
            {
                using StreamWriter sws = new(LogPath, true, System.Text.Encoding.UTF8);
                sws.WriteLine("{");
                sws.WriteLine("[time]:" + DateTime.Now.ToString());
                sws.WriteLine("[type]:" + type.ToString());
                sws.WriteLine("[exception message]:" + logString);
                sws.WriteLine("[stack trace]:\n" + stackTrace + "}");
            }
            catch { }
        }

        public static void Log()
        {
            try
            {
#if UNITY_EDITOR
                if (LogMethodEnabled)
                {
                    using StreamWriter sws = new(LogPath, true, System.Text.Encoding.UTF8);
                    var temp = GetStackTraceModelName();
                    if (temp[^1] == nameof(Log))
                    {
                        var newcat = temp.SubArray(0, temp.Length - 1);
                    }
                    sws.WriteLine(System.DateTime.Now.ToString() + " :\n" + temp.LinkAndInsert('\t'));
                }
#endif
            }
            catch { }
        }

        public static void LogMessage(string message)
        {
            try
            {
#if UNITY_EDITOR
                if (LogMethodEnabled)
                {
                    using StreamWriter sws = new(LogPath, true, System.Text.Encoding.UTF8);
                    var temp = GetStackTraceModelName();
                    if (temp[^1] == nameof(LogMessage))
                    {
                        var newcat = temp.SubArray(0, temp.Length - 1);
                    }
                    sws.WriteLine(System.DateTime.Now.ToString() + " : " + message + " :\n" + temp.LinkAndInsert('\t'));
                }
#else
                if (LogMethodEnabled)
                {
                    using StreamWriter sws = new(LogPath, true, System.Text.Encoding.UTF8);
                    sws.WriteLine(System.DateTime.Now.ToString() + " : " + message);
                }
#endif
            }
            catch { }
        }

        public static string[] GetStackTraceModelName()
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
            System.Diagnostics.StackFrame[] sfs = st.GetFrames();
            List<string> result = new();
            for (int i = sfs.Length - 1; i >= 0; i--)
            {
                //非用户代码,系统方法及后面的都是系统调用，不获取用户代码调用结束
                if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == sfs[i].GetILOffset()) continue;

                string _methodName = sfs[i].GetMethod().Name;
                if (FilterdName.Contains(_methodName)) continue;
                result.Add(_methodName + "%" + sfs[i].GetFileName() + ":" + sfs[i].GetFileLineNumber());
            }
            return result.ToArray();
        }
        public static string[] GetStackTraceModelName(int depth)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
            System.Diagnostics.StackFrame[] sfs = st.GetFrames();
            List<string> result = new();
            for (int i = sfs.Length - 1; i >= 0; i--)
            {
                //非用户代码,系统方法及后面的都是系统调用，不获取用户代码调用结束
                if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == sfs[i].GetILOffset()) continue;
                if (depth-- <= 0) break;

                string _methodName = sfs[i].GetMethod().Name;
                if (FilterdName.Contains(_methodName)) continue;
                result.Add(_methodName + "%" + sfs[i].GetFileName() + ":" + sfs[i].GetFileLineNumber());
            }
            return result.ToArray();
        }
    }

    #endregion

    #region ProcessStart

    /// <summary>
    /// PC Window
    /// </summary>
    public static class WindowProcessExtenion
    {
        public static void StartProcess(string path, string Arguments)
        {
            var pro = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    Arguments = Arguments
                }
            };
            pro.Start();
        }

        public static void ReStart()
        {
            string[] strs = new string[]
             {
            "@echo off",
            "echo wscript.sleep 1000 > sleep.vbs",
            "start /wait sleep.vbs",
            "start /d \"{0}\" {1}",
            "del /f /s /q sleep.vbs",
            "exit"
             };
            string path = Application.dataPath + "/../";


            List<string> prefabs = new List<string>(Directory.GetFiles(Application.dataPath + "/../", "*.exe", SearchOption.AllDirectories));
            foreach (string keyx in prefabs)
            {
                string _path = Application.dataPath;
                _path = _path.Remove(_path.LastIndexOf("/")) + "/";
                Debug.LogError(_path);
                string _name = Path.GetFileName(keyx);
                strs[3] = string.Format(strs[3], _path, _name);
                Application.OpenURL(path);
            }

            string batPath = Application.dataPath + "/../restart.bat";
            if (File.Exists(batPath))
            {
                File.Delete(batPath);
            }
            using (FileStream fileStream = File.OpenWrite(batPath))
            {
                using StreamWriter writer = new(fileStream, System.Text.Encoding.GetEncoding("UTF-8"));
                foreach (string s in strs)
                {
                    writer.WriteLine(s);
                }
                writer.Close();
            }
            Application.Quit();
            Application.OpenURL(batPath);

        }
    }

    #endregion
}

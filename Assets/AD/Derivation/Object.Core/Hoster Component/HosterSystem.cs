using System;
using System.Collections.Generic;
using AD.Utility.Object;
using AD.Experimental.GameEditor;
using AD.BASE;
using AD.Utility;
using System.Collections;
using AD.Experimental.HosterSystem.Diagram;
using System.Linq;

namespace AD.Experimental.HosterSystem
{
    public static class HosterExtension
    {
        internal static Dictionary<Type, IHosterTag> StaticTags = new();

        public static T AddOrGetHosterComponent<T>(this IMainHoster self) where T : IHosterComponent, new()
        {
            return self.GetHosterComponent<T>() ?? self.AddHosterComponent<T>();
        }
        public static List<IHosterComponent> GetHosterComponents<Key>(this IMainHoster self) where Key : IHosterTag, new()
        {
            List<IHosterComponent> result = new();
            if (StaticTags.ContainsKey(typeof(Key)))
            {
                IHosterTag tag = StaticTags[typeof(Key)];
                foreach (var item in self.HosterComponents)
                {
                    if (item.Key == tag) result.Add(item.Value);
                }
            }
            return result;
        }
    }

    public class HosterSystem : MonoSystem, IMainHoster
    {
        #region HosterSystem

        public static Dictionary<Type, Type> StaticTags = new();

        /// <summary>
        /// 从这里获取Key单例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
		public static IHosterTag ObtainKey<T>() where T : IHosterComponent, new()
        {
            if (StaticTags.TryGetValue(typeof(T), out Type type)) return HosterExtension.StaticTags[type];
            else return null;
        }

        /// <summary>
        /// 从这里获取Key单例
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
		public static IHosterTag ObtainKey(Type T)
        {
            if (StaticTags.TryGetValue(T, out Type type)) return HosterExtension.StaticTags[type];
            else return null;
        }

        /// <summary>
        /// 不能撤销的调用，需要取消请自行操作StaticTags
        /// <para>一般这是不允许重新设置的</para>
        /// </summary>
        /// <typeparam name="T">HosterComponent类型</typeparam>
        /// <typeparam name="Key">Tag类型</typeparam>
        public static void RegisterKey<T, Key>() where T : IHosterComponent, new() where Key : IHosterTag, new()
        {
            if (StaticTags.TryAdd(typeof(T), typeof(Key)))
            {
                HosterExtension.StaticTags.Add(typeof(Key), new Key());
            }
        }

        /// <summary>
        /// 继承并在初始化函数中注册Key
        /// </summary>
        public override void Init()
        {
            this.StartCoroutine(MakeInit());
            RegisterKey<TransformDiagram, TransformDiagramKey>();
            RegisterKey<AddDiagram, AddDiagramKey>();
        }

        #endregion

        #region IMainHoster Assets

        public Dictionary<IHosterTag, IHosterComponent> HosterComponents { get; set; } = new();

        public List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; } = new();
        public bool IsOpenListView { get; set; } = false;
        public ISerializeHierarchyEditor MatchHierarchyEditor { get => this; set { throw new ADException("Not Support"); } }

        public HierarchyItem MatchItem
        {
            get => MatchItems.Count == 0 ? null : MatchItems[0];
            set
            {
                if (MatchItems.Count == 0) MatchItems.Add(value);
                else MatchItems[0] = value;
            }
        }
        public List<HierarchyItem> MatchItems { get; private set; } = new();
        public ICanSerializeOnCustomEditor MatchTarget => this;
        public ICanSerializeOnCustomEditor ParentTarget { get; set; }

        #region 注意，这些序号需要使用接口来获取确定的属性

        public int SerializeIndex { get => -100000; set { } }
        int ICanSerializeOnCustomEditor.SerializeIndex => -100000;
        int ICanSerialize.SerializeIndex { get => -100000; set { } }

        #endregion

        #endregion

        #region Resources

        private List<HosterBase> childs = new();

        #endregion

        #region IMainHoster Func

        public T AddHosterComponent<T>() where T : IHosterComponent, new()
        {
            if (HosterComponents.TryGetValue(HosterSystem.ObtainKey<T>(), out var component)) return (T)component;
            else
            {
                T temp = new T();
                temp.SetParent(this);
                HosterComponents.Add(HosterSystem.ObtainKey<T>(), temp);
                MatchPropertiesEditors.Add(temp);
                return temp;
            }
        }

        public int AddHosterComponents(params Type[] components)
        {
            int count = 0;
            foreach (var componentType in components)
            {
                if (!HosterComponents.TryGetValue(HosterSystem.ObtainKey(componentType), out var component))
                {
                    IHosterComponent temp = componentType.CreateInstance<IHosterComponent>();

                    temp.SetParent(this);
                    HosterComponents.Add(HosterSystem.ObtainKey(componentType), temp);
                    MatchPropertiesEditors.Add(temp);
                    count++;
                }
            }
            return count;
        }

        public virtual void ClickOnLeft() { }

        public virtual void ClickOnRight() { }

        public virtual void DoUpdate() { }

        public List<ICanSerializeOnCustomEditor> GetChilds()
        {
            return childs.GetSubList<HosterBase, ICanSerializeOnCustomEditor>();
        }

        public T GetHosterComponent<T>() where T : IHosterComponent, new()
        {
            if (HosterComponents.TryGetValue(HosterSystem.ObtainKey<T>(), out var component)) return (T)component;
            else return default;
        }

        public void OnSerialize(HierarchyItem MatchItem)
        {
            MatchItem.SetTitle("Root(Base Group)");
        }

        public bool RemoveHosterComponent<T>() where T : IHosterComponent, new()
        {
            if (this.HosterComponents.ContainsKey(HosterSystem.ObtainKey<T>()))
            {
                if (!MatchPropertiesEditors.Remove(HosterComponents[HosterSystem.ObtainKey<T>()])) return false;
                if (HosterComponents.Remove(HosterSystem.ObtainKey<T>())) return true;
                foreach (var component in HosterComponents)
                {
                    component.Value.Enable = false;
                }
            }
            return false;
        }

        public bool RemoveHosterComponentByKey<Key>() where Key : IHosterTag, new()
        {
            if (this.HosterComponents.ContainsKey(HosterExtension.StaticTags[typeof(Key)]))
            {
                if (!MatchPropertiesEditors.Remove(HosterComponents[HosterExtension.StaticTags[typeof(Key)]])) return false;
                if (HosterComponents.Remove(HosterExtension.StaticTags[typeof(Key)])) return true;
                foreach (var component in HosterComponents)
                {
                    component.Value.Enable = false;
                }
            }
            return false;
        }

        #endregion

        #region Diagram

        protected AddDiagram AddDiagramMenu => HosterComponents[HosterSystem.ObtainKey<AddDiagram>()] as AddDiagram;

        #endregion

        protected virtual void Start()
        {
            GameEditorApp.instance.RegisterSystem<HosterSystem>(this);
            AddHosterComponent<TransformDiagram>();
            AddHosterComponent<AddDiagram>();
        }

        private IEnumerator MakeInit()
        {
            yield return null;
            Architecture.GetController<Hierarchy>().AddOnTop(this);
        }

        public void Update()
        {
            DoUpdate();
            foreach (var component in HosterComponents)
            {
                if (component.Value.Enable) component.Value.DoUpdate();
            }
            foreach (var child in childs)
            {
                child.Update();
            }
        }
    }

    /// <summary>
    /// 主核接口
    /// </summary>
    public interface IBaseHoster
    {
        void DoUpdate();
    }
    /// <summary>
    /// 仅作为托管槽标识符
    /// </summary>
    public interface IHosterTag { }
    /// <summary>
    /// 将自身作为左侧面板展示器，同时作为托管对象（主类型）
    /// </summary>
    public interface IMainHoster : ISerializeHierarchyEditor, ICanSerializeOnCustomEditor, IBaseHoster
    {
        /// <summary>
        /// 需要使用HosterSystem.ObtainKey来获取注册用的TagKey
        /// </summary>
        Dictionary<IHosterTag, IHosterComponent> HosterComponents { get; }

        /// <summary>
        /// 添加类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T AddHosterComponent<T>() where T : IHosterComponent, new();

        /// <summary>
        /// 获取类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetHosterComponent<T>() where T : IHosterComponent, new();

        /// <summary>
        /// 通过类型数组添加类型
        /// </summary>
        /// <param name="components"></param>
        /// <returns>成功的数量</returns>
        int AddHosterComponents(params Type[] components);

        /// <summary>
        /// 通过类型清除托管组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool RemoveHosterComponent<T>() where T : IHosterComponent, new();

        /// <summary>
        /// 通过Key清除托管组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool RemoveHosterComponentByKey<Key>() where Key : IHosterTag, new();

    }
    /// <summary>
    /// 将自身作为右侧面板展示器，同时作为托管对象的托管组件（托管对象（附））
    /// </summary>
    public interface IHosterComponent : ISerializePropertiesEditor, IBaseHoster
    {
        IMainHoster Parent { get; }
        bool Enable { get; set; }
        void DoSetup();
        void DoCleanup();
        void SetParent(IMainHoster Parent);
    }

    /// <summary>
    /// 继承 IMainHoster ( ISerializeHierarchyEditor, ICanSerializeOnCustomEditor, IBaseHoster )
    /// </summary>
    public abstract class HosterBase : IMainHoster
    {
        #region EditGroup

        public virtual EditGroup EditGroup { get; protected set; }

        public ColliderLayer SubColliderLayer => EditGroup.ColliderLayer;
        public ViewLayer ViewLayer => EditGroup.ViewLayer;

        #endregion

        #region IMainHoster Assets

        public Dictionary<IHosterTag, IHosterComponent> HosterComponents { get; set; } = new();
        public List<IHosterComponent> NoMatchHosterComponents = new();

        public List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; } = new();
        public bool IsOpenListView { get; set; } = false;
        public ISerializeHierarchyEditor MatchHierarchyEditor { get => this; set { throw new ADException("Not Support"); } }

        public HierarchyItem MatchItem
        {
            get => MatchItems.Count == 0 ? null : MatchItems[0];
            set
            {
                if (MatchItems.Count == 0) MatchItems.Add(value);
                else MatchItems[0] = value;
            }
        }
        public List<HierarchyItem> MatchItems { get; private set; } = new();
        public ICanSerializeOnCustomEditor MatchTarget => this;
        public ICanSerializeOnCustomEditor ParentTarget { get; set; }

        #region 注意，这些序号需要使用接口来获取确定的属性

        public virtual int SerializeIndex { get; set; }
        int ICanSerializeOnCustomEditor.SerializeIndex => this.SerializeIndex;
        int ICanSerialize.SerializeIndex { get; set; }

        #endregion

        private List<HosterBase> childs = new();

        #endregion

        #region IMainHoster Func

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>如果槽位已有对象，返回该对象，否则新生成对象并返回</returns>
        public T AddHosterComponent<T>() where T : IHosterComponent, new()
        {
            var tag_key = HosterSystem.ObtainKey<T>();
            //存在Tag
            //存在Tag且已经存在组件，有两种情况，一种是T就是目标类型
            //另一种则是T是基类，这种情况下Tag应该也是多类同时使用的
            if (tag_key != null && HosterComponents.TryGetValue(tag_key, out var component)) return (T)component;
            //否则不限制槽位
            return DoAdd(tag_key);

            T DoAdd(IHosterTag tag_key)
            {
                T temp = new T();
                temp.SetParent(this);
                if (tag_key != null) HosterComponents.Add(tag_key, temp);
                else NoMatchHosterComponents.Add(temp);
                MatchPropertiesEditors.Add(temp);
                temp.DoSetup();
                return temp;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="components"></param>
        /// <returns>成功添加的数量</returns>
        public int AddHosterComponents(params Type[] components)
        {
            int count = 0;
            foreach (var componentType in components)
            {
                var tag_key = HosterSystem.ObtainKey(componentType);
                if (tag_key != null)
                {
                    if (!HosterComponents.TryGetValue(tag_key, out var component))
                    {
                        count++;
                        DoAdd(componentType, tag_key);
                    }
                }
                else
                {
                    count++;
                    DoAdd(componentType, tag_key);
                }
            }
            return count;

            void DoAdd(Type componentType, IHosterTag tag_key)
            {
                IHosterComponent temp = componentType.CreateInstance<IHosterComponent>();

                temp.SetParent(this);
                if (tag_key != null) HosterComponents.Add(tag_key, temp);
                else NoMatchHosterComponents.Add(temp);
                MatchPropertiesEditors.Add(temp);
                temp.DoSetup();
            }
        }

        public virtual void ClickOnLeft() { }

        public virtual void ClickOnRight() { }

        public virtual void DoUpdate() { }

        public List<ICanSerializeOnCustomEditor> GetChilds()
        {
            return childs.GetSubList<HosterBase, ICanSerializeOnCustomEditor>();
        }

        public T GetHosterComponent<T>() where T : IHosterComponent, new()
        {
            if (HosterComponents.TryGetValue(HosterSystem.ObtainKey<T>(), out var component)) return (T)component;
            else return default;
        }

        public abstract void OnSerialize(HierarchyItem MatchItem);

        public bool RemoveHosterComponent<T>() where T : IHosterComponent, new()
        {
            try
            {
                IHosterTag key = HosterSystem.ObtainKey<T>();
                if (key != null)
                {
                    if (this.HosterComponents.ContainsKey(key))
                    {
                        MatchPropertiesEditors.Remove(HosterComponents[key]);
                        HosterComponents[key].DoCleanup();
                        HosterComponents.Remove(key);
                        return true;
                    }
                }
                else
                {
                    NoMatchHosterComponents.RemoveAll(T => T.GetType() == typeof(T));
                    this.MatchPropertiesEditors.RemoveAll(T => T.GetType() == typeof(T));
                }
            }
            catch
            {
                foreach (var component in HosterComponents)
                {
                    component.Value.Enable = false;
                }
                ADGlobalSystem.ThrowLogicError("RemoveHosterComponent");
            }
            return false;
        }

        /// <summary>
        /// 补充函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool RemoveHosterComponent<T>(T target) where T : IHosterComponent, new()
        {
            try
            {
                IHosterTag key = HosterSystem.ObtainKey<T>();
                if (key != null)
                {
                    if (this.HosterComponents.ContainsKey(key))
                    {
                        MatchPropertiesEditors.Remove(HosterComponents[key]);
                        HosterComponents[key].DoCleanup();
                        HosterComponents.Remove(key);
                        return true;
                    }
                }
                else
                {
                    if (target != null)
                    {
                        NoMatchHosterComponents.Remove(target);
                        this.MatchPropertiesEditors.Remove(target);
                    }
                }
            }
            catch
            {
                foreach (var component in HosterComponents)
                {
                    component.Value.Enable = false;
                }
                ADGlobalSystem.ThrowLogicError("RemoveHosterComponent");
            }
            return false;
        }

        public bool RemoveHosterComponentByKey<Key>() where Key : IHosterTag, new()
        {
            IHosterTag tag_key = HosterExtension.StaticTags[typeof(Key)];
            if (tag_key == null) return false;
            if (this.HosterComponents.ContainsKey(tag_key))
            {
                MatchPropertiesEditors.Remove(HosterComponents[tag_key]);
                HosterComponents[tag_key].DoCleanup();
                HosterComponents.Remove(tag_key);
            }
            return false;
        }

        #endregion

        #region Diagram

        protected AddDiagram AddDiagramMenu => HosterComponents[HosterSystem.ObtainKey<AddDiagram>()] as AddDiagram;

        #endregion

        public HosterBase()
        {
            AddHosterComponent<AddDiagram>();
        }

        public void Update()
        {
            DoUpdate();
            foreach (var component in HosterComponents)
            {
                if (component.Value.Enable) component.Value.DoUpdate();
            }
            foreach (var component in NoMatchHosterComponents)
            {
                if (component.Enable) component.DoUpdate();
            }
            foreach (var child in childs)
            {
                child.Update();
            }
        }
    }

    /// <summary>
    /// Diagram组件的基本实现，是PropertiesBlock（Properties面板控件）与IHosterComponent的实现
    /// </summary>
    public abstract class BaseDiagram : IHosterComponent
    {
        public IMainHoster Parent { get; set; }

        public bool Enable { get; set; }
        public PropertiesItem MatchItem { get; set; }

        public ICanSerializeOnCustomEditor MatchTarget => Parent;

        public abstract bool IsDirty { get; set; }
        public abstract int SerializeIndex { get; set; }

        public virtual void DoCleanup() { }

        public virtual void DoSetup() { }

        public virtual void DoUpdate() { }

        public virtual void OnSerialize() { }

        #region Command

        public virtual void SetParent(IMainHoster Parent)
        {
            this.Parent = Parent;
        }

        #endregion
    }

    /// <summary>
    /// Diagram组件的扩展实现，是PropertiesBlock（Properties面板控件）与IHosterComponent的实现
    /// </summary>
    public abstract class BaseDiagram<T> : PropertiesBlock<T>, IHosterComponent where T : class, ICanSerializeOnCustomEditor
    {
        protected BaseDiagram(string layer, int index = 0) : base(layer, index)
        {
        }

        public IMainHoster Parent { get; set; }

        public bool Enable { get; set; }

        public virtual void DoCleanup() { }

        public virtual void DoSetup() { }

        public virtual void DoUpdate() { }

        #region Command

        public virtual void SetParent(IMainHoster Parent)
        {
            this.Parent = Parent;
        }

        #endregion
    }
}

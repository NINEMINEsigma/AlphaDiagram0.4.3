using System;
using System.Collections;
using System.Collections.Generic;
using AD.Utility.Object;
using UnityEngine;
using AD.Experimental.GameEditor;
using AD.BASE;

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

	public class HosterSystem : MonoSystem
	{
		public static Dictionary<Type, Type> StaticTags = new();

		public static IHosterTag ObtainKey<Key>() where Key : IHosterTag, new()
		{
			if (!HosterExtension.StaticTags.ContainsKey(typeof(Key))) HosterExtension.StaticTags.Add(typeof(Key), new Key());
			return HosterExtension.StaticTags[typeof(Key)];
		}

		/// <summary>
		/// 不能撤销的调用，需要取消请自行操作StaticTags
		/// <para>一般这是不允许重新设置的</para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="Key"></typeparam>
		public static void RegisterKey<T, Key>() where T : IHosterComponent, new() where Key : IHosterTag, new()
		{
			StaticTags.TryAdd(typeof(T), typeof(Key));
		}

        protected virtual void Start()
        {
			GameEditorApp.instance.RegisterSystem(this);
        }

        public override void Init()
		{

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
	}

    /// <summary>
    /// 继承 IMainHoster ( ISerializeHierarchyEditor, ICanSerializeOnCustomEditor, IBaseHoster )
    /// </summary>
    public abstract class HosterBase: IMainHoster
    {
        #region EditGroup

        public virtual EditGroup EditGroup { get; protected set; }

		public ColliderLayer SubColliderLayer => EditGroup.ColliderLayer;
		public ViewLayer ViewLayer => EditGroup.ViewLayer;

		#endregion

		#region IMainHoster Assets

		public Dictionary<IHosterTag, IHosterComponent> HosterComponents { get; set; } = new();

		public List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; } = new();
		public bool IsOpenListView { get; set; } = false;
		public ISerializeHierarchyEditor MatchHierarchyEditor { get => this; set { throw new ADException("Not Support"); } }

        public HierarchyItem MatchItem { get; set; }
		public ICanSerializeOnCustomEditor MatchTarget => this;
        public ICanSerializeOnCustomEditor ParentTarget { get; set; }

        #region 注意，这些序号需要使用接口来获取确定的属性

        public int SerializeIndex { get; set; }
		int ICanSerializeOnCustomEditor.SerializeIndex => this.SerializeIndex;
        int ICanSerialize.SerializeIndex { get; set; }

        #endregion

        #endregion

        #region IMainHoster Func

        public T AddHosterComponent<T>() where T : IHosterComponent, new()
        {
            throw new NotImplementedException();
        }

        public int AddHosterComponents(params Type[] components)
        {
            throw new NotImplementedException();
        }

        public void ClickOnLeft()
        {
            throw new NotImplementedException();
        }

        public void ClickOnRight()
        {
            throw new NotImplementedException();
        }

        public void DoUpdate()
        {
            throw new NotImplementedException();
        }

        public List<ICanSerializeOnCustomEditor> GetChilds()
        {
            throw new NotImplementedException();
        }

        public T GetHosterComponent<T>() where T : IHosterComponent, new()
        {
            throw new NotImplementedException();
        }

        public void OnSerialize()
        {
            throw new NotImplementedException();
        }

        public bool RemoveHosterComponent<T>() where T : IHosterComponent, new()
        {
            throw new NotImplementedException();
        }

        public bool RemoveHosterComponentByKey<Key>() where Key : IHosterTag, new()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}

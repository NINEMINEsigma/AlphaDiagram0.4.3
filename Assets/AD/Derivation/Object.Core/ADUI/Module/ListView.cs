using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AD.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AD.UI
{
    public abstract class ListViewItem : PropertyModule,IComparable<ListViewItem>
    {

        public ListViewItem()
        {
            this.ElementArea = nameof(ListViewItem);

        }

        public abstract ListViewItem Init();

        public virtual int CompareTo(ListViewItem other)
        {
            return this.SortIndex.CompareTo(other.SortIndex);
        }

        public virtual int SortIndex{ get; set; }
    }

    public class ListView : PropertyModule
    {
        public ListView()
        {
            this.ElementArea = nameof(ListView);
        }

        public static readonly string ListViewDefaultPerfabSpawnKey = "ListViewDefault";
        public static string LVDPSK => ListViewDefaultPerfabSpawnKey;

        [Header("ListView")]
        [SerializeField] private ScrollRect _Scroll;
        [SerializeField] private VerticalLayoutGroup _List;
        [SerializeField] private TMP_Text _Title;
        [SerializeField] private ListViewItem Prefab;
        [SerializeField] private int index = 0;

        public static Comparison<Transform> StaticSortChildComparison = null;
        public Comparison<Transform> SortChildComparison = null;

        public bool IsNeedHoldMyself = false;//because it sometime used as a sub-object hider when start

        public ScrollRect.ScrollRectEvent onValueChanged
        {
            get => _Scroll.onValueChanged;
            set => _Scroll.onValueChanged = value;
        }

        protected override void Start()
        {
            base.Start();
            //this.Init();
            if (IsNeedHoldMyself) this.gameObject.SetActive(false);
        }

        public void Init()
        {
            index = 0;
            Childs = new();
            Pool = new();
        }

        public void SetTitle(string title)
        {
            _Title.text = title;
        }
        public void SetPrefab(ListViewItem prefab)
        {
            Prefab = prefab;
        }

        public ListViewItem GenerateItem()
        {
            if (Prefab == null) return null;
            GameObject item  = Spawn(ListViewDefaultPerfabSpawnKey, Prefab.gameObject);
            this[index] = item;
            item.GetComponent<ListViewItem>().SortIndex = index++;
            return item.GetComponent<ListViewItem>().Init();
        }

        protected override void LetChildDestroy(GameObject child)
        {
            Despawn(ListViewDefaultPerfabSpawnKey, child);
        }

        protected override void LetChildAdd(GameObject child)
        {
            child.transform.SetParent(_List.gameObject.transform, false);
        }

        public void SortChilds()
        {
            if (Childs.Count < 2) return;
            if (SortChildComparison != null) _List.gameObject.SortChilds(SortChildComparison);
            else if (StaticSortChildComparison != null) _List.gameObject.SortChilds(StaticSortChildComparison);
            else _List.gameObject.SortChildComponentTransform<ListViewItem>();
        }

        public GameObject FindItem(int index)
        {
            return this[index];
        }

        public void Clear()
        {
            foreach (var child in Childs)
            {
                LetChildDestroy(child.Value);
            }
            Childs.Clear();
            /*List<int> indexs = new();
            indexs.AddRange(from KeyValuePair<int, GameObject> item in Childs
                            select item.Key);
            foreach (int index in indexs)
            {
                Remove(index);
            }
            */
        }
    }
}

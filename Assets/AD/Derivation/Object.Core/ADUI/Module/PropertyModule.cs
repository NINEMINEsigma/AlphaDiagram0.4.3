using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace AD.UI
{
    public abstract class PropertyModule : ADUI
    {
        [Header("PropertyModule")]
        public Dictionary<int, GameObject> Childs = new();
        private GridLayoutGroup _GridLayoutGroup = null;
        public virtual GridLayoutGroup GridLayoutGroup => IsNeedLayoutGourp ? _GridLayoutGroup : null;

        public PropertyModule()
        {
            ElementArea = "PropertyModule";
        }

        protected virtual void Start()
        {
            ADUI.Initialize(this); 
        }
        protected virtual void OnDestroy()
        {
            ADUI.Destory(this);
        }

        private bool _IsNeedLayoutGourp = false;
        protected virtual GridLayoutGroup HowGetOrAddGridLayoutGroup()
        {
            if (_GridLayoutGroup == null) return gameObject.GetOrAddComponent<GridLayoutGroup>();
            else return _GridLayoutGroup;
        }
        protected virtual bool IsNeedLayoutGourp
        {
            get
            {
                if (_IsNeedLayoutGourp)
                {
                    _GridLayoutGroup = HowGetOrAddGridLayoutGroup();
                    _GridLayoutGroup.enabled = true;
                }
                return _IsNeedLayoutGourp;
            }
            set
            {
                if (value)
                {
                    _GridLayoutGroup = HowGetOrAddGridLayoutGroup();
                    _GridLayoutGroup.enabled = true;
                }
                else if (_GridLayoutGroup != null) _GridLayoutGroup.enabled = false;
                _IsNeedLayoutGourp = value;
            }
        }

        public GameObject this[int index]
        {
            get
            {
                return Childs[index];
            }
            set
            {
                Add(index, value);
            }
        }

        public void Add(int key, GameObject child)
        {
            if (Childs.ContainsKey(key))
                LetChildDestroy(Childs[key]);
            if (child != null)
            {
                Childs[key] = child;
                LetChildAdd(child);
            }
        }

        protected virtual void LetChildAdd(GameObject child)
        {
            child.transform.SetParent(transform, false);
        }

        protected virtual void LetChildDestroy(GameObject child)
        {
            GameObject.Destroy(child);
        }

        public void Remove(int index)
        {
            if (Childs.ContainsKey(index))
                LetChildDestroy(Childs[index]);
            Childs.Remove(index);
        }

        public void Remove(GameObject target)
        {
            var result = Childs.FirstOrDefault(T => T.Value == target);
            Remove(result.Key);
            GameObject.Destroy(result.Value);
        }

        protected Dictionary<string, Queue<GameObject>> Pool = new();

        protected virtual GameObject Spawn(string key, GameObject perfab)
        {
            if (Pool.ContainsKey(key) && Pool[key].Count != 0)
            {
                var cat = Pool[key].Dequeue();
                cat.SetActive(true);
                return cat;
            }
            else
            {
                return EmptyAdd(key, perfab);
            }

            GameObject EmptyAdd(string key, GameObject perfab)
            {
                Pool.TryAdd(key, new());
                return GameObject.Instantiate(perfab);
            }
        }

        protected virtual GameObject Spawn(string key, GameObject perfab,Transform parent)
        {
            GameObject target = Spawn(key, perfab);
            target.transform.SetParent(parent);
            return target;
        }

        protected virtual void Despawn(string key, GameObject target)
        {
            if (Pool.ContainsKey(key))
            {
                Pool[key].Enqueue(target);
                target.SetActive(false);
            }
            else
            {
                Debug.LogAssertion("A GameoObject is not regist but try despawn", this);
            }
        } 
    }
}

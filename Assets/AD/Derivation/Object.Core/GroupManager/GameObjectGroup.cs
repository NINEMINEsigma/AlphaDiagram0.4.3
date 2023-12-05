using System.Collections.Generic;
using UnityEngine;

namespace AD.Utility.Object
{
    public abstract class TGroup<T> : MonoBehaviour, ITGroup where T : Component
    {
        public List<T> group;

        public void Select(int index)
        {
            for (int i = 0; i < group.Count; i++)
            {
                var current = group[i];
                current.gameObject.SetActive(i == index);
            }
        }

        public void TrySelect(int index)
        {
            if (index > 0 && index < group.Count) Select(index);
        }
    }

    public interface ITGroup
    {
        void Select(int index);
        void TrySelect(int index);
    }

    public class GameObjectGroup : MonoBehaviour, ITGroup
    {
        public List<GameObject> group;

        public void Select(int index)
        {
            for (int i = 0; i < group.Count; i++)
            {
                group[i].SetActive(i == index);
            }
        }

        public void TrySelect(int index)
        {
            if (index >= 0 && index < group.Count) Select(index);
        }
    }
}

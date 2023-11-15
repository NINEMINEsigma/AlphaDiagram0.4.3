using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AD.Utility
{
    public static class TransformExtension
    {
        private static Transform[] GetTransforms(this GameObject self, Comparison<Transform> comparison, bool IsActiveInHierarchy)
        {
            List<Transform> transforms = new List<Component>(
                self.GetComponentsInChildren(typeof(Transform))).ConvertAll(c => (Transform)c
                );
            transforms.RemoveAll(T => !(T.gameObject.activeInHierarchy || !IsActiveInHierarchy) && T == self.transform);

            transforms.Sort(comparison);

            return transforms.ToArray();
        }

        public static void SortChilds(this GameObject self, Comparison<Transform> comparison, bool IsActiveInHierarchy = true)
        {
            foreach (Transform item in self.GetTransforms(comparison, IsActiveInHierarchy))
            {
                item.SetAsLastSibling();
            }
        }

        private static T[] GetComponents<T>(this GameObject self, bool IsActiveInHierarchy) where T : Component, IComparable<T>
        {
            var transforms = self.GetComponentsInChildren<Transform>().ToList();
            transforms.RemoveAll(T => !(T.gameObject.activeInHierarchy || !IsActiveInHierarchy) && T == self.transform);
            List<T> result = new();
            foreach (Transform item in transforms)
            {
                if (item.TryGetComponent<T>(out var component))
                {
                    result.Add(component);
                }
            }
            result.Sort();
            return result.ToArray();
        }

        public static void SortChildComponentTransform<T>(this GameObject self, bool IsActiveInHierarchy = true) where T : Component, IComparable<T>
        {
            foreach (var item in self.GetComponents<T>(IsActiveInHierarchy))
            {
                item.transform.SetAsLastSibling();
            }
        }
    }
}

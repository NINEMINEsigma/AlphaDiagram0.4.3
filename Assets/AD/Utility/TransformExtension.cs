using System;
using System.Collections.Generic;
using System.Linq;
using AD.BASE;
using UnityEngine;

namespace AD.Utility
{
    public static class TransformExtension
    {
        public static List<GameObject> GetChilds(this GameObject self)
        {
            //只能用这种方法，有bug
            return new List<Component>(self.GetComponentsInChildren(typeof(Transform))).ConvertAll(c => (Transform)c).GetSubList(T => true, T => T.gameObject);
        }

        public static List<GameObject> GetChilds(this Transform self)
        {
            //只能用这种方法，有bug
            return new List<Component>(self.GetComponentsInChildren(typeof(Transform))).ConvertAll(c => (Transform)c).GetSubList(T => true, T => T.gameObject);
        }

        public static List<GameObject> GetChilds(this Component self)
        {
            //只能用这种方法，有bug
            return new List<Component>(self.GetComponentsInChildren(typeof(Transform))).ConvertAll(c => (Transform)c).GetSubList(T => true, T => T.gameObject);
        }

        private static Transform[] GetTransforms(this GameObject self, Comparison<Transform> comparison, bool IsActiveInHierarchy)
        {
            //只能用这种方法，有bug
            List<Transform> transforms = new List<Component>(self.GetComponentsInChildren(typeof(Transform))).ConvertAll(c => (Transform)c);
            //List<Transform> transforms = self.GetComponentsInChildren<Transform>().ToList();
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

        public static void ClampToParent(this RectTransform self)
        {
            if (self.parent == null) return;
            Vector3 pos = self.localPosition;

            var parent = self.parent.As<RectTransform>();

            Vector3 minPosition = parent.rect.min - self.rect.min;
            Vector3 maxPosition = parent.rect.max - self.rect.max;

            pos.x = Mathf.Clamp(self.localPosition.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(self.localPosition.y, minPosition.y, maxPosition.y);

            self.localPosition = pos;
        }

        public static void SetX(this Transform self, float x)
        {
            self.position = self.position.SetX(x);
        }

        public static void SetY(this Transform self, float y)
        {
            self.position = self.position.SetY(y);
        }

        public static void SetZ(this Transform self, float z)
        {
            self.position = self.position.SetZ(z);
        }

        public static void AddX(this Transform self, float x)
        {
            self.position = self.position.AddX(x);
        }

        public static void AddY(this Transform self, float y)
        {
            self.position = self.position.AddY(y);
        }

        public static void AddZ(this Transform self, float z)
        {
            self.position = self.position.AddZ(z);
        }

        /// <summary>
        /// UI Face Will Backward Position
        /// </summary>
        /// <param name="self"></param>
        /// <param name="position"></param>
        public static void FaceAt(this Transform self, Vector3 position)
        {
            self.LookAt(self.position.GetSymmetryPoint(position));
        }

        /// <summary>
        /// UI Face Will Backward Position
        /// </summary>
        /// <param name="self"></param>
        /// <param name="position"></param>
        public static void FaceAt(this Transform self, Transform to)
        {
            self.LookAt(self.position.GetSymmetryPoint(to.position));
        }
    }
}

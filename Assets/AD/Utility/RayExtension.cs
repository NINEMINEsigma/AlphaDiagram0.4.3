using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AD.Utility
{
    public static class RayExtension
    {
        /// <summary>
        /// 在使用结束后，为了销毁使用的Line，需要使用DestroyRayInfo方法手动清除实体，因为只有主线程能调用unity api
        /// </summary>
        [Serializable]
        public class RayInfo
        {
            public RayInfo(Ray ray, float far)
            {
                RayForm = ray; mask = new LayerMask();
                Physics.RaycastNonAlloc(ray, RaycastHitForms, far, mask);
                Physics.Raycast(ray, out NearestRaycastHitForm);
            }
            public RayInfo(Ray ray, float far, string layer)
            {
                RayForm = ray; mask = LayerMask.GetMask(layer); 
                Physics.RaycastNonAlloc(ray, RaycastHitForms, far, mask);
                Physics.Raycast(ray, out NearestRaycastHitForm);
            }

            public Ray RayForm { get; private set; }
            public RaycastHit[] RaycastHitForms = new RaycastHit[32];
            public RaycastHit NearestRaycastHitForm;
            public Color RendererColor;
            public float width;
            public float far;

            public LayerMask mask;

            public void Update(Ray RayForm, Color rendererColor, Transform parent, float width, float far)
            {
                this.RayForm = RayForm;
                this.RendererColor = rendererColor;
                this.width = width;
                this.far = far;
                if (Line == null)
                {
                    Line = new GameObject("Ray-Line Renderer").AddComponent<LineRenderer>();
                    Line.material = Resources.Load<Material>("AD/Material/LineRenderer - Default 2 Point");
                }
                if (Line.transform.parent != parent)
                {
                    Line.transform.SetParent(parent);
                }
                Update();
            }

            public void Update(Ray RayForm, Color rendererColor, Transform parent, float width)
            {
                Update(RayForm, rendererColor, parent, width, far);
            }

            public void Update(Ray RayForm, Color rendererColor, float far)
            {
                this.RayForm = RayForm;
                this.RendererColor = rendererColor;
                this.far = far;
                if (Line != null)
                {
                    GameObject.DestroyImmediate(Line.gameObject);
                }
                Update();
            }

            public void Update(Ray RayForm, Color rendererColor)
            {
                this.RayForm = RayForm;
                this.RendererColor = rendererColor;
                if (Line != null)
                {
                    GameObject.Destroy(Line.gameObject);
                }
                Update();
            }

            public void Update(bool IsDraw=true)
            {
                Physics.RaycastNonAlloc(RayForm, RaycastHitForms, far, mask);
                Physics.Raycast(RayForm, out NearestRaycastHitForm);
                if (!IsDraw) return;
                if (Line == null)
                {
                    Debug.DrawLine(RayForm.origin, RayForm.origin + RayForm.direction * far);
                }
                else
                {
                    Line.positionCount = 2;
                    Line.SetPositions(new Vector3[] { RayForm.origin, RayForm.origin + RayForm.direction * far });
                    Line.startColor = RendererColor;
                    Line.endColor = RendererColor;
                    Line.widthCurve = AnimationCurve.Linear(0, width, 1, width);
                    Line.loop = false;
                }
            }

            public void Update(Vector3 from, Vector3 target, Color rendererColor)
            {
                if (Line != null)
                {
                    GameObject.DestroyImmediate(Line.gameObject);
                }
                Debug.DrawLine(from, target, rendererColor);
            }

            public void Update(Vector3 from, Vector3 target, Color rendererColor, Transform parent, float width = 1)
            {
                if (Line == null)
                {
                    Line = new GameObject("Ray-Line Renderer").AddComponent<LineRenderer>();
                    Line.material = Resources.Load<Material>("AD/Material/LineRenderer - Default 2 Point");
                }
                if (Line.transform.parent != parent)
                {
                    Line.transform.SetParent(parent);
                }
                RayForm = new Ray(from, target - from);
                far = Vector3.Distance(from, target);
                this.width = width;
                RendererColor = rendererColor;
                Physics.RaycastNonAlloc(RayForm, RaycastHitForms, far, mask);
                Line.positionCount = 2;
                Line.SetPositions(new Vector3[] { from, target });
                Line.startColor = RendererColor;
                Line.endColor = RendererColor;
                Line.widthCurve = AnimationCurve.Linear(0, width, 1, width);
                Line.loop = false;
            }

            public void Update(Camera camera, Color rendererColor, bool IsNeedLineRenderer, float width, float far)
            {
                var ray = new Ray(camera.transform.position, camera.transform.position + camera.transform.forward * far);
                if (IsNeedLineRenderer) Update(ray, rendererColor, camera.transform, width, far);
                else Update(ray, rendererColor, far);
            }

            public LineRenderer Line;

            public static void DestroyRayInfo(RayInfo rayInfo)
            {
                if (rayInfo.Line != null) GameObject.DestroyImmediate(rayInfo.Line.gameObject);
            }
        }

        public static RayInfo GetRay(this Vector3 origin, Vector3 direction, float far, string layer)
        {
            return new RayInfo(new Ray(origin, direction), far, layer);
        }
        public static RayInfo GetRay(this Vector3 origin, Vector3 direction, float far)
        {
            return new RayInfo(new Ray(origin, direction), far);
        }
        public static RayInfo GetRay(this Vector3 self, Vector3 target, string layer)
        {
            return new RayInfo(new Ray(self, target), Vector3.Distance(self, target), layer);
        }
        public static RayInfo GetRay(this Vector3 self, Vector3 target)
        {
            return new RayInfo(new Ray(self, target), Vector3.Distance(self, target));
        }

        public static RayInfo GetRay(this Camera self)
        {
            return GetRay(self.transform.position, self.transform.forward);
        }
    }
}

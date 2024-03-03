using System;
using System.Collections.Generic;
using AD.UI;
using AD.Utility;
using UnityEngine;
using static AD.Utility.RayExtension;

namespace AD.Experimental.Performance
{
    /// <summary>
    /// Debug 绘制camera的 视口、视锥 和 FOV
    /// </summary>
    /// 
    [ExecuteInEditMode, RequireComponent(typeof(Camera))]
    public class ConeAllegation : MonoBehaviour
    {
        private const float Width = 0.001f;
        public float _farDistance = 10;//远视口距离
        public float _nearDistance = 3;//近视口距离

        public bool IsNeedLineRenderer = false;
        public bool IfJustRendererView = false;

        public ADSerializableDictionary<int, RayInfo> RayInfos_OnDrawFarView = new();
        public ADSerializableDictionary<int, RayInfo> RayInfos_OnDrawNearView = new();
        public ADSerializableDictionary<int, RayInfo> RayInfos_OnDrawFOV = new();
        public ADSerializableDictionary<int, RayInfo> RayInfos_OnDrawConeOfCameraVision = new();


        private Camera _camera;
        private Camera TargetCamera
        {
            get
            {
                if (_camera == null) _camera = GetComponent<Camera>();
                return _camera;
            }
        }

        private void Update()
        {
            OnDrawFarView(IsNeedLineRenderer);
            OnDrawNearView(IsNeedLineRenderer);
            OnDrawFOV(IsNeedLineRenderer);
            OnDrawConeOfCameraVision(IsNeedLineRenderer);

            if (IfJustRendererView) return;

            foreach (var item in Items)
            {
                if (item.OnEnCone.GetPersistentEventCount() + item.OnQuCone.GetPersistentEventCount() == 0)
                {
                    item.gameObject.SetActive(true);
                }
                item.IsOnCone = true;
            }

            Plane[] planes = GetFrustumPlanes();

            foreach (var plane in planes)
            {
                foreach (var item in Items)
                {
                    if (item.IsOnCone)
                        foreach (var point in item.Pointers)
                        {
                            if (!plane.GetSide(point))
                            {
                                if (item.OnEnCone.GetPersistentEventCount() + item.OnQuCone.GetPersistentEventCount() == 0)
                                {
                                    item.gameObject.SetActive(false);
                                }
                                item.IsOnCone = false;
                                break;
                            }
                        }
                }
            }

        }

        private void OnDestroy()
        {
            foreach (var item in RayInfos_OnDrawFarView)
            {
                RayExtension.RayInfo.DestroyRayInfo(item.Value);
            }
            foreach (var item in RayInfos_OnDrawNearView)
            {
                RayExtension.RayInfo.DestroyRayInfo(item.Value);
            }
            foreach (var item in RayInfos_OnDrawFOV)
            {
                RayExtension.RayInfo.DestroyRayInfo(item.Value);
            }
            foreach (var item in RayInfos_OnDrawConeOfCameraVision)
            {
                RayExtension.RayInfo.DestroyRayInfo(item.Value);
            }
        }

        /// <summary>
        /// 绘制较远的视口
        /// </summary>
        void OnDrawFarView(bool IsR)
        {
            Vector3[] corners = GetCorners(_farDistance);

            //中心线
            Vector3 vecStart = TargetCamera.transform.position;
            Vector3 vecEnd = vecStart + TargetCamera.transform.forward * _farDistance;

            var current = RayInfos_OnDrawFarView;

            if (IsR)
            {
                if (!current.ContainsKey(0)) current.Add(0, TargetCamera.GetRay()); current[0].Update(corners[0], corners[1], Color.red, TargetCamera.transform, Width);
                if (!current.ContainsKey(1)) current.Add(1, TargetCamera.GetRay()); current[1].Update(corners[1], corners[3], Color.red, TargetCamera.transform, Width);
                if (!current.ContainsKey(2)) current.Add(2, TargetCamera.GetRay()); current[2].Update(corners[3], corners[2], Color.red, TargetCamera.transform, Width);
                if (!current.ContainsKey(3)) current.Add(3, TargetCamera.GetRay()); current[3].Update(corners[2], corners[0], Color.red, TargetCamera.transform, Width);
                if (!current.ContainsKey(4)) current.Add(4, TargetCamera.GetRay()); current[4].Update(vecStart, vecEnd, Color.red, TargetCamera.transform, Width);
            }
            else
            {
                if (!current.ContainsKey(0)) current.Add(0, TargetCamera.GetRay()); current[0].Update(corners[0], corners[1], Color.red);
                if (!current.ContainsKey(1)) current.Add(1, TargetCamera.GetRay()); current[1].Update(corners[1], corners[3], Color.red);
                if (!current.ContainsKey(2)) current.Add(2, TargetCamera.GetRay()); current[2].Update(corners[3], corners[2], Color.red);
                if (!current.ContainsKey(3)) current.Add(3, TargetCamera.GetRay()); current[3].Update(corners[2], corners[0], Color.red);
                if (!current.ContainsKey(4)) current.Add(4, TargetCamera.GetRay()); current[4].Update(vecStart, vecEnd, Color.red);
                /*// for debugging
                Debug.DrawLine(corners[0], corners[1], Color.red); // UpperLeft -> UpperRight
                Debug.DrawLine(corners[1], corners[3], Color.red); // UpperRight -> LowerRight
                Debug.DrawLine(corners[3], corners[2], Color.red); // LowerRight -> LowerLeft
                Debug.DrawLine(corners[2], corners[0], Color.red); // LowerLeft -> UpperLeft

                Debug.DrawLine(vecStart, vecEnd, Color.red);*/
            }
        }

        /// <summary>
        /// 绘制较近的视口
        /// </summary>
        void OnDrawNearView(bool IsR)
        {
            Vector3[] corners = GetCorners(_nearDistance);

            var current = RayInfos_OnDrawNearView;

            if (IsR)
            {
                if (!current.ContainsKey(0)) current.Add(0, TargetCamera.GetRay()); current[0].Update(corners[0], corners[1], Color.red, TargetCamera.transform, Width);
                if (!current.ContainsKey(1)) current.Add(1, TargetCamera.GetRay()); current[1].Update(corners[1], corners[3], Color.red, TargetCamera.transform, Width);
                if (!current.ContainsKey(2)) current.Add(2, TargetCamera.GetRay()); current[2].Update(corners[3], corners[2], Color.red, TargetCamera.transform, Width);
                if (!current.ContainsKey(3)) current.Add(3, TargetCamera.GetRay()); current[3].Update(corners[2], corners[0], Color.red, TargetCamera.transform, Width);
            }
            else
            {
                if (!current.ContainsKey(0)) current.Add(0, TargetCamera.GetRay()); current[0].Update(corners[0], corners[1], Color.red);
                if (!current.ContainsKey(1)) current.Add(1, TargetCamera.GetRay()); current[1].Update(corners[1], corners[3], Color.red);
                if (!current.ContainsKey(2)) current.Add(2, TargetCamera.GetRay()); current[2].Update(corners[3], corners[2], Color.red);
                if (!current.ContainsKey(3)) current.Add(3, TargetCamera.GetRay()); current[3].Update(corners[2], corners[0], Color.red);
            }
        }

        /// <summary>
        /// 绘制 camera 的 FOV
        /// </summary>
        void OnDrawFOV(bool IsR)
        {
            float halfFOV = (_camera.fieldOfView * 0.5f) * Mathf.Deg2Rad;//一半fov
            float halfHeight = _farDistance * Mathf.Tan(halfFOV);//distance距离位置，相机视口高度的一半

            //起点
            Vector3 vecStart = TargetCamera.transform.position;

            //上中
            Vector3 vecUpCenter = vecStart;
            vecUpCenter.y -= halfHeight;
            vecUpCenter.z += _farDistance;

            //下中
            Vector3 vecBottomCenter = vecStart;
            vecBottomCenter.y += halfHeight;
            vecBottomCenter.z += _farDistance;

            var current = RayInfos_OnDrawFOV;

            if (IsR)
            {
                if (!current.ContainsKey(0)) current.Add(0, TargetCamera.GetRay()); current[0].Update(vecStart, vecUpCenter, Color.blue, TargetCamera.transform, Width);
                if (!current.ContainsKey(1)) current.Add(1, TargetCamera.GetRay()); current[1].Update(vecStart, vecBottomCenter, Color.blue, TargetCamera.transform, Width);
            }
            else
            {
                if (!current.ContainsKey(0)) current.Add(0, TargetCamera.GetRay()); current[0].Update(vecStart, vecUpCenter, Color.blue);
                if (!current.ContainsKey(1)) current.Add(1, TargetCamera.GetRay()); current[1].Update(vecStart, vecBottomCenter, Color.blue);
            }

            /*Debug.DrawLine(vecStart, vecUpCenter, Color.blue);
            Debug.DrawLine(vecStart, vecBottomCenter, Color.blue);*/
        }

        /// <summary>
        /// 绘制 camera 的视锥 边沿
        /// </summary>
        void OnDrawConeOfCameraVision(bool IsR)
        {
            Vector3[] corners = GetCorners(_farDistance);

            var CameraTransform = TargetCamera.transform;

            var current = RayInfos_OnDrawConeOfCameraVision;

            if (IsR)
            {
                if (!current.ContainsKey(0)) current.Add(0, TargetCamera.GetRay()); current[0].Update(CameraTransform.position, corners[1], Color.green, TargetCamera.transform, Width);
                if (!current.ContainsKey(1)) current.Add(1, TargetCamera.GetRay()); current[1].Update(CameraTransform.position, corners[3], Color.green, TargetCamera.transform, Width);
                if (!current.ContainsKey(2)) current.Add(2, TargetCamera.GetRay()); current[2].Update(CameraTransform.position, corners[2], Color.green, TargetCamera.transform, Width);
                if (!current.ContainsKey(3)) current.Add(3, TargetCamera.GetRay()); current[3].Update(CameraTransform.position, corners[0], Color.green, TargetCamera.transform, Width);
            }
            else
            {
                if (!current.ContainsKey(0)) current.Add(0, TargetCamera.GetRay()); current[0].Update(CameraTransform.position, corners[1], Color.green);
                if (!current.ContainsKey(1)) current.Add(1, TargetCamera.GetRay()); current[1].Update(CameraTransform.position, corners[3], Color.green);
                if (!current.ContainsKey(2)) current.Add(2, TargetCamera.GetRay()); current[2].Update(CameraTransform.position, corners[2], Color.green);
                if (!current.ContainsKey(3)) current.Add(3, TargetCamera.GetRay()); current[3].Update(CameraTransform.position, corners[0], Color.green);
            }
            /*// for debugging
            Debug.DrawLine(CameraTransform.position, corners[1], Color.green); // UpperLeft -> UpperRight
            Debug.DrawLine(CameraTransform.position, corners[3], Color.green); // UpperRight -> LowerRight
            Debug.DrawLine(CameraTransform.position, corners[2], Color.green); // LowerRight -> LowerLeft
            Debug.DrawLine(CameraTransform.position, corners[0], Color.green); // LowerLeft -> UpperLeft*/
        }

        //获取相机视口四个角的坐标
        //参数 distance  视口距离
        Vector3[] GetCorners(float distance)
        {
            Vector3[] corners = new Vector3[4];

            //fov为垂直视野  水平fov取决于视口的宽高比  以度为单位

            var CameraTransform = TargetCamera.transform;

            float halfFOV = (_camera.fieldOfView * 0.5f) * Mathf.Deg2Rad;//一半fov
            float aspect = _camera.aspect;//相机视口宽高比

            float height = distance * Mathf.Tan(halfFOV);//distance距离位置，相机视口高度的一半
            float width = height * aspect;//相机视口宽度的一半

            //左上
            corners[0] = CameraTransform.position - (CameraTransform.right * width);//相机坐标 - 视口宽的一半
            corners[0] += CameraTransform.up * height;//+视口高的一半
            corners[0] += CameraTransform.forward * distance;//+视口距离

            // 右上
            corners[1] = CameraTransform.position + (CameraTransform.right * width);//相机坐标 + 视口宽的一半
            corners[1] += CameraTransform.up * height;//+视口高的一半
            corners[1] += CameraTransform.forward * distance;//+视口距离

            // 左下
            corners[2] = CameraTransform.position - (CameraTransform.right * width);//相机坐标 - 视口宽的一半
            corners[2] -= CameraTransform.up * height;//-视口高的一半
            corners[2] += CameraTransform.forward * distance;//+视口距离

            // 右下
            corners[3] = CameraTransform.position + (CameraTransform.right * width);//相机坐标 + 视口宽的一半
            corners[3] -= CameraTransform.up * height;//-视口高的一半
            corners[3] += CameraTransform.forward * distance;//+视口距离

            return corners;
        }

        [Flags]
        public enum RectInFrustumKey
        {
            None = 0,
            LeftTop = 1 << 0,
            RightTop = 1 << 1,
            LeftButtom = 1 << 2,
            RightButtom = 1 << 3,
        }

        public RectInFrustumKey IsRectInFrustum(RectTransform rectTransform)
        {
            Plane[] planes = GetFrustumPlanes();
            Vector3[] rectP = rectTransform.GetRect();
            RectInFrustumKey rectE = (RectInFrustumKey)(1 << 4 - 1);
            for (int i = 0, iMax = planes.Length; i < iMax; ++i)
            {
                switch (rectE)
                {
                    case RectInFrustumKey.LeftTop when !planes[i].GetSide(rectP[0]):
                        rectE -= RectInFrustumKey.LeftTop;
                        break;
                    case RectInFrustumKey.RightTop when !planes[i].GetSide(rectP[1]):
                        rectE -= RectInFrustumKey.RightTop;
                        break;
                    case RectInFrustumKey.LeftButtom when !planes[i].GetSide(rectP[2]):
                        rectE -= RectInFrustumKey.LeftButtom;
                        break;
                    case RectInFrustumKey.RightButtom when !planes[i].GetSide(rectP[3]):
                        rectE -= RectInFrustumKey.RightTop;
                        break;
                    default:
                        break;
                }
            }
            return rectE;
        }

        public bool IsPointInFrustum(Vector3 point)
        {
            Plane[] planes = GetFrustumPlanes();

            for (int i = 0, iMax = planes.Length; i < iMax; ++i)
            {
                //判断一个点是否在平面的正方向上
                if (!planes[i].GetSide(point))
                {
                    return false;
                }
            }
            return true;
        }

        Plane[] GetFrustumPlanes()
        {
            return GeometryUtility.CalculateFrustumPlanes(TargetCamera);
        }

        public List<ConeAllegationItem> Items = new();
    }
}

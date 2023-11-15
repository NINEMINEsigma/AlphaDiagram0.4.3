using System;
using System.Collections.Generic;
using AD.UI;
using UnityEngine;

namespace AD.Experimental.Performance
{
    /// <summary>
    /// Debug 绘制camera的 视口、视锥 和 FOV
    /// </summary>
    /// 
    [ExecuteInEditMode,RequireComponent(typeof( Camera))]
    public class ConeAllegation : MonoBehaviour
    {
        public float _farDistance = 10;//远视口距离
        public float _nearDistance = 3;//近视口距离

        private Camera _camera;
        private Camera TargetCamera
        {
            get
            {
                if (_camera == null) _camera = GetComponent<Camera>();
                return _camera;
            }
        }

        /// <summary>
        /// 绘制图形
        /// </summary>
        void OnDrawGizmos()
        {
            OnDrawFarView();
            OnDrawNearView();
            OnDrawFOV();
            OnDrawConeOfCameraVision();
        }

        /// <summary>
        /// 绘制较远的视口
        /// </summary>
        void OnDrawFarView()
        {
            Vector3[] corners = GetCorners(_farDistance);

            // for debugging
            Debug.DrawLine(corners[0], corners[1], Color.red); // UpperLeft -> UpperRight
            Debug.DrawLine(corners[1], corners[3], Color.red); // UpperRight -> LowerRight
            Debug.DrawLine(corners[3], corners[2], Color.red); // LowerRight -> LowerLeft
            Debug.DrawLine(corners[2], corners[0], Color.red); // LowerLeft -> UpperLeft


            //中心线
            Vector3 vecStart = TargetCamera.transform.position;
            Vector3 vecEnd = vecStart;
            vecEnd += TargetCamera.transform.forward * _farDistance;
            Debug.DrawLine(vecStart, vecEnd, Color.red);
        }

        /// <summary>
        /// 绘制较近的视口
        /// </summary>
        void OnDrawNearView()
        {
            Vector3[] corners = GetCorners(_nearDistance);

            // for debugging
            Debug.DrawLine(corners[0], corners[1], Color.green);//左上-右上
            Debug.DrawLine(corners[1], corners[3], Color.green);//右上-右下
            Debug.DrawLine(corners[3], corners[2], Color.green);//右下-左下
            Debug.DrawLine(corners[2], corners[0], Color.green);//左下-左上
        }

        /// <summary>
        /// 绘制 camera 的 FOV
        /// </summary>
        void OnDrawFOV()
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

            Debug.DrawLine(vecStart, vecUpCenter, Color.blue);
            Debug.DrawLine(vecStart, vecBottomCenter, Color.blue);
        }

        /// <summary>
        /// 绘制 camera 的视锥 边沿
        /// </summary>
        void OnDrawConeOfCameraVision()
        {
            Vector3[] corners = GetCorners(_farDistance);

            var CameraTransform = TargetCamera.transform;
            // for debugging
            Debug.DrawLine(CameraTransform.position, corners[1], Color.green); // UpperLeft -> UpperRight
            Debug.DrawLine(CameraTransform.position, corners[3], Color.green); // UpperRight -> LowerRight
            Debug.DrawLine(CameraTransform.position, corners[2], Color.green); // LowerRight -> LowerLeft
            Debug.DrawLine(CameraTransform.position, corners[0], Color.green); // LowerLeft -> UpperLeft
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

        private void Update()
        {
            foreach (var item in Items)
            {
                if (item.OnEnCone.GetPersistentEventCount() + item.OnQuCone.GetPersistentEventCount() == 0)
                {
                    item.gameObject.SetActive(true);
                }
                item.IsOnCone = true;
            }

            Plane[] planes = GetFrustumPlanes();

            foreach(var plane in planes)
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
    }
}

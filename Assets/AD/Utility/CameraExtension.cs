using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using static AD.Utility.RayExtension;

namespace AD.Utility
{
    public static class CameraExtension
    {
        /// <summary>
        /// 世界坐标转换为屏幕坐标
        /// </summary>
        /// <param name="worldPoint">屏幕坐标</param>
        /// <returns></returns>
        public static Vector2 WorldPointToScreenPoint(this Camera self, Vector3 worldPoint)
        {
            Vector2 screenPoint = self.WorldToScreenPoint(worldPoint);
            return screenPoint;
        }

        /// <summary>
        /// 屏幕坐标转换为世界坐标
        /// </summary>
        /// <param name="screenPoint">屏幕坐标</param>
        /// <param name="planeZ">距离摄像机 Z 平面的距离</param>
        /// <returns></returns>
        public static Vector3 ScreenPointToWorldPoint(this Camera self, Vector2 screenPoint, float planeZ)
        {
            Vector3 position = new(screenPoint.x, screenPoint.y, planeZ);
            Vector3 worldPoint = self.ScreenToWorldPoint(position);
            return worldPoint;
        }

        // RectTransformUtility.WorldToScreenPoint
        // RectTransformUtility.ScreenPointToWorldPointInRectangle
        // RectTransformUtility.ScreenPointToLocalPointInRectangle
        // 上面三个坐标转换的方法使用 Camera 的地方
        // 当 Canvas renderMode 为 RenderMode.ScreenSpaceCamera、RenderMode.WorldSpace 时 传递参数 canvas.worldCamera
        // 当 Canvas renderMode 为 RenderMode.ScreenSpaceOverlay 时 传递参数 null

        // UI 坐标转换为屏幕坐标
        public static Vector2 UIPointToScreenPoint(this Camera self, Vector3 worldPoint)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(self, worldPoint);
            return screenPoint;
        }

        /// <summary>
        /// 屏幕坐标转换为 UGUI 坐标
        /// </summary>
        /// <param name="self"></param>
        /// <param name="rt"></param>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public static Vector3 ScreenPointToUIPoint(this Camera self, RectTransform rt, Vector2 screenPoint)
        {
            //UI屏幕坐标转换为世界坐标
            // 当 Canvas renderMode 为 RenderMode.ScreenSpaceCamera、RenderMode.WorldSpace 时 uiCamera 不能为空
            // 当 Canvas renderMode 为 RenderMode.ScreenSpaceOverlay 时 uiCamera 可以为空
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, screenPoint, self, out Vector3 globalMousePos);
            // 转换后的 globalMousePos 使用下面方法赋值
            // target 为需要使用的 UI RectTransform
            // rt 可以是 target.GetComponent<RectTransform>(), 也可以是 target.parent.GetComponent<RectTransform>()
            // target.transform.position = globalMousePos;
            return globalMousePos;
        }

        /// <summary>
        /// 屏幕坐标转换为 UGUI RectTransform 的 anchoredPosition
        /// </summary>
        /// <param name="self"></param>
        /// <param name="parentRT"></param>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public static Vector2 ScreenPointToUILocalPoint(this Camera self, RectTransform parentRT, Vector2 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenPoint, self, out Vector2 localPos);
            // 转换后的 localPos 使用下面方法赋值
            // target 为需要使用的 UI RectTransform
            // parentRT 是 target.parent.GetComponent<RectTransform>()
            // 最后赋值 target.anchoredPosition = localPos;
            return localPos;
        }

        public static RayInfo RayUpdate(this Camera self, RayInfo current, Color color, bool IsNeedRenderer, float width, float far)
        {
            if (current == null) current = self.GetRay();
            current.Update(self, color, IsNeedRenderer, width, far);
            return current;
        }

        public static RayInfo RayCatchUpdate(this Camera self)
        {
            return self.transform.position.GetRay(self.ScreenPointToWorldPoint(Mouse.current.position.ReadValue(), 1000));
        }

    }
}

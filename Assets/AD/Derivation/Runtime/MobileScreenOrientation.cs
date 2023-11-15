using System;
using UnityEngine;
using AD.Utility;

namespace AD.Utility
{
    [Serializable]
    public class SetUp
    {
        [Tooltip("敏感度")]
        public float sensitivity = 15f;   //敏感度

        [Tooltip("最大水平移动速度")]
        public float maxturnSpeed = 35f;    // 最大移动速度

        [Tooltip("最大垂直傾斜角移动速度")]
        public float maxTilt = 35f;    // 最大倾斜角

        [Tooltip("位移加成速率")]
        public float posRate = 1.5f;
    }

}

namespace AD.Experimental.Runtime
{
    [Serializable]
    public class MobileScreenOrientation : MonoBehaviour
    {
        public enum MotionAxial
        {
            All = 1,  //全部轴
            None = 2,
            x = 3,
            y = 4,
            z = 5
        }

        public enum MotionMode
        {
            Postion = 1,   //只是位置变化
            Rotation = 2,
            All = 3    //全部变化
        }

        //就是这里比较笨了。本来使用UnityEditor类库的多选功能。但是这个类库不支持移动平台。
        public MotionAxial motionAxial1 = MotionAxial.y;
        public MotionAxial motionAxial2 = MotionAxial.None;

        public MotionMode motionMode = MotionMode.Rotation;   //运动模式

        public SetUp setUp;

        Vector3 m_MobileOrientation;   //手机陀螺仪变化的值

        Vector3 m_tagerTransform;
        Vector3 m_tagerPos;
        public Vector3 ReversePosition = Vector3.one; //基于陀螺仪方向的取反

        private void Awake()
        {
#if UNITY_EDITOR
            this.enabled = false;
#elif UNITY_WINDOW
            this.enabled = false;
#elif UNITY_ANDROID
            Init();
#elif UNITY_IOS
            Init();
#endif 
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR

#elif UNITY_WINDOW

#elif UNITY_ANDROID
            Refresh();
#elif UNITY_IOS
            Refresh();
#endif 
        }

        public void Init()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            m_tagerTransform = Vector3.zero;
            m_tagerPos = Vector3.zero;

        }

        public void Refresh()
        {
            m_MobileOrientation = Input.acceleration;

            switch (motionAxial1)   //不操作任何轴
            {
                case MotionAxial.None when motionAxial2 == MotionAxial.None:
                    return;
                case MotionAxial.x when motionAxial2 == MotionAxial.None:
                    m_tagerTransform.x = Mathf.Lerp(m_tagerTransform.x, m_MobileOrientation.y * setUp.maxTilt * ReversePosition.x, 0.2f);
                    break;
                case MotionAxial.y when motionAxial2 == MotionAxial.None:
                    m_tagerTransform.y = Mathf.Lerp(m_tagerTransform.y, -m_MobileOrientation.x * setUp.maxturnSpeed * ReversePosition.y, 0.2f);
                    break;
                case MotionAxial.z when motionAxial2 == MotionAxial.None:
                    m_tagerTransform.z = Mathf.Lerp(m_tagerTransform.z, -m_MobileOrientation.z * setUp.maxTilt * ReversePosition.z, 0.2f);
                    break;
                case MotionAxial.x when motionAxial2 == MotionAxial.y:
                    m_tagerTransform.y = Mathf.Lerp(m_tagerTransform.y, -m_MobileOrientation.x * setUp.maxturnSpeed * ReversePosition.y, 0.2f);
                    m_tagerTransform.x = Mathf.Lerp(m_tagerTransform.x, m_MobileOrientation.y * setUp.maxTilt * ReversePosition.x, 0.2f);
                    break;
                case MotionAxial.y when motionAxial2 == MotionAxial.x:
                    m_tagerTransform.y = Mathf.Lerp(m_tagerTransform.y, -m_MobileOrientation.x * setUp.maxturnSpeed * ReversePosition.y, 0.2f);
                    m_tagerTransform.x = Mathf.Lerp(m_tagerTransform.x, m_MobileOrientation.y * setUp.maxTilt * ReversePosition.x, 0.2f);
                    break;
                case MotionAxial.x when motionAxial2 == MotionAxial.z:
                    m_tagerTransform.x = Mathf.Lerp(m_tagerTransform.x, m_MobileOrientation.y * setUp.maxTilt * ReversePosition.x, 0.2f);
                    m_tagerTransform.z = Mathf.Lerp(m_tagerTransform.z, -m_MobileOrientation.z * setUp.maxTilt * ReversePosition.z, 0.2f);
                    break;
                case MotionAxial.z when motionAxial2 == MotionAxial.x:
                    m_tagerTransform.x = Mathf.Lerp(m_tagerTransform.x, m_MobileOrientation.y * setUp.maxTilt * ReversePosition.x, 0.2f);
                    m_tagerTransform.z = Mathf.Lerp(m_tagerTransform.z, -m_MobileOrientation.z * setUp.maxTilt * ReversePosition.z, 0.2f);
                    break;
                case MotionAxial.y when motionAxial2 == MotionAxial.z:
                    m_tagerTransform.y = Mathf.Lerp(m_tagerTransform.y, -m_MobileOrientation.x * setUp.maxturnSpeed * ReversePosition.y, 0.2f);
                    m_tagerTransform.z = Mathf.Lerp(m_tagerTransform.z, -m_MobileOrientation.z * setUp.maxTilt * ReversePosition.z, 0.2f);
                    break;
                case MotionAxial.z when motionAxial2 == MotionAxial.y:
                    m_tagerTransform.y = Mathf.Lerp(m_tagerTransform.y, -m_MobileOrientation.x * setUp.maxturnSpeed * ReversePosition.y, 0.2f);
                    m_tagerTransform.z = Mathf.Lerp(m_tagerTransform.z, -m_MobileOrientation.z * setUp.maxTilt * ReversePosition.z, 0.2f);
                    break;
                case MotionAxial.All when motionAxial2 == MotionAxial.All:
                    m_tagerTransform.y = Mathf.Lerp(m_tagerTransform.y, -m_MobileOrientation.x * setUp.maxturnSpeed * ReversePosition.y, 0.2f);
                    m_tagerTransform.x = Mathf.Lerp(m_tagerTransform.x, m_MobileOrientation.y * setUp.maxTilt * ReversePosition.x, 0.2f);
                    m_tagerTransform.z = Mathf.Lerp(m_tagerTransform.z, m_MobileOrientation.z * setUp.maxTilt * ReversePosition.z, 0.2f);
                    break;
            }

            m_tagerPos.x = m_tagerTransform.y;
            m_tagerPos.y = -m_tagerTransform.x;
            m_tagerPos.z = m_tagerTransform.z;

            switch (motionMode)
            {
                case MotionMode.Postion:
                    gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, m_tagerPos * setUp.posRate, Time.deltaTime * setUp.sensitivity);
                    break;
                case MotionMode.Rotation:
                    gameObject.transform.localRotation = Quaternion.Lerp(gameObject.transform.localRotation, Quaternion.Euler(m_tagerTransform), Time.deltaTime * setUp.sensitivity);
                    break;
                default:
                    gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, m_tagerPos * setUp.posRate, Time.deltaTime * setUp.sensitivity);
                    gameObject.transform.localRotation = Quaternion.Lerp(gameObject.transform.localRotation, Quaternion.Euler(m_tagerTransform), Time.deltaTime * setUp.sensitivity);
                    break;
            }
        }
    }
}

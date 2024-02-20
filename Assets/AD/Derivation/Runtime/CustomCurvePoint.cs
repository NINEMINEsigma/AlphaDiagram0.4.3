using AD.Utility;
using UnityEngine;
using UnityEngine.InputSystem;


namespace AD.Experimental.Runtime
{
    public class CustomCurvePoint : MonoBehaviour
    {
        [Tooltip("锁定X轴")] public bool m_isLockX = false;
        [Tooltip("锁定Y轴")] public bool m_isLockY = true;
        [Tooltip("锁定Z轴")] public bool m_isLockZ = false;

        [HideInInspector] public CustomCurvePoint m_controlObject;
        [HideInInspector] public CustomCurvePoint m_controlObject2;

        public CustomCurve LinkCurve;
        public bool IsAnchorPoint;

        private Vector3 offsetPos1 = Vector3.zero;
        private Vector3 offsetPos2 = Vector3.zero;
        [SerializeField] private LineRenderer lineRenderer;

        public void Setup(CustomCurve linkCurve)
        {
            LinkCurve = linkCurve;
        }

        void Start()
        {
            lineRenderer.widthMultiplier = 0.03f;
            lineRenderer.positionCount = 0;
        }
        void OnMouseDown()
        {
            if (IsAnchorPoint) OffsetPos();
        }
        public (Vector3,Vector3) OffsetPos()
        {
            if (m_controlObject && m_controlObject2) return (m_controlObject.transform.position - transform.position, m_controlObject2.transform.position - transform.position);
            else if (m_controlObject) return (offsetPos1 = m_controlObject.transform.position - transform.position, offsetPos2);
            else if (m_controlObject2) return (offsetPos1, offsetPos2 = m_controlObject2.transform.position - transform.position);
            else return (offsetPos1, offsetPos2);
        }
        void OnMouseDrag()
        {
            Vector3 pos0 = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos = mousePos.SetZ(pos0.z);
            Vector3 mousePosInWorld = Camera.main.ScreenToWorldPoint(mousePos);
            Vector3 thisPos = mousePosInWorld;
            if (m_isLockX)
                thisPos.x = transform.position.x;
            if (m_isLockY)
                thisPos.y = transform.position.y;
            if (m_isLockZ)
                thisPos.z = transform.position.z;
            transform.position = thisPos;
            LinkCurve.UpdateLine(this, offsetPos1, offsetPos2);
        }
        private void DrawControlLine()
        {
            if (!IsAnchorPoint || (!m_controlObject && !m_controlObject2)) return;
            if (lineRenderer)
            {
                lineRenderer.positionCount = (m_controlObject && m_controlObject2) ? 3 : 2;
                if (m_controlObject && !m_controlObject2)
                {
                    lineRenderer.SetPosition(0, m_controlObject.transform.position);
                    lineRenderer.SetPosition(1, transform.position);
                }
                if (m_controlObject2 && !m_controlObject)
                {
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, m_controlObject2.transform.position);
                }
                if (m_controlObject && m_controlObject2)
                {
                    lineRenderer.SetPosition(0, m_controlObject.transform.position);
                    lineRenderer.SetPosition(1, transform.position);
                    lineRenderer.SetPosition(2, m_controlObject2.transform.position);
                }
            }
        }
        void Update()
        {
            DrawControlLine();
        }
    }
}

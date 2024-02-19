using System.Collections.Generic;
using AD.BASE;
using UnityEngine;

namespace AD.Experimental.Runtime
{
    public class CustomCurve : MonoBehaviour
    {
        public List<Transform> m_allPoints;
        [SerializeField] private CustomCurvePoint m_anchorPoint;
        [SerializeField] private CustomCurvePoint m_controlPoint;
        private GameObject m_pointParent;
        [SerializeField] private LineRenderer m_lineRenderer;

        private int m_curveCount = 0;
        [SerializeField] private int SEGMENT_COUNT = 60;//曲线取点个数（取点越多这个长度越趋向于精确）

        public void Init()
        {
            var anchorPoint = LoadPoint(m_anchorPoint,transform.position).gameObject;
            m_allPoints.Add(anchorPoint.transform);
        }
        public void AddPoint(Vector3 anchorPointPos)
        {
            //初始化时m_allPoints添加了一个player
            if (m_allPoints.Count == 0) return;
            Transform lastPoint = m_allPoints[m_allPoints.Count - 1];
            CustomCurvePoint controlPoint2 = LoadPoint(m_controlPoint, lastPoint.position + new Vector3(0, 0, -1));
            CustomCurvePoint controlPoint = LoadPoint(m_controlPoint, anchorPointPos + new Vector3(0, 0, 1));
            CustomCurvePoint anchorPoint = LoadPoint(m_anchorPoint, anchorPointPos);

            anchorPoint.GetComponent<CustomCurvePoint>().m_controlObject = controlPoint.gameObject;
            lastPoint.GetComponent<CustomCurvePoint>().m_controlObject2 = controlPoint2.gameObject;

            m_allPoints.Add(controlPoint2.transform);
            m_allPoints.Add(controlPoint.transform);
            m_allPoints.Add(anchorPoint.transform);

            DrawCurve();
        }

        public void AddPoint()
        {
            AddPoint(transform.position);
        }
        public void DeletePoint(GameObject anchorPoint)
        {
            if (anchorPoint == null) return;
            CustomCurvePoint curvePoint = anchorPoint.GetComponent<CustomCurvePoint>();
            if (curvePoint && anchorPoint.tag.Equals("AnchorPoint"))
            {
                if (curvePoint.m_controlObject)
                {
                    m_allPoints.Remove(curvePoint.m_controlObject.transform);
                    Destroy(curvePoint.m_controlObject);
                }
                if (curvePoint.m_controlObject2)
                {
                    m_allPoints.Remove(curvePoint.m_controlObject2.transform);
                    Destroy(curvePoint.m_controlObject2);
                }
                if (m_allPoints.IndexOf(curvePoint.transform) == (m_allPoints.Count - 1))
                {//先判断删除的是最后一个元素再移除
                    m_allPoints.Remove(curvePoint.transform);
                    Transform lastPoint = m_allPoints[m_allPoints.Count - 2];
                    GameObject lastPointCtrObject = lastPoint.GetComponent<CustomCurvePoint>().m_controlObject2;
                    if (lastPointCtrObject)
                    {
                        m_allPoints.Remove(lastPointCtrObject.transform);
                        Destroy(lastPointCtrObject);
                        lastPoint.GetComponent<CustomCurvePoint>().m_controlObject2 = null;
                    }
                }
                else
                {
                    m_allPoints.Remove(curvePoint.transform);
                }
                Destroy(anchorPoint);
                if (m_allPoints.Count == 1)
                {
                    m_lineRenderer.positionCount = 0;
                }
            }

            DrawCurve();
        }
        public void UpdateLine(CustomCurvePoint curvePoint, Vector3 offsetPos1, Vector3 offsetPos2)
        {
            if (curvePoint)
            {
                if (curvePoint.m_controlObject)
                    curvePoint.m_controlObject.transform.position = curvePoint.transform.position + offsetPos1;
                if (curvePoint.m_controlObject2)
                    curvePoint.m_controlObject2.transform.position = curvePoint.transform.position + offsetPos2;
            }
            DrawCurve();
        }
        public List<Vector3> HiddenLine(bool isHidden = false)
        {
            m_pointParent.SetActive(isHidden);
            m_lineRenderer.enabled = isHidden;
            List<Vector3> pathPoints = new List<Vector3>();
            if (!isHidden)
            {
                for (int i = 0; i < m_lineRenderer.positionCount; i++)
                {
                    pathPoints.Add(m_lineRenderer.GetPosition(i));
                }
            }
            return pathPoints;
        }

        public void DrawCurve()//画曲线
        {
            if (m_allPoints.Count < 4) return;
            m_curveCount = (int)m_allPoints.Count / 3;
            for (int j = 0; j < m_curveCount; j++)
            {
                for (int i = 1; i <= SEGMENT_COUNT; i++)
                {
                    float t = (float)i / (float)SEGMENT_COUNT;
                    int nodeIndex = j * 3;
                    Vector3 pixel =
                        CalculateCubicBezierPoint(t,
                                                  m_allPoints[nodeIndex].position,
                                                  m_allPoints[nodeIndex + 1].position,
                                                  m_allPoints[nodeIndex + 2].position,
                                                  m_allPoints[nodeIndex + 3].position);
                    m_lineRenderer.positionCount = j * SEGMENT_COUNT + i;
                    m_lineRenderer.SetPosition((j * SEGMENT_COUNT) + (i - 1), pixel);
                }
            }
        }
        private CustomCurvePoint LoadPoint(CustomCurvePoint pointPrefab, Vector3 pos)
        {
            if (null == m_pointParent) m_pointParent = this.gameObject;
            CustomCurvePoint newpoint = pointPrefab.PrefabInstantiate();
            newpoint.Setup(this);
            newpoint.gameObject.name = newpoint.gameObject.name.Replace("(Clone)", "");
            newpoint.gameObject.transform.SetParent(m_pointParent.transform);
            newpoint.gameObject.transform.position = pos;
            return newpoint;
        }

        //贝塞尔曲线公式：B(t)=P0*(1-t)^3 + 3*P1*t(1-t)^2 + 3*P2*t^2*(1-t) + P3*t^3 ,t属于[0,1].
        Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }
    }
}

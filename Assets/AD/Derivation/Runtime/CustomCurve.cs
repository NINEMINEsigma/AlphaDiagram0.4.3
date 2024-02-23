using System;
using System.Collections.Generic;
using AD.BASE;
using AD.Utility;
using UnityEngine;

namespace AD.Experimental.Runtime
{
    public class CustomCurve : MonoBehaviour
    {
        public List<CustomCurvePoint> m_allPoints;
        [SerializeField] private CustomCurvePoint m_anchorPoint;
        [SerializeField] private CustomCurvePoint m_controlPoint;
        private GameObject m_pointParent;
        [SerializeField] private LineRenderer m_lineRenderer;

        public EaseCurve EaseCurve = new();

        private int m_curveCount = 0;
        public int SEGMENT_COUNT = 60;//曲线取点个数（取点越多这个长度越趋向于精确）

        public void Init()
        {
            foreach (var item in m_allPoints)
            {
                DeletePoint(item);
            }
            m_allPoints.Add(LoadPoint(m_anchorPoint,transform.position));
        }

        public void AddPoint(Vector3 anchorPointPos)
        {
            if (m_allPoints.Count == 0)
            {
                Init();
            }
            CustomCurvePoint lastPoint = m_allPoints[^1];
            CustomCurvePoint controlPoint2 = LoadPoint(m_controlPoint, lastPoint.transform.position + new Vector3(0, 0, -1));
            CustomCurvePoint controlPoint = LoadPoint(m_controlPoint, anchorPointPos + new Vector3(0, 0, 1));
            CustomCurvePoint anchorPoint = LoadPoint(m_anchorPoint, anchorPointPos);

            controlPoint2.transform.SetParent(anchorPoint.transform,false);
            controlPoint.transform.SetParent(anchorPoint.transform,false);

            anchorPoint.m_controlObject = controlPoint;
            lastPoint.m_controlObject2 = controlPoint2;

            m_allPoints.Add(controlPoint2);
            m_allPoints.Add(controlPoint);
            m_allPoints.Add(anchorPoint);

            controlPoint2.IsAnchorPoint = false;
            controlPoint.IsAnchorPoint = false;
            anchorPoint.IsAnchorPoint = true;

            DrawCurve();
        }
        public void AddPoint()
        {
            AddPoint(transform.position);
        }

        public void DeletePoint(CustomCurvePoint anchorPoint)
        {
            if (anchorPoint == null && !anchorPoint.IsAnchorPoint) return;

            if (anchorPoint.m_controlObject)
            {
                m_allPoints.Remove(anchorPoint.m_controlObject);
                Destroy(anchorPoint.m_controlObject.gameObject);
            }
            if (anchorPoint.m_controlObject2)
            {
                m_allPoints.Remove(anchorPoint.m_controlObject2);
                Destroy(anchorPoint.m_controlObject2.gameObject);
            }
            if (m_allPoints[^1] == anchorPoint)
            {
                m_allPoints.Remove(anchorPoint);
                CustomCurvePoint lastPoint = m_allPoints[^2];
                CustomCurvePoint lastPointCtrObject = lastPoint.m_controlObject2;
                if (lastPointCtrObject)
                {
                    m_allPoints.Remove(lastPointCtrObject);
                    Destroy(lastPointCtrObject.gameObject);
                    lastPoint.m_controlObject2 = null;
                }
            }
            else
            {
                m_allPoints.Remove(anchorPoint);
            }
            Destroy(anchorPoint.gameObject);
            if (m_allPoints.Count == 1)
            {
                m_lineRenderer.positionCount = 0;
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

        public void DrawCurve()
        {
            if (m_allPoints.Count < 4) return;
            m_curveCount = (int)m_allPoints.Count / 3;
            m_lineRenderer.positionCount = m_curveCount * SEGMENT_COUNT;
            for (int j = 0; j < m_curveCount; j++)
            {
                for (int i = 1; i <= SEGMENT_COUNT; i++)
                {
                    float t = EaseCurve.Evaluate((float)i / (float)SEGMENT_COUNT, false);
                    int nodeIndex = j * 3;
                    Vector3 pixel =
                        CalculateCubicBezierPoint(t,
                                                  m_allPoints[nodeIndex].transform.position,
                                                  m_allPoints[nodeIndex + 1].transform.position,
                                                  m_allPoints[nodeIndex + 2].transform.position,
                                                  m_allPoints[nodeIndex + 3].transform.position);
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
            //newpoint.gameObject.transform.SetParent(m_pointParent.transform);
            newpoint.gameObject.transform.position = pos;
            return newpoint;
        }

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

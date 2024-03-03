using System;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Utility
{
    public enum EaseCurveType
    {
        Linear = 0,
        InQuad = 1,
        OutQuad = 2,
        InOutQuad = 3,
        InCubic = 4,
        OutCubic = 5,
        InOutCubic = 6,
        InQuart = 7,
        OutQuart = 8,
        InOutQuart = 9,
        InQuint = 10,
        OutQuint = 11,
        InOutQuint = 12,
        InSine = 13,
        OutSine = 14,
        InOutSine = 15,
        InExpo = 16,
        OutExpo = 17,
        InOutExpo = 18,
        InCirc = 19,
        OutCirc = 20,
        InOutCirc = 21,
        InBounce = 22,
        OutBounce = 23,
        InOutBounce = 24,
        InElastic = 25,
        OutElastic = 26,
        InOutElastic = 27,
        InBack = 28,
        OutBack = 29,
        InOutBack = 30,
        Custom = 31
    }

    [System.Serializable]
    public class EaseCurve : AlgorithmBase
    {
        [SerializeField] private EaseCurveType m_CurveType;
        public EaseCurveType CurveType { get => m_CurveType; private set => m_CurveType = value; }

        private static readonly Type[] _ArgsTypes = { typeof(float), typeof(bool) };
        public override Type[] ArgsTypes => _ArgsTypes;

        private static readonly Type _ReturnType = typeof(float);
        public override Type ReturnType => _ReturnType;

        public EaseCurve()
        {
            this.CurveType = EaseCurveType.Linear;
        }

        public EaseCurve(EaseCurveType animationCurveType)
        {
            this.CurveType = animationCurveType;
        }

        public override string ToString()
        {
            return nameof(EaseCurve) + "[" + CurveType.ToString() + "]";
        }

        public float Evaluate(float t, bool IsClamp = false)
        {
            float from = 0;
            float to = 1;

            if (IsClamp)
            {
                t = Mathf.Max(t, 0);
                t = Mathf.Min(t, 1);
            }

            return CurveType switch
            {
                EaseCurveType.Linear => Linear(from, to, t),
                EaseCurveType.InQuad => InQuad(from, to, t),
                EaseCurveType.OutQuad => OutQuad(from, to, t),
                EaseCurveType.InOutQuad => InOutQuad(from, to, t),
                EaseCurveType.InCubic => InCubic(from, to, t),
                EaseCurveType.OutCubic => OutCubic(from, to, t),
                EaseCurveType.InOutCubic => InOutCubic(from, to, t),
                EaseCurveType.InQuart => InQuart(from, to, t),
                EaseCurveType.OutQuart => OutQuart(from, to, t),
                EaseCurveType.InOutQuart => InOutQuart(from, to, t),
                EaseCurveType.InQuint => InQuint(from, to, t),
                EaseCurveType.OutQuint => OutQuint(from, to, t),
                EaseCurveType.InOutQuint => InOutQuint(from, to, t),
                EaseCurveType.InSine => InSine(from, to, t),
                EaseCurveType.OutSine => OutSine(from, to, t),
                EaseCurveType.InOutSine => InOutSine(from, to, t),
                EaseCurveType.InExpo => InExpo(from, to, t),
                EaseCurveType.OutExpo => OutExpo(from, to, t),
                EaseCurveType.InOutExpo => InOutExpo(from, to, t),
                EaseCurveType.InCirc => InCirc(from, to, t),
                EaseCurveType.OutCirc => OutCirc(from, to, t),
                EaseCurveType.InOutCirc => InOutCirc(from, to, t),
                EaseCurveType.InBounce => InBounce(from, to, t),
                EaseCurveType.OutBounce => OutBounce(from, to, t),
                EaseCurveType.InOutBounce => InOutBounce(from, to, t),
                EaseCurveType.InElastic => InElastic(from, to, t),
                EaseCurveType.OutElastic => OutElastic(from, to, t),
                EaseCurveType.InOutElastic => InOutElastic(from, to, t),
                EaseCurveType.InBack => InBack(from, to, t),
                EaseCurveType.OutBack => OutBack(from, to, t),
                EaseCurveType.InOutBack => InOutBack(from, to, t),
                _ => throw new AD.BASE.ADException("Not Support")
            };
        }

        public float Evaluate(float t, EaseCurveType curveType, bool IsClamp)
        {
            float from = 0;
            float to = 1;

            if (IsClamp)
            {
                t = Mathf.Max(t, 0);
                t = Mathf.Min(t, 1);
            }

            return curveType switch
            {
                EaseCurveType.Linear => Linear(from, to, t),
                EaseCurveType.InQuad => InQuad(from, to, t),
                EaseCurveType.OutQuad => OutQuad(from, to, t),
                EaseCurveType.InOutQuad => InOutQuad(from, to, t),
                EaseCurveType.InCubic => InCubic(from, to, t),
                EaseCurveType.OutCubic => OutCubic(from, to, t),
                EaseCurveType.InOutCubic => InOutCubic(from, to, t),
                EaseCurveType.InQuart => InQuart(from, to, t),
                EaseCurveType.OutQuart => OutQuart(from, to, t),
                EaseCurveType.InOutQuart => InOutQuart(from, to, t),
                EaseCurveType.InQuint => InQuint(from, to, t),
                EaseCurveType.OutQuint => OutQuint(from, to, t),
                EaseCurveType.InOutQuint => InOutQuint(from, to, t),
                EaseCurveType.InSine => InSine(from, to, t),
                EaseCurveType.OutSine => OutSine(from, to, t),
                EaseCurveType.InOutSine => InOutSine(from, to, t),
                EaseCurveType.InExpo => InExpo(from, to, t),
                EaseCurveType.OutExpo => OutExpo(from, to, t),
                EaseCurveType.InOutExpo => InOutExpo(from, to, t),
                EaseCurveType.InCirc => InCirc(from, to, t),
                EaseCurveType.OutCirc => OutCirc(from, to, t),
                EaseCurveType.InOutCirc => InOutCirc(from, to, t),
                EaseCurveType.InBounce => InBounce(from, to, t),
                EaseCurveType.OutBounce => OutBounce(from, to, t),
                EaseCurveType.InOutBounce => InOutBounce(from, to, t),
                EaseCurveType.InElastic => InElastic(from, to, t),
                EaseCurveType.OutElastic => OutElastic(from, to, t),
                EaseCurveType.InOutElastic => InOutElastic(from, to, t),
                EaseCurveType.InBack => InBack(from, to, t),
                EaseCurveType.OutBack => OutBack(from, to, t),
                EaseCurveType.InOutBack => InOutBack(from, to, t),
                _ => throw new AD.BASE.ADException("Not Support")
            };
        }

        public override object Invoke(params object[] args)
        {
            bool isClamp = args.Length > 1 && (bool)args[1];
            return Evaluate((float)args[0], isClamp);
        }

        public static float Linear(float from, float to, float t)
        {
            float c = to - from;
            t /= 1f;
            return c * t / 1f + from;
        }

        public static float InQuad(float from, float to, float t)
        {
            float c = to - from;
            t /= 1f;
            return c * t * t + from;
        }

        public static float OutQuad(float from, float to, float t)
        {
            float c = to - from;
            t /= 1f;
            return -c * t * (t - 2f) + from;
        }

        public static float InOutQuad(float from, float to, float t)
        {
            float c = to - from;
            t /= 0.5f;
            if (t < 1) return c / 2f * t * t + from;
            t--;
            return -c / 2f * (t * (t - 2) - 1) + from;
        }

        public static float InCubic(float from, float to, float t)
        {
            float c = to - from;
            t /= 1f;
            return c * t * t * t + from;
        }

        public static float OutCubic(float from, float to, float t)
        {
            float c = to - from;
            t /= 1f;
            t--;
            return c * (t * t * t + 1) + from;
        }

        public static float InOutCubic(float from, float to, float t)
        {
            float c = to - from;
            t /= 0.5f;
            if (t < 1) return c / 2f * t * t * t + from;
            t -= 2;
            return c / 2f * (t * t * t + 2) + from;
        }

        public static float InQuart(float from, float to, float t)
        {
            float c = to - from;
            t /= 1f;
            return c * t * t * t * t + from;
        }

        public static float OutQuart(float from, float to, float t)
        {
            float c = to - from;
            t /= 1f;
            t--;
            return -c * (t * t * t * t - 1) + from;
        }

        public static float InOutQuart(float from, float to, float t)
        {
            float c = to - from;
            t /= 0.5f;
            if (t < 1) return c / 2f * t * t * t * t + from;
            t -= 2;
            return -c / 2f * (t * t * t * t - 2) + from;
        }

        public static float InQuint(float from, float to, float t)
        {
            float c = to - from;
            t /= 1f;
            return c * t * t * t * t * t + from;
        }

        public static float OutQuint(float from, float to, float t)
        {
            float c = to - from;
            t /= 1f;
            t--;
            return c * (t * t * t * t * t + 1) + from;
        }

        public static float InOutQuint(float from, float to, float t)
        {
            float c = to - from;
            t /= 0.5f;
            if (t < 1) return c / 2f * t * t * t * t * t + from;
            t -= 2;
            return c / 2f * (t * t * t * t * t + 2) + from;
        }

        public static float InSine(float from, float to, float t)
        {
            float c = to - from;
            return -c * Mathf.Cos(t / 1f * (Mathf.PI / 2f)) + c + from;
        }

        public static float OutSine(float from, float to, float t)
        {
            float c = to - from;
            return c * Mathf.Sin(t / 1f * (Mathf.PI / 2f)) + from;
        }

        public static float InOutSine(float from, float to, float t)
        {
            float c = to - from;
            return -c / 2f * (Mathf.Cos(Mathf.PI * t / 1f) - 1) + from;
        }

        public static float InExpo(float from, float to, float t)
        {
            float c = to - from;
            return c * Mathf.Pow(2, 10 * (t / 1f - 1)) + from;
        }

        public static float OutExpo(float from, float to, float t)
        {
            float c = to - from;
            return c * (-Mathf.Pow(2, -10 * t / 1f) + 1) + from;
        }

        public static float InOutExpo(float from, float to, float t)
        {
            float c = to - from;
            t /= 0.5f;
            if (t < 1f) return c / 2f * Mathf.Pow(2, 10 * (t - 1)) + from;
            t--;
            return c / 2f * (-Mathf.Pow(2, -10 * t) + 2) + from;
        }

        public static float InCirc(float from, float to, float t)
        {
            float c = to - from;
            t /= 1f;
            return -c * (Mathf.Sqrt(1 - t * t) - 1) + from;
        }

        public static float OutCirc(float from, float to, float t)
        {
            float c = to - from;
            t /= 1f;
            t--;
            return c * Mathf.Sqrt(1 - t * t) + from;
        }

        public static float InOutCirc(float from, float to, float t)
        {
            float c = to - from;
            t /= 0.5f;
            if (t < 1) return -c / 2f * (Mathf.Sqrt(1 - t * t) - 1) + from;
            t -= 2;
            return c / 2f * (Mathf.Sqrt(1 - t * t) + 1) + from;
        }

        public static float InBounce(float from, float to, float t)
        {
            float c = to - from;
            return c - OutBounce(0f, c, 1f - t) + from; //does this work?
        }

        public static float OutBounce(float from, float to, float t)
        {
            float c = to - from;

            if ((t /= 1f) < (1 / 2.75f))
            {
                return c * (7.5625f * t * t) + from;
            }
            else if (t < (2 / 2.75f))
            {
                return c * (7.5625f * (t -= (1.5f / 2.75f)) * t + .75f) + from;
            }
            else if (t < (2.5 / 2.75))
            {
                return c * (7.5625f * (t -= (2.25f / 2.75f)) * t + .9375f) + from;
            }
            else
            {
                return c * (7.5625f * (t -= (2.625f / 2.75f)) * t + .984375f) + from;
            }
        }

        public static float InOutBounce(float from, float to, float t)
        {
            float c = to - from;
            if (t < 0.5f) return InBounce(0, c, t * 2f) * 0.5f + from;
            return OutBounce(0, c, t * 2 - 1) * 0.5f + c * 0.5f + from;

        }

        public static float InElastic(float from, float to, float t)
        {
            float c = to - from;
            if (t == 0) return from;
            if ((t /= 1f) == 1) return from + c;
            float p = 0.3f;
            float s = p / 4f;
            return -(c * Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t - s) * (2 * Mathf.PI) / p)) + from;
        }

        public static float OutElastic(float from, float to, float t)
        {
            float c = to - from;
            if (t == 0) return from;
            if ((t /= 1f) == 1) return from + c;
            float p = 0.3f;
            float s = p / 4f;
            return (c * Mathf.Pow(2, -10 * t) * Mathf.Sin((t - s) * (2 * Mathf.PI) / p) + c + from);
        }

        public static float InOutElastic(float from, float to, float t)
        {
            float c = to - from;
            if (t == 0) return from;
            if ((t /= 0.5f) == 2) return from + c;
            float p = 0.3f * 1.5f;
            float s = p / 4f;
            if (t < 1)
                return -0.5f * (c * Mathf.Pow(2, 10 * (t -= 1f)) * Mathf.Sin((t - 2) * (2 * Mathf.PI) / p)) + from;
            return c * Mathf.Pow(2, -10 * (t -= 1)) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) * 0.5f + c + from;
        }

        public static float InBack(float from, float to, float t)
        {
            float c = to - from;
            float s = 1.70158f;
            t /= 0.5f;
            return c * t * t * ((s + 1) * t - s) + from;
        }

        public static float OutBack(float from, float to, float t)
        {
            float c = to - from;
            float s = 1.70158f;
            t = t / 1f - 1f;
            return c * (t * t * ((s + 1) * t + s) + 1) + from;
        }

        public static float InOutBack(float from, float to, float t)
        {
            float c = to - from;
            float s = 1.70158f;
            t /= 0.5f;
            if (t < 1) return c / 2f * (t * t * (((s *= (1.525f)) + 1) * t - s)) + from;
            t -= 2;
            return c / 2f * (t * t * (((s *= (1.525f)) + 1) * t + s) + 2) + from;
        }
    }

    [Serializable]
    public class CustomCurvePoint
    {
        public Vector3 Position;
        public bool IsAnchorPoint;
        public CustomCurvePoint m_controlObject;
        public CustomCurvePoint m_controlObject2;

        public CustomCurvePoint(Vector3 position, bool isAnchorPoint)
        {
            Position = position;
            IsAnchorPoint = isAnchorPoint;
        }

        public static implicit operator Vector3(CustomCurvePoint point) => point.Position;
    }

    public interface ICustomCurveSource
    {
        List<CustomCurvePoint> AllPoints { get; set; }
        int SEGMENT_COUNT { get; set; }

        event Action<Vector3[]> OnCurveDraw;

        void AddPoint();
        void AddPoint(Vector3 anchorPointPos);
        Vector3[] CreateCurve();
        Vector3[] CreateCurve(List<CustomCurvePoint> allPoints, int segmentCount);
        void DeletePoint(CustomCurvePoint anchorPoint);
        Vector3[] DrawCurve();
        void Init(Vector3 initPosition);
        void UpdateLine(CustomCurvePoint curvePoint, Vector3 offsetPos1, Vector3 offsetPos2);
    }

    [Serializable]
    public class CustomCurveSource : ICustomCurveSource
    {
        [SerializeField] private List<CustomCurvePoint> allPoints;
        public List<CustomCurvePoint> AllPoints { get => allPoints; set => allPoints = value; }

        public EaseCurve EaseCurve = new();

        [SerializeField] private int segment_count = 60;
        public int SEGMENT_COUNT { get => segment_count; set => segment_count = value; }

        public event Action<Vector3[]> OnCurveDraw;

        public void Init(Vector3 initPosition)
        {
            AllPoints ??= new();
            AllPoints.Clear();
            AllPoints.Add(new(initPosition, true));
        }

        public void AddPoint(Vector3 anchorPointPos)
        {
            if (AllPoints.Count == 0)
            {
                Init(Vector3.zero);
            }
            CustomCurvePoint lastPoint = AllPoints[^1];
            CustomCurvePoint controlPoint2 = LoadPoint(false, lastPoint + new Vector3(0, 0, -1));
            CustomCurvePoint controlPoint = LoadPoint(false, anchorPointPos + new Vector3(0, 0, 1));
            CustomCurvePoint anchorPoint = LoadPoint(true, anchorPointPos);

            anchorPoint.m_controlObject = controlPoint;
            lastPoint.m_controlObject2 = controlPoint2;

            AllPoints.Add(controlPoint2);
            AllPoints.Add(controlPoint);
            AllPoints.Add(anchorPoint);

            DrawCurve();
        }
        public void AddPoint()
        {
            AddPoint(Vector3.zero);
        }

        public void DeletePoint(CustomCurvePoint anchorPoint)
        {
            if (anchorPoint == null && !anchorPoint.IsAnchorPoint) return;

            if (anchorPoint.m_controlObject != null)
            {
                AllPoints.Remove(anchorPoint.m_controlObject);
            }
            if (anchorPoint.m_controlObject2 != null)
            {
                AllPoints.Remove(anchorPoint.m_controlObject2);
            }
            if (AllPoints[^1] == anchorPoint)
            {
                AllPoints.Remove(anchorPoint);
                CustomCurvePoint lastPoint = AllPoints[^2];
                CustomCurvePoint lastPointCtrObject = lastPoint.m_controlObject2;
                if (lastPointCtrObject != null)
                {
                    AllPoints.Remove(lastPointCtrObject);
                    lastPoint.m_controlObject2 = null;
                }
            }
            else
            {
                AllPoints.Remove(anchorPoint);
            }

            DrawCurve();
        }

        public void UpdateLine(CustomCurvePoint curvePoint, Vector3 offsetPos1, Vector3 offsetPos2)
        {
            if (curvePoint != null)
            {
                if (curvePoint.m_controlObject != null)
                    curvePoint.m_controlObject.Position = curvePoint.Position + offsetPos1;
                if (curvePoint.m_controlObject2 != null)
                    curvePoint.m_controlObject2.Position = curvePoint.Position + offsetPos2;
            }

            DrawCurve();
        }

        public Vector3[] DrawCurve()
        {
            if (AllPoints.Count < 4) return null;
            Vector3[] line = CreateCurve(AllPoints, SEGMENT_COUNT, EaseCurve);
            OnCurveDraw?.Invoke(line);
            return line;
        }

        public Vector3[] CreateCurve()
        {
            return CreateCurve(AllPoints, SEGMENT_COUNT, EaseCurve);
        }
        public Vector3[] CreateCurve(List<CustomCurvePoint> allPoints, int segmentCount)
        {
            return CreateCurve(allPoints, segmentCount, EaseCurve);
        }
        public Vector3[] CreateCurve(List<CustomCurvePoint> allPoints, int segmentCount, EaseCurve easeCurve)
        {
            if (allPoints.Count < 4) return null;
            int m_curveCount = (int)allPoints.Count / 3;
            Vector3[] line = new Vector3[m_curveCount * segmentCount];
            for (int j = 0; j < m_curveCount; j++)
            {
                for (int i = 1; i <= segmentCount; i++)
                {
                    float t = easeCurve.Evaluate((float)i / (float)segmentCount, false);
                    int nodeIndex = j * 3;
                    Vector3 pixel =
                         CalculateCubicBezierPoint(t,
                                                  allPoints[nodeIndex],
                                                  allPoints[nodeIndex + 1],
                                                  allPoints[nodeIndex + 2],
                                                  allPoints[nodeIndex + 3].Position);
                    line[(j * segmentCount) + (i - 1)] = pixel;
                }
            }
            return line;
        }

        private CustomCurvePoint LoadPoint(bool isAnchorPoint, Vector3 pos)
        {
            return new CustomCurvePoint(pos, isAnchorPoint);
        }

        private static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
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

    [Serializable]
    public class CustomCurveSourceLinner: ICustomCurveSource
    {
        [SerializeField] private List<CustomCurvePoint> allPoints;
        public List<CustomCurvePoint> AllPoints { get => allPoints; set => allPoints = value; }

        [SerializeField] private int segment_count = 60;
        public int SEGMENT_COUNT { get => segment_count; set => segment_count = value; }

        public event Action<Vector3[]> OnCurveDraw;

        public void Init(Vector3 initPosition)
        {
            AllPoints ??= new();
            AllPoints.Clear();
            AllPoints.Add(new(initPosition, true));
        }

        public void AddPoint(Vector3 anchorPointPos)
        {
            if (AllPoints.Count == 0)
            {
                Init(Vector3.zero);
            }
            CustomCurvePoint lastPoint = AllPoints[^1];
            CustomCurvePoint controlPoint2 = LoadPoint(false, lastPoint + new Vector3(0, 0, -1));
            CustomCurvePoint controlPoint = LoadPoint(false, anchorPointPos + new Vector3(0, 0, 1));
            CustomCurvePoint anchorPoint = LoadPoint(true, anchorPointPos);

            anchorPoint.m_controlObject = controlPoint;
            lastPoint.m_controlObject2 = controlPoint2;

            AllPoints.Add(controlPoint2);
            AllPoints.Add(controlPoint);
            AllPoints.Add(anchorPoint);

            DrawCurve();
        }
        public void AddPoint()
        {
            AddPoint(Vector3.zero);
        }

        public void DeletePoint(CustomCurvePoint anchorPoint)
        {
            if (anchorPoint == null && !anchorPoint.IsAnchorPoint) return;

            if (anchorPoint.m_controlObject != null)
            {
                AllPoints.Remove(anchorPoint.m_controlObject);
            }
            if (anchorPoint.m_controlObject2 != null)
            {
                AllPoints.Remove(anchorPoint.m_controlObject2);
            }
            if (AllPoints[^1] == anchorPoint)
            {
                AllPoints.Remove(anchorPoint);
                CustomCurvePoint lastPoint = AllPoints[^2];
                CustomCurvePoint lastPointCtrObject = lastPoint.m_controlObject2;
                if (lastPointCtrObject != null)
                {
                    AllPoints.Remove(lastPointCtrObject);
                    lastPoint.m_controlObject2 = null;
                }
            }
            else
            {
                AllPoints.Remove(anchorPoint);
            }

            DrawCurve();
        }

        public void UpdateLine(CustomCurvePoint curvePoint, Vector3 offsetPos1, Vector3 offsetPos2)
        {
            if (curvePoint != null)
            {
                if (curvePoint.m_controlObject != null)
                    curvePoint.m_controlObject.Position = curvePoint.Position + offsetPos1;
                if (curvePoint.m_controlObject2 != null)
                    curvePoint.m_controlObject2.Position = curvePoint.Position + offsetPos2;
            }

            DrawCurve();
        }

        public Vector3[] DrawCurve()
        {
            if (AllPoints.Count < 4) return null;
            Vector3[] line = CreateCurve(AllPoints, SEGMENT_COUNT);
            OnCurveDraw?.Invoke(line);
            return line;
        }

        public Vector3[] CreateCurve()
        {
            return CreateCurve(AllPoints, SEGMENT_COUNT);
        }
        public Vector3[] CreateCurve(List<CustomCurvePoint> allPoints, int segmentCount)
        {
            if (allPoints.Count < 4) return null;
            int m_curveCount = (int)allPoints.Count / 3;
            Vector3[] line = new Vector3[m_curveCount * segmentCount];
            for (int j = 0; j < m_curveCount; j++)
            {
                for (int i = 1; i <= segmentCount; i++)
                {
                    float t = (float)i / (float)segmentCount;
                    int nodeIndex = j * 3;
                    Vector3 pixel =
                         CalculateCubicBezierPoint(t,
                                                  allPoints[nodeIndex],
                                                  allPoints[nodeIndex + 1],
                                                  allPoints[nodeIndex + 2],
                                                  allPoints[nodeIndex + 3].Position);
                    line[(j * segmentCount) + (i - 1)] = pixel;
                }
            }
            return line;
        }

        private CustomCurvePoint LoadPoint(bool isAnchorPoint, Vector3 pos)
        {
            return new CustomCurvePoint(pos, isAnchorPoint);
        }

        private static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
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

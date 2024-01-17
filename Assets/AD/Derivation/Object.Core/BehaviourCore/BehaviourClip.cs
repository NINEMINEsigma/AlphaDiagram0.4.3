using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Experimental.Runtime
{
    [CreateAssetMenu(fileName = "New BehaviourClip", menuName = "AD/Behaviour/Clip", order = 50)]
    public class BehaviourClip : AD.Experimental.EditorAsset.Cache.AbstractScriptableObject
    {
        public string clipName => BindKey;
        public BehaviourSource source;

        private static IEnumerator DoUpdate(MonoBehaviour target, BehaviourClip source)
        {
            for (float i = source.source.StartTime; i < source.source.EndTime; i += Time.deltaTime)
            {
                if (!source.source.IsUpdate) yield break;
                source.DoUpdate(target);
                yield return new WaitForEndOfFrame();
            }
        }

        private static IEnumerator DoUpdate(MonoBehaviour target, BehaviourClip source, float initTime)
        {
            for (float i = initTime; i < source.source.EndTime; i += Time.deltaTime)
            {
                if (!source.source.IsUpdate) yield break;
                source.DoUpdate(target, i);
                yield return new WaitForEndOfFrame();
            }
            source.DoUpdate(target, source.source.EndTime);
        }

        public void DoUpdate(MonoBehaviour target, float current)
        {
            target.transform.localPosition = new(source.Position.x.DoCurrent(current), source.Position.y.DoCurrent(current), source.Position.z.DoCurrent(current));
            target.transform.localEulerAngles = new(source.Rotation.x.DoCurrent(current), source.Rotation.y.DoCurrent(current), source.Rotation.z.DoCurrent(current));
            target.transform.localScale = source.Scale;
            if (source.MethodMessage)
            {
                target.SendMessage(source.MethodMessage.MethodName, SendMessageOptions.DontRequireReceiver);
            }
        }

        public void DoUpdate(MonoBehaviour target)
        {
            target.transform.localPosition = source.Position;
            target.transform.localEulerAngles = source.Rotation;
            target.transform.localScale = source.Scale;
            if (source.MethodMessage)
            {
                target.SendMessage(source.MethodMessage.MethodName, SendMessageOptions.DontRequireReceiver);
            }
        }

        public void UpdateBeviour(MonoBehaviour target)
        {
            target.StartCoroutine(DoUpdate(target, this));
        }

        public void UpdateBeviour(MonoBehaviour target, float initTime)
        {
            target.StartCoroutine(DoUpdate(target, this, initTime));
        }
    }

    [Serializable]
    public class BehaviourSource
    {
        [Serializable]
        public class SourcePair
        {
            public float x, y, z, w;
            public AnimationCurve curve;

            public static float current;

            public SourcePair(float x, float y, float z, float w, AnimationCurve curve)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
                this.curve = curve;
            }

            public SourcePair(float x, float y, float z, float w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
                this.curve = AnimationCurve.Linear(0, 0, 1, 1);
            }

            public SourcePair()
            {
            }

            public float StartTime { get => x; set => x = value; }
            public float EndTime { get => y; set => y = value; }
            public float StartValue { get => z; set => z = value; }
            public float EndValue { get => w; set => w = value; }
            public float TotalTime => EndTime - StartTime;
            public float IntervalValue => EndValue - StartValue;
            public static implicit operator float(SourcePair source)
            {
                return source.DoCurrent(current);
            }
            public float DoCurrent(float current)
            {
                return curve.Evaluate((current - StartTime) / TotalTime) * IntervalValue + StartValue;
            }

            public float A { get => x; set => x = value; }
            public float B { get => y; set => y = value; }
            public float C { get => z; set => z = value; }
            public float D { get => w; set => w = value; }

            public void Setup(float x, float y, float z, float w, AnimationCurve curve)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
                this.curve = curve;
            }

            public void Setup(float x, float y, float z, float w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
            }

            public void Setup(SourcePair other)
            {
                this.x = other.x;
                this.y = other.y;
                this.z = other.z;
                this.w = other.w;
                this.curve = other.curve;
            }

            public bool Equals(SourcePair other)
            {

                return
                    this.x.Equals(other.x) &&
                    this.y.Equals(other.y) &&
                    this.z.Equals(other.z) &&
                    this.w.Equals(other.w) &&
                    this.curve.Equals(other.curve);
            }

            public T Temp<T>() where T : SourcePair, new()
            {
                T result = new T();
                result.Setup(this);
                return result;
            }

            public SourcePair Temp()
            {
                SourcePair result = new SourcePair();
                result.Setup(this);
                return result;
            }
        }

        [Serializable]
        public class EntryHelper<T>
        {
            public string StartTime, EndTime;
            public T StartValue, EndValue;

            public EntryHelper(string startTime, string endTime, T startValue, T endValue)
            {
                StartTime = startTime;
                EndTime = endTime;
                StartValue = startValue;
                EndValue = endValue;
            }
        }

        [Serializable]
        public class Vector3Entry
#if UNITY_EDITOR
            : ISerializationCallbackReceiver
#endif
        {
            private const string DefaultWhenValueNotSame = " --- ";

            //[HideInInspector]
            public SourcePair x, y, z;
            public static float Current { get => SourcePair.current; set => SourcePair.current = value; }
            public static float SetCurrent(float current) => SourcePair.current = current;

#if UNITY_EDITOR
            public EntryHelper<Vector3> xyz;
            private SourcePair _x, _y, _z;
#endif

            public Vector3Entry(SourcePair x, SourcePair y, SourcePair z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public Vector3Entry() : this(new(), new(), new())
            {
            }

#if UNITY_EDITOR

            public void OnAfterDeserialize()
            {
                if (_x != null && _y != null && _z != null && _x.Equals(x) && _y.Equals(y) && _z.Equals(z))
                {
                    if (xyz.StartTime != DefaultWhenValueNotSame && float.TryParse(xyz.StartTime, out var StartTime))
                    {
                        x.StartTime = y.StartTime = z.StartTime = StartTime;
                    }
                    if (xyz.EndTime != DefaultWhenValueNotSame && float.TryParse(xyz.EndTime, out var EndValue))
                    {
                        x.EndTime = y.EndTime = z.EndTime = EndValue;
                    }
                    x.StartValue = xyz.StartValue.x;
                    y.StartValue = xyz.StartValue.y;
                    z.StartValue = xyz.StartValue.z;
                    x.EndValue = xyz.EndValue.x;
                    y.EndValue = xyz.EndValue.y;
                    z.EndValue = xyz.EndValue.z;
                }
                xyz = null;
            }

            public void OnBeforeSerialize()
            {
                xyz = new(
                    (x.StartTime == y.StartTime && y.StartTime == z.StartTime) ? x.StartTime.ToString() : DefaultWhenValueNotSame,
                    (x.EndTime == y.EndTime && y.EndTime == z.EndTime) ? x.EndTime.ToString() : DefaultWhenValueNotSame,
                    new(x.StartValue, y.StartValue, z.StartValue),
                    new(x.EndValue, y.EndValue, z.EndValue));
                _x = x.Temp(); _y = y.Temp(); _z = z.Temp();
            }

#endif

            public static implicit operator Vector3(Vector3Entry source)
            {
                return new Vector3(source.x, source.y, source.z);
            }
        }

        [Serializable]
        public class MessageEntry
        {
            private bool IsInvoked = false;
            public float InvokeTime;
            public string MethodName;
            public static float Current { get => SourcePair.current; set => SourcePair.current = value; }
            public static float SetCurrent(float current) => SourcePair.current = current;
            public static implicit operator bool(MessageEntry source)
            {
                if (source.InvokeTime <= 0) return false;
                if (Current < source.InvokeTime)
                {
                    source.IsInvoked = false;
                    return false;
                }
                else if (source.IsInvoked == true)
                {
                    return false;
                }
                else// if(source.IsInvoked==false)
                {
                    return true;
                }
            }
        }

        //Match Timer
        public float StartTime = 0, EndTime = 100;
        public bool IsUpdate = true;
        //Local
        public Vector3Entry Position = new(), Rotation = new(), Scale = new(new(0, 1, 100, 1), new(0, 1, 100, 1), new(0, 1, 100, 1));
        public MessageEntry MethodMessage = new();
    }

}

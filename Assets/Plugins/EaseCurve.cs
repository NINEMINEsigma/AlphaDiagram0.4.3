using UnityEngine;
using System;

namespace Ichni
{
	[System.Serializable]
	public abstract class IchniAnimationCurve
	{
		public string animationCurveName;

		public new abstract Type GetType();
		public abstract float Evaluate(float t);
	}
	
	public enum AnimationCurveType //预设动画曲线类型
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
	public class PresetAnimationCurve : IchniAnimationCurve
	{
		public AnimationCurveType animationCurveType; //动画曲线类型

		public PresetAnimationCurve()
		{
			this.animationCurveType = AnimationCurveType.Linear;
			this.animationCurveName = "New AnimationCurve";
		}

		public PresetAnimationCurve(AnimationCurveType animationCurveType, string animationCurveName = "New AnimationCurve") //常用构造函数，根据类型
		{
			this.animationCurveType = animationCurveType;
			this.animationCurveName = animationCurveName;
		}

		public override Type GetType()
		{
			return typeof(PresetAnimationCurve);
		}

		public override float Evaluate(float t) //根据Type选择曲线，并计算t点(0,1)时曲线的值，若越界返回0或1
		{
			float from = 0;
			float to = 1;

			t = Mathf.Max(t, 0);
			t = Mathf.Min(t, 1);

			switch (animationCurveType)
			{
				case AnimationCurveType.Linear:
					return Linear(from, to, t);
				case AnimationCurveType.InQuad:
					return InQuad(from, to, t);
				case AnimationCurveType.OutQuad:
					return OutQuad(from, to, t);
				case AnimationCurveType.InOutQuad:
					return InOutQuad(from, to, t);
				case AnimationCurveType.InCubic:
					return InCubic(from, to, t);
				case AnimationCurveType.OutCubic:
					return OutCubic(from, to, t);
				case AnimationCurveType.InOutCubic:
					return InOutCubic(from, to, t);
				case AnimationCurveType.InQuart:
					return InQuart(from, to, t);
				case AnimationCurveType.OutQuart:
					return OutQuart(from, to, t);
				case AnimationCurveType.InOutQuart:
					return InOutQuart(from, to, t);
				case AnimationCurveType.InQuint:
					return InQuint(from, to, t);
				case AnimationCurveType.OutQuint:
					return OutQuint(from, to, t);
				case AnimationCurveType.InOutQuint:
					return InOutQuint(from, to, t);
				case AnimationCurveType.InSine:
					return InSine(from, to, t);
				case AnimationCurveType.OutSine:
					return OutSine(from, to, t);
				case AnimationCurveType.InOutSine:
					return InOutSine(from, to, t);
				case AnimationCurveType.InExpo:
					return InExpo(from, to, t);
				case AnimationCurveType.OutExpo:
					return OutExpo(from, to, t);
				case AnimationCurveType.InOutExpo:
					return InOutExpo(from, to, t);
				case AnimationCurveType.InCirc:
					return InCirc(from, to, t);
				case AnimationCurveType.OutCirc:
					return OutCirc(from, to, t);
				case AnimationCurveType.InOutCirc:
					return InOutCirc(from, to, t);
				case AnimationCurveType.InBounce:
					return InBounce(from, to, t);
				case AnimationCurveType.OutBounce:
					return OutBounce(from, to, t);
				case AnimationCurveType.InOutBounce:
					return InOutBounce(from, to, t);
				case AnimationCurveType.InElastic:
					return InElastic(from, to, t);
				case AnimationCurveType.OutElastic:
					return OutElastic(from, to, t);
				case AnimationCurveType.InOutElastic:
					return InOutElastic(from, to, t);
				case AnimationCurveType.InBack:
					return InBack(from, to, t);
				case AnimationCurveType.OutBack:
					return OutBack(from, to, t);
				case AnimationCurveType.InOutBack:
					return InOutBack(from, to, t);
			}

			return 0;
		}


		public bool Equals(PresetAnimationCurve _Right)
		{
			return this.animationCurveType == _Right.animationCurveType;

        }

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		//以下为曲线计算式
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


	[System.Serializable]
	public class CustomAnimationCurve : IchniAnimationCurve
	{
		public AnimationCurve customAnimationCurve;
		
		public CustomAnimationCurve()
		{
			this.customAnimationCurve = AnimationCurve.Linear(0, 1, 0, 1);
			this.animationCurveName = "New AnimationCurve";
		}

		public CustomAnimationCurve(AnimationCurve customAnimationCurve, string animationCurveName = "New AnimationCurve") //常用构造函数，根据类型
		{
			this.customAnimationCurve = customAnimationCurve;
			this.animationCurveName = animationCurveName;
		}

		public void UpdateAnimationCurve(AnimationCurve nowCustomAnimationCurve)
		{
			this.customAnimationCurve = nowCustomAnimationCurve;
		}
		
		public override Type GetType()
		{
			return typeof(CustomAnimationCurve);
		}

		public override float Evaluate(float t)
		{
			t = Mathf.Max(t, 0);
			t = Mathf.Min(t, 1);

			float v = customAnimationCurve.Evaluate(t);
			return v;
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using UnityEngine;

namespace AD.Utility
{
    public static class PrimitiveExtension
    {
        public static float Sum(this IEnumerator<float> self)
        {
            float sum = 0;
            while (self.MoveNext())
            {
                sum += self.Current;
            }
            return sum;
        }

        public static float Sum(this float[] self)
        {
            float sum = 0;
            foreach (var value in self)
            {
                sum += value;
            }
            return sum;
        }

        public static float Average(this float[] self)
        {
            return self.Sum() / (float)(self.Length);
        }

        public static float Max(this float[] self)
        {
            if (self.Length == 0) throw new ADException("this array is no value");
            float max = self[0];
            foreach (var value in self)
                if (value > max) max = value;
            return max;
        }

        public static bool Is_pow_2(ulong _Value)
        {
            return _Value != 0 && (_Value & (_Value - 1)) == 0;
        }

        public static float Clamp(this float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }

        public static int Clamp(this int value, int min, int max)
        {
            return Mathf.Clamp(value, min, max);
        }

        public static bool ExecuteAll(params bool[] values)
        {
            bool result = true;
            foreach (var value in values)
            {
                result = result && value;
            }
            return result;
        }

        public static bool ExecuteAny(params bool[] values)
        {
            bool result = false;
            foreach (var value in values)
            {
                result = result || value;
                if (result) return true;
            }
            return false;
        }

        public static bool ExecuteAll(params Func<bool>[] values)
        {
            bool result = true;
            foreach (var value in values)
            {
                result = result && value();
            }
            return result;
        }

        public static bool ExecuteAny(params Func<bool>[] values)
        {
            bool result = false;
            foreach (var value in values)
            {
                result = result || value();
                if (result) return true;
            }
            return false;
        }

        public static bool ExecuteAll<T>(T input, params Func<T, bool>[] values)
        {
            bool result = true;
            foreach (var value in values)
            {
                result = result && value(input);
            }
            return result;
        }

        public static bool ExecuteAny<T>(T input, params Func<T, bool>[] values)
        {
            bool result = false;
            foreach (var value in values)
            {
                result = result || value(input);
                if (result) return true;
            }
            return false;
        }

        public static int ExecuteCount(params bool[] values)
        {
            int result = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i]) result++;
            }
            return result;
        }
    }
}

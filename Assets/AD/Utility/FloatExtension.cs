using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using UnityEngine;

namespace AD.Utility
{
    public static class FloatExtension
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
                if(value > max) max = value;
            return max;
        }

    }
}

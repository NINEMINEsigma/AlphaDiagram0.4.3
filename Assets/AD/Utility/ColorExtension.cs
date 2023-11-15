using UnityEngine;
using System.Runtime.InteropServices;
//using System.Drawing;
using System.Numerics;
using System;

namespace AD.Utility
{
    public static class ColorExtension
    {
        public static Color Result_R(this Color self, float value)
        {
            return new Color(value, self.g, self.b, self.a);
        }

        public static Color Result_G(this Color self, float value)
        {
            return new Color(self.r, value, self.b, self.a);
        }

        public static Color Result_B(this Color self, float value)
        {
            return new Color(self.r, self.g, value, self.a);
        }

        public static Color Result_A(this Color self, float value)
        {
            return new Color(self.r, self.g, self.b, value);
        }

        public static Color LerpTo(this Color self, Color _Right, float value)
        {
            return Color.Lerp(self, _Right, value);
        }

    }
}

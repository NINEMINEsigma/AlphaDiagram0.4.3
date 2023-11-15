using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Utility
{
    public static class VectorExtension
    {
        public static Vector3 ToVector3 (this Vector2 self)
        {
            return self;
        }

        public static Vector2 ToVector2(this Vector3 self)
        {
            return self;
        }

        public static Vector2 SetX(this Vector2 self,float x)
        {
            self.x = x;
            return self;
        }

        public static Vector2 SetY(this Vector2 self, float y)
        {
            self.y = y;
            return self;
        }

        public static Vector3 SetX(this Vector3 self, float x)
        {
            self.x = x;
            return self;
        }

        public static Vector3 SetY(this Vector3 self, float y)
        {
            self.y = y;
            return self;
        }

        public static Vector3 SetZ(this Vector3 self, float z)
        {
            self.z = z;
            return self;
        }

    }
}

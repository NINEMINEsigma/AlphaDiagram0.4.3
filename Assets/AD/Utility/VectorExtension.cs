using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Utility
{
    public static class VectorExtension
    {
        public static Vector3 ToVector3(this Vector2 self)
        {
            return self;
        }

        public enum Vec22Vec3FillType
        {
            x, y, z
        }

        public static Vector3 ToVector3(this Vector2 self, Vec22Vec3FillType type, float filled = 0)
        {
            return type switch
            {
                Vec22Vec3FillType.x => new Vector3(filled, self.x, self.y),
                Vec22Vec3FillType.y => new Vector3(self.x, filled, self.y),
                Vec22Vec3FillType.z => new Vector3(self.x, self.y, filled),
                _ => throw new AD.BASE.ADException("Unknown Axis"),
            };
        }

        public static Vector2 ToVector2(this Vector3 self)
        {
            return self;
        }


        public enum Vec32Vec2IgnoreType
        {
            x, y, z
        }


        public static Vector2 ToVector2(this Vector3 self, Vec32Vec2IgnoreType type)
        {
            return type switch
            {
                Vec32Vec2IgnoreType.x => new Vector2(self.y, self.z),
                Vec32Vec2IgnoreType.y => new Vector2(self.x, self.z),
                Vec32Vec2IgnoreType.z => new Vector2(self.x, self.y),
                _ => throw new AD.BASE.ADException("Unknown Axis"),
            };
        }

        public static Vector2 SetX(this Vector2 self, float x)
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

        public static Vector2 AddX(this Vector2 self, float x)
        {
            self.x += x;
            return self;
        }

        public static Vector2 AddY(this Vector2 self, float y)
        {
            self.y += y;
            return self;
        }

        public static Vector3 AddX(this Vector3 self, float x)
        {
            self.x += x;
            return self;
        }

        public static Vector3 AddY(this Vector3 self, float y)
        {
            self.y += y;
            return self;
        }

        public static Vector3 AddZ(this Vector3 self, float z)
        {
            self.z += z;
            return self;
        }

        public static Vector2 SetXY(this Vector2 self, float x, float y)
        {
            self.x = x;
            self.y = y;
            return self;
        }

        public static Vector2 AddXY(this Vector2 self, float x, float y)
        {
            self.x += x;
            self.y += y;
            return self;
        }

        public static Vector3 SetXY(this Vector3 self, float x, float y)
        {
            self.x = x;
            self.y = y;
            return self;
        }

        public static Vector3 SetYZ(this Vector3 self, float y, float z)
        {
            self.y = y;
            self.z = z;
            return self;
        }

        public static Vector3 SetXZ(this Vector3 self, float x, float z)
        {
            self.x = x;
            self.z = z;
            return self;
        }

        public static Vector3 SetXYZ(this Vector3 self, float x, float y, float z)
        {
            self.x = x;
            self.y = y;
            self.z = z;
            return self;
        }

        public static Vector3 GetSymmetryPoint(this Vector3 position, Vector3 symmetry)
        {
            return new Vector3(position.x * 2 - symmetry.x, position.y * 2 - symmetry.y, position.z * 2 - symmetry.z);
        }

    }
}

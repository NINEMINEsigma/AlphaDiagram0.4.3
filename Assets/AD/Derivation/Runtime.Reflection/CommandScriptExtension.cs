using System;
using System.Linq;
using AD.Utility;
using UnityEngine;

namespace AD.Experimental.Runtime.Reflection
{
    public static class CommandScriptExtension
    {

    }

    public class UnBuilder
    {
        public class GameObjectResult
        {
            public GameObject target;

            public Transform GetTransform()
            {
                return target.transform;
            }

            public Vector2 GetPosition()
            {
                return target.transform.position;
            }

            public GameObjectResult SetPosition(float x,float y,float z)
            {
                target.transform.position = new Vector3(x, y, z);
                return this;
            }

            public GameObjectResult AddComponent(string ComponentName)
            {
                target.AddComponent(ComponentName.ToType());
                return this;
            }

            public GameObjectResult GetComponent(string ComponentName)
            {
                target.GetComponent(ComponentName.ToType());
                return this;
            }

            public GameObjectResult GetOrAddComponent(string ComponentName)
            {
                var t = ComponentName.ToType();
                var r = target.GetComponent(t);
                if (r == null) target.AddComponent(t);
                return this;
            }

            public GameObjectResult SetName(string Name)
            {
                target.name = Name;
                return this;
            }

            public GameObjectResult SetTag(string Tag)
            {
                target.tag = Tag;
                return this;
            }

            public GameObjectResult SetParent(string tag, string ParentName)
            {
                var a = GameObject.FindGameObjectsWithTag(tag).FirstOrDefault(T => T.name == ParentName);
                target.transform.SetParent(a.transform);
                return this;
            }

            // target.transform.rotation.SetEulerRotation
        }

        public GameObjectResult GenerateGameObject()
        {
            GameObjectResult result = new GameObjectResult
            {
                target = new GameObject()
            };
            return result;
        }

        public GameObjectResult GenerateGameObjectWithPrimitiveTypeName(string PrimitiveTypeName)
        {
            GameObjectResult result = new GameObjectResult
            {
                target = GameObject.CreatePrimitive((PrimitiveType)Enum.Parse(typeof(PrimitiveType), PrimitiveTypeName))
            };
            return result;
        }
    }

}

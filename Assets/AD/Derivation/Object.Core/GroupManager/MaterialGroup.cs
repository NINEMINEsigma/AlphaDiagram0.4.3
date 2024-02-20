using System;
using AD.BASE;
using UnityEngine;

namespace AD.Utility.Object
{
    [ExecuteAlways]
    public class MaterialGroup : ADController
    {
        [Serializable]
        public class SourceEntry
        {
            private void InSetupMainMaterial(Material mat)
            {
                if (materials == null || materials.Length == 0)
                {
                    materials = new Material[1] { mat };
                }
                else
                {
                    materials[0] = mat;
                }
            }

            public Material MainMaterial => material;
            public Material material
            {
                get
                {
                    if (material == null) return null;
                    return materials[0];
                }
                set
                {
                    InSetupMainMaterial(value);
                }
            }
            public Material[] materials;

            public enum ValueType
            {
                Float, Int
            }

            public enum ShiftType
            {
                Add, Sub
            }

            public enum ControlType
            {
                Time, Script
            }

            public ValueType valueType = ValueType.Float;
            public ShiftType shiftType = ShiftType.Add;
            public ControlType controlType = ControlType.Time;
            public float speed = 1;

            public void Update(string name)
            {
                if (materials == null || materials.Length == 0 || controlType == ControlType.Script) return;
                foreach (var current in materials)
                {
                    if (valueType == ValueType.Float) current.SetFloat(name, (float)(speed * Time.time));
                    else current.SetInt(name, (int)(speed * Time.time));
                }
            }
        }

        public ADSerializableDictionary<string, SourceEntry> SourcePairs = new();
        public bool IsExecuteAlways = false;

        private void Update()
        {
            if (Application.isEditor && !Application.isPlaying && IsExecuteAlways == false) return;
            foreach (var current in SourcePairs)
            {
                if (string.IsNullOrEmpty(current.Key)) continue;
                try
                {
                    current.Value.Update(current.Key);
                }
                catch { }
            }
        }

        public override void Init()
        {
            SourcePairs.Clear();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using AD.Utility;
using UnityEngine;

namespace AD.UI
{
    [CreateAssetMenu(fileName = "New InkGradientAssets", menuName = "AD/InkGradientAssets", order = 131)]
    public class InkGradientAssets : AD.Experimental.EditorAsset.Cache.AbstractScriptableObject
    {
        [Header("Normal")]
        public Sprite Normal;
        [Header("Happy")]
        public Sprite Happy;
        public Sprite Smile;
        public Sprite Laugh;
        [Header("Sad")]
        public Sprite Sad;
        public Sprite Bitter;
        public Sprite Heartbroken;
        [Header("Angry")]
        public Sprite Angry;
        public Sprite Furious;
        [Header("Others")]
        //²»Âú
        public Sprite Dissatisfied;
        //±ÕÄ¿
        public Sprite EyesClosed;
        //Ñá¶ñ
        public Sprite Disgusted;
        //ÖåÃ¼
        public Sprite Frown;
        [Header("Difference")]
        public ADSerializableDictionary<string, Sprite> Sources;

        public Sprite Obtain(string name)
        {
            if (name.Equals(nameof(Normal), System.StringComparison.CurrentCultureIgnoreCase))
            {
                return Normal;
            }
            else if (ObtainHappy(name, out var happy))
            {
                return happy;
            }
            else if (ObtainSad(name, out var sad))
            {
                return sad;
            }
            else if (ObtainAngry(name, out var angry))
            {
                return angry;
            }
            else if (name.Equals(nameof(Dissatisfied), System.StringComparison.CurrentCultureIgnoreCase))
            {
                return Dissatisfied;
            }
            else if (name.Equals(nameof(EyesClosed), System.StringComparison.CurrentCultureIgnoreCase))
            {
                return EyesClosed;
            }
            else if (name.Equals(nameof(Frown), System.StringComparison.CurrentCultureIgnoreCase))
            {
                return Frown;
            }
            else return Sources.TryGetValue(name, out var result) ? result : Normal;
        }

        public bool ObtainHappy(string name, out Sprite happy)
        {
            happy = Happy;
            if (name.Equals(nameof(Happy), System.StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            else if (name.Equals(nameof(Smile), System.StringComparison.CurrentCultureIgnoreCase))
            {
                happy = Smile != null ? Smile : Happy;
                return true;
            }
            else if (name.Equals(nameof(Laugh), System.StringComparison.CurrentCultureIgnoreCase))
            {
                happy = Laugh != null ? Laugh : Happy;
                return true;
            }
            else return false;
        }

        public bool ObtainSad(string name, out Sprite sad)
        {
            sad = Sad;
            if (name.Equals(nameof(Sad), System.StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            else if (name.Equals(nameof(Bitter), System.StringComparison.CurrentCultureIgnoreCase))
            {
                sad = Bitter != null ? Bitter : Happy;
                return true;
            }
            else if (name.Equals(nameof(Heartbroken), System.StringComparison.CurrentCultureIgnoreCase))
            {
                sad = Heartbroken != null ? Heartbroken : Happy;
                return true;
            }
            else return false;
        }

        public bool ObtainAngry(string name, out Sprite angry)
        {
            angry = Angry;
            if (name.Equals(nameof(Angry), System.StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            else if (name.Equals(nameof(Furious), System.StringComparison.CurrentCultureIgnoreCase))
            {
                angry = Furious != null ? Furious : Happy;
                return true;
            }
            else return false;
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class ColorPanel : MonoBehaviour, IPointerClickHandler, IDragHandler
    {
        public const int TexPixelLength = 256;
        Texture2D _tex2d;
        Texture2D tex2d
        {
            get
            {
                if (_tex2d == null)
                {
                    _tex2d = new Texture2D(TexPixelLength, TexPixelLength, TextureFormat.RGB24, true);
                    ri.texture = tex2d;
                }
                return _tex2d;
            }
        }
        public UnityEngine.UI.RawImage ri;

        Color[,] _arrayColor;
        Color[,] arrayColor
        {
            get
            {
                if (_arrayColor == null) _arrayColor = new Color[TexPixelLength, TexPixelLength];
                return _arrayColor;
            }
        }

        public RectTransform circleRect;

        public void SetColorPanel(Color endColor)
        {
            Color[] CalcArray = CalcArrayColor(endColor);
            tex2d.SetPixels(CalcArray);
            tex2d.Apply();
        }

        Color[] CalcArrayColor(Color endColor)
        {
            Color value = (endColor - Color.white) / (TexPixelLength - 1);
            for (int i = 0; i < TexPixelLength; i++)
            {
                arrayColor[i, TexPixelLength - 1] = Color.white + value * i;
            }
            for (int i = 0; i < TexPixelLength; i++)
            {
                value = (arrayColor[i, TexPixelLength - 1] - Color.black) / (TexPixelLength - 1);
                for (int j = 0; j < TexPixelLength; j++)
                {
                    arrayColor[i, j] = Color.black + value * j;
                }
            }
            List<Color> listColor = new List<Color>();
            for (int i = 0; i < TexPixelLength; i++)
            {
                for (int j = 0; j < TexPixelLength; j++)
                {
                    listColor.Add(arrayColor[j, i]);
                }
            }

            return listColor.ToArray();
        }

        public Color GetColorByPosition(Vector2 pos)
        {
            Texture2D tempTex2d = (Texture2D)ri.texture;
            Color getColor = tempTex2d.GetPixel((int)pos.x, (int)pos.y);

            return getColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 wordPos;
            //将UGUI的坐标转为世界坐标  
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform, eventData.position, eventData.pressEventCamera, out wordPos))
                circleRect.position = wordPos;

            circleRect.GetComponent<ColorCircle>().setShowColor();
        }
    }
}

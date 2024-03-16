using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Experimental.AD
{
    public static class EditorWindowHelper
    {
        //[MenuItem("Window/AD/Node Editor", priority = 3031)]
        public static _T OpenEditor<_T>() where _T : GridEditorWindow<_T>
        {
            _T editor = EditorWindow.GetWindow<_T>();
            editor.Init();
            editor.AfterInit();
            return editor;
        }

        public static void OpenGenericMenu(List<ContextPair> events, bool allowDuplicateNames, object useData)
        {
            GenericMenu menu = new();
            menu.allowDuplicateNames = allowDuplicateNames;

            foreach (var item in events)
            {
                if (item.IsSeparator)
                {
                    menu.AddSeparator("");
                }
                else if (item.Enable)
                {
                    menu.AddItem(new GUIContent(item.Name), item.InitEnable, item.menuFunction, useData);
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent(item.Name), item.InitEnable);
                }
            }

            menu.ShowAsContext();
            Event.current.Use();
        }

        public static void DrawFullBackground(Vector2 size, Color backgroundColor)
        {
            EditorGUI.DrawRect(new Rect(0, 0, size.x, size.y), backgroundColor);
        }

        public static void DrawFullBackground(float width, float height, Color backgroundColor)
        {
            EditorGUI.DrawRect(new Rect(0, 0, width, height), backgroundColor);
        }
    }

    public class ContextPair
    {
        public string Name = "Option";
        public bool Enable = true;
        public bool InitEnable = true;

        public GenericMenu.MenuFunction2 menuFunction = null;

        public bool IsSeparator = false;
    }

    public interface SubjectItem
    {
        Vector2 Position { get; set; }
        Vector2 SizeData { get; set; }
        int ID { get; }
        GUIContent MyGUIContent { get; }
        GUILayoutOption[] MyGUILayerOutOptions { get; }
        GUI.WindowFunction OnWindowFunction { get; }
    }

    public abstract class GridEditorWindow<T> : EditorWindow where T : GridEditorWindow<T>
    {
        [HideInInspector] private Color backgroundColor = new(0.4f, 0.4f, 0.4f);
        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                EditorUtility.SetDirty(this);
            }
        }
        [HideInInspector] private Color gridColor = new(0.1f, 0.1f, 0.1f);
        public Color GridColor
        {
            get => gridColor;
            set
            {
                gridColor = value;
                EditorUtility.SetDirty(this);
            }
        }

        [HideInInspector] private float baseScale = 0.4f;
        public float BaseScale
        {
            get => baseScale;
            set
            {
                baseScale = value;
                EditorUtility.SetDirty(this);
            }
        }
        public float SmallGridSpacing => 50 * BaseScale;
        public float SmallGridOpacity => BaseScale;
        public float BigGridSpacing => SmallGridSpacing * 5;
        public float BigGridOpacity => SmallGridOpacity * 5;

        [HideInInspector] private Vector2 offset = Vector2.zero;
        public Vector2 OffsetPosition
        {
            get => offset;
            set
            {
                offset = value;
                EditorUtility.SetDirty(this);
            }
        }

        public bool IsUpdateOnMouseMove
        {
            get => wantsMouseMove;
            set => wantsMouseMove = value;
        }

        public float DragSpeed = 1;
        public float MouseDeltaZoomSpeed = 0.005f;

        public virtual float MinGridSize => 0.2f;
        public virtual float MaxGridSize => 1;

        public List<ContextPair> OnContextClickEvent = new();

        public virtual void Init()
        {

        }

        public virtual void AfterInit()
        {

        }

        protected virtual void OnInspectorUpdate()
        {

        }

        protected virtual void OnDrawGrid()
        {
            DrawGridBackground();
            DrawGridLine(SmallGridSpacing, SmallGridOpacity);
            DrawGridLine(BigGridSpacing, BigGridOpacity);
        }

        protected virtual void OnMouseOverWindow()
        {
            switch (Event.current.type)
            {
                case EventType.ContextClick:
                    {
                        EditorWindowHelper.OpenGenericMenu(this.OnContextClickEvent, false, null);
                    }
                    break;
                case EventType.MouseDrag:
                    {
                        offset += Event.current.delta * DragSpeed;
                        Repaint();
                    }
                    break;
                case EventType.ScrollWheel:
                    {
                        if (Event.current.isScrollWheel)
                        {
                            baseScale += HandleUtility.niceMouseDeltaZoom * MouseDeltaZoomSpeed;
                            baseScale = Mathf.Clamp(baseScale, MinGridSize, MaxGridSize);
                        }
                        Repaint();
                    }
                    break;
                default:
                    break;
            }
        }

        protected virtual void OnGUI()
        {
            OnDrawGrid();
            HowDrawGUI();
            if (EditorWindow.mouseOverWindow == this)
            {
                OnMouseOverWindow();
            }
        }

        protected virtual void HowDrawGUI()
        {

        }

        private void DrawGridBackground()
        {
            EditorWindowHelper.DrawFullBackground(position.width, position.height, backgroundColor);
        }

        private void DrawGridLine(float gridSpacing, float gridOpacity)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);
            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);
            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }
            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }
            Handles.color = Color.white;
            Handles.EndGUI();
        }

        protected virtual void SubWindow(SubjectItem target)
        {
            if (target.MyGUILayerOutOptions != null)
                GUILayout.Window(target.ID, new Rect((target.Position + OffsetPosition) * BaseScale, target.SizeData * BaseScale), target.OnWindowFunction, target.MyGUIContent, target.MyGUILayerOutOptions);
            else
                GUILayout.Window(target.ID, new Rect((target.Position + OffsetPosition) * BaseScale, target.SizeData * BaseScale), target.OnWindowFunction, target.MyGUIContent);
        }

        protected virtual void SubBlock(SubjectItem target)
        {
            GUI.Box(new Rect((target.Position + OffsetPosition) * BaseScale, target.SizeData * BaseScale), target.MyGUIContent);
            var cat = target.SizeData * BaseScale * 0.1f;
            if (GUI.Button(new Rect((target.Position + OffsetPosition) * BaseScale + cat, target.SizeData * BaseScale - cat * 2), target.GetType().Name))
                target.OnWindowFunction.Invoke(target.ID);
        }
    }
}

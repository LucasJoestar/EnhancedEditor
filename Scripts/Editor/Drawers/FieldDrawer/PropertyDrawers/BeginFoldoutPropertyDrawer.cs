// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="BeginFoldoutAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(BeginFoldoutAttribute))]
	public class BeginFoldoutPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        private static List<BeginFoldoutPropertyDrawer> availableFoldouts = new List<BeginFoldoutPropertyDrawer>();
        private static readonly List<Color> colorBuffer = new List<Color>();

        private readonly AnimBool foldout = new AnimBool();
        internal string id = string.Empty;

        private float beginPos = 0f;
        private float endPos = 0f;
        private float height = 0f;
        private float colorPos = 0f;

        private bool isBoxGroup = false;

        // -----------------------

        public override void OnEnable()
        {
            BeginFoldoutAttribute _attribute = Attribute as BeginFoldoutAttribute;
            id = SerializedProperty.name + _attribute.Label.text + _attribute.Color.ToString();

            _attribute.foldout = foldout.value
                               = SessionState.GetBool(id, false);

            // Try to reconnect this foldout, as some properties can be recreated while
            // already existing (like the ObjectReference type properties).
            if (EndFoldoutPropertyDrawer.ReconnectFoldout(this, id))
            {
                EnhancedEditorGUIUtility.Repaint(SerializedProperty.serializedObject);
            }
            else
            {
                availableFoldouts.Add(this);
            }
        }

        public override bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            // Check if this begin group is connected to an end to avoid any GUI trouble.
            if (!EndFoldoutPropertyDrawer.IsConnected(this))
            {
                _height = 0f;
                return false;
            }

            BeginFoldoutAttribute _attribute = Attribute as BeginFoldoutAttribute;
            Event _event = Event.current;

            if (_attribute.HasColor && (_event.type == EventType.Repaint))
            {
                // Use the previous Repaint event position to avoid any offset with the EndFoldout position update.
                Rect _temp = EditorGUI.IndentedRect(_position);
                _temp = new Rect(_temp.x - EnhancedEditorGUIUtility.FoldoutWidth,
                                 _temp.y + 1f,
                                 _temp.width + EnhancedEditorGUIUtility.FoldoutWidth + 2f,
                                 (endPos - colorPos) + height);

                EditorGUI.DrawRect(_temp, _attribute.Color);

                // Draw an outline all around the color box.
                // Use the GUI class instead of EditorGUI to do not take over the following properties hot control.
                _temp = new Rect(_temp.x - 1f,
                                 _position.y,
                                 _temp.width + 2f,
                                 _temp.height + 2f);

                GUI.Label(_temp, GUIContent.none, EditorStyles.helpBox);

				colorPos = _position.y;
            }

            // Update the foldout value during the next event to avoid glitches and for a smoother draw.
            if (foldout.target != _attribute.foldout)
            {
                _attribute.foldout = foldout.target;
                SessionState.SetBool(id, _attribute.foldout);
            }

            _position.height = _height
                             = EditorGUIUtility.singleLineHeight;

            foldout.target = EditorGUI.Foldout(_position, foldout.target, _attribute.Label, true);
            EditorGUI.indentLevel++;

            // Only register the begin height on Repaint event,
            // as the Layout event always have uncalculated rects.
            if (_event.type == EventType.Repaint)
            {
                beginPos = _position.yMax;

                // Use a color buffer for russian-dolls-like foldouts repaint.
                if (_attribute.HasColor)
                {
                    colorBuffer.Add(_attribute.Color);
                }
            }
            else if (colorBuffer.Count > 0)
            {
                // Clear to avoid stacking unpopped out colors, which can occur when something
                // prevent the associated EndFoldout from being called (like with ObjectReference type properties).
                colorBuffer.Clear();
            }

            // When this group is folded, disable all the following controls by encapsulting them within a zero rect group.
            isBoxGroup = foldout.faded == 0f;
            if (isBoxGroup)
            {
                GUI.BeginGroup(Rect.zero);
            }

            return false;
        }
        #endregion

        #region Utility
        internal static bool GetFoldout(out BeginFoldoutPropertyDrawer _foldout)
        {
            // Pop out last inserted entry if any.
            int _index = availableFoldouts.Count - 1;
            if (_index < 0)
            {
                _foldout = null;
                return false;
            }

            _foldout = availableFoldouts[_index];
            availableFoldouts.RemoveAt(_index);

            return true;
        }

        internal bool PopFoldout(float _endPos, float _height, out float _beginPos, out float _fade, out bool _hasColor)
        {
            BeginFoldoutAttribute _attribute = Attribute as BeginFoldoutAttribute;
            EditorGUI.indentLevel--;

            // Once again, only update positions on Repaint event.
            if (Event.current.type == EventType.Repaint)
            {
                endPos = _endPos;

                // Remove this foldout color from the buffer.
                if (_attribute.HasColor)
                {
                    colorBuffer.RemoveAt(colorBuffer.Count - 1);
                }
            }

            // Get the current end position of the group, used to properly draw its color box.
            height = _height;

            _beginPos = beginPos;
            _fade = foldout.faded;
            _hasColor = _attribute.HasColor;

            // Enable back the next controls by exiting from the zero rect group.
            if (isBoxGroup)
            {
                GUI.EndGroup();
                isBoxGroup = false;
            }

            // Repaint while animating.
            if (foldout.isAnimating)
                EnhancedEditorGUIUtility.Repaint(SerializedProperty.serializedObject);

            return _attribute.foldout;
        }

        internal static Color PopColor()
        {
            Color _color = (colorBuffer.Count == 0)
                         ? EnhancedEditorGUIUtility.GUIThemeBackgroundColor
                         : colorBuffer[colorBuffer.Count - 1];

            return _color;
        }
        #endregion
    }
}

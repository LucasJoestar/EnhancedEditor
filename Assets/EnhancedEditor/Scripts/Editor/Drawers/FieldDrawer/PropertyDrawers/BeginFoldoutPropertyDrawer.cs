// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="BeginFoldoutAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(BeginFoldoutAttribute))]
	public class BeginFoldoutPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        private static BeginFoldoutPropertyDrawer[] availableFoldouts = new BeginFoldoutPropertyDrawer[] { };
        private static readonly List<Color> colorBuffer = new List<Color>();

        public string PropertyName { get; private set; }

        private float beginHeight = 0f;
        private float endHeight = 0f;

        private float lerpHeight = 0f;
        private float colorHeight = 0f;

        private bool foldout = false;

        // -----------------------

        public override void OnEnable(SerializedProperty _property)
        {
            BeginFoldoutAttribute _attribute = (BeginFoldoutAttribute)Attribute;
            _attribute.Foldout = SessionState.GetBool(_property.name, false);

            foldout = _attribute.Foldout;
            PropertyName = _property.name;

            // Try to recover foldout, for some properties can be recreated while
            // already existing (like ObjectReference field properties).
            if (!EndFoldoutPropertyDrawer.RecoverFoldout(this, _property.name))
                UnityEditor.ArrayUtility.Add(ref availableFoldouts, this);
        }

        public override bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            BeginFoldoutAttribute _attribute = (BeginFoldoutAttribute)Attribute;

            Event _event = Event.current;
            if (_attribute.HasColor && (_event.type == EventType.Repaint))
            {
                // Use colorHeight as previous repaint value
                // to avoid offset with EndFoldout height update.
                Rect _colorRect = EditorGUI.IndentedRect(_position);
				EditorGUI.DrawRect(new Rect(_colorRect.x + EnhancedEditorGUIUtility.BoxLeftOffset,
											_position.y + 1f,
											_colorRect.width + EnhancedEditorGUIUtility.BoxRightOffset,
											(endHeight - colorHeight) + lerpHeight),
								   _attribute.Color);

				// Draw an outline all around the color box.
				// Use GUI instead of EditorGUI (and so with indented rect)
				// to do not take over following properties hot control.
				GUI.Label(new Rect(_colorRect.x + EnhancedEditorGUIUtility.BoxLeftOffset - 1f,
                                   _position.y,
								   _colorRect.width + EnhancedEditorGUIUtility.BoxRightOffset + 2f,
                                   endHeight - colorHeight + lerpHeight + 2f),
                          GUIContent.none, EditorStyles.helpBox);

				colorHeight = _position.y;
            }

            // Update foldout value in next event to avoid glitches
            // for a smoother draw.
            if (foldout != _attribute.Foldout)
            {
                _attribute.Foldout = foldout;
                SessionState.SetBool(_property.name, foldout);
            }

            _height = EditorGUIUtility.singleLineHeight;
            _position.height = _height;

            foldout = EditorGUI.Foldout(_position, _attribute.Foldout, _attribute.Label, true);
            EditorGUI.indentLevel++;

            // Register begin height only on Repaint event,
            // as Layout event do not calculate rects and mess with everything.
            if (_event.type == EventType.Repaint)
            {
                beginHeight = _position.yMax;

                // Use color buffer for russian-dolls-like foldouts repaint.
                if (_attribute.HasColor)
                    colorBuffer.Add(_attribute.Color);
            }
            else if (colorBuffer.Count > 0)
            {
                // Clear to avoid not popped out colors, when something
                // prevent associated EndFoldout from being called
                // (like ObjectReference field properties).
                colorBuffer.Clear();
            }

            return false;
        }
        #endregion

        #region Utility
        internal static BeginFoldoutPropertyDrawer GetFoldout()
        {
            int _index = availableFoldouts.Length - 1;
            BeginFoldoutPropertyDrawer _drawer = availableFoldouts[_index];
            UnityEditor.ArrayUtility.RemoveAt(ref availableFoldouts, _index);

            return _drawer;
        }

        internal static Color PopColor()
        {
            return (colorBuffer.Count == 0) ?
                        EnhancedEditorGUIUtility.GetGUIBackgroundColor() :
                        colorBuffer[colorBuffer.Count - 1];
        }

        internal bool PopFoldout(float _position, float _lerpHeight, out float _height)
        {
            BeginFoldoutAttribute _attribute = (BeginFoldoutAttribute)Attribute;
            if (Event.current.type == EventType.Repaint)
            {
                endHeight = _position;

                // Update color buffer on pop.
                if (_attribute.HasColor)
                    colorBuffer.RemoveAt(colorBuffer.Count - 1);
            }

            // Get actual foldout lerp height,
            // used to properly draw color box.
            lerpHeight = _lerpHeight;

			EditorGUI.indentLevel--;
            _height = beginHeight;

            return _attribute.Foldout;
        }
        #endregion
    }
}

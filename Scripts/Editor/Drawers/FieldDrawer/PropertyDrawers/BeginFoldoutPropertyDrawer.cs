// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="BeginFoldoutAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(BeginFoldoutAttribute))]
    public sealed class BeginFoldoutPropertyDrawer : EnhancedPropertyDrawer {
        #region Drawer Content
        private static List<BeginFoldoutPropertyDrawer> availableFoldouts = new List<BeginFoldoutPropertyDrawer>();
        private static readonly List<Color> colorBuffer = new List<Color>();

        private readonly AnimBool foldout = new AnimBool();
        internal string id = string.Empty;

        private float beginPos = 0f;
        private float endPos   = 0f;
        private float height   = 0f;
        private float colorPos = 0f;

        private bool isBoxGroup = false;

        // -----------------------

        public override void OnEnable() {
            BeginFoldoutAttribute _attribute = Attribute as BeginFoldoutAttribute;
            id = EnhancedEditorUtility.GetSerializedPropertyID(SerializedProperty);

            _attribute.foldout = foldout.value
                               = SessionState.GetBool(id, false);

            // Try to reconnect this foldout, as some properties can be recreated while
            // already existing (like the ObjectReference type properties).
            if (EndFoldoutPropertyDrawer.ReconnectFoldout(this, id)) {
                EnhancedEditorGUIUtility.Repaint(SerializedProperty.serializedObject);
            } else {
                availableFoldouts.Add(this);
            }
        }

        public override bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height) {
            // Check if this begin group is connected to an end to avoid any GUI trouble.
            if (!EndFoldoutPropertyDrawer.IsConnected(this)) {
                _height = 0f;
                return false;
            }

            BeginFoldoutAttribute _attribute = Attribute as BeginFoldoutAttribute;
            Event _event = Event.current;

            if (_attribute.HasColor && (_event.type == EventType.Repaint)) {
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
            if (foldout.target != _attribute.foldout) {
                _attribute.foldout = foldout.target;
                SessionState.SetBool(id, _attribute.foldout);
            }

            foldout.target = OnDrawHeader(_position, _attribute, foldout.target, out _height);
            _position.height = _height;

            EditorGUI.indentLevel++;

            // Only register the begin height on Repaint event,
            // as the Layout event always have uncalculated rects.
            if (_event.type == EventType.Repaint) {
                beginPos = _position.yMax;

                // Use a color buffer for russian-dolls-like foldouts repaint.
                if (_attribute.HasColor) {
                    colorBuffer.Add(_attribute.Color);
                }
            } else if (colorBuffer.Count > 0) {
                // Clear to avoid stacking unpopped out colors, which can occur when something
                // prevent the associated EndFoldout from being called (like with ObjectReference type properties).
                colorBuffer.Clear();
            }

            // When this group is folded, disable all the following controls by encapsulting them within a zero rect group.
            isBoxGroup = foldout.faded == 0f;
            if (isBoxGroup) {
                GUI.BeginGroup(Rect.zero);
            }

            return false;
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        /// <summary>
        /// Called when drawing this foldout header.
        /// </summary>
        private bool OnDrawHeader(Rect _position, BeginFoldoutAttribute _attribute, bool _foldout, out float _height) {
            _position.height = _height
                             = EditorGUIUtility.singleLineHeight;

            GUIStyle _style = _attribute.HeaderStyle;
            if (_style == GUIStyle.none) {
                _style = EditorStyles.foldout;
            }

            // Encapsulate.
            if (_attribute.Encapsulate) {
                _height += 2f;

                Rect _temp = new Rect(_position) {
                    x = 0f,
                    width = Screen.width,
                    height = 1f,
                };

                _height += 2f;
                _position.y += 2f;

                using (EnhancedEditorGUI.ZeroIndentScope()) {
                    EnhancedEditorGUI.HorizontalLine(_temp, SuperColor.Black.Get(), 0f);
                    EnhancedEditorGUI.HorizontalLine(new Rect(0f, endPos + height, Screen.width, 1f), SuperColor.Black.Get(), 0f);
                }
            }

            // Full width.
            if (_attribute.FullWidth) {
                _position.x = 0f;
                _position.width = Screen.width;
            }

            // Header.
            _foldout = GUI.Toggle(_position, _foldout, _attribute.Label, _style);

            // Icon.
            string _icon  = _attribute.Icon;

            if (!string.IsNullOrEmpty(_icon)) {
                Rect _iconPosition = new Rect(_position)
                {
                    xMin   = _position.xMax - 50f,
                    width  = 32f,
                    y      = _position.y - 2f,
                };

                GUIContent _iconGUI = EditorGUIUtility.IconContent(_icon);
                EditorGUI.LabelField(_iconPosition, _iconGUI);
            }

            // Background.
            Rect _fullRect = new Rect(_position) {
                x     = 0f,
                width = Screen.width,
                yMin  = _position.yMax + 0f,
                yMax  = endPos + height,
            };

            _style = _attribute.BackgroundStyle;

            if ((_fullRect.yMax > _position.yMax) && (_fullRect.height > 5f) && (_style != GUIStyle.none)) {

                using (EnhancedEditorGUI.ZeroIndentScope()) {
                    EditorGUI.LabelField(_fullRect, GUIContent.none, _style);
                }
            }

            return _foldout;
        }
        #endregion

        #region Utility
        internal static bool GetFoldout(out BeginFoldoutPropertyDrawer _foldout) {
            // Pop out last inserted entry if any.
            int _index = availableFoldouts.Count - 1;
            if (_index < 0) {
                _foldout = null;
                return false;
            }

            _foldout = availableFoldouts[_index];
            availableFoldouts.RemoveAt(_index);

            return true;
        }

        internal bool PopFoldout(float _endPos, float _height, out float _beginPos, out float _fade, out BeginFoldoutAttribute _attribute) {
            _attribute = Attribute as BeginFoldoutAttribute;
            EditorGUI.indentLevel--;

            // Once again, only update positions on Repaint event.
            if (Event.current.type == EventType.Repaint) {
                endPos = _endPos;

                // Remove this foldout color from the buffer.
                if (_attribute.HasColor) {
                    colorBuffer.RemoveAt(colorBuffer.Count - 1);
                }
            }

            // Get the current end position of the group, used to properly draw its color box.
            height = _height;

            _beginPos = beginPos;
            _fade = foldout.faded;

            // Enable back the next controls by exiting from the zero rect group.
            if (isBoxGroup) {
                GUI.EndGroup();
                isBoxGroup = false;
            }

            // Repaint while animating.
            if (foldout.isAnimating)
                EnhancedEditorGUIUtility.Repaint(SerializedProperty.serializedObject);

            return _attribute.foldout;
        }

        internal static Color PopColor() {
            Color _color = (colorBuffer.Count == 0)
                         ? EnhancedEditorGUIUtility.GUIThemeBackgroundColor
                         : colorBuffer[colorBuffer.Count - 1];

            return _color;
        }
        #endregion
    }
}

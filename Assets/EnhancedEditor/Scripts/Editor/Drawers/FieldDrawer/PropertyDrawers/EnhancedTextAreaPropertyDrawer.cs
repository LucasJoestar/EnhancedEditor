// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="EnhancedTextAreaAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(EnhancedTextAreaAttribute))]
	public class EnhancedTextAreaPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public static readonly float MinHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2f;
        private float height = MinHeight;

        // -----------------------

        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            // Header.
            _position.height = _height
                             = EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(_position, _label);
            _position.y += _position.height;

            // Get area height only on repaint event, as it does not take
            // word wrap into account during Layout.
            GUIStyle _style = EnhancedEditorStyles.TextArea;
            if (Event.current.type == EventType.Repaint)
            {
                height = Mathf.Max(MinHeight, _style.CalcHeight(new GUIContent(_property.stringValue), _position.width) + 5f);
            }

            _height += _position.height
                    = height;

            _property.stringValue = EditorGUI.TextArea(_position, _property.stringValue, _style);
            return true;
        }
        #endregion
    }
}

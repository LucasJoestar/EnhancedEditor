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
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="BlockAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(BlockAttribute))]
	public class BlockPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        private const float PrefixSpacing = 15f;

        // -----------------------

        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            float _y = _position.y;

            // Property label header.
            _position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(_position, _label);

            // We only want to draw children fields of this property (fields within the class / struct),
            // so store next object property to break chain when getting to it.
            SerializedProperty _next = _property.Copy();
            _next.Next(false);

            if (!_property.NextVisible(true))
            {
                _height = _position.height;
                return true;
            }

            // Draws properties prefix label left-sided to their value.
            EnhancedEditorGUIUtility.PushAnchor(EditorStyles.label, TextAnchor.MiddleRight);
            do
            {
                // Break when getting outside property class / struct.
                if (SerializedProperty.EqualContents(_property, _next))
                    break;

                _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
                _position.height = EditorGUI.GetPropertyHeight(_property);

                DrawProperty(_position, _property);
            } while (_property.NextVisible(false));


            EnhancedEditorGUIUtility.PopAnchor(EditorStyles.label);

            _height = _position.yMax - _y;
            return true;
        }
        #endregion

        #region Utility
        private void DrawProperty(Rect _position, SerializedProperty _property)
        {
            float _xMax = _position.xMax;
            _position.width = EditorGUIUtility.labelWidth - PrefixSpacing;

            EditorGUI.LabelField(_position, EnhancedEditorGUIUtility.GetPropertyLabel(_property));

            _position.xMin = _position.xMax + PrefixSpacing;
            _position.xMax = _xMax;

            EditorGUI.PropertyField(_position, _property, GUIContent.none);
        }
        #endregion
    }
}

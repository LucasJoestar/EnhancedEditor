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
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="ReadOnlyAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            EnhancedEditorGUIUtility.PushEnable(false);
            _height = 0f;

            return false;
        }

        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            ReadOnlyAttribute _attribute = (ReadOnlyAttribute)Attribute;
            if (_attribute.UseRadioToggle)
            {
                EditorGUI.Toggle(_position, _label, _property.boolValue, EditorStyles.radioButton);
                _height = _position.height;

                return true;
            }

            _height = 0f;
            return false;
        }

        public override void OnAfterGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            EnhancedEditorGUIUtility.PopEnable();
            _height = 0f;
        }
        #endregion
    }
}

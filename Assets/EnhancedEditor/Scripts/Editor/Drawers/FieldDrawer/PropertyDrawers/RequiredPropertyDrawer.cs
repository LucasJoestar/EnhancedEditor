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
    /// Special drawer for fields with the attribute <see cref="RequiredAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(RequiredAttribute))]
    public class RequiredPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        private int id = -1;

        // -----------------------

        public override bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            // Incompatible property.
            if (_property.propertyType != SerializedPropertyType.ObjectReference)
            {
                _height = 0f;
                return false;
            }

            id = EnhancedEditorGUIUtility.GetControlID(_label, FocusType.Keyboard);

            _position.height = EnhancedEditorGUI.RequiredHelpBox(_position, _label, _property.objectReferenceValue);
            _height = _position.height - EditorGUIUtility.standardVerticalSpacing;

            using (var _scope = new EditorGUI.PropertyScope(_position, GUIContent.none, _property)) { }

            // Get new object value from object.
            if (EnhancedEditorGUI.GetRequiredObject(id, _property, out Object _value))
            {
                _property.objectReferenceValue = _value;
                GUI.changed = true;
            }

            return false;
        }

        public override void OnContextMenu(GenericMenu _menu)
        {
            if (SerializedProperty.propertyType == SerializedPropertyType.ObjectReference)
            {
                EnhancedEditorGUI.AddRequiredUtilityToMenu(id, SerializedProperty, _menu);
            }
        }
        #endregion
    }
}

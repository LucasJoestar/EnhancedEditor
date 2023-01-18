// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="Reference{T}"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(Reference<>), true)]
    public class ReferencePropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            _property.NextVisible(true);
            EditorGUI.PropertyField(_position, _property, _label);
            
            return _position.height;
        }
        #endregion
    }
}

// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="Pair{T, U}"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(Pair<,>), true)]
    public class PairPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            EnhancedEditorGUI.BlockField(_position, _property, out float _height);
            return _height;
        }
        #endregion
    }
}

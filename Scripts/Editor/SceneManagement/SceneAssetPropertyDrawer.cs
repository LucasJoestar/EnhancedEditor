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
    /// Custom <see cref="SceneAsset"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneAsset), true)]
    public class SceneAssetPropertyDrawer : EnhancedPropertyEditor
    {
        #region Drawer Content
        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            float _height = _position.height
                          = EditorGUIUtility.singleLineHeight;

            EnhancedEditorGUI.SceneAssetField(_position, _property, _label);
            return _height;
        }
        #endregion
    }
}

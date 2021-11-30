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
    public class SceneAssetPropertyDrawer : PropertyDrawer
    {
        #region Drawer Content
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            EnhancedEditorGUI.SceneAssetField(_position, _property, _label);
        }
        #endregion
    }
}

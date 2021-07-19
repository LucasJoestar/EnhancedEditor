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
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="AssetPreviewAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(AssetPreviewAttribute))]
    public class AssetPreviewPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            AssetPreviewAttribute _attribute = (AssetPreviewAttribute)Attribute;
            _attribute.Foldout = EnhancedEditorGUI.AssetPreviewField(_position, _property, _label, _attribute.Size, _attribute.Foldout, out _height);

            return true;
        }
        #endregion
    }
}

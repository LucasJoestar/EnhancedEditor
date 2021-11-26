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
    /// Special drawer for fields with the attribute <see cref="AssetPreviewAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(AssetPreviewAttribute))]
    public class AssetPreviewPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            AssetPreviewAttribute _attribute = Attribute as AssetPreviewAttribute;
            EnhancedEditorGUI.AssetPreviewField(_position, _property, _label, out _height, _attribute.Size);

            _height += _position.height;
            return true;
        }
        #endregion
    }
}

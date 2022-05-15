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
    [CustomDrawer(typeof(FolderAttribute))]
    public class FolderPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            FolderAttribute _attribute = Attribute as FolderAttribute;
            EnhancedEditorGUI.FolderField(_position, _property, _label, _attribute.AllowOutsideProjectFolder);

            _height = _position.height;
            return true;
        }
        #endregion
    }
}

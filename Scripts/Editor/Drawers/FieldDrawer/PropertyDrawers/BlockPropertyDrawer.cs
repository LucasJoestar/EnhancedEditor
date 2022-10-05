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
    /// Special drawer for fields with the attribute <see cref="BlockAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(BlockAttribute))]
	public class BlockPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            BlockAttribute _attribute = Attribute as BlockAttribute;

            EnhancedEditorGUI.BlockField(_position, _property, _label, _attribute.ShowHeader);
            _height = EditorGUI.GetPropertyHeight(_property, true);

            if (!_attribute.ShowHeader) {
                _height -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            return true;
        }
        #endregion
    }
}

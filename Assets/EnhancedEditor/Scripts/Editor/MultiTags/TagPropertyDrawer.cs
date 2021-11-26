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
    /// Custom <see cref="Tag"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(Tag), true)]
	public class TagPropertyDrawer : PropertyDrawer
    {
        #region Drawer Content
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            EnhancedEditorGUI.TagField(_position, _property, _label);
        }
        #endregion
    }
}

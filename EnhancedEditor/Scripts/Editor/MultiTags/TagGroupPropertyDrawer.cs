// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Custom <see cref="TagGroup"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(TagGroup), true)]
	public class TagGroupPropertyDrawer : PropertyDrawer
    {
        #region Drawer Content
        private static readonly Dictionary<string, float> groupHeight = new Dictionary<string, float>();

        // -----------------------

        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            string _key = _property.propertyPath;

            // Only calculate the height if it has not been cached.
            if (!groupHeight.TryGetValue(_key, out float _height))
            {
                Rect _position = EnhancedEditorGUIUtility.GetViewControlRect();
                _height = _position.height + EnhancedEditorGUI.GetTagGroupExtraHeight(_position, _property, _label);

                return _height;
            }

            return _height;
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // Register this property to cache its height.
            string _key = _property.propertyPath;
            if (!groupHeight.ContainsKey(_key))
            {
                groupHeight.Add(_key, 0f);
            }

            // As the full height is given on position, set it for one line only.
            _position.height = EditorGUIUtility.singleLineHeight;
            EnhancedEditorGUI.TagGroupField(_position, _property, _label, out float _extraHeight);

            groupHeight[_key] = _position.height + _extraHeight;
        }
        #endregion
    }
}

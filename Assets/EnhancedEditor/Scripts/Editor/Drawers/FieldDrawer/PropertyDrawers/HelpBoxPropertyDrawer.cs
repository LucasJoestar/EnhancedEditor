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
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="HelpBoxAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public const int WidthSubtractor = 52;

        // -----------------------

        public float GetHeight(HelpBoxAttribute _attribute)
        {
            float _height =  Mathf.Max(EnhancedEditorGUIUtility.DefaultHelpBoxHeight, EditorStyles.helpBox.CalcHeight(_attribute.Label,
                                       EditorGUIUtility.currentViewWidth - WidthSubtractor)) + EditorGUIUtility.standardVerticalSpacing;

            return _height;
        }

        public override bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            HelpBoxAttribute _attribute = (HelpBoxAttribute)Attribute;
            if (_attribute.IsAbove)
            {
                _height = GetHeight(_attribute);
                _position.height = _height;

                EditorGUI.HelpBox(_position, _attribute.Label.text, (UnityEditor.MessageType)(int)_attribute.Type);
            }
            else
                _height = 0f;
            

            return false;
        }

        public override void OnAfterGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            HelpBoxAttribute _attribute = (HelpBoxAttribute)Attribute;
            if (!_attribute.IsAbove)
            {
                _height = GetHeight(_attribute);
                _position.height = _height;

                EditorGUI.HelpBox(_position, _attribute.Label.text, (UnityEditor.MessageType)(int)_attribute.Type);
            }
            else
                _height = 0f;
        }
        #endregion
    }
}

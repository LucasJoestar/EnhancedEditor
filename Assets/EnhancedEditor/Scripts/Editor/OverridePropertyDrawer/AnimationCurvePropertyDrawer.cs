﻿// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Custom <see cref="AnimationCurve"/> drawer, with additional context click utilities.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnimationCurve), true)]
    public class AnimationCurvePropertyDrawer : PropertyDrawer
    {
        #region Drawer Content
        /// <summary>
        /// Current curve value in buffer (used to copy / paste).
        /// </summary>
        public static AnimationCurve CurveBuffer = null;

        // -----------------------

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // Add copy / paste options on context click menu.
            // Do not use context click event to avoid opening curve editor.
            Event _event = Event.current;
            if ((_event.type == EventType.MouseDown) && (_event.button == 1) && _position.Contains(_event.mousePosition))
            {
                GenericMenu _menu = new GenericMenu();
                OnCurveBufferContextMenu(_menu, _property);

                _menu.ShowAsContext();
                _event.Use();
            }

            // Draw base property field.
            EditorGUI.PropertyField(_position, _property, _label);
        }

        internal static void OnCurveBufferContextMenu(GenericMenu _menu, SerializedProperty _property)
        {
            // Copy menu item.
            _menu.AddItem(new GUIContent("Copy", "Copy this animation curve properties"), false, () => CurveBuffer = _property.animationCurveValue);

            // Paste menu item.
            GUIContent _pasteContent = new GUIContent("Paste", "Paste copied animation curve properties");
            if (CurveBuffer != null)
            {
                _menu.AddItem(_pasteContent, false, () =>
                {
                    if (CurveBuffer != null)
                    {
                        _property.animationCurveValue = CurveBuffer;
                        _property.serializedObject.ApplyModifiedProperties();
                    }
                });
            }
            else
                _menu.AddDisabledItem(_pasteContent);
        }
        #endregion
    }
}

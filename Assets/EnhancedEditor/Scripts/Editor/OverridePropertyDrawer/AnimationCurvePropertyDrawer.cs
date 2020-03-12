using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(AnimationCurve))]
    public class AnimationCurvePropertyDrawer : PropertyDrawer
    {
        #region Fields
        /**************************
         *******   BUFFER   *******
         *************************/

        /// <summary>
        /// Buffer used to copy / paste animation curve.
        /// </summary>
        private static AnimationCurve curveBuffer = null;
        #endregion

        #region Methods
        /*****************************
         *****   UNITY METHODS   *****
         ****************************/

        // Make your own IMGUI based GUI for the property
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // Add copy / paste options on right click menu
            Event _event = Event.current;
            if ((_event.type == EventType.MouseDown) && (_event.button == 1) && (_position.Contains(_event.mousePosition)))
            {
                GenericMenu _menu = new GenericMenu();
                GenericMenu.MenuFunction _copy = () => curveBuffer = _property.animationCurveValue;
                GenericMenu.MenuFunction _paste = () =>
                {
                    if (curveBuffer == null) return;

                    _property.animationCurveValue = curveBuffer;
                    _property.serializedObject.ApplyModifiedProperties();
                };

                _menu.AddItem(new GUIContent("Copy", "Copy this animation curve properties"), false, _copy);
                _menu.AddItem(new GUIContent("Paste", "Paste copied animation curve properties"), false, _paste);

                _menu.ShowAsContext();
                _event.Use();
            }

            // Draw property field
            EditorGUI.PropertyField(_position, _property, _label);
        }
        #endregion
    }
}

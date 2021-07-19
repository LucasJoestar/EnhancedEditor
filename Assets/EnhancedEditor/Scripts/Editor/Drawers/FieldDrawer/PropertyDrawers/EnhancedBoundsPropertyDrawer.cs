// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="EnhancedBoundsAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(EnhancedBoundsAttribute))]
	public class EnhancedBoundsPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        private EnhancedBoundsAttribute boundsAttribute = null;
        private bool isValid = false;

        BoxBoundsHandle handles = null;
        MonoBehaviour behaviour = null;

        // -----------------------

        public override void OnEnable(SerializedProperty _property)
        {
            boundsAttribute = (EnhancedBoundsAttribute)Attribute;
            isValid = (_property.propertyType == SerializedPropertyType.Bounds
                    || _property.propertyType == SerializedPropertyType.BoundsInt)
                   && (_property.serializedObject.targetObject is MonoBehaviour);

            handles = new BoxBoundsHandle();
            handles.SetColor(boundsAttribute.Color);

            behaviour = _property.serializedObject.targetObject as MonoBehaviour;
        }

        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            if (!isValid)
            {
                _height = 0f;
                return false;
            }

            _position.height = _height
                             = (EditorGUIUtility.singleLineHeight * 3f) + (EditorGUIUtility.standardVerticalSpacing * 2f);

            EditorGUI.PropertyField(_position, _property, _label);
            return true;
        }

        public override void OnSceneGUI(SerializedProperty _property, SceneView _scene)
        {
            if (!isValid)
                return;

            // Update handle value, draw its handle, then update property value.
            if (_property.propertyType == SerializedPropertyType.Bounds)
            {
                handles.center = _property.boundsValue.center + behaviour.transform.position;
                handles.size = _property.boundsValue.size;

                DrawHandles();

                Bounds _newBounds = new Bounds()
                {
                    center = handles.center - behaviour.transform.position,
                    size = handles.size
                };
                _property.boundsValue = _newBounds;
            }
            else
            {
                handles.center = _property.boundsIntValue.center + behaviour.transform.position;
                handles.size = _property.boundsIntValue.size;

                DrawHandles();

                Vector3 _position = handles.center - behaviour.transform.position;
                BoundsInt _newBounds = new BoundsInt()
                {
                    position = new Vector3Int((int)_position.x, (int)_position.y, (int)_position.z),
                    size = new Vector3Int((int)handles.size.x, (int)handles.size.y, (int)handles.size.z)
                };
                _property.boundsIntValue = _newBounds;
            }
            
            _property.serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Utility
        private void DrawHandles()
        {
            Matrix4x4 _rotatedMatrix = Handles.matrix * Matrix4x4.TRS(Vector3.zero, behaviour.transform.rotation, Vector3.one);
            using (new Handles.DrawingScope(_rotatedMatrix))
            {
                handles.DrawHandle();
            }
        }
        #endregion
    }
}

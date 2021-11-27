// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="EnhancedBoundsAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(EnhancedBoundsAttribute))]
	public class EnhancedBoundsPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        private MonoBehaviour[] behaviours = new MonoBehaviour[] { };
        private BoxBoundsHandle handle = null;

        private bool isValid = false;

        // -----------------------

        public override void OnEnable()
        {
            EnhancedBoundsAttribute _attribute = Attribute as EnhancedBoundsAttribute;

            // This attribute only work with Bounds and BoundsInt property types.
            if ((SerializedProperty.propertyType == SerializedPropertyType.Bounds || SerializedProperty.propertyType == SerializedPropertyType.BoundsInt)
             && (SerializedProperty.serializedObject.targetObject is MonoBehaviour))
            {
                handle = new BoxBoundsHandle();
                handle.SetColor(_attribute.Color);

                // Only draw handles for target objects in a valid open scene.
                foreach (Object _object in SerializedProperty.serializedObject.targetObjects)
                {
                    MonoBehaviour _behaviour = _object as MonoBehaviour;
                    Scene _scene = _behaviour.gameObject.scene;

                    if ((_scene != null) && _scene.IsValid() && _scene.isLoaded)
                    {
                        ArrayUtility.Add(ref behaviours, _behaviour);
                    }
                }

                isValid = behaviours.Length > 0;
            }
        }

        public override void OnSceneGUI(SceneView _scene)
        {
            if (!isValid)
                return;

            if (SerializedProperty.propertyType == SerializedPropertyType.Bounds)
            {
                // Bounds property.
                foreach (MonoBehaviour _mono in behaviours)
                {
                    Bounds _bounds = SerializedProperty.boundsValue;

                    handle.center = _bounds.center + _mono.transform.position;
                    handle.size = _bounds.size;

                    using (var _changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        DrawHandles(_mono);

                        if (_changeCheck.changed)
                        {
                            SerializedProperty.boundsValue = new Bounds()
                            {
                                center = handle.center - _mono.transform.position,
                                size = handle.size
                            };

                            SerializedProperty.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
            else
            {
                // BoundsInt property.
                foreach (MonoBehaviour _mono in behaviours)
                {
                    BoundsInt _bounds = SerializedProperty.boundsIntValue;

                    handle.center = _bounds.center + _mono.transform.position;
                    handle.size = _bounds.size;
                    
                    using (var _changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        DrawHandles(_mono);

                        if (_changeCheck.changed)
                        {
                            Vector3Int _size = new Vector3Int(Mathf.RoundToInt(handle.size.x), Mathf.RoundToInt(handle.size.y), Mathf.RoundToInt(handle.size.z));
                            Vector3 _position = handle.center - _mono.transform.position - ((Vector3)_size / 2f);

                            SerializedProperty.boundsIntValue = new BoundsInt()
                            {
                                position = new Vector3Int(Mathf.RoundToInt(_position.x), Mathf.RoundToInt(_position.y), Mathf.RoundToInt(_position.z)),
                                size = _size
                            };

                            SerializedProperty.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }

            // ----- Local Method ----- \\

            void DrawHandles(MonoBehaviour _mono)
            {
                Matrix4x4 _matrix = Handles.matrix * Matrix4x4.TRS(Vector3.zero, _mono.transform.rotation, Vector3.one);
                using (var _scope = new Handles.DrawingScope(_matrix))
                {
                    handle.DrawHandle();
                }
            }
        }
        #endregion
    }
}

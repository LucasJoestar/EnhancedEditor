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
    /// Custom <see cref="SceneAsset"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneAsset), true)]
    public class SceneAssetPropertyDrawer : PropertyDrawer
    {
        #region Drawer Content
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // Loads the database to ensure that one exist in the project.
            var _database = BuildSceneDatabase.Database;

            // Get the GUID property.
            SerializedProperty _guidProperty = _property.FindPropertyRelative("guid");

            // Get the scene asset from registered guid.
            string _path = AssetDatabase.GUIDToAssetPath(_guidProperty.stringValue);
            UnityEditor.SceneAsset _scene = string.IsNullOrEmpty(_path)
                                          ? null
                                          : AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(_path);

            _label.text = string.IsNullOrEmpty(_path)
                        ? "None"
                        : _scene.name;

            // Property field.
            using (var _scope = new EditorGUI.PropertyScope(_position, _label, _property))
            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                _scene = EditorGUI.ObjectField(_position, _label, _scene, typeof(UnityEditor.SceneAsset), false) as UnityEditor.SceneAsset;

                if (_changeCheck.changed)
                {
                    string _guid = (_scene != null)
                                 ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_scene))
                                 : string.Empty;

                    _guidProperty.stringValue = _guid;
                }
            }
        }
        #endregion
    }
}

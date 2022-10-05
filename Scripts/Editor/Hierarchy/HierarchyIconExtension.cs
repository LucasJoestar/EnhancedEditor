// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Hierarchy window extension class used to draw an icon next to a <see cref="GameObject"/>.
    /// </summary>
	[InitializeOnLoad]
    public class HierarchyIconExtension : UnityObjectEditor {
        #region Content
        static HierarchyIconExtension() {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        }

        // -----------------------

        static void HandleHierarchyWindowItemOnGUI(int _id, Rect _position) {
            GameObject _object = EditorUtility.InstanceIDToObject(_id) as GameObject;

            if (!ReferenceEquals(_object, null) && _object.TryGetComponent(out ExtendedBehaviour _behaviour)) {
                Texture _icon = _behaviour.icon;

                GUIContent _content = EnhancedEditorGUIUtility.GetLabelGUI(string.Empty);
                _content.image = _icon;

                _position.xMin = _position.xMax - 16f;

                EditorGUI.LabelField(_position, _content);
            }
        }
        #endregion
    }
}

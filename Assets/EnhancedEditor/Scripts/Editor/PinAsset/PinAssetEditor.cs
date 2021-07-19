// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;
using UnityEditor;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Custom <see cref="PinAsset"/> editor.
    /// </summary>
	[CustomEditor(typeof(PinAsset))]
	public class PinAssetEditor : UnityEditor.Editor
    {
        #region Editor Content
        private static readonly GUIContent sectionGUI = new GUIContent("PIN ASSET");
        private static readonly GUIContent buttonGUI = new GUIContent("PING OBJECT", "Ping the associated object in its scene.");

        // -----------------------

        public override void OnInspectorGUI()
        {
            EnhancedEditorGUILayout.Section(sectionGUI, SectionAttribute.DefaultLineWidth);
            if (GUILayout.Button(buttonGUI))
            {
                PinAsset _asset = serializedObject.targetObject as PinAsset;
                _asset.PinAssetInScene();
            }
        }
        #endregion
    }
}

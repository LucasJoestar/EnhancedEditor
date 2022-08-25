// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="SceneBundle"/> editor, used to add the embedded scenes into the build settings.
    /// </summary>
    [CustomEditor(typeof(SceneBundle), true)]
    public class SceneBundleEditor : UnityObjectEditor {
        #region Editor GUI
        private const float ButtonHeight = 25f;

        private static readonly GUIContent addScenesToBuildGUI = new GUIContent("Add Scene(s) to Build", "Adds all scenes in this bundle into the build settings.");
        private static readonly Color buttonColor = SuperColor.Lime.Get();

        private SceneBundle bundle = null;

        // -----------------------

        protected override void OnEnable() {
            base.OnEnable();
            bundle = target as SceneBundle;
        }

        public override void OnInspectorGUI() {
            for (int _i = 0; _i < bundle.Scenes.Length; _i++) {
                SceneAsset _scene = bundle.Scenes[_i];

                if (_scene.BuildIndex == -1) {
                    // If at least one scene is not in build, draw a button to register all missing ones.
                    using (var _scope = EnhancedGUI.GUIColor.Scope(buttonColor)) {
                        if (GUILayout.Button(addScenesToBuildGUI, GUILayout.Height(ButtonHeight))) {
                            AddScenesInBuild(_i);
                        }
                    }

                    GUILayout.Space(10f);
                    break;
                }
            }

            base.OnInspectorGUI();
        }

        // -----------------------

        private void AddScenesInBuild(int _index) {
            EditorBuildSettingsScene[] _buildScenes = EditorBuildSettings.scenes;

            for (int _i = _index; _i < bundle.Scenes.Length; _i++) {
                SceneAsset _scene = bundle.Scenes[_i];

                if (_scene.BuildIndex == -1) {
                    string _path = AssetDatabase.GUIDToAssetPath(_scene.guid);
                    int _sceneIndex = System.Array.FindIndex(_buildScenes, s => s.path == _path);

                    if (_sceneIndex == -1) {
                        EditorBuildSettingsScene _buildScene = new EditorBuildSettingsScene(_path, true);
                        ArrayUtility.Add(ref _buildScenes, _buildScene);
                    } else {
                        _buildScenes[_sceneIndex].enabled = true;
                    }
                }
            }

            EditorBuildSettings.scenes = _buildScenes;
        }
        #endregion
    }
}

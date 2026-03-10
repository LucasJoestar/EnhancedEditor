// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Editor class managing preprocess operations - when entering play mode and before a build.
    /// </summary>
    [InitializeOnLoad]
    public sealed class EditorPreprocessManager : IPreprocessBuildWithReport {
        #region Global Members
        int IOrderedCallback.callbackOrder => 999;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        static EditorPreprocessManager() {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        #endregion

        #region Preprocess
        // -------------------------------------------
        // Callback(s)
        // -------------------------------------------

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport _report) { // Called just before a build is started.
            PreProcess();
            AssetDatabase.SaveAssets();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _state) {
            if (_state == PlayModeStateChange.EnteredPlayMode) {
                PreProcess();
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        private static void PreProcess() {
            // Delegate.
            PreprocessManager.SetEditorDelegates(LoadAssets);

            // Callbacks.
            ScriptableSettings[] _objects = EnhancedEditorUtility.LoadAssets<ScriptableSettings>();
            for (int i = _objects.Length; i-- > 0;) {

                ScriptableSettings _settings = _objects[i];
                if (_settings is IPreprocessCallback _callback) {

                    if (_callback.OnPreprocess()) {
                        EditorUtility.SetDirty(_settings);
                    }
                }
            }

            // ----- Local Method ----- \\

            static Object[] LoadAssets(Type _type) {
                return EnhancedEditorUtility.LoadAssets(_type);
            }
        }
        #endregion
    }
}

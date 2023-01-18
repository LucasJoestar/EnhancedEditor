// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Editor class manipulating and updating the data contained in the <see cref="FlagDatabase"/>.
    /// </summary>
    [InitializeOnLoad]
    public class FlagDatabaseManager : IPreprocessBuildWithReport {
        #region Global Members
        private static readonly AutoManagedResource<FlagDatabase> resource = new AutoManagedResource<FlagDatabase>("FlagDatabase", false);

        int IOrderedCallback.callbackOrder => 999;

        /// <summary>
        /// Database containing informations about all flags included in build.
        /// </summary>
        public static FlagDatabase Database => resource.GetResource();

        // -----------------------

        static FlagDatabaseManager() {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            FlagDatabase.EditorFlagDatabaseGetter = () => Database;
        }
        #endregion

        #region Management
        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport _report) {
            // Called just before a build is started.
            UpdateDatabase();
            AssetDatabase.SaveAssets();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _state) {
            if (_state == PlayModeStateChange.EnteredPlayMode) {
                UpdateDatabase();
            }
        }

        private static void UpdateDatabase() {
            // Register all holders in the database.
            Database.holders = EnhancedEditorUtility.LoadAssets<FlagHolder>();
            EditorUtility.SetDirty(Database);
        }
        #endregion
    }
}

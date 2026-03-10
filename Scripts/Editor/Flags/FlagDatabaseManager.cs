// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using UnityEditor;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Editor class manipulating and updating the data contained in the <see cref="FlagDatabase"/>.
    /// </summary>
    [InitializeOnLoad]
    public sealed class FlagDatabaseManager {
        #region Global Members
        private static readonly AutoManagedResource<FlagDatabase> resource = new AutoManagedResource<FlagDatabase>("FlagDatabase", false);

        /// <summary>
        /// Database containing informations about all flags included in build.
        /// </summary>
        public static FlagDatabase Database => resource.GetResource();

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        static FlagDatabaseManager() {
            FlagDatabase.EditorFlagDatabaseGetter = () => Database;
        }
        #endregion
    }
}

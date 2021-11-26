// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor
{
    /// <summary>
    /// Use this on a <see cref="string"/> to make it a folder selection field.
    /// </summary>
	public class FolderAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        /// <summary>
        /// Allow or not to select a folder located outside the project.
        /// </summary>
        public readonly bool AllowOutsideProjectFolder = false;

        // -----------------------

        /// <param name="_allowOutsideProjectFolder"><inheritdoc cref="AllowOutsideProjectFolder" path="/summary"/></param>
        /// <inheritdoc cref="FolderAttribute"/>
        public FolderAttribute(bool _allowOutsideProjectFolder = false)
        {
            AllowOutsideProjectFolder = _allowOutsideProjectFolder;
        }
        #endregion
    }
}

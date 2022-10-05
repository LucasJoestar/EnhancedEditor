// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor
{
    /// <summary>
    /// Displays a struct or a serializable class within a single block, without any foldout.
    /// </summary>
    public class BlockAttribute : EnhancedPropertyAttribute {
        #region Global Members
        /// <summary>
        /// Whether the associated struct or class name should be displayed as a header or not.
        /// </summary>
        public readonly bool ShowHeader = false;

        // -----------------------

        /// <param name="_showHeader"><inheritdoc cref="ShowHeader" path="/summary"/></param>
        /// <inheritdoc cref="BlockAttribute"/>
        public BlockAttribute(bool _showHeader = false) {
            ShowHeader = _showHeader;
        }
        #endregion
    }
}

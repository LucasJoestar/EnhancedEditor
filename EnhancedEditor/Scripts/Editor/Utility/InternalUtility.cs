// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// 
    /// </summary>
	internal class InternalUtility
    {
        #region Content
        /// <summary>
        /// 
        /// </summary>
        public const string MenuItemPath = "Tools/Enhanced Editor/";

        public static string Get(string _s)
        {
            return MenuItemPath + _s;
        }
        #endregion
    }
}

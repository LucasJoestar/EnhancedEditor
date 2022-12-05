// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class EnhancedProjectItem {
        #region Global Members
        public string Guid = string.Empty;

        public Color Color = Color.white;
        public Texture Icon = null;
        public Texture OpenFolderIcon = null;
        public Texture EmptyFolderIcon = null;

        // -----------------------

        public EnhancedProjectItem(string _guid) {
            Guid = _guid;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [NonEditable("This data should not be manipulated manually.")]
    public class EnhancedProjectItemDatabase : ScriptableObject {
        #region Global Members
        
        #endregion
    }
}

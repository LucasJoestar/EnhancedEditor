// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("EnhancedEditor.Editor")]
namespace EnhancedEditor
{
    /// <summary>
    /// <see cref="GameObject"/>-extending class containing multiple editor notes and runtime features.
    /// </summary>
    #pragma warning disable IDE0052
    public class EnhancedBehaviour : MonoBehaviour
    {
        #region Global Members
        #if UNITY_EDITOR
        [SerializeField, EnhancedTextArea] private string comment = string.Empty;
        #endif

        /// <summary>
        /// All tags assigned to this <see cref="GameObject"/>.
        /// </summary>
        public TagGroup Tags = new TagGroup();

        #if UNITY_EDITOR
        [SerializeField, ReadOnly] private string lastModifiedBy = string.Empty;
        [SerializeField, ReadOnly] private string lastModifiedDate = string.Empty;
        #endif

        /// <summary>
        /// When set to true, the associated <see cref="GameObject"/> will not be automatically destroyed on scene loading.
        /// </summary>
        public bool IsPersistent = false;
        #endregion

        #region Editor Utility
        /// <summary>
        /// Called internally in editor to update the last time
        /// the associated <see cref="GameObject"/> has been modified.
        /// </summary>
        internal void UpdateLastModifiedState()
        {
            #if UNITY_EDITOR
            lastModifiedBy = Environment.UserName;
            lastModifiedDate = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            #endif
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Update modified state on object creation.
            if (lastModifiedDate == string.Empty)
                UpdateLastModifiedState();
        }
        #endif
        #endregion

        #region MonoBehaviour
        private void Start()
        {
            // Mark persistent objects as don't destroy on load.
            if (IsPersistent)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        #endregion
    }
}

// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Globalization;
using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// GameObject-extended class containing multiple editor notes and runtime features.
    /// </summary>
    #pragma warning disable IDE0052
    public class EnhancedBehaviour : MonoBehaviour
    {
        #region Global Members
        #if UNITY_EDITOR
        [SerializeField, EnhancedTextArea] private string comment = string.Empty;
        #endif

        public TagGroup Tags = new TagGroup();

        #if UNITY_EDITOR
        [SerializeField, ReadOnly] private string lastModifiedBy = string.Empty;
        [SerializeField, ReadOnly] private string lastModifiedDate = string.Empty;
        #endif

        public Tag Tag = new Tag();
        public bool IsPersistent = false;
        #endregion

        #region Editor Utility
        /// <summary>
        /// Called internally in editor to update the last time
        /// the associated <see cref="GameObject"/> has been modified.
        /// <para/>
        /// You should simply ignore this.
        /// </summary>
        public void UpdateLastModifiedState()
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
            // Persistent objects do persist through scene loadings?
            if (IsPersistent)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        #endregion
    }
}

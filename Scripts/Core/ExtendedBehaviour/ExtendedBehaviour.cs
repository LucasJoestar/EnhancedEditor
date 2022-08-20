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
namespace EnhancedEditor {
    /// <summary>
    /// <see cref="GameObject"/>-extending class containing multiple editor notes and runtime features.
    /// </summary>
    #pragma warning disable IDE0052
    public class ExtendedBehaviour : MonoBehaviour {
        #region Global Members
        #if UNITY_EDITOR
        [SerializeField, Enhanced, EnhancedTextArea] private string comment = string.Empty;
        #endif

        /// <summary>
        /// All tags assigned to this <see cref="GameObject"/>.
        /// </summary>
        public TagGroup Tags = new TagGroup();

        #if UNITY_EDITOR
        [SerializeField, Enhanced, ReadOnly] private string lastModifiedBy = string.Empty;
        [SerializeField, Enhanced, ReadOnly] private string lastModifiedDate = string.Empty;
        #endif

        #if UNITY_EDITOR
        private const string EditorOnlyTag = "EditorOnly";

        [Space(10f), HelpBox("Editor preview objects are meant to be only used in editor mode, outside of play", MessageType.Info, false)]
        [SerializeField, Enhanced, ShowIf("IsPersistent", ConditionType.False)] private bool IsEditorPreview = false;
        #endif
        #endregion

        #region Editor Utility
        /// <summary>
        /// Called internally in editor to update the last time
        /// the associated <see cref="GameObject"/> has been modified.
        /// </summary>
        internal void UpdateLastModifiedState() {
            #if UNITY_EDITOR
            lastModifiedBy = Environment.UserName;
            lastModifiedDate = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            #endif
        }

        #if UNITY_EDITOR
        private void OnValidate() {
            if (Application.isPlaying) {
                return;
            }

            // Update modified state on object creation.
            if (lastModifiedDate == string.Empty) {
                UpdateLastModifiedState();
            }

            // Markes this object as editor only, so it won't be included in any build.
            if (IsEditorPreview && !gameObject.CompareTag(EditorOnlyTag)) {
                gameObject.tag = EditorOnlyTag;
            }
        }

        private void Awake() {
            // Destroy preview objects.
            if (IsEditorPreview) {
                Destroy(gameObject);
            }
        }
        #endif
        #endregion
    }
}

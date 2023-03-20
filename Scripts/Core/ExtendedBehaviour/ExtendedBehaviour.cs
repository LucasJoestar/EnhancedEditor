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
    [ScriptGizmos(false, true)]
    [AddComponentMenu(InternalUtility.MenuPath + "Extended Behaviour"), DisallowMultipleComponent]
    #pragma warning disable IDE0052
    public class ExtendedBehaviour : MonoBehaviour {
        #region Global Members
        #if UNITY_EDITOR
        [SerializeField, Enhanced, EnhancedTextArea(true)] private string comment = string.Empty;
        #endif

        [Space(10f)]

        /// <summary>
        /// All tags assigned to this <see cref="GameObject"/>.
        /// </summary>
        public TagGroup Tags = new TagGroup();

        #if UNITY_EDITOR
        [Space(10f)]

        [SerializeField, Enhanced, ReadOnly] private string lastModifiedBy = string.Empty;
        [SerializeField, Enhanced, ReadOnly] private string lastModifiedDate = string.Empty;

        [Space(10f)]

        [HelpBox("Editor preview objects are meant to be only used in editor mode, outside of play", MessageType.Info, true)]
        [SerializeField, Enhanced, ShowIf("IsPersistent", ConditionType.False)] private bool IsEditorPreview = false;

        [Tooltip("If true, displays this object as a hierarchy header when the Enhanced Hierarchy is enabled")]
        [SerializeField] internal bool isHeader = false;

        [Tooltip("If true, override the default settings used to customize the way this object is displayed in the hierarchy")]
        [SerializeField] internal bool overrideHierarchyStyle = false;

        [Space(10f)]

        [Tooltip("Enhanced Hierarchy related style, used to customize the way this object is displayed in the hierarchy")]
        [SerializeField, Enhanced, ShowIf("overrideHierarchyStyle")] internal HierarchyStyle hierarchyStyle = new HierarchyStyle();
        #endif
        #endregion

        #region Editor Utility
        #if UNITY_EDITOR
        private const string EditorOnlyTag = "EditorOnly";
        private const string UntaggedTag = "Untagged";

        // -----------------------

        private void OnValidate() {
            if (Application.isPlaying) {
                return;
            }

            // Update modified state on object creation.
            if (lastModifiedDate == string.Empty) {
                UpdateLastModifiedState();
            }

            // Markes this object as editor only, so it won't be included in any build.
            if (IsEditorPreview != gameObject.CompareTag(EditorOnlyTag)) {
                gameObject.tag = IsEditorPreview ? EditorOnlyTag : UntaggedTag;
            }
        }

        private void Awake() {
            // Destroy preview objects.
            if (IsEditorPreview) {
                Destroy(gameObject);
            }
        }
        #endif

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
        #endregion
    }
}

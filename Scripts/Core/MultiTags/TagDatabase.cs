// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="ScriptableObject"/> database containing all <see cref="TagData"/> in the project.
    /// </summary>
    [NonEditable("Please use the Multi-Tags window to edit the project tags.")]
    public sealed class TagDatabase : ScriptableSettings {
        #region Global Members
        [SerializeField] internal TagData[] tags = new TagData[] { };
        [SerializeField] internal long counter   = 0;

        // -----------------------

        /// <summary>
        /// All <see cref="TagData"/> defined in the project.
        /// </summary>
        public TagData[] Tags {
            get { return tags; }
        }

        /// <summary>
        /// Total amount of tags in the project.
        /// </summary>
        public int Count {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Tags.Length; }
        }
        #endregion

        #region Initialization
        internal protected override void Init() {
            MultiTags.Database = this;
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        private void OnEnable() {
            // On object creation, get all existing Unity tags.
            if ((counter == 0) && (tags.Length == 0)) {
                string[] _tags = InternalEditorUtility.tags;

                foreach (string _tag in _tags) {
                    Color _color = TagData.DefaultColor.Get();
                    CreateTag(_tag, _color);
                }
            }
        }
        #endif
        #endregion

        #region Management
        internal Tag CreateTag(string _name, Color _color) {
            // Increase counter first, so all tags id will be above 0 (used to determine non-null default tags).
            counter++;

            TagData _data = new TagData(counter, _name, _color);
            ArrayUtility.Add(ref tags, _data);

            #if UNITY_EDITOR
            // Only sort tags in editor, for users comfort; avoid loop and string costs at runtime.
            SortTagsByName();
            #endif

            return new Tag(_data.ID);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Does a tag with a specific name exist in the project?
        /// </summary>
        /// <param name="_name">Tag name to check.</param>
        /// <returns>True if a tag with this name exist and was found in the project, false otherwise.</returns>
        public bool DoesTagExist(string _name) {
            int _length = tags.Length;

            for (int i = 0; i < _length; i++) {
                if (tags[i].name.Equals(_name, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Does a tag with a specific id exist in the project?
        /// </summary>
        /// <param name="_id">Tag id to check.</param>
        /// <returns>True if a tag with this id exist and was found in the project, false otherwise.</returns>
        public bool DoesTagExist(long _id) {
            int _length = tags.Length;

            for (int i = 0; i < _length; i++) {
                if (tags[i].ID == _id)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Sorts all tags in the database by their name.
        /// </summary>
        public void SortTagsByName() {
            Array.Sort(tags, (a, b) => {
                return a.CompareNameTo(b);
            });
        }
        #endregion

        #region Editor Utility
        /// <summary>
        /// Delegate used to open the multi-tags window.
        /// </summary>
        internal static Action OnOpenMultiTagsWindow = null;

        // -----------------------

        [Button(SuperColor.Green, IsDrawnOnTop = false)]
        private static void OpenMultiTagsWindow() {
            OnOpenMultiTagsWindow?.Invoke();
        }
        #endregion
    }
}

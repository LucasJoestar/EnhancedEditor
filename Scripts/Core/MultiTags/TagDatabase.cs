// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// <see cref="ScriptableObject"/> database containing all <see cref="TagData"/> in the project.
    /// </summary>
    [NonEditable("Please use the Multi-Tags window to edit the project tags.")]
    public class TagDatabase : ScriptableObject
    {
        #region Global Members
        [SerializeField] internal TagData[] tags = new TagData[] { };
        [SerializeField] internal long counter = 0;

        /// <summary>
        /// All <see cref="TagData"/> defined in the project.
        /// </summary>
        public TagData[] Tags => tags;

        /// <summary>
        /// Total amount of tags in the project.
        /// </summary>
        public int Count => Tags.Length;
        #endregion

        #region Initialization
        private void OnEnable()
        {
            #if UNITY_EDITOR
            // On object creation, get all existing Unity tags.
            if ((counter == 0) && (tags.Length == 0))
            {
                string[] _tags = UnityEditorInternal.InternalEditorUtility.tags;
                foreach (string _tag in _tags)
                {
                    Color _color = TagData.DefaultColor.Get();
                    CreateTag(_tag, _color);
                }
            }
            #endif
        }
        #endregion

        #region Management
        internal Tag CreateTag(string _name, Color _color)
        {
            // Increase counter first, so all tags id will be above 0 (used to determine non-null default tags).
            counter++;

            TagData _data = new TagData(counter, _name, _color);
            ArrayUtility.Add(ref tags, _data);

            #if UNITY_EDITOR
            // Only sort tags in editor, for users comfort; avoid loop and string costs at runtime.
            SortTagsByName();
            #endif

            Tag _tag = new Tag(_data.ID);
            return _tag;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Sorts all tags in the database by their name.
        /// </summary>
        public void SortTagsByName()
        {
            Array.Sort(tags, (a, b) =>
            {
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
        #pragma warning disable IDE0051
        private static void OpenMultiTagsWindow()
        {
            OnOpenMultiTagsWindow?.Invoke();
        }
        #endregion
    }
}

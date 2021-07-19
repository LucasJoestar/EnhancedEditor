// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor
{
    /// <summary>
    /// <see cref="Tag"/> array wrapper, used to define multiple tags in a single group.
    /// <para/>
    /// Contains utility methods to dynamically modify array content (and allows the use of a custom drawer in editor).
    /// </summary>
    [Serializable]
    public class TagGroup
    {
        #region Global Members
        /// <summary>
        /// All <see cref="Tag"/>s defined in the group.
        /// </summary>
        public Tag[] Tags = new Tag[] { };

        /// <summary>
        /// Amount of <see cref="Tag"/>s in the group.
        /// </summary>
        public int Length => Tags.Length;

        // -----------------------

        /// <summary>
        /// Creates a new group for <see cref="Tag"/>s.
        /// </summary>
        public TagGroup() { }

        /// <inheritdoc cref="TagGroup()"/>
        /// <param name="_tags"><see cref="Tag"/>s to initialize group with.</param>
        public TagGroup(Tag[] _tags)
        {
            Tags = _tags;
        }
        #endregion

        #region Operator
        public Tag this[int _index]
        {
            get => Tags[_index];
            set => Tags[_index] = value;
        }
        #endregion

        #region Management
        /// <summary>
        /// Adds a tag to the group.
        /// </summary>
        /// <param name="_tag">Tag to add.</param>
        public void AddTag(Tag _tag)
        {
            ArrayExtensions.Add(ref Tags, _tag);
        }

        /// <summary>
        /// Removes a tag from the group.
        /// </summary>
        /// <param name="_tag">Tag to remove.</param>
        public void RemoveTag(Tag _tag)
        {
            ArrayExtensions.Remove(ref Tags, _tag);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Does this group contains a specific tag?
        /// </summary>
        /// <param name="_tag">Tag to check presence.</param>
        /// <returns>True if the group contains the tag, false otherwise.</returns>
        public bool Contains(Tag _tag)
        {
            foreach (Tag _toCompare in Tags)
            {
                if (_tag == _toCompare)
                    return true;
            }

            return false;
        }
        #endregion
    }
}

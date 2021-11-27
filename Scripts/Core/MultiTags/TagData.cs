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
    /// Multi-Tags system class used to define a tag in the project.
    /// <para/>
    /// It is recommanded to use <see cref="Tag"/> and <see cref="TagGroup"/> instances
    /// instead of direct <see cref="TagData"/> references during runtime.
    /// </summary>
    [Serializable]
    #pragma warning disable IDE0044
    public class TagData
    {
        /// <summary>
        /// Default color assigned to new tags.
        /// </summary>
        public const SuperColor DefaultColor = SuperColor.White;

        /// <summary>
        /// Default "unknown" tag, used as an alternative non-null value when no matching tag can be found.
        /// </summary>
        public static readonly TagData UnknownTag = new TagData(-1, "——", DefaultColor.Get());

        #region Global Members
        [SerializeField, ReadOnly] internal string name = "NewTag";
        [SerializeField, ReadOnly] private long id = 0;

        /// <summary>
        /// ID of this tag. This is the only identifier of the object,
        /// and as so cannot be changed.
        /// </summary>
        public long ID => id;

        /// <summary>
        /// Name of this tag.
        /// <br/> Always prefer using utility methods from the <see cref="MultiTags"/> class
        /// for any modification or comparison.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Color of this tag (mostly used in editor).
        /// </summary>
        public Color Color = DefaultColor.Get();

        // -----------------------

        /// <summary>
        /// Creates a new <see cref="TagData"/>, and with it a new tag in the project.
        /// <para/>
        /// Internally restrict its use to avoid poor id setup and name duplicates.
        /// Please use <see cref="TagDatabase.CreateTag(string, Color)"/> to create a new tag instead.
        /// </summary>
        /// <param name="_id">ID of the tag to create (related to <see cref="TagDatabase.counter"/>).</param>
        /// <param name="_name">Name of this tag.</param>
        /// <param name="_color">Color of this tag (mostly used in editor).</param>
        internal TagData(long _id, string _name, Color _color)
        {
            id = _id;
            name = _name;
            Color = _color;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Compares this tag with another one according to their name.
        /// </summary>
        /// <param name="_tag">Tag to compare to this one.</param>
        /// <returns>Comparison value between the two names.</returns>
        public int CompareNameTo(TagData _tag)
        {
            return name.CompareTo(_tag.name);
        }
        #endregion
    }
}

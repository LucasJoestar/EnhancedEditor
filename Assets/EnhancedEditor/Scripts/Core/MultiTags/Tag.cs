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
    /// Tag identifier, especially used within the <see cref="EnhancedBehaviour"/> component
    /// to assign multiple tags to a single <see cref="GameObject"/>.
    /// </summary>
    [Serializable]
    public struct Tag
    {
        #region Global Members
        /// <summary>
        /// Id of the tag. This should not be set directly,
        /// always prefer using utiliy methods from the <see cref="MultiTags"/> class during runtime.
        /// </summary>
        public long ID;

        // -----------------------

        /// <summary>
        /// Tag identifier. Id defines which <see cref="TagObject"/> this object refers to.
        /// <para/>
        /// You can use utility methods from the <see cref="MultiTags"/> class to get tags id during runtime.
        /// </summary>
        /// <param name="_id"><see cref="TagObject"/> related identifier.</param>
        public Tag(long _id)
        {
            ID = _id;
        }
        #endregion

        #region Operators
        public static bool operator ==(Tag _a, Tag _b)
        {
            bool _equals = _a.ID == _b.ID;
            return _equals;
        }

        public static bool operator !=(Tag _a, Tag _b)
        {
            return !(_a == _b);
        }

        // -----------------------

        public override bool Equals(object obj)
        {
            if (!(obj is Tag _object))
                return false;

            return _object == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}

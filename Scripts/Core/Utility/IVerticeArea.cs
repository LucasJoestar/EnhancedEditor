// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using System.Collections.Generic;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Base interface to inherit any object with a delimited area made of multiple vertices.
    /// </summary>
    public interface IVerticeArea {
        #region Content
        /// <summary>
        /// Total count of vertices of this area.
        /// </summary>
        int VerticeCount { get; }

        /// <summary>
        /// Get all vertices from this area.
        /// </summary>
        /// <param name="_vertices">Buffer where to store vertices result.</param>
        void GetAreaVertices(List<Vector3> _vertices);

        /// <inheritdoc cref="GetAreaVertices(List{Vector3})"/>
        void GetAreaVertices(ref Vector3[] _vertices);

        /// <summary>
        /// Set all vertices of this area.
        /// </summary>
        /// <param name="_vertices">New vertices of this area.</param>
        void SetAreaVertices(IList<Vector3> _vertices);

        /// <summary>
        /// Pins/unpins this area draw.
        /// </summary>
        void Pin(bool _isPinned);
        #endregion
    }

    /// <summary>
    /// Contains multiple <see cref="IVerticeArea"/>-related utility members.
    /// </summary>
    public static class VerticeAreaUtility {
        #region Content
        public const float AreaPointHandlesAlpha = .7f;
        public const float AreaLineHandlesAlpha  = .4f;
        public const float AreaFillHandlesAlpha  = .4f;

        public const float AreaPointHandlesSize  = .2f;
        public const float AreaLineRectSize      = .09f;
        public const float AreaFillHandlesSize   = .4f;

        public static readonly Color AreaPreviewPointColor = SuperColor.HarvestGold.Get(AreaPointHandlesAlpha);
        public static readonly Color AreaPreviewLineColor  = SuperColor.White.Get(AreaLineHandlesAlpha);
        public static readonly Color AreaPointColor = new Color(.855f, .855f, 0f, AreaPointHandlesAlpha);
        public static readonly Color AreaLineColor  = SuperColor.Crimson.Get(1f);

        public static readonly SuperColor AreaFillColor = SuperColor.Crimson;
        #endregion
    }
}

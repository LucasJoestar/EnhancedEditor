// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Displays a preview of this object reference asset below this field.
    /// </summary>
    public class AssetPreviewAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public const float DefaultSize = 76f;
        public const float MinSize = 24f;

        /// <summary>
        /// Size of the asset preview (in pixels).
        /// </summary>
        public readonly float Size = DefaultSize;

        // -----------------------

        /// <param name="_size"><inheritdoc cref="Size" path="/summary"/></param>
        /// <inheritdoc cref="AssetPreviewAttribute"/>
        public AssetPreviewAttribute(float _size = DefaultSize)
        {
            Size = Mathf.Max(MinSize, _size);
        }
        #endregion
    }
}

// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Displays a preview of this field object reference value below its property drawer.
    /// </summary>
    public class AssetPreviewAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public const float DefaultSize = 75f;
        public const float MinSize = 25f;

        public readonly float Size = DefaultSize;
        public bool Foldout = true;

        // -----------------------

        /// <inheritdoc cref="AssetPreviewAttribute"/>
        /// <param name="_size">Object preview texture size (in pixels).</param>
        public AssetPreviewAttribute(float _size = DefaultSize)
        {
            Size = Mathf.Max(MinSize, _size);
        }
        #endregion
    }
}

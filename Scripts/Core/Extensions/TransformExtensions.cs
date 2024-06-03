// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Object = UnityEngine.Object;

namespace EnhancedEditor {
    /// <summary>
    /// Multiple extension methods related to the <see cref="Transform"/> class.
    /// </summary>
	public static class TransformExtensions {
        #region Content
        /// <summary>
        /// Get the value of a specific world-coordinates vector, relatively to this transform local orientation.
        /// </summary>
        /// <param name="_transform">Transform of the object.</param>
        /// <param name="_vector">Vector to get relative value.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RelativeVector(this Transform _transform, Vector3 _vector) {
            return _vector.RotateInverse(_transform.rotation);
        }

        /// <summary>
        /// Get the value of a specific local-space oriented vector in world space.
        /// </summary>
        /// <param name="_transform">Transform of the object.</param>
        /// <param name="_vector">Vector to get world value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WorldVector(this Transform _transform, Vector3 _vector) {
            return _vector.Rotate(_transform.rotation);
        }

        /// <summary>
        /// Set this transform local values.
        /// </summary>
        /// <param name="_transform">This transform instance.</param>
        /// <param name="_position">The local position of the transform.</param>
        /// <param name="_rotation">The local rotation of the transform.</param>
        /// <param name="_scale">The local scale of the transform.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocal(this Transform _transform, Vector3 _position, Quaternion _rotation, Vector3 _scale) {
            _transform.SetLocalPositionAndRotation(_position, _rotation);
            _transform.localScale = _scale;
        }

        /// <summary>
        /// Set this transform local values.
        /// </summary>
        /// <param name="_transform">This transform instance.</param>
        /// <param name="_other">Other transform to copy local values from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocal(this Transform _transform, Transform _other) {
            SetLocal(_transform, _other.localPosition, _other.localRotation, _other.localScale);
        }

        /// <summary>
        /// Resets this transform local values (position, rotation and scale) back to default.
        /// </summary>
        /// <param name="_transform">This transform instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetLocal(this Transform _transform) {
            _transform.SetLocal(Vector3.zero, Quaternion.identity, Vector3.one);
        }

        /// <summary>
        /// Resursively finds a child object from this <see cref="Transform"/>.
        /// </summary>
        /// <param name="_transform">Root <see cref="Transform"/> instance.</param>
        /// <param name="_childName">Name of the child to find.</param>
        /// <param name="_child">Found matchinf child object.</param>
        /// <param name="_exactName">If true, checks for a child object with exactly the given name. Otherwise, checks if the given string is contained in the child name.</param>
        /// <returns>True if a matching child could be successfully found, false otherwise.</returns>
        public static bool FindChildResursive(this Transform _transform, string _childName, out Transform _child, bool _exactName = true) {

            foreach (Transform _temp in _transform) {

                // Exact name.
                if (_exactName) {

                    if (_temp.name.Equals(_childName, StringComparison.Ordinal)) {
                        _child = _temp;
                        return true;
                    }

                    continue;
                }

                if (_temp.name.ToLower().Contains(_childName.ToLower(), StringComparison.Ordinal)) {
                    _child = _temp;
                    return true;
                }

                if (FindChildResursive(_temp, _childName, out _child, _exactName)) {
                    return true;
                }
            }

            _child = null;
            return false;
        }

        /// <summary>
        /// Destroy all child Game Objects from the specified Transform.
        /// </summary>
        /// <param name="_transform">This transform instance to destroy child from.</param>
        public static void DestroyChildren(this Transform _transform) {
            for (var i = _transform.childCount; i-- > 0;) {
                GameObject _gameObject = _transform.GetChild(i).gameObject;

                #if UNITY_EDITOR
                if (!Application.isPlaying) {
                    Undo.DestroyObjectImmediate(_gameObject);
                    continue;
                }
                #endif

                Object.Destroy(_gameObject);
            }
        }
        #endregion
    }
}
